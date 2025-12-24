using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.Types;
using static GaugeOMatic.GameData.FrameworkData;
using static GaugeOMatic.GameData.JobData;
using static GaugeOMatic.GameData.Overrides;
using static GaugeOMatic.GameData.Sheets;
using static GaugeOMatic.GameData.StatusRef.StatusActor;
using static GaugeOMatic.Trackers.Tracker;
using DalamudIStatus = Dalamud.Game.ClientState.Statuses.IStatus;
using StatusExcelRow = Lumina.Excel.Sheets.Status;

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
        Name = StatusAliases.TryGetValue(ID, out var alias) ? alias : ExcelRow != null ? ExcelRow.Value.Name.ToString() : "Unknown";

        var excelJob = GetJobByCategory(ExcelRow?.ClassJobCategory.RowId ?? 0);
        Job = excelJob != Job.None ? excelJob : job;
        Role = role;

        Icon = ExcelRow?.Icon ?? 66313;

        AppliedTo = appliedTo;
        AppliedBy = appliedBy;
        MaxStacks = StatusMaxes.TryGetValue(id, out var m) ? m :
                    ExcelRow != null ? Math.Max(1, (int)ExcelRow.Value.MaxStacks) : 1;

        MaxTime = maxtime;
        SeeAlso = seeAlso;

        if (MaxStacks > 1 && Icon != null) Icon = (uint?)(Icon.Value + (MaxStacks - 1));
    }

    public StatusRef() { }

    public static implicit operator StatusRef(uint i) => StatusData.TryGetValue(i, out var result) ? result : new();

    public static Func<DalamudIStatus, bool> StatusMatch(uint id, StatusActor appliedBy = Self) => s => PlayerCheck && s.StatusId == id && CheckSource(s, appliedBy);

    private static bool CheckSource(DalamudIStatus s, StatusActor appliedBy)
    {
        try
        {
            var sourceId = s.SourceId;
            var sourceOwnerId = s.SourceObject?.OwnerId ?? sourceId;

            switch (appliedBy)
            {
                case Any:
                    break;
                case Self:
                {
                    var playerId = LocalPlayer.ObjId;
                    return sourceId == playerId || sourceOwnerId == playerId;
                }
                case Party:
                {
                    foreach (var partyMember in PartyList)
                        if (sourceId == partyMember.EntityId || sourceOwnerId == partyMember.EntityId)
                            return true;
                    break;
                }
                case Target:
                {
                    var target = LocalPlayer.TargetObjId;
                    if (sourceId == target || sourceOwnerId == target) return true;
                    break;
                }
            }

            return true;
        }
        catch
        {
            return false;
        }
    }

    private static List<DalamudIStatus> PlayerStatusList =>
        ClientState.IsLoggedIn && ObjectTable.LocalPlayer != null ?
            ObjectTable.LocalPlayer.StatusList.ToList()
            : [];

    private static List<DalamudIStatus> TargetStatusList =>
        ClientState.IsLoggedIn && ObjectTable.LocalPlayer is { TargetObject.ObjectKind: ObjectKind.BattleNpc }
            ? ((IBattleNpc)ObjectTable.LocalPlayer.TargetObject).StatusList.ToList()
            : [];

    public bool TryGetStatus(out DalamudIStatus? result, StatusActor appliedTo, StatusActor appliedBy = Self)
    {
        result = null;

        var statusList = appliedTo switch
        {
            Self => PlayerStatusList,
            Target => TargetStatusList,
            _ => []
        };

        if (statusList is { Count: > 0 })
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

        return false;
    }

    public bool TryGetStatus(StatusActor appliedTo, StatusActor appliedBy = Self)
    {
        if (!PlayerCheck) return false;

        var statusList = appliedTo == Self ? PlayerStatusList : TargetStatusList;
        return statusList.Any(StatusMatch(ID, appliedBy)) || (SeeAlso != null && statusList.Any(s => SeeAlso.Any(s2 => s2 == s.StatusId)));
    }

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
        else
        {
            try
            {
                if (TryGetStatus(out var status, AppliedTo, AppliedBy))
                {
                    state = 1;
                    count = status is { Param: > 0 } ? status.Param : 1;
                    gaugeValue = MaxTime == 0 ? maxGauge : Math.Abs(status?.RemainingTime ?? 0f);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.StackTrace ?? "");
                return GetTrackerData(0f);
            }

        }

        uint? iconOverride = Icon != null && count > 0 && maxCount > 1 ? (uint)(Icon.Value - (maxCount - count)) : null;

        return new(count, maxCount, gaugeValue, maxGauge, state, 1, preview) { IconOverride = iconOverride };
    }

}
