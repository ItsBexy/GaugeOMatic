using FFXIVClientStructs.FFXIV.Client.UI;
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

public class SAMModule : JobModule
{
    public override Job Job => SAM;
    public override Job Class => Job.None;
    public override Role Role => Melee;
    public override List<AddonOption> AddonOptions => new()
    {
        new("JobHudSAM0", "Kenki Gauge"),
        new("JobHudSAM1", "Sen Gauge"),
        new("_ParameterWidget", "Parameter Bar")
    };

    public override List<MenuOption> JobGaugeMenu { get; } = new(){
        new("Kenki Gauge",nameof(KenkiGaugeTracker)),
        new("Meditation Gauge",nameof(MeditationGaugeTracker)),
        new("Sen Gauge - Seal Count",nameof(SenSealTracker)),
        new("Sen Gauge - Setsu Seal",nameof(SenGaugeSetsuTracker)),
        new("Sen Gauge - Getsu Seal",nameof(SenGaugeGetsuTracker)),
        new("Sen Gauge - Ka Seal",nameof(SenGaugeKaTracker))
    };
        
    public SAMModule(TrackerManager trackerManager, TrackerConfig[] trackerConfigList) : base(trackerManager, trackerConfigList) { }

    public override void Save()
    {
        Configuration.TrackerConfigs.SAM = SaveOrder;
        Configuration.Save();
    }

    public override void TweakUI(ref UpdateFlags update)
    {
        var hideKenki = TweakConfigs.SAMHideKenki;
        var hideMeditation = TweakConfigs.SAMHideMeditation;
        var hideSen = TweakConfigs.SAMHideSen;

        if (Bool1("Hide Kenki Gauge", ref hideKenki, ref update)) TweakConfigs.SAMHideKenki = hideKenki;
        HideWarning(hideKenki);
        if (Bool1("Hide Meditation Gauge", ref hideMeditation, ref update)) TweakConfigs.SAMHideMeditation = hideMeditation;
        HideWarning(hideMeditation);
        if (Bool1("Hide Sen Gauge", ref hideSen, ref update)) TweakConfigs.SAMHideSen = hideSen;
        HideWarning(hideSen);

        if (update.HasFlag(UpdateFlags.Save)) ApplyTweaks();
    }

    public override unsafe void ApplyTweaks()
    {
        var kenkiGauge = (AddonJobHudSAM0*)GameGui.GetAddonByName("JobHudSAM0");
        if (kenkiGauge != null && kenkiGauge->GaugeStandard.KenkiContainer != null)
        {
            var simple0 = kenkiGauge->JobHud.UseSimpleGauge;
            var hideKenki = TweakConfigs.SAMHideKenki;
            var hideMeditation = TweakConfigs.SAMHideMeditation;

            kenkiGauge->GaugeStandard.KenkiContainer->Color.A = (byte)(hideKenki || simple0 ? 0 : 255);
            kenkiGauge->GaugeStandard.MeditationContainer->Color.A = (byte)(hideMeditation || simple0 ? 0 : 255);
                
            kenkiGauge->GaugeSimple.KenkiContainer->Color.A = (byte)(hideKenki || !simple0 ? 0 : 255);
            kenkiGauge->GaugeSimple.MeditationContainer->Color.A = (byte)(hideMeditation || !simple0 ? 0 : 255);
        }

        var senGauge = (AddonJobHudSAM1*)GameGui.GetAddonByName("JobHudSAM1");
        if (senGauge != null && senGauge->GaugeStandard.Container != null)
        {
            var hideSen = TweakConfigs.SAMHideSen;
            var simple1 = senGauge->JobHud.UseSimpleGauge;
            senGauge->GaugeStandard.Container->ToggleVisibility(!hideSen && !simple1);
            senGauge->GaugeSimple.Container->Color.A = (byte)(hideSen || !simple1 ? 0 : 255);
        }
    }
}

public partial class TweakConfigs
{
    public bool SAMHideKenki { get; set; }
    public bool SAMHideMeditation { get; set; }
    public bool SAMHideSen { get; set; }
}
