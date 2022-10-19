using System;
using System.Runtime.InteropServices;
using Dalamud.Game;

namespace Matrix;

[UnmanagedFunctionPointer(CallingConvention.ThisCall, CharSet = CharSet.Ansi)]
public delegate IntPtr SetNamePlateDelegate(IntPtr addon, bool isPrefixTitle, bool displayTitle, IntPtr title,
    IntPtr name, IntPtr fcName, int iconID);

public sealed class PluginAddressResolver : BaseAddressResolver
{
    private const string AddonNamePlate_SetNamePlateSignature =
        "48 89 5C 24 ?? 48 89 6C 24 ?? 56 57 41 54 41 56 41 57 48 83 EC 40 44 0F B6 E2";

    public IntPtr AddonNamePlate_SetNamePlatePtr;

    protected override void Setup64Bit(SigScanner scanner)
    {
        AddonNamePlate_SetNamePlatePtr = scanner.ScanText(AddonNamePlate_SetNamePlateSignature);
    }
}
