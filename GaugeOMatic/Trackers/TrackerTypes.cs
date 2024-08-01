using FFXIVClientStructs.FFXIV.Client.UI;
using GaugeOMatic.GameData;
using GaugeOMatic.Windows;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using static GaugeOMatic.GameData.JobData;
using static GaugeOMatic.Trackers.RefType;

namespace GaugeOMatic.Trackers;

[AttributeUsage(AttributeTargets.Class)]
public class TrackerDisplayAttribute : Attribute
{
    public Job Job = Job.None;
    public Role Role = Role.None;
    public uint GameIcon = 60071;
    public string? BarDesc;
    public string? CounterDesc;
    public string? StateDesc;
    public string? Footer;

    public TrackerDisplayAttribute() { }

    public TrackerDisplayAttribute(uint gameIcon, Job job = Job.None, Role role = Role.None)
    {
        GameIcon = gameIcon;
        Job = job;
        Role = role;
    }

    public TrackerDisplayAttribute(Job job, string? barDesc = null, string? counterDesc = null, string? stateDesc = null, string? footer = null)
    {
        GameIcon = GetJobIcon(job);
        Job = job;

        BarDesc = barDesc;
        CounterDesc = counterDesc;
        StateDesc = stateDesc;
        Footer = footer;
    }

    // ReSharper disable once UnusedMember.Global
    public TrackerDisplayAttribute(Job job, uint gameIcon, string? barDesc = null, string? counterDesc = null, string? stateDesc = null, string? footer = null)
    {
        GameIcon = gameIcon;
        Job = job;

        BarDesc = barDesc;
        CounterDesc = counterDesc;
        StateDesc = stateDesc;
        Footer = footer;
    }

    public void DrawTooltip(string name) => Tooltips.DrawTooltip(GameIcon, name, BarDesc, CounterDesc, StateDesc, Footer);
}

public abstract partial class Tracker
{
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public virtual string TermCount => "Count";
    public virtual string TermGauge => "Timer";
    public virtual string[] StateNames => new[] { "Inactive", "Active" };

    public virtual void DrawTooltip() { }

    public abstract RefType RefType { get; }
    public abstract TrackerData GetCurrentData(float? preview = null);
}

[TrackerDisplay(10453)]
public sealed class StatusTracker : Tracker
{
    public override void DrawTooltip() => ((StatusRef)ItemRef!).DrawTooltip();
    public override RefType RefType => Status;
    public override string DisplayName => ItemRef?.Name ?? string.Empty;
    public override uint GameIcon => ItemRef!.Icon ?? 10453;

    public override string TermCount => "Stacks";
    public override string TermGauge => "Timer";
    public override string[] StateNames { get; } = { "Inactive", "Active" };

    public override TrackerData GetCurrentData(float? preview = null) => new((StatusRef)ItemRef!, preview);
}

[TrackerDisplay(101)]
public sealed class ActionTracker : Tracker
{
    public override void DrawTooltip() => ((ActionRef)ItemRef!).DrawTooltip();
    public override RefType RefType => RefType.Action;
    public override string DisplayName => ((ActionRef)ItemRef!).GetAdjustedName();
    public override uint GameIcon => ((ActionRef)ItemRef!).GetAdjustedIcon() ?? 101;

    public override string TermCount => "Charges";
    public override string TermGauge => "Timer";
    public override string[] StateNames { get; } = { "Unavailable", "Available" };

    public override TrackerData GetCurrentData(float? preview = null) => new((ActionRef)ItemRef!, preview);
}

[TrackerDisplay(29)]
public abstract unsafe class JobGaugeTracker<T> : Tracker where T : unmanaged
{
    public override void DrawTooltip()
    {
        var displayAttr = (TrackerDisplayAttribute?)GetType().GetCustomAttributes(typeof(TrackerDisplayAttribute), true).FirstOrDefault();
        displayAttr?.DrawTooltip(DisplayName);
    }
    public override RefType RefType => JobGauge;
    public abstract Job Job { get; }
    public override uint GameIcon => GetJobIcon(Job);

    public override string TermCount => "Count";
    public override string TermGauge => "Gauge";

    public override string[] StateNames { get; } = { "Inactive", "Active" };

    public abstract string GaugeAddonName { get; }

    public AddonJobHud* GaugeAddon => (AddonJobHud*)GameGui.GetAddonByName(GaugeAddonName);
    public T* GaugeData => (T*)GaugeAddon->DataCurrentPointer;
}

[TrackerDisplay(Job.None,61233)]
public class ParameterTracker : Tracker
{
    public override void DrawTooltip() => ((ParamRef)ItemRef!).DrawTooltip();

    public override RefType RefType => Parameter;
    public override string DisplayName => ItemRef?.Name ?? string.Empty;
    public override uint GameIcon => 61233;
    public override TrackerData GetCurrentData(float? preview = null) => new((ParamRef)ItemRef!, preview);

}

[TrackerDisplay(60071)]
public class EmptyTracker : Tracker
{
    public override void DrawTooltip() { }
    public override RefType RefType => None;
    public override string DisplayName => "[ Track... ]";
    public override uint GameIcon => 60071;

    public override string TermCount => "Count";
    public override string TermGauge => "Timer";
    public override string[] StateNames { get; } = { "Inactive", "Active" };

    public override TrackerData GetCurrentData(float? preview = null) => new(0, 1, 0, 1, 0, 1, 0);
}
