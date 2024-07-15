using GaugeOMatic.Trackers;
using GaugeOMatic.Windows;
using System.Collections.Generic;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;
using GaugeOMatic.Widgets;
using static GaugeOMatic.GameData.JobData;
using static GaugeOMatic.GameData.JobData.Job;
using static GaugeOMatic.GameData.JobData.Role;
using static GaugeOMatic.JobModules.TweakUI;
using static GaugeOMatic.Windows.ItemRefMenu;

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
        new("Paradox", nameof(ParadoxTracker))
    };

    public override void TweakUI(ref UpdateFlags update)
    {
        // todo: "recolor MP bar by element" tweak

        WidgetUI.ToggleControls("Hide Elemental Gauge", ref TweakConfigs.BLMHide0, ref update);
        HideWarning(TweakConfigs.BLMHide0);
        WidgetUI.ToggleControls("Hide Astral Gauge", ref TweakConfigs.BLMHide1, ref update);
        HideWarning(TweakConfigs.BLMHide1);

        if (update.HasFlag(UpdateFlags.Save))
        {
            ApplyTweaks0();
            ApplyTweaks1();
        }
    }

    public override unsafe void ApplyTweaks0()
    {
        var elementalGauge = (AddonJobHudBLM0*)GameGui.GetAddonByName("JobHudBLM0");
        if (elementalGauge != null && elementalGauge->GaugeStandard.Container != null)
        {
            var hide0 = TweakConfigs.BLMHide0;
            var simple = ((AddonJobHud*)elementalGauge)->UseSimpleGauge;
            elementalGauge->GaugeStandard.Container->Color.A = (byte)(hide0 || simple ? 0 : 255);
            elementalGauge->GaugeSimple.Container->Color.A = (byte)(hide0 || !simple ? 0 : 255);
        }
    }

    public override unsafe void ApplyTweaks1()
    {
        var astralGauge = (AddonJobHudBLM1*)GameGui.GetAddonByName("JobHudBLM1");
        if (astralGauge != null && astralGauge->GaugeStandard.Container != null)
        {
            var hide1 = TweakConfigs.BLMHide1;
            var simple = ((AddonJobHud*)astralGauge)->UseSimpleGauge;
            astralGauge->GaugeStandard.Container->Color.A = (byte)(hide1 || simple ? 0 : 255);
            ((AtkUnitBase*)astralGauge)->GetNodeById(15)->SetAlpha((byte)(hide1 || !simple ? 0 : 255));
        }
    }
}

public partial class TweakConfigs
{
    public bool BLMHide0;
    public bool BLMHide1;
}
