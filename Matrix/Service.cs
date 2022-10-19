using Dalamud.Data;
using Dalamud.Game.ClientState;
using Dalamud.Game.Gui;
using Dalamud.IoC;
using Dalamud.Plugin;
using Matrix.Config;

namespace Matrix;

internal class Service
{
    internal static PluginAddressResolver Address { get; set; }

    internal static Configuration Config { get; set; }

    [PluginService]
    internal static DalamudPluginInterface Interface { get; private set; }
    
    [PluginService]
    internal static DataManager DataManager { get; private set; }
    
    [PluginService]
    internal static ClientState ClientState { get; private set; }
    
    [PluginService]
    internal static ChatGui ChatGui { get; private set; }
}
