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
        var localPlayer = Service.ClientState.LocalPlayer;
        var localName = "";
        var localFcName = "";
        if (localPlayer != null)
        {
            localName = localPlayer.Name.TextValue;
            localFcName = localPlayer.CompanyTag.TextValue;
        }

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
                if (ImGui.InputText("角色名", ref fakeName, 100))
                {
                    Plugin.Config.FakeNameText = fakeName;
                    Plugin.Config.SaveConfig();
                }

                var fakeFcName = Plugin.Config.FakeFcNameText;
                if (ImGui.InputText("部队简称", ref fakeFcName, 100))
                {
                    Plugin.Config.FakeFcNameText = fakeFcName;
                    Plugin.Config.SaveConfig();
                }

                if (ImGui.Button("重置"))
                {
                    Plugin.Config.FakeNameText = localName;
                    Plugin.Config.FakeFcNameText = localFcName;
                    Plugin.Config.SaveConfig();
                }
            }
        }

        // foreach (var gameObject in Service.ObjectTable)
        // {
        //     ImGui.Text($"{gameObject.ObjectId.ToString()} {gameObject.Name.TextValue}");
        // }
    }
}
