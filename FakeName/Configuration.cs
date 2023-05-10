using Dalamud.Configuration;
using System;
using System.Collections.Generic;

namespace FakeName;

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 0;

    public bool Enabled = false;
    
    public bool AllPlayerReplace = false;

    public string FakeNameText = "";

    public HashSet<string> CharacterNames = new HashSet<string>();
    
    internal void SaveConfig()
    {
        Service.Interface.SavePluginConfig(this);
    }
}
