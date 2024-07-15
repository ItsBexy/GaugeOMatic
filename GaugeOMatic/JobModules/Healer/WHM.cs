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
        new("Lilies", nameof(LilyTracker)),
        new("Blood Lily", nameof(BloodLilyTracker))
    };

    public WHMModule(TrackerManager trackerManager, TrackerConfig[] trackerConfigList) : base(trackerManager, trackerConfigList, "JobHudWHM0") { }

    public override void Save()
    {
        Configuration.TrackerConfigs.WHM = SaveOrder;
        Configuration.Save();
    }

    public override void TweakUI(ref UpdateFlags update)
    {
        ToggleControls("Hide Healing Gauge", ref TweakConfigs.WHMHide0, ref update);
        HideWarning(TweakConfigs.WHMHide0);

        if (update.HasFlag(UpdateFlags.Save)) ApplyTweaks0();
    }

    public override unsafe void ApplyTweaks0()
    {
        var healingGauge = (AddonJobHudWHM0*)GameGui.GetAddonByName("JobHudWHM0");
        if (healingGauge != null && healingGauge->GaugeStandard.Container != null)
        {
            var hide0 = TweakConfigs.WHMHide0;
            var simple = ((AddonJobHud*)healingGauge)->UseSimpleGauge;
            healingGauge->GaugeStandard.Container->Color.A = (byte)(hide0 || simple ? 0 : 255);
            healingGauge->GaugeSimple.Container->Color.A = (byte)(hide0 || !simple ? 0 : 255);
        }
    }
}

public partial class TweakConfigs
{
    public bool WHMHide0;
}
