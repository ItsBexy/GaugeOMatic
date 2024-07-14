using GaugeOMatic.Trackers;
using GaugeOMatic.Windows;
using System.Collections.Generic;
using static GaugeOMatic.GameData.JobData;
using static GaugeOMatic.GameData.JobData.Job;
using static GaugeOMatic.GameData.JobData.Role;
using static GaugeOMatic.Windows.ItemRefMenu;

namespace GaugeOMatic.JobModules;

public class PLDModule : JobModule
{
    public override Job Job => PLD;
    public override Job Class => GLA;
    public override Role Role => Tank;
    public override List<AddonOption> AddonOptions => new()
    {
        new("JobHudPLD0", "Oath Gauge"),
        new("_ParameterWidget", "Parameter Bar")
    };

    public override List<MenuOption> JobGaugeMenu { get; } = new() {
        new("Oath Gauge", nameof(OathGaugeTracker))
    };

    public PLDModule(TrackerManager trackerManager, TrackerConfig[] trackerConfigList) : base(trackerManager, trackerConfigList) { }

    public override void Save()
    {
        Configuration.TrackerConfigs.PLD = SaveOrder;
        Configuration.Save();
    }

    public override void TweakUI(ref UpdateFlags update)
    {
        /*ToggleControls("Hide Oath Gauge", ref TweakConfigs.PLDHide0, ref update);
        HideWarning(TweakConfigs.PLDHide0);*/

        if (update.HasFlag(UpdateFlags.Save)) ApplyTweaks();
    }

    public override void ApplyTweaks()
    {
       /* var oathGauge = (AddonJobHudPLD0*)GameGui.GetAddonByName("JobHudPLD0");
        if (oathGauge != null && oathGauge->GaugeStandard.Container != null)
        {
            var pldHide0 = TweakConfigs.PLDHide0;
            var simple0 = oathGauge->AddonJobHud.UseSimpleGauge;
            oathGauge->GaugeStandard.Container->Color.A = (byte)(pldHide0 || simple0 ? 0 : 255);
            oathGauge->GaugeSimple.Container->Color.A = (byte)(pldHide0 || !simple0 ? 0 : 255);
        }*/
    }
}

public partial class TweakConfigs
{
   // public bool PLDHide0;
}
