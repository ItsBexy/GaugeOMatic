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

public class BLMModule : JobModule
{
    public override Job Job => BLM;
    public override Job Class => THM;
    public override Role Role => Caster;

    public override List<AddonOption> AddonOptions => new()
    {
        new("JobHudBLM0", "Elemental Gauge"),
        new("_ParameterWidget", "Parameter Bar")
    };

    public BLMModule(TrackerManager trackerManager, TrackerConfig[] trackerConfigList) : base(trackerManager, trackerConfigList, "JobHudBLM0", "JobHudBLM1") { }

    public override void Save()
    {
        Configuration.TrackerConfigs.BLM = SaveOrder;
        Configuration.Save();
    }

    public override List<MenuOption> JobGaugeMenu { get; } = new()
    {
        new("Element Status", nameof(ElementTracker)),
        new("Astral Fire", nameof(AstralFireTracker)),
        new("Umbral Ice", nameof(UmbralIceTracker)),
        new("Enochian / Polyglot", nameof(EnochianTracker)),
        new("Umbral Hearts", nameof(UmbralHeartTracker)),
        new("Paradox", nameof(ParadoxTracker)),
        new("Astral Soul Stacks", nameof(AstralSoulTracker))
    };

    public override void TweakUI(ref UpdateFlags update)
    {
        // todo: "recolor MP bar by element" tweak

        Heading("Elemental Gauge");
        ToggleControls("Hide Elemental Gauge", ref TweakConfigs.BLMHide0, ref update);

        Heading("Astral Gauge");
        ToggleControls("Hide Astral Gauge", ref TweakConfigs.BLMHide1, ref update);
    }

    public override unsafe void ApplyTweaks0(IntPtr gaugeAddon)
    {
        var gauge = (AddonJobHudBLM0*)gaugeAddon;
        VisibilityTweak(TweakConfigs.BLMHide0, ((AddonJobHud*)gauge)->UseSimpleGauge, gauge->GaugeStandard.Container, gauge->GaugeSimple.Container);
    }

    public override unsafe void ApplyTweaks1(IntPtr gaugeAddon)
    {
        var gauge = (AddonJobHudBLM1*)gaugeAddon;
        VisibilityTweak(TweakConfigs.BLMHide1, gauge->UseSimpleGauge, gauge->GaugeStandard.Container, gauge->GetNodeById(15));
    }
}

public partial class TweakConfigs
{
    public bool BLMHide0;
    public bool BLMHide1;
}
