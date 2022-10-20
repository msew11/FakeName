using System;
using Dalamud.Hooking;
using Dalamud.Utility.Signatures;
using Matrix.Utils;

namespace Matrix.GameFunctions;

internal class HookSetNamePlate : IDisposable
{
    internal const string SetNamePlateSignature =
        "48 89 5C 24 ?? 48 89 6C 24 ?? 56 57 41 54 41 56 41 57 48 83 EC 40 44 0F B6 E2";

    private delegate IntPtr SetNamePlateDelegate(
        IntPtr addon, bool isPrefixTitle, bool displayTitle, IntPtr title, IntPtr name, IntPtr fcName, int iconId);

    [Signature(SetNamePlateSignature, DetourName = nameof(SetNamePlateDetour))]
    private Hook<SetNamePlateDelegate> SetNamePlateHook { get; init; } = null!;

    private unsafe IntPtr SetNamePlateDetour(
        IntPtr namePlateObjectPtr, bool isPrefixTitle, bool displayTitle, IntPtr title,
        IntPtr name, IntPtr fcName, int iconId)
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
            var fakeNamePtr = SeStringUtils.SeStringToPtr(Plugin.Config.FakeName);
            return SetNamePlateHook.Original(namePlateObjectPtr, isPrefixTitle, displayTitle, title, fakeNamePtr,
                                             fcName, iconId);
        }
        else
        {
            return SetNamePlateHook.Original(namePlateObjectPtr, isPrefixTitle, displayTitle, title, name, fcName,
                                             iconId);
        }
    }

    private Plugin Plugin { get; }

    public HookSetNamePlate(Plugin plugin)
    {
        Plugin = plugin;
        
        SignatureHelper.Initialise(this);

        SetNamePlateHook.Enable();
    }

    public void Dispose()
    {
        SetNamePlateHook.Dispose();
    }
}
