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

public class MNKModule : JobModule
{
    public override Job Job => MNK;
    public override Job Class => PGL;
    public override Role Role => Melee;
    public override List<AddonOption> AddonOptions => new()
    {
        new("JobHudMNK0", "Beast Chakra Gauge"),
        new("JobHudMNK1", "Chakra Gauge"),
        new("_ParameterWidget", "Parameter Bar")
    };

    public override List<MenuOption> JobGaugeMenu { get; } = new()
    {
        new("Chakra Gauge", nameof(ChakraGaugeTracker))
    };

    public MNKModule(TrackerManager trackerManager, TrackerConfig[] trackerConfigList) : base(trackerManager, trackerConfigList, "JobHudMNK0", "JobHudMNK1") { }

    public override void Save()
    {
        Configuration.TrackerConfigs.MNK = SaveOrder;
        Configuration.Save();
    }

    public override void TweakUI(ref UpdateFlags update)
    {
        Heading("Beast Chakra Gauge");
        ToggleControls("Hide Beast Chakra Gauge", ref TweakConfigs.MNKHide0, ref update);
        HideInfo(TweakConfigs.MNKHide0);

        Heading("Chakra Gauge");
        ToggleControls("Hide Chakra Gauge", ref TweakConfigs.MNKHide1, ref update);
        HideInfo(TweakConfigs.MNKHide1);
    }

    public override unsafe void ApplyTweaks0(IntPtr gaugeAddon)
    {
        var gauge = (AddonJobHudMNK0*)gaugeAddon;
        VisibilityTweak(TweakConfigs.MNKHide0, gauge->UseSimpleGauge, gauge->GaugeStandard.Container, gauge->GaugeSimple.Container);
    }

    public override unsafe void ApplyTweaks1(IntPtr gaugeAddon)
    {
        var gauge = (AddonJobHudMNK1*)gaugeAddon;
        VisibilityTweak(TweakConfigs.MNKHide1, gauge->UseSimpleGauge, gauge->GaugeStandard.Container, gauge->GaugeSimple.Container);
    }
}

public partial class TweakConfigs
{
    public bool MNKHide1;
    public bool MNKHide0;
}
