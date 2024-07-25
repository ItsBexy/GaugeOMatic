using FFXIVClientStructs.FFXIV.Client.UI;
using GaugeOMatic.Trackers;
using GaugeOMatic.Windows;
using System;
using System.Collections.Generic;
using static GaugeOMatic.GameData.JobData;
using static GaugeOMatic.GameData.JobData.Job;
using static GaugeOMatic.GameData.JobData.Role;
using static GaugeOMatic.JobModules.Tweaks;
using static GaugeOMatic.JobModules.Tweaks.TweakUI;
using static GaugeOMatic.Widgets.WidgetUI;
using static GaugeOMatic.Windows.ItemRefMenu;

namespace GaugeOMatic.JobModules;

public class WARModule : JobModule
{
    public override Job Job => WAR;
    public override Job Class => MRD;
    public override Role Role => Tank;
    public override List<AddonOption> AddonOptions => new()
    {
        new("JobHudWAR0", "Beast Gauge"),
        new("_ParameterWidget", "Parameter Bar")
    };

    public override List<MenuOption> JobGaugeMenu { get; } = new() {
        new("Beast Gauge", nameof(BeastGaugeTracker))
    };

    public WARModule(TrackerManager trackerManager, TrackerConfig[] trackerConfigList) : base(trackerManager, trackerConfigList, "JobHudWAR0") { }

    public override void Save()
    {
        Configuration.TrackerConfigs.WAR = SaveOrder;
        Configuration.Save();
    }

    public override void TweakUI(ref UpdateFlags update)
    {
        Heading("Beast Gauge");
        ToggleControls("Hide Beast Gauge", ref TweakConfigs.WARHide0, ref update);
        HideInfo(TweakConfigs.WARHide0);
    }

    public override unsafe void ApplyTweaks0(IntPtr gaugeAddon)
    {
        var gauge = (AddonJobHudWAR0*)gaugeAddon;
        VisibilityTweak(TweakConfigs.WARHide0, gauge->UseSimpleGauge, gauge->GaugeStandard.Container, gauge->GaugeSimple.BarContainer);
    }
}

public partial class TweakConfigs
{
    public bool WARHide0;
}
