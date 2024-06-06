using System;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using ECommons.DalamudServices;
using ECommons.EzIpcManager;
using FakeName.Data;
using Newtonsoft.Json;

namespace FakeName;

public class IpcProcessor : IDisposable
{
    public const uint MajorVersion = 2;
    public const uint MinorVersion = 1;
    
    [EzIPCEvent]
    readonly Action Ready;

    [EzIPCEvent]
    readonly Action Disposing;
    
    [EzIPCEvent]
    public readonly Action<string> LocalCharacterDataChanged;

    public IpcProcessor()
    {
        EzIPC.Init(this);
        NotifyReady();
    }

    public void Dispose()
    {
        NotifyDisposing();
    }

    [EzIPC("ApiVersion")]
    (uint, uint) ApiVersion()
    {
        return (MajorVersion, MinorVersion);
    }


    [EzIPC("GetLocalCharacterData")]
    string GetLocalCharacterData()
    {
        var player = Svc.ClientState.LocalPlayer;
        if (player == null)
        {
            return string.Empty;
        }

        if (!C.Enabled)
        {
            return string.Empty;
        }

        if (!C.TryGetCharacterConfig(player.Name.TextValue, player.HomeWorld.Id, out var characterConfig))
        {
            return string.Empty;
        }

        return JsonConvert.SerializeObject((CharacterData)characterConfig);
    }

    [EzIPC("ClearCharacterData")]
    void ClearCharacterData(Character character)
    {
        if (character is not PlayerCharacter playerCharacter) return;
        var world = playerCharacter.HomeWorld.Id;
        var name = playerCharacter.Name.TextValue;
        if (Idm.TryGetCharacterConfig(name, world, out var characterConfig))
        {
            if (Idm.TryGetWorldDic(world, out var worldDic))
            {
                worldDic.Remove(name);
            }
        }
    }
    
    [EzIPC]
    void SetCharacterData(Character character, string dataJson)
    {
        try
        {
            if (character is not PlayerCharacter playerCharacter) return;
            ClearCharacterData(character);
        
            if (dataJson == string.Empty)
            {
                return;
            }
        
            var titleData = JsonConvert.DeserializeObject<CharacterData>(dataJson);
            if (titleData == null)
            {
                return;
            }
        
            var world = playerCharacter.HomeWorld.Id;
            var name = playerCharacter.Name.TextValue;
            Idm.AddOrUpdCharacter(name, world, titleData);
        }
        catch (Exception e)
        {
            e.Log();
        }
    }
    
    public void ChangedLocalCharacterData(CharacterConfig? characterConfig) {
        var json = characterConfig == null? string.Empty : JsonConvert.SerializeObject((CharacterData)characterConfig);
        LocalCharacterDataChanged(json);
    }

    public void NotifyReady()
    {
        Ready();
    }

    public void NotifyDisposing()
    {
        ChangedLocalCharacterData(null);
        Disposing();
    }
    
    
}
