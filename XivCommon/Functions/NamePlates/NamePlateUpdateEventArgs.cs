using System;
using Dalamud.Game.Text.SeStringHandling;

namespace XivCommon.Functions.NamePlates {
    /// <summary>
    /// Arguments for the name plate update event
    /// </summary>
    public class NamePlateUpdateEventArgs {
        /// <summary>
        /// The object ID associated with this name plate.
        /// </summary>
        public uint ObjectId { get; }

        /// <summary>
        /// The name string.
        /// </summary>
        public SeString Name { get; set; } = null!;

        /// <summary>
        /// The FC tag string for name plates that use it. Set to the empty string to disable.
        /// </summary>
        public SeString FreeCompany { get; set; } = null!;

        /// <summary>
        /// The title string for name plates that use it. Set to the empty string to disable.
        /// </summary>
        public SeString Title { get; set; } = null!;

        /// <summary>
        /// The level string for name plates that use it. Set to the empty string to disable.
        /// </summary>
        ///
        public SeString Level { get; set; } = null!;

        /// <summary>
        /// <para>
        /// The letter that appears after enemy names, such as A, B, etc.
        /// </para>
        /// <para>
        /// <b>Setting this property will always cause a memory leak.</b>
        /// </para>
        /// </summary>
        public SeString EnemyLetter {
            get;
            [Obsolete("Setting this property will always cause a memory leak.")]
            set;
        } = null!;

        /// <summary>
        /// The icon to be shown on this name plate. Use <see cref="uint.MaxValue"/> for no icon.
        /// </summary>
        public uint Icon { get; set; }

        /// <summary>
        /// The colour of this name plate.
        /// </summary>
        public RgbaColour Colour { get; set; } = new();

        /// <summary>
        /// <para>
        /// The type of this name plate.
        /// </para>
        /// <para>
        /// Changing this without setting the appropriate fields can cause the game to crash.
        /// </para>
        /// </summary>
        public PlateType Type { get; set; }

        /// <summary>
        /// A bitmask of flags for the name plate.
        /// </summary>
        public int Flags { get; set; }

        internal NamePlateUpdateEventArgs(uint objectId) {
            this.ObjectId = objectId;
        }
    }
}
