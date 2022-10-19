using Dalamud.Game.Command;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using Matrix.Config;
using Matrix.Hooks;
using Matrix.Utils;
using Matrix.Windows;

namespace Matrix;

public sealed class Plugin : IDalamudPlugin
{
    public string Name => "Matrix";
    private const string CommandName = "/matrix";

    private DalamudPluginInterface PluginInterface { get; init; }

    private CommandManager CommandManager { get; init; }

    public readonly WindowSystem WindowSystem = new("Matrix");
    private readonly MainWindow mainWindow;
    private readonly ConfigWindow configWindow;
    
    // Hooks
    private readonly SetNamePlateHook setNamePlateHook;

    public Plugin(DalamudPluginInterface pluginInterface, CommandManager commandManager) {
        this.PluginInterface = pluginInterface;
        this.CommandManager = commandManager;
        
        pluginInterface.Create<Service>();
        
        // Utils初始化
        SeStringUtils.Initialize();

        // 加载配置
        Service.Config = this.PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
        Service.Config.MyFakeName = SeStringUtils.Text(Service.Config.FakeName);
        
        // 
        Service.Address = new PluginAddressResolver();
        Service.Address.Setup();

        // 初始化hooks
        this.setNamePlateHook = new SetNamePlateHook(Service.Address);
        setNamePlateHook.Enable();

        // you might normally want to embed resources and load them from the manifest stream
        // var imagePath = Path.Combine(PluginInterface.AssemblyLocation.Directory?.FullName!, "goat.png");
        // var goatImage = this.PluginInterface.UiBuilder.LoadImage(imagePath);

        // 初始化窗口
        mainWindow = new MainWindow(this);
        WindowSystem.AddWindow(mainWindow);

        configWindow = new ConfigWindow(this);
        WindowSystem.AddWindow(configWindow);

        this.CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
        {
            HelpMessage = "打开Matrix"
        });

        this.PluginInterface.UiBuilder.Draw += DrawUi;
        this.PluginInterface.UiBuilder.OpenConfigUi += DrawConfigUi;
    }

    public void Dispose()
    {
        this.setNamePlateHook.Dispose();
        
        this.WindowSystem.RemoveAllWindows();
        this.CommandManager.RemoveHandler(CommandName);
    }

    private void OnCommand(string command, string args)
    {
        mainWindow.IsOpen = true;
    }

    private void DrawUi()
    {
        this.WindowSystem.Draw();
    }

    public void DrawConfigUi()
    {
        configWindow.IsOpen = true;
    }
}
