using System;
using System.Collections.Generic;
using System.Linq;
using static GaugeOMatic.GameData.ActionFlags;
using static GaugeOMatic.GameData.JobData;
using static GaugeOMatic.GameData.Overrides;
using static GaugeOMatic.GameData.Sheets;
using static GaugeOMatic.GameData.StatusRef;
using static GaugeOMatic.GameData.StatusRef.StatusActor;
using ActionExcelRow = Lumina.Excel.GeneratedSheets.Action;

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
    public ushort? GetAdjustedIcon() => GetAdjustedAction().Icon;

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
        ExcelRow = ActionSheet?.GetRow(id);

        if (ExcelRow != null)
        {
            Name = ActionAliases.TryGetValue(ID, out var alias) ? alias : ExcelRow.Name;
            NameChain = Name;
            Icon = ExcelRow.Icon;

            var category = ExcelRow.ClassJobCategory.Row;
            Job = GetJobByCategory(category);
            Role = GetRoleByCategory(category);

            LastKnownCooldown = ExcelRow.Recast100ms / 10f;

            SetFlag(LongCooldown, ExcelRow.Recast100ms > 50);
            SetFlag(HasCharges, ExcelRow.MaxCharges > 0);
            SetFlag(ComboBonus, ExcelRow.Recast100ms > 50);
            SetFlag(RoleAction, Role != Role.None);
            SetFlag(ComboBonus, ExcelRow.ActionCombo.Row > 0);
            SetFlag(Unassignable, !ExcelRow.IsPlayerAction);
            SetFlag(CanGetAnts, AntActions.Contains(ID));
            SetFlag(CostsMP, ExcelRow.PrimaryCostType == 3 && ExcelRow.PrimaryCostValue > 0);

            CheckForUpgrades();
            CheckTransformations();
            CheckStatusEffects();

            if (ActionOverrideFuncs.ContainsKey(ID)) ActionOverrideFuncs[ID].Invoke(this);
        }
        else
        {
            Name = "Unknown";
            Job = Job.None;
            LastKnownCooldown = 2.5f;
            Icon = null;
        }

        void CheckTransformations()
        {
            if (ActionIndirectionSheet == null) return;

            foreach (var newId in from row in ActionIndirectionSheet where row.PreviousComboAction.Row == ID select row.Name.Row)
            {
                if (!ActionData.ContainsKey(newId)) ActionData.TryAdd(newId, new(newId));

                var newAction = ActionData[newId];
                newAction.SetFlag(TransformedButton, true);

                newAction.BaseActionId = GetBaseAction().ID;
            }
        }

        void CheckForUpgrades()
        {
            if (ActionUiSheet == null) return;

            foreach (var upgrade in ActionUiSheet)
            {
                var baseId = upgrade.Unknown1;
                var newId = upgrade.Unknown0;
                if (baseId == ID && newId != ID)
                {
                    var newActionRow = ActionSheet!.GetRow(newId);

                    Flags |= Upgrade;
                    NameChain += " / " + newActionRow!.Name;

                    if (!ActionData.ContainsKey(newId)) ActionData.TryAdd(newId, new(newId));

                    var newAction = ActionData[newId];
                    newAction.SetFlag(Upgrade, true);
                    newAction.HideFromDropdown = true;
                    newAction.BaseActionId = ID;
                }
            }
        }

        void CheckStatusEffects()
        {
            var aps = ExcelRow!.ActionProcStatus;
            if (aps.Row != 0 && aps.Value != null)
            {
                SetFlag(RequiresStatus, true);

                var procStatus = aps.Value.Status;
                var statusId = procStatus.Row;

                if (!StatusData.ContainsKey(statusId)) StatusData.TryAdd(statusId, new(statusId, Job, Self, Self) { HideFromDropdown = true });

                ReadyStatus = statusId;
            }
            else if (ExcelRow.PrimaryCostType == 32)
            {
                SetFlag(RequiresStatus, true);
                ReadyStatus = ExcelRow.PrimaryCostValue;
            }
            else if (ExcelRow.SecondaryCostType == 32)
            {
                SetFlag(RequiresStatus, true);
                ReadyStatus = ExcelRow.SecondaryCostValue;
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
