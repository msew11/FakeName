using System;
using System.Collections.Generic;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;

namespace XivCommon.Functions.FriendList {
    /// <summary>
    /// The class containing friend list functionality
    /// </summary>
    public class FriendList {
        // Updated: 5.58-HF1
        private const int InfoOffset = 0x28;
        private const int LengthOffset = 0x10;
        private const int ListOffset = 0x98;

        /// <summary>
        /// <para>
        /// A live list of the currently-logged-in player's friends.
        /// </para>
        /// <para>
        /// The list is empty if not logged in.
        /// </para>
        /// </summary>
        public unsafe IList<FriendListEntry> List {
            get {
                var friendListAgent = (IntPtr) Framework.Instance()
                    ->GetUiModule()
                    ->GetAgentModule()
                    ->GetAgentByInternalId(AgentId.SocialFriendList);
                if (friendListAgent == IntPtr.Zero) {
                    return Array.Empty<FriendListEntry>();
                }

                var info = *(IntPtr*) (friendListAgent + InfoOffset);
                if (info == IntPtr.Zero) {
                    return Array.Empty<FriendListEntry>();
                }

                var length = *(ushort*) (info + LengthOffset);
                if (length == 0) {
                    return Array.Empty<FriendListEntry>();
                }

                var list = *(IntPtr*) (info + ListOffset);
                if (list == IntPtr.Zero) {
                    return Array.Empty<FriendListEntry>();
                }

                var entries = new List<FriendListEntry>(length);
                for (var i = 0; i < length; i++) {
                    var entry = *(FriendListEntry*) (list + i * FriendListEntry.Size);
                    entries.Add(entry);
                }

                return entries;
            }
        }

        internal FriendList() {
        }
    }
}
