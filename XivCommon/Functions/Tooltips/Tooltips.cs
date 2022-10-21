using System;
using System.Runtime.InteropServices;
using Dalamud.Game;
using Dalamud.Game.Gui;
using Dalamud.Hooking;

namespace XivCommon.Functions.Tooltips {
    /// <summary>
    /// The class containing tooltip functionality
    /// </summary>
    public class Tooltips : IDisposable {
        private static class Signatures {
            internal const string AgentItemDetailUpdateTooltip = "E8 ?? ?? ?? ?? 48 8B 5C 24 ?? 48 89 AE";
            internal const string AgentActionDetailUpdateTooltip = "E8 ?? ?? ?? ?? EB 68 FF 50 40";
            internal const string SadSetString = "E8 ?? ?? ?? ?? F6 47 14 08";
        }

        // Last checked: 6.0
        // E8 ?? ?? ?? ?? EB 68 FF 50 40
        private const int AgentActionDetailUpdateFlagOffset = 0x58;

        internal unsafe delegate void StringArrayDataSetStringDelegate(IntPtr self, int index, byte* str, byte updatePtr, byte copyToUi, byte dontSetModified);

        private unsafe delegate byte ItemUpdateTooltipDelegate(IntPtr agent, int** numberArrayData, byte*** stringArrayData, float a4);

        private unsafe delegate void ActionUpdateTooltipDelegate(IntPtr agent, int** numberArrayData, byte*** stringArrayData);

        private StringArrayDataSetStringDelegate? SadSetString { get; }
        private Hook<ItemUpdateTooltipDelegate>? ItemUpdateTooltipHook { get; }
        private Hook<ActionUpdateTooltipDelegate>? ActionGenerateTooltipHook { get; }

        /// <summary>
        /// The delegate for item tooltip events.
        /// </summary>
        public delegate void ItemTooltipEventDelegate(ItemTooltip itemTooltip, ulong itemId);

        /// <summary>
        /// The tooltip for action tooltip events.
        /// </summary>
        public delegate void ActionTooltipEventDelegate(ActionTooltip actionTooltip, HoveredAction action);

        /// <summary>
        /// <para>
        /// The event that is fired when an item tooltip is being generated for display.
        /// </para>
        /// <para>
        /// Requires the <see cref="Hooks.Tooltips"/> hook to be enabled.
        /// </para>
        /// </summary>
        public event ItemTooltipEventDelegate? OnItemTooltip;

        /// <summary>
        /// <para>
        /// The event that is fired when an action tooltip is being generated for display.
        /// </para>
        /// <para>
        /// Requires the <see cref="Hooks.Tooltips"/> hook to be enabled.
        /// </para>
        /// </summary>
        public event ActionTooltipEventDelegate? OnActionTooltip;

        private GameGui GameGui { get; }
        private ItemTooltip? ItemTooltip { get; set; }
        private ActionTooltip? ActionTooltip { get; set; }

        internal Tooltips(SigScanner scanner, GameGui gui, bool enabled) {
            this.GameGui = gui;

            if (scanner.TryScanText(Signatures.SadSetString, out var setStringPtr, "Tooltips - StringArrayData::SetString")) {
                this.SadSetString = Marshal.GetDelegateForFunctionPointer<StringArrayDataSetStringDelegate>(setStringPtr);
            } else {
                return;
            }

            if (!enabled) {
                return;
            }

            if (scanner.TryScanText(Signatures.AgentItemDetailUpdateTooltip, out var updateItemPtr, "Tooltips - Items")) {
                unsafe {
                    this.ItemUpdateTooltipHook = new Hook<ItemUpdateTooltipDelegate>(updateItemPtr, this.ItemUpdateTooltipDetour);
                }

                this.ItemUpdateTooltipHook.Enable();
            }

            if (scanner.TryScanText(Signatures.AgentActionDetailUpdateTooltip, out var updateActionPtr, "Tooltips - Actions")) {
                unsafe {
                    this.ActionGenerateTooltipHook = new Hook<ActionUpdateTooltipDelegate>(updateActionPtr, this.ActionUpdateTooltipDetour);
                }

                this.ActionGenerateTooltipHook.Enable();
            }
        }

        /// <inheritdoc />
        public void Dispose() {
            this.ActionGenerateTooltipHook?.Dispose();
            this.ItemUpdateTooltipHook?.Dispose();
        }

        private unsafe byte ItemUpdateTooltipDetour(IntPtr agent, int** numberArrayData, byte*** stringArrayData, float a4) {
            var ret = this.ItemUpdateTooltipHook!.Original(agent, numberArrayData, stringArrayData, a4);

            if (ret > 0) {
                try {
                    this.ItemUpdateTooltipDetourInner(numberArrayData, stringArrayData);
                } catch (Exception ex) {
                    Logger.LogError(ex, "Exception in item tooltip detour");
                }
            }

            return ret;
        }

        private unsafe void ItemUpdateTooltipDetourInner(int** numberArrayData, byte*** stringArrayData) {
            this.ItemTooltip = new ItemTooltip(this.SadSetString!, stringArrayData, numberArrayData);

            try {
                this.OnItemTooltip?.Invoke(this.ItemTooltip, this.GameGui.HoveredItem);
            } catch (Exception ex) {
                Logger.LogError(ex, "Exception in OnItemTooltip event");
            }
        }

        private unsafe void ActionUpdateTooltipDetour(IntPtr agent, int** numberArrayData, byte*** stringArrayData) {
            var flag = *(byte*) (agent + AgentActionDetailUpdateFlagOffset);
            this.ActionGenerateTooltipHook!.Original(agent, numberArrayData, stringArrayData);

            if (flag == 0) {
                return;
            }

            try {
                this.ActionUpdateTooltipDetourInner(numberArrayData, stringArrayData);
            } catch (Exception ex) {
                Logger.LogError(ex, "Exception in action tooltip detour");
            }
        }

        private unsafe void ActionUpdateTooltipDetourInner(int** numberArrayData, byte*** stringArrayData) {
            this.ActionTooltip = new ActionTooltip(this.SadSetString!, stringArrayData, numberArrayData);

            try {
                this.OnActionTooltip?.Invoke(this.ActionTooltip, this.GameGui.HoveredAction);
            } catch (Exception ex) {
                Logger.LogError(ex, "Exception in OnActionTooltip event");
            }
        }
    }
}
