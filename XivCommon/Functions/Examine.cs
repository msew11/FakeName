using System;
using System.Runtime.InteropServices;
using Dalamud.Game;
using Dalamud.Game.ClientState.Objects.Types;
using Framework = FFXIVClientStructs.FFXIV.Client.System.Framework.Framework;

namespace XivCommon.Functions {
    /// <summary>
    /// Class containing examine functions
    /// </summary>
    public class Examine {
        private static class Signatures {
            internal const string RequestCharacterInfo = "48 89 5C 24 ?? 57 48 83 EC 40 BA ?? ?? ?? ?? 48 8B D9 E8 ?? ?? ?? ?? 48 8B F8 48 85 C0 74 16";
        }

        private delegate long RequestCharInfoDelegate(IntPtr ptr);

        private RequestCharInfoDelegate? RequestCharacterInfo { get; }

        internal Examine(SigScanner scanner) {
            // got this by checking what accesses rciData below
            if (scanner.TryScanText(Signatures.RequestCharacterInfo, out var rciPtr, "Examine")) {
                this.RequestCharacterInfo = Marshal.GetDelegateForFunctionPointer<RequestCharInfoDelegate>(rciPtr);
            }
        }

        /// <summary>
        /// Opens the Examine window for the specified object.
        /// </summary>
        /// <param name="object">Object to open window for</param>
        /// <exception cref="InvalidOperationException">If the signature for this function could not be found</exception>
        public void OpenExamineWindow(GameObject @object) {
            this.OpenExamineWindow(@object.ObjectId);
        }

        /// <summary>
        /// Opens the Examine window for the object with the specified ID.
        /// </summary>
        /// <param name="objectId">Object ID to open window for</param>
        /// <exception cref="InvalidOperationException">If the signature for this function could not be found</exception>
        public unsafe void OpenExamineWindow(uint objectId) {
            if (this.RequestCharacterInfo == null) {
                throw new InvalidOperationException("Could not find signature for Examine function");
            }

            // NOTES LAST UPDATED: 6.0

            // offsets and stuff come from the beginning of case 0x2c (around line 621 in IDA)
            // if 29f8 ever changes, I'd just scan for it in old binary and find what it is in the new binary at the same spot
            // 40 55 53 57 41 54 41 55 41 56 48 8D 6C 24 ??
            var agentModule = (IntPtr) Framework.Instance()->GetUiModule()->GetAgentModule();
            var rciData = Marshal.ReadIntPtr(agentModule + 0x1A0);

            // offsets at sig E8 ?? ?? ?? ?? 33 C0 EB 4C
            // this is called at the end of the 2c case
            var raw = (uint*) rciData;
            *(raw + 10) = objectId;
            *(raw + 11) = objectId;
            *(raw + 12) = objectId;
            *(raw + 13) = 0xE0000000;
            *(raw + 301) = 0;

            this.RequestCharacterInfo(rciData);
        }
    }
}
