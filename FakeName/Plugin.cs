using Dalamud.Game.Command;
using Dalamud.Plugin;
using XivCommon;

namespace FakeName;

public class Plugin : IDalamudPlugin
{
    public string Name => "FakeName";

    internal Configuration Config { get; }
    
    internal readonly XivCommonBase Common;
    internal GameFunctions2 Functions { get; }

    // private HookSetNamePlate HookSetNamePlate { get; }

    internal WindowManager WindowManager { get; }
    internal NameRepository NameRepository { get; }
    private Obscurer Obscurer { get; }
    private Commands Commands { get; }

    public Plugin(DalamudPluginInterface pluginInterface, CommandManager commandManager)
    {
        pluginInterface.Create<Service>();

        // 加载配置
        Config = Service.Interface.GetPluginConfig() as Configuration ?? new Configuration();
        
        // XivCommon
        Common = new XivCommonBase();
        Functions = new GameFunctions2(this);
        
        // 游戏方法
        // HookSetNamePlate = new HookSetNamePlate(this);

        WindowManager = new WindowManager(this);
        NameRepository = new NameRepository(this);
        Obscurer = new Obscurer(this);
        Commands = new Commands(this);
    }

    public void Dispose()
    {
        Commands.Dispose();
        Obscurer.Dispose();
        NameRepository.Dispose();
        WindowManager.Dispose();

        // HookSetNamePlate.Dispose();
        Functions.Dispose();
        Common.Dispose();
    }
}
