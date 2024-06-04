using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace FakeName.Data;

public class IpcDataManager
{
    public Dictionary<uint, Dictionary<string, CharacterConfig>> WorldCharacterDictionary = new();
    
    public bool TryGetCharacterConfig(string name, uint world, [MaybeNullWhen(false)] out CharacterConfig characterConfig) {
        characterConfig = null;
        if (!WorldCharacterDictionary.TryGetValue(world, out var w)) return false;
        return w.TryGetValue(name, out characterConfig);
    }
    
    public bool TryAddCharacter(string name, uint homeWorld) {
        if (!WorldCharacterDictionary.ContainsKey(homeWorld)) WorldCharacterDictionary.Add(homeWorld, new Dictionary<string, CharacterConfig>());
        if (WorldCharacterDictionary.TryGetValue(homeWorld, out var world))
        {
            var characterConfig = new CharacterConfig();
            characterConfig.Name = name;
            characterConfig.World = homeWorld;
            return world.TryAdd(name, characterConfig);
        }

        return false;
    }
    
    public bool TryAddCharacter(string name, uint homeWorld, CharacterConfig characterConfig) {
        if (!WorldCharacterDictionary.ContainsKey(homeWorld)) WorldCharacterDictionary.Add(homeWorld, new Dictionary<string, CharacterConfig>());
        if (WorldCharacterDictionary.TryGetValue(homeWorld, out var world))
        {
            characterConfig.World = homeWorld;
            characterConfig.Name = name;
            return world.TryAdd(name, characterConfig);
        }

        return false;
    }
    
    public bool TryGetWorldDic(uint world, [MaybeNullWhen(false)] out Dictionary<string, CharacterConfig> worldDic) {
        worldDic = null;
        return WorldCharacterDictionary.TryGetValue(world, out worldDic);
    }

    public void AddOrUpdCharacter(string name, uint homeWorld, CharacterConfig characterConfig)
    {
        if (!WorldCharacterDictionary.ContainsKey(homeWorld)) WorldCharacterDictionary.Add(homeWorld, new Dictionary<string, CharacterConfig>());
        if (WorldCharacterDictionary.TryGetValue(homeWorld, out var world))
        {
            characterConfig.World = homeWorld;
            characterConfig.Name = name;
            world.Add(name, characterConfig);
        }
    }
}
