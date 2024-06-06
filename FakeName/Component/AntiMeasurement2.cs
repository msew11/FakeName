using System;
using System.Linq;
using System.Reflection;
using Dalamud.Support;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using ECommons.DalamudServices;
using ECommons.Logging;

namespace FakeName.Component;

internal static class AntiMeasurement2
{
    public static bool _init = false;
    public static bool Init 
    {
        get => _init;
        set
        {
            if (_init || value == false) return;
            _init = value;

            var myMethod = typeof(AntiMeasurement2).GetMethods().FirstOrDefault(m => m.Name == nameof(SendMeasurement));
            
            var oldMethod = typeof(Troubleshooting).Assembly.GetTypes().FirstOrDefault(t => t.Name.Equals("EventTracking"))?.GetMethods().FirstOrDefault(m => m.Name == nameof(SendMeasurement));

            PluginLog.Debug($"AntiMeasurement {oldMethod != null}");
            ExchangeMethod(oldMethod, myMethod);
        }
    }

    private static bool ExchangeMethod(MethodInfo? targetMethod, MethodInfo? injectMethod)
    {
        if (targetMethod == null || injectMethod == null)
        {
            return false;
        }
        RuntimeHelpers.PrepareMethod(targetMethod.MethodHandle);
        RuntimeHelpers.PrepareMethod(injectMethod.MethodHandle);

        /*foreach (var b in targetMethod.GetMethodBody().GetILAsByteArray())
        {
            
            PluginLog.Debug(b.ToString("X2") + " ");
        }*/

        unsafe
        {
            PluginLog.Debug($"AntiMeasurement {IntPtr.Size}");
            if (IntPtr.Size == 4)
            {
                int* tar = (int*)targetMethod.MethodHandle.Value.ToPointer() + 2;
                int* inj = (int*)injectMethod.MethodHandle.Value.ToPointer() + 2;
                *tar = *inj;
            }
            else
            {
                ulong* tar = (ulong*)targetMethod.MethodHandle.Value.ToPointer() + 1;
                ulong* inj = (ulong*)injectMethod.MethodHandle.Value.ToPointer() + 1;
                PluginLog.Debug($"AntiMeasurement {tar->ToString()} {inj->ToString()}");
                // *tar = *inj;
            }
        }
        return true;
    }

    public static async Task SendMeasurement(ulong contentId, uint actorId, uint homeWorldId)
    {
        P.msg = "AntiMeasurement::SendMeasurement";
        PluginLog.Debug("AntiMeasurement::SendMeasurement");
        
        var chatGui = Svc.Chat;
        if (chatGui == null)
        {
            return;
        }
        
        chatGui.Print("AntiMeasurement::SendMeasurement");
    }

    public static void PrintWelcomeMessage()
    {
        
        var chatGui = Svc.Chat;
        if (chatGui == null)
        {
            return;
        }
        
        chatGui.Print("AntiMeasurement::PrintWelcomeMessage");
        
    }
}
