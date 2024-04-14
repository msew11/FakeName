using System;
using Dalamud.Plugin.Services;
using FakeName.Config;
using FakeName.Utils;
using FFXIVClientStructs.FFXIV.Client.Game.Group;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Info;

namespace FakeName.Component;

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
            OnUpdateDeal(framework);
        }
        catch (Exception e)
        {
            Service.Log.Error("PartyListComponent Err");
            Console.WriteLine(e);
        }
    }

    private unsafe void OnUpdateDeal(IFramework framework)
    {
        if (DateTime.Now - lastUpdate > TimeSpan.FromSeconds(5))
        {
            var groupManager = GroupManager.Instance();
            if (groupManager->MemberCount > 0)
            {
                ReplacePartyListHud(groupManager);
            }
            else
            {
                ReplaceCrossPartyListHud();
            }
            
            lastUpdate = DateTime.Now;
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
}
