using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Logging;
using Dalamud.Plugin.Services;
using FakeName.Config;
using FakeName.Utils;
using FFXIVClientStructs.FFXIV.Client.Game.Group;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Client.UI.Info;
using Lumina.Excel.GeneratedSheets;

namespace FakeName.Component;

public partial class PartyListComponent : IDisposable
{
    private readonly PluginConfig config;

    public partial class TeamMember
    { }
    
    public List<TeamMember> TeamList;
    
    private DateTime _lastUpdate = DateTime.Today;
    public PartyListComponent(PluginConfig config)
    {
        this.config = config;
        
        TeamList = new List<TeamMember>();
        Service.Framework.Update += OnUpdate;
    }

    public void Dispose()
    {
        Service.Framework.Update -= OnUpdate;
    }
    
    private unsafe void OnUpdate(IFramework framework)
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
        if (DateTime.Now - _lastUpdate > TimeSpan.FromSeconds(5))
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
            
            _lastUpdate = DateTime.Now;
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

    public unsafe void UpdateTeamList()
    {
        this.TeamList = new List<TeamMember>();

        var groupManager = GroupManager.Instance();
        if (groupManager->MemberCount > 0)
        {
            this.AddMembersFromGroupManager(groupManager);
        }
        else
        {
            var cwProxy = InfoProxyCrossRealm.Instance();
            if (cwProxy->IsInCrossRealmParty != 0)
            {
                var localIndex = cwProxy->LocalPlayerGroupIndex;
                this.AddMembersFromCRGroup(cwProxy->CrossRealmGroupArraySpan[localIndex], true);

                for (var i = 0; i < cwProxy->CrossRealmGroupArraySpan.Length; i++)
                {
                    if (i == localIndex)
                    {
                        continue;
                    }

                    this.AddMembersFromCRGroup(cwProxy->CrossRealmGroupArraySpan[i]);
                }
            }
        }

        // Add self if not in party
        if (this.TeamList.Count == 0 && Service.ClientState.LocalPlayer != null)
        {
            var selfName = Service.ClientState.LocalPlayer.Name.TextValue;
            var selfWorldId = Service.ClientState.LocalPlayer.HomeWorld.Id;
            var selfJobId = Service.ClientState.LocalPlayer.ClassJob.Id;
            this.AddTeamMember(selfName, (ushort)selfWorldId, selfJobId, true);
        }
    }
    
    private unsafe void AddMembersFromCRGroup(CrossRealmGroup crossRealmGroup, bool isLocalPlayerGroup = false)
    {
        for (var i = 0; i < crossRealmGroup.GroupMemberCount; i++)
        {
            var groupMember = crossRealmGroup.GroupMembersSpan[i];
            this.AddTeamMember(SeStringUtils.ReadSeString(groupMember.Name).TextValue, (ushort)groupMember.HomeWorld, groupMember.ClassJobId, isLocalPlayerGroup);
        }
    }

    private unsafe void AddMembersFromGroupManager(GroupManager* groupManager)
    {
        var partyMemberList = AgentModule.Instance()->GetAgentHUD()->PartyMemberListSpan;
        var groupManagerIndexLeft = Enumerable.Range(0, groupManager->MemberCount).ToList();

        for (var i = 0; i < groupManager->MemberCount; i++)
        {
            var hudPartyMember = partyMemberList[i];
            var hudPartyMemberNameRaw = hudPartyMember.Name;
            if (hudPartyMemberNameRaw != null)
            {
                var hudPartyMemberName = SeStringUtils.ReadSeString(hudPartyMemberNameRaw).TextValue;
                for (var j = 0; j < groupManager->MemberCount; j++)
                {
                    // handle duplicate names from different worlds
                    if (!groupManagerIndexLeft.Contains(j))
                    {
                        continue;
                    }

                    var partyMember = groupManager->GetPartyMemberByIndex(j);
                    if (partyMember != null)
                    {
                        var partyMemberName = SeStringUtils.ReadSeString(partyMember->Name).TextValue;
                        if (hudPartyMemberName.Equals(partyMemberName))
                        {
                            this.AddTeamMember(partyMemberName, partyMember->HomeWorld, partyMember->ClassJob, true);
                            groupManagerIndexLeft.Remove(j);
                            break;
                        }
                    }
                }
            }
        }

        for (var i = 0; i < 20; i++)
        {
            var allianceMember = groupManager->GetAllianceMemberByIndex(i);
            if (allianceMember != null)
            {
                this.AddTeamMember(SeStringUtils.ReadSeString(allianceMember->Name).TextValue, allianceMember->HomeWorld, allianceMember->ClassJob, false);
            }
        }
    }

    private void AddTeamMember(string fullName, ushort worldId, uint jobId, bool isInParty)
    {
        var world = Service.DataManager.GetExcelSheet<World>()?.FirstOrDefault(x => x.RowId == worldId);
        if (world is not { IsPublic: true })
        {
            return;
        }

        if (fullName == string.Empty)
        {
            return;
        }

        var splitName = fullName.Split(' ');
        if (splitName.Length != 2)
        {
            return;
        }

        this.TeamList.Add(new TeamMember { FirstName = splitName[0], LastName = splitName[1], World = world.Name, JobId = jobId, IsInParty = isInParty });
    }

    private unsafe AddonPartyList.PartyListMemberStruct? GetPartyMemberStruct(uint idx)
    {
        var partyListAddon = (AddonPartyList*) Service.GameGui.GetAddonByName("_PartyList", 1);

        if (partyListAddon == null)
        {
            PluginLog.Warning("PartyListAddon null!");
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
