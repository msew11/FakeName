using System;
using Dalamud.Game;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Game.Gui;
using Dalamud.Game.Gui.PartyFinder;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Component.GUI;
using XivCommon.Functions;
using XivCommon.Functions.FriendList;
using XivCommon.Functions.Housing;
using XivCommon.Functions.NamePlates;
using XivCommon.Functions.Tooltips;
using Framework = FFXIVClientStructs.FFXIV.Client.System.Framework.Framework;

namespace XivCommon {
    /// <summary>
    /// A class containing game functions
    /// </summary>
    public class GameFunctions : IDisposable {
        private GameGui GameGui { get; }

        private Dalamud.Game.Framework Framework { get; }

        internal UiAlloc UiAlloc { get; }

        /// <summary>
        /// Chat functions
        /// </summary>
        public Chat Chat { get; }

        /// <summary>
        /// Party Finder functions and events
        /// </summary>
        public PartyFinder PartyFinder { get; }

        /// <summary>
        /// BattleTalk functions and events
        /// </summary>
        public BattleTalk BattleTalk { get; }

        /// <summary>
        /// Examine functions
        /// </summary>
        public Examine Examine { get; }

        /// <summary>
        /// Talk events
        /// </summary>
        public Talk Talk { get; }

        /// <summary>
        /// Chat bubble functions and events
        /// </summary>
        public ChatBubbles ChatBubbles { get; }

        /// <summary>
        /// Tooltip events
        /// </summary>
        public Tooltips Tooltips { get; }

        /// <summary>
        /// Name plate tools and events
        /// </summary>
        public NamePlates NamePlates { get; }

        /// <summary>
        /// Duty Finder functions
        /// </summary>
        public DutyFinder DutyFinder { get; }

        /// <summary>
        /// Friend list functions
        /// </summary>
        public FriendList FriendList { get; }

        /// <summary>
        /// Journal functions
        /// </summary>
        public Journal Journal { get; }
        
        /// <summary>
        /// Housing functions
        /// </summary>
        public Housing Housing { get; }

        internal GameFunctions(Hooks hooks) {
            this.Framework = Util.GetService<Dalamud.Game.Framework>();
            this.GameGui = Util.GetService<GameGui>();

            var objectTable = Util.GetService<ObjectTable>();
            var partyFinderGui = Util.GetService<PartyFinderGui>();
            var scanner = Util.GetService<SigScanner>();

            this.UiAlloc = new UiAlloc(scanner);
            this.Chat = new Chat(scanner);
            this.PartyFinder = new PartyFinder(scanner, partyFinderGui, hooks);
            this.BattleTalk = new BattleTalk(hooks.HasFlag(Hooks.BattleTalk));
            this.Examine = new Examine(scanner);
            this.Talk = new Talk(scanner, hooks.HasFlag(Hooks.Talk));
            this.ChatBubbles = new ChatBubbles(objectTable, scanner, hooks.HasFlag(Hooks.ChatBubbles));
            this.Tooltips = new Tooltips(scanner, this.GameGui, hooks.HasFlag(Hooks.Tooltips));
            this.NamePlates = new NamePlates(this, scanner, hooks.HasFlag(Hooks.NamePlates));
            this.DutyFinder = new DutyFinder(scanner);
            this.Journal = new Journal(scanner);
            this.FriendList = new FriendList();
            this.Housing = new Housing(scanner);
        }

        /// <inheritdoc />
        public void Dispose() {
            this.NamePlates.Dispose();
            this.Tooltips.Dispose();
            this.ChatBubbles.Dispose();
            this.Talk.Dispose();
            this.BattleTalk.Dispose();
            this.PartyFinder.Dispose();
        }

        /// <summary>
        /// Convenience method to get a pointer to <see cref="Framework"/>.
        /// </summary>
        /// <returns>pointer to struct</returns>
        [Obsolete("Use Framework.Instance()")]
        public unsafe Framework* GetFramework() {
            return (Framework*) this.Framework.Address.BaseAddress;
        }

        /// <summary>
        /// Gets the pointer to the UI module
        /// </summary>
        /// <returns>Pointer</returns>
        [Obsolete("Use Framework.Instance()->GetUiModule()")]
        public unsafe IntPtr GetUiModule() {
            return (IntPtr) this.GetFramework()->GetUiModule();
        }

        /// <summary>
        /// Gets the pointer to the RaptureAtkModule
        /// </summary>
        /// <returns>Pointer</returns>
        [Obsolete("Use Framework.Instance()->GetUiModule()->GetRaptureAtkModule()")]
        public unsafe IntPtr GetAtkModule() {
            return (IntPtr) this.GetFramework()->GetUiModule()->GetRaptureAtkModule();
        }

        /// <summary>
        /// Gets the pointer to the agent module
        /// </summary>
        /// <returns>Pointer</returns>
        [Obsolete("Use Framework.Instance()->GetUiModule()->GetAgentModule()")]
        public unsafe IntPtr GetAgentModule() {
            return (IntPtr) this.GetFramework()->GetUiModule()->GetAgentModule();
        }

        /// <summary>
        /// Gets the pointer to an agent from its internal ID.
        /// </summary>
        /// <param name="id">internal id of agent</param>
        /// <returns>Pointer</returns>
        /// <exception cref="InvalidOperationException">if the signature for the function could not be found</exception>
        [Obsolete("Use Framework.Instance()->GetUiModule()->GetAgentModule()->GetAgentByInternalId(AgentId)")]
        public unsafe IntPtr GetAgentByInternalId(uint id) {
            return (IntPtr) this.GetFramework()->GetUiModule()->GetAgentModule()->GetAgentByInternalId((AgentId) id);
        }

        /// <summary>
        /// Gets the pointer to the AtkStage singleton
        /// </summary>
        /// <returns>Pointer</returns>
        /// <exception cref="InvalidOperationException">if the signature for the function could not be found</exception>
        [Obsolete("Use AtkStage.GetSingleton()")]
        public unsafe IntPtr GetAtkStageSingleton() {
            return (IntPtr) AtkStage.GetSingleton();
        }
    }
}
