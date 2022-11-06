using System;
using System.Linq;
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

    // Constructor
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
        var size = text.Payloads.Count;
        var replaceName = Plugin.NameRepository.GetReplaceName();
        
        // size 主要为了过滤掉聊天，不然所有聊天历史中的名字都会被替换，聊天的替换走ChatMessage
        if (!textValue.Contains(playerName) || size > 10)
        {
            return;
        }

        PluginLog.Debug($"AtkTextNodeSetText 替换文本:{textValue} size={size.ToString()}");

        text.ReplacePlayerName(playerName, replaceName);
        overwrite = text;
    }
}
