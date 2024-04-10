using System;
using Dalamud.Interface.Windowing;
using FakeName.Config;

namespace FakeName.Windows;

internal class WindowManager : IDisposable
{
    internal readonly WindowSystem WindowSystem = new("FakeName");
    internal ConfigWindow ConfigWindow { get; }

    public WindowManager(Plugin plugin, PluginConfig config)
    {
        ConfigWindow = new ConfigWindow(plugin, config);
        WindowSystem.AddWindow(ConfigWindow);

        Service.Interface.UiBuilder.Draw += DrawUi;
        Service.Interface.UiBuilder.OpenConfigUi += ConfigWindow.Open;
        Service.Interface.UiBuilder.OpenMainUi += ConfigWindow.Open;
    }

    public void Dispose()
    {
        Service.Interface.UiBuilder.Draw -= DrawUi;
        Service.Interface.UiBuilder.OpenConfigUi -= ConfigWindow.Open;
        Service.Interface.UiBuilder.OpenMainUi -= ConfigWindow.Open;
        
        WindowSystem.RemoveAllWindows();
    }

    private void DrawUi()
    {
        WindowSystem.Draw();
    }
}
