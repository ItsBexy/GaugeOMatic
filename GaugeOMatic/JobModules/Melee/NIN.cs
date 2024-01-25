using FFXIVClientStructs.FFXIV.Client.UI;
using GaugeOMatic.Trackers;
using GaugeOMatic.Windows;
using System.Collections.Generic;
using static GaugeOMatic.GameData.JobData;
using static GaugeOMatic.GameData.JobData.Job;
using static GaugeOMatic.GameData.JobData.Role;
using static GaugeOMatic.JobModules.TweakUI;
using static GaugeOMatic.Widgets.WidgetUI;
using static GaugeOMatic.Windows.ItemRefMenu;

namespace GaugeOMatic.JobModules;

public class NINModule : JobModule
{
    public override Job Job => NIN;
    public override Job Class => ROG;
    public override Role Role => Melee;
    public override List<AddonOption> AddonOptions => new()
    {
        new("JobHudNIN0", "Ninki Gauge"),
        new("JobHudNIN1", "Huton Gauge"),
        new("_ParameterWidget", "Parameter Bar")
    };

    public override List<MenuOption> JobGaugeMenu { get; } = new() {
        new("Ninki Gauge",nameof(NinkiGaugeTracker)),
        new("Huton Gauge",nameof(HutonGaugeTracker))
    };

    public NINModule(TrackerManager trackerManager, TrackerConfig[] trackerConfigList) : base(trackerManager, trackerConfigList) { }

    public override void Save()
    {
        Configuration.TrackerConfigs.NIN = SaveOrder;
        Configuration.Save();
    }

    public override void TweakUI(ref UpdateFlags update)
    {
        ToggleControls("Hide Ninki Gauge",ref TweakConfigs.NINHideNinki, ref update);
        HideWarning(TweakConfigs.NINHideNinki);
        
        ToggleControls("Hide Huton Gauge",ref TweakConfigs.NINHideHuton, ref update);
        HideWarning(TweakConfigs.NINHideHuton);

        if (update.HasFlag(UpdateFlags.Save)) ApplyTweaks();
    }

    public override unsafe void ApplyTweaks()
    {
        var ninkiGauge = (AddonJobHudNIN0*)GameGui.GetAddonByName("JobHudNIN0");
        if (ninkiGauge != null && ninkiGauge->GaugeStandard.Container != null)
        {
            var hideNinki = TweakConfigs.NINHideNinki;
            var simple0 = ninkiGauge->JobHud.UseSimpleGauge;
            ninkiGauge->GaugeStandard.Container->Color.A = (byte)(hideNinki || simple0 ? 0 : 255);
            ninkiGauge->GaugeSimple.Container->Color.A = (byte)(hideNinki || !simple0 ? 0 : 255);
        }

        var hutonGauge = (AddonJobHudNIN1*)GameGui.GetAddonByName("JobHudNIN1");
        if (hutonGauge != null && hutonGauge->GaugeStandard.Container != null)
        {
            var hideHuton = TweakConfigs.NINHideHuton;
            var simple1 = hutonGauge->JobHud.UseSimpleGauge;
            hutonGauge->GaugeStandard.Container->ToggleVisibility(!hideHuton && !simple1);
            hutonGauge->GaugeSimple.Container->Color.A = (byte)(hideHuton || !simple1 ? 0 : 255);
        }
    }
}

public partial class TweakConfigs
{
    public bool NINHideNinki;
    public bool NINHideHuton;
}
