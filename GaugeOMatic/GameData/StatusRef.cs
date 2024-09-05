using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.ClientState.Statuses;
using System;
using System.Collections.Generic;
using System.Linq;
using static Dalamud.Game.ClientState.Objects.Enums.ObjectKind;
using static GaugeOMatic.GameData.JobData;
using static GaugeOMatic.GameData.Overrides;
using static GaugeOMatic.GameData.Sheets;
using static GaugeOMatic.GameData.StatusRef.StatusActor;
using static GaugeOMatic.Trackers.Tracker;
using DalamudStatus = Dalamud.Game.ClientState.Statuses.Status;
using StatusExcelRow = Lumina.Excel.GeneratedSheets2.Status;

namespace GaugeOMatic.GameData;

public partial class StatusRef : ItemRef
{
    public enum StatusActor
    {
        Any = 0,
        Self = 1,
        Target = 2,
        Party = 3
    }

    public StatusActor AppliedTo; // todo: allow these to be overridden within a tracker, so that multiple trackers for the same status can coexist with different AppliedTo/AppliedBy values
    public StatusActor AppliedBy;
    public float MaxTime;
    public int MaxStacks;
    public List<uint>? SeeAlso;
    public StatusExcelRow? ExcelRow;

    public StatusRef Clone() =>
        new()
        {
            Job = Job,
            Role = Role,
            ID = ID,
            Name = Name,
            HideFromDropdown = HideFromDropdown,
            Icon = Icon,

            AppliedTo = AppliedTo,
            AppliedBy = AppliedBy,
            MaxTime = MaxTime,
            MaxStacks = MaxStacks,
            SeeAlso = SeeAlso,
            ExcelRow = ExcelRow
        };

    public StatusRef(uint id, Job job, StatusActor appliedTo, StatusActor appliedBy, float maxtime = 1, Role role = Role.None, List<uint>? seeAlso = null)
    {
        ID = id;
        ExcelRow = StatusSheet?.GetRow(ID);
        Name = StatusAliases.TryGetValue(ID, out var alias) ? alias : ExcelRow != null ? ExcelRow.Name : "Unknown";

        var excelJob = GetJobByCategory(ExcelRow?.ClassJobCategory.Row ?? 0);
        Job = excelJob != Job.None ? excelJob : job;
        Role = role;

        Icon = (ushort?)ExcelRow?.Icon;

        AppliedTo = appliedTo;
        AppliedBy = appliedBy;
        MaxStacks = StatusMaxes.TryGetValue(id, out var m) ? m :
                    ExcelRow != null ? Math.Max(1, (int)ExcelRow.MaxStacks) : 1;

        MaxTime = maxtime;
        SeeAlso = seeAlso;

        if (MaxStacks > 1 && Icon != null) Icon = (ushort?)(Icon.Value + (MaxStacks - 1));
    }

    public StatusRef() { }

    public static implicit operator StatusRef(uint i) => StatusData.TryGetValue(i, out var result) ? result : new();

    public static Func<DalamudStatus, bool> StatusMatch(uint id, StatusActor appliedBy = Self) => s => s.StatusId == id && CheckSource(s, appliedBy);

    private static bool CheckSource(DalamudStatus s, StatusActor appliedBy)
    {
        var sourceId = s.SourceId;
        var sourceOwnerId = s.SourceObject?.OwnerId ?? sourceId;

        switch (appliedBy)
        {
            case Any:
                return true;
            case Self:
            {
                var playerId = ClientState.LocalPlayer?.GameObjectId;
                return sourceId == playerId || sourceOwnerId == playerId;
            }
            case Party:
            {
                foreach (var partyMember in PartyList)
                    if (sourceId == partyMember.ObjectId || sourceOwnerId == partyMember.ObjectId)
                        return true;
                break;
            }
            case Target:
            {
                var target = ClientState.LocalPlayer?.TargetObject?.GameObjectId;
                if (sourceId == target || sourceOwnerId == target) return true;
                break;
            }
        }

        return true;
    }

    public bool TryGetStatus(out DalamudStatus? result, StatusActor appliedTo, StatusActor appliedBy = Self)
    {
        var statusList = appliedTo == Self ? PlayerStatus : TargetStatus;

        if (statusList != null)
        {
            foreach (var status in statusList.Where(StatusMatch(ID, appliedBy)))
            {
                result = status;
                return true;
            }

            if (SeeAlso != null)
            {
                foreach (var seeID in SeeAlso)
                {
                    foreach (var status in statusList.Where(StatusMatch(seeID, appliedBy)))
                    {
                        result = status;
                        return true;
                    }
                }
            }
        }

        result = null;
        return false;
    }

    public bool TryGetStatus(StatusActor appliedTo, StatusActor appliedBy = Self)
    {
        var statusList = appliedTo == Self ? PlayerStatus : TargetStatus;
        return statusList != null && (statusList.Any(StatusMatch(ID, appliedBy)) || (SeeAlso != null && statusList.Any(s => SeeAlso.Any(s2 => s2 == s.StatusId))));
    }

    public static StatusList? PlayerStatus => ClientState.LocalPlayer?.StatusList;
    public static StatusList? TargetStatus =>
        ClientState.LocalPlayer?.TargetObject?.ObjectKind == BattleNpc
            ? ((IBattleNpc)ClientState.LocalPlayer.TargetObject).StatusList
            : null;

    public override TrackerData GetTrackerData(float? preview)
    {
        var maxCount = MaxStacks;
        var maxGauge = Math.Max(0.0001f, MaxTime);

        var count = 0;
        var state = 0;
        var gaugeValue = 0f;
        if (preview != null)
        {
            state = preview > 0 ? 1 : 0;
            count = (int)(preview.Value * maxCount)!;
            gaugeValue = preview.Value * maxGauge;
        }
        else if (TryGetStatus(out var status, AppliedTo, AppliedBy))
        {
            state = 1;
            count = status is { StackCount: > 0 } ? status.StackCount : 1;
            gaugeValue = MaxTime == 0 ? maxGauge : Math.Abs(status?.RemainingTime ?? 0f);
        }

        return new(count, maxCount, gaugeValue, maxGauge, state, 1, preview);
    }

}
