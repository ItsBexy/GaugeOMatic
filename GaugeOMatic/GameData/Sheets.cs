using System;
using Lumina.Excel;
using Lumina.Excel.GeneratedSheets2;
using static GaugeOMatic.GameData.JobData;
using static GaugeOMatic.GameData.JobData.Role;
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
}
