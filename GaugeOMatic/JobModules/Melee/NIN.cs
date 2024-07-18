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

public class NINModule : JobModule
{
    public override Job Job => NIN;
    public override Job Class => ROG;
    public override Role Role => Melee;
    public override List<AddonOption> AddonOptions => new()
    {
        new("JobHudNIN0", "Ninki Gauge"),
        new("JobHudNIN1v70", "Kazematoi"),
        new("_ParameterWidget", "Parameter Bar")
    };

    public override List<MenuOption> JobGaugeMenu { get; } = new() {
        new("Ninki Gauge", nameof(NinkiGaugeTracker)),
        new("Kazematoi Stacks", nameof(KazematoiTracker)),
    };

    public NINModule(TrackerManager trackerManager, TrackerConfig[] trackerConfigList) : base(trackerManager, trackerConfigList, "JobHudNIN0", "JobHudNIN1v70") { }

    public override void Save()
    {
        Configuration.TrackerConfigs.NIN = SaveOrder;
        Configuration.Save();
    }

    public override void TweakUI(ref UpdateFlags update)
    {
        ToggleControls("Hide Ninki Gauge", ref TweakConfigs.NINHide0, ref update);
        HideWarning(TweakConfigs.NINHide0);
        ToggleControls("Hide Kazematoi", ref TweakConfigs.NINHide1, ref update);
        HideWarning(TweakConfigs.NINHide1);
    }

    public override unsafe void ApplyTweaks0(IntPtr gaugeAddon)
    {
        var gauge = (AddonJobHudNIN0*)gaugeAddon;
        VisibilityTweak(TweakConfigs.NINHide0, gauge->UseSimpleGauge, gauge->GaugeStandard.Container, gauge->GaugeSimple.Container);
    }

    public override unsafe void ApplyTweaks1(IntPtr gaugeAddon)
    {
        var gauge = (AddonJobHudNIN1*)gaugeAddon;
        VisibilityTweak(TweakConfigs.NINHide1, gauge->UseSimpleGauge, gauge->GaugeStandard.Container, gauge->GetNodeById(17));
    }
}

public partial class TweakConfigs
{
    public bool NINHide0;
    public bool NINHide1;
}
