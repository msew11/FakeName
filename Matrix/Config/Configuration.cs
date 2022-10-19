using System;
using Dalamud.Configuration;
using Dalamud.Game.Text.SeStringHandling;
using Matrix.Utils;

namespace Matrix.Config;

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 0;

    public string FakeName { get; set; } = "";

    [NonSerialized]
    public SeString MyFakeName = SeStringUtils.SeStringFromPtr(SeStringUtils.emptyPtr);

    public void Save()
    {
        this.MyFakeName = SeStringUtils.Text(this.FakeName);
        Service.Interface.SavePluginConfig(this);
    }
}
