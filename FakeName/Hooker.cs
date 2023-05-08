using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Hooking;
using Dalamud.Logging;
using Dalamud.Utility.Signatures;
using System;
using System.Linq;

namespace FakeName;

public class Hooker
{
    private delegate void AtkTextNodeSetTextDelegate(IntPtr node, IntPtr text);

    /// <summary>
    /// https://github.com/aers/FFXIVClientStructs/blob/main/FFXIVClientStructs/FFXIV/Component/GUI/AtkTextNode.cs#L79
    /// </summary>
    [Signature("E8 ?? ?? ?? ?? 8D 4E 32", DetourName = nameof(AtkTextNodeSetTextDetour))]
    private Hook<AtkTextNodeSetTextDelegate> AtkTextNodeSetTextHook { get; init; }

    private delegate void SetNamePlateDelegate(IntPtr addon, bool isPrefixTitle, 
        bool displayTitle, IntPtr titlePtr, IntPtr namePtr, IntPtr fcNamePtr, int iconId);

    /// <summary>
    /// https://github.com/Haplo064/JobIcons/blob/master/PluginAddressResolver.cs#L34
    /// </summary>
    [Signature("48 89 5C 24 ?? 48 89 6C 24 ?? 56 57 41 54 41 56 41 57 48 83 EC 40 44 0F B6 E2", DetourName = nameof(SetNamePlateDetour))]
    private Hook<SetNamePlateDelegate> SetNamePlateHook { get; init; }

    internal unsafe Hooker()
    {
        SignatureHelper.Initialise(this);

        AtkTextNodeSetTextHook.Enable();
        SetNamePlateHook.Enable();
    }

    public unsafe void Dispose()
    {
        AtkTextNodeSetTextHook.Dispose();
        SetNamePlateHook.Dispose();
    }

    private void AtkTextNodeSetTextDetour(IntPtr node, IntPtr text)
    {
        if (!Service.Config.Enabled)
        {
            AtkTextNodeSetTextHook.Original(node,text);
            return;
        }

        AtkTextNodeSetTextHook.Original(node, Replacer.ChangeName(text));
    }

    private unsafe void SetNamePlateDetour(IntPtr namePlateObjectPtr, bool isPrefixTitle,
        bool displayTitle, IntPtr titlePtr, IntPtr namePtr, IntPtr fcNamePtr, int iconId)
    {
        try
        {
            if (!Service.Config.Enabled)
            {
                SetNamePlateHook.Original(namePlateObjectPtr, isPrefixTitle, displayTitle,
                    titlePtr, namePtr, fcNamePtr, iconId);
                return;
            }

            var nameSe = Replacer.GetSeStringFromPtr(namePtr).TextValue;
            if (Service.ClientState.LocalPlayer != null && GetNames(Service.ClientState.LocalPlayer.Name.TextValue).Contains(nameSe))
            {
                SetNamePlateHook.Original(namePlateObjectPtr, isPrefixTitle, displayTitle,
                    titlePtr, Replacer.GetPtrFromSeString(Service.Config.FakeNameText), fcNamePtr, iconId);
                return;
            }

            if (!Service.Config.AllPlayerReplace)
            {
                SetNamePlateHook.Original(namePlateObjectPtr, isPrefixTitle, displayTitle,
                    titlePtr, namePtr, fcNamePtr, iconId);
                return;
            }

            SetNamePlateHook.Original(namePlateObjectPtr, isPrefixTitle, displayTitle,
                titlePtr, Replacer.GetPtrFromSeString(Replacer.ChangeName(nameSe)), fcNamePtr, iconId);
        }
        catch (Exception ex)
        {
            PluginLog.Error(ex, "Failed to change name plate");
        }
    }

    private static string[] GetNames(string name)
    {
        var names = name.Split(' ');
        if (names.Length != 2) return new string[] { name };

        var first = names[0];
        var last = names[1];
        var firstShort = first.ToUpper()[0] + ".";
        var lastShort = last.ToUpper()[0] + ".";

        return new string[]
        {
            name,
            $"{first} {lastShort}",
            $"{firstShort} {last}",
            $"{firstShort} {lastShort}",
            first, last,
        };
    }
}
