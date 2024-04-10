using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using FakeName.Config;
using FakeName.GameFunctions;
using FakeName.Services;
using FakeName.Windows;

namespace FakeName;

public class Plugin : IDalamudPlugin
{
    public string Name => "FakeName";

    internal PluginConfig Config { get; }
    
    
    //internal readonly XivCommonBase Common;
    //internal NamePlates NamePlates { get; }
    
    internal AtkTextNodeSetText AtkTextNodeSetText { get; }
    internal SetNamePlate SetNamePlate { get; }
    internal ChatMessage ChatMessage { get; }

    internal WindowManager WindowManager { get; }
    internal NameRepository NameRepository { get; }

    private Commands Commands { get; }

    public Plugin(DalamudPluginInterface pluginInterface, ICommandManager commandManager)
    {
        pluginInterface.Create<Service>();

        // 加载配置
        this.Config = Service.Interface.GetPluginConfig() as PluginConfig ?? new PluginConfig();

        this.WindowManager = new WindowManager(this, Config);
        this.NameRepository = new NameRepository(this);
        this.Commands = new Commands(this);
        
        //this.Common = new XivCommonBase(Hooks.NamePlates);
        //this.NamePlates = new NamePlates(this);
        this.AtkTextNodeSetText = new AtkTextNodeSetText(this);
        this.SetNamePlate = new SetNamePlate(this);
        this.ChatMessage = new ChatMessage(this);
    }

    public void Dispose()
    {
        this.ChatMessage.Dispose();
        this.SetNamePlate.Dispose();
        this.AtkTextNodeSetText.Dispose();
        
        //this.NamePlates.Dispose();
        //this.Common.Dispose();
        
        this.Commands.Dispose();
        this.NameRepository.Dispose();
        this.WindowManager.Dispose();
    }
}
