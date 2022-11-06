using System;
using System.Linq;
using System.Text.RegularExpressions;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Hooking;
using Dalamud.Logging;
using Dalamud.Utility.Signatures;

namespace FakeName.GameFunctions;

public class AtkTextNodeSetText
{
    private Plugin Plugin { get; }

    private static class Signatures
    {
        internal const string AtkTextNodeSetText = "E8 ?? ?? ?? ?? 8D 4E 32";
    }

    // Hook
    private delegate void AtkTextNodeSetTextDelegate(IntPtr node, IntPtr text);

    [Signature(Signatures.AtkTextNodeSetText, DetourName = nameof(AtkTextNodeSetTextDetour))]
    private Hook<AtkTextNodeSetTextDelegate> AtkTextNodeSetTextHook { get; init; } = null!;

    // Event
    private delegate void AtkTextNodeSetTextEventDelegate(IntPtr node, IntPtr text, ref SeString? overwrite);

    private event AtkTextNodeSetTextEventDelegate? OnAtkTextNodeSetText;

    internal AtkTextNodeSetText(Plugin plugin)
    {
        this.Plugin = plugin;
        SignatureHelper.Initialise(this);

        this.AtkTextNodeSetTextHook.Enable();
        this.OnAtkTextNodeSetText += DealAtkTextNodeSetText;
    }

    public void Dispose()
    {
        this.OnAtkTextNodeSetText -= DealAtkTextNodeSetText;
        this.AtkTextNodeSetTextHook.Disable();
        this.AtkTextNodeSetTextHook.Dispose();
    }

    private unsafe void AtkTextNodeSetTextDetour(IntPtr node, IntPtr text)
    {
        SeString? overwrite = null;
        this.OnAtkTextNodeSetText?.Invoke(node, text, ref overwrite);

        if (overwrite != null)
        {
            fixed (byte* newText = overwrite.Encode().Terminate())
            {
                this.AtkTextNodeSetTextHook.Original(node, (IntPtr)newText);
            }

            return;
        }

        this.AtkTextNodeSetTextHook.Original(node, text);
    }

    private static readonly Regex Coords = new(@"^X:\W*\d+.*Y:\W*\d+.*(?:Z:\W*\d+.*)?$", RegexOptions.Compiled);

    private void DealAtkTextNodeSetText(IntPtr node, IntPtr textPtr, ref SeString? overwrite)
    {
        if (!Plugin.Config.Enabled)
        {
            return;
        }

        var text = Util.ReadRawSeString(textPtr);

        if (text.Payloads.All(payload => payload.Type != PayloadType.RawText))
        {
            return;
        }

        var player = Service.ClientState.LocalPlayer;
        if (player == null)
        {
            return;
        }

        var playerName = player.Name.TextValue;
        var textValue = text.TextValue;
        var replaceName = Plugin.NameRepository.GetReplacement();
        if (!textValue.Contains(playerName))
        {
            return;
        }

        PluginLog.Debug($"AtkTextNodeSetText1 替换文本:{textValue}");

        text.ReplacePlayerName(playerName, replaceName);
        overwrite = text;
    }
}
