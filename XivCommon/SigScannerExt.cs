using System;
using System.Collections.Generic;
using Dalamud.Game;

namespace XivCommon {
    internal static class SigScannerExt {
        /// <summary>
        /// Scan for a signature in memory.
        /// </summary>
        /// <param name="scanner">SigScanner to use for scanning</param>
        /// <param name="sig">signature to search for</param>
        /// <param name="result">pointer where signature was found or <see cref="IntPtr.Zero"/> if not found</param>
        /// <param name="name">name of this signature - if specified, a warning will be printed if the signature could not be found</param>
        /// <returns>true if signature was found</returns>
        internal static bool TryScanText(this SigScanner scanner, string sig, out IntPtr result, string? name = null) {
            result = IntPtr.Zero;
            try {
                result = scanner.ScanText(sig);
                return true;
            } catch (KeyNotFoundException) {
                if (name != null) {
                    Util.PrintMissingSig(name);
                }

                return false;
            }
        }
    }
}
