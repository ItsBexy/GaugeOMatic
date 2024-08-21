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
    public virtual string[] StateNames => ["Inactive", "Active"];

    public virtual TrackerDisplayAttribute DisplayAttr => (TrackerDisplayAttribute?)GetType().GetCustomAttributes(typeof(TrackerDisplayAttribute), true).FirstOrDefault() ?? new();

    public abstract RefType RefType { get; }
    public abstract TrackerData GetCurrentData(float? preview = null);
}

public sealed class StatusTracker : Tracker
{
    public StatusRef StatusRef => StatusRefCopy ??= ((StatusRef)ItemRef!.ID).Clone();
    public StatusRef? StatusRefCopy { get; private set; } // make a local copy for the tracker so we can modify it

    public override RefType RefType => Status;
    public override TrackerDisplayAttribute DisplayAttr => StatusRef;

    public override string TermCount => "Stacks";
    public override string TermGauge => "Timer";
    public override string[] StateNames { get; } = ["Inactive", "Active"];

    public override TrackerData GetCurrentData(float? preview = null) => StatusRef.GetTrackerData(preview);
}

public sealed class ActionTracker : Tracker
{
    public ActionRef ActionRef => ActionRefCopy ??= ItemRef!.ID;
    public ActionRef? ActionRefCopy { get; private set; }

    public override RefType RefType => Action;
    public override TrackerDisplayAttribute DisplayAttr => ActionRef;

    public override string TermCount => "Charges";
    public override string TermGauge => "Timer";
    public override string[] StateNames { get; } = ["Unavailable", "Available"];

    public override TrackerData GetCurrentData(float? preview = null) => ActionRef.GetTrackerData(preview);
}

public abstract unsafe class JobGaugeTracker<T> : Tracker where T : unmanaged
{
    public override RefType RefType => JobGauge;

    public override string TermCount => "Count";
    public override string TermGauge => "Gauge";

    public override string[] StateNames { get; } = ["Inactive", "Active"];

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
    public override string[] StateNames { get; } = ["Inactive", "Active"];

    public override TrackerData GetCurrentData(float? preview = null) => new(0, 1, 0, 1, 0, 1, 0);
}
