using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using FakeName.Component;
using FakeName.Config;
using FakeName.Hook;
using FakeName.Windows;

namespace FakeName;

public class Plugin : IDalamudPlugin
{
    public string Name => "FakeName";
    
    internal PluginConfig Config { get; }
    
    
    //internal readonly XivCommonBase Common;
    //internal NamePlates NamePlates { get; }
    
    internal AtkTextNodeSetTextHook AtkTextNodeSetTextHook { get; }
    // internal SetNamePlateHook SetNamePlateHook { get; }
    internal UpdateNamePlateHook UpdateNamePlateHook { get; }
    internal UpdateNamePlateNpcHook UpdateNamePlateNpcHook { get; }
    // internal ChatMessage ChatMessage { get; }
    
    internal DutyComponent DutyComponent { get; }
    internal PartyListComponent PartyListComponent { get; }

    internal WindowManager WindowManager { get; }
    // internal NameRepository NameRepository { get; }

    private Commands Commands { get; }

    public Plugin(DalamudPluginInterface pluginInterface, ICommandManager commandManager)
    {
        pluginInterface.Create<Service>();

        // 加载配置
        this.Config = Service.Interface.GetPluginConfig() as PluginConfig ?? new PluginConfig();

        this.WindowManager = new WindowManager(this, Config);
        this.Commands = new Commands(this);

        this.DutyComponent = new DutyComponent();
        this.PartyListComponent = new PartyListComponent(Config);
        //this.NameRepository = new NameRepository(this);
        
        //this.Common = new XivCommonBase(Hooks.NamePlates);
        //this.NamePlates = new NamePlates(this);
        this.AtkTextNodeSetTextHook = new AtkTextNodeSetTextHook(this, Config);
        // this.SetNamePlateHook = new SetNamePlateHook(this, Config);
        this.UpdateNamePlateHook = new UpdateNamePlateHook(this, Config, DutyComponent);
        this.UpdateNamePlateNpcHook = new UpdateNamePlateNpcHook(this, Config);
        // his.ChatMessage = new ChatMessage(this);
    }

    public void Dispose()
    {
        
        // this.ChatMessage.Dispose();
        // this.SetNamePlateHook.Dispose();
        this.UpdateNamePlateHook.Dispose();
        this.UpdateNamePlateNpcHook.Dispose();
        this.AtkTextNodeSetTextHook.Dispose();
        
        //this.NamePlates.Dispose();
        //this.Common.Dispose();
        
        this.Commands.Dispose();
        // this.NameRepository.Dispose();
        this.WindowManager.Dispose();
        
        DutyComponent.Dispose();
    }
}
