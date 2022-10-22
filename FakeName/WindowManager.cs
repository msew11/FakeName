using System;
using Dalamud.Interface.Windowing;
using FakeName.Windows;

namespace FakeName;

internal class WindowManager : IDisposable
{
    internal readonly WindowSystem WindowSystem = new("FakeName");
    internal ConfigWindow ConfigWindow { get; }

    public WindowManager(Plugin plugin)
    {
        ConfigWindow = new ConfigWindow(plugin);
        WindowSystem.AddWindow(ConfigWindow);

        Service.Interface.UiBuilder.Draw += DrawUi;
        Service.Interface.UiBuilder.OpenConfigUi += ConfigWindow.Open;
    }

    public void Dispose()
    {
        Service.Interface.UiBuilder.Draw -= DrawUi;
        Service.Interface.UiBuilder.OpenConfigUi -= ConfigWindow.Open;
        
        WindowSystem.RemoveAllWindows();
        
        ConfigWindow.Dispose();
    }

    private void DrawUi()
    {
        WindowSystem.Draw();
    }
}
