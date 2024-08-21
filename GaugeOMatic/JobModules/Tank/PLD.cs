using FFXIVClientStructs.FFXIV.Client.UI;
using GaugeOMatic.Trackers;
using System;
using System.Collections.Generic;
using CustomNodes;
using static GaugeOMatic.GameData.JobData;
using static GaugeOMatic.GameData.JobData.Job;
using static GaugeOMatic.GameData.JobData.Role;
using static GaugeOMatic.JobModules.Tweaks;
using static GaugeOMatic.Widgets.WidgetUI;
using static GaugeOMatic.Windows.Dropdowns.TrackerDropdown;
using GaugeOMatic.Windows.Dropdowns;

namespace GaugeOMatic.JobModules;

public class PLDModule(TrackerManager trackerManager, TrackerConfig[] trackerConfigList)
    : JobModule(trackerManager, trackerConfigList, "JobHudPLD0")
{
    public override Job Job => PLD;
    public override Job Class => GLA;
    public override Role Role => Tank;
    public override List<AddonOption> AddonOptions =>
    [
        new("JobHudPLD0", "Oath Gauge"),
        new("_ParameterWidget", "Parameter Bar")
    ];

    public override List<MenuOption> JobGaugeMenu { get; } = [new("Oath Gauge", nameof(OathGaugeTracker))];

    public override void Save()
    {
        Configuration.TrackerConfigs.PLD = SaveOrder;
        Configuration.Save();
    }

    public override void TweakUI()
    {
        Heading("Oath Gauge");
        ToggleControls("Hide Oath Gauge", ref TweakConfigs.PLDHide0);
    }

    public override unsafe void ApplyTweaks0(IntPtr gaugeAddon)
    {
        var gauge = (AddonJobHudPLD0*)gaugeAddon;
        var gaugeIndex = new AddonIndex(gaugeAddon);
        VisibilityTweak(TweakConfigs.PLDHide0, gauge->UseSimpleGauge, gauge->GaugeStandard.Container, gaugeIndex[14u]);
    }
}

public partial class TweakConfigs
{
    public bool PLDHide0;
}
