using ImGuiNET;

namespace FakeName.Gui;

public static class TabSettings
{
    public static void Draw()
    {
        ImGui.Checkbox("启用", ref C.Enabled);

        ImGui.Checkbox("隐藏发电按钮", ref C.HideSupport);
    }
}
