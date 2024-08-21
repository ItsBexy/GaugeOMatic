using FFXIVClientStructs.FFXIV.Client.UI;
using GaugeOMatic.Trackers;
using GaugeOMatic.Windows.Dropdowns;
using System;
using System.Collections.Generic;
using static GaugeOMatic.GameData.JobData;
using static GaugeOMatic.GameData.JobData.Job;
using static GaugeOMatic.GameData.JobData.Role;
using static GaugeOMatic.JobModules.Tweaks;
using static GaugeOMatic.Widgets.WidgetUI;
using static GaugeOMatic.Windows.Dropdowns.TrackerDropdown;

namespace GaugeOMatic.JobModules;

public class WHMModule(TrackerManager trackerManager, TrackerConfig[] trackerConfigList)
    : JobModule(trackerManager, trackerConfigList, "JobHudWHM0")
{
    public override Job Job => WHM;
    public override Job Class => CNJ;
    public override Role Role => Healer;
    public override List<AddonOption> AddonOptions =>
    [
        new("JobHudWHM0", "Healing Gauge"),
        new("_ParameterWidget", "Parameter Bar")
    ];

    public override List<MenuOption> JobGaugeMenu =>
    [
        new("Lilies", nameof(LilyTracker)),
        new("Blood Lily", nameof(BloodLilyTracker))
    ];

    public override void Save()
    {
        Configuration.TrackerConfigs.WHM = SaveOrder;
        Configuration.Save();
    }

    public override void TweakUI()
    {
        Heading("Healing Gauge");
        ToggleControls("Hide Healing Gauge", ref TweakConfigs.WHMHide0);
    }

    public override unsafe void ApplyTweaks0(IntPtr gaugeAddon)
    {
        var gauge = (AddonJobHudWHM0*)gaugeAddon;
        VisibilityTweak(TweakConfigs.WHMHide0, gauge->UseSimpleGauge, gauge->GaugeStandard.Container, gauge->GaugeSimple.Container);
    }
}

public partial class TweakConfigs
{
    public bool WHMHide0;
}
