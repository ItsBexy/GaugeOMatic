using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.ClientState.Statuses;
using System;
using System.Collections.Generic;
using System.Linq;
using static GaugeOMatic.GameData.JobData;
using static GaugeOMatic.GameData.JobData.Role;
using static GaugeOMatic.GaugeOMatic.Service;

namespace GaugeOMatic.GameData;

public abstract partial class StatusData
{
    public class StatusRef : ItemRef
    {
        public StatusHolder StatusHolder;
        public float MaxTime;
        public int MaxStacks;
        public List<uint>? SeeAlso;

        public StatusRef(
            uint id, Job job, string name, StatusHolder statusHolder, int maxStacks = 1, float maxtime = 1,
            Role role = None, List<uint>? seeAlso = null)
        {
            Job = job;
            Role = role;
            ID = id;
            Name = name;

            StatusHolder = statusHolder;
            MaxStacks = maxStacks;
            MaxTime = maxtime;
            SeeAlso = seeAlso;
        }

        public StatusRef() { }

        public static implicit operator StatusRef(uint i) => Statuses.TryGetValue(i, out var result) ? result : new();
    }

    public enum StatusHolder { Self, Target }

    public static StatusList? PlayerStatus => ClientState.LocalPlayer?.StatusList;

    public static StatusList? TargetStatus =>
        ClientState.LocalPlayer?.TargetObject?.ObjectKind == ObjectKind.BattleNpc
            ? ((BattleNpc)ClientState.LocalPlayer.TargetObject).StatusList
            : null;
    
    public static bool TryGetStatus(StatusList? statusList, uint id, out Status? result)
    {
        if (statusList != null)
        {
            foreach (var status in statusList.Where(StatusMatch(id)))
            {
                result = status;
                return true;
            }
        }
        result = null;
        return false;
    }

    private static Func<Status, bool> StatusMatch(uint id) => s => s.StatusId == id && s.SourceId == ClientState.LocalPlayer?.ObjectId;
}
