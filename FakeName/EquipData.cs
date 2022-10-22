using System.Runtime.InteropServices;

namespace FakeName;

[StructLayout(LayoutKind.Sequential)]
internal struct EquipData {
    internal ushort Model;
    internal byte Variant;
    internal byte Dye;
}
