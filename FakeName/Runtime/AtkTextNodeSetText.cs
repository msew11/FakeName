using System;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Hooking;
using Dalamud.Utility.Signatures;
using FakeName.Utils;

namespace FakeName.Runtime;

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
        Service.Hook.InitializeFromAttributes(this);

        this.AtkTextNodeSetTextHook.Enable();
        this.OnAtkTextNodeSetText += DealAtkTextNodeSetText;
        //this.OnAtkTextNodeSetText += ResolvePartyMemberName;
    }

    public void Dispose()
    {
        //this.OnAtkTextNodeSetText -= ResolvePartyMemberName;
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

        var text = SeStringUtils.ReadRawSeString(textPtr);

        if (text.Payloads.Count > 20)
        {
            return;
        }
        
        var change = Plugin.NameRepository.DealReplace(text);

        if (change)
        {
            Service.Log.Debug($"AtkTextNodeSetText {text.TextValue}");
            overwrite = text;
        }
    }
}
