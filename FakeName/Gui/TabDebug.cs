using System;
using System.Linq;
using System.Reflection;
using Dalamud.Support;
using ECommons.ImGuiMethods;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using ImGuiNET;
using Newtonsoft.Json;
using OtterGui;

namespace FakeName.Gui;

public static class TabDebug
{
    public static void Draw()
    {
        ImGuiEx.EzTabBar("##debug",[
            ("IDM", DrawIdm, null, true),
            ("Config", DrawConfig, null, true),
            ("Cache", DrawCache, null, true),
            ("AntiMeasurement", DrawAnti, null, true),
        ]);
    }

    public static void DrawAnti()
    {
        
        ImGui.Text($"{P.msg}");
        ImGui.Separator();
        var type = typeof(Troubleshooting).Assembly.GetTypes().FirstOrDefault(t => t.Name.Equals("EventTracking"));
        ImGui.Text($"type == null: {type == null}");
        ImGui.Separator();

        if (type != null)
        {
            var method = type.GetRuntimeMethods().FirstOrDefault(m => m.Name == "SendMeasurement");
            ImGui.Text($"method == null: {method == null}");
            ImGui.Separator();
        }
    }

    public static void DrawIdm()
    {
        var count = 0;
        foreach (var (world, worldDic) in Idm.WorldCharacterDictionary)
        {
            ImGui.Text(world.ToString());
            foreach (var (name, cfg) in worldDic)
            {
                ImGui.Text($"    {name} {JsonConvert.SerializeObject(cfg)}");
                count++;
            }
        }
        ImGui.Separator();
        ImGui.Text($"count :{count}");
    }

    public static void DrawConfig()
    {
        var count = 0;
        foreach (var (world, worldDic) in C.WorldCharacterDictionary)
        {
            ImGui.Text(world.ToString());
            foreach (var (name, cfg) in worldDic)
            {
                ImGui.Text($"    {name} {JsonConvert.SerializeObject(cfg)}");
                count++;
            }
        }
        ImGui.Separator();
        ImGui.Text($"count :{count}");
    }

    public static void DrawCache()
    {
        foreach (var cfg in C.Characters)
        {
            ImGui.Text($"{cfg.WorldName()} {cfg.Name}");
        }
        ImGui.Separator();
        ImGui.Text($"count :{C.Characters.Count}");
    }
}
