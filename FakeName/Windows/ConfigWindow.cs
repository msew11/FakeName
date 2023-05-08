using Dalamud.Interface.Windowing;
using ImGuiNET;

namespace FakeName.Windows;

internal class ConfigWindow : Window
{
    public ConfigWindow() : base("Fake Name")
    {
    }

    public void Open() => IsOpen = true;

    public override void Draw()
    {
        var localPlayer = Service.ClientState.LocalPlayer;
        var localName = "";
        if (localPlayer != null)
        {
            localName = localPlayer.Name.TextValue;
        }

        if (ImGui.Checkbox("Enable", ref Service.Config.Enabled))
        {
            Service.Config.SaveConfig();
        }

        if (ImGui.Checkbox("Change All Player's Name", ref Service.Config.AllPlayerReplace))
        {
            Service.Config.SaveConfig();
        }

        if (ImGui.InputText("Character Name", ref Service.Config.FakeNameText, 100))
        {
            Service.Config.SaveConfig();
        }

        if (ImGui.Button("Reset"))
        {
            Service.Config.FakeNameText = localName;
            Service.Config.SaveConfig();
        }
    }
}
