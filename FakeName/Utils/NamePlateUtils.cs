using System;
using System.Runtime.InteropServices;
using Dalamud.Logging;
using Dalamud.Plugin;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using FFXIVClientStructs.FFXIV.Client.System.String;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace FakeName.Utils;

// https://github.com/shdwp/xivPartyIcons/blob/main/PartyIcons/Api/XivApi.cs
public static class NamePlateUtils
{
    private static IntPtr _RaptureAtkModulePtr = IntPtr.Zero;

    public static IntPtr RaptureAtkModulePtr
    {
        get
        {
            if (_RaptureAtkModulePtr == IntPtr.Zero)
            {
                unsafe
                {
                    var framework = Framework.Instance();
                    var uiModule = framework->GetUiModule();

                    _RaptureAtkModulePtr = new IntPtr(uiModule->GetRaptureAtkModule());
                }
            }

            return _RaptureAtkModulePtr;
        }
    }
    
    public static SafeAddonNamePlate GetSafeAddonNamePlate() => new(Service.Interface);
    
    public class SafeAddonNamePlate
    {
        private readonly DalamudPluginInterface Interface;

        public IntPtr Pointer => Service.GameGui.GetAddonByName("NamePlate", 1);

        public SafeAddonNamePlate(DalamudPluginInterface pluginInterface)
        {
            Interface = pluginInterface;
        }

        public unsafe SafeNamePlateObject GetNamePlateObject(int index)
        {
            if (Pointer == IntPtr.Zero)
            {
                return null;
            }

            var npObjectArrayPtrPtr = Pointer + Marshal
                                                .OffsetOf(typeof(AddonNamePlate), nameof(AddonNamePlate.NamePlateObjectArray)).ToInt32();
            var npObjectArrayPtr = Marshal.ReadIntPtr(npObjectArrayPtrPtr);

            if (npObjectArrayPtr == IntPtr.Zero)
            {
                // Service.Log.Verbose($"[{GetType().Name}] NamePlateObjectArray was null");
                return null;
            }

            var npObjectPtr = npObjectArrayPtr + Marshal.SizeOf(typeof(AddonNamePlate.NamePlateObject)) * index;

            return new SafeNamePlateObject(npObjectPtr, index);
        }
    }
    
    public class SafeNamePlateObject
    {
        public readonly IntPtr Pointer;
        public readonly AddonNamePlate.NamePlateObject Data;
        
        private int _Index;
        private SafeNamePlateInfo _NamePlateInfo;

        public SafeNamePlateObject(IntPtr pointer, int index = -1)
        {
            Pointer = pointer;
            Data = Marshal.PtrToStructure<AddonNamePlate.NamePlateObject>(pointer);
            _Index = index;
        }

        public int Index
        {
            get
            {
                if (_Index == -1)
                {
                    var addon = GetSafeAddonNamePlate();
                    var npObject0 = addon.GetNamePlateObject(0);

                    if (npObject0 == null)
                    {
                        PluginLog.Verbose($"[{GetType().Name}] NamePlateObject0 was null");

                        return -1;
                    }

                    var npObjectBase = npObject0.Pointer;
                    var npObjectSize = Marshal.SizeOf(typeof(AddonNamePlate.NamePlateObject));
                    var index = (Pointer.ToInt64() - npObjectBase.ToInt64()) / npObjectSize;

                    if (index < 0 || index >= 50)
                    {
                        PluginLog.Verbose($"[{GetType().Name}] NamePlateObject index was out of bounds");

                        return -1;
                    }

                    _Index = (int) index;
                }

                return _Index;
            }
        }

        public SafeNamePlateInfo NamePlateInfo
        {
            get
            {
                if (_NamePlateInfo == null)
                {
                    var rapturePtr = RaptureAtkModulePtr;

                    if (rapturePtr == IntPtr.Zero)
                    {
                        PluginLog.Verbose($"[{GetType().Name}] RaptureAtkModule was null");

                        return null;
                    }

                    var npInfoArrayPtr = RaptureAtkModulePtr + Marshal.OffsetOf(typeof(RaptureAtkModule),
                        nameof(RaptureAtkModule.NamePlateInfoArray)).ToInt32();
                    var npInfoPtr = npInfoArrayPtr + Marshal.SizeOf(typeof(RaptureAtkModule.NamePlateInfo)) * Index;
                    _NamePlateInfo = new SafeNamePlateInfo(npInfoPtr);
                }

                return _NamePlateInfo;
            }
        }

        #region Getters

        public unsafe IntPtr IconImageNodeAddress => Marshal.ReadIntPtr(Pointer + Marshal
            .OffsetOf(typeof(AddonNamePlate.NamePlateObject), nameof(AddonNamePlate.NamePlateObject.IconImageNode))
            .ToInt32());

        public unsafe IntPtr NameNodeAddress => Marshal.ReadIntPtr(Pointer + Marshal
            .OffsetOf(typeof(AddonNamePlate.NamePlateObject), nameof(AddonNamePlate.NamePlateObject.NameText))
            .ToInt32());

