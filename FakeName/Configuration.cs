using System;
using Dalamud.Configuration;

namespace FakeName;

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 0;

    public bool Enabled = false;
    
    public bool PartyMemberReplace = false;

    public string FakeNameText = "";
    
    internal void SaveConfig()
    {
        Service.Interface.SavePluginConfig(this);
    }
}
