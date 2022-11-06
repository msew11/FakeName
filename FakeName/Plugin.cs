using Dalamud.Game.Command;
using Dalamud.Plugin;
using FakeName.GameFunctions;
using FakeName.Services;

namespace FakeName;

public class Plugin : IDalamudPlugin
{
    public string Name => "FakeName";

    internal Configuration Config { get; }
    
    
    //internal readonly XivCommonBase Common;
    //internal NamePlates NamePlates { get; }
    
    internal AtkTextNodeSetText AtkTextNodeSetText { get; }
    internal SetNamePlate SetNamePlate { get; }
    internal ChatMessage ChatMessage { get; }

    internal WindowManager WindowManager { get; }
    internal NameRepository NameRepository { get; }

    private Commands Commands { get; }

    public Plugin(DalamudPluginInterface pluginInterface, CommandManager commandManager)
    {
        pluginInterface.Create<Service>();

        // 加载配置
        this.Config = Service.Interface.GetPluginConfig() as Configuration ?? new Configuration();

        this.WindowManager = new WindowManager(this);
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
