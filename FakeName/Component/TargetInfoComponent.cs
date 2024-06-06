using System;
using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using Dalamud.Game.ClientState.Objects.SubKinds;
using ECommons.DalamudServices;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace FakeName.Component;

public class TargetInfoComponent : IDisposable
{
    
    private DateTime lastUpdate = DateTime.Today;
    
    public TargetInfoComponent()
    {
        Svc.AddonLifecycle.RegisterListener(AddonEvent.PostUpdate, "_TargetInfo", TargetInfoUpdate);
        Svc.AddonLifecycle.RegisterListener(AddonEvent.PostUpdate, "_TargetInfo", TargetTargetUpdate);
        Svc.AddonLifecycle.RegisterListener(AddonEvent.PostUpdate, "_TargetInfoMainTarget", TargetInfoMainTargetUpdate);
        Svc.AddonLifecycle.RegisterListener(AddonEvent.PostUpdate, "_TargetInfoMainTarget", TargetTargetUpdate);
        Svc.AddonLifecycle.RegisterListener(AddonEvent.PostUpdate, "_FocusTargetInfo", FocusTargetInfoUpdate);
        Svc.AddonLifecycle.RegisterListener(AddonEvent.PostUpdate, "_WideText", WideTextUpdate);
    }

    public void Dispose()
    {
        Svc.AddonLifecycle.UnregisterListener(TargetInfoUpdate);
        Svc.AddonLifecycle.UnregisterListener(TargetTargetUpdate);
        Svc.AddonLifecycle.UnregisterListener(TargetInfoMainTargetUpdate);
        Svc.AddonLifecycle.UnregisterListener(FocusTargetInfoUpdate);
        Svc.AddonLifecycle.UnregisterListener(WideTextUpdate);
    }

    private unsafe void TargetInfoUpdate(AddonEvent type, AddonArgs args)
    {
        var addon = (AtkUnitBase*)args.Addon;
        if (addon->IsVisible)
        {
            RefreshTargetInfo(addon);
        }
    }

    private unsafe void TargetInfoMainTargetUpdate(AddonEvent type, AddonArgs args)
    {
        var addon = (AtkUnitBase*)args.Addon;
        if (addon->IsVisible)
        {
            RefreshIndependentTargetInfo(addon);
        }
    }

    private unsafe void TargetTargetUpdate(AddonEvent type, AddonArgs args)
    {
        var addon = (AtkUnitBase*)args.Addon;
        if (addon->IsVisible)
        {
            var resNode = addon->GetNodeById(3);
            if (resNode != null && resNode->IsVisible)
            {
                RefreshTargetTarget((AtkUnitBase*)args.Addon);
            }
        }
    }
    
    private unsafe void FocusTargetInfoUpdate(AddonEvent type, AddonArgs args)
    {
        var addon = (AtkUnitBase*)args.Addon;
        if (addon->IsVisible)
        {
            RefreshFocusTargetInfo(addon);
        }
    }
    
    private unsafe void WideTextUpdate(AddonEvent type, AddonArgs args)
    {
        var addon = (AtkUnitBase*)args.Addon;
        if (addon->IsVisible)
        {
            RefreshWideText(addon);
        }
    }

    private unsafe bool RefreshTargetInfo(AtkUnitBase* addon)
    {
        if (!C.Enabled)
        {
            return false;
        }
        
        var localPlayer = Svc.ClientState.LocalPlayer;
        if (localPlayer == null)
        {
            return false;
        }
        
        var targetObj = Svc.Targets.Target;
        if (targetObj == null)
        {
            return false;
        }

        if (targetObj is not PlayerCharacter targetChar)
        {
            return false;
        }

        if (!P.TryGetConfig(targetChar.Name.TextValue, targetChar.HomeWorld.Id, out var characterConfig))
        {
            return false;
        }
        
        AtkTextNode* textNode = addon->GetTextNodeById(16);
        var text = textNode->NodeText.ToString();
        
        var oriName = targetChar.Name.TextValue;
        var oriFcName = $"«{targetChar.CompanyTag.TextValue}»";
        var newName = characterConfig.FakeNameText.Length > 0 ? characterConfig.FakeNameText : targetChar.Name.TextValue;
        var newFcName = characterConfig.FakeFcNameText.Length > 0 ? $"«{characterConfig.FakeFcNameText}»" : $"«{targetChar.CompanyTag.TextValue}»";
        
        if (!(newName.Contains(oriName) && text.Contains(newName)))
        {
            text = text.Replace(oriName, newName);
        }
        if (!(newFcName.Contains(oriFcName) && text.Contains(newFcName)))
        {
            text = text.Replace(oriFcName, newFcName);
        }
        textNode->NodeText.SetString(text);

        return true;
    }

