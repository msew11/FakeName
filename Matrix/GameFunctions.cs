using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Hooking;
using Dalamud.Logging;
using Dalamud.Utility.Signatures;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using FFXIVClientStructs.FFXIV.Client.System.Framework;

namespace Matrix;

internal class GameFunctions2 : IDisposable {
    private static class Signatures {
        internal const string GenerateName = "E8 ?? ?? ?? ?? 48 8D 8B ?? ?? ?? ?? E8 ?? ?? ?? ?? 48 85 C0 74 1B 48 8D 8B ?? ?? ?? ?? E8 ?? ?? ?? ?? 48 8B 8B ?? ?? ?? ?? 48 8B D0 E8 ?? ?? ?? ?? 48 8B CB 48 8B 7C 24";
        internal const string Utf8StringCtor = "E8 ?? ?? ?? ?? 44 2B F7";
        internal const string Utf8StringDtor = "80 79 21 00 75 12";
        internal const string AtkTextNodeSetText = "E8 ?? ?? ?? ?? 8D 4E 32";
        internal const string LoadExd = "40 53 56 57 48 81 EC ?? ?? ?? ?? 48 8B 05 ?? ?? ?? ?? 48 33 C4 48 89 84 24 ?? ?? ?? ?? 41 0F B6 D9";
        internal const string CharacterInitialise = "48 89 5C 24 ?? 48 89 6C 24 ?? 48 89 74 24 ?? 57 48 83 EC 30 48 8B F9 48 8B EA 48 81 C1 ?? ?? ?? ?? E8";
        internal const string CharacterIsMount = "40 53 48 83 EC 20 48 8B 01 48 8B D9 FF 50 10 83 F8 08 75 08";
        internal const string FlagSlotUpdate = "48 89 5C 24 ?? 48 89 74 24 ?? 57 48 83 EC 20 8B DA 49 8B F0 48 8B F9 83 FA 0A";
    }

    #region Delegates

    private delegate IntPtr GenerateNameDelegate(int race, int clan, int gender, IntPtr first, IntPtr last);

    private delegate IntPtr Utf8StringCtorDelegate(IntPtr memory);

    private delegate void Utf8StringDtorDelegate(IntPtr memory);

    private delegate void AtkTextNodeSetTextDelegate(IntPtr node, IntPtr text);

    private delegate byte LoadExdDelegate(IntPtr a1, string sheetName, byte a3, byte a4);

    private delegate IntPtr GetExcelModuleDelegate(IntPtr uiModule);

    private delegate IntPtr CharacterIsMountDelegate(IntPtr actor);

    private delegate char CharacterInitialiseDelegate(IntPtr actorPtr, IntPtr customizeDataPtr);

    private delegate IntPtr FlagSlotUpdateDelegate(IntPtr actorPtr, uint slot, IntPtr equipData);

    #endregion

    #region Functions

    [Signature(Signatures.Utf8StringCtor)]
    private Utf8StringCtorDelegate Utf8StringCtor { get; init; } = null!;

    [Signature(Signatures.Utf8StringDtor)]
    private Utf8StringDtorDelegate Utf8StringDtor { get; init; } = null!;

    [Signature(Signatures.GenerateName)]
    private GenerateNameDelegate InternalGenerateName { get; init; } = null!;

    [Signature(Signatures.LoadExd)]
    private LoadExdDelegate LoadExd { get; init; } = null!;

    #endregion

    #region Hooks

    [Signature(Signatures.AtkTextNodeSetText, DetourName = nameof(AtkTextNodeSetTextDetour))]
    private Hook<AtkTextNodeSetTextDelegate> AtkTextNodeSetTextHook { get; init; } = null!;

    [Signature(Signatures.CharacterIsMount, DetourName = nameof(CharacterIsMountDetour))]
    private Hook<CharacterIsMountDelegate> CharacterIsMountHook { get; init; } = null!;

    [Signature(Signatures.CharacterInitialise, DetourName = nameof(CharacterInitialiseDetour))]
    private Hook<CharacterInitialiseDelegate> CharacterInitializeHook { get; init; } = null!;

    [Signature(Signatures.FlagSlotUpdate, DetourName = nameof(FlagSlotUpdateDetour))]
    private Hook<FlagSlotUpdateDelegate> FlagSlotUpdateHook { get; init; } = null!;

    #endregion

    #region Events

    internal delegate void AtkTextNodeSetTextEventDelegate(IntPtr node, IntPtr text, ref SeString? overwrite);

    internal event AtkTextNodeSetTextEventDelegate? AtkTextNodeSetText;

    internal unsafe delegate void CharacterInitialiseEventDelegate(GameObject* gameObj, IntPtr humanPtr, IntPtr customiseDataPtr);

    internal event CharacterInitialiseEventDelegate? CharacterInitialise;

    internal unsafe delegate void FlagSlotUpdateEventDelegate(GameObject* gameObj, uint slot, EquipData* equipData);

    internal event FlagSlotUpdateEventDelegate? FlagSlotUpdate;

    #endregion

    private Plugin Plugin { get; }

