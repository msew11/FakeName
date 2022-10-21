using System;
using Dalamud.Game;

namespace XivCommon.Functions.Housing {
    /// <summary>
    /// The class containing housing functionality
    /// </summary>
    public class Housing {
        private static class Signatures {
            internal const string HousingPointer = "48 8B 05 ?? ?? ?? ?? 48 83 78 ?? ?? 74 16 48 8D 8F ?? ?? ?? ?? 66 89 5C 24 ?? 48 8D 54 24 ?? E8 ?? ?? ?? ?? 48 8B 7C 24";
        }

        private IntPtr HousingPointer { get; }

        /// <summary>
        /// Gets the raw struct containing information about the player's current location in a housing ward.
        ///
        /// <returns>struct if player is in a housing ward, null otherwise</returns>
        /// </summary>
        // Updated: 6.0
        // 48 89 5C 24 ?? 48 89 6C 24 ?? 48 89 74 24 ?? 57 41 56 41 57 48 83 EC 20 49 8B 00 (ward?)
        public unsafe RawHousingLocation? RawLocation {
            get {
                if (this.HousingPointer == IntPtr.Zero) {
                    return null;
                }

                var loc = Util.FollowPointerChain(this.HousingPointer, new[] { 0, 0 });
                if (loc == IntPtr.Zero) {
                    return null;
                }

                var locPtr = (RawHousingLocation*) (loc + 0x96a0);
                return *locPtr;
            }
        }

        /// <summary>
        /// Gets process information about the player's current location in a housing ward.
        ///
        /// <returns>information class if player is in a housing ward, null otherwise</returns>
        /// </summary>
        public HousingLocation? Location {
            get {
                var loc = this.RawLocation;
                return loc == null ? null : new HousingLocation(loc.Value);
            }
        }

        internal Housing(SigScanner scanner) {
            if (scanner.TryGetStaticAddressFromSig(Signatures.HousingPointer, out var ptr)) {
                this.HousingPointer = ptr;
            }
        }
    }
}
