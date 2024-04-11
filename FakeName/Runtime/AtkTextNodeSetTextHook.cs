using System;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Hooking;
using Dalamud.Utility.Signatures;
using FakeName.Config;
using FakeName.Utils;

namespace FakeName.Runtime;

public class AtkTextNodeSetTextHook
{
    private readonly Plugin plugin;
    private readonly PluginConfig config;

    [Signature(Signatures.AtkTextNodeSetText, DetourName = nameof(AtkTextNodeSetTextDetour))]
    private readonly Hook<AtkTextNodeSetTextDelegate> hook = null!;

    // Constructor
    internal AtkTextNodeSetTextHook(Plugin plugin, PluginConfig config)
    {
        this.plugin = plugin;
        this.config = config;
        
        Service.Hook.InitializeFromAttributes(this);

        this.hook.Enable();
    }

    public void Dispose()
    {
        this.hook.Disable();
        this.hook.Dispose();
    }

    private void AtkTextNodeSetTextDetour(IntPtr node, IntPtr text)
    {
        try
        {
            AtkTextNodeSetText(node, text);
        }
        catch (Exception ex)
        {
            Service.Log.Error(ex, "AtkTextNodeSetTextDetour encountered a critical error");
            hook.Original(node, text);
        }
    }

    private unsafe void AtkTextNodeSetText(IntPtr node, IntPtr textPtr)
    {
        if (!plugin.Config.Enabled)
        {
            hook.Original(node, textPtr);
            return;
        }

        var character = Service.ClientState.LocalPlayer;
        if (character == null)
        {
            hook.Original(node, textPtr);
            return;
        }

        var charaName = character.Name.TextValue;
        var fcName = character.CompanyTag.TextValue;
        if (!config.TryGetCharacterConfig(charaName, character.HomeWorld.Id, out var characterConfig) || characterConfig == null)
        {
            hook.Original(node, textPtr);
            return;
        }

        var text = SeStringUtils.ReadRawSeString(textPtr);

        foreach (var payload in text.Payloads) {
            switch (payload) {
                /*case PlayerPayload pp:
                    if (pp.PlayerName.Contains(charaName)) {
                        pp.PlayerName = pp.PlayerName.Replace(charaName, characterConfig.FakeNameText);
                    }
                
                    break;*/
                case TextPayload txt:
                    if (txt.Text.Equals(charaName))
                    {
                        txt.Text = txt.Text.Replace(charaName, characterConfig.FakeNameText);
                    }
                    else if (txt.Text.Equals($"{charaName} «{fcName}»"))
                    {
                        txt.Text = txt.Text.Replace(charaName, characterConfig.FakeNameText)
                                      .Replace(fcName, characterConfig.FakeFcNameText);
                    }
                    else if (txt.Text.Contains($"级 {charaName}"))
                    {
                        txt.Text = txt.Text.Replace(charaName, characterConfig.FakeNameText);
                    }
                    else if (txt.Text.Contains(charaName))
                    {
                        Service.Log.Debug($"包含角色名的文本:{txt.Text}");
                    }
                    
                    break;
            }
        }
        
        fixed (byte* newText = text.Encode().Terminate())
        {
            hook.Original(node, (IntPtr)newText);
        }
    }
}
