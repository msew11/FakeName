using System;
using Dalamud.Logging;

namespace XivCommon {
    internal static class Logger {
        private static string Format(string msg) {
            return $"[XIVCommon] {msg}";
        }

        internal static void Log(string msg) {
            PluginLog.Log(Format(msg));
        }

        internal static void LogWarning(string msg) {
            PluginLog.LogWarning(Format(msg));
        }

        internal static void LogError(string msg) {
            PluginLog.LogError(Format(msg));
        }

        internal static void LogError(Exception ex, string msg) {
            PluginLog.LogError(ex, Format(msg));
        }
    }
}
