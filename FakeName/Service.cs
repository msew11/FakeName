using Dalamud.Game;
using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;

namespace FakeName;

internal class Service
{
    internal static Configuration Config { get; set; }

    [PluginService]
    internal static DalamudPluginInterface Interface { get; private set; }

    [PluginService]
    internal static ClientState ClientState { get; private set; }

    [PluginService]
    internal static CommandManager CommandManager { get; private set; }

    [PluginService]
    internal static ObjectTable ObjectTable { get; private set; }
    [PluginService]
    public static Framework Framework  { get; private set; }

}
