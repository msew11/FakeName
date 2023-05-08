using Dalamud.Configuration;
using System;

namespace FakeName;

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 0;

    public bool Enabled = false;
    
    public bool AllPlayerReplace = false;

    public string FakeNameText = "";
    
    internal void SaveConfig()
    {
        Service.Interface.SavePluginConfig(this);
    }
}
