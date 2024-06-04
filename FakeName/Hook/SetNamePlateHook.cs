using System;
using System.Linq;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Hooking;
using Dalamud.Utility.Signatures;
using ECommons.DalamudServices;
using FakeName.Data;
using FakeName.Utils;

namespace FakeName.Hook;

public class SetNamePlateHook : IDisposable
{
    [Signature(Signatures.SetNamePlate, DetourName = nameof(SetNamePlateDetour))]
    private readonly Hook<SetNamePlateDelegate> hook = null!;

    public SetNamePlateHook()
    {

        Svc.Hook.InitializeFromAttributes(this);

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
            return SetNamePlate(namePlateObjectPtr, isPrefixTitle, displayTitle, titlePtr, namePtr, fcNamePtr, prefix, iconId);
        }
        catch (Exception ex)
        {
            ex.Log();
            return hook.Original(namePlateObjectPtr, isPrefixTitle, displayTitle, titlePtr, namePtr, fcNamePtr, prefix, iconId);
        }
    }

    private unsafe IntPtr SetNamePlate(
        IntPtr namePlateObjectPtr, bool isPrefixTitle, bool displayTitle,
        IntPtr titlePtr, IntPtr namePtr, IntPtr fcNamePtr, IntPtr prefix, int iconId
    ) {
        if (!C.Enabled)
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
            //Service.Log.Debug($"非玩家");
            return hook.Original(namePlateObjectPtr, isPrefixTitle, displayTitle, titlePtr, namePtr, fcNamePtr, prefix, iconId);
        }

        var character = (PlayerCharacter?) Svc.Objects.FirstOrDefault(t => t is PlayerCharacter && t.ObjectId == actorId);
        if (character == null)
        {
            //Service.Log.Debug($"非玩家");
            return hook.Original(namePlateObjectPtr, isPrefixTitle, displayTitle, titlePtr, namePtr, fcNamePtr, prefix, iconId);
        }

        if (!C.TryGetCharacterConfig(character.Name.TextValue, character.HomeWorld.Id, out var characterConfig))
        {
            return hook.Original(namePlateObjectPtr, isPrefixTitle, displayTitle, titlePtr, namePtr, fcNamePtr, prefix, iconId);
        }

        if (!characterConfig.IconReplace)
        {
            return hook.Original(namePlateObjectPtr, isPrefixTitle, displayTitle, titlePtr, namePtr, fcNamePtr, prefix, iconId);
        }
        
        var name = SeStringUtils.ReadRawSeString(namePtr);
        var title = SeStringUtils.ReadRawSeString(titlePtr);
        var fcName = SeStringUtils.ReadRawSeString(fcNamePtr);
        // Service.Log.Debug($"SetNamePlate：{name} {title} {fcName} {iconId}");

        /*if (!string.IsNullOrEmpty(characterConfig.FakeNameText))
        {
            var nameText = SeStringUtils.ReadRawSeString(namePtr);
            // Service.Log.Debug($"角色Id:{actorId}");
            // Service.Log.Debug($"替换了角色名称:{nameText.TextValue}=>{characterConfig.FakeNameText} np:{npInfo.Name}");
            
            nameText.ReplaceSeStringText(character.Name.TextValue, characterConfig.FakeNameText);
            fixed (byte* newNamePtr = nameText.Encode().Terminate())
            {
                namePtr = (IntPtr)newNamePtr;
            }
        }

        if (!string.IsNullOrEmpty(characterConfig.FakeFcNameText))
        {
            var fcNameText = SeStringUtils.ReadRawSeString(fcNamePtr);
            // Service.Log.Debug($"替换了部队简称:{fcNameText.TextValue}=>{characterConfig.FakeFcNameText} np:{npInfo.FcName}");
            
            fcNameText.ReplaceSeStringText(character.CompanyTag.TextValue, characterConfig.FakeFcNameText);
            fixed (byte* newFcNamePtr = fcNameText.Encode().Terminate())
            {
                fcNamePtr = (IntPtr)newFcNamePtr;
            }
        }*/
        
        // 61524
        return hook.Original(namePlateObjectPtr, isPrefixTitle, displayTitle, titlePtr, namePtr, fcNamePtr, prefix, characterConfig.IconId);
    }
}
