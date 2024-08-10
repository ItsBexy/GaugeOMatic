using FFXIVClientStructs.FFXIV.Client.UI;
using GaugeOMatic.Trackers;
using GaugeOMatic.Windows.Dropdowns;
using System;
using System.Collections.Generic;
using System.Numerics;
using static GaugeOMatic.GameData.JobData;
using static GaugeOMatic.GameData.JobData.Job;
using static GaugeOMatic.GameData.JobData.Role;
using static GaugeOMatic.JobModules.Tweaks;
using static GaugeOMatic.Trackers.Tracker;
using static GaugeOMatic.Widgets.WidgetUI;
using static GaugeOMatic.Windows.Dropdowns.TrackerDropdown;

namespace GaugeOMatic.JobModules;

public class MCHModule : JobModule
{
    public override Job Job => MCH;
    public override Job Class => Job.None;
    public override Role Role => Ranged;
    public override List<AddonOption> AddonOptions => new()
    {
        new("JobHudMCH0", "Heat Gauge"),
        new("_ParameterWidget", "Parameter Bar")
    };

    public override List<MenuOption> JobGaugeMenu { get; } = new()
    {
        new ("Heat Gauge", nameof(HeatGaugeTracker)),
        new ("Battery Gauge", nameof(BatteryGaugeTracker)),
        new ("Automaton Timer", nameof(AutomatonTracker))
    };

    public MCHModule(TrackerManager trackerManager, TrackerConfig[] trackerConfigList) : base(trackerManager, trackerConfigList, "JobHudMCH0") { }

    public override void Save()
    {
        Configuration.TrackerConfigs.MCH = SaveOrder;
        Configuration.Save();
    }

    public override void TweakUI(ref UpdateFlags update)
    {
        Heading("Heat Gauge");
        ToggleControls("Hide Heat Gauge", ref TweakConfigs.MCHHide0Heat, ref update);
        ToggleControls("Hide Battery Gauge", ref TweakConfigs.MCHHide0Battery, ref update);

        if (!TweakConfigs.MCHHide0Battery)
        {
            PositionControls("Move Battery Gauge", ref TweakConfigs.MCHBatteryPos, ref update);
        }
    }

    public override unsafe void ApplyTweaks0(IntPtr gaugeAddon)
    {
        var gauge = (AddonJobHudMCH0*)gaugeAddon;
        VisibilityTweak(TweakConfigs.MCHHide0Heat, gauge->UseSimpleGauge, gauge->GetNodeById(3), gauge->GetNodeById(34));
        VisibilityTweak(TweakConfigs.MCHHide0Battery, gauge->UseSimpleGauge, gauge->GetNodeById(17), gauge->GetNodeById(39));

        if (gauge != null && gauge->GaugeStandard.HeatContainer != null)
        {
            var batteryPos = TweakConfigs.MCHBatteryPos;
            gauge->GaugeStandard.BatteryContainer->SetPositionFloat(batteryPos.X, batteryPos.Y + 59);
            gauge->GaugeSimple.BatteryContainer->SetPositionFloat(batteryPos.X, batteryPos.Y + 72);
        }
    }
}

public partial class TweakConfigs
{
    public bool MCHHide0Heat;
    public bool MCHHide0Battery;
    public Vector2 MCHBatteryPos;
}
