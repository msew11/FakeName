using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Interface;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Components;
using ECommons;
using ECommons.Configuration;
using ECommons.DalamudServices;
using ECommons.ImGuiMethods;
using ECommons.Logging;
using FakeName.Data;
using ImGuiNET;
using Lumina.Excel;
using Lumina.Excel.GeneratedSheets;
using OtterGui;
using OtterGui.Classes;
using OtterGui.Filesystem;
using OtterGui.FileSystem.Selector;
using OtterGui.Raii;

namespace FakeName.OtterGuiHandlers;

public sealed class FakeNameFileSystem : FileSystem<CharacterConfig> , IDisposable
{
    string FilePath = Path.Combine(Svc.PluginInterface.ConfigDirectory.FullName, "FakeNameFileSystem.json");
    public readonly FakeNameFileSystem.FileSystemSelector Selector;
    public FakeNameFileSystem(OtterGuiHandler h)
    {
        EzConfig.OnSave += Save;
        try
        {
            var info = new FileInfo(FilePath);
            if (info.Exists)
            {
                this.Load(info, C.Characters, ConvertToIdentifier, ConvertToName);
            }
            Selector = new(this, h);
        }
        catch (Exception e)
        {
            e.Log();
        }
    }

    public void Dispose()
    {
        EzConfig.OnSave -= Save;
    }

    public void DoDelete(CharacterConfig characterConfig)
    {
        PluginLog.Debug($"Deleting {characterConfig.Id}");
        C.Characters.Remove(characterConfig);
        if(FindLeaf(characterConfig, out var leaf))
        {
            this.Delete(leaf);
        }
        this.Save();
    }

    public bool FindLeaf(CharacterConfig characterConfig, [MaybeNullWhen(false)] out Leaf leaf)
    {
        leaf = Root.GetAllDescendants(ISortMode<CharacterConfig>.Lexicographical)
            .OfType<Leaf>()
            .FirstOrDefault(l => l.Value == characterConfig);
        return leaf != null;
    }

    public bool TryGetPathById(Guid id, [MaybeNullWhen(false)] out string path)
    {
        if (FindLeaf(C.Characters.FirstOrDefault(x => x.Guid == id), out var leaf))
        {
            path = leaf.FullName();
            return true;
        }
        path = default;
        return false;
    }

    public string ConvertToName(CharacterConfig characterConfig)
    {
        PluginLog.Debug($"Request conversion of {characterConfig.Id} {characterConfig.World} {characterConfig.Name} to name");
        return $"{characterConfig.Name}({characterConfig.WorldName()})";
    }

    private string ConvertToIdentifier(CharacterConfig characterConfig)
    {
        PluginLog.Debug($"Request conversion of {characterConfig.Id} {characterConfig.World} {characterConfig.Name} to identifier");
        return characterConfig.Id;
    }

