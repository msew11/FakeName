using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Logging;
using System;
using System.Linq;
using System.Runtime.InteropServices;

namespace FakeName;

internal static class Replacer 
{
    public static IntPtr ChangeName(IntPtr seStringPtr)
    {
        if (seStringPtr == IntPtr.Zero) return seStringPtr;

        try
        {
            var str = GetSeStringFromPtr(seStringPtr);
            if (ChangeSeString(ref str))
            {
                return GetPtrFromSeString(str);
            }
            else
            {
                return seStringPtr;
            }
        }
        catch (Exception ex)
        {
            PluginLog.Error(ex, "Something wrong with change name!");
            return seStringPtr;
        }
    }

    public static IntPtr GetPtrFromSeString(SeString str)
    {
        var bytes = str.Encode();
        var pointer = Marshal.AllocHGlobal(bytes.Length + 1);
        try
        {
            Marshal.Copy(bytes, 0, pointer, bytes.Length);
            Marshal.WriteByte(pointer, bytes.Length, 0);

            return pointer;
        }
        finally
        {
            Marshal.FreeHGlobal(pointer);
        }
    }

    public static SeString GetSeStringFromPtr(IntPtr seStringPtr)
    {
        var offset = 0;
        unsafe
        {
            while (*(byte*)(seStringPtr + offset) != 0)
                offset++;
        }
        var bytes = new byte[offset];
        Marshal.Copy(seStringPtr, bytes, 0, offset);
        return SeString.Parse(bytes);
    }

    public static bool ChangeSeString(ref SeString seString)
    {
        try
        {
            if (seString.Payloads.All(payload => payload.Type != PayloadType.RawText)) return false;

            var player = Service.ClientState.LocalPlayer;
            if (player == null) return false;

            var result = ReplacePlayerName(seString, GetNames(player.Name.TextValue), Service.Config.FakeNameText);

            if (Service.Config.AllPlayerReplace)
            {
                foreach (var obj in Service.ObjectTable)
                {
                    if (obj is not PlayerCharacter member) continue;
                    var memberName = member.Name.TextValue;
                    if (memberName == player.Name.TextValue) continue;

                    var jobData = member.ClassJob.GameData;
                    if (jobData == null) continue;

                    var nickName = ChangeName(memberName);

                    result = ReplacePlayerName(seString, memberName, nickName) || result;
                }
            }

            return result;
        }
        catch(Exception ex)
        {
            PluginLog.Error(ex, "Something wrong with replacement!");
            return false;
        }
    }

    public static string ChangeName(string str)
    {
        if(string.IsNullOrEmpty(str)) return str;
        return string.Join(" . ", str.Split(' ').Select(s => s.ToUpper().FirstOrDefault()));
    }

    private static string[] GetNames(string name)
    {
        var names = name.Split(' ');
        if (names.Length != 2) return new string[] { name };

        var first = names[0];

        return new string[]
        {
            name, 
            first,
        };
    }

    private static bool ReplacePlayerName(this SeString text, string[] names, string replacement)
    {
        foreach (var name in names)
        {
            if(ReplacePlayerName(text, name, replacement))
            {
                return true;
            }
        }
        return false;
    }

    private static bool ReplacePlayerName(this SeString text, string name, string replacement)
    {
        if (string.IsNullOrEmpty(name)) return false;

        var result = false;
        foreach (var payLoad in text.Payloads)
        {
            if (payLoad is TextPayload load)
            {
                if (string.IsNullOrEmpty(load.Text)) continue;

                var t = load.Text.Replace(name, replacement);
                if (t == load.Text) continue;
                load.Text = t;
                result = true;
            }
        }
        return result;
    }
}
