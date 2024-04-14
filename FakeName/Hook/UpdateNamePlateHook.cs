﻿using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Hooking;
using Dalamud.Utility.Signatures;
using FakeName.Component;
using FakeName.Config;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace FakeName.Hook;

internal class UpdateNamePlateHook : IDisposable
{
    private readonly Plugin plugin;
    private readonly PluginConfig config;

    private readonly DutyComponent dutyComponent;

    [Signature(Signatures.UpdateNamePlate, DetourName = nameof(UpdateNamePlateDetour))]
    private readonly Hook<UpdateNamePlateDelegate> hook = null!;
    
    private readonly Dictionary<uint, KeyValuePair<string, string>> modifiedNamePlates = new();

    public UpdateNamePlateHook(Plugin plugin, PluginConfig config, DutyComponent dutyComponent)
    {
        this.plugin = plugin;
        this.config = config;
        this.dutyComponent = dutyComponent;

        Service.Hook.InitializeFromAttributes(this);
        hook.Enable();
    }

    public void Dispose()
    {
        hook.Disable();
    }

    private unsafe void* UpdateNamePlateDetour(
        RaptureAtkModule* raptureAtkModule, RaptureAtkModule.NamePlateInfo* namePlateInfo, NumberArrayData* numArray,
        StringArrayData* stringArray, BattleChara* battleChara, int numArrayIndex, int stringArrayIndex)
    {
        try
        {
            return UpdateNamePlate(raptureAtkModule, namePlateInfo, numArray, stringArray, battleChara, numArrayIndex, stringArrayIndex);
        }
        catch (Exception ex)
        {
            Service.Log.Error(ex, "UpdateNamePlateDetour encountered a critical error");
            return hook.Original(raptureAtkModule, namePlateInfo, numArray, stringArray, battleChara, numArrayIndex, stringArrayIndex);
        }
    }

    private unsafe void* UpdateNamePlate(
        RaptureAtkModule* raptureAtkModule, RaptureAtkModule.NamePlateInfo* namePlateInfo, NumberArrayData* numArray,
        StringArrayData* stringArray, BattleChara* battleChara, int numArrayIndex, int stringArrayIndex)
    {
        if (!plugin.Config.Enabled)
        {
            //namePlateInfo->DisplayTitle.SetString(newName);
            TryCleanUp(namePlateInfo);
            return hook.Original(raptureAtkModule, namePlateInfo, numArray, stringArray, battleChara, numArrayIndex, stringArrayIndex);
        }

        // if (gameObject->ObjectKind != 9)
        // {
        //     return hook.Original(raptureAtkModule, namePlateInfo, numArray, stringArray, battleChara, numArrayIndex, stringArrayIndex);
        // }
        
        var actorId = namePlateInfo->ObjectID.ObjectID;
        if (actorId == 0xE0000000)
        {
            return hook.Original(raptureAtkModule, namePlateInfo, numArray, stringArray, battleChara, numArrayIndex, stringArrayIndex);
        }
        
        var character = (PlayerCharacter?) Service.Objects.FirstOrDefault(t => t is PlayerCharacter && t.ObjectId == actorId);
        if (character == null)
        {
            return hook.Original(raptureAtkModule, namePlateInfo, numArray, stringArray, battleChara, numArrayIndex, stringArrayIndex);
        }
        
        if (!config.TryGetCharacterConfig(character.Name.TextValue, character.HomeWorld.Id, out var characterConfig) || characterConfig == null)
        {
            return hook.Original(raptureAtkModule, namePlateInfo, numArray, stringArray, battleChara, numArrayIndex, stringArrayIndex);
        }
        
        string oldName = namePlateInfo->Name.ToString();
        if (!oldName.Equals(characterConfig.FakeNameText))
        {
            namePlateInfo->Name.SetString(characterConfig.FakeNameText);
            if (!modifiedNamePlates.TryGetValue(actorId, out var old))
            {
                modifiedNamePlates[actorId] = new KeyValuePair<string, string>(character.Name.TextValue, character.CompanyTag.TextValue);
                Service.Log.Debug($"添加Player的Dic actorId:{actorId} <{character.Name.TextValue}, {character.CompanyTag.TextValue}>");
            }
            Service.Log.Debug($"替换了角色名：{oldName}->{characterConfig.FakeNameText}");
        }
        
        if (character.CurrentWorld.Id == character.HomeWorld.Id && !dutyComponent.InDuty && character.CompanyTag.TextValue.Length > 0)
        {
            var newFcName = characterConfig.FakeFcNameText.Length > 0 ? $"«{characterConfig.FakeFcNameText}»" : $"«{character.CompanyTag.TextValue}»";
            if (!namePlateInfo->FcName.ToString().Equals(newFcName))
            {
                Service.Log.Debug($"替换了部队简称：{namePlateInfo->FcName}->{newFcName} tag:{Service.ClientState.TerritoryType} duty:{Service.DutyState.IsDutyStarted}");
                
                namePlateInfo->FcName.SetString(newFcName);
            }
        }
        
        return hook.Original(raptureAtkModule, namePlateInfo, numArray, stringArray, battleChara, numArrayIndex, stringArrayIndex);
    }
    
    private unsafe void TryCleanUp(RaptureAtkModule.NamePlateInfo* namePlateInfo)
    {
        var actorId = namePlateInfo->ObjectID.ObjectID;
        if (!modifiedNamePlates.TryGetValue(actorId, out var old))
        {
            return;
        }
        Service.Log.Debug($"恢复了角色名：{namePlateInfo->Name}->{old.Key}");
        Service.Log.Debug($"恢复了角色部队：{namePlateInfo->FcName}->{old.Value}");
        namePlateInfo->Name.SetString($"{old.Key}");
        namePlateInfo->FcName.SetString($"«{old.Value}»");
        modifiedNamePlates.Remove(actorId);
    }
}
