using GaugeOMatic.Trackers;
using GaugeOMatic.Windows;
using System.Collections.Generic;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;
using static GaugeOMatic.GameData.JobData;
using static GaugeOMatic.GameData.JobData.Job;
using static GaugeOMatic.GameData.JobData.Role;
using static GaugeOMatic.Windows.ItemRefMenu;
using static GaugeOMatic.JobModules.TweakUI;
using static GaugeOMatic.Widgets.WidgetUI;

namespace GaugeOMatic.JobModules;

public class GNBModule : JobModule
{
    public override Job Job => GNB;
    public override Job Class => Job.None;
    public override Role Role => Tank;
    public override List<AddonOption> AddonOptions => new()
    {
        new("JobHudGNB0", "Powder Gauge"),
        new("_ParameterWidget", "Parameter Bar")
    };

    public override List<MenuOption> JobGaugeMenu { get; } = new()
    {
        new("Powder Gauge", nameof(PowderGaugeTracker))
    };

    public GNBModule(TrackerManager trackerManager, TrackerConfig[] trackerConfigList) : base(trackerManager, trackerConfigList, "JobHudGNB0") { }

    public override void Save()
    {
        Configuration.TrackerConfigs.GNB = SaveOrder;
        Configuration.Save();
    }

    public override void TweakUI(ref UpdateFlags update)
    {
        ToggleControls("Hide Powder Gauge", ref TweakConfigs.GNBHide0, ref update);
        HideWarning(TweakConfigs.GNBHide0);

        if (update.HasFlag(UpdateFlags.Save)) ApplyTweaks0();
    }

    public override unsafe void ApplyTweaks0()
    {
        var powderGauge = (AddonJobHudGNB0*)GameGui.GetAddonByName("JobHudGNB0");
        if (powderGauge != null && powderGauge->GaugeStandard.Container != null)
        {
            var gnbHide0 = TweakConfigs.GNBHide0;
            var simple0 = ((AddonJobHud*)powderGauge)->UseSimpleGauge;
            powderGauge->GaugeStandard.Container->Color.A = (byte)(gnbHide0 || simple0 ? 0 : 255);
            ((AtkUnitBase*)powderGauge)->GetNodeById(19)->ToggleVisibility(!gnbHide0 && simple0);
        }
    }
}

public partial class TweakConfigs
{
    public bool GNBHide0;
}
