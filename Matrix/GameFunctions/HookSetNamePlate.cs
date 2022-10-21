using System;
using Dalamud.Hooking;
using Dalamud.Utility.Signatures;
using Matrix.Utils;

namespace Matrix.GameFunctions;

internal class HookSetNamePlate : IDisposable
{
    internal const string Signature =
        "48 89 5C 24 ?? 48 89 6C 24 ?? 56 57 41 54 41 56 41 57 48 83 EC 40 44 0F B6 E2";

    private delegate IntPtr Delegate(
        IntPtr addon, bool isPrefixTitle, bool displayTitle, IntPtr title, IntPtr name, IntPtr fcName, int iconId);

    [Signature(Signature, DetourName = nameof(Detour))]
    private Hook<Delegate> Hook { get; init; } = null!;

    private IntPtr Detour(
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
            return Hook.Original(namePlateObjectPtr, isPrefixTitle, displayTitle, title, fakeNamePtr,
                                 fcName, iconId);
        }
        else
        {
            return Hook.Original(namePlateObjectPtr, isPrefixTitle, displayTitle, title, name, fcName,
                                 iconId);
        }
    }

    private Plugin Plugin { get; }

    public HookSetNamePlate(Plugin plugin)
    {
        Plugin = plugin;
        
        SignatureHelper.Initialise(this);

        Hook.Enable();
    }

    public void Dispose()
    {
        Hook.Disable();
        Hook.Dispose();
    }
}
