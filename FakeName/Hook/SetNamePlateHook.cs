/*
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
        hook.Dispose();
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
        var actorId = npInfo.Data.ObjectId.ObjectId;
        if (actorId == 0xE0000000)
        {
            return hook.Original(namePlateObjectPtr, isPrefixTitle, displayTitle, titlePtr, namePtr, fcNamePtr, prefix, iconId);
        }

        if (!npObject.IsPlayer)
        {
            //Service.Log.Debug($"非玩家");
            return hook.Original(namePlateObjectPtr, isPrefixTitle, displayTitle, titlePtr, namePtr, fcNamePtr, prefix, iconId);
        }

        var character = (IPlayerCharacter?) Svc.Objects.FirstOrDefault(t => t is IPlayerCharacter && t.GameObjectId == actorId);
        if (character == null)
        {
            //Service.Log.Debug($"非玩家");
            return hook.Original(namePlateObjectPtr, isPrefixTitle, displayTitle, titlePtr, namePtr, fcNamePtr, prefix, iconId);
        }

        if (!P.TryGetConfig(character.Name.TextValue, character.HomeWorld.RowId, out var characterConfig))
        {
            return hook.Original(namePlateObjectPtr, isPrefixTitle, displayTitle, titlePtr, namePtr, fcNamePtr, prefix, iconId);
        }

        if (!characterConfig.IconReplace)
        {
            return hook.Original(namePlateObjectPtr, isPrefixTitle, displayTitle, titlePtr, namePtr, fcNamePtr, prefix, iconId);
        }
        
        // var name = SeStringUtils.ReadRawSeString(namePtr);
        // var title = SeStringUtils.ReadRawSeString(titlePtr);
        // var fcName = SeStringUtils.ReadRawSeString(fcNamePtr);
        return hook.Original(namePlateObjectPtr, isPrefixTitle, displayTitle, titlePtr, namePtr, fcNamePtr, prefix, characterConfig.IconId);
    }
}
*/
