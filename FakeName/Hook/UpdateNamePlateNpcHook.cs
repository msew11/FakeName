using System;
using System.Collections.Generic;
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
    
    private readonly Dictionary<uint, string> modifiedNamePlates = new();

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
        hook.Dispose();
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
            TryCleanUp(namePlateInfo, gameObject);
            return hook.Original(raptureAtkModule, namePlateInfo, numArray, stringArray, gameObject, numArrayIndex, stringArrayIndex);
        }

        if (gameObject->ObjectKind == (byte)ObjectKind.EventNpc)
        {
            return hook.Original(raptureAtkModule, namePlateInfo, numArray, stringArray, gameObject, numArrayIndex, stringArrayIndex);
        }

        if (gameObject->ObjectKind != (byte)ObjectKind.Companion)
        {
            // Service.Log.Debug($"{namePlateInfo->Name.ToString()}");
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
        
        if (!config.TryGetCharacterConfig(character.Name.TextValue, character.HomeWorld.Id, out var characterConfig))
        {
            return hook.Original(raptureAtkModule, namePlateInfo, numArray, stringArray, gameObject, numArrayIndex, stringArrayIndex);
        }
        
        // Service.Log.Debug($"{namePlateInfo->Title.ToString()} {namePlateInfo->DisplayTitle.ToString()} {gameObject->ObjectID} {namePlateInfo->ObjectID.ObjectID}");

        var newDisplayTitle = $"《{characterConfig.FakeNameText}》";
        string oldDisplayTitle = namePlateInfo->DisplayTitle.ToString();
        if (!oldDisplayTitle.Equals(newDisplayTitle))
        {
            // Service.Log.Debug($"替换了宠物[{namePlateInfo->Name}]的displayTitle:{oldDisplayTitle}->{newDisplayTitle}");
            namePlateInfo->DisplayTitle.SetString(newDisplayTitle);
            namePlateInfo->IsDirty = true;
        }
        
        modifiedNamePlates[actorId] = character.Name.TextValue;
        
        return hook.Original(raptureAtkModule, namePlateInfo, numArray, stringArray, gameObject, numArrayIndex, stringArrayIndex);
    }

    private unsafe void TryCleanUp(RaptureAtkModule.NamePlateInfo* namePlateInfo, GameObject* gameObject)
    {
        if (gameObject->ObjectKind != 9)
        {
            return;
        }
        
        var actorId = namePlateInfo->ObjectID.ObjectID;
        if (!modifiedNamePlates.TryGetValue(actorId, out var old))
        {
            return;
        }
        //Service.Log.Debug($"恢复了宠物[{namePlateInfo->Name}]的displayTitle:{namePlateInfo->DisplayTitle.ToString()}->{old}");
        namePlateInfo->DisplayTitle.SetString($"《{old}》");
        namePlateInfo->IsDirty = true;
        modifiedNamePlates.Remove(actorId);
    }
}
