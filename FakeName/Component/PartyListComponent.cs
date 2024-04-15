using System;
using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using Dalamud.Plugin.Services;
using FakeName.Config;
using FakeName.Utils;
using FFXIVClientStructs.FFXIV.Client.Game.Group;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Info;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace FakeName.Component;

public class PartyListComponent : IDisposable
{
    private readonly PluginConfig config;
    
    private DateTime lastUpdate = DateTime.Today;
    public PartyListComponent(PluginConfig config)
    {
        this.config = config;
        
        // Service.Framework.Update += OnUpdate;
        Service.AddonLifecycle.RegisterListener(AddonEvent.PreSetup, "_PartyList", ObPartyListUpdate);
        Service.AddonLifecycle.RegisterListener(AddonEvent.PostSetup, "_PartyList", ObPartyListUpdate);
        // Service.AddonLifecycle.RegisterListener(AddonEvent.PreUpdate, "_PartyList", ObPartyListUpdate);
        // Service.AddonLifecycle.RegisterListener(AddonEvent.PostUpdate, "_PartyList", ObPartyListUpdate);
        // Service.AddonLifecycle.RegisterListener(AddonEvent.PreDraw, "_PartyList", ObPartyListUpdate);
        // Service.AddonLifecycle.RegisterListener(AddonEvent.PostDraw, "_PartyList", ObPartyListUpdate);
        Service.AddonLifecycle.RegisterListener(AddonEvent.PreFinalize, "_PartyList", ObPartyListUpdate);
        // Service.AddonLifecycle.RegisterListener(AddonEvent.PreRequestedUpdate, "_PartyList", ObPartyListUpdate);
        Service.AddonLifecycle.RegisterListener(AddonEvent.PreRefresh, "_PartyList", ObPartyListUpdate);
        Service.AddonLifecycle.RegisterListener(AddonEvent.PostRefresh, "_PartyList", ObPartyListUpdate);
        // Service.AddonLifecycle.RegisterListener(AddonEvent.PreReceiveEvent, "_PartyList", ObPartyListUpdate);
        // Service.AddonLifecycle.RegisterListener(AddonEvent.PostReceiveEvent, "_PartyList", ObPartyListUpdate);
        
        Service.AddonLifecycle.RegisterListener(AddonEvent.PostRequestedUpdate, "_PartyList", ObPartyListUpdate);
        
        
    }

    public void Dispose()
    {
        // Service.Framework.Update -= OnUpdate;
        Service.AddonLifecycle.UnregisterListener(ObPartyListUpdate);
    }
    
    private void OnUpdate(IFramework framework)
    {
        try
        {
            if (DateTime.Now - lastUpdate > TimeSpan.FromSeconds(5))
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
    
    private void ObPartyListUpdate(AddonEvent type, AddonArgs args)
    {
        RefreshPartyList();
        // Service.Log.Debug($"{type.ToString()} {args.AddonName} {Service.PartyList.Length}");
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
            var cwProxy = InfoProxyCrossRealm.Instance();
            if (cwProxy->IsInCrossRealmParty != 0)
            {
                ReplaceCrossPartyListHud(cwProxy);
            }
        }
    }

    public unsafe void ReplacePartyListHud(GroupManager* groupManager)
    {
        Service.Log.Verbose($"party count:{groupManager->MemberCount}");
        for (var i = 0; i < groupManager->MemberCount; i++)
        {
            var partyMember = groupManager->GetPartyMemberByIndex(i);
            var partyMemberName = SeStringUtils.ReadSeString(partyMember->Name).TextValue;
            var memberStructOptional = GetPartyMemberStruct((uint)i);
            if (!memberStructOptional.HasValue)
            {
                continue;
            }
            
            var memberStruct = memberStructOptional.Value;
            var nameNode = memberStruct.Name;
            
            deal(i, partyMemberName, partyMember->HomeWorld, nameNode);

            // if (partyMemberName.Equals(characterConfig.FakeNameText) || !nameNode->NodeText.ToString().Contains(partyMemberName))
            // {
            //     continue;
            // }

            // var newName = characterConfig.FakeNameText.Length > 0 ? characterConfig.FakeNameText : partyMemberName;
            // var newText = nameNode->NodeText.ToString().Replace(partyMemberName, newName);
            // nameNode->NodeText.SetString(newText);
        }
    }

    public unsafe void ReplaceCrossPartyListHud(InfoProxyCrossRealm* cwProxy)
    {
        var localIndex = cwProxy->LocalPlayerGroupIndex;
        var crossRealmGroup = cwProxy->CrossRealmGroupArraySpan[localIndex];

        Service.Log.Verbose($"crossParty count:{crossRealmGroup.GroupMemberCount}");
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
            
            var memberStruct = memberStructOptional.Value;
            var nameNode = memberStruct.Name;
            
            deal(i, groupMemberName, (ushort) groupMember.HomeWorld, nameNode);
        }
    }

    private unsafe void deal(int idx, string name, uint worldId, AtkTextNode* nameNode)
    {
        var nameText = nameNode->NodeText.ToString();

        var playerName = "";
        if (nameText.Contains(" \u0002\u0012\u0002Y\u0003 "))
        {
            playerName = nameText.Split(" \u0002\u0012\u0002Y\u0003 ")[1];
        }
        else
        {
            playerName = nameText.Split(" ")[1];
        }
        Service.Log.Verbose($"party {idx} [{name}] [{nameText}] [{playerName}]");
        
        
        if (!config.TryGetCharacterConfig(name, worldId, out var characterConfig) || characterConfig == null)
        {
            return;
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
