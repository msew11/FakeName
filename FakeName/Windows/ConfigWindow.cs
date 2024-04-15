using System.Linq;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Interface;
using Dalamud.Interface.Components;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Windowing;
using FakeName.Config;
using ImGuiNET;
using World = Lumina.Excel.GeneratedSheets.World;

namespace FakeName.Windows;

internal class ConfigWindow : Window
{
    private readonly PluginConfig config;
    private readonly Plugin plugin;
    
    private Vector2 iconButtonSize = new(16);
    
    private CharacterConfig? selectedCharaCfg;
    private string selectedName = string.Empty;
    private uint selectedWorld;

    public ConfigWindow(Plugin plugin, PluginConfig config) : base("FakeName")
    {
        this.config = config;
        this.plugin = plugin;
    }

    public void Open()
    {
        IsOpen = true;
    }
    
    public override void OnClose() {
        Service.Interface.SavePluginConfig(config);
        base.OnClose();
    }

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
                
                if (ImGui.IsItemHovered()) ImGui.SetTooltip("Add current character");
                
                ImGui.SameLine();
                if (ImGuiComponents.IconButton(FontAwesomeIcon.DotCircle)) {
                    if (Service.Targets.Target is PlayerCharacter pc) {
                        config.TryAddCharacter(pc.Name.TextValue, pc.HomeWorld.Id);
                    }
                }
                if (ImGui.IsItemHovered()) ImGui.SetTooltip("Add targeted character");
                ImGui.SameLine();
            }
            
            if (ImGuiComponents.IconButton(FontAwesomeIcon.Cog)) {
                selectedCharaCfg = null;
                selectedName = string.Empty;
                selectedWorld = 0;
            }
            if (ImGui.IsItemHovered()) ImGui.SetTooltip("Plugin Options");
            iconButtonSize = ImGui.GetItemRectSize() + ImGui.GetStyle().ItemSpacing;
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
                ImGui.Text("FakeName Options");
                ImGui.Separator();

                if (ImGui.Checkbox("Enable", ref config.Enabled))
                {
                    Service.Interface.SavePluginConfig(config);
                }

                
                // ImGui.Checkbox("小队模糊(非跨服)", ref config.PartyMemberReplace);
            }
        }
        ImGui.EndChild();
    }

    public void DrawCharacterView(CharacterConfig? characterConfig, GameObject? activeCharacter, ref bool modified)
    {
        if (characterConfig == null) return;
        
        var fakeName = characterConfig.FakeNameText;
        if (ImGui.InputText("角色名", ref fakeName, 100))
        {
            characterConfig.FakeNameText = fakeName;
            Service.Interface.SavePluginConfig(config);
            modified = true;
        }

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
                if (ImGui.Selectable($"{name}##{world.Name.RawString}", selectedCharaCfg == characterConfig)) {
                    selectedCharaCfg = characterConfig;
                    selectedName = name;
                    selectedWorld = world.RowId;
                }
                
                if (ImGui.BeginPopupContextItem()) {
                    if (ImGui.Selectable($"Remove '{name} @ {world.Name.RawString}' from Config")) {
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
}
