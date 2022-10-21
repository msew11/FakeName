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
        if (ImGui.BeginTabBar("##tabbar"))
        {
            if (ImGui.BeginTabItem("改名"))
            {
                ImGui.Text("角色名");
                
                var fakeName = Plugin.Config.FakeNameText;
                if (ImGui.InputText("", ref fakeName, 18))
                {
                    Plugin.Config.FakeNameText = fakeName;
                    Plugin.SaveConfig();
                }
            }
        }
    }
}
