namespace XivCommon.Functions.Housing {
    /// <summary>
    /// Information about a player's current location in a housing ward.
    /// </summary>
    public class HousingLocation {
        /// <summary>
        /// The housing ward that the player is in.
        /// </summary>
        public ushort Ward;
        /// <summary>
        /// <para>
        /// The yard that the player is in.
        /// </para>
        /// <para>
        /// This is the same as plot number but indicates that the player is in
        /// the exterior area (the yard) of that plot.
        /// </para>
        /// </summary>
        public ushort? Yard;
        /// <summary>
        /// The plot that the player is in.
        /// </summary>
        public ushort? Plot;
        /// <summary>
        /// The apartment wing (1 or 2 for normal or subdivision) that the
        /// player is in.
        /// </summary>
        public ushort? ApartmentWing;
        /// <summary>
        /// The apartment that the player is in.
        /// </summary>
        public ushort? Apartment;

        internal HousingLocation(RawHousingLocation loc) {
            var ward = loc.CurrentWard;

            if ((loc.CurrentPlot & 0x80) > 0) {
                // the struct is in apartment mode
                this.ApartmentWing = (ushort?) ((loc.CurrentPlot & ~0x80) + 1);
                this.Apartment = (ushort?) (ward >> 6);
                this.Ward = (ushort) ((ward & 0x3F) + 1);
                if (this.Apartment == 0) {
                    this.Apartment = null;
                }
            } else if (loc.InsideIndicator == 0) {
                // inside a plot
                this.Plot = (ushort?) (loc.CurrentPlot + 1);
            } else if (loc.CurrentYard != 0xFF) {
                // not inside a plot
                // yard is 0xFF when not in one
                this.Yard = (ushort?) (loc.CurrentYard + 1);
            }

            if (this.Ward == 0) {
                this.Ward = (ushort) (ward + 1);
            }
        }
    }
}
