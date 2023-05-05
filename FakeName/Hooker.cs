using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Hooking;
using Dalamud.Utility.Signatures;
using System;

namespace FakeName;

public class Hooker
{
    private delegate void AtkTextNodeSetTextDelegate(IntPtr node, IntPtr text);

    /// <summary>
    /// https://github.com/aers/FFXIVClientStructs/blob/main/FFXIVClientStructs/FFXIV/Component/GUI/AtkTextNode.cs#L79
    /// </summary>
    [Signature("E8 ?? ?? ?? ?? 8D 4E 32", DetourName = nameof(AtkTextNodeSetTextDetour))]
    private Hook<AtkTextNodeSetTextDelegate> AtkTextNodeSetTextHook { get; init; } = null!;

    private delegate void SetNamePlateDelegate(
    IntPtr addon, bool isPrefixTitle, bool displayTitle,
    IntPtr titlePtr, IntPtr namePtr, IntPtr fcNamePtr, int iconId);

    /// <summary>
    /// https://github.com/Haplo064/JobIcons/blob/master/PluginAddressResolver.cs#L34
    /// </summary>
    [Signature("48 89 5C 24 ?? 48 89 6C 24 ?? 56 57 41 54 41 56 41 57 48 83 EC 40 44 0F B6 E2", DetourName = nameof(SetNamePlateDetour))]
    private Hook<SetNamePlateDelegate> SetNamePlateHook { get; init; } = null!;

    internal unsafe Hooker()
    {
        SignatureHelper.Initialise(this);

        AtkTextNodeSetTextHook.Enable();
        SetNamePlateHook.Enable();
        Service.ChatGui.ChatMessage += OnChatMessage;
    }

    public unsafe void Dispose()
    {
        AtkTextNodeSetTextHook.Dispose();
        SetNamePlateHook.Dispose();
        Service.ChatGui.ChatMessage -= OnChatMessage;

    }

    private void AtkTextNodeSetTextDetour(IntPtr node, IntPtr text)
    {
        AtkTextNodeSetTextHook.Original(node, Replacer.ChangeName(text));
    }

    private unsafe void SetNamePlateDetour(
    IntPtr namePlateObjectPtr, bool isPrefixTitle, bool displayTitle,
    IntPtr titlePtr, IntPtr namePtr, IntPtr fcNamePtr, int iconId)
    {
        SetNamePlateHook.Original(
            namePlateObjectPtr, isPrefixTitle, displayTitle,
            titlePtr, Replacer.ChangeName(namePtr), fcNamePtr, iconId
        );
    }

    private void OnChatMessage(
    XivChatType type, uint senderId, ref SeString sender, ref SeString message, ref bool isHandled)
    {
        Replacer.ChangeSeString(ref sender);
    }
}
