using System;
using System.Collections.Generic;
using System.Linq;
using static GaugeOMatic.GameData.ActionFlags;
using static GaugeOMatic.GameData.JobData;
using static GaugeOMatic.GameData.Overrides;
using static GaugeOMatic.GameData.Sheets;
using static GaugeOMatic.GameData.StatusRef;
using static GaugeOMatic.GameData.StatusRef.StatusActor;
using ActionExcelRow = Lumina.Excel.Sheets.Action;

namespace GaugeOMatic.GameData;

public partial class ActionRef : ItemRef
{
    public ActionExcelRow? ExcelRow;
    public ActionFlags Flags;

    public StatusRef? ReadyStatus;
    public float LastKnownCooldown;

    public unsafe uint GetAdjustedId() => ActionManager->GetAdjustedActionId(ID);
    public ActionRef GetAdjustedAction() => GetAdjustedId();
    public string GetAdjustedName() => !(Job == Current && HasFlag(Upgrade)) ? NameChain : GetAdjustedAction().Name;
    public uint? GetAdjustedIcon() => GetAdjustedAction().Icon;

    public string NameChain = "";

    public uint? BaseActionId;
    public ActionRef GetBaseAction()
    {
        var baseId = BaseActionId;
        while (baseId != null && ActionData[(uint)baseId].BaseActionId != null) baseId = ActionData[(uint)baseId].BaseActionId;
        return baseId ?? this;
    }

    public static implicit operator ActionRef(uint i) => ActionData.TryGetValue(i, out var result) ? result : new(i);

    public ActionRef(uint id)
    {
        ID = id;
        Flags = None;
        ExcelRow = ActionSheet?.GetRowOrDefault(id);

        if (ExcelRow == null)
        {
            Name = "Unknown";
            Job = Job.None;
            LastKnownCooldown = 2.5f;
            Icon = null;
        }
        else
        {
            var excelRow = ExcelRow.Value;
            Name = ActionAliases.TryGetValue(ID, out var alias) ? alias : excelRow.Name.ToString();
            NameChain = Name;
            Icon = excelRow.Icon;

            var category = excelRow.ClassJobCategory.RowId;
            Job = GetJobByCategory(category);
            Role = GetRoleByCategory(category);

            LastKnownCooldown = excelRow.Recast100ms / 10f;

            SetFlag(LongCooldown, excelRow.Recast100ms > 50);
            SetFlag(HasCharges, excelRow.MaxCharges > 0);
            SetFlag(ComboBonus, excelRow.Recast100ms > 50);
            SetFlag(RoleAction, Role != Role.None);
            SetFlag(ComboBonus, excelRow.ActionCombo.RowId > 0);
            SetFlag(Unassignable, !excelRow.IsPlayerAction);
            SetFlag(CanGetAnts, AntActions.Contains(ID));
            SetFlag(CostsMP, excelRow is { PrimaryCostType: 3, PrimaryCostValue: > 0 });

            CheckForUpgrades();
            CheckTransformations();
            CheckStatusEffects(excelRow);

            if (ActionOverrideFuncs.TryGetValue(ID, out var func)) func.Invoke(this);
        }

        return;

        void CheckTransformations()
        {
            if (ActionIndirectionSheet == null) return;

            foreach (var newId in from row in ActionIndirectionSheet where row.PreviousComboAction.RowId == ID select row.Name.RowId)
            {
                if (!ActionData.ContainsKey(newId)) ActionData.TryAdd(newId, new ActionRef(newId));

                var newAction = ActionData[newId];
                newAction.SetFlag(TransformedButton, true);

                newAction.BaseActionId = GetBaseAction().ID;
            }
        }

        void CheckForUpgrades()
        {
            if (ActionUiSheet == null) return;

            foreach (var upgrade in ActionUiSheet.Flatten())
            {
                var baseId = upgrade.BaseAction.RowId;
                var newId = upgrade.UpgradeAction.RowId;
                if (baseId == ID && newId != ID)
                {
                    var newActionRow = ActionSheet!.GetRowOrDefault(newId);

                    Flags |= Upgrade;
                    NameChain += " / " + newActionRow?.Name.ToString();

                    if (!ActionData.ContainsKey(newId)) ActionData.TryAdd(newId, new ActionRef(newId));

                    var newAction = ActionData[newId];
                    newAction.SetFlag(Upgrade, true);
                    if (HasFlag(CanGetAnts)) newAction.SetFlag(CanGetAnts, true);
                    newAction.HideFromDropdown = true;
                    newAction.BaseActionId = ID;
                }
            }
        }

        void CheckStatusEffects(ActionExcelRow excelRow)
        {
            var aps = excelRow.ActionProcStatus;
            if (aps.RowId != 0)
            {
                SetFlag(RequiresStatus, true);

                var procStatus = aps.Value.Status;
                var statusId = procStatus.RowId;

                if (!StatusData.ContainsKey(statusId)) StatusData.TryAdd(statusId, new StatusRef(statusId, Job, Self, Self) { HideFromDropdown = true });

                ReadyStatus = statusId;
            }
            else if (excelRow.PrimaryCostType == 32)
            {
                SetFlag(RequiresStatus, true);
                ReadyStatus = excelRow.PrimaryCostValue;
            }
            else if (excelRow.SecondaryCostType == 32)
            {
                SetFlag(RequiresStatus, true);
                ReadyStatus = excelRow.SecondaryCostValue.RowId;
            }
        }
    }

    public void SetFlag(ActionFlags flags, bool val)
    {
        if (val) Flags |= flags;
        else Flags &= ~flags;
    }

    public bool HasFlag(ActionFlags flag, ActionFlags exclude = None) => (flag == 0 || (Flags & flag) != 0) && (exclude & Flags) == 0;

    internal static Dictionary<uint, ActionRef> ActionData = new();

    public static void PopulateActions()
    {
        if (ActionSheet != null)
            foreach (var a in ActionSheet.Where(ActionFilter))
                ActionData.TryAdd(a.RowId, new(a.RowId));

        HideUntrackableActions();
    }

    private static void HideUntrackableActions() //todo: put together a list of actions that are marked as untrackable and see which ones we can handle somehow
    {
        foreach (var a in ActionData.Where(static a => HiddenActions.Contains(a.Key) || !a.Value.HasFlag(Trackable))) a.Value.HideFromDropdown = true;
    }
}

[Flags]
public enum ActionFlags
{
    None = 0,
    Upgrade = 0x1,
    HasCharges = 0x2,
    LongCooldown = 0x4,
    RequiresStatus = 0x8,
    ComboBonus = 0x10,
    TransformedButton = 0x20,
    Unassignable = 0x40,
    CanGetAnts = 0x80,
    CostsMP = 0x100,
    RoleAction = 0x200,
    Trackable = 0x1FE // has at least one condition that makes its ready state trackable
}
