using GaugeOMatic.GameData;
using static GaugeOMatic.GameData.ParamRef.ParamTypes;
using static GaugeOMatic.GameData.StatusRef;

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
