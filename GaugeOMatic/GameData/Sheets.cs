using System;
using System.Collections.Generic;
using System.Linq;
using Lumina.Excel;
using Lumina.Excel.GeneratedSheets2;
using static GaugeOMatic.GameData.JobData;
using static GaugeOMatic.GameData.JobData.Role;
using static GaugeOMatic.GameData.StatusRef.StatusActor;
using static GaugeOMatic.Windows.Dropdowns.TrackerDropdown;
using Action = Lumina.Excel.GeneratedSheets.Action;

namespace GaugeOMatic.GameData;

internal static class Sheets
{
    public static ExcelSheet<Action>? ActionSheet { get; } = DataManager.Excel.GetSheet<Action>("Action");
    public static ExcelSheet<Status>? StatusSheet { get; } = DataManager.Excel.GetSheet<Status>("Status");
    public static ExcelSheet<ClassJobActionUI>? ActionUiSheet { get; } = DataManager.Excel.GetSheet<ClassJobActionUI>("ClassJobActionUI");
    public static ExcelSheet<ActionIndirection>? ActionIndirectionSheet { get; } = DataManager.Excel.GetSheet<ActionIndirection>("ActionIndirection");

    public static Func<Action, bool> ActionFilter = static a => (a.IsPlayerAction || a.ActionProcStatus.Row > 0) && !a.IsPvP && //todo: make sure not to filter out sprint
                                                                (GetJobByCategory(a.ClassJobCategory.Row) != Job.None ||
                                                                 GetRoleByCategory(a.ClassJobCategory.Row) != None);

    public static List<MenuOption> AllStatuses { get; } = new(StatusSheet?.Select(static r => (MenuOption)new StatusRef(r.RowId, Job.None, Self, Self, 1, All)) ?? new List<MenuOption>());

}
