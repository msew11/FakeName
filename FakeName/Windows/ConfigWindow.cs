using System;
using Dalamud.Interface.Windowing;
using ImGuiNET;

namespace FakeName.Windows;

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
        if (ImGui.BeginTabBar("##tabbar"))
        {
            if (ImGui.BeginTabItem("改名"))
            {
                var enabled = Plugin.Config.Enabled;
                if (ImGui.Checkbox("Enable", ref enabled))
                {
                    Plugin.Config.Enabled = enabled;
                    Plugin.Config.SaveConfig();
                }

                var fakeName = Plugin.Config.FakeNameText;
                if (ImGui.InputText("角色名", ref fakeName, 18))
                {
                    Plugin.Config.FakeNameText = fakeName;
                    Plugin.Config.SaveConfig();
                }
            }
        }
    }
}
