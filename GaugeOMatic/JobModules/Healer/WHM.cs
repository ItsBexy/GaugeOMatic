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

public class WHMModule : JobModule
{
    public override Job Job => WHM;
    public override Job Class => CNJ;
    public override Role Role => Healer;
    public override List<AddonOption> AddonOptions => new()
    {
        new("JobHudWHM0", "Healing Gauge"),
        new("_ParameterWidget", "Parameter Bar")
    };

    public override List<MenuOption> JobGaugeMenu => new()
    {
        new("Lilies",nameof(LilyTracker)),
        new("Blood Lily",nameof(BloodLilyTracker))
    };

    public WHMModule(TrackerManager trackerManager, TrackerConfig[] trackerConfigList) : base(trackerManager, trackerConfigList) { }

    public override void Save()
    {
        Configuration.TrackerConfigs.WHM = SaveOrder;
        Configuration.Save();
    }

    public override void TweakUI(ref UpdateFlags update)
    {
        var hideAll = TweakConfigs.WHMHideAll;

        if (Bool1("Hide Healing Gauge", ref hideAll, ref update)) TweakConfigs.WHMHideAll = hideAll;
        HideWarning(hideAll);

        if (update.HasFlag(UpdateFlags.Save)) ApplyTweaks();
    }

    public override unsafe void ApplyTweaks()
    {
        var healingGauge = (AddonJobHudWHM0*)GameGui.GetAddonByName("JobHudWHM0");
        if (healingGauge != null && healingGauge->GaugeStandard.Container != null)
        {
            var hideAll = TweakConfigs.WHMHideAll;
            var simple = healingGauge->JobHud.UseSimpleGauge;
            healingGauge->GaugeStandard.Container->Color.A = (byte)(hideAll || simple ? 0 : 255);
            healingGauge->GaugeSimple.Container->Color.A = (byte)(hideAll || !simple ? 0 : 255);
        }
    }
}

public partial class TweakConfigs
{
    public bool WHMHideAll { get; set; }
}
