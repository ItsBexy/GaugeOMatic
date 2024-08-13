using CustomNodes;
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
        if (!TweakConfigs.MNKHide0)
        {
            ToggleControls("Reverse Order", ref TweakConfigs.MNK0Reverse, ref update);
            Info("Reverses the order of the Beast Chakras. Useful if you\nprefer to arrange your combo buttons from right to left.");
        }

        Heading("Chakra Gauge");
        ToggleControls("Hide Chakra Gauge", ref TweakConfigs.MNKHide1, ref update);
    }

    public override unsafe void ApplyTweaks0(IntPtr gaugeAddon)
    {
        var gauge = (AddonJobHudMNK0*)gaugeAddon;
        VisibilityTweak(TweakConfigs.MNKHide0, gauge->UseSimpleGauge, gauge->GaugeStandard.Container, gauge->GaugeSimple.Container);

        ReorderTweak(gaugeAddon);
    }

    private void ReorderTweak(AddonIndex gaugeIndex)
    {
        if (TweakConfigs.MNK0Reverse)
        {
            gaugeIndex[4u].SetX(106);
            gaugeIndex[7u].SetX(66);
            gaugeIndex[11u].SetX(10);

            gaugeIndex[40u].SetX(139);
            gaugeIndex[43u].SetX(90);
            gaugeIndex[47u].SetX(20);
        }
        else
        {
            gaugeIndex[4u].SetX(10);
            gaugeIndex[7u].SetX(58);
            gaugeIndex[11u].SetX(106);

            gaugeIndex[40u].SetX(20);
            gaugeIndex[43u].SetX(70);
            gaugeIndex[47u].SetX(120);
        }
    }

    public override unsafe void ApplyTweaks1(IntPtr gaugeAddon)
    {
        var gauge = (AddonJobHudMNK1*)gaugeAddon;
        VisibilityTweak(TweakConfigs.MNKHide1, gauge->UseSimpleGauge, gauge->GaugeStandard.Container, gauge->GaugeSimple.Container);
    }
}

public partial class TweakConfigs
{
    public bool MNKHide0;
    public bool MNK0Reverse;

    public bool MNKHide1;
}
