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

    public override List<MenuOption> JobGaugeMenu { get; } = new()    {
        new("Oath Gauge",nameof(OathGaugeTracker))
    };

    public PLDModule(TrackerManager trackerManager, TrackerConfig[] trackerConfigList) : base(trackerManager, trackerConfigList) { }

    public override void Save()
    {
        Configuration.TrackerConfigs.PLD = SaveOrder;
        Configuration.Save();
    }

    public override void TweakUI(ref UpdateFlags update)
    {
        var pldHide0 = TweakConfigs.PLDHide0;

        if (Bool1("Hide Oath Gauge", ref pldHide0, ref update)) TweakConfigs.PLDHide0 = pldHide0;
        HideWarning(pldHide0);

        if (update.HasFlag(UpdateFlags.Save)) ApplyTweaks();
    }

    public override unsafe void ApplyTweaks()
    {
        var oathGauge = (AddonJobHudPLD0*)GameGui.GetAddonByName("JobHudPLD0");
        if (oathGauge != null && oathGauge->GaugeStandard.Container != null)
        {
            var pldHide0 = TweakConfigs.PLDHide0;
            var simple0 = oathGauge->JobHud.UseSimpleGauge;
            oathGauge->GaugeStandard.Container->Color.A = (byte)(pldHide0 || simple0 ? 0 : 255);
            oathGauge->GaugeSimple.Container->Color.A = (byte)(pldHide0 || !simple0 ? 0 : 255);
        }
    }
}

public partial class TweakConfigs
{
    public bool PLDHide0 { get; set; }
}
