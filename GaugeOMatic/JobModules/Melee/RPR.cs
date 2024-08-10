using FFXIVClientStructs.FFXIV.Client.UI;
using GaugeOMatic.Trackers;
using GaugeOMatic.Windows.Dropdowns;
using System;
using System.Collections.Generic;
using static GaugeOMatic.GameData.JobData;
using static GaugeOMatic.GameData.JobData.Job;
using static GaugeOMatic.GameData.JobData.Role;
using static GaugeOMatic.JobModules.Tweaks;
using static GaugeOMatic.Trackers.Tracker;
using static GaugeOMatic.Widgets.WidgetUI;
using static GaugeOMatic.Windows.Dropdowns.TrackerDropdown;

namespace GaugeOMatic.JobModules;

public class RPRModule : JobModule
{
    public override Job Job => RPR;
    public override Job Class => Job.None;
    public override Role Role => Melee;
    public override List<AddonOption> AddonOptions => new()
    {
        new("JobHudRRP0", "Soul Gauge"),
        new("JobHudRRP1", "Death Gauge"),
        new("_ParameterWidget", "Parameter Bar")
    };

    public override List<MenuOption> JobGaugeMenu { get; } = new() {
        new("Soul Gauge", nameof(SoulGaugeTracker)),
        new("Shroud Gauge", nameof(ShroudGaugeTracker)),
        new("Lemure Shroud", nameof(LemureShroudTracker)),
        new("Void Shroud", nameof(VoidShroudTracker))
    };

    public RPRModule(TrackerManager trackerManager, TrackerConfig[] trackerConfigList) : base(trackerManager, trackerConfigList, "JobHudRRP0", "JobHudRRP1") { }

    public override void Save()
    {
        Configuration.TrackerConfigs.RPR = SaveOrder;
        Configuration.Save();
    }

    public override void TweakUI(ref UpdateFlags update)
    {
        Heading("Soul Gauge");
        ToggleControls("Hide Soul Gauge", ref TweakConfigs.RPRHide0, ref update);

        Heading("Death Gauge");
        ToggleControls("Hide Death Gauge", ref TweakConfigs.RPRHide1, ref update);
    }

    public override unsafe void ApplyTweaks0(IntPtr gaugeAddon)
    {
        var gauge = (AddonJobHudRRP0*)gaugeAddon;
        VisibilityTweak(TweakConfigs.RPRHide0, gauge->UseSimpleGauge, gauge->GaugeStandard.Container, gauge->GaugeSimple.Container);
    }

    public override unsafe void ApplyTweaks1(IntPtr gaugeAddon)
    {
        var gauge = (AddonJobHudRRP1*)gaugeAddon;
        VisibilityTweak(TweakConfigs.RPRHide1, gauge->UseSimpleGauge, gauge->GaugeStandard.Container, gauge->GaugeSimple.Container);
    }
}

public partial class TweakConfigs
{
    public bool RPRHide0;
    public bool RPRHide1;
}
