using ECommons.DalamudServices;
using ImGuiNET;

namespace FakeName.Gui;

public static class TabSettings
{
    public static void Draw()
    {
        if (ImGui.Checkbox("启用", ref C.Enabled))
        {
            var localPlayer = Svc.ClientState.LocalPlayer;
            if (localPlayer!= null)
            {
                if (C.Enabled && C.TryGetCharacterConfig(localPlayer.Name.TextValue, localPlayer.HomeWorld.Id, out var characterConfig))
                {
                    P.IpcProcessor.ChangedLocalCharacterData(characterConfig);
                }
                else
                {
                    P.IpcProcessor.ChangedLocalCharacterData(null);
                }
            }
        }


        ImGui.Checkbox("隐藏发电按钮", ref C.HideSupport);
    }
}
