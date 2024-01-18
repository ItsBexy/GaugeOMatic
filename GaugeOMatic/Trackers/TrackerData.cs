using FFXIVClientStructs.FFXIV.Client.Game;
using System;
using static GaugeOMatic.GameData.ActionData;
using static GaugeOMatic.GameData.StatusData;
using static GaugeOMatic.GameData.StatusData.StatusHolder;
using ActionManager = FFXIVClientStructs.FFXIV.Client.Game.ActionManager;

namespace GaugeOMatic.Trackers;

public abstract partial class Tracker
{
    public TrackerData CurrentData;
    public TrackerData PreviousData;

    public struct TrackerData
    {
        public int Count;        // A counter value such as Charges or Stacks
        public int MaxCount;
        public float GaugeValue; // Usually Time, but could also be job resources, HP/MP, etc
        public float MaxGauge;
        public int State;       // Status Active / Action Ready / etc
        public int MaxState;

        public unsafe TrackerData(ActionRef a, float? preview = null)
        {
            var actionManager = ActionManager.Instance();

            MaxCount = GetMaxCharges(a.ID);
            var cooldownLength = actionManager->GetRecastTime(ActionType.Action, a.ID);
            if (cooldownLength == 0 || Math.Abs(cooldownLength - a.CooldownLength) < 0.05f) MaxGauge = a.CooldownLength;
            else
            {
                Actions[a.ID] = new(a.ID, a.Job, a.Name, cooldownLength, a.MaxCharges, a.Role);
                MaxGauge = cooldownLength;
            }

            Count = preview == null ? GetCharges(a.ID) : (int)(preview * MaxCount);
            GaugeValue = preview == null ? GetCooldownTime(a.ID, MaxGauge) : (float)(preview * MaxGauge);
            State = Count > 0?1:0;
            MaxState = 1;
        }

        public TrackerData(StatusRef s, float? preview = null)
        {
            MaxCount = s.MaxStacks;
            MaxGauge = s.MaxTime;

            if (preview == null)
            {
                var statusList = s.StatusHolder == Self ? PlayerStatus : TargetStatus;

                var found = TryGetStatus(statusList, s.ID, out var status);
                if (!found && s.SeeAlso != null)
                {
                    foreach (var seeID in s.SeeAlso)
                    {
                        found = TryGetStatus(statusList, seeID, out status);
                        if (found) break;
                    }
                }

                if (found)
                {
                    State = 1;
                    Count = status is { StackCount: > 0 } ? status.StackCount : 1;
                    GaugeValue = Math.Abs(status?.RemainingTime ?? 0f);
                }
                else
                {
                    State = 0;
                    Count = 0;
                    GaugeValue = 0;
                }
                
            }
            else
            {
                State = preview > 0?1:0;
                Count = (int)(preview * MaxCount);
                GaugeValue = (float)(preview * MaxGauge);
            }
            MaxState = 1;
        }

        public TrackerData(
            int count, int maxCount, float gaugeValue, float maxGauge, int state, int maxState, float? preview = null)
        {
            Count = preview == null ? count : (int)(preview.Value * maxCount);
            MaxCount = maxCount;
            GaugeValue = preview == null ? gaugeValue : preview.Value * maxGauge;
            MaxGauge = maxGauge;
            State = preview == null ? state : (int)(preview.Value * maxState);
            MaxState = maxState;
        }
    }
}
