using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using Dalamud.Game;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Logging;
using XivCommon.Functions.NamePlates;

namespace Matrix;

internal class Obscurer : IDisposable {
    private Plugin Plugin { get; }

    private Stopwatch UpdateTimer { get; } = new();
    private IReadOnlySet<string> Friends { get; set; }

    internal unsafe Obscurer(Plugin plugin) {
        Plugin = plugin;

        UpdateTimer.Start();

        Friends = Plugin.Common.Functions.FriendList.List
            .Select(friend => friend.Name.TextValue)
            .ToHashSet();

        Service.Framework.Update += OnFrameworkUpdate;
        Plugin.Functions.AtkTextNodeSetText += OnAtkTextNodeSetText;
        // Plugin.Functions.CharacterInitialise += OnCharacterInitialise;
        // Plugin.Functions.FlagSlotUpdate += OnFlagSlotUpdate;
        Plugin.Common.Functions.NamePlates.OnUpdate += OnNamePlateUpdate;
        Service.ChatGui.ChatMessage += OnChatMessage;
    }

    public unsafe void Dispose() {
        Service.ChatGui.ChatMessage -= OnChatMessage;
        Plugin.Common.Functions.NamePlates.OnUpdate -= OnNamePlateUpdate;
        Plugin.Functions.AtkTextNodeSetText -= OnAtkTextNodeSetText;
        // Plugin.Functions.CharacterInitialise -= OnCharacterInitialise;
        // Plugin.Functions.FlagSlotUpdate -= OnFlagSlotUpdate;
        Service.Framework.Update -= OnFrameworkUpdate;
    }

    private static readonly ConditionFlag[] DutyFlags = {
        ConditionFlag.BoundByDuty,
        ConditionFlag.BoundByDuty56,
        ConditionFlag.BoundByDuty95,
        ConditionFlag.BoundToDuty97,
    };

    private bool IsInDuty() {
        return DutyFlags.Any(flag => Service.Condition[flag]);
    }

    private void OnFrameworkUpdate(Framework framework) {
        if (UpdateTimer.Elapsed < TimeSpan.FromSeconds(5) || IsInDuty()) {
            return;
        }

        Friends = Plugin.Common.Functions.FriendList.List
            .Select(friend => friend.Name.TextValue)
            .ToHashSet();
        UpdateTimer.Restart();
    }

    private static readonly Regex Coords = new(@"^X: \d+. Y: \d+.(?: Z: \d+.)?$", RegexOptions.Compiled);

    private void OnAtkTextNodeSetText(IntPtr node, IntPtr textPtr, ref SeString? overwrite) {
        // A catch-all for UI text. This is slow, so specialised methods should be preferred.

        var text = Util.ReadRawSeString(textPtr);

        if (text.Payloads.All(payload => payload.Type != PayloadType.RawText)) {
            return;
        }

        var tval = text.TextValue;
        if (string.IsNullOrWhiteSpace(tval) || tval.All(c => !char.IsLetter(c)) || Coords.IsMatch(tval)) {
            return;
        }

        var changed = ChangeNames(text);
        if (changed) {
            overwrite = text;
        }
    }

    // private unsafe bool ShouldObscureAppearance(GameObject* gameObj) {
    //     if (gameObj == null) {
    //         return false;
    //     }
    //
    //     if (gameObj->ObjectKind != (byte) FFXIVClientStructs.FFXIV.Client.Game.Object.ObjectKind.Pc) {
    //         return false;
    //     }
    //
    //     var gameObject = Service.ObjectTable.CreateObjectReference((IntPtr) gameObj)!;
    //     return gameObject is Character chara && ShouldObscureAppearance(chara);
    // }

    // private unsafe bool ShouldObscureAppearance(Character chara) {
    //     if (!Plugin.Config.Enabled) {
    //         return false;
    //     }
    //
    //     var name = chara.RawName()!;
    //
    //     if (Plugin.Config.ObscureAppearancesExcludeFriends && Friends.Contains(name)) {
    //         return false;
    //     }
    //
    //     var player = *(GameObject**) Plugin.ObjectTable.Address;
    //     if (player != null && player->ObjectID == chara.ObjectId) {
    //         return Plugin.Config.ObscureAppearancesSelf;
    //     }
    //
    //     var party = Plugin.PartyList.Select(member => member.ObjectId);
    //     if (party.Contains(chara.ObjectId)) {
    //         return Plugin.Config.ObscureAppearancesParty;
    //     }
    //
    //     return Plugin.Config.ObscureAppearancesOthers;
    // }
    
