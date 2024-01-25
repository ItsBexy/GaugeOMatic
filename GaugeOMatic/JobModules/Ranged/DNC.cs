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
        new("Fourfold Feathers",nameof(FourfoldTracker)),
        new("Esprit Gauge",nameof(EspritGaugeTracker)),
        new("Dance Steps",nameof(DanceStepTracker))
    };

    public DNCModule(TrackerManager trackerManager, TrackerConfig[] trackerConfigList) : base(trackerManager, trackerConfigList) { }

    public override void Save()
    {
        Configuration.TrackerConfigs.DNC = SaveOrder;
        Configuration.Save();
    }


    public override void TweakUI(ref UpdateFlags update)
    {
        ToggleControls("Hide Fourfold Feathers",ref TweakConfigs.DNCHideFeathers, ref update);
        HideWarning(TweakConfigs.DNCHideFeathers);

        if (update.HasFlag(UpdateFlags.Save)) ApplyTweaks();
    }

    public override unsafe void ApplyTweaks()
    {
        var fourfoldFeathers = (AddonJobHudDNC1*)GameGui.GetAddonByName("JobHudDNC1");
        if (fourfoldFeathers != null && fourfoldFeathers->GaugeStandard.Container != null)
        {
            var hideFeathers = TweakConfigs.DNCHideFeathers;
            var simple1 = fourfoldFeathers->JobHud.UseSimpleGauge;
            fourfoldFeathers->GaugeStandard.Container->Color.A = (byte)(hideFeathers || simple1 ? 0 : 255);
            fourfoldFeathers->GaugeSimple.Container->Color.A = (byte)(hideFeathers || !simple1 ? 0 : 255);
        }
    }
}

public partial class TweakConfigs
{
    public bool DNCHideFeathers;
}
