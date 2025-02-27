/*
using System;
using System.Linq;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Interface;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Components;
using Dalamud.Interface.Utility;
using Dalamud.Utility;
using ECommons.DalamudServices;
using FakeName.Data;
using ImGuiNET;
using Lumina.Excel;
using World = Lumina.Excel.GeneratedSheets.World;

namespace FakeName.Windows;

internal class TabCharacterOld
{
    static ExcelSheet<World>? Worlds => Svc.Data.GetExcelSheet<World>();
    
    static Vector2 IconButtonSize = new(16);
    static float SupportButtonOffset;
    
    static CharacterConfig? SelectedCharaCfg;
    static string SelectedName = string.Empty;
    static uint SelectedWorld;

    static string CustomName = string.Empty;
    static uint CustomWorld;

    public static void Draw()
    {
        var modified = false;
        ImGui.BeginGroup();
        {
            if (ImGui.BeginChild("character_select", ImGuiHelpers.ScaledVector2(240, 0) - IconButtonSize with { X = 0 }, true)) {
                DrawCharacterList();
            }
            ImGui.EndChild();
            
            var charListSize = ImGui.GetItemRectSize().X;
            
            if (Svc.ClientState.LocalPlayer != null) {
                if (ImGuiComponents.IconButton(FontAwesomeIcon.User)) {
                    if (Svc.ClientState.LocalPlayer != null) {
                        P.Config.TryAddCharacter(Svc.ClientState.LocalPlayer.Name.TextValue, Svc.ClientState.LocalPlayer.HomeWorld.RowId);
                    }
                }
                
                if (ImGui.IsItemHovered()) ImGui.SetTooltip("添加本地角色");
                
                ImGui.SameLine();
                if (ImGuiComponents.IconButton(FontAwesomeIcon.DotCircle)) {
                    if (Svc.Targets.Target is PlayerCharacter pc) {
                        P.Config.TryAddCharacter(pc.Name.TextValue, pc.HomeWorld.RowId);
                    }
                }
                if (ImGui.IsItemHovered()) ImGui.SetTooltip("添加目标角色");
                ImGui.SameLine();
                
                ImGui.SameLine();
                if (ImGuiComponents.IconButton(FontAwesomeIcon.Plus)) {
                    ImGui.OpenPopup("AddCustomChara");
                }
                if (ImGui.IsItemHovered()) ImGui.SetTooltip("添加指定角色");
                ImGui.SameLine();

                if (Worlds != null)
                {
                    if (ImGui.BeginPopup("AddCustomChara", ImGuiWindowFlags.AlwaysAutoResize))
                    {
                        ImGui.TextColored(ImGuiColors.DalamudYellow, "添加指定角色");
                        ImGui.Separator();

                        var worldRow = Worlds.FirstOrDefault(w => w.IsPublic && !w.Name.RawData.IsEmpty && w.Region == 2 && w.RowId == CustomWorld);
                        if (ImGui.BeginCombo("####指定角色服务器", worldRow != null ? worldRow.Name.RawString : "请选择服务器", ImGuiComboFlags.HeightLarge))
                        {
                            foreach (var world in Worlds.Where(w => w.IsPublic && !w.Name.RawData.IsEmpty && w.Region == 2))
                            {
                                if (ImGui.Selectable($"{world.Name.RawString} ({world.RowId})",
                                                     world.RowId == CustomWorld))
                                {
                                    CustomWorld = world.RowId;
                                }
                            }
                            
                            ImGui.EndCombo();
                        }
                        
                        ImGui.SameLine();
                        if (ImGuiComponents.IconButton("AddCustomCurrency", FontAwesomeIcon.Plus))
                        {
                            if (CustomWorld != 0)
                            {
                                P.Config.TryAddCharacter(CustomName, CustomWorld);
                            }
                        }

                        if (ImGui.InputTextWithHint("####指定角色名", "角色名", ref CustomName, 100))
                        {
                        
                        }
                        
                        ImGui.EndPopup();
                    }
                }
            }
            
            if (ImGuiComponents.IconButton(FontAwesomeIcon.Cog)) {
                SelectedCharaCfg = null;
                SelectedName = string.Empty;
                SelectedWorld = 0;
            }
            if (ImGui.IsItemHovered()) ImGui.SetTooltip("选项");
            IconButtonSize = ImGui.GetItemRectSize() + ImGui.GetStyle().ItemSpacing;
            
            if (!P.Config.HideSupport) {
                ImGui.SameLine();
                if (SupportButtonOffset > 0) ImGui.SetCursorPosX(MathF.Max(ImGui.GetCursorPosX(), charListSize - SupportButtonOffset + ImGui.GetStyle().WindowPadding.X));
                if (ImGuiComponents.IconButtonWithText(FontAwesomeIcon.Coffee, "发电", ImGuiColors.ParsedPurple)) {
                    Util.OpenLink("https://afdian.net/a/msew11");
                }
                SupportButtonOffset = ImGui.GetItemRectSize().X;
            }
        }
        ImGui.EndGroup();
        
        ImGui.SameLine();
        if (ImGui.BeginChild("character_view", ImGuiHelpers.ScaledVector2(0), true))
        {
            if (SelectedCharaCfg != null)
            {
                var activePlayer = Svc.Objects.FirstOrDefault(t => t is PlayerCharacter playerCharacter && playerCharacter.Name.TextValue == SelectedName && playerCharacter.HomeWorld.RowId == SelectedWorld);

                DrawCharacterView(SelectedCharaCfg, activePlayer, ref modified);
            }
            else
            {
                ImGui.Text("FakeName 选项");
                ImGui.Separator();

                if (ImGui.Checkbox("启用", ref P.Config.Enabled))
                {
                    Svc.PluginInterface.SavePluginConfig(P.Config);
                }

                if (ImGui.Checkbox("匿名模式", ref P.Config.IncognitoMode))
                {
                    Svc.PluginInterface.SavePluginConfig(P.Config);
                }

                if (ImGui.Checkbox("隐藏发电按钮", ref P.Config.HideSupport))
                {
                    Svc.PluginInterface.SavePluginConfig(P.Config);
                }
                
                // ImGuiHelpers.ScaledDummy(10);
                // ImGui.Text("Local Player");
                // ImGui.Separator();
                // if (Service.ClientState.LocalPlayer != null)
                // {
                //     ImGui.Text($"{Service.ClientState.LocalPlayer.ObjectId}");
                // }
                // ImGui.Separator();
                // ImGui.Text("Objects");
                // foreach (var gameObject in Service.Objects)
                // {
                //     if (gameObject.ObjectKind == ObjectKind.BattleNpc)
                //     {
                //         ImGui.Text($"{gameObject.Name} | {gameObject.ObjectId} | {gameObject.OwnerId}");
                //     }
                // }
                // ImGui.Separator();
                // ImGui.Text($"Buddy {Service.BuddyList.Length}");
                // foreach (var buddyMember in Service.BuddyList)
                // {
                //     var gameObject = buddyMember.GameObject;
                //     ImGui.Text($"{buddyMember.ObjectId}");
                //     if (gameObject != null)
                //     {
                //         ImGui.Text($"{gameObject.Name} | {gameObject.RawName()} | {gameObject.OwnerId}");
                //     }
                // }

                // ImGui.Checkbox("小队模糊(非跨服)", ref config.PartyMemberReplace);
            }
        }
        ImGui.EndChild();
    }

    public static void DrawCharacterView(CharacterConfig? characterConfig, GameObject? activeCharacter, ref bool modified)
    {
        if (characterConfig == null) return;
        var world = Worlds?.GetRow(SelectedWorld);
        ImGui.Text($"{(world == null ? string.Empty:world.Name.RawString)} {IncognitoModeName(SelectedName)}");
        ImGui.Separator();
        
        // IconId
        var iconReplace = characterConfig.IconReplace;
        if (ImGui.Checkbox("##替换图标Id", ref iconReplace))
        {
            characterConfig.IconReplace = iconReplace;
            Svc.PluginInterface.SavePluginConfig(P.Config);
            modified = true;
        }
        if (ImGui.IsItemHovered()) ImGui.SetTooltip("替换图标Id");
        ImGui.SameLine();
        ImGui.SetCursorPosX(50);
        var iconId = characterConfig.IconId;
        if (ImGui.InputInt("图标Id", ref iconId))
        {
            characterConfig.IconId = iconId;
            Svc.PluginInterface.SavePluginConfig(P.Config);
            modified = true;
        }
        
        // Name
        ImGui.SetCursorPosX(50);
        var fakeName = characterConfig.FakeNameText;
        if (ImGui.InputText("角色名", ref fakeName, 100))
        {
            characterConfig.FakeNameText = fakeName;
            Svc.PluginInterface.SavePluginConfig(P.Config);
            modified = true;
        }
        
        // FcName
        var hideFcName = characterConfig.HideFcName;
        if (ImGui.Checkbox("##隐藏部队简称", ref hideFcName))
        {
            characterConfig.HideFcName = hideFcName;
            Svc.PluginInterface.SavePluginConfig(P.Config);
            modified = true;
        }
        if (ImGui.IsItemHovered()) ImGui.SetTooltip("隐藏部队简称");
        ImGui.SameLine();
        ImGui.SetCursorPosX(50);
        var fakeFcName = characterConfig.FakeFcNameText;
        if (ImGui.InputText("部队简称", ref fakeFcName, 100))
        {
            characterConfig.FakeFcNameText = fakeFcName;
            Svc.PluginInterface.SavePluginConfig(P.Config);
            modified = true;
        }
    }

    public static void DrawCharacterList()
    {
        foreach (var (worldId, characters) in P.Config.WorldCharacterDictionary.ToArray()) {
            var world = Svc.Data.GetExcelSheet<World>()?.GetRow(worldId);
            if (world == null) continue;
            
            ImGui.TextDisabled($"{world.Name.RawString}");
            ImGui.Separator();

            foreach (var (name, characterConfig) in characters.ToArray()) {
                if (ImGui.Selectable($"{IncognitoModeName(name).PadRight(7, '\u3000')}", SelectedCharaCfg == characterConfig)) {
                    SelectedCharaCfg = characterConfig;
                    SelectedName = name;
                    SelectedWorld = world.RowId;
                }
                ImGui.SameLine();
                ImGui.SetCursorPosX(100);
                if (ImGui.Selectable($"[{characterConfig.FakeNameText}]##{world.Name.RawString}", false)) {
                }
                
                if (ImGui.BeginPopupContextItem()) {
                    if (ImGui.Selectable($"移除 '{IncognitoModeName(name)} @ {world.Name.RawString}'")) {
                        characters.Remove(name);
                        if (SelectedCharaCfg == characterConfig) SelectedCharaCfg = null;
                        if (characters.Count == 0) {
                            P.Config.WorldCharacterDictionary.Remove(worldId);
                        }
                    }
                    ImGui.EndPopup();
                }
            }

            ImGuiHelpers.ScaledDummy(10);
        }
    }

    /*public static string IncognitoModeName(string name)
    {
        if (!P.Config.IncognitoMode)
        {
            return name;
        }
        else
        {
            return name.Substring(0, 1) + "...";
        }
    }#1#
}
*/
