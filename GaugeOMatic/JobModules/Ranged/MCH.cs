using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;
using GaugeOMatic.Trackers;
using GaugeOMatic.Windows;
using System.Collections.Generic;
using System.Numerics;
using static GaugeOMatic.GameData.JobData;
using static GaugeOMatic.GameData.JobData.Job;
using static GaugeOMatic.GameData.JobData.Role;
using static GaugeOMatic.JobModules.TweakUI;
using static GaugeOMatic.Widgets.WidgetUI;
using static GaugeOMatic.Windows.ItemRefMenu;

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
        ToggleControls("Hide Heat Gauge", ref TweakConfigs.MCHHide0Heat, ref update);
        HideWarning(TweakConfigs.MCHHide0Heat);
        ToggleControls("Hide Battery Gauge", ref TweakConfigs.MCHHide0Battery, ref update);
        HideWarning(TweakConfigs.MCHHide0Battery);

        if (!TweakConfigs.MCHHide0Battery)
        {
            PositionControls("Move Battery Gauge", ref TweakConfigs.MCHBatteryPos, ref update);
            if (update.HasFlag(UpdateFlags.Save)) ApplyTweaks0();
        }
    }

    public override unsafe void ApplyTweaks0()
    {
        var heatGauge = (AddonJobHudMCH0*)GameGui.GetAddonByName("JobHudMCH0");
        if (heatGauge != null && heatGauge->GaugeStandard.HeatContainer != null)
        {
            var simple0 = ((AddonJobHud*)heatGauge)->UseSimpleGauge;

            var hideHeat = TweakConfigs.MCHHide0Heat;
            var hideBattery = TweakConfigs.MCHHide0Battery;
            var batteryPos = TweakConfigs.MCHBatteryPos;

            ((AtkUnitBase*)heatGauge)->GetNodeById(3)->ToggleVisibility(!hideHeat && !simple0);
            ((AtkUnitBase*)heatGauge)->GetNodeById(34)->ToggleVisibility(!hideHeat && simple0);

            ((AtkUnitBase*)heatGauge)->GetNodeById(17)->ToggleVisibility(!hideBattery && !simple0);
            ((AtkUnitBase*)heatGauge)->GetNodeById(39)->ToggleVisibility(!hideBattery && simple0);

            heatGauge->GaugeStandard.BatteryContainer->SetPositionFloat(batteryPos.X, batteryPos.Y + 59);
            heatGauge->GaugeSimple.BatteryContainer->SetPositionFloat(batteryPos.X, batteryPos.Y + 72);
        }
    }
}

public partial class TweakConfigs
{
    public bool MCHHide0Heat;
    public bool MCHHide0Battery;
    public Vector2 MCHBatteryPos;
}
