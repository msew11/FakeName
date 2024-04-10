using System;
using System.Collections.Generic;
using Dalamud.Configuration;

namespace FakeName.Config;

[Serializable]
public class PluginConfig : IPluginConfiguration
{
    
    public int Version { get; set; } = 0;

    public bool Enabled { get; set; } = false;
    
    public bool PartyMemberReplace { get; set; } = false;

    public string FakeNameText { get; set; } = "";
    
    public string FakeFcNameText { get; set; } = "";

    public Dictionary<uint, Dictionary<string, CharacterConfig>> WorldCharacterDictionary = new();

    internal void SaveConfig()
    {
        Service.Interface.SavePluginConfig(this);
    }
    
    public bool TryAddCharacter(string name, uint homeWorld) {
        if (!WorldCharacterDictionary.ContainsKey(homeWorld)) WorldCharacterDictionary.Add(homeWorld, new Dictionary<string, CharacterConfig>());
        if (WorldCharacterDictionary.TryGetValue(homeWorld, out var world)) {
            return world.TryAdd(name, new CharacterConfig());
        }

        return false;
    }
}