        public AtkImageNode IconImageNode => Marshal.PtrToStructure<AtkImageNode>(IconImageNodeAddress);
        public AtkTextNode NameTextNode => Marshal.PtrToStructure<AtkTextNode>(NameNodeAddress);

        #endregion

        public unsafe bool IsVisible => Data.IsVisible;

        public unsafe bool IsLocalPlayer => Data.IsLocalPlayer;

        public bool IsPlayer => Data.NameplateKind == 0;

        /// <returns>True if the icon scale was changed.</returns>
        // public bool SetIconScale(float scale, bool force = false)
        // {
        //     if (force || !IsIconScaleEqual(scale))
        //     {
        //         Instance.SetNodeScale(IconImageNodeAddress, scale, scale);
        //         return true;
        //     }
        //
        //     return false;
        // }

        /// <returns>True if the name scale was changed.</returns>
        // public bool SetNameScale(float scale, bool force = false)
        // {
        //     if (force || !IsNameScaleEqual(scale))
        //     {
        //         Instance.SetNodeScale(NameNodeAddress, scale, scale);
        //         return true;
        //     }
        //
        //     return false;
        // }

        public unsafe void SetName(IntPtr ptr)
        {
            NameTextNode.SetText("aaa");
        }

        public void SetIcon(int icon)
        {
            IconImageNode.LoadIconTexture(icon, 1);
        }

        public void SetIconPosition(short x, short y)
        {
            var iconXAdjustPtr = Pointer + Marshal.OffsetOf(typeof(AddonNamePlate.NamePlateObject),
                nameof(AddonNamePlate.NamePlateObject.IconXAdjust)).ToInt32();
            var iconYAdjustPtr = Pointer + Marshal.OffsetOf(typeof(AddonNamePlate.NamePlateObject),
                nameof(AddonNamePlate.NamePlateObject.IconYAdjust)).ToInt32();
            Marshal.WriteInt16(iconXAdjustPtr, x);
            Marshal.WriteInt16(iconYAdjustPtr, y);
        }
        
        private static bool NearlyEqual(float left, float right, float tolerance)
        {
            return Math.Abs(left - right) <= tolerance; 
        }
        
        private bool IsIconScaleEqual(float scale) =>
            NearlyEqual(scale, IconImageNode.AtkResNode.ScaleX, ScaleTolerance) &&
            NearlyEqual(scale, IconImageNode.AtkResNode.ScaleY, ScaleTolerance);

        private bool IsNameScaleEqual(float scale) =>
            NearlyEqual(scale, NameTextNode.AtkResNode.ScaleX, ScaleTolerance) &&
            NearlyEqual(scale, NameTextNode.AtkResNode.ScaleY, ScaleTolerance);
        
        private const float ScaleTolerance = 0.001f;
    }
    
    public class SafeNamePlateInfo
    {
        public readonly IntPtr Pointer;
        public readonly RaptureAtkModule.NamePlateInfo Data;

        public SafeNamePlateInfo(IntPtr pointer)
        {
            Pointer = pointer; //-0x10;
            Data = Marshal.PtrToStructure<RaptureAtkModule.NamePlateInfo>(Pointer);
        }

        #region Getters

        public IntPtr NameAddress => GetStringPtr(nameof(RaptureAtkModule.NamePlateInfo.Name));

        public string Name => GetString(NameAddress);

        public IntPtr FcNameAddress => GetStringPtr(nameof(RaptureAtkModule.NamePlateInfo.FcName));

        public string FcName => GetString(FcNameAddress);

        public IntPtr TitleAddress => GetStringPtr(nameof(RaptureAtkModule.NamePlateInfo.Title));

        public string Title => GetString(TitleAddress);

        public IntPtr DisplayTitleAddress => GetStringPtr(nameof(RaptureAtkModule.NamePlateInfo.DisplayTitle));

        public string DisplayTitle => GetString(DisplayTitleAddress);

        public IntPtr LevelTextAddress => GetStringPtr(nameof(RaptureAtkModule.NamePlateInfo.LevelText));

        public string LevelText => GetString(LevelTextAddress);

        #endregion

        // public bool IsPlayerCharacter() => XivApi.IsPlayerCharacter(Data.ObjectID.ObjectID);
        //
        // public bool IsPartyMember() => XivApi.IsPartyMember(Data.ObjectID.ObjectID);
        //
        // public bool IsAllianceMember() => XivApi.IsAllianceMember(Data.ObjectID.ObjectID);
        //
        // public uint GetJobID() => GetJobId(Data.ObjectID.ObjectID);

        private unsafe IntPtr GetStringPtr(string name)
        {
            var namePtr = Pointer + Marshal.OffsetOf(typeof(RaptureAtkModule.NamePlateInfo), name).ToInt32();
            var stringPtrPtr =
                namePtr + Marshal.OffsetOf(typeof(Utf8String), nameof(Utf8String.StringPtr)).ToInt32();
            var stringPtr = Marshal.ReadIntPtr(stringPtrPtr);

            return stringPtr;
        }

        private string GetString(IntPtr stringPtr) => Marshal.PtrToStringUTF8(stringPtr);
    }
}
