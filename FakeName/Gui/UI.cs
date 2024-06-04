using System;
using Dalamud.Interface;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Components;
using Dalamud.Interface.Utility;
using Dalamud.Utility;
using ECommons;
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
        TabCharacter.Draw();
        /*ImGuiEx.EzTabBar("##main",[
            ("角色设置", TabCharacter.Draw, null, true),
            ("插件设置", TabSettings.Draw, null, true),
        ]);*/
    }

    private static float SupportButtonOffset;
    
    public static void DrawSupportButton()
    {
        var cur = ImGui.GetCursorPos();

        if (SupportButtonOffset > 0) 
        {
            ImGui.SetCursorPosX(cur.X + ImGui.GetContentRegionAvail().X - SupportButtonOffset);
        }

        if (ImGuiComponents.IconButton(FontAwesomeIcon.Coffee, ImGuiColors.ParsedPurple)) {
            Util.OpenLink("https://afdian.net/a/msew11");
        }
        SupportButtonOffset = ImGui.GetItemRectSize().X;
        ImGui.SetCursorPos(cur);
    }
}
