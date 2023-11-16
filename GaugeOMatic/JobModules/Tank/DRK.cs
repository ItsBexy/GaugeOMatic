using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;
using GaugeOMatic.Trackers;
using GaugeOMatic.Windows;
using System.Collections.Generic;
using static GaugeOMatic.GameData.JobData;
using static GaugeOMatic.GameData.JobData.Job;
using static GaugeOMatic.GameData.JobData.Role;
using static GaugeOMatic.GaugeOMatic.Service;
using static GaugeOMatic.JobModules.TweakUI;
using static GaugeOMatic.Windows.ItemRefMenu;

namespace GaugeOMatic.JobModules;

public class DRKModule : JobModule
{
    public override Job Job => DRK;
    public override Job Class => Job.None;
    public override Role Role => Tank;
    public override List<AddonOption> AddonOptions => new()
    {
        new("JobHudDRK0", "Blood Gauge"),
        new("JobHudDRK1", "Darkside Gauge"),
        new("_ParameterWidget", "Parameter Bar")
    };

    public override List<MenuOption> JobGaugeMenu { get; } = new()
    {
        new("Blood Gauge",nameof(BloodGaugeTracker)),
        new("Darkside Gauge",nameof(DarksideGaugeTracker)),
        new("Living Shadow",nameof(LivingShadowTracker))
    };

    public DRKModule(TrackerManager trackerManager, TrackerConfig[] trackerConfigList) : base(trackerManager, trackerConfigList) { }

    public override void Save()
    {
        Configuration.TrackerConfigs.DRK = SaveOrder;
        Configuration.Save();
    }

    public override void TweakUI(ref UpdateFlags update)
    {
        var drkHide0 = TweakConfigs.DRKHide0;
        var drkHide1 = TweakConfigs.DRKHide1;

        if (Bool1("Hide Blood Gauge", ref drkHide0, ref update)) TweakConfigs.DRKHide0 = drkHide0;
        HideWarning(drkHide0);
        if (Bool1("Hide Darkside Gauge", ref drkHide1, ref update)) TweakConfigs.DRKHide1 = drkHide1;
        HideWarning(drkHide1);

        if (update.HasFlag(UpdateFlags.Save)) ApplyTweaks();
    }

    public override unsafe void ApplyTweaks()
    {
        var bloodGauge = (AddonJobHudDRK0*)GameGui.GetAddonByName("JobHudDRK0");
        if (bloodGauge != null && bloodGauge->GaugeStandard.Container != null)
        {
            var drkHide0 = TweakConfigs.DRKHide0;
            var simple0 = bloodGauge->JobHud.UseSimpleGauge;
            bloodGauge->GaugeStandard.Container->Color.A = (byte)(drkHide0 || simple0 ? 0 : 255);
            bloodGauge->GaugeSimple.Container->Color.A = (byte)(drkHide0 || !simple0 ? 0 : 255);
        }

        var darkSideGauge = (AddonJobHudDRK1*)GameGui.GetAddonByName("JobHudDRK1");
        if (darkSideGauge != null && darkSideGauge->GaugeStandard.Container != null)
        {
            var drkHide1 = TweakConfigs.DRKHide1;
            var simple1 = darkSideGauge->JobHud.UseSimpleGauge;
            darkSideGauge->GaugeStandard.Container->Color.A = (byte)(drkHide1 || simple1 ? 0 : 255);
            ((AtkUnitBase*)darkSideGauge)->GetNodeById(20)->ToggleVisibility(!drkHide1 && simple1);
        }
    }
}

public partial class TweakConfigs
{
    public bool DRKHide0 { get; set; }
    public bool DRKHide1 { get; set; }
}
