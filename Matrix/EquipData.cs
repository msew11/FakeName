using System.Runtime.InteropServices;

namespace Matrix;

[StructLayout(LayoutKind.Sequential)]
internal struct EquipData {
    internal ushort Model;
    internal byte Variant;
    internal byte Dye;
}
