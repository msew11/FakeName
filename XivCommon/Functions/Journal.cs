using System;
using System.Runtime.InteropServices;
using Dalamud.Game;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using Lumina.Excel.GeneratedSheets;
using Framework = FFXIVClientStructs.FFXIV.Client.System.Framework.Framework;

namespace XivCommon.Functions {
    /// <summary>
    /// Journal functions
    /// </summary>
    public class Journal {
        private static class Signatures {
            internal const string OpenQuest = "E8 ?? ?? ?? ?? 48 8B 74 24 ?? 48 8B 7C 24 ?? 48 83 C4 30 5B C3 48 8B CB";
            internal const string IsQuestCompleted = "E8 ?? ?? ?? ?? 41 88 84 2C";
        }

        private delegate IntPtr OpenQuestDelegate(IntPtr agent, int questId, int a3, ushort a4, byte a5);

        private delegate byte IsQuestCompletedDelegate(ushort questId);

        private readonly OpenQuestDelegate? _openQuest;
        private readonly IsQuestCompletedDelegate? _isQuestCompleted;

        internal Journal(SigScanner scanner) {
            if (scanner.TryScanText(Signatures.OpenQuest, out var openQuestPtr, "Journal (open quest)")) {
                this._openQuest = Marshal.GetDelegateForFunctionPointer<OpenQuestDelegate>(openQuestPtr);
            }

            if (scanner.TryScanText(Signatures.IsQuestCompleted, out var questCompletedPtr, "Journal (quest completed)")) {
                this._isQuestCompleted = Marshal.GetDelegateForFunctionPointer<IsQuestCompletedDelegate>(questCompletedPtr);
            }
        }

        /// <summary>
        /// Opens the quest journal to the given quest.
        /// </summary>
        /// <param name="quest">quest to show</param>
        /// <exception cref="InvalidOperationException">if the open quest function could not be found in memory</exception>
        public void OpenQuest(Quest quest) {
            this.OpenQuest(quest.RowId);
        }

        /// <summary>
        /// Opens the quest journal to the given quest ID.
        /// </summary>
        /// <param name="questId">ID of quest to show</param>
        /// <exception cref="InvalidOperationException">if the open quest function could not be found in memory</exception>
        public unsafe void OpenQuest(uint questId) {
            if (this._openQuest == null) {
                throw new InvalidOperationException("Could not find signature for open quest function");
            }

            var agent = (IntPtr) Framework.Instance()->GetUiModule()->GetAgentModule()->GetAgentByInternalId(AgentId.Journal);

            this._openQuest(agent, (int) (questId & 0xFFFF), 1, 0, 1);
        }

        /// <summary>
        /// Checks if the given quest is completed.
        /// </summary>
        /// <param name="quest">quest to check</param>
        /// <returns>true if the quest is completed</returns>
        /// <exception cref="InvalidOperationException">if the function for checking quest completion could not be found in memory</exception>
        public bool IsQuestCompleted(Quest quest) {
            return this.IsQuestCompleted(quest.RowId);
        }

        /// <summary>
        /// Checks if the given quest ID is completed.
        /// </summary>
        /// <param name="questId">ID of quest to check</param>
        /// <returns>true if the quest is completed</returns>
        /// <exception cref="InvalidOperationException">if the function for checking quest completion could not be found in memory</exception>
        public bool IsQuestCompleted(uint questId) {
            if (this._isQuestCompleted == null) {
                throw new InvalidOperationException("Could not find signature for quest completed function");
            }

            return this._isQuestCompleted((ushort) (questId & 0xFFFF)) != 0;
        }
    }
}
