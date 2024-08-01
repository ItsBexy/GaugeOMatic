using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.ClientState.Statuses;
using System;
using System.Collections.Generic;
using System.Linq;
using static Dalamud.Game.ClientState.Objects.Enums.ObjectKind;
using static GaugeOMatic.GameData.JobData;
using static GaugeOMatic.GameData.Overrides;
using static GaugeOMatic.GameData.Sheets;
using static GaugeOMatic.GameData.StatusRef.StatusHolderType;
using DalamudStatus = Dalamud.Game.ClientState.Statuses.Status;
using StatusExcelRow = Lumina.Excel.GeneratedSheets.Status;

namespace GaugeOMatic.GameData;

public partial class StatusRef : ItemRef
{
    public StatusHolderType StatusHolder;
    public float MaxTime;
    public int MaxStacks;
    public List<uint>? SeeAlso;
    public StatusExcelRow? ExcelRow;

    public StatusRef(uint id, Job job, StatusHolderType statusHolder, float maxtime = 1, Role role = Role.None, List<uint>? seeAlso = null)
    {
        ID = id;
        ExcelRow = StatusSheet?.GetRow(ID);
        Name = StatusAliases.TryGetValue(ID, out var alias) ? alias : ExcelRow != null ? ExcelRow.Name : "Unknown";

        var excelJob = GetJobByCategory(ExcelRow?.ClassJobCategory.Row ?? 0);
        Job = excelJob != Job.None ? excelJob : job;
        Role = role;

        Icon = (ushort?)ExcelRow?.Icon;

        StatusHolder = statusHolder;
        MaxStacks = StatusMaxes.TryGetValue(id,out var m) ? m :
                    ExcelRow != null ? Math.Max(1, (int)ExcelRow.MaxStacks) : 1;

        MaxTime = maxtime;
        SeeAlso = seeAlso;

        if (MaxStacks > 1 && Icon != null) Icon = (ushort?)(Icon.Value + (MaxStacks - 1));
    }

    public StatusRef() { }

    public static implicit operator StatusRef(uint i) => StatusData.TryGetValue(i, out var result) ? result : new();

    public static Func<DalamudStatus, bool> StatusMatch(uint id, ulong? playerId) =>
        s =>
        {
            if (s.StatusId != id) return false;
            return playerId == s.SourceId || playerId == s.SourceObject?.OwnerId;
        };

    public bool TryGetStatus(out DalamudStatus? result)
    {
        var statusList = StatusHolder == Self ? PlayerStatus : TargetStatus;

        if (statusList != null)
        {
            var playerId = ClientState.LocalPlayer?.GameObjectId;

            foreach (var status in statusList.Where(StatusMatch(ID, playerId)))
            {
                result = status;
                return true;
            }

            if (SeeAlso != null)
            {
                foreach (var seeID in SeeAlso)
                {
                    foreach (var status in statusList.Where(StatusMatch(seeID, playerId)))
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

    public bool TryGetStatus()
    {
        var statusList = StatusHolder == Self ? PlayerStatus : TargetStatus;
        return statusList != null && (statusList.Any(StatusMatch(ID, ClientState.LocalPlayer?.GameObjectId)) || (SeeAlso != null && statusList.Any(s => SeeAlso.Any(s2 => s2 == s.StatusId))));
    }

    public static StatusList? TargetStatus =>
        ClientState.LocalPlayer?.TargetObject?.ObjectKind == BattleNpc
            ? ((IBattleNpc)ClientState.LocalPlayer.TargetObject).StatusList
            : null;

    public enum StatusHolderType { Self, Target }


}
