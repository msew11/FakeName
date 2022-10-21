using System;
using System.Runtime.InteropServices;
using Dalamud.Game;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Hooking;

namespace XivCommon.Functions {
    /// <summary>
    /// Class containing Talk events
    /// </summary>
    public class Talk : IDisposable {
        private static class Signatures {
            internal const string SetAtkValue = "E8 ?? ?? ?? ?? 41 03 ED";
            internal const string ShowMessageBox = "4C 8B DC 55 57 41 55 49 8D 6B 98";
        }

        // Updated: 5.5
        private const int TextOffset = 0;
        private const int NameOffset = 0x10;
        private const int StyleOffset = 0x38;

        private delegate void AddonTalkV45Delegate(IntPtr addon, IntPtr a2, IntPtr data);

        private Hook<AddonTalkV45Delegate>? AddonTalkV45Hook { get; }

        private delegate IntPtr SetAtkValueStringDelegate(IntPtr atkValue, IntPtr text);

        private SetAtkValueStringDelegate SetAtkValueString { get; } = null!;

        /// <summary>
        /// The delegate for Talk events.
        /// </summary>
        public delegate void TalkEventDelegate(ref SeString name, ref SeString text, ref TalkStyle style);

        /// <summary>
        /// <para>
        /// The event that is fired when NPCs talk.
        /// </para>
        /// <para>
        /// Requires the <see cref="Hooks.Talk"/> hook to be enabled.
        /// </para>
        /// </summary>
        public event TalkEventDelegate? OnTalk;

        internal Talk(SigScanner scanner, bool hooksEnabled) {
            if (scanner.TryScanText(Signatures.SetAtkValue, out var setAtkPtr, "Talk - set atk value")) {
                this.SetAtkValueString = Marshal.GetDelegateForFunctionPointer<SetAtkValueStringDelegate>(setAtkPtr);
            } else {
                return;
            }

            if (!hooksEnabled) {
                return;
            }

            if (scanner.TryScanText(Signatures.ShowMessageBox, out var showMessageBoxPtr, "Talk")) {
                this.AddonTalkV45Hook = new Hook<AddonTalkV45Delegate>(showMessageBoxPtr, this.AddonTalkV45Detour);
                this.AddonTalkV45Hook.Enable();
            }
        }

        /// <inheritdoc />
        public void Dispose() {
            this.AddonTalkV45Hook?.Dispose();
        }

        private void AddonTalkV45Detour(IntPtr addon, IntPtr a2, IntPtr data) {
            if (this.OnTalk == null) {
                goto Return;
            }

            try {
                this.AddonTalkV45DetourInner(data);
            } catch (Exception ex) {
                Logger.LogError(ex, "Exception in Talk detour");
            }

            Return:
            this.AddonTalkV45Hook!.Original(addon, a2, data);
        }

        private void AddonTalkV45DetourInner(IntPtr data) {
            var rawName = Util.ReadTerminated(Marshal.ReadIntPtr(data + NameOffset + 8));
            var rawText = Util.ReadTerminated(Marshal.ReadIntPtr(data + TextOffset + 8));
            var style = (TalkStyle) Marshal.ReadByte(data + StyleOffset);

            var name = SeString.Parse(rawName);
            var text = SeString.Parse(rawText);

            try {
                this.OnTalk?.Invoke(ref name, ref text, ref style);
            } catch (Exception ex) {
                Logger.LogError(ex, "Exception in Talk event");
            }

            var newName = name.Encode().Terminate();
            var newText = text.Encode().Terminate();

            Marshal.WriteByte(data + StyleOffset, (byte) style);

            unsafe {
                fixed (byte* namePtr = newName, textPtr = newText) {
                    this.SetAtkValueString(data + NameOffset, (IntPtr) namePtr);
                    this.SetAtkValueString(data + TextOffset, (IntPtr) textPtr);
                }
            }
        }
    }

    /// <summary>
    /// Talk window styles.
    /// </summary>
    public enum TalkStyle : byte {
        /// <summary>
        /// The normal style with a white background.
        /// </summary>
        Normal = 0,

        /// <summary>
        /// A style with lights on the top and bottom border.
        /// </summary>
        Lights = 2,

        /// <summary>
        /// A style used for when characters are shouting.
        /// </summary>
        Shout = 3,

        /// <summary>
        /// Like <see cref="Shout"/> but with flatter edges.
        /// </summary>
        FlatShout = 4,

        /// <summary>
        /// The style used when dragons (and some other NPCs) talk.
        /// </summary>
        Dragon = 5,

        /// <summary>
        /// The style used for Allagan machinery.
        /// </summary>
        Allagan = 6,

        /// <summary>
        /// The style used for system messages.
        /// </summary>
        System = 7,

        /// <summary>
        /// A mixture of the system message style and the dragon style.
        /// </summary>
        DragonSystem = 8,

        /// <summary>
        /// The system message style with a purple background.
        /// </summary>
        PurpleSystem = 9,
    }
}
