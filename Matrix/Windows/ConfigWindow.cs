using System;
using Dalamud.Interface.Windowing;
using ImGuiNET;

namespace Matrix.Windows;

internal class ConfigWindow : Window, IDisposable
{
    private Plugin Plugin { get; }

    public ConfigWindow(Plugin plugin) : base("Config")
    {
        Plugin = plugin;
    }

    public void Dispose() { }

    public void Open()
    {
        IsOpen = true;
    }

    public override void Draw()
    {
        var fakeName = Plugin.Config.FakeNameText;
        if (ImGui.InputText($"角色名", ref fakeName, 18))
        {
            Plugin.Config.FakeNameText = fakeName;
            Plugin.SaveConfig();
        }
    }
}
