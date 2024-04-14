using System;
using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Plugin.Services;
using FakeName.Config;
using FakeName.Utils;
using FFXIVClientStructs.FFXIV.Client.Game.Group;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Info;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace FakeName.Component;

// tip 找下有没有在某个hud出现时的事件
public partial class HudComponent : IDisposable
{
    private readonly PluginConfig config;
    
    private DateTime lastUpdate = DateTime.Today;
    public HudComponent(PluginConfig config)
    {
        this.config = config;
        
        Service.Framework.Update += OnUpdate;
        Service.AddonLifecycle.RegisterListener(AddonEvent.PostRequestedUpdate, "_TargetInfoMainTarget", OnTargetInfoAddonPostDraw);
        Service.AddonLifecycle.RegisterListener(AddonEvent.PostRequestedUpdate, "_FocusTargetInfo", OnFocusTargetAddonPostDraw);
        Service.AddonLifecycle.RegisterListener(AddonEvent.PostRequestedUpdate, "_WideText", OnWideTextAddonPostDraw);
    }

    public void Dispose()
    {
        Service.Framework.Update -= OnUpdate;
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
                RefreshPartyList();
                lastUpdate = DateTime.Now;
            }
        }
        catch (Exception e)
        {
            Service.Log.Error("PartyListComponent Err");
            Console.WriteLine(e);
        }
    }
    
    private void OnTargetInfoAddonPostDraw(AddonEvent type, AddonArgs args)
    {
        Service.Log.Debug($"{type.ToString()} {args}");
        RefreshTargetInfo();
    }
    
    private void OnFocusTargetAddonPostDraw(AddonEvent type, AddonArgs args)
    {
        Service.Log.Debug($"{type.ToString()} {args}");
        RefreshFocusTargetInfo();
    }
    
    private void OnWideTextAddonPostDraw(AddonEvent type, AddonArgs args)
    {
        Service.Log.Debug($"{type.ToString()} {args}");
        RefreshWideText();
    }


    /**
     * 刷新小队列表
     */
    public unsafe void RefreshPartyList()
    {
        if (!config.Enabled)
        {
            return;
        }
        
        var groupManager = GroupManager.Instance();
        if (groupManager->MemberCount > 0)
        {
            ReplacePartyListHud(groupManager);
        }
        else
        {
            ReplaceCrossPartyListHud();
        }
    }

    public unsafe void ReplacePartyListHud(GroupManager* groupManager)
    {
        Service.Log.Debug($"party count:{groupManager->MemberCount}");
        for (var i = 0; i < groupManager->MemberCount; i++)
        {
            var partyMember = groupManager->GetPartyMemberByIndex(i);
            var partyMemberName = SeStringUtils.ReadSeString(partyMember->Name).TextValue;
            var memberStructOptional = GetPartyMemberStruct((uint)i);
            if (!memberStructOptional.HasValue)
            {
                continue;
            }
            
            if (!config.TryGetCharacterConfig(partyMemberName, partyMember->HomeWorld, out var characterConfig) || characterConfig == null)
            {
                continue;
            }
            
            var memberStruct = memberStructOptional.Value;
            var nameNode = memberStruct.Name;

            if (partyMemberName.Equals(characterConfig.FakeNameText) || !nameNode->NodeText.ToString().Contains(partyMemberName))
            {
                continue;
            }

            var newText = nameNode->NodeText.ToString().Replace(partyMemberName, characterConfig.FakeNameText);
            nameNode->NodeText.SetString(newText);
            Service.Log.Debug($"party {i} {partyMemberName} {partyMember->HomeWorld}");
        }
    }

    public unsafe void ReplaceCrossPartyListHud()
    {
        var cwProxy = InfoProxyCrossRealm.Instance();
        if (cwProxy->IsInCrossRealmParty == 0)
        {
            // 不在跨服团队
            return;
        }
        
        var localIndex = cwProxy->LocalPlayerGroupIndex;
        var crossRealmGroup = cwProxy->CrossRealmGroupArraySpan[localIndex];

        Service.Log.Debug($"crossParty count:{crossRealmGroup.GroupMemberCount}");
        for (var i = 0; i < crossRealmGroup.GroupMemberCount; i++)
        {
            var groupMember = crossRealmGroup.GroupMembersSpan[i];
            var groupMemberName = SeStringUtils.ReadSeString(groupMember.Name).TextValue;
            var memberStructOptional = GetPartyMemberStruct((uint)i);
            if (!memberStructOptional.HasValue)
            {
                continue;
            }
            
            if (!config.TryGetCharacterConfig(groupMemberName, (uint)groupMember.HomeWorld, out var characterConfig) || characterConfig == null)
            {
                continue;
            }
            
            var memberStruct = memberStructOptional.Value;
            var nameNode = memberStruct.Name;

            if (groupMemberName.Equals(characterConfig.FakeNameText) || !nameNode->NodeText.ToString().Contains(groupMemberName))
            {
                continue;
            }
            
            var newText = nameNode->NodeText.ToString().Replace(groupMemberName, characterConfig.FakeNameText);
            nameNode->NodeText.SetString(newText);
            Service.Log.Debug($"crossParty {i} {groupMemberName} {groupMember.HomeWorld}");
        }
    }

    private unsafe AddonPartyList.PartyListMemberStruct? GetPartyMemberStruct(uint idx)
    {
        var partyListAddon = (AddonPartyList*) Service.GameGui.GetAddonByName("_PartyList", 1);

        if (partyListAddon == null)
        {
            Service.Log.Warning("PartyListAddon null!");
            return null;
        }

        return idx switch
        {
            0 => partyListAddon->PartyMember.PartyMember0,
            1 => partyListAddon->PartyMember.PartyMember1,
            2 => partyListAddon->PartyMember.PartyMember2,
            3 => partyListAddon->PartyMember.PartyMember3,
            4 => partyListAddon->PartyMember.PartyMember4,
            5 => partyListAddon->PartyMember.PartyMember5,
            6 => partyListAddon->PartyMember.PartyMember6,
            7 => partyListAddon->PartyMember.PartyMember7,
            _ => throw new ArgumentException($"Invalid index: {idx}")
        };
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
            Service.Log.Debug($"{textNode->NodeText.ToString()}");
        }
    }
}
