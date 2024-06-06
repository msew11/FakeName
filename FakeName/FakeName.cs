using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Dalamud.Plugin;
using ECommons;
using ECommons.Configuration;
using ECommons.DalamudServices;
using ECommons.Logging;
using ECommons.Schedulers;
using ECommons.SimpleGui;
using FakeName.Component;
using FakeName.Data;
using FakeName.Gui;
using FakeName.Hook;
using FakeName.OtterGuiHandlers;

namespace FakeName;

public class FakeName : IDalamudPlugin
{
    public static FakeName P;
    public static Config C => P.NewConfig;
    public static IpcDataManager Idm => P.IpcDataManager;

    public PluginConfig Config;
    public Config NewConfig;
    public IpcDataManager IpcDataManager;
    
    public OtterGuiHandler OtterGuiHandler;
    
    public AtkTextNodeSetTextHook AtkTextNodeSetTextHook;
    public SetNamePlateHook SetNamePlateHook;
    public UpdateNamePlateHook UpdateNamePlateHook;
    public UpdateNamePlateNpcHook UpdateNamePlateNpcHook;
    
    public DutyComponent DutyComponent;
    public TargetInfoComponent TargetInfoComponent;
    public PartyListComponent PartyListComponent;

    public IpcProcessor IpcProcessor;

    public string msg = "null";

    public FakeName(DalamudPluginInterface pi)
    {
        P = this;
        ECommonsMain.Init(pi, this);

        _ = new TickScheduler(() =>
        {
            NewConfig = EzConfig.Init<Config>();
            Config = Svc.PluginInterface.GetPluginConfig() as PluginConfig ?? new PluginConfig();
            OldConfigMove(Config, NewConfig);
            IpcDataManager = new();
            
            EzConfigGui.Init(UI.Draw);
            EzCmd.Add("/fakename", EzConfigGui.Open, "打开FakeName");
            EzCmd.Add("/fn", EzConfigGui.Open, "打开FakeName");
            OtterGuiHandler = new();

            // 尝试修复节点
            RepairFileSystem();
            
            DutyComponent = new();
            TargetInfoComponent = new();
            PartyListComponent = new();
            AtkTextNodeSetTextHook = new();
            SetNamePlateHook = new();
            UpdateNamePlateHook = new(DutyComponent);
            UpdateNamePlateNpcHook = new();
            IpcProcessor = new();

        });
    }

    public void OldConfigMove(PluginConfig oldConfig, Config newConfig)
    {
        if (oldConfig.WorldCharacterDictionary.Count > 0)
        {
            foreach (var (worldId, characters) in P.Config.WorldCharacterDictionary.ToArray())
            {
                foreach (var (name, characterConfig) in characters.ToArray())
                {
                    C.TryAddCharacter(name, worldId, characterConfig);
                    characters.Remove(name);
                    if (characters.Count == 0) {
                        P.Config.WorldCharacterDictionary.Remove(worldId);
                    }
                }
            }
        }
        
        foreach (var (_, characters) in C.WorldCharacterDictionary.ToArray())
        {
            foreach (var (_, characterConfig) in characters.ToArray())
            {
                C.Characters.Add(characterConfig);
            }
        }
    }

    public void RepairFileSystem()
    {
        foreach (var characterConfig in C.Characters)
        {
            var fs = P.OtterGuiHandler.FakeNameFileSystem;

            if (!fs.FindLeaf(characterConfig, out var leaf))
            {
                fs.CreateLeaf(fs.Root, fs.ConvertToName(characterConfig), characterConfig);
                PluginLog.Debug($"CreateLeaf {characterConfig.Name}({characterConfig.World}) {leaf==null}");
            }
        }
    }

    public void Dispose()
    {
        
        Safe(()=>IpcProcessor.Dispose());
        
        // this.ChatMessage.Dispose();
        Safe(()=>SetNamePlateHook.Dispose());
        Safe(()=>UpdateNamePlateHook.Dispose());
        Safe(()=>UpdateNamePlateNpcHook.Dispose());
        Safe(()=>AtkTextNodeSetTextHook.Dispose());
        
        Safe(()=>DutyComponent.Dispose());
        Safe(()=>TargetInfoComponent.Dispose());
        Safe(()=>PartyListComponent.Dispose());
        
        Safe(()=>OtterGuiHandler.Dispose());
        
        
        //this.NamePlates.Dispose();
        //this.Common.Dispose();
        ECommonsMain.Dispose();
        P = null;
    }

    public static string IncognitoModeName(string name)
    {
        if (!C.IncognitoMode)
        {
            return name;
        }
        else
        {
            return name.Substring(0, 1) + "...";
        }
    }
    
    public bool TryGetConfig(string name, uint world, [MaybeNullWhen(false)] out CharacterConfig characterConfig) {
        if (Idm.TryGetCharacterConfig(name, world, out characterConfig))
        {
            PluginLog.Debug($"找到了{characterConfig.Name}的ipc配置：");
            return true;
        }

        if (C.TryGetCharacterConfig(name, world, out characterConfig))
        {
            return true;
        }

        return false;
    }
}
