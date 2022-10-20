using System;
using Dalamud.Game.Command;

namespace Matrix;

internal class Commands : IDisposable {
    private Plugin Plugin { get; }

    internal Commands(Plugin plugin) {
        Plugin = plugin;

        Service.CommandManager.AddHandler("/matrix", new CommandInfo(this.OnCommand) {
            HelpMessage = "打开Matrix",
        });
    }

    public void Dispose() {
        Service.CommandManager.RemoveHandler("/matrix");
    }

    private void OnCommand(string command, string arguments) {
        Plugin.WindowManager.ConfigWindow.Open();
    }
}
