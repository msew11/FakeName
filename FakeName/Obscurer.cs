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

namespace FakeName;

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

    private static readonly Regex Coords = new(@"^X:\W*\d+.*Y:\W*\d+.*(?:Z:\W*\d+.*)?$", RegexOptions.Compiled);

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
            var replacement = Plugin.NameRepository.GetReplacement();
            var textValue = text.TextValue;
            if (textValue.Contains(replacement))
            {
                // 已经改过了 不改
                return false;
            }
            if (!textValue.Contains(playerName))
            {
                // 已经改过了 不改
                return false;
            }
            
            text.ReplacePlayerName(playerName, replacement);
            changed = true;
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
