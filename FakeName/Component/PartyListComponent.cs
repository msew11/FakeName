using System;
using Dalamud.Plugin.Services;
using FakeName.Config;
using FakeName.Utils;
using FFXIVClientStructs.FFXIV.Client.Game.Group;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Info;

namespace FakeName.Component;

// tip 找下有没有在某个hud出现时的事件
public partial class PartyListComponent : IDisposable
{
    private readonly PluginConfig config;
    
    private DateTime lastUpdate = DateTime.Today;
    public PartyListComponent(PluginConfig config)
    {
        this.config = config;
        
        Service.Framework.Update += OnUpdate;
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
                RefreshPartyList();
                lastUpdate = DateTime.Now;
            }
        }
        catch (Exception e)
        {
            Service.Log.Error("PartyListComponent Err", e);
        }
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

            var newName = characterConfig.FakeNameText.Length > 0 ? characterConfig.FakeNameText : partyMemberName;
            var newText = nameNode->NodeText.ToString().Replace(partyMemberName, newName);
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
                Service.Log.Debug("crossParty a");
                continue;
            }
            
            if (!config.TryGetCharacterConfig(groupMemberName, (uint)groupMember.HomeWorld, out var characterConfig) || characterConfig == null)
            {
                Service.Log.Debug("crossParty b");
                continue;
            }
            
            var memberStruct = memberStructOptional.Value;
            var nameNode = memberStruct.Name;
            var newName = characterConfig.FakeNameText.Length > 0 ? characterConfig.FakeNameText : groupMemberName;

            if (nameNode->NodeText.ToString().EndsWith(newName))
            {
                // Service.Log.Debug($"crossParty c {groupMemberName} {characterConfig.FakeNameText} {nameNode->NodeText.ToString()}");
                continue;
            }
            
            
            Service.Log.Debug($"crossParty {nameNode->NodeText.ToString()}");
            Service.Log.Debug($"crossParty {newName}");
            
            var newText = nameNode->NodeText.ToString().Replace(groupMemberName, newName);
            nameNode->NodeText.SetString(newText);
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
}
