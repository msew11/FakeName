using System;
using Dalamud.Configuration;
using Dalamud.Game.Text.SeStringHandling;
using Matrix.Utils;

namespace Matrix;

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 0;

    public string FakeNameText { get; set; } = "";

    [NonSerialized]
    public SeString FakeName = SeStringUtils.SeStringFromPtr(SeStringUtils.emptyPtr);
}