    // private unsafe void OnCharacterInitialise(GameObject* gameObj, IntPtr humanPtr, IntPtr customiseDataPtr) {
    //     if (!ShouldObscureAppearance(gameObj)) {
    //         return;
    //     }
    //
    //     var npc = Plugin.AppearanceRepository.GetNpc(gameObj->ObjectID);
    //
    //     var customise = (byte*) customiseDataPtr;
    //     customise[(int) CustomizeIndex.Race] = (byte) npc.Race.Row;
    //     customise[(int) CustomizeIndex.Gender] = npc.Gender;
    //     customise[(int) CustomizeIndex.ModelType] = npc.BodyType;
    //     customise[(int) CustomizeIndex.Height] = npc.Height;
    //     customise[(int) CustomizeIndex.Tribe] = (byte) npc.Tribe.Row;
    //     customise[(int) CustomizeIndex.FaceType] = npc.Face;
    //     customise[(int) CustomizeIndex.HairStyle] = npc.HairStyle;
    //     customise[(int) CustomizeIndex.HasHighlights] = npc.HairHighlight;
    //     customise[(int) CustomizeIndex.SkinColor] = npc.SkinColor;
    //     customise[(int) CustomizeIndex.EyeColor] = npc.EyeColor;
    //     customise[(int) CustomizeIndex.HairColor] = npc.HairColor;
    //     customise[(int) CustomizeIndex.HairColor2] = npc.HairHighlightColor;
    //     customise[(int) CustomizeIndex.FaceFeatures] = npc.FacialFeature;
    //     customise[(int) CustomizeIndex.FaceFeaturesColor] = npc.FacialFeatureColor;
    //     customise[(int) CustomizeIndex.Eyebrows] = npc.Eyebrows;
    //     customise[(int) CustomizeIndex.EyeColor2] = npc.EyeHeterochromia;
    //     customise[(int) CustomizeIndex.EyeShape] = npc.EyeShape;
    //     customise[(int) CustomizeIndex.NoseShape] = npc.Nose;
    //     customise[(int) CustomizeIndex.JawShape] = npc.Jaw;
    //     customise[(int) CustomizeIndex.LipStyle] = npc.Mouth;
    //     customise[(int) CustomizeIndex.LipColor] = npc.LipColor;
    //     customise[(int) CustomizeIndex.RaceFeatureSize] = npc.BustOrTone1;
    //     customise[(int) CustomizeIndex.RaceFeatureType] = npc.ExtraFeature1;
    //     customise[(int) CustomizeIndex.BustSize] = npc.ExtraFeature2OrBust;
    //     customise[(int) CustomizeIndex.Facepaint] = npc.FacePaint;
    //     customise[(int) CustomizeIndex.FacepaintColor] = npc.FacePaintColor;
    // }

    // private enum EquipSlot : uint {
    //     Head = 0,
    //     Body = 1,
    //     Hands = 2,
    //     Legs = 3,
    //     Feet = 4,
    //     Ears = 5,
    //     Neck = 6,
    //     Wrists = 7,
    //     RightRing = 8,
    //     LeftRing = 9,
    // }

    // private unsafe void OnFlagSlotUpdate(GameObject* gameObj, uint slot, EquipData* equipData) {
    //     if (equipData == null) {
    //         return;
    //     }
    //
    //     if (!ShouldObscureAppearance(gameObj)) {
    //         return;
    //     }
    //
    //     var chara = (FFXIVClientStructs.FFXIV.Client.Game.Character.Character*) gameObj;
    //     var (mainHand, offHand) = Plugin.AppearanceRepository.GetHands(chara->ClassJob, gameObj->ObjectID);
    //
    //     var npc = Plugin.AppearanceRepository.GetNpc(gameObj->ObjectID);
    //     var itemSlot = (EquipSlot) slot;
    //     var info = itemSlot switch {
    //         EquipSlot.Head => (npc.ModelHead, npc.DyeHead.Row),
    //         EquipSlot.Body => (npc.ModelBody, npc.DyeBody.Row),
    //         EquipSlot.Hands => (npc.ModelHands, npc.DyeHands.Row),
    //         EquipSlot.Legs => (npc.ModelLegs, npc.DyeLegs.Row),
    //         EquipSlot.Feet => (npc.ModelFeet, npc.DyeFeet.Row),
    //         EquipSlot.Ears => (npc.ModelEars, npc.DyeEars.Row),
    //         EquipSlot.Neck => (npc.ModelNeck, npc.DyeNeck.Row),
    //         EquipSlot.Wrists => (npc.ModelWrists, npc.DyeWrists.Row),
    //         EquipSlot.RightRing => (npc.ModelRightRing, npc.DyeRightRing.Row),
    //         EquipSlot.LeftRing => (npc.ModelLeftRing, npc.DyeLeftRing.Row),
    //         // EquipSlot.MainHand => (mainHand.ModelMain, npc.DyeMainHand.Row),
    //         // EquipSlot.OffHand => (mainHand.ModelSub != 0 ? mainHand.ModelSub : offHand?.ModelMain ?? 0, npc.DyeOffHand.Row),
    //         _ => (uint.MaxValue, uint.MaxValue),
    //     };
    //
    //     if (info.Item1 == uint.MaxValue) {
    //         return;
    //     }
    //
    //     equipData->Model = (ushort) (info.Item1 & 0xFFFF);
    //     equipData->Variant = (byte) ((info.Item1 >> 16) & 0xFF);
    //     equipData->Dye = (byte) info.Item2;
    // }

