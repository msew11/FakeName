using System;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;

namespace FakeName.Hook;

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

        // var change = Plugin.NameRepository.DealReplace(text);
        // if (change)
        // {
        //     Service.Log.Debug($"ChatMessage {text.TextValue}");
        // }
    }
}
