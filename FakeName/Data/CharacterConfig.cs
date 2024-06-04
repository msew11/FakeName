using System;
using ECommons.DalamudServices;
using World = Lumina.Excel.GeneratedSheets.World;

namespace FakeName.Data;

public class CharacterConfig
{
    internal string Id => Guid.ToString();
    public Guid Guid = Guid.NewGuid();

    public uint World = 0;

    public string Name = "";

    public bool IconReplace { get; set; } = false;

    public int IconId { get; set; } = 0;

    public string FakeNameText { get; set; } = "";

    public bool HideFcName { get; set; } = false;

    public string FakeFcNameText { get; set; } = "";

    internal string WorldName()
    {
        var world = Svc.Data.GetExcelSheet<World>()?.GetRow(World);
        if (world == null)
        {
            return "Unknown";
        }

        return world.Name.RawString;
    }
    internal string IncognitoName()
    {
        if (!C.IncognitoMode)
        {
            return Name;
        }

        if (FakeNameText.Length == 0)
        {
            return Name.Substring(0, 1) + "...";
        }

        return FakeNameText;
    }
}

public class CharacterData
{
    public bool IconReplace { get; set; } = false;

    public int IconId { get; set; } = 0;

    public string FakeNameText { get; set; } = "";

    public bool HideFcName { get; set; } = false;

    public string FakeFcNameText { get; set; } = "";

    public static implicit operator CharacterData(CharacterConfig config) => new() {
        IconReplace = config.IconReplace,
        IconId = config.IconId,
        FakeNameText = config.FakeNameText,
        HideFcName = config.HideFcName,
        FakeFcNameText = config.FakeFcNameText
    };
    public static implicit operator CharacterConfig(CharacterData data) => new() {
        IconReplace = data.IconReplace,
        IconId = data.IconId,
        FakeNameText = data.FakeNameText,
        HideFcName = data.HideFcName,
        FakeFcNameText = data.FakeFcNameText,
    };
}
