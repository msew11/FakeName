using System.Runtime.InteropServices;

namespace XivCommon.Functions.Housing {
    /// <summary>
    /// Information about the player's current location in a housing ward as
    /// kept by the game's internal structures.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public readonly struct RawHousingLocation {
        /// <summary>
        /// The zero-indexed plot number that the player is in.
        ///
        /// <para>
        /// Contains apartment data when inside an apartment building.
        /// </para>
        /// </summary>
        public readonly ushort CurrentPlot; // a0 -> a2
        /// <summary>
        /// The zero-indexed ward number that the player is in.
        ///
        /// <para>
        /// Contains apartment data when inside an apartment building.
        /// </para>
        /// </summary>
        public readonly ushort CurrentWard; // a2 -> a4
        private readonly uint unknownBytes1; // a4 -> a8
        /// <summary>
        /// The zero-indexed yard number that the player is in.
        ///
        /// <para>
        /// Is <c>0xFF</c> when not in a yard.
        /// </para>
        /// </summary>
        public readonly byte CurrentYard; // a8 -> a9
        private readonly byte unknownBytes2; // a9 -> aa
        /// <summary>
        /// A byte that is zero when the player is inside a plot.
        /// </summary>
        public readonly byte InsideIndicator; // aa -> ab
    }
}
