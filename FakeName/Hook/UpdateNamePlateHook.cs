using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Hooking;
using Dalamud.Utility.Signatures;
using FakeName.Component;
using FakeName.Config;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;
using BattleChara = FFXIVClientStructs.FFXIV.Client.Game.Character.BattleChara;

namespace FakeName.Hook;

internal class UpdateNamePlateHook : IDisposable
{
    private readonly Plugin plugin;
    private readonly PluginConfig config;

    private readonly DutyComponent dutyComponent;

    [Signature(Signatures.UpdateNamePlate, DetourName = nameof(UpdateNamePlateDetour))]
    private readonly Hook<UpdateNamePlateDelegate> hook = null!;
    
    private readonly Dictionary<uint, string> modifiedNamePlates = new();

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
        hook.Dispose();
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
        
        var actorId = namePlateInfo->ObjectID.ObjectID;
        if (actorId == 0xE0000000)
        {
            return hook.Original(raptureAtkModule, namePlateInfo, numArray, stringArray, battleChara, numArrayIndex, stringArrayIndex);
        }
        
        var gameObject = Service.Objects.FirstOrDefault(t => t.ObjectId == actorId);
        if (gameObject == null)
        {
            return hook.Original(raptureAtkModule, namePlateInfo, numArray, stringArray, battleChara, numArrayIndex, stringArrayIndex);
        }

        if (gameObject is PlayerCharacter character)
        {
            if (!config.TryGetCharacterConfig(character.Name.TextValue, character.HomeWorld.Id, out var characterConfig) || characterConfig == null)
            {
                return hook.Original(raptureAtkModule, namePlateInfo, numArray, stringArray, battleChara, numArrayIndex, stringArrayIndex);
            }
        
            string oldName = namePlateInfo->Name.ToString();
            var newName = characterConfig.FakeNameText.Length > 0 ? characterConfig.FakeNameText : character.Name.TextValue;
            var changed = false;
            if (!oldName.Equals(newName))
            {
                namePlateInfo->Name.SetString(newName);
                changed = true;
                //Service.Log.Debug($"替换了角色名：{oldName}->{newName}");
            }
        
            if (characterConfig.HideFcName)
            {
                namePlateInfo->FcName.SetString("");
                changed = true;
            }
            else if (character.CurrentWorld.Id == character.HomeWorld.Id && !dutyComponent.InDuty && character.CompanyTag.TextValue.Length > 0)
            {
                var newFcName = characterConfig.FakeFcNameText.Length > 0 ? $" «{characterConfig.FakeFcNameText}»" : $" «{character.CompanyTag.TextValue}»";
                if (!namePlateInfo->FcName.ToString().Equals(newFcName))
                {
                    //Service.Log.Debug($"替换了部队简称：{namePlateInfo->FcName}->{newFcName} tag:{Service.ClientState.TerritoryType} duty:{Service.DutyState.IsDutyStarted}");
                    namePlateInfo->FcName.SetString(newFcName);
                    changed = true;
                }
            }

            modifiedNamePlates[actorId] = character.Name.TextValue;
            if (changed)
            {
                namePlateInfo->IsDirty = true;
            }
        
            return hook.Original(raptureAtkModule, namePlateInfo, numArray, stringArray, battleChara, numArrayIndex, stringArrayIndex);
        }

        if (gameObject is BattleNpc battleNpc)
        {
            var owner = (PlayerCharacter?) Service.Objects.FirstOrDefault(t => t is PlayerCharacter && t.ObjectId == battleNpc.OwnerId);
            if (owner == null)
            {
                return hook.Original(raptureAtkModule, namePlateInfo, numArray, stringArray, battleChara, numArrayIndex, stringArrayIndex);
            }
            
            if (!config.TryGetCharacterConfig(owner.Name.TextValue, owner.HomeWorld.Id, out var characterConfig) || characterConfig == null)
            {
                return hook.Original(raptureAtkModule, namePlateInfo, numArray, stringArray, battleChara, numArrayIndex, stringArrayIndex);
            }
            
            // Service.Log.Debug($"{namePlateInfo->Title.ToString()} {battleNpc.ObjectId} {namePlateInfo->ObjectID.ObjectID}");
            // Service.Log.Debug($"{namePlateInfo->Title.ToString()} {namePlateInfo->DisplayTitle.ToString()} {battleNpc.ObjectId} {namePlateInfo->ObjectID.ObjectID}");
            
            var newDisplayTitle = $"《{characterConfig.FakeNameText}》";
            string oldDisplayTitle = namePlateInfo->DisplayTitle.ToString();
            if (!oldDisplayTitle.Equals(newDisplayTitle))
            {
                // Service.Log.Debug($"替换了搭档[{namePlateInfo->Name}]的displayTitle:{oldDisplayTitle}->{newDisplayTitle}");
                namePlateInfo->DisplayTitle.SetString(newDisplayTitle);
                namePlateInfo->IsDirty = true;
            }
            modifiedNamePlates[actorId] = owner.Name.TextValue;

            return hook.Original(raptureAtkModule, namePlateInfo, numArray, stringArray, battleChara, numArrayIndex, stringArrayIndex);
        }

        return hook.Original(raptureAtkModule, namePlateInfo, numArray, stringArray, battleChara, numArrayIndex, stringArrayIndex);
    }
    
    private unsafe void TryCleanUp(RaptureAtkModule.NamePlateInfo* namePlateInfo)
    {
        var localPlayer = Service.ClientState.LocalPlayer;
        if (localPlayer == null)
        {
            return;
        }
        
        var actorId = namePlateInfo->ObjectID.ObjectID;
        var gameObject = Service.Objects.FirstOrDefault(t => t.ObjectId == actorId);
        if (gameObject == null)
        {
            return;
        }

        if (gameObject is PlayerCharacter character)
        {
            var changed = false;
            var name = character.Name.TextValue;
            if (!namePlateInfo->Name.ToString().Equals(name))
            {
                // Service.Log.Debug($"恢复了角色名：{namePlateInfo->Name}->{name}");
                namePlateInfo->Name.SetString($"{name}");
                changed = true;
            }

            var fcName = $"«{character.CompanyTag.TextValue}»";
            if (fcName.Length > 0 && !namePlateInfo->FcName.ToString().Equals(fcName))
            {
                //Service.Log.Debug($"恢复了角色部队：{namePlateInfo->FcName}->{fcName}");
                namePlateInfo->FcName.SetString(fcName);
                changed = true;
            }

            if (changed)
            {
                namePlateInfo->IsDirty = true;
            }
            
            modifiedNamePlates.Remove(actorId);
        }

        if (gameObject is BattleNpc)
        {
            if (!modifiedNamePlates.TryGetValue(actorId, out var old))
            {
                return;
            }
            namePlateInfo->DisplayTitle.SetString($"《{old}》");
            namePlateInfo->IsDirty = true;
            modifiedNamePlates.Remove(actorId);
        }
    }
}
