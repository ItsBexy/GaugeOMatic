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

    public BLMModule(TrackerManager trackerManager, TrackerConfig[] trackerConfigList) : base(trackerManager, trackerConfigList) { }

    public override void Save()
    {
        Configuration.TrackerConfigs.BLM = SaveOrder;
        Configuration.Save();
    }

    public override List<MenuOption> JobGaugeMenu { get; } = new()
    {
        new("Element Status",nameof(ElementTracker)),
        new("Astral Fire",nameof(AstralFireTracker)),
        new("Umbral Ice",nameof(UmbralIceTracker)),
        new("Enochian / Polyglot",nameof(EnochianTracker)),
        new("Umbral Hearts",nameof(UmbralHeartTracker)),
        new("Paradox",nameof(ParadoxTracker))
    };

    public override void TweakUI(ref UpdateFlags update)
    {
        // todo: "recolor MP bar by element" tweak

        ToggleControls("Hide Elemental Gauge",ref TweakConfigs.BLMHideAll, ref update);
        HideWarning(TweakConfigs.BLMHideAll);

        if (update.HasFlag(UpdateFlags.Save)) ApplyTweaks();
    }

    public override unsafe void ApplyTweaks()
    {
        var elementalGauge = (AddonJobHudBLM0*)GameGui.GetAddonByName("JobHudBLM0");
        if (elementalGauge != null && elementalGauge->GaugeStandard.Container != null)
        {
            var hideAll = TweakConfigs.BLMHideAll;
            var simple = elementalGauge->JobHud.UseSimpleGauge;
            elementalGauge->GaugeStandard.Container->Color.A = (byte)(hideAll || simple ? 0 : 255);
            elementalGauge->GaugeSimple.Container->Color.A = (byte)(hideAll || !simple ? 0 : 255);
        }
    }
}

public partial class TweakConfigs
{
    public bool BLMHideAll;
}
