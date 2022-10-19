using System;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using Matrix.Config;

namespace Matrix.Windows;

public class ConfigWindow : Window, IDisposable
{

    public ConfigWindow(Plugin plugin) : base("Config")
    {
        this.SizeCondition = ImGuiCond.Always;
    }

    public void Dispose() { }

    public override void Draw()
    {
        

        var fakeName = Service.Config.FakeName;
        if (ImGui.InputText($"角色名", ref fakeName, 18))
        {
            Service.Config.FakeName = fakeName;
            Service.Config.Save();
        }


        // can't ref a property, so use a local copy
        //var configValue = this.configuration.SomePropertyToBeSavedAndWithADefault;
        // if (ImGui.Checkbox("Random Config Bool", ref configValue))
        // {
        //     this.configuration.SomePropertyToBeSavedAndWithADefault = configValue;
        //     // can save immediately on change, if you don't want to provide a "Save and Close" button
        //     this.configuration.Save();
        // }
    }
}
