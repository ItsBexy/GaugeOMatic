using Dalamud.Interface;
using FFXIVClientStructs.FFXIV.Client.UI;
using GaugeOMatic.GameData;
using System;
using System.Diagnostics.CodeAnalysis;
using static Dalamud.Interface.FontAwesomeIcon;
using static GaugeOMatic.GameData.ActionData;
using static GaugeOMatic.GameData.JobData;
using static GaugeOMatic.GameData.StatusData;
using static GaugeOMatic.Trackers.RefType;
using static GaugeOMatic.Trackers.Tracker;
using static GaugeOMatic.Trackers.Tracker.IconColor;
using static GaugeOMatic.Utility.Color;

namespace GaugeOMatic.Trackers;

[AttributeUsage(AttributeTargets.Class)]
public class TrackerDisplayAttribute : Attribute
{
    public Job Job = Job.None;
    public Role Role = Role.None;
    public FontAwesomeIcon Icon = Question;
    public ColorRGB Color = 0x494949ff;
    public string TypeDesc = "[ Track... ]";
    public string? ToolText;

    public TrackerDisplayAttribute() { }

    public TrackerDisplayAttribute(FontAwesomeIcon icon, IconColor color, string typeDesc, Job job = Job.None, Role role = Role.None)
    {
        Color = (uint)color;
        Icon = icon;
        TypeDesc = typeDesc;
        Job = job;
        Role = role;
    }

    public TrackerDisplayAttribute(Job job, string toolText)
    {
        Icon = Gauge;
        Color = (uint)JobGaugeColor;
        TypeDesc = $"{job} Gauge Tracker";
        Job = job;
        ToolText = toolText;
    }
}

public abstract partial class Tracker
{
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public virtual string TermCount => "Count";
    public virtual string TermGauge => "Timer";
    public virtual string[] StateNames => new[] { "Inactive", "Active" };

    public abstract RefType RefType { get; }
    public abstract TrackerData GetCurrentData(float? preview = null);

    public enum IconColor : uint
    {
        NoneColor = 0x494949ffu,
        StatusColor = 0x1c6e68ffu,
        ActionColor = 0xaa372dffu,
        JobGaugeColor = 0x2b455cffu
    }
}

[TrackerDisplay(Tags, StatusColor, "Status Tracker")]
public sealed class StatusTracker : Tracker
{
    public override RefType RefType => Status;
    public override string DisplayName => ItemRef?.Name ?? string.Empty;

    public override string TermCount => "Stacks";
    public override string TermGauge => "Timer";
    public override string[] StateNames { get; } = { "Inactive", "Active" };

    public override TrackerData GetCurrentData(float? preview = null) => new((StatusRef)ItemRef!, preview);
}

[TrackerDisplay(FistRaised, ActionColor, "Action Tracker")]
public sealed class ActionTracker : Tracker
{
    public override RefType RefType => RefType.Action;
    public override string DisplayName => ItemRef?.Name ?? string.Empty;

    public override string TermCount => "Charges";
    public override string TermGauge => "Timer";
    public override string[] StateNames { get; } = { "Unavailable", "Available" };

    public override TrackerData GetCurrentData(float? preview = null) => new((ActionRef)ItemRef!, preview);
}

[TrackerDisplay(Gauge, JobGaugeColor, "Job Gauge")]
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

[TrackerDisplay(BarsProgress, NoneColor, "Other")]
public class ParameterTracker : Tracker
{
    public override RefType RefType => Parameter;
    public override string DisplayName => ItemRef?.Name ?? string.Empty;
    public override TrackerData GetCurrentData(float? preview = null) => new((ParamRef)ItemRef!, preview);

}

[TrackerDisplay(Question, NoneColor, "No Tracker Assigned")]
public class EmptyTracker : Tracker
{
    public override RefType RefType => RefType.None;
    public override string DisplayName => "[ Track... ]";

    public override string TermCount => "Count";
    public override string TermGauge => "Timer";
    public override string[] StateNames { get; } = { "Inactive", "Active" };

    public override TrackerData GetCurrentData(float? preview = null) => new(0, 1, 0, 1, 0, 1, 0);
}
