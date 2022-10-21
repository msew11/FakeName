using Dalamud.Game.Text.SeStringHandling;

namespace XivCommon.Functions.Tooltips {
    /// <summary>
    /// The class allowing for item tooltip manipulation
    /// </summary>
    public unsafe class ItemTooltip : BaseTooltip {
        internal ItemTooltip(Tooltips.StringArrayDataSetStringDelegate sadSetString, byte*** stringArrayData, int** numberArrayData) : base(sadSetString, stringArrayData, numberArrayData) {
        }

        /// <summary>
        /// Gets or sets the SeString for the given string enum.
        /// </summary>
        /// <param name="its">the string to retrieve/update</param>
        public SeString this[ItemTooltipString its] {
            get => this[(int) its];
            set => this[(int) its] = value;
        }

        /// <summary>
        /// Gets or sets which fields are visible on the tooltip.
        /// </summary>
        public ItemTooltipFields Fields {
            get => (ItemTooltipFields) (*(*(this.NumberArrayData + 4) + 4));
            set => *(*(this.NumberArrayData + 4) + 4) = (int) value;
        }
    }
}