    private IntPtr First { get; }
    private IntPtr Last { get; }

    private Dictionary<IntPtr, uint> HumansToIds { get; } = new();

    internal GameFunctions2(Plugin plugin) {
        this.Plugin = plugin;

        SignatureHelper.Initialise(this);

        this.AtkTextNodeSetTextHook.Enable();
        this.CharacterInitializeHook.Enable();
        this.CharacterIsMountHook.Enable();
        this.FlagSlotUpdateHook.Enable();

        this.First = Marshal.AllocHGlobal(128);
        this.Last = Marshal.AllocHGlobal(128);

        this.Utf8StringCtor(this.First);
        this.Utf8StringCtor(this.Last);

        Service.ClientState.TerritoryChanged += this.OnTerritoryChange;
    }

    public void Dispose() {
        Service.ClientState.TerritoryChanged -= this.OnTerritoryChange;
        this.Utf8StringDtor(this.Last);
        this.Utf8StringDtor(this.First);
        Marshal.FreeHGlobal(this.Last);
        Marshal.FreeHGlobal(this.First);
        this.AtkTextNodeSetTextHook.Dispose();
        this.CharacterInitializeHook.Dispose();
        this.CharacterIsMountHook.Dispose();
        this.FlagSlotUpdateHook.Dispose();
    }

    private void OnTerritoryChange(object? sender, ushort e) {
        this.HumansToIds.Clear();
    }

    private unsafe void AtkTextNodeSetTextDetour(IntPtr node, IntPtr text) {
        SeString? overwrite = null;
        this.AtkTextNodeSetText?.Invoke(node, text, ref overwrite);

        if (overwrite != null) {
            fixed (byte* newText = overwrite.Encode().Terminate()) {
                this.AtkTextNodeSetTextHook.Original(node, (IntPtr) newText);
            }

            return;
        }

        this.AtkTextNodeSetTextHook.Original(node, text);
    }

    private IntPtr _lastActor = IntPtr.Zero;

    private unsafe IntPtr CharacterIsMountDetour(IntPtr characterPtr) {
        var chara = (GameObject*) characterPtr;
        if (chara != null && chara->ObjectKind == (byte) ObjectKind.Pc) {
            this._lastActor = characterPtr;
        } else {
            this._lastActor = IntPtr.Zero;
        }

        return this.CharacterIsMountHook.Original(characterPtr);
    }

    private unsafe char CharacterInitialiseDetour(IntPtr actorPtr, IntPtr customizeDataPtr) {
        if (this._lastActor != IntPtr.Zero) {
            try {
                var id = ((GameObject*) this._lastActor)->ObjectID;

                // remove other humans that may have had this object id
                foreach (var (human, objId) in this.HumansToIds.ToList()) {
                    if (objId == id) {
                        this.HumansToIds.Remove(human);
                    }
                }

                this.HumansToIds[actorPtr] = id;
                this.CharacterInitialise?.Invoke((GameObject*) this._lastActor, actorPtr, customizeDataPtr);
                this._lastActor = IntPtr.Zero;
            } catch (Exception e) {
                PluginLog.LogError(e, "yeet");
            }
        }

        return this.CharacterInitializeHook.Original(actorPtr, customizeDataPtr);
    }

    private unsafe IntPtr FlagSlotUpdateDetour(IntPtr actorPtr, uint slot, IntPtr equipDataPtr) {
        if (this.HumansToIds.TryGetValue(actorPtr, out var objId)) {
            var obj = Service.ObjectTable.FirstOrDefault(obj => obj.ObjectId == objId);
            if (obj != null) {
                try {
                    this.FlagSlotUpdate?.Invoke((GameObject*) obj.Address, slot, (EquipData*) equipDataPtr);
                } catch (Exception e) {
                    PluginLog.LogError(e, "yeet3");
                }
            }
        }

        return this.FlagSlotUpdateHook.Original(actorPtr, slot, equipDataPtr);
    }

    public string? GenerateName(int race, int clan, int gender) {
        if (this.InternalGenerateName(race, clan, gender, this.First, this.Last) == IntPtr.Zero) {
            return null;
        }

        var first = Marshal.PtrToStringUTF8(Marshal.ReadIntPtr(this.First));
        var last = Marshal.PtrToStringUTF8(Marshal.ReadIntPtr(this.Last));

        if (string.IsNullOrEmpty(first) || string.IsNullOrEmpty(last)) {
            return null;
        }

        return $"{first} {last}";
    }

    public unsafe void LoadSheet(string name) {
        var ui = (IntPtr) Framework.Instance()->GetUiModule();
        var getExcelModulePtr = *(*(IntPtr**) ui + 5);
        var getExcelModule = Marshal.GetDelegateForFunctionPointer<GetExcelModuleDelegate>(getExcelModulePtr);
        var excelModule = getExcelModule(ui);
        var exdModule = *(IntPtr*) (excelModule + 8);
        var excel = *(IntPtr*) (exdModule + 0x20);

        this.LoadExd(excel, name, 0, 1);
    }
}
