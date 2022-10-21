using System.Runtime.InteropServices;
using FFXIVClientStructs.FFXIV.Client.Graphics;

namespace XivCommon.Functions.NamePlates {
    [StructLayout(LayoutKind.Explicit, Size = 0x28)]
    internal unsafe struct NumberArrayData {
        [FieldOffset(0x0)]
        public AtkArrayData AtkArrayData;

        [FieldOffset(0x20)]
        public int* IntArray;

        public void SetValue(int index, int value) {
            if (index >= this.AtkArrayData.Size) {
                return;
            }

            if (this.IntArray[index] == value) {
                return;
            }

            this.IntArray[index] = value;
            this.AtkArrayData.HasModifiedData = 1;
        }
    }

    [StructLayout(LayoutKind.Explicit, Size = 0x20)]
    internal unsafe struct AtkArrayData {
        [FieldOffset(0x0)]
        public void* vtbl;

        [FieldOffset(0x8)]
        public int Size;

        [FieldOffset(0x1C)]
        public byte Unk1C;

        [FieldOffset(0x1D)]
        public byte Unk1D;

        [FieldOffset(0x1E)]
        public byte HasModifiedData;

        [FieldOffset(0x1F)]
        public byte Unk1F; // initialized to -1
    }

    [StructLayout(LayoutKind.Explicit, Size = 0x30)]
    internal unsafe struct StringArrayData {
        [FieldOffset(0x0)]
        public AtkArrayData AtkArrayData;

        [FieldOffset(0x20)]
        public byte** StringArray; // char * *

        [FieldOffset(0x28)]
        public byte* UnkString; // char *
    }

    /// <summary>
    /// The various different name plate types
    /// </summary>
    public enum PlateType {
        /// <summary>
        /// A normal player name plate
        /// </summary>
        Player = 0,

        /// <summary>
        /// A name plate with the icon and FC tag removed
        /// </summary>
        NoIconOrFc = 1, // 2, 5

        /// <summary>
        /// A name plate with a level string visible, title always below the name, and FC tag removed
        /// </summary>
        LevelNoFc = 3, // 4

        /// <summary>
        /// A name plate with only the name visible
        /// </summary>
        NameOnly = 6,

        /// <summary>
        /// A name plate with only the level string and name visible
        /// </summary>
        LevelAndName = 7,

        /// <summary>
        /// A name plate where the title always appears below the name and the FC tag is removed
        /// </summary>
        LowTitleNoFc = 8,
    }

    /// <summary>
    /// A colour, represented in the RGBA format.
    /// </summary>
    public class RgbaColour {
        /// <summary>
        /// The red component of the colour.
        /// </summary>
        public byte R { get; set; }

        /// <summary>
        /// The green component of the colour.
        /// </summary>
        public byte G { get; set; }

        /// <summary>
        /// The blue component of the colour.
        /// </summary>
        public byte B { get; set; }

        /// <summary>
        /// The alpha component of the colour.
        /// </summary>
        public byte A { get; set; } = byte.MaxValue;

        /// <summary>
        /// Converts an unsigned integer into an RgbaColour.
        /// </summary>
        /// <param name="rgba">32-bit integer representing an RGBA colour</param>
        /// <returns>an RgbaColour equivalent to the integer representation</returns>
        public static implicit operator RgbaColour(uint rgba) {
            var r = (byte) ((rgba >> 24) & 0xFF);
            var g = (byte) ((rgba >> 16) & 0xFF);
            var b = (byte) ((rgba >> 8) & 0xFF);
            var a = (byte) (rgba & 0xFF);

            return new RgbaColour {
                R = r,
                G = g,
                B = b,
                A = a,
            };
        }

        /// <summary>
        /// Converts an RgbaColour into an unsigned integer representation.
        /// </summary>
        /// <param name="rgba">an RgbaColour to convert</param>
        /// <returns>32-bit integer representing an RGBA colour</returns>
        public static implicit operator uint(RgbaColour rgba) {
            return (uint) ((rgba.R << 24)
                           | (rgba.G << 16)
                           | (rgba.B << 8)
                           | rgba.A);
        }

        /// <summary>
        /// Converts a ByteColor into an RgbaColour.
        /// </summary>
        /// <param name="rgba">ByteColor</param>
        /// <returns>equivalent RgbaColour</returns>
        public static implicit operator RgbaColour(ByteColor rgba) {
            return (uint) ((rgba.R << 24)
                           | (rgba.G << 16)
                           | (rgba.B << 8)
                           | rgba.A);
        }

        /// <summary>
        /// Converts an RgbaColour into a ByteColor.
        /// </summary>
        /// <param name="rgba">RgbaColour</param>
        /// <returns>equivalent ByteColour</returns>
        public static implicit operator ByteColor(RgbaColour rgba) {
            return new() {
                R = rgba.R,
                G = rgba.G,
                B = rgba.B,
                A = rgba.A,
            };
        }
    }
}
