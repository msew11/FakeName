using System;
using System.Linq;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Interface;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Components;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Windowing;
using Dalamud.Utility;
using FakeName.Config;
using ImGuiNET;
using Lumina.Excel;
using World = Lumina.Excel.GeneratedSheets.World;

namespace FakeName.Windows;

internal class ConfigWindow : Window
{
    private readonly PluginConfig config;
    private readonly Plugin plugin;
    
    private Vector2 iconButtonSize = new(16);
    private ExcelSheet<World>? worlds;
    
    private CharacterConfig? selectedCharaCfg;
    private string selectedName = string.Empty;
    private uint selectedWorld;

    private string customName = string.Empty;
    private uint customWorld;

    public ConfigWindow(Plugin plugin, PluginConfig config) : base("FakeName")
    {
        this.config = config;
        this.plugin = plugin;
        this.worlds = Service.Data.GetExcelSheet<World>();
    }

    public void Open()
    {
        IsOpen = true;
    }
    
    public override void OnClose() {
        Service.Interface.SavePluginConfig(config);
        base.OnClose();
    }
    
    private float supportButtonOffset;

    public override void Draw()
    {
        var modified = false;
        ImGui.BeginGroup();
        {
            if (ImGui.BeginChild("character_select", ImGuiHelpers.ScaledVector2(240, 0) - iconButtonSize with { X = 0 }, true)) {
                DrawCharacterList();
            }
            ImGui.EndChild();
            
            var charListSize = ImGui.GetItemRectSize().X;
            
            if (Service.ClientState.LocalPlayer != null) {
                if (ImGuiComponents.IconButton(FontAwesomeIcon.User)) {
                    if (Service.ClientState.LocalPlayer != null) {
                        config.TryAddCharacter(Service.ClientState.LocalPlayer.Name.TextValue, Service.ClientState.LocalPlayer.HomeWorld.Id);
                    }
                }
                
                if (ImGui.IsItemHovered()) ImGui.SetTooltip("添加本地角色");
                
                ImGui.SameLine();
                if (ImGuiComponents.IconButton(FontAwesomeIcon.DotCircle)) {
                    if (Service.Targets.Target is PlayerCharacter pc) {
                        config.TryAddCharacter(pc.Name.TextValue, pc.HomeWorld.Id);
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

                if (worlds != null)
                {
                    if (ImGui.BeginPopup("AddCustomChara", ImGuiWindowFlags.AlwaysAutoResize))
                    {
                        ImGui.TextColored(ImGuiColors.DalamudYellow, "添加指定角色");
                        ImGui.Separator();

                        var worldRow = worlds.FirstOrDefault(w => w.IsPublic && !w.Name.RawData.IsEmpty && w.Region == 2 && w.RowId == customWorld);
                        if (ImGui.BeginCombo("####指定角色服务器", worldRow != null ? worldRow.Name.RawString : "请选择服务器", ImGuiComboFlags.HeightLarge))
                        {
                            foreach (var world in worlds.Where(w => w.IsPublic && !w.Name.RawData.IsEmpty && w.Region == 2))
                            {
                                if (ImGui.Selectable($"{world.Name.RawString} ({world.RowId})",
                                                     world.RowId == customWorld))
                                {
                                    customWorld = world.RowId;
                                }
                            }
                            
                            ImGui.EndCombo();
                        }
                        
                        ImGui.SameLine();
                        if (ImGuiComponents.IconButton("AddCustomCurrency", FontAwesomeIcon.Plus))
                        {
                            if (customWorld != 0)
                            {
                                config.TryAddCharacter(customName, customWorld);
                            }
                        }

                        if (ImGui.InputTextWithHint("####指定角色名", "角色名", ref customName, 100))
                        {
                        
                        }
                        
                        ImGui.EndPopup();
                    }
                }
            }
            
            if (ImGuiComponents.IconButton(FontAwesomeIcon.Cog)) {
                selectedCharaCfg = null;
                selectedName = string.Empty;
                selectedWorld = 0;
            }
            if (ImGui.IsItemHovered()) ImGui.SetTooltip("选项");
            iconButtonSize = ImGui.GetItemRectSize() + ImGui.GetStyle().ItemSpacing;
            
            if (!config.HideSupport) {
                ImGui.SameLine();
                if (supportButtonOffset > 0) ImGui.SetCursorPosX(MathF.Max(ImGui.GetCursorPosX(), charListSize - supportButtonOffset + ImGui.GetStyle().WindowPadding.X));
                if (ImGuiComponents.IconButtonWithText(FontAwesomeIcon.Coffee, "发电", ImGuiColors.ParsedPurple)) {
                    Util.OpenLink("https://afdian.net/a/msew11");
                }
                supportButtonOffset = ImGui.GetItemRectSize().X;
            }
        }
        ImGui.EndGroup();
        
        ImGui.SameLine();
        if (ImGui.BeginChild("character_view", ImGuiHelpers.ScaledVector2(0), true))
        {
            if (selectedCharaCfg != null)
            {
                var activePlayer = Service.Objects.FirstOrDefault(t => t is PlayerCharacter playerCharacter && playerCharacter.Name.TextValue == selectedName && playerCharacter.HomeWorld.Id == selectedWorld);

                DrawCharacterView(selectedCharaCfg, activePlayer, ref modified);
            }
            else
            {
                ImGui.Text("FakeName 选项");
                ImGui.Separator();

                if (ImGui.Checkbox("启用", ref config.Enabled))
                {
                    Service.Interface.SavePluginConfig(config);
                }

                if (ImGui.Checkbox("匿名模式", ref config.IncognitoMode))
                {
                    Service.Interface.SavePluginConfig(config);
                }

                if (ImGui.Checkbox("隐藏发电按钮", ref config.HideSupport))
                {
                    Service.Interface.SavePluginConfig(config);
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

    public void DrawCharacterView(CharacterConfig? characterConfig, GameObject? activeCharacter, ref bool modified)
    {
        if (characterConfig == null) return;
        var world = worlds?.GetRow(selectedWorld);
        ImGui.Text($"{(world == null ? string.Empty:world.Name.RawString)} {IncognitoModeName(selectedName)}");
        ImGui.Separator();
        
        // IconId
        var iconReplace = characterConfig.IconReplace;
        if (ImGui.Checkbox("##替换图标Id", ref iconReplace))
        {
            characterConfig.IconReplace = iconReplace;
            Service.Interface.SavePluginConfig(config);
            modified = true;
        }
        if (ImGui.IsItemHovered()) ImGui.SetTooltip("替换图标Id");
        ImGui.SameLine();
        ImGui.SetCursorPosX(50);
        var iconId = characterConfig.IconId;
        if (ImGui.InputInt("图标Id", ref iconId))
        {
            characterConfig.IconId = iconId;
            Service.Interface.SavePluginConfig(config);
            modified = true;
        }
        
        // Name
        ImGui.SetCursorPosX(50);
        var fakeName = characterConfig.FakeNameText;
        if (ImGui.InputText("角色名", ref fakeName, 100))
        {
            characterConfig.FakeNameText = fakeName;
            Service.Interface.SavePluginConfig(config);
            modified = true;
        }
        
        // FcName
        var hideFcName = characterConfig.HideFcName;
        if (ImGui.Checkbox("##隐藏部队简称", ref hideFcName))
        {
            characterConfig.HideFcName = hideFcName;
            Service.Interface.SavePluginConfig(config);
            modified = true;
        }
        if (ImGui.IsItemHovered()) ImGui.SetTooltip("隐藏部队简称");
        ImGui.SameLine();
        ImGui.SetCursorPosX(50);
        var fakeFcName = characterConfig.FakeFcNameText;
        if (ImGui.InputText("部队简称", ref fakeFcName, 100))
        {
            characterConfig.FakeFcNameText = fakeFcName;
            Service.Interface.SavePluginConfig(config);
            modified = true;
        }
    }

    public void DrawCharacterList()
    {
        foreach (var (worldId, characters) in config.WorldCharacterDictionary.ToArray()) {
            var world = Service.Data.GetExcelSheet<World>()?.GetRow(worldId);
            if (world == null) continue;
            
            ImGui.TextDisabled($"{world.Name.RawString}");
            ImGui.Separator();

            foreach (var (name, characterConfig) in characters.ToArray()) {
                if (ImGui.Selectable($"{IncognitoModeName(name).PadRight(7, '\u3000')}", selectedCharaCfg == characterConfig)) {
                    selectedCharaCfg = characterConfig;
                    selectedName = name;
                    selectedWorld = world.RowId;
                }
                ImGui.SameLine();
                ImGui.SetCursorPosX(100);
                if (ImGui.Selectable($"[{characterConfig.FakeNameText}]##{world.Name.RawString}", false)) {
                }
                
                if (ImGui.BeginPopupContextItem()) {
                    if (ImGui.Selectable($"移除 '{IncognitoModeName(name)} @ {world.Name.RawString}'")) {
                        characters.Remove(name);
                        if (selectedCharaCfg == characterConfig) selectedCharaCfg = null;
                        if (characters.Count == 0) {
                            config.WorldCharacterDictionary.Remove(worldId);
                        }
                    }
                    ImGui.EndPopup();
                }
            }

            ImGuiHelpers.ScaledDummy(10);
        }
    }

    public string IncognitoModeName(string name)
    {
        if (!config.IncognitoMode)
        {
            return name;
        }
        else
        {
            return name.Substring(0, 1) + "...";
        }
    }
}
