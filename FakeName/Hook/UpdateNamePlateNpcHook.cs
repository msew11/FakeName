using System;
using System.Linq;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Hooking;
using Dalamud.Utility.Signatures;
using FakeName.Config;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace FakeName.Hook;

internal class UpdateNamePlateNpcHook : IDisposable
{
    private readonly Plugin plugin;
    private readonly PluginConfig config;

    [Signature(Signatures.UpdateNamePlateNpc, DetourName = nameof(UpdateNamePlateNpcDetour))]
    private readonly Hook<UpdateNameplateNpcDelegate> hook = null!;

    public UpdateNamePlateNpcHook(Plugin plugin, PluginConfig config)
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

    private unsafe void* UpdateNamePlateNpcDetour(
        RaptureAtkModule* raptureAtkModule, RaptureAtkModule.NamePlateInfo* namePlateInfo, NumberArrayData* numArray,
        StringArrayData* stringArray, GameObject* gameObject, int numArrayIndex, int stringArrayIndex)
    {
        try
        {
            return UpdateNamePlateNpc(raptureAtkModule, namePlateInfo, numArray, stringArray, gameObject, numArrayIndex, stringArrayIndex);
        }
        catch (Exception ex)
        {
            Service.Log.Error(ex, "UpdateNamePlateNpcDetour encountered a critical error");
            return hook.Original(raptureAtkModule, namePlateInfo, numArray, stringArray, gameObject, numArrayIndex, stringArrayIndex);
        }
    }

    private unsafe void* UpdateNamePlateNpc(
        RaptureAtkModule* raptureAtkModule, RaptureAtkModule.NamePlateInfo* namePlateInfo, NumberArrayData* numArray,
        StringArrayData* stringArray, GameObject* gameObject, int numArrayIndex, int stringArrayIndex)
    {
        if (!plugin.Config.Enabled)
        {
            //namePlateInfo->DisplayTitle.SetString(newName);
            return hook.Original(raptureAtkModule, namePlateInfo, numArray, stringArray, gameObject, numArrayIndex, stringArrayIndex);
        }

        if (gameObject->ObjectKind != 9)
        {
            return hook.Original(raptureAtkModule, namePlateInfo, numArray, stringArray, gameObject, numArrayIndex, stringArrayIndex);
        }
        
        var actorId = namePlateInfo->ObjectID.ObjectID;
        if (actorId == 0xE0000000)
        {
            return hook.Original(raptureAtkModule, namePlateInfo, numArray, stringArray, gameObject, numArrayIndex, stringArrayIndex);
        }
        
        var character = (PlayerCharacter?) Service.Objects.FirstOrDefault(t => t is PlayerCharacter && t.ObjectId == actorId);
        if (character == null)
        {
            return hook.Original(raptureAtkModule, namePlateInfo, numArray, stringArray, gameObject, numArrayIndex, stringArrayIndex);
        }
        
        if (!config.TryGetCharacterConfig(character.Name.TextValue, character.HomeWorld.Id, out var characterConfig) || characterConfig == null)
        {
            return hook.Original(raptureAtkModule, namePlateInfo, numArray, stringArray, gameObject, numArrayIndex, stringArrayIndex);
        }

        var newDisplayTitle = $"《{characterConfig.FakeNameText}》";
        if (!namePlateInfo->DisplayTitle.ToString().Equals(newDisplayTitle))
        {
            namePlateInfo->DisplayTitle.SetString(newDisplayTitle);
            Service.Log.Debug($"替换了宠物{namePlateInfo->Name}title:{newDisplayTitle}");
        }
        
        return hook.Original(raptureAtkModule, namePlateInfo, numArray, stringArray, gameObject, numArrayIndex, stringArrayIndex);
    }
}
