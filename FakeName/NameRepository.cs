using System;

namespace FakeName;

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
    
    internal string GetReplaceName()
    {
        return Plugin.Config.FakeNameText;
    }
    
    internal string GetReplaceFcName()
    {
        return Plugin.Config.FakeFcNameText;
    }
}
