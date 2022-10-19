using System;
using Dalamud.Hooking;
using Dalamud.Logging;
using Matrix.Utils;

namespace Matrix.Hooks;

public class SetNamePlateHook : IDisposable
{
    private readonly Hook<SetNamePlateDelegate> hook;

    public SetNamePlateHook(PluginAddressResolver address)
    {
        hook = new Hook<SetNamePlateDelegate>(address.AddonNamePlate_SetNamePlatePtr, SetNamePlateDetour);
    }

    public void Enable()
    {
        hook.Enable();
    }

    public void Disable()
    {
        hook.Disable();
    }

    public void Dispose()
    {
        Disable();
        hook.Dispose();
    }

    public IntPtr SetNamePlateDetour(
        IntPtr namePlateObjectPtr, bool isPrefixTitle, bool displayTitle, IntPtr title,
        IntPtr name, IntPtr fcName, int iconID)
    {
        // 角色名
        var localPlayer = Service.ClientState.LocalPlayer;
        var localPlayerName = "";
        if (localPlayer != null)
        {
            localPlayerName = localPlayer.Name.TextValue;
        }
        
        var currentName = SeStringUtils.SeStringFromPtr(name);

        if (localPlayerName == currentName.TextValue)
        {
            var fakeNamePtr = SeStringUtils.SeStringToPtr(Service.Config.MyFakeName);
            return hook.Original(namePlateObjectPtr, isPrefixTitle, displayTitle, title, fakeNamePtr, fcName, iconID);
        }
        else
        {
            return hook.Original(namePlateObjectPtr, isPrefixTitle, displayTitle, title, name, fcName, iconID);
        }
    }
}
