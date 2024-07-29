using FFXIVClientStructs.FFXIV.Client.UI;
using GaugeOMatic.Trackers;
using GaugeOMatic.Windows;
using System;
using System.Collections.Generic;
using static GaugeOMatic.GameData.JobData;
using static GaugeOMatic.GameData.JobData.Job;
using static GaugeOMatic.GameData.JobData.Role;
using static GaugeOMatic.JobModules.Tweaks;
using static GaugeOMatic.Widgets.WidgetUI;
using static GaugeOMatic.Windows.ItemRefMenu;

namespace GaugeOMatic.JobModules;

public class DNCModule : JobModule
{
    public override Job Job => DNC;
    public override Job Class => Job.None;
    public override Role Role => Ranged;
    public override List<AddonOption> AddonOptions => new()
    {
        new("JobHudDNC0", "Step Gauge"),
        new("JobHudDNC1", "Fourfold Feathers"),
        new("_ParameterWidget", "Parameter Bar")
    };

    public override List<MenuOption> JobGaugeMenu { get; } = new()
    {
        new("Fourfold Feathers", nameof(FourfoldTracker)),
        new("Esprit Gauge", nameof(EspritGaugeTracker)),
        new("Dance Steps", nameof(DanceStepTracker))
    };

    public DNCModule(TrackerManager trackerManager, TrackerConfig[] trackerConfigList) : base(trackerManager, trackerConfigList, "JobHudDNC0", "JobHudDNC1") { }

    public override void Save()
    {
        Configuration.TrackerConfigs.DNC = SaveOrder;
        Configuration.Save();
    }

    public override void TweakUI(ref UpdateFlags update)
    {
        Heading("Step Gauge");
        ToggleControls("Hide Step Gauge", ref TweakConfigs.DNCHide0, ref update);

        Heading("Fourfold Feathers");
        ToggleControls("Hide Fourfold Feathers", ref TweakConfigs.DNCHide1, ref update);
    }

    public override unsafe void ApplyTweaks0(IntPtr gaugeAddon)
    {
        var gauge = (AddonJobHudDNC0*)gaugeAddon;
        VisibilityTweak(TweakConfigs.DNCHide0, gauge->UseSimpleGauge, gauge->GaugeStandard.Container, gauge->GaugeSimple.Container);
    }

    public override unsafe void ApplyTweaks1(IntPtr gaugeAddon)
    {
        var gauge = (AddonJobHudDNC1*)gaugeAddon;
        VisibilityTweak(TweakConfigs.DNCHide1, gauge->UseSimpleGauge, gauge->GaugeStandard.Container, gauge->GaugeSimple.Container);
    }
}

public partial class TweakConfigs
{
    public bool DNCHide0;
    public bool DNCHide1;
}
