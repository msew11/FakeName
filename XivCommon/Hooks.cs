using System;

namespace XivCommon {
    /// <summary>
    /// Flags for which hooks to use
    /// </summary>
    [Flags]
    public enum Hooks {
        /// <summary>
        /// No hook.
        ///
        /// This flag is used to disable all hooking.
        /// </summary>
        None = 0,

        /// <summary>
        /// The Tooltips hooks.
        ///
        /// This hook is used in order to enable the tooltip events.
        /// </summary>
        Tooltips = 1 << 0,

        /// <summary>
        /// The BattleTalk hook.
        ///
        /// This hook is used in order to enable the BattleTalk events.
        /// </summary>
        BattleTalk = 1 << 1,

        /// <summary>
        /// Hooks used for refreshing Party Finder listings.
        /// </summary>
        PartyFinderListings = 1 << 2,

        /// <summary>
        /// Hooks used for Party Finder join events.
        /// </summary>
        PartyFinderJoins = 1 << 3,

        /// <summary>
        /// All Party Finder hooks.
        ///
        /// This hook is used in order to enable all Party Finder functions.
        /// </summary>
        PartyFinder = PartyFinderListings | PartyFinderJoins,

        /// <summary>
        /// The Talk hooks.
        ///
        /// This hook is used in order to enable the Talk events.
        /// </summary>
        Talk = 1 << 4,

        /// <summary>
        /// The chat bubbles hooks.
        ///
        /// This hook is used in order to enable the chat bubbles events.
        /// </summary>
        ChatBubbles = 1 << 5,

        // 1 << 6 used to be ContextMenu

        /// <summary>
        /// The name plate hooks.
        ///
        /// This hook is used in order to enable name plate functions.
        /// </summary>
        NamePlates = 1 << 7,
    }

    internal static class HooksExt {
        internal const Hooks DefaultHooks = Hooks.None;
    }
}
