using Dalamud.Game.Command;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Plugin;
using FakeName.Windows;
using System.Linq;

namespace FakeName;

public class Plugin : IDalamudPlugin
{
    public string Name => "FakeName";
    
    internal Hooker AtkTextNodeSetText { get; }

    internal WindowManager WindowManager { get; }

    public Plugin(DalamudPluginInterface pluginInterface, CommandManager commandManager)
    {
        pluginInterface.Create<Service>();
        Service.Config = Service.Interface.GetPluginConfig() as Configuration ?? new Configuration();

        WindowManager = new WindowManager(this);
        
        AtkTextNodeSetText = new Hooker();

        Service.CommandManager.AddHandler("/fakename", new CommandInfo(this.OnCommand)
        {
            HelpMessage = "打开FakeName",
        });
    }

    public void Dispose()
    {
        Service.CommandManager.RemoveHandler("/fakename");

        AtkTextNodeSetText.Dispose();
        WindowManager.Dispose();
    }

    private void OnCommand(string command, string arguments)
    {
        WindowManager.ConfigWindow.Open();
    }
}
