using FFXIVClientStructs.FFXIV.Client.UI;
using GaugeOMatic.Trackers;
using GaugeOMatic.Windows.Dropdowns;
using System;
using System.Collections.Generic;
using static GaugeOMatic.GameData.JobData;
using static GaugeOMatic.GameData.JobData.Job;
using static GaugeOMatic.GameData.JobData.Role;
using static GaugeOMatic.JobModules.Tweaks;
using static GaugeOMatic.Trackers.Tracker;
using static GaugeOMatic.Widgets.WidgetUI;
using static GaugeOMatic.Windows.Dropdowns.TrackerDropdown;

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
        Heading("Aetherflow Gauge");
        ToggleControls("Hide Aetherflow Gauge", ref TweakConfigs.SMNHide0, ref update);

        Heading("Trance Gauge");
        ToggleControls("Hide Trance Gauge", ref TweakConfigs.SMNHide1, ref update);
    }

    public override unsafe void ApplyTweaks0(IntPtr gaugeAddon)
    {
        var gauge = (AddonJobHudSMN0*)gaugeAddon;
        VisibilityTweak(TweakConfigs.SMNHide0, gauge->UseSimpleGauge, gauge->GetNodeById(2), gauge->GetNodeById(11));
    }

    public override unsafe void ApplyTweaks1(IntPtr gaugeAddon)
    {
        var gauge = (AddonJobHudSMN1*)gaugeAddon;
        VisibilityTweak(TweakConfigs.SMNHide1, gauge->UseSimpleGauge, gauge->GaugeStandard.Container, gauge->GetNodeById(82));
    }
}

public partial class TweakConfigs
{
    public bool SMNHide0;
    public bool SMNHide1;
}
