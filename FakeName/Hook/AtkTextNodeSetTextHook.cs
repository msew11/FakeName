using System;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Hooking;
using Dalamud.Utility.Signatures;
using FakeName.Config;
using FakeName.Utils;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;

namespace FakeName.Hook;

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
            TryUpdLoginUi(node, textPtr);
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
                    // else if (txt.Text.Equals($"{charaName} «{fcName}»"))
                    // {
                    //     txt.Text = txt.Text.Replace(charaName, characterConfig.FakeNameText)
                    //                   .Replace(fcName, characterConfig.FakeFcNameText);
                    // }
                    // else if (txt.Text.Contains($"级 {charaName}"))
                    // {
                    //     txt.Text = txt.Text.Replace(charaName, characterConfig.FakeNameText);
                    // }
                    else if (txt.Text.Contains(charaName))
                    {
                        Service.Log.Verbose($"包含角色名的文本:{txt.Text}");
                    }
                    
                    break;
            }
        }
        
        fixed (byte* newText = text.Encode().Terminate())
        {
            hook.Original(node, (IntPtr)newText);
        }
    }
    
    private unsafe void TryUpdLoginUi(IntPtr node, IntPtr textPtr)
    {
        var agent = AgentLobby.Instance();
        if (agent == null)
        {
            hook.Original(node, textPtr);
            return;
        }
        
        config.TryGetWorldDic(agent->WorldId, out var worldDic);
        if (worldDic == null)
        {
            hook.Original(node, textPtr);
            return;
        }
        
        var text = SeStringUtils.ReadRawSeString(textPtr);
        Service.Log.Verbose($"包含角色名的文本:{text.TextValue}");
        foreach (var pair in worldDic)
        {
            var charaName = pair.Key;
            var characterConfig = pair.Value;
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
                            Service.Log.Debug($"world[{agent->WorldId}] 替换{txt.Text} {charaName}->{characterConfig.FakeNameText}");
                            txt.Text = txt.Text.Replace(charaName, characterConfig.FakeNameText);
                        }
                        else if (txt.Text.Equals($"要以{charaName}登录吗？"))
                        {
                            txt.Text = txt.Text.Replace(charaName, characterConfig.FakeNameText);
                        }
                        else if (txt.Text.Contains(charaName))
                        {
                            Service.Log.Verbose($"包含角色名的文本:{txt.Text}");
                        }
                    
                        break;
                }
            }
        }
        
        fixed (byte* newText = text.Encode().Terminate())
        {
            hook.Original(node, (IntPtr)newText);
        }
    }
}
