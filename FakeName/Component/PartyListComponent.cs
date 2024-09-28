using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using Dalamud.Plugin.Services;
using ECommons.DalamudServices;
using FakeName.Utils;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Info;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace FakeName.Component;

public class PartyListComponent : IDisposable
{
    
    private DateTime lastUpdate = DateTime.Today;
    public PartyListComponent()
    {
        Svc.AddonLifecycle.RegisterListener(AddonEvent.PostRequestedUpdate, "_PartyList", OnPartyListUpdate);
    }

    public void Dispose()
    {
        Svc.AddonLifecycle.UnregisterListener(OnPartyListUpdate);
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
            e.Log();
        }
    }
    
    private void OnPartyListUpdate(AddonEvent type, AddonArgs args)
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
            e.Log();
        }

        // Service.Log.Debug($"{type.ToString()} {args.AddonName} {Service.PartyList.Length}");
    }

    /**
     * 刷新小队列表
     */
    public unsafe void RefreshPartyList()
    {
        if (!C.Enabled)
        {
            return;
        }
        
        var localPlayer = Svc.ClientState.LocalPlayer;
        if (localPlayer == null)
        {
            return;
        }
        
        var partyListMemberStructs = GetPartyListAddon();
        var cwProxy = InfoProxyCrossRealm.Instance();
        foreach (var memberStruct in partyListMemberStructs)
        {
            var nodeText = memberStruct.Name->NodeText.ToString();
            var nameNode = memberStruct.Name;
            // Service.Log.Debug($"partyList文本 {nodeText}");
            var match = Regex.Match(nodeText, "^(?:.*级\\s)?(?:\u0002\u0012\u0002Y\u0003)?\\s?(.*?)(?:\u0002\u001a\u0002\u0001\u0003)?$");
            if (match.Success)
            {
                var memberName = match.Groups[1].Value;
                // Service.Log.Debug($"匹配到文本 [{memberName}]");
                if (memberName.Equals(localPlayer.Name.TextValue))
                {
                    ReplaceSelf(memberName, localPlayer.HomeWorld.Id, nameNode);
                }
                else
                {
                    if (Svc.Party.Any())
                    {
                        // 同服小队
                        ReplacePartyListHud(memberName, nameNode);
                    }
                    else
                    {
                        // 跨服小队
                        ReplaceCrossPartyListHud(memberName, nameNode, cwProxy);
                    }
                }
            }
        }
    }

    public unsafe void ReplaceSelf(string memberName, uint world, AtkTextNode* nameNode)
    {
        if (!P.TryGetConfig(memberName, world, out var characterConfig))
        {
            return;
        }
            
        nameNode->NodeText.SetString(nameNode->NodeText.ToString().Replace(memberName, characterConfig.FakeNameText));
    }

    public unsafe void ReplacePartyListHud(string memberName, AtkTextNode* nameNode)
    {
        foreach (var partyMember in Svc.Party)
        {
            if (!partyMember.Name.TextValue.Equals(memberName))
            {
                continue;
            }

            if (!P.TryGetConfig(memberName, partyMember.World.Id, out var characterConfig))
            {
                continue;
            }
            
            nameNode->NodeText.SetString(nameNode->NodeText.ToString().Replace(memberName, characterConfig.FakeNameText));
            break;
        }
    }

    public unsafe void ReplaceCrossPartyListHud(string memberName, AtkTextNode* nameNode, InfoProxyCrossRealm* cwProxy)
    {
        var localIndex = cwProxy->LocalPlayerGroupIndex;
        var crossRealmGroup = cwProxy->CrossRealmGroups[localIndex];
        
        for (var i = 0; i < crossRealmGroup.GroupMemberCount; i++)
        {
            var groupMember = crossRealmGroup.GroupMembers[i];
            var groupMemberName = groupMember.NameString;
            
            if (!memberName.Equals(groupMemberName))
            {
                continue;
            }

            if (!P.TryGetConfig(memberName, (ushort)groupMember.HomeWorld, out var characterConfig))
            {
                continue;
            }
            
            nameNode->NodeText.SetString(nameNode->NodeText.ToString().Replace(memberName, characterConfig.FakeNameText));
            // Service.Log.Debug($"{ nameNode->NodeText.ToString()}");
            break;
        }
    }

    private unsafe List<AddonPartyList.PartyListMemberStruct> GetPartyListAddon()
    {
        var partyListAddon = (AddonPartyList*) Svc.GameGui.GetAddonByName("_PartyList", 1);
        
        List<AddonPartyList.PartyListMemberStruct> p = [
            partyListAddon->PartyMembers[0],
            partyListAddon->PartyMembers[1],
            partyListAddon->PartyMembers[2],
            partyListAddon->PartyMembers[3],
            partyListAddon->PartyMembers[4],
            partyListAddon->PartyMembers[5],
            partyListAddon->PartyMembers[6],
            partyListAddon->PartyMembers[7]
        ];

        return p.Where(n => n.Name->NodeText.ToString().Length > 0).ToList();
    }
}
