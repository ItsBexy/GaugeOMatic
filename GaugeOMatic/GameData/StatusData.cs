using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.ClientState.Statuses;
using System;
using System.Collections.Generic;
using System.Linq;
using static GaugeOMatic.GameData.JobData;
using static GaugeOMatic.GameData.JobData.Role;

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

        public bool TryGetStatus(out Status? result)
        {
            var statusList = StatusHolder == StatusHolder.Self ? PlayerStatus : TargetStatus;

            if (statusList != null)
            {
                var playerId = ClientState.LocalPlayer?.ObjectId;

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
            var statusList = StatusHolder == StatusHolder.Self ? PlayerStatus : TargetStatus;
            return statusList != null && (statusList.Any(StatusMatch(ID, ClientState.LocalPlayer?.ObjectId)) || (SeeAlso != null && statusList.Any(s => SeeAlso.Any(s2 => s2 == s.StatusId))));
        }

    }

    public enum StatusHolder { Self, Target }

    public static StatusList? PlayerStatus => ClientState.LocalPlayer?.StatusList;

    public static StatusList? TargetStatus =>
        ClientState.LocalPlayer?.TargetObject?.ObjectKind == ObjectKind.BattleNpc
            ? ((BattleNpc)ClientState.LocalPlayer.TargetObject).StatusList
            : null;

    private static Func<Status, bool> StatusMatch(uint id, uint? playerId) =>
        s =>
        {
            if (s.StatusId != id) return false;
            return playerId == s.SourceId || playerId == s.SourceObject?.OwnerId;
        };
}
