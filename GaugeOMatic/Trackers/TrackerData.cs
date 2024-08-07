using GaugeOMatic.GameData;
using static GaugeOMatic.GameData.ParamRef.ParamTypes;

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

        public TrackerData(ActionRef a, float? preview = null) => this = a.GetTrackerData(preview);

        public TrackerData(StatusRef s, float? preview = null) => this = s.GetTrackerData(preview);

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

                        LabelOverride = Sheets.ActionSheet?.GetRow(ClientState.LocalPlayer.CastActionId)?.Name ?? " ";

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
