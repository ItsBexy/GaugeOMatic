using GaugeOMatic.Trackers;
using GaugeOMatic.Windows;
using System.Collections.Generic;
using FFXIVClientStructs.FFXIV.Client.UI;
using static GaugeOMatic.GameData.JobData;
using static GaugeOMatic.GameData.JobData.Job;
using static GaugeOMatic.GameData.JobData.Role;
using static GaugeOMatic.Windows.ItemRefMenu;
using static GaugeOMatic.JobModules.TweakUI;
using static GaugeOMatic.Widgets.WidgetUI;

namespace GaugeOMatic.JobModules;

public class DNCModule : JobModule
{
    public override Job Job => DNC;
    public override Job Class => Job.None;
    public override Role Role => Ranged;
    public override List<AddonOption> AddonOptions => new()
    {
        new("JobHudDNC0", "Step Gauge"),
        new("JobHudDNC1", "Fourfold Feathers"),
        new("_ParameterWidget", "Parameter Bar")
    };

    public override List<MenuOption> JobGaugeMenu { get; } = new()
    {
        new("Fourfold Feathers", nameof(FourfoldTracker)),
        new("Esprit Gauge", nameof(EspritGaugeTracker)),
        new("Dance Steps", nameof(DanceStepTracker))
    };

    public DNCModule(TrackerManager trackerManager, TrackerConfig[] trackerConfigList) : base(trackerManager, trackerConfigList, "JobHudDNC0", "JobHudDNC1") { }

    public override void Save()
    {
        Configuration.TrackerConfigs.DNC = SaveOrder;
        Configuration.Save();
    }

    public override void TweakUI(ref UpdateFlags update)
    {
        ToggleControls("Hide Step Gauge", ref TweakConfigs.DNCHide0, ref update);
        HideWarning(TweakConfigs.DNCHide0);

        ToggleControls("Hide Fourfold Feathers", ref TweakConfigs.DNCHide1, ref update);
        HideWarning(TweakConfigs.DNCHide1);

        if (update.HasFlag(UpdateFlags.Save))
        {
            ApplyTweaks0();
            ApplyTweaks1();
        }
    }

    public override unsafe void ApplyTweaks0()
    {
        var stepGauge = (AddonJobHudDNC0*)GameGui.GetAddonByName("JobHudDNC0");
        if (stepGauge != null && stepGauge->GaugeStandard.Container != null)
        {
            var hideFeathers = TweakConfigs.DNCHide0;
            var simple0 = ((AddonJobHud*)stepGauge)->UseSimpleGauge;
            stepGauge->GaugeStandard.Container->Color.A = (byte)(hideFeathers || simple0 ? 0 : 255);
            stepGauge->GaugeSimple.Container->Color.A = (byte)(hideFeathers || !simple0 ? 0 : 255);
        }
    }

    public override unsafe void ApplyTweaks1()
    {
        var fourfoldFeathers = (AddonJobHudDNC1*)GameGui.GetAddonByName("JobHudDNC1");
        if (fourfoldFeathers != null && fourfoldFeathers->GaugeStandard.Container != null)
        {
            var hideFeathers = TweakConfigs.DNCHide1;
            var simple1 = ((AddonJobHud*)fourfoldFeathers)->UseSimpleGauge;
            fourfoldFeathers->GaugeStandard.Container->Color.A = (byte)(hideFeathers || simple1 ? 0 : 255);
            fourfoldFeathers->GaugeSimple.Container->Color.A = (byte)(hideFeathers || !simple1 ? 0 : 255);
        }
    }
}

public partial class TweakConfigs
{
    public bool DNCHide0;
    public bool DNCHide1;
}
