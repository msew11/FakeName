using System;
using System.Linq;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Hooking;
using Dalamud.Utility.Signatures;
using FakeName.Api;
using FakeName.Config;
using FakeName.Utils;

namespace FakeName.Runtime;

internal class NamePlateUpdater : IDisposable
{
    private readonly Plugin plugin;
    private readonly PluginConfig config;

    [Signature(Signatures.SetNamePlate, DetourName = nameof(SetNamePlateDetour))]
    private readonly Hook<SetNamePlateDelegate> hook = null!;

    public NamePlateUpdater(Plugin plugin, PluginConfig config)
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

    private IntPtr SetNamePlateDetour(
        IntPtr namePlateObjectPtr, bool isPrefixTitle, bool displayTitle,
        IntPtr titlePtr, IntPtr namePtr, IntPtr fcNamePtr, IntPtr prefix, int iconId)
    {
        try
        {
            return DealSetNamePlateEvent(namePlateObjectPtr, isPrefixTitle, displayTitle, titlePtr, namePtr, fcNamePtr, prefix, iconId);
        }
        catch (Exception ex)
        {
            Service.Log.Error(ex, "[FakeName]SetNamePlateDetour encountered a critical error");
            return hook.Original(namePlateObjectPtr, isPrefixTitle, displayTitle, titlePtr, namePtr, fcNamePtr, prefix, iconId);
        }
    }

    private unsafe IntPtr DealSetNamePlateEvent(
        IntPtr namePlateObjectPtr, bool isPrefixTitle, bool displayTitle,
        IntPtr titlePtr, IntPtr namePtr, IntPtr fcNamePtr, IntPtr prefix, int iconId
    ) {
        if (!plugin.Config.Enabled)
        {
            return hook.Original(namePlateObjectPtr, isPrefixTitle, displayTitle, titlePtr, namePtr, fcNamePtr, prefix, iconId);
        }
        
        var npObject = new NamePlateUtils.SafeNamePlateObject(namePlateObjectPtr);
        var npInfo = npObject.NamePlateInfo;
        var actorId = npInfo.Data.ObjectID.ObjectID;
        if (actorId == 0xE0000000)
        {
            return hook.Original(namePlateObjectPtr, isPrefixTitle, displayTitle, titlePtr, namePtr, fcNamePtr, prefix, iconId);
        }

        if (!npObject.IsPlayer)
        {
            return hook.Original(namePlateObjectPtr, isPrefixTitle, displayTitle, titlePtr, namePtr, fcNamePtr, prefix, iconId);
        }

        var character = (PlayerCharacter?) Service.Objects.FirstOrDefault(t => t is PlayerCharacter && t.ObjectId == actorId);
        if (character == null)
        {
            return hook.Original(namePlateObjectPtr, isPrefixTitle, displayTitle, titlePtr, namePtr, fcNamePtr, prefix, iconId);
        }

        if (!config.TryGetCharacterConfig(character.Name.TextValue, character.HomeWorld.Id, out var characterConfig) || characterConfig == null)
        {
            return hook.Original(namePlateObjectPtr, isPrefixTitle, displayTitle, titlePtr, namePtr, fcNamePtr, prefix, iconId);
        }

        if (!string.IsNullOrEmpty(characterConfig.FakeNameText))
        {
            var nameText = SeStringUtils.ReadRawSeString(namePtr);
            Service.Log.Debug($"[FakeName]替换了角色名称:{nameText.TextValue}=>{characterConfig.FakeNameText} np:{npInfo.Name}");
            nameText.ReplaceSeStringText(character.Name.TextValue, characterConfig.FakeNameText);
            
            fixed (byte* newNamePtr = nameText.Encode().Terminate())
            {
                namePtr = (IntPtr)newNamePtr;
            }
        }

        if (!string.IsNullOrEmpty(characterConfig.FakeFcNameText))
        {
            var fcNameText = SeStringUtils.ReadRawSeString(fcNamePtr);
            Service.Log.Debug($"[FakeName]替换了部队简称:{fcNameText.TextValue}=>{characterConfig.FakeFcNameText} np:{npInfo.FcName}");
            fcNameText.ReplaceSeStringText(character.CompanyTag.TextValue, characterConfig.FakeFcNameText);
            
            fixed (byte* newFcNamePtr = fcNameText.Encode().Terminate())
            {
                fcNamePtr = (IntPtr)newFcNamePtr;
            }
        }
        
        return hook.Original(namePlateObjectPtr, isPrefixTitle, displayTitle, titlePtr, namePtr, fcNamePtr, prefix, iconId);
    }
}
