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

public class SMNModule : JobModule
{
    public sealed override Job Job => SMN;
    public override Job Class => ACN;
    public override Role Role => Caster;
    public override List<AddonOption> AddonOptions => new()
    {
        new("JobHudSMN0", "Aetherflow Gauge"),
        new("JobHudSMN1", "Trance Gauge"),
        new("_ParameterWidget", "Parameter Bar")
    };

    public override List<MenuOption> JobGaugeMenu => new()
    {
        new("Aetherflow Gauge", nameof(AetherflowSMNGaugeTracker))
    };

    public SMNModule(TrackerManager trackerManager, TrackerConfig[] trackerConfigList) : base(trackerManager, trackerConfigList, "JobHudSMN0", "JobHudSMN1") { }

    public override void Save()
    {
        Configuration.TrackerConfigs.SMN = SaveOrder;
        Configuration.Save();
    }

    public override void TweakUI(ref UpdateFlags update)
    {
        ToggleControls("Hide Aetherflow Gauge", ref TweakConfigs.SMNHide0, ref update);
        HideWarning(TweakConfigs.SMNHide0);
        ToggleControls("Hide Trance Gauge", ref TweakConfigs.SMNHide1, ref update);
        HideWarning(TweakConfigs.SMNHide1);

        if (update.HasFlag(UpdateFlags.Save))
        {
            ApplyTweaks0();
            ApplyTweaks1();
        }
    }

    public override unsafe void ApplyTweaks0()
    {
        var aetherflowGauge = (AddonJobHudSMN0*)GameGui.GetAddonByName("JobHudSMN0");
        if (aetherflowGauge != null && aetherflowGauge->GaugeStandard.Stack1 != null)
        {
            var hideAetherflow = TweakConfigs.SMNHide0;
            var simple0 = ((AddonJobHud*)aetherflowGauge)->UseSimpleGauge;
            ((AtkUnitBase*)aetherflowGauge)->GetNodeById(2)->ToggleVisibility(!hideAetherflow && !simple0);
            ((AtkUnitBase*)aetherflowGauge)->GetNodeById(11)->ToggleVisibility(!hideAetherflow && simple0);
        }
    }

    public override unsafe void ApplyTweaks1()
    {
        var tranceGauge = (AddonJobHudSMN1*)GameGui.GetAddonByName("JobHudSMN1");
        if (tranceGauge != null)
        {
            var hideTrance = TweakConfigs.SMNHide1;
            var simple1 = ((AddonJobHud*)tranceGauge)->UseSimpleGauge;
            tranceGauge->GaugeStandard.Container->ToggleVisibility(!hideTrance && !simple1);
            ((AtkUnitBase*)tranceGauge)->GetNodeById(82)->ToggleVisibility(!hideTrance && simple1);
        }
    }
}

public partial class TweakConfigs
{
    public bool SMNHide0;
    public bool SMNHide1;
}
