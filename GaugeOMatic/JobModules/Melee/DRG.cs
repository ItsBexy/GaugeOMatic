using GaugeOMatic.Trackers;
using GaugeOMatic.Windows;
using System.Collections.Generic;
using static GaugeOMatic.GameData.JobData;
using static GaugeOMatic.GameData.JobData.Job;
using static GaugeOMatic.GameData.JobData.Role;
using static GaugeOMatic.Windows.ItemRefMenu;

namespace GaugeOMatic.JobModules;

public class DRGModule : JobModule
{
    public override Job Job => DRG;
    public override Job Class => LNC;
    public override Role Role => Melee;
    public override List<AddonOption> AddonOptions => new()
    {
        new("JobHudDRG0", "Dragon Gauge"),
        new("_ParameterWidget", "Parameter Bar")
    };

    public override List<MenuOption> JobGaugeMenu { get; } = new()
    {
        new("Firstminds' Focus", nameof(FirstmindsFocusTracker))
    };

    public DRGModule(TrackerManager trackerManager, TrackerConfig[] trackerConfigList) : base(trackerManager, trackerConfigList) { }

    public override void Save()
    {
        Configuration.TrackerConfigs.DRG = SaveOrder;
        Configuration.Save();
    }

    public override void TweakUI(ref UpdateFlags update)
    {
       /* ToggleControls("Hide Dragon Gauge", ref TweakConfigs.DRGHideAll, ref update);
        HideWarning(TweakConfigs.DRGHideAll);*/

        if (update.HasFlag(UpdateFlags.Save)) ApplyTweaks();
    }

    public override void ApplyTweaks()
    {
      /*  var dragonGauge = (AddonJobHudDRG0*)GameGui.GetAddonByName("JobHudDRG0");
        if (dragonGauge != null && dragonGauge->GaugeStandard.Container != null)
        {
            var hideAll = TweakConfigs.DRGHideAll;
            var simple = dragonGauge->AddonJobHud.UseSimpleGauge;
            dragonGauge->GaugeStandard.Container->Color.A = (byte)(hideAll || simple ? 0 : 255);
            dragonGauge->GaugeSimple.Container->Color.A = (byte)(hideAll || !simple ? 0 : 255);
        }*/
    }
}

public partial class TweakConfigs
{
   // public bool DRGHideAll;
}
