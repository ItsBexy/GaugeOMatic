using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;
using GaugeOMatic.Trackers;
using GaugeOMatic.Windows;
using System.Collections.Generic;
using System.Numerics;
using static GaugeOMatic.GameData.JobData;
using static GaugeOMatic.GameData.JobData.Job;
using static GaugeOMatic.GameData.JobData.Role;
using static GaugeOMatic.GaugeOMatic.Service;
using static GaugeOMatic.JobModules.TweakUI;
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
        new ("Heat Gauge",nameof(HeatGaugeTracker)),
        new ("Battery Gauge",nameof(BatteryGaugeTracker)),
        new ("Automaton Timer",nameof(AutomatonTracker))
    };

    public MCHModule(TrackerManager trackerManager, TrackerConfig[] trackerConfigList) : base(trackerManager, trackerConfigList) { }

    public override void Save()
    {
        Configuration.TrackerConfigs.MCH = SaveOrder;
        Configuration.Save();
    }

    public override void TweakUI(ref UpdateFlags update)
    {
        var hideAll = TweakConfigs.MCHHideAll;
        var batteryPos = TweakConfigs.MCHBatteryPos;

        if (Bool1("Hide Heat Gauge", ref hideAll, ref update)) TweakConfigs.MCHHideAll = hideAll;
        HideWarning(hideAll);

        if (PositionControls("Move Battery Gauge", ref batteryPos, ref update)) TweakConfigs.MCHBatteryPos = batteryPos;

        if (update.HasFlag(UpdateFlags.Save)) ApplyTweaks();
    }

    public override unsafe void ApplyTweaks()
    {
        var heatGauge = (AddonJobHudMCH0*)GameGui.GetAddonByName("JobHudMCH0");
        if (heatGauge != null && heatGauge->GaugeStandard.HeatContainer != null)
        {
            var simple0 = heatGauge->JobHud.UseSimpleGauge;

            var hideAll = TweakConfigs.MCHHideAll;
            var batteryPos = TweakConfigs.MCHBatteryPos;

            ((AtkUnitBase*)heatGauge)->GetNodeById(2)->ToggleVisibility(!hideAll && !simple0);
            ((AtkUnitBase*)heatGauge)->GetNodeById(33)->ToggleVisibility(!hideAll && simple0);

            heatGauge->GaugeStandard.BatteryContainer->SetPositionFloat(batteryPos.X,batteryPos.Y + 59);
            heatGauge->GaugeSimple.BatteryContainer->SetPositionFloat(batteryPos.X, batteryPos.Y + 72);
        }
    }
}

public partial class TweakConfigs
{
    public bool MCHHideAll { get; set; }
    public Vector2 MCHBatteryPos { get; set; }
}
