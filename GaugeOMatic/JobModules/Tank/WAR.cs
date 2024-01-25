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

public class WARModule : JobModule
{
    public override Job Job => WAR;
    public override Job Class => MRD;
    public override Role Role => Tank;
    public override List<AddonOption> AddonOptions => new()
    {
        new("JobHudWAR0", "Beast Gauge"),
        new("_ParameterWidget", "Parameter Bar")
    };

    public override List<MenuOption> JobGaugeMenu { get; } = new()    {
        new("Beast Gauge",nameof(BeastGaugeTracker))
    };

    public WARModule(TrackerManager trackerManager, TrackerConfig[] trackerConfigList) : base(trackerManager, trackerConfigList) { }

    public override void Save()
    {
        Configuration.TrackerConfigs.WAR = SaveOrder;
        Configuration.Save();
    }

    public override void TweakUI(ref UpdateFlags update)
    {
        ToggleControls("Hide Beast Gauge",ref TweakConfigs.WARHide0, ref update);
        HideWarning(TweakConfigs.WARHide0);

        if (update.HasFlag(UpdateFlags.Save)) ApplyTweaks();
    }

    public override unsafe void ApplyTweaks()
    {
        var beastGauge = (AddonJobHudWAR0*)GameGui.GetAddonByName("JobHudWAR0");
        if (beastGauge != null && beastGauge->GaugeStandard.Container != null)
        {
            var warHide0 = TweakConfigs.WARHide0;
            var simple0 = beastGauge->JobHud.UseSimpleGauge;
            beastGauge->GaugeStandard.Container->Color.A = (byte)(warHide0 || simple0 ? 0 : 255);
            beastGauge->GaugeSimple.BarContainer->Color.A = (byte)(warHide0 || !simple0 ? 0 : 255);
        }
    }
}

public partial class TweakConfigs
{
    public bool WARHide0;
}
