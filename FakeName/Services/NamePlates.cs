using System;
using System.Linq;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Logging;
using XivCommon.Functions.NamePlates;

namespace FakeName.Services;

internal class NamePlates : IDisposable
{
    private Plugin Plugin { get; }

    internal NamePlates(Plugin plugin)
    {
        Plugin = plugin;

        // Plugin.Common.Functions.NamePlates.OnUpdate += OnNamePlateUpdate;
    }

    public unsafe void Dispose()
    {
        // Plugin.Common.Functions.NamePlates.OnUpdate -= OnNamePlateUpdate;
    }

    private void OnNamePlateUpdate(NamePlateUpdateEventArgs args)
    {
        if (!Plugin.Config.Enabled)
        {
            return;
        }

        // find the object this nameplate references
        var obj = Service.ObjectTable.FirstOrDefault(o => o.ObjectId == args.ObjectId);
        if (obj == null)
        {
            PluginLog.Debug($"NamePlates return2 {args.ObjectId.ToString()} {args.Name.TextValue}");
            return;
        }

        // handle owners
        if (obj.OwnerId != 0xE0000000)
        {
            if (Service.ObjectTable.FirstOrDefault(o => o.ObjectId == obj.OwnerId) is not { } owner)
            {
                PluginLog.Debug($"NamePlates return3");
                return;
            }

            obj = owner;
        }

        // only work for characters
        if (obj.ObjectKind != ObjectKind.Player || obj is not Character chara)
        {
            PluginLog.Debug($"NamePlates return4");
            return;
        }

        void Change(string name)
        {
            ChangeName(args.Name, name);
            ChangeName(args.Title, name);
        }
        
        
        PluginLog.Debug($"NamePlates change");

        var name = chara.Name.TextValue;
        var playerId = Service.ClientState.LocalPlayer?.ObjectId;

        if (chara.ObjectId == playerId)
        {
            Change(name);
        }
    }

    // note: 将text的文本改掉
    private void ChangeName(SeString text, string name)
    {
        var textValue = text.TextValue;
        PluginLog.Debug($"a text={textValue} name={name}");

        // note: 生成了一个替换名replacement
        var replacement = Plugin.NameRepository.GetReplaceName();

        // note: 将text的文本替换成replacement
        text.ReplacePlayerName(name, replacement);
    }
}