    private unsafe bool RefreshIndependentTargetInfo(AtkUnitBase* addon)
    {
        if (!C.Enabled)
        {
            return false;
        }
        
        var localPlayer = Svc.ClientState.LocalPlayer;
        if (localPlayer == null)
        {
            return false;
        }
        
        var targetObj = Svc.Targets.Target;
        if (targetObj == null)
        {
            return false;
        }

        if (targetObj is not PlayerCharacter targetChar)
        {
            return false;
        }

        if (!P.TryGetConfig(targetChar.Name.TextValue, targetChar.HomeWorld.Id, out var characterConfig))
        {
            return false;
        }
        
        AtkTextNode* textNode = addon->GetTextNodeById(10);
        var text = textNode->NodeText.ToString();

        var oriName = targetChar.Name.TextValue;
        var oriFcName = $"«{targetChar.CompanyTag.TextValue}»";
        var newName = characterConfig.FakeNameText.Length > 0 ? characterConfig.FakeNameText : oriName;
        var newFcName = characterConfig.FakeFcNameText.Length > 0 ? $"«{characterConfig.FakeFcNameText}»" : oriFcName;
        
        if (!(newName.Contains(oriName) && text.Contains(newName)))
        {
            text = text.Replace(oriName, newName);
        }
        if (!(newFcName.Contains(oriFcName) && text.Contains(newFcName)))
        {
            text = text.Replace(oriFcName, newFcName);
        }
        textNode->NodeText.SetString(text);

        return true;
    }

    private unsafe bool RefreshTargetTarget(AtkUnitBase* addon)
    {
        if (!C.Enabled)
        {
            return false;
        }
        
        var localPlayer = Svc.ClientState.LocalPlayer;
        if (localPlayer == null)
        {
            return false;
        }
        
        var targetObj = Svc.Targets.Target;
        if (targetObj == null)
        {
            return false;
        }
        
        AtkTextNode* textNode = addon->GetTextNodeById(7);
        var text = textNode->NodeText.ToString();
        
        PlayerCharacter? targetTargetChara = null;
        var targetTargetObj = targetObj.TargetObject;
        if (targetTargetObj != null)
        {
            if (targetTargetObj is PlayerCharacter obj && text.Contains(obj.Name.TextValue))
            {
                targetTargetChara = obj;
            }
            else if (text.Contains(localPlayer.Name.TextValue))
            {
                targetTargetChara = localPlayer;
            }
        }
        else
        {
            targetTargetChara = localPlayer;
        }

        if (targetTargetChara == null)
        {
            return false;
        }

        if (!P.TryGetConfig(targetTargetChara.Name.TextValue, targetTargetChara.HomeWorld.Id, out var characterConfig))
        {
            return false;
        }
        
        var oriName = targetTargetChara.Name.TextValue;
        var newName = characterConfig.FakeNameText.Length > 0 ? characterConfig.FakeNameText : targetTargetChara.Name.TextValue;
        
        if (!(newName.Contains(oriName) && text.Contains(newName)))
        {
            text = text.Replace(oriName, newName);
        }
        textNode->NodeText.SetString(text);

        return true;
    }
    
    private unsafe bool RefreshFocusTargetInfo(AtkUnitBase* addon)
    {
        if (!C.Enabled)
        {
            return false;
        }
        
        var localPlayer = Svc.ClientState.LocalPlayer;
        if (localPlayer == null)
        {
            return false;
        }

        var focusTarget = Svc.Targets.FocusTarget;
        if (focusTarget == null)
        {
            return false;
        }

        if (focusTarget is not PlayerCharacter targetChar)
        {
            return false;
        }

        if (!P.TryGetConfig(targetChar.Name.TextValue, targetChar.HomeWorld.Id, out var characterConfig))
        {
            return false;
        }
        
        AtkTextNode* textNode = addon->GetTextNodeById(10);
        var text = textNode->NodeText.ToString();
        
        var oriName = targetChar.Name.TextValue;
        var newName = characterConfig.FakeNameText.Length > 0 ? characterConfig.FakeNameText : targetChar.Name.TextValue;
        
        if (!(newName.Contains(oriName) && text.Contains(newName)))
        {
            text = text.Replace(oriName, newName);
        }
        textNode->NodeText.SetString(text);

        return true;
    }

    private unsafe bool RefreshWideText(AtkUnitBase* addon)
    {
        if (!C.Enabled)
        {
            return false;
        }
        
        var localPlayer = Svc.ClientState.LocalPlayer;
        if (localPlayer == null)
        {
            return false;
        }
        
        // AtkUnitBase* addon = (AtkUnitBase*)Service.GameGui.GetAddonByName("_WideText", 2);
        // if (addon == null)
        // {
        //     return false;
        // }

        var change = RefreshPlayerWideText(localPlayer.Name.TextValue, localPlayer.HomeWorld.Id, addon);
        if (!change)
        {
            // 小队成员的倒计时
            foreach (var partyMember in Svc.Party)
            {
                change = RefreshPlayerWideText(partyMember.Name.TextValue, partyMember.World.Id, addon);
                if (change)
                {
                    return change;
                }
            }
        }

        return false;
    }

    private unsafe bool RefreshPlayerWideText(string name, uint worldId, AtkUnitBase* wideTextAddon)
    {
        if (!P.TryGetConfig(name, worldId, out var characterConfig))
        {
            return false;
        }
        
        AtkTextNode* textNode = wideTextAddon->GetTextNodeById(3);
        var text = textNode->NodeText.ToString();
        
        var oriName = name;
        var newName = characterConfig.FakeNameText.Length > 0 ? characterConfig.FakeNameText : name;
        if (text.EndsWith($"（{name}）") || text.StartsWith($"{name}取消了"))
        {
            if (!(newName.Contains(oriName) && text.Contains(newName)))
            {
                text = text.Replace(oriName, newName);
            }
            textNode->NodeText.SetString(text);
            return true;
        }

        return false;
    }
}
