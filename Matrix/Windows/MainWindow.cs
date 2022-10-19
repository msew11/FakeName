using System;
using System.Numerics;
using Dalamud.Interface.Windowing;
using ImGuiNET;

namespace Matrix.Windows;

public class MainWindow : Window, IDisposable
{
    // private TextureWrap GoatImage;
    private readonly Plugin plugin;

    public MainWindow(Plugin plugin) : base("Matrix", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
    {
        this.SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(375, 330),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };

        //this.GoatImage = goatImage;
        this.plugin = plugin;
    }

    public void Dispose()
    {
        // this.GoatImage.Dispose();
    }

    public override void Draw()
    {
        ImGui.Text("Matrix");

        if (ImGui.Button("Show Settings"))
        {
            this.plugin.DrawConfigUi();
        }

        ImGui.Spacing();

        ImGui.Text("Have a goat:");
        ImGui.Indent(55);
        //ImGui.Image(this.GoatImage.ImGuiHandle, new Vector2(this.GoatImage.Width, this.GoatImage.Height));
        ImGui.Unindent(55);
    }
}
