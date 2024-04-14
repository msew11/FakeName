using System;
using Dalamud.Game.Command;

namespace FakeName;

internal class Commands : IDisposable {
    private Plugin Plugin { get; }

    internal Commands(Plugin plugin) {
        Plugin = plugin;

        Service.CommandManager.AddHandler("/fakename", new CommandInfo(this.OnCommand) {
            HelpMessage = "打开FakeName",
        });

        Service.CommandManager.AddHandler("/fn", new CommandInfo(this.OnCommand) {
            HelpMessage = "打开FakeName",
        });
    }

    public void Dispose() {
        Service.CommandManager.RemoveHandler("/fakename");
        Service.CommandManager.RemoveHandler("/fn");
    }

    private void OnCommand(string command, string arguments) {
        Plugin.WindowManager.ConfigWindow.Open();
    }
}
