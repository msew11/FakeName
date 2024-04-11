using System;
using System.Linq;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Hooking;
using Dalamud.Utility.Signatures;
using FakeName.Config;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace FakeName.Runtime;

internal class UpdateNamePlateHook : IDisposable
{
    private readonly Plugin plugin;
    private readonly PluginConfig config;

    [Signature(Signatures.UpdateNamePlate, DetourName = nameof(UpdateNamePlateDetour))]
    private readonly Hook<UpdateNamePlateDelegate> hook = null!;

    public UpdateNamePlateHook(Plugin plugin, PluginConfig config)
    {
        this.plugin = plugin;
        this.config = config;

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

        if (!namePlateInfo->Name.ToString().Equals(characterConfig.FakeNameText))
        {
            Service.Log.Debug($"替换了角色名：{namePlateInfo->Name}->{characterConfig.FakeNameText}");
            namePlateInfo->Name.SetString(characterConfig.FakeNameText);
        }
        
        
        if (character.CurrentWorld.Id == character.HomeWorld.Id)
        {
            var newFcName = characterConfig.FakeFcNameText.Length > 0 ? $"«{characterConfig.FakeFcNameText}»" : characterConfig.FakeNameText;
            if (!namePlateInfo->FcName.ToString().Equals(newFcName))
            {
                Service.Log.Debug($"替换了部队简称：{namePlateInfo->FcName}->{newFcName}");
                namePlateInfo->FcName.SetString(newFcName);
            }
        }
        
        return hook.Original(raptureAtkModule, namePlateInfo, numArray, stringArray, battleChara, numArrayIndex, stringArrayIndex);
    }
}
