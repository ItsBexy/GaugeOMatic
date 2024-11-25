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

public class DRGModule(TrackerManager trackerManager, TrackerConfig[] trackerConfigList)
    : JobModule(trackerManager, trackerConfigList, "JobHudDRG0")
{
    public override Job Job => DRG;
    public override Job Class => LNC;
    public override Role Role => Melee;
    public override List<AddonOption> AddonOptions =>
    [
        new("JobHudDRG0", "Dragon Gauge"),
        new("_ParameterWidget", "Parameter Bar")
    ];

    public override List<MenuOption> JobGaugeMenu { get; } = [new("Firstminds' Focus", nameof(FirstmindsFocusTracker))];

    public override void Save()
    {
        Configuration.TrackerConfigs.DRG = SaveOrder;
        Configuration.Save();
    }

    public override void TweakUI()
    {
        Heading("Dragon Gauge");
        ToggleControls("Hide Dragon Gauge", ref TweakConfigs.DRGHide0);
    }

    public override unsafe void ApplyTweaks0(IntPtr gaugeAddon)
    {
        var gauge = (AddonJobHudDRG0*)gaugeAddon;
        VisibilityTweak(TweakConfigs.DRGHide0, gauge->UseSimpleGauge, gauge->GaugeStandard.Container, gauge->GaugeSimple.Container);
    }
}

public partial class TweakConfigs
{
    public bool DRGHide0;
}
