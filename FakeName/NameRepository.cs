using System;
using System.Linq;
using Dalamud.Game.Text.SeStringHandling;

namespace FakeName;

internal class NameRepository : IDisposable
{
    private Plugin Plugin { get; }

    internal bool Initialised;

    internal NameRepository(Plugin plugin)
    {
        this.Plugin = plugin;
        Initialised = true;
    }

    public void Dispose() { }
    
    internal string GetReplaceName()
    {
        return Plugin.Config.FakeNameText;
    }
    
    internal string GetReplaceFcName()
    {
        return Plugin.Config.FakeFcNameText;
    }
    
    internal bool DealReplace(SeString text)
    {
        var change = false;
        if (text.Payloads.All(payload => payload.Type != PayloadType.RawText))
        {
            return false;
        }
        
        var localPlayer = Service.ClientState.LocalPlayer;
        if (localPlayer == null)
        {
            return false;
        }

        var localPlayerName = localPlayer.Name.TextValue;
        var textValue = text.TextValue;
        var replaceName = Plugin.NameRepository.GetReplaceName();
        
        // 模糊小队成员
        if (Plugin.Config.PartyMemberReplace)
        {
            foreach (var member in Service.PartyList)
            {
            
                var memberName = member.Name.TextValue;
                if (memberName == localPlayerName)
                {
                    continue;
                }
            
                var jobData = member.ClassJob.GameData;
                if (jobData == null)
                {
                    continue;
                }

                var memberReplace = $"{jobData.Name.RawString}[{member.Name.TextValue[0].ToString()}]";

                if (!textValue.Contains(memberName))
                {
                    continue;
                }

                text.ReplacePlayerName(memberName, memberReplace);
                change = true;
            }
        }
        
        if (!textValue.Contains(localPlayerName) || localPlayerName == replaceName)
        {
            return change;
        }
        
        text.ReplacePlayerName(localPlayerName, replaceName);
        return true;
    }
}
