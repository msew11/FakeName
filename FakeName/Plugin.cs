using Dalamud.Game.Command;
using Dalamud.Plugin;
using FakeName.Windows;

namespace FakeName;

public class Plugin : IDalamudPlugin
{
    public string Name => "FakeName";
    
    internal Hooker Hooker { get; }

    internal WindowManager WindowManager { get; }

    public Plugin(DalamudPluginInterface pluginInterface, CommandManager commandManager)
    {
        pluginInterface.Create<Service>();
        Service.Config = Service.Interface.GetPluginConfig() as Configuration ?? new Configuration();

        WindowManager = new WindowManager();
        
        Hooker = new Hooker();

        Service.CommandManager.AddHandler("/fakename", new CommandInfo(OnCommand)
        {
            HelpMessage = "Open a config window about fake name.",
        });
    }

    public void Dispose()
    {
        Service.CommandManager.RemoveHandler("/fakename");

        Hooker.Dispose();
        WindowManager.Dispose();
    }

    private void OnCommand(string command, string arguments)
    {
        WindowManager.ConfigWindow.Open();
    }
}
