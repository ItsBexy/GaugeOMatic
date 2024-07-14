using GaugeOMatic.Trackers;
using GaugeOMatic.Windows;
using System.Collections.Generic;
using static GaugeOMatic.GameData.JobData;
using static GaugeOMatic.GameData.JobData.Job;
using static GaugeOMatic.GameData.JobData.Role;
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
        new("Blood Gauge", nameof(BloodGaugeTracker)),
        new("Darkside Gauge", nameof(DarksideGaugeTracker)),
        new("Living Shadow", nameof(LivingShadowTracker))
    };

    public DRKModule(TrackerManager trackerManager, TrackerConfig[] trackerConfigList) : base(trackerManager, trackerConfigList) { }

    public override void Save()
    {
        Configuration.TrackerConfigs.DRK = SaveOrder;
        Configuration.Save();
    }

    public override void TweakUI(ref UpdateFlags update)
    {
       /* ToggleControls("Hide Blood Gauge", ref TweakConfigs.DRKHide0, ref update);
        HideWarning(TweakConfigs.DRKHide0);
        ToggleControls("Hide Darkside Gauge", ref TweakConfigs.DRKHide1, ref update);
        HideWarning(TweakConfigs.DRKHide1);*/

        if (update.HasFlag(UpdateFlags.Save)) ApplyTweaks();
    }

    public override void ApplyTweaks()
    {
        /*var bloodGauge = (AddonJobHudDRK0*)GameGui.GetAddonByName("JobHudDRK0");
        if (bloodGauge != null && bloodGauge->GaugeStandard.Container != null)
        {
            var drkHide0 = TweakConfigs.DRKHide0;
            var simple0 = bloodGauge->AddonJobHud.UseSimpleGauge;
            bloodGauge->GaugeStandard.Container->Color.A = (byte)(drkHide0 || simple0 ? 0 : 255);
            bloodGauge->GaugeSimple.Container->Color.A = (byte)(drkHide0 || !simple0 ? 0 : 255);
        }

        var darkSideGauge = (AddonJobHudDRK1*)GameGui.GetAddonByName("JobHudDRK1");
        if (darkSideGauge != null && darkSideGauge->GaugeStandard.Container != null)
        {
            var drkHide1 = TweakConfigs.DRKHide1;
            var simple1 = darkSideGauge->AddonJobHud.UseSimpleGauge;
            darkSideGauge->GaugeStandard.Container->Color.A = (byte)(drkHide1 || simple1 ? 0 : 255);
            ((AtkUnitBase*)darkSideGauge)->GetNodeById(20)->ToggleVisibility(!drkHide1 && simple1);
        }*/
    }
}

public partial class TweakConfigs
{
 /*   public bool DRKHide0;
    public bool DRKHide1;*/
}
