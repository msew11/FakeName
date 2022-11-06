using Dalamud.Game.Command;
using Dalamud.Plugin;
using FakeName.GameFunctions;

namespace FakeName;

public class Plugin : IDalamudPlugin
{
    public string Name => "FakeName";

    internal Configuration Config { get; }
    
    // internal readonly XivCommonBase Common;
    // internal GameFunctions2 Functions { get; }

    // GameFunctions
    // private HookSetNamePlate HookSetNamePlate { get; }
    private AtkTextNodeSetText AtkTextNodeSetText { get; }

    internal WindowManager WindowManager { get; }
    internal NameRepository NameRepository { get; }
    // private Obscurer Obscurer { get; }
    private Commands Commands { get; }

    public Plugin(DalamudPluginInterface pluginInterface, CommandManager commandManager)
    {
        pluginInterface.Create<Service>();

        // 加载配置
        this.Config = Service.Interface.GetPluginConfig() as Configuration ?? new Configuration();
        
        // XivCommon
        // Common = new XivCommonBase();
        // Functions = new GameFunctions2(this);
        // HookSetNamePlate = new HookSetNamePlate(this);
        this.AtkTextNodeSetText = new AtkTextNodeSetText(this);

        this.WindowManager = new WindowManager(this);
        this.NameRepository = new NameRepository(this);
        // Obscurer = new Obscurer(this);
        this.Commands = new Commands(this);
    }

    public void Dispose()
    {
        this.Commands.Dispose();
        // Obscurer.Dispose();
        this.NameRepository.Dispose();
        this.WindowManager.Dispose();
        
        this.AtkTextNodeSetText.Dispose();

        
        // HookSetNamePlate.Dispose();
        // Functions.Dispose();
        // Common.Dispose();
    }
}
