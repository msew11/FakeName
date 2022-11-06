using System;
using Dalamud.Hooking;
using Dalamud.Logging;
using Dalamud.Utility.Signatures;
using FakeName.Utils;

namespace FakeName.GameFunctions;

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
        var localName = "";
        if (localPlayer != null)
        {
            localName = localPlayer.Name.TextValue;
        }
    
        var currentName = SeStringUtils.SeStringFromPtr(name);
    
        if (localName == currentName.TextValue)
        {
            var fakeName = SeStringUtils.Text(Plugin.Config.FakeNameText);
            var fakeFcName = SeStringUtils.Text($"«{Plugin.Config.FakeFcNameText}»");
            var fakeNamePtr = SeStringUtils.SeStringToPtr(fakeName);
            var fakeFcNamePtr = SeStringUtils.SeStringToPtr(fakeFcName);
            return Hook.Original(namePlateObjectPtr, isPrefixTitle, displayTitle, title, fakeNamePtr,
                                 fakeFcNamePtr, iconId);
        }

        return Hook.Original(namePlateObjectPtr, isPrefixTitle, displayTitle, title, name, fcName,
                             iconId);
    }

    private Plugin Plugin { get; }

    public HookSetNamePlate(Plugin plugin)
    {
        Plugin = plugin;
        
        SignatureHelper.Initialise(this);
        SeStringUtils.Initialize();

        Hook.Enable();
    }

    public void Dispose()
    {
        Hook.Disable();
        Hook.Dispose();
    }
}
