using Dalamud.Game.Command;
using Dalamud.Plugin;
using Matrix.GameFunctions;
using Matrix.Utils;

namespace Matrix;

public class Plugin : IDalamudPlugin
{
    public string Name => "Matrix";

    internal Configuration Config { get; }

    internal HookSetNamePlate HookSetNamePlate { get; }

    internal WindowManager WindowManager { get; }

    private Commands Commands { get; }

    public Plugin(DalamudPluginInterface pluginInterface, CommandManager commandManager)
    {
        pluginInterface.Create<Service>();

        SeStringUtils.Initialize();

        // 加载配置
        Config = Service.Interface.GetPluginConfig() as Configuration ?? new Configuration();
        Config.FakeName = SeStringUtils.Text(Config.FakeNameText);
        
        // 游戏方法
        HookSetNamePlate = new HookSetNamePlate(this);

        WindowManager = new WindowManager(this);
        Commands = new Commands(this);
    }

    public void Dispose()
    {
        WindowManager.Dispose();
        Commands.Dispose();

        HookSetNamePlate.Dispose();
    }

    internal void SaveConfig()
    {
        Service.Interface.SavePluginConfig(Config);
        Config.FakeName = SeStringUtils.Text(Config.FakeNameText);
    }
}
