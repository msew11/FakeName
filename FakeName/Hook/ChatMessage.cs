using System;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using ECommons.DalamudServices;

namespace FakeName.Hook;

internal class ChatMessage : IDisposable
{
    private FakeName FakeName { get; }

    internal ChatMessage(FakeName fakeName)
    {
        FakeName = fakeName;

        Svc.Chat.ChatMessage += OnChatMessage;
    }

    public void Dispose()
    {
        Svc.Chat.ChatMessage -= OnChatMessage;
    }

    private void OnChatMessage(
        XivChatType type, uint senderId, ref SeString sender, ref SeString message, ref bool isHandled)
    {
        ChangeNames(sender);
        ChangeNames(message);
    }

    private void ChangeNames(SeString text)
    {
        if (!FakeName.Config.Enabled)
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
