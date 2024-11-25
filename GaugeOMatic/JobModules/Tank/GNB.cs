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

public class GNBModule(TrackerManager trackerManager, TrackerConfig[] trackerConfigList)
    : JobModule(trackerManager, trackerConfigList, "JobHudGNB0")
{
    public override Job Job => GNB;
    public override Job Class => Job.None;
    public override Role Role => Tank;
    public override List<AddonOption> AddonOptions =>
    [
        new("JobHudGNB0", "Powder Gauge"),
        new("_ParameterWidget", "Parameter Bar")
    ];

    public override List<MenuOption> JobGaugeMenu { get; } = [new("Powder Gauge", nameof(PowderGaugeTracker))];

    public override void Save()
    {
        Configuration.TrackerConfigs.GNB = SaveOrder;
        Configuration.Save();
    }

    public override void TweakUI()
    {
        Heading("Powder Gauge");
        ToggleControls("Hide Powder Gauge", ref TweakConfigs.GNBHide0);
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
