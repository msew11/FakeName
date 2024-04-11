using System;
using System.Runtime.InteropServices;

namespace FakeName.Api;

[UnmanagedFunctionPointer(CallingConvention.ThisCall, CharSet = CharSet.Ansi)]
public delegate IntPtr SetNamePlateDelegate(IntPtr addon, bool isPrefixTitle, bool displayTitle, IntPtr title,
                                            IntPtr name, IntPtr fcName, IntPtr prefix, int iconId);

public static class Signatures
{
    public const string SetNamePlate =
        "E8 ?? ?? ?? ?? E9 ?? ?? ?? ?? 48 8B 5C 24 ?? 45 38 BE";
}
