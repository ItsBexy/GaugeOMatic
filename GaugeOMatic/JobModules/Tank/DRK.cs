using FFXIVClientStructs.FFXIV.Client.UI;
using GaugeOMatic.Trackers;
using GaugeOMatic.Windows.Dropdowns;
using System;
using System.Collections.Generic;
using static GaugeOMatic.GameData.JobData;
using static GaugeOMatic.GameData.JobData.Job;
using static GaugeOMatic.GameData.JobData.Role;
using static GaugeOMatic.JobModules.Tweaks;
using static GaugeOMatic.Widgets.Common.WidgetUI;
using static GaugeOMatic.Windows.Dropdowns.TrackerDropdown;

namespace GaugeOMatic.JobModules;

public class DRKModule(TrackerManager trackerManager, TrackerConfig[] trackerConfigList)
    : JobModule(trackerManager, trackerConfigList, "JobHudDRK0", "JobHudDRK1")
{
    public override Job Job => DRK;
    public override Job Class => Job.None;
    public override Role Role => Tank;
    public override List<AddonOption> AddonOptions =>
    [
        new("JobHudDRK0", "Blood Gauge"),
        new("JobHudDRK1", "Darkside Gauge"),
        new("_ParameterWidget", "Parameter Bar")
    ];

    public override List<MenuOption> JobGaugeMenu { get; } =
    [
        new("Blood Gauge", nameof(BloodGaugeTracker)),
        new("Darkside Gauge", nameof(DarksideGaugeTracker)),
        new("Living Shadow", nameof(LivingShadowTracker))
    ];

    public override void Save()
    {
        Configuration.TrackerConfigs.DRK = SaveOrder;
        Configuration.Save();
    }

    public override void TweakUI()
    {
        Heading("Blood Gauge");
        ToggleControls("Hide Blood Gauge", ref TweakConfigs.DRKHide0);

        Heading("Darkside Gauge");
        ToggleControls("Hide Darkside Gauge", ref TweakConfigs.DRKHide1);
    }

    public override unsafe void ApplyTweaks0(IntPtr gaugeAddon)
    {
        var gauge = (AddonJobHudDRK0*)gaugeAddon;
        VisibilityTweak(TweakConfigs.DRKHide0, gauge->UseSimpleGauge, gauge->GaugeStandard.Container, gauge->GaugeSimple.Container);
    }

    public override unsafe void ApplyTweaks1(IntPtr gaugeAddon)
    {
        var gauge = (AddonJobHudDRK1*)gaugeAddon;
        VisibilityTweak(TweakConfigs.DRKHide1, gauge->UseSimpleGauge, gauge->GaugeStandard.Container, gauge->GetNodeById(20));
    }
}

public partial class TweakConfigs
{
    public bool DRKHide0;
    public bool DRKHide1;
}
