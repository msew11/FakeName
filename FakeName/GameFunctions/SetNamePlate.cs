using System;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Hooking;
using Dalamud.Utility.Signatures;

namespace FakeName.GameFunctions;

internal class SetNamePlate : IDisposable
{
    private Plugin Plugin { get; }

    private static class Signatures
    {
        internal const string SetNamePlate =
            "48 89 5C 24 ?? 48 89 6C 24 ?? 56 57 41 54 41 56 41 57 48 83 EC 40 44 0F B6 E2";
    }

    // Hook
    private delegate void SetNamePlateDelegate(
        IntPtr addon, bool isPrefixTitle, bool displayTitle,
        IntPtr titlePtr, IntPtr namePtr, IntPtr fcNamePtr, int iconId
    );

    [Signature(Signatures.SetNamePlate, DetourName = nameof(SetNamePlateDetour))]
    private Hook<SetNamePlateDelegate> SetNamePlateHook { get; init; } = null!;

    // Event
    private delegate void SetNamePlateEventDelegate(
        IntPtr namePlateObjectPtr, bool isPrefixTitle, bool displayTitle,
        IntPtr titlePtr, IntPtr namePtr, IntPtr fcNamePtr, int iconId,
        ref SeString? title, ref SeString? name, ref SeString? fcName
    );

    private event SetNamePlateEventDelegate? OnSetNamePlate;

    // Constructor
    public SetNamePlate(Plugin plugin)
    {
        Plugin = plugin;

        SignatureHelper.Initialise(this);
        // SeStringUtils.Initialize();

        SetNamePlateHook.Enable();
        this.OnSetNamePlate += DealSetNamePlateEvent;
    }

    public void Dispose()
    {
        this.OnSetNamePlate -= DealSetNamePlateEvent;
        SetNamePlateHook.Disable();
        SetNamePlateHook.Dispose();
    }

    private unsafe void SetNamePlateDetour(
        IntPtr namePlateObjectPtr, bool isPrefixTitle, bool displayTitle,
        IntPtr titlePtr, IntPtr namePtr, IntPtr fcNamePtr, int iconId)
    {
        SeString? title = null;
        SeString? name = null;
        SeString? fcName = null;
        this.OnSetNamePlate?.Invoke(
            namePlateObjectPtr, isPrefixTitle, displayTitle,
            titlePtr, namePtr, fcNamePtr, iconId,
            ref title, ref name, ref fcName
        );

        if (title!= null && name != null && fcName != null)
        {
            fixed (byte* newTitle = title.Encode().Terminate(), newName = name.Encode().Terminate(), newFcName = fcName.Encode().Terminate())
            {
                this.SetNamePlateHook.Original(
                    namePlateObjectPtr, isPrefixTitle, displayTitle,
                    (IntPtr)newTitle, (IntPtr)newName, (IntPtr)newFcName, iconId
                );
            }
            return;
        }

        this.SetNamePlateHook.Original(
            namePlateObjectPtr, isPrefixTitle, displayTitle,
            titlePtr, namePtr, fcNamePtr, iconId
        );
    }

    private void DealSetNamePlateEvent(
        IntPtr namePlateObjectPtr, bool isPrefixTitle, bool displayTitle,
        IntPtr titlePtr, IntPtr namePtr, IntPtr fcNamePtr, int iconId,
        ref SeString? title, ref SeString? name, ref SeString? fcName
    )
    {
        if (!Plugin.Config.Enabled)
        {
            return;
        }

        var player = Service.ClientState.LocalPlayer;
        if (player == null)
        {
            return;
        }

        var titleText = Util.ReadRawSeString(titlePtr);
        var text = Util.ReadRawSeString(namePtr);
        var fcText = Util.ReadRawSeString(fcNamePtr);
        
        // 替换前的名字
        var textValue = text.TextValue;
        var localName = player.Name.TextValue;

        var change = Plugin.NameRepository.DealReplace(text);
        if (!change)
        {
            return;
        }
        
        if (textValue == localName)
        {
            // 替换fc
            var localFcName = player.CompanyTag.TextValue;

            if (!string.IsNullOrEmpty(localFcName))
            {
                var replaceFcName = Plugin.NameRepository.GetReplaceFcName();
                fcText.ReplacePlayerName(localFcName, replaceFcName);
            }
        }

        title = titleText;
        name = text;
        fcName = fcText;
    }
}
