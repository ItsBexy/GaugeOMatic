using System;
using System.Collections.Generic;
using System.Linq;
using Lumina.Excel;
using static GaugeOMatic.GameData.JobData;
using static GaugeOMatic.GameData.JobData.Role;
using static GaugeOMatic.GameData.StatusRef.StatusActor;
using static GaugeOMatic.Windows.Dropdowns.TrackerDropdown;
using Lumina.Excel.Sheets;
using Action = Lumina.Excel.Sheets.Action;
using ActionIndirection = Lumina.Excel.Sheets.ActionIndirection;
using Status = Lumina.Excel.Sheets.Status;
using ActionProcStatus = Lumina.Excel.Sheets.ActionProcStatus;

namespace GaugeOMatic.GameData;

internal static class Sheets
{
    public static ExcelSheet<Action>? ActionSheet { get; } = DataManager.Excel.GetSheet<Action>(null,"Action");
    public static ExcelSheet<Status>? StatusSheet { get; } = DataManager.Excel.GetSheet<Status>(null,"Status");
    public static ExcelSheet<ActionProcStatus>? ApsSheet { get; } = DataManager.Excel.GetSheet<ActionProcStatus>(null,"ActionProcStatus");
    public static SubrowExcelSheet<ClassJobActionUI>? ActionUiSheet { get; } = DataManager.Excel.GetSubrowSheet<ClassJobActionUI>(null,"ClassJobActionUI");
    public static ExcelSheet<ActionIndirection>? ActionIndirectionSheet { get; } = DataManager.Excel.GetSheet<ActionIndirection>(null,"ActionIndirection");

    public static readonly List<uint> WhiteList = [3]; // Sprint sure is lonely

    public static Func<Action, bool> ActionFilter = static a => WhiteList.Contains(a.RowId) ||
                                                                (!a.IsPvP && (a.IsPlayerAction || a.ActionProcStatus.RowId > 0) &&
                                                                 (GetJobByCategory(a.ClassJobCategory.RowId) != Job.None || GetRoleByCategory(a.ClassJobCategory.RowId) != None));


    public static List<MenuOption> AllStatuses { get; } =
    [
        ..StatusSheet.Select(static r => (MenuOption)new StatusRef(r.RowId, Job.None, Self, Self, 1, All))
    ];

}
