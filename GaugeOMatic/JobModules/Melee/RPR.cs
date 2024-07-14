using GaugeOMatic.Trackers;
using GaugeOMatic.Windows;
using System.Collections.Generic;
using static GaugeOMatic.GameData.JobData;
using static GaugeOMatic.GameData.JobData.Job;
using static GaugeOMatic.GameData.JobData.Role;
using static GaugeOMatic.Windows.ItemRefMenu;

namespace GaugeOMatic.JobModules;

public class RPRModule : JobModule
{
    public override Job Job => RPR;
    public override Job Class => Job.None;
    public override Role Role => Melee;
    public override List<AddonOption> AddonOptions => new()
    {
        new("JobHudRRP0", "Soul Gauge"),
        new("JobHudRRP1", "Death Gauge"),
        new("_ParameterWidget", "Parameter Bar")
    };

    public override List<MenuOption> JobGaugeMenu { get; } = new() {
        new("Soul Gauge", nameof(SoulGaugeTracker)),
        new("Shroud Gauge", nameof(ShroudGaugeTracker)),
        new("Lemure Shroud", nameof(LemureShroudTracker)),
        new("Void Shroud", nameof(VoidShroudTracker))
    };

    public RPRModule(TrackerManager trackerManager, TrackerConfig[] trackerConfigList) : base(trackerManager, trackerConfigList) { }

    public override void Save()
    {
        Configuration.TrackerConfigs.RPR = SaveOrder;
        Configuration.Save();
    }


    public override void TweakUI(ref UpdateFlags update)
    {
       /* ToggleControls("Hide Soul Gauge", ref TweakConfigs.RPRHideSoul, ref update);
        HideWarning(TweakConfigs.RPRHideSoul);
        ToggleControls("Hide Death Gauge", ref TweakConfigs.RPRHideDeath, ref update);
        HideWarning(TweakConfigs.RPRHideDeath);*/

        if (update.HasFlag(UpdateFlags.Save)) ApplyTweaks();
    }

    public override void ApplyTweaks()
    {
        /*var soulGauge = (AddonJobHudRRP0*)GameGui.GetAddonByName("JobHudRRP0");
        if (soulGauge != null && soulGauge->GaugeStandard.Container != null)
        {
            var hideSoul = TweakConfigs.RPRHideSoul;
            var simple0 = soulGauge->AddonJobHud.UseSimpleGauge;
            soulGauge->GaugeStandard.Container->Color.A = (byte)(hideSoul || simple0 ? 0 : 255);
            soulGauge->GaugeSimple.Container->Color.A = (byte)(hideSoul || !simple0 ? 0 : 255);
        }

        var deathGauge = (AddonJobHudRRP1*)GameGui.GetAddonByName("JobHudRRP1");
        if (deathGauge != null && deathGauge->GaugeStandard.Container != null)
        {
            var hideDeath = TweakConfigs.RPRHideDeath;
            var simple1 = deathGauge->AddonJobHud.UseSimpleGauge;
            deathGauge->GaugeStandard.Container->Color.A = (byte)(hideDeath || simple1 ? 0 : 255);
            deathGauge->GaugeSimple.Container->Color.A = (byte)(hideDeath || !simple1 ? 0 : 255);
        }*/
    }
}

public partial class TweakConfigs
{
  //  public bool RPRHideSoul;
  //  public bool RPRHideDeath;
}
