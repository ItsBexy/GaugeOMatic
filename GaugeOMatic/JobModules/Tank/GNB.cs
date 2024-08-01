using FFXIVClientStructs.FFXIV.Client.UI;
using GaugeOMatic.Trackers;
using GaugeOMatic.Windows;
using System;
using System.Collections.Generic;
using static GaugeOMatic.GameData.JobData;
using static GaugeOMatic.GameData.JobData.Job;
using static GaugeOMatic.GameData.JobData.Role;
using static GaugeOMatic.JobModules.Tweaks;
using static GaugeOMatic.Trackers.Tracker;
using static GaugeOMatic.Widgets.WidgetUI;
using static GaugeOMatic.Windows.ItemRefMenu;

namespace GaugeOMatic.JobModules;

public class GNBModule : JobModule
{
    public override Job Job => GNB;
    public override Job Class => Job.None;
    public override Role Role => Tank;
    public override List<AddonOption> AddonOptions => new()
    {
        new("JobHudGNB0", "Powder Gauge"),
        new("_ParameterWidget", "Parameter Bar")
    };

    public override List<MenuOption> JobGaugeMenu { get; } = new()
    {
        new("Powder Gauge", nameof(PowderGaugeTracker))
    };

    public GNBModule(TrackerManager trackerManager, TrackerConfig[] trackerConfigList) : base(trackerManager, trackerConfigList, "JobHudGNB0") { }

    public override void Save()
    {
        Configuration.TrackerConfigs.GNB = SaveOrder;
        Configuration.Save();
    }

    public override void TweakUI(ref UpdateFlags update)
    {
        Heading("Powder Gauge");
        ToggleControls("Hide Powder Gauge", ref TweakConfigs.GNBHide0, ref update);
    }

    public override unsafe void ApplyTweaks0(IntPtr gaugeAddon)
    {
        var gauge = (AddonJobHudGNB0*)gaugeAddon;
        VisibilityTweak(TweakConfigs.GNBHide0, gauge->UseSimpleGauge, gauge->GaugeStandard.Container, gauge->GetNodeById(19));
    }
}

public partial class TweakConfigs
{
    public bool GNBHide0;
}
