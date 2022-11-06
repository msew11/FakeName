using System;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Logging;

namespace FakeName.Services;

internal class ChatMessage : IDisposable
{
    private Plugin Plugin { get; }

    internal ChatMessage(Plugin plugin)
    {
        Plugin = plugin;

        Service.ChatGui.ChatMessage += OnChatMessage;
    }

    public void Dispose()
    {
        Service.ChatGui.ChatMessage -= OnChatMessage;
    }

    private void OnChatMessage(
        XivChatType type, uint senderId, ref SeString sender, ref SeString message, ref bool isHandled)
    {
        ChangeNames(sender);
        ChangeNames(message);
    }

    private void ChangeNames(SeString text)
    {
        if (!Plugin.Config.Enabled)
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
        
        if (!textValue.Contains(playerName))
        {
            return;
        }

        PluginLog.Debug($"ChatMessage 替换文本:{textValue} size={size.ToString()}");
        text.ReplacePlayerName(playerName, replaceName);
    }
}
