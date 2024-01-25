using Lumina.Excel;
using System;
using System.Linq;
using System.Runtime.InteropServices;
using static GaugeOMatic.GameData.JobData;
using static GaugeOMatic.GameData.JobData.Role;
using Action = Lumina.Excel.GeneratedSheets.Action;
// ReSharper disable UnusedMember.Global

namespace GaugeOMatic.GameData;

// methods for retrieving action data from sheets.
// choosing for now to hardcode the dictionary instead of building it on load, since some values need adjustment
// (eg, max charge count becomes wrong when a trait upgrades it)

public partial class ActionData
{
    // only looking for non-PvP player actions that have a longer recast than a GCD, and/or have charges
    private static Func<Action, bool> Filter() => static a => (IsRoleAction(a) || IsJobAction(a)) && !a.IsPvP && !(a.Recast100ms <= 25 && a.MaxCharges <= 0);

    private static bool IsRoleAction(Action a) => ParseRole(a) != None;

    private static bool IsJobAction(Action a) => a.ClassJob.Value != null && Enum.GetNames<Job>().Contains(a.ClassJob!.Value.Abbreviation);

    private static Job ParseJob(Action a) => Enum.Parse<Job>(a.ClassJob.Value!.Abbreviation);

    private static Role ParseRole(Action a) =>
        a.ClassJobCategory.Row switch
        {
            113 => Tank,
            114 => Melee,
            115 => Ranged,
            116 => Caster,
            117 => Healer,
            118 => Melee | Ranged,
            120 => Healer | Caster,
            161 => Melee | Tank | Ranged,
            _ => None
        };

    public static ExcelSheet<Action>? GetActionSheet() => DataManager.Excel.GetSheet<Action>("Action");
    
    public static void PopulateActions()
    {
        var actionSheet = GetActionSheet();
        if (actionSheet == null) return;

        foreach (var a in actionSheet.Where(Filter()))
        {
            var role = ParseRole(a);
            var job = role == None ? ParseJob(a) : Job.None;

            Actions.TryAdd(a.RowId, new(a.RowId, job, a.Name, a.Recast100ms * 100, GetMaxChargesAtLevel?.Invoke(a.RowId, 90u) ?? 1));
        }
    }

    // useful hook because the MaxCharges in the table don't account for increases via traits
    public delegate ushort GetMaxChargesAtLevelDelegate(uint id, uint level);
    internal static GetMaxChargesAtLevelDelegate? GetMaxChargesAtLevel;
    
    public static void SetupHooks()
    {
        var getMaxChargesPtr = SigScanner.ScanText("E8 ?? ?? ?? ?? 33 DB 8B C8");
        if (getMaxChargesPtr != IntPtr.Zero) GetMaxChargesAtLevel = Marshal.GetDelegateForFunctionPointer<GetMaxChargesAtLevelDelegate>(getMaxChargesPtr);
    }
}

