using System;
using System.Linq;
using Lumina.Excel;
using Lumina.Excel.GeneratedSheets;

namespace FakeName.Component;

public enum ZoneType
{
    Overworld,
    Dungeon,
    Raid,
    AllianceRaid,
    Foray,
}

public class DutyComponent : IDisposable
{
    public bool InDuty => ZoneType != ZoneType.Overworld;

    private ExcelSheet<ContentFinderCondition> _contentFinderConditionsSheet;
    
    public ZoneType ZoneType { get; private set; } = ZoneType.Overworld;

    public DutyComponent()
    {
        _contentFinderConditionsSheet = Service.DataManager.GameData.GetExcelSheet<ContentFinderCondition>() ?? throw new InvalidOperationException();
        Service.ClientState.TerritoryChanged += OnTerritoryChanged;
    }

    public void Dispose()
    {
        Service.ClientState.TerritoryChanged -= OnTerritoryChanged;
    }

    private void OnTerritoryChanged(ushort e)
    {
        var content =
            _contentFinderConditionsSheet.FirstOrDefault(t => t.TerritoryType.Row == Service.ClientState.TerritoryType);

        if (content == null)
        {
            ZoneType = ZoneType.Overworld;
        }
        else
        {
            var memberType = content.ContentMemberType.Row;

            if (content.RowId == 16 || content.RowId == 15)
            {
                // Praetorium and Castrum Meridianum
                memberType = 2;
            }

            if (content.RowId == 735 || content.RowId == 778)
            {
                // Bozja
                memberType = 127;
            }
            
            switch (memberType)
            {
                case 2:
                    ZoneType = ZoneType.Dungeon;

                    break;

                case 3:
                    ZoneType = ZoneType.Raid;

                    break;

                case 4:
                    ZoneType = ZoneType.AllianceRaid;

                    break;

                case 127:
                    ZoneType = ZoneType.Foray;

                    break;

                default:
                    ZoneType = ZoneType.Dungeon;

                    break;
            }
        }
    }
}
