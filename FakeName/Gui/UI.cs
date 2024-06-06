using Dalamud.Interface;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Components;
using Dalamud.Utility;
using ECommons.Configuration;
using ECommons.DalamudServices;
using ECommons.ImGuiMethods;
using ECommons.Throttlers;
using ImGuiNET;

namespace FakeName.Gui;

public class UI
{
    public static void Draw()
    {
        if (EzThrottler.Throttle("PeriodicConfigSave", 30 * 1000))
        {
            Svc.PluginInterface.SavePluginConfig(P.Config);
            EzConfig.Save();
        }

        DrawSupportButton();
        ImGui.Checkbox("启用", ref C.Enabled);
#if DEBUG
        ImGuiEx.EzTabBar("##main",[
            ("角色设置", TabCharacter.Draw, null, true),
            ("Debug", TabDebug.Draw, null, true),
        ]);
#endif
        TabCharacter.Draw();
    }

    private static float ButtonOffset;
    
    public static void DrawSupportButton()
    {
        var cur = ImGui.GetCursorPos();

        ImGui.SetCursorPosX(cur.X + ImGui.GetContentRegionAvail().X - ButtonOffset);
        ImGui.BeginGroup();
        if (ImGuiComponents.IconButton(FontAwesomeIcon.Coffee, ImGuiColors.ParsedPurple)) {
            Util.OpenLink("https://afdian.net/a/msew11");
        }
        ImGui.SameLine();
        if (ImGuiComponents.IconButton(FontAwesomeIcon.Globe, ImGuiColors.ParsedGrey)) {
            Util.OpenLink("https://github.com/msew11/FakeName");
        }
        ImGui.EndGroup();
        ButtonOffset = ImGui.GetItemRectSize().X;
        ImGui.SetCursorPos(cur);
    }
}
