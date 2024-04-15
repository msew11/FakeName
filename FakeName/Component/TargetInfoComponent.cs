using System;
using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Plugin.Services;
using FakeName.Config;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace FakeName.Component;

// tip 找下有没有在某个hud出现时的事件
public class TargetInfoComponent : IDisposable
{
    private readonly PluginConfig config;
    
    private DateTime lastUpdate = DateTime.Today;
    public TargetInfoComponent(PluginConfig config)
    {
        this.config = config;
        
        //Service.Framework.Update += OnUpdate;
        Service.AddonLifecycle.RegisterListener(AddonEvent.PostRequestedUpdate, "_TargetInfoMainTarget", OnTargetInfoAddonPostDraw);
        Service.AddonLifecycle.RegisterListener(AddonEvent.PostRequestedUpdate, "_FocusTargetInfo", OnFocusTargetAddonPostDraw);
        Service.AddonLifecycle.RegisterListener(AddonEvent.PostDraw, "_WideText", OnWideTextAddonPostDraw);
    }

    public void Dispose()
    {
        //Service.Framework.Update -= OnUpdate;
        Service.AddonLifecycle.UnregisterListener(OnTargetInfoAddonPostDraw);
        Service.AddonLifecycle.UnregisterListener(OnFocusTargetAddonPostDraw);
        Service.AddonLifecycle.UnregisterListener(OnWideTextAddonPostDraw);
    }
    
    private void OnUpdate(IFramework framework)
    {
        try
        {
            if (DateTime.Now - lastUpdate > TimeSpan.FromSeconds(1))
            {
                RefreshTargetInfo();
                RefreshFocusTargetInfo();
                RefreshWideText();
                lastUpdate = DateTime.Now;
            }
        }
        catch (Exception e)
        {
            Service.Log.Error("TargetInfoComponent Err", e);
        }
    }
    
    private void OnTargetInfoAddonPostDraw(AddonEvent type, AddonArgs args)
    {
        //Service.Log.Verbose($"{type.ToString()} {args.AddonName}");
        RefreshTargetInfo();
    }
    
    private void OnFocusTargetAddonPostDraw(AddonEvent type, AddonArgs args)
    {
        //Service.Log.Verbose($"{type.ToString()} {args.AddonName}");
        RefreshFocusTargetInfo();
    }
    
    private void OnWideTextAddonPostDraw(AddonEvent type, AddonArgs args)
    {
        //Service.Log.Debug($"{type.ToString()} {args.AddonName}");
        RefreshWideText();
    }

    private unsafe void RefreshTargetInfo()
    {
        if (!config.Enabled)
        {
            return;
        }
        
        var localPlayer = Service.ClientState.LocalPlayer;
        if (localPlayer == null)
        {
            return;
        }
        
        var targetObj = Service.Targets.Target;
        if (targetObj == null)
        {
            return;
        }

        if (targetObj is not PlayerCharacter targetChar)
        {
            return;
        }

        if (!config.TryGetCharacterConfig(targetChar.Name.TextValue, targetChar.HomeWorld.Id, out var characterConfig) ||characterConfig == null)
        {
            return;
        }
        
        AtkUnitBase* targetInfoAddon = (AtkUnitBase*)Service.GameGui.GetAddonByName("_TargetInfoMainTarget", 1);
        if (targetInfoAddon == null || !targetInfoAddon->IsVisible)
        {
            return;
        }
        
        AtkTextNode* textNode = targetInfoAddon->GetTextNodeById(10);
        var text = textNode->NodeText.ToString();
        
        var newName = characterConfig.FakeNameText.Length > 0 ? characterConfig.FakeNameText : targetChar.Name.TextValue;
        var newFcName = characterConfig.FakeFcNameText.Length > 0 ? $"«{characterConfig.FakeFcNameText}»" : $"«{targetChar.CompanyTag.TextValue}»";
        textNode->NodeText.SetString(text.Replace(targetChar.Name.TextValue, newName).Replace($"«{targetChar.CompanyTag.TextValue}»", newFcName));

        RefreshTargetTarget(targetChar, targetInfoAddon);
    }

    private unsafe void RefreshTargetTarget(PlayerCharacter chara, AtkUnitBase* targetInfoAddon)
    {
        var targetObj = chara.TargetObject;
        if (targetObj == null)
        {
            return;
        }

        if (targetObj is not PlayerCharacter targetChar)
        {
            return;
        }

        if (!config.TryGetCharacterConfig(targetChar.Name.TextValue, targetChar.HomeWorld.Id, out var characterConfig) ||characterConfig == null)
        {
            return;
        }
        
        AtkTextNode* textNode = targetInfoAddon->GetTextNodeById(7);
        var text = textNode->NodeText.ToString();
        
        var newName = characterConfig.FakeNameText.Length > 0 ? characterConfig.FakeNameText : targetChar.Name.TextValue;
        textNode->NodeText.SetString(text.Replace(targetChar.Name.TextValue, newName));
    }
    
    private unsafe void RefreshFocusTargetInfo()
    {
        if (!config.Enabled)
        {
            return;
        }
        
        var localPlayer = Service.ClientState.LocalPlayer;
        if (localPlayer == null)
        {
            return;
        }

        var focusTarget = Service.Targets.FocusTarget;
        if (focusTarget == null)
        {
            return;
        }

        if (focusTarget is not PlayerCharacter targetChar)
        {
            return;
        }

        if (!config.TryGetCharacterConfig(targetChar.Name.TextValue, targetChar.HomeWorld.Id, out var characterConfig) ||characterConfig == null)
        {
            return;
        }
        
        AtkUnitBase* focusTargetAddon = (AtkUnitBase*)Service.GameGui.GetAddonByName("_FocusTargetInfo", 1);
        if (focusTargetAddon == null || !focusTargetAddon->IsVisible)
        {
            return;
        }
        
        AtkTextNode* textNode = focusTargetAddon->GetTextNodeById(10);
        var text = textNode->NodeText.ToString();
        
        var newName = characterConfig.FakeNameText.Length > 0 ? characterConfig.FakeNameText : targetChar.Name.TextValue;
        textNode->NodeText.SetString(text.Replace(targetChar.Name.TextValue, newName));
    }

    private unsafe void RefreshWideText()
    {
        if (!config.Enabled)
        {
            return;
        }
        
        var localPlayer = Service.ClientState.LocalPlayer;
        if (localPlayer == null)
        {
            return;
        }
        
        AtkUnitBase* wideTextAddon = (AtkUnitBase*)Service.GameGui.GetAddonByName("_WideText", 2);
        if (wideTextAddon == null)
        {
            return;
        }

        CheckPlayerWideText(localPlayer.Name.TextValue, localPlayer.HomeWorld.Id, wideTextAddon);
        
        // 增加小队其他成员倒计时
    }

    private unsafe void CheckPlayerWideText(string name, uint worldId, AtkUnitBase* wideTextAddon)
    {
        if (!config.TryGetCharacterConfig(name, worldId, out var characterConfig) || characterConfig == null)
        {
            return;
        }
        
        AtkTextNode* textNode = wideTextAddon->GetTextNodeById(3);
        var text = textNode->NodeText.ToString();
        
        var newName = characterConfig.FakeNameText.Length > 0 ? characterConfig.FakeNameText : name;
        if (text.EndsWith($"（{name}）") || text.StartsWith($"{name}取消了"))
        {
            textNode->NodeText.SetString(text.Replace(name, newName));
            Service.Log.Debug($"WideText {textNode->NodeText.ToString()}");
        }
    }
}
