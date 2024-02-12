using GaugeOMatic.GameData;
using System;
using static GaugeOMatic.GameData.ActionData;
using static GaugeOMatic.GameData.ParamRef.ParamTypes;
using static GaugeOMatic.GameData.StatusData;

namespace GaugeOMatic.Trackers;

public abstract partial class Tracker
{
    public TrackerData CurrentData;
    public TrackerData PreviousData;

    public struct TrackerData
    {
        public int Count = 0;        // A counter value such as Charges or Stacks
        public int MaxCount;
        public float GaugeValue = 0; // Usually Time, but could also be job resources, HP/MP, etc
        public float MaxGauge;
        public int State = 0;        // Status Active / Action Ready / etc
        public int MaxState;

        public bool HasLabelOverride = false;
        public string? LabelOverride = null;

        public TrackerData(ActionRef a, float? preview = null)
        {
            MaxCount = a.GetMaxCharges();
            Count = preview != null ? (int)(preview * MaxCount) :
                        MaxCount > 1? a.GetCurrentCharges() :
                                      a.IsReady()?1:0;

            MaxGauge = a.GetCooldownTotal();
            GaugeValue = preview == null ? a.GetCooldownRemaining(MaxGauge) : (float)(preview * MaxGauge);

            MaxState = 1;
            State = preview == null? a.IsReady() ? 1 : 0 : (int)Math.Round(preview.Value);
        }

        public TrackerData(StatusRef s, float? preview = null)
        {
            MaxCount = s.MaxStacks;
            MaxGauge = s.MaxTime;
            MaxState = 1;

            if (preview != null)
            {
                State = preview > 0 ? 1 : 0;
                Count = (int)(preview * MaxCount);
                GaugeValue = (float)(preview * MaxGauge);
            }
            else if (s.TryGetStatus(out var status))
            {
                State = 1;
                Count = status is { StackCount: > 0 } ? status.StackCount : 1;
                GaugeValue = Math.Abs(status?.RemainingTime ?? 0f);
            }
        }

        public TrackerData(ParamRef p, float? preview = null)
        {
            Count = 0;
            MaxCount = 1;
            GaugeValue = 0;
            MaxGauge = 1;
            State = 0;
            MaxState = 1;

            if (ClientState.LocalPlayer != null)
            {
                if (p.ParamType == HP)
                {
                    MaxGauge = ClientState.LocalPlayer.MaxHp;
                    GaugeValue = preview != null ? preview.Value * MaxGauge : ClientState.LocalPlayer.CurrentHp;
                }
                else if (p.ParamType == MP)
                {
                    MaxGauge = ClientState.LocalPlayer.MaxMp;
                    GaugeValue = preview != null ? preview.Value * MaxGauge : ClientState.LocalPlayer.CurrentMp;
                }
                else if (p.ParamType == Castbar)
                {
                    HasLabelOverride = true;
                    if (ClientState.LocalPlayer.IsCasting)
                    {
                        MaxGauge = ClientState.LocalPlayer.TotalCastTime;
                        GaugeValue = ClientState.LocalPlayer.CurrentCastTime;

                        LabelOverride = GetActionSheet()?.GetRow(ClientState.LocalPlayer.CastActionId)?.Name ?? " ";

                    } else if (preview != null)
                    {
                        MaxGauge = 1;
                        GaugeValue = preview.Value;
                    }
                }
            }
        }

        public TrackerData(int count, int maxCount, float gaugeValue, float maxGauge, int state, int maxState, float? preview = null)
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
