using Dalamud.Game.ClientState;
using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;

namespace Matrix;

internal class Service
{

    [PluginService]
    internal static DalamudPluginInterface Interface { get; private set; }
    
    [PluginService]
    internal static ClientState ClientState { get; private set; }

    [PluginService]
    internal static CommandManager CommandManager { get; private set; }
}
