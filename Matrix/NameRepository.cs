using System;

namespace Matrix;

internal class NameRepository : IDisposable
{
    private Plugin Plugin { get; }

    internal bool Initialised;

    internal NameRepository(Plugin plugin)
    {
        this.Plugin = plugin;
        Initialised = true;
    }

    public void Dispose() { }
    
    internal string GetReplacement()
    {
        return Plugin.Config.FakeNameText;
    }
}
