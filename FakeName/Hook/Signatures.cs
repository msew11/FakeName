using System;
using System.Runtime.InteropServices;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace FakeName.Hook;

[UnmanagedFunctionPointer(CallingConvention.ThisCall, CharSet = CharSet.Ansi)]
public delegate IntPtr SetNamePlateDelegate(IntPtr addon, bool isPrefixTitle, bool displayTitle, IntPtr title,
                                            IntPtr name, IntPtr fcName, IntPtr prefix, int iconId);

[UnmanagedFunctionPointer(CallingConvention.ThisCall, CharSet = CharSet.Ansi)]
public delegate void AtkTextNodeSetTextDelegate(IntPtr node, IntPtr text);

[UnmanagedFunctionPointer(CallingConvention.ThisCall, CharSet = CharSet.Ansi)]
public unsafe delegate void* UpdateNameplateNpcDelegate(
    RaptureAtkModule* raptureAtkModule, RaptureAtkModule.NamePlateInfo* namePlateInfo, NumberArrayData* numArray,
    StringArrayData* stringArray, GameObject* gameObject, int numArrayIndex, int stringArrayIndex);

[UnmanagedFunctionPointer(CallingConvention.ThisCall, CharSet = CharSet.Ansi)]
public unsafe delegate void* UpdateNamePlateDelegate(
    RaptureAtkModule* raptureAtkModule, RaptureAtkModule.NamePlateInfo* namePlateInfo, NumberArrayData* numArray,
    StringArrayData* stringArray, BattleChara* battleChara, int numArrayIndex, int stringArrayIndex);

public static class Signatures
{
    public const string SetNamePlate =
        "E8 ?? ?? ?? ?? E9 ?? ?? ?? ?? 48 8B 5C 24 ?? 45 38 BE";
    
    public const string AtkTextNodeSetText = "E8 ?? ?? ?? ?? 8D 4E 32";

    // from: https://github.com/Caraxi/Honorific/blob/master/Plugin.cs
    public const string UpdateNamePlateNpc =
        "48 89 5C 24 ?? 48 89 6C 24 ?? 48 89 74 24 ?? 4C 89 44 24 ?? 57 41 54 41 55 41 56 41 57 48 83 EC 20 48 8B 74 24 ??";

    // from: https://github.com/Caraxi/Honorific/blob/master/Plugin.cs
    public const string UpdateNamePlate = "40 55 56 57 41 56 48 81 EC ?? ?? ?? ?? 48 8B 84 24";
}
