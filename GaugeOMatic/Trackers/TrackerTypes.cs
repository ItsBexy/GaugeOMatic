using FFXIVClientStructs.FFXIV.Client.UI;
using GaugeOMatic.GameData;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using static GaugeOMatic.GameData.JobData;
using static GaugeOMatic.Trackers.RefType;

namespace GaugeOMatic.Trackers;

public abstract partial class Tracker
{
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public virtual string TermCount => "Count";
    public virtual string TermGauge => "Timer";
    public virtual string[] StateNames => new[] { "Inactive", "Active" };

    public virtual TrackerDisplayAttribute DisplayAttr => (TrackerDisplayAttribute?)GetType().GetCustomAttributes(typeof(TrackerDisplayAttribute), true).FirstOrDefault() ?? new();

    public abstract RefType RefType { get; }
    public abstract TrackerData GetCurrentData(float? preview = null);
}

public sealed class StatusTracker : Tracker
{
    public override RefType RefType => Status;
    public override TrackerDisplayAttribute DisplayAttr => ItemRef!;

    public override string TermCount => "Stacks";
    public override string TermGauge => "Timer";
    public override string[] StateNames { get; } = { "Inactive", "Active" };

    public override TrackerData GetCurrentData(float? preview = null) => (ItemRef!).GetTrackerData(preview,TrackerConfig);
}

public sealed class ActionTracker : Tracker
{
    public override RefType RefType => Action;
    public override TrackerDisplayAttribute DisplayAttr => new((ActionRef)ItemRef!);

    public override string TermCount => "Charges";
    public override string TermGauge => "Timer";
    public override string[] StateNames { get; } = { "Unavailable", "Available" };

    public override TrackerData GetCurrentData(float? preview = null) => (ItemRef!).GetTrackerData(preview);
}

public abstract unsafe class JobGaugeTracker<T> : Tracker where T : unmanaged
{
    public override RefType RefType => JobGauge;

    public override string TermCount => "Count";
    public override string TermGauge => "Gauge";

    public override string[] StateNames { get; } = { "Inactive", "Active" };

    public abstract string GaugeAddonName { get; }

    public AddonJobHud* GaugeAddon => (AddonJobHud*)GameGui.GetAddonByName(GaugeAddonName);
    public T* GaugeData => (T*)GaugeAddon->DataCurrentPointer;
}

public class ParameterTracker : Tracker
{
    public override RefType RefType => Parameter;
    public override TrackerDisplayAttribute DisplayAttr => ParamRef.Attrs[((ParamRef)ItemRef!).ParamType];
    public override TrackerData GetCurrentData(float? preview = null) => ItemRef!.GetTrackerData(preview);
}

public class EmptyTracker : Tracker
{
    public override RefType RefType => None;
    public override TrackerDisplayAttribute DisplayAttr => new("[ Track... ]", Job.None, 60071);

    public override string TermCount => "Count";
    public override string TermGauge => "Timer";
    public override string[] StateNames { get; } = { "Inactive", "Active" };

    public override TrackerData GetCurrentData(float? preview = null) => new(0, 1, 0, 1, 0, 1, 0);
}