    private void OnNamePlateUpdate(NamePlateUpdateEventArgs args) {
        // only replace nameplates that have objects in the table
        // note: 
        if (!Plugin.Config.Enabled || !Plugin.NameRepository.Initialised || args.ObjectId == 0xE0000000) {
            return;
        }

        // find the object this nameplate references
        // note: 遍历ObjectTable，找到事件中ObjectId对应的GameObject
        var obj = Service.ObjectTable.FirstOrDefault(o => o.ObjectId == args.ObjectId);
        if (obj == null) {
            return;
        }

        // handle owners
        // note: 如果找到的GameObject的OwnerId存在，就找到它的Owner，并将操作对象改为Owner
        if (obj.OwnerId != 0xE0000000) {
            if (Service.ObjectTable.FirstOrDefault(o => o.ObjectId == obj.OwnerId) is not { } owner) {
                return;
            }

            obj = owner;
        }

        // only work for characters
        // note: 当前Object的类型需要是玩家
        if (obj.ObjectKind != ObjectKind.Player || obj is not Character chara) {
            return;
        }

        var info = GetInfo(chara);

        void Change(string name) {
            ChangeName(args.Name, name);
            ChangeName(args.Title, name);
        }

        var name = chara.Name.TextValue;
        // note: 玩家自己的ObjectId
        var playerId = Service.ClientState.LocalPlayer?.ObjectId;
        
        // note: 如果配置中配了，并且当前对象是自己
        if (chara.ObjectId == playerId) {
            Change(name);
        }
    }

    private void OnChatMessage(XivChatType type, uint senderId, ref SeString sender, ref SeString message, ref bool isHandled) {
        ChangeNames(sender);
        ChangeNames(message);
    }

    // note: 将text的文本改掉
    private void ChangeName(SeString text, string name) {
        // note: 生成了一个替换名replacement
        var replacement = Plugin.NameRepository.GetReplacement();

        // note: 将text的文本替换成replacement
        text.ReplacePlayerName(name, replacement);
    }

    // PERFORMANCE NOTE: This potentially loops over the party list twice and the object
    //                   table once entirely. Should be avoided if being used in a
    //                   position where the player to replace is known.
    private bool ChangeNames(SeString text) {
        if (!Plugin.Config.Enabled || !Plugin.NameRepository.Initialised) {
            return false;
        }

        var changed = false;

        var player = Service.ClientState.LocalPlayer;

        if (player != null) {
            var playerName = player.RawName()!;
            if (Plugin.NameRepository.GetReplacement() is { } replacement) {
                text.ReplacePlayerName(playerName, replacement);
                changed = true;
            }
        }

        return changed;
    }

    private (byte race, byte clan, byte gender) GetInfo(Character chara) {
        // if (ShouldObscureAppearance(chara)) {
        //     var npc = Plugin.AppearanceRepository.GetNpc(chara.ObjectId);
        //     return (
        //         (byte) npc.Race.Row,
        //         (byte) ((npc.Tribe.Row - 1) % 2),
        //         npc.Gender
        //     );
        // }

        return (
            chara.Customize[(byte) CustomizeIndex.Race],
            (byte) ((chara.Customize[(byte) CustomizeIndex.Tribe] - 1) % 2),
            chara.Customize[(byte) CustomizeIndex.Gender]
        );
    }
}
