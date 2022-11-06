using System;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Hooking;
using Dalamud.Logging;
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

        var titleText = Util.ReadRawSeString(titlePtr);
        var text = Util.ReadRawSeString(namePtr);
        var fcText = Util.ReadRawSeString(fcNamePtr);

        var player = Service.ClientState.LocalPlayer;
        if (player == null)
        {
            return;
        }

        var localName = player.Name.TextValue;
        var localFcName = player.CompanyTag.TextValue;
        
        var titleTextValue = titleText.TextValue;
        var textValue = text.TextValue;
        var fcTextValue = fcText.TextValue;
        
        var replaceName = Plugin.NameRepository.GetReplaceName();
        var replaceFcName = Plugin.NameRepository.GetReplaceFcName();
        
        if (textValue != localName)
        {
            return;
        }

        // PluginLog.Debug($"SetNamePlate 替换 name:{textValue} fc:{fcTextValue}");
        // PluginLog.Debug($"SetNamePlate 替换 title:{titleTextValue}");

        if (textValue == localName)
        {
            // 替换name
            text.ReplacePlayerName(localName, replaceName);
            
            if (!string.IsNullOrEmpty(localFcName))
            {
                // 替换fc
                fcText.ReplacePlayerName(localFcName, replaceFcName);
            }
        }
        
        /*if (titleTextValue == localName)
        {
            titleText.ReplacePlayerName(localName, replaceName);
        }*/

        title = titleText;
        name = text;
        fcName = fcText;
        
        
        


        // 角色名
        /*var localPlayer = Service.ClientState.LocalPlayer;
        var localName = "";
        if (localPlayer != null)
        {
            localName = localPlayer.Name.TextValue;
        }
    
    
        if (localName == currentName.TextValue)
        {
            var fakeName = SeStringUtils.Text(Plugin.Config.FakeNameText);
            var fakeFcName = SeStringUtils.Text($"«{Plugin.Config.FakeFcNameText}»");
            var fakeNamePtr = SeStringUtils.SeStringToPtr(fakeName);
            var fakeFcNamePtr = SeStringUtils.SeStringToPtr(fakeFcName);
            return SetNamePlateHook.Original(namePlateObjectPtr, isPrefixTitle, displayTitle, title, fakeNamePtr,
                                 fakeFcNamePtr, iconId);
        }

        return SetNamePlateHook.Original(namePlateObjectPtr, isPrefixTitle, displayTitle, title, name, fcName,
                             iconId);*/
    }
}
