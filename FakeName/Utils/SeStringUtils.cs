using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;

namespace FakeName.Utils;

internal static class SeStringUtils {
    internal static void ReplaceSeStringText(this SeString seString, string text, string replacement) {
        if (string.IsNullOrEmpty(text)) {
            return;
        }

        foreach (var payload in seString.Payloads) {
            switch (payload) {
                // case PlayerPayload pp:
                //     if (pp.PlayerName.Contains(name)) {
                //         pp.PlayerName = pp.PlayerName.Replace(name, replacement);
                //     }
                //
                //     break;
                case TextPayload txt:
                    txt.Text = txt.Text.Replace(text, replacement);

                    break;
            }
        }
    }

    internal static byte[] Terminate(this byte[] bs) {
        var terminated = new byte[bs.Length + 1];
        Array.Copy(bs, terminated, bs.Length);
        terminated[^1] = 0;
        return terminated;
    }

    internal static SeString ReadRawSeString(IntPtr ptr) {
        var bytes = ReadRawBytes(ptr);
        return SeString.Parse(bytes);
    }

    private static unsafe byte[] ReadRawBytes(IntPtr ptr) {
        if (ptr == IntPtr.Zero) {
            return Array.Empty<byte>();
        }

        var bytes = new List<byte>();

        var bytePtr = (byte*) ptr;
        while (*bytePtr != 0) {
            bytes.Add(*bytePtr);
            bytePtr += 1;
        }

        return bytes.ToArray();
    }
    
    public static unsafe SeString ReadSeString(byte* ptr)
    {
        var offset = 0;
        while (true)
        {
            var b = *(ptr + offset);
            if (b == 0)
            {
                break;
            }

            offset += 1;
        }

        var bytes = new byte[offset];
        Marshal.Copy(new nint(ptr), bytes, 0, offset);
        return SeString.Parse(bytes);
    }
}