    public void Save()
    {
        try
        {
            using var fileStream = new FileStream(FilePath, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
            using var streamWriter = new StreamWriter(fileStream);
            this.SaveToFile(streamWriter, SaveConverter, true);
        }
        catch(Exception ex)
        {
            ex.Log("Error saving FakeNameFileSystem:");
        }
    }

    private (string, bool) SaveConverter(CharacterConfig characterConfig, string arg2)
    {
        PluginLog.Debug($"Saving {characterConfig.Id} {characterConfig.World} {characterConfig.Name}");
        return (characterConfig.Id, true);
    }

    public class FileSystemSelector : FileSystemSelector<CharacterConfig, FileSystemSelector.State>
    {
        string newName = "";
        string clipboardText = null;
        CharacterConfig cloneConfig = null;
        string customName = string.Empty;
        uint customWorld;
        public override ISortMode<CharacterConfig> SortMode => ISortMode<CharacterConfig>.FoldersFirst;
        static FakeNameFileSystem Fs => P.OtterGuiHandler.FakeNameFileSystem;
        static ExcelSheet<World>? Worlds => Svc.Data.GetExcelSheet<World>();
        public FileSystemSelector(FakeNameFileSystem fs, OtterGuiHandler h) : base(fs, Svc.KeyState, h.Logger, (e) => e.Log())
        {
            AddButton(NewLocalCharaButton, 0);
            AddButton(NewTargetCharaButton, 1);
            AddButton(NewCustomCharaButton, 2);
            // AddButton(ImportButton, 10);
            // AddButton(CopyToClipboardButton, 20);
            // AddButton(DeleteButton, 1000);

            UnsubscribeRightClickLeaf(RenameLeaf);
            SubscribeRightClickLeaf(DeleteCharaConfig);
        }

        protected override uint CollapsedFolderColor => ImGuiColors.DalamudViolet.ToUint();
        protected override uint ExpandedFolderColor => CollapsedFolderColor;

        protected override void DrawLeafName(Leaf leaf, in State state, bool selected)
        {
            var flag = selected ? ImGuiTreeNodeFlags.Selected | LeafFlags : LeafFlags;
            using var _ = ImRaii.TreeNode( $"{leaf.Value.IncognitoName()}                                         ", flag);
        }

        private void NewLocalCharaButton(Vector2 size)
        {
            if (ImGuiUtil.DrawDisabledButton(FontAwesomeIcon.User.ToIconString(), size, "添加本地角色", false, true))
            {
                if (Svc.ClientState.LocalPlayer != null) {
                    var name = Svc.ClientState.LocalPlayer.Name.TextValue;
                    var world = Svc.ClientState.LocalPlayer.HomeWorld.Id;
                    if (C.TryAddCharacter(name, world))
                    {
                        if (C.TryGetCharacterConfig(name, world, out var characterConfig))
                        {
                            C.Characters.Add(characterConfig);
                            Fs.CreateLeaf(Fs.Root, Fs.ConvertToName(characterConfig), characterConfig);
                        }
                    }
                    
                }
            }
        }

        private void NewTargetCharaButton(Vector2 size)
        {
            if (ImGuiUtil.DrawDisabledButton(FontAwesomeIcon.DotCircle.ToIconString(), size, "添加目标角色", false, true))
            {
                if (Svc.Targets.Target is PlayerCharacter pc) {
                    var name = pc.Name.TextValue;
                    var world = pc.HomeWorld.Id;
                    if (C.TryAddCharacter(name, world))
                    {
                        if (C.TryGetCharacterConfig(name, world, out var characterConfig))
                        {
                            C.Characters.Add(characterConfig);
                            Fs.CreateLeaf(Fs.Root, Fs.ConvertToName(characterConfig), characterConfig);
                        }
                    }
                }
            }
        }

        private void NewCustomCharaButton(Vector2 size)
        {
            if (ImGuiUtil.DrawDisabledButton(FontAwesomeIcon.Plus.ToIconString(), size, "添加指定角色", false, true))
            {
                ImGui.OpenPopup("AddCustomCharaContext");
            }
            if (Worlds != null)
            {
                if (ImGui.BeginPopup("AddCustomCharaContext", ImGuiWindowFlags.AlwaysAutoResize))
                {
                    ImGui.TextColored(ImGuiColors.DalamudYellow, "添加指定角色");
                    ImGui.Separator();

                    var worldRow = Worlds.FirstOrDefault(w => w.IsPublic && !w.Name.RawData.IsEmpty && w.Region == 2 && w.RowId == customWorld);
                    if (ImGui.BeginCombo("##指定角色服务器", worldRow != null ? worldRow.Name.RawString : "请选择服务器", ImGuiComboFlags.HeightLarge))
                    {
                        foreach (var world in Worlds.Where(w => w.IsPublic && !w.Name.RawData.IsEmpty && w.Region == 2))
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
                    if (ImGuiComponents.IconButton("AddCustomChara", FontAwesomeIcon.Plus))
                    {
                        if (customWorld != 0)
                        {
                            if (C.TryAddCharacter(customName, customWorld))
                            {
                                if (C.TryGetCharacterConfig(customName, customWorld, out var characterConfig))
                                {
                                    C.Characters.Add(characterConfig);
                                    Fs.CreateLeaf(Fs.Root, Fs.ConvertToName(characterConfig), characterConfig);
                                }
                            }
                        }
                    }

                    ImGui.InputTextWithHint("##指定角色名", "角色名", ref customName, 100);
                        
                    ImGui.EndPopup();
                }
            }
        }

        private void DeleteCharaConfig(Leaf leaf)
        {
            if (ImGui.Selectable("移除")) {
                var world = leaf.Value.World;
                var name = leaf.Value.Name;
                if (C.TryGetCharacterConfig(name, world, out var characterConfig))
                {
                    if (C.TryGetWorldDic(world, out var worldDic))
                    {
                        if (worldDic.Remove(name))
                        {
                            Fs.DoDelete(characterConfig);
                            C.Characters.Remove(characterConfig);
                        }
                    }
                }
            }
        }

        private void DrawNewMoodlePopup()
        {
            if (!ImGuiUtil.OpenNameField("##NewMoodle", ref newName))
                return;

            if (newName == "")
            {
                Notify.Error($"Name can not be empty!");
                return;
            }

            if (clipboardText != null)
            {
                try
                {
                    var newCharacterConfig = EzConfig.DefaultSerializationFactory.Deserialize<CharacterConfig>(clipboardText);
                    if (newCharacterConfig != null)
                    {
                        Fs.CreateLeaf(Fs.Root, newName, newCharacterConfig);
                        C.Characters.Add(newCharacterConfig);
                    }
                    else
                    {
                        Notify.Error($"Invalid clipboard data");
                    }
                }
                catch (Exception e)
                {
                    e.LogVerbose();
                    Notify.Error($"Error: {e.Message}");
                }
            }
            else if (cloneConfig != null)
            {

            }
            else
            {
                try
                {
                    var newStatus = new CharacterConfig();
                    Fs.CreateLeaf(Fs.Root, newName, newStatus);
                    C.Characters.Add(newStatus);
                }
                catch (Exception e)
                {
                    e.LogVerbose();
                    Notify.Error($"This name already exists!");
                }
            }

            newName = string.Empty;
        }

        protected override void DrawPopups()
        {
            DrawNewMoodlePopup();
        }

        public record struct State { }
        protected override bool ApplyFilters(IPath path)
        {
            return FilterValue.Length > 0 && !path.FullName().Contains(this.FilterValue, StringComparison.OrdinalIgnoreCase);
        }

    }
}
