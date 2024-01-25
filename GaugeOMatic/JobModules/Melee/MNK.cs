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

public class MNKModule : JobModule
{
    public override Job Job => MNK;
    public override Job Class => PGL;
    public override Role Role => Melee;
    public override List<AddonOption> AddonOptions => new()
    {
        new("JobHudMNK0", "Master's Gauge"),
        new("JobHudMNK1", "Chakra Gauge"),
        new("_ParameterWidget", "Parameter Bar")
    };

    public override List<MenuOption> JobGaugeMenu { get; } = new()
    {
        new("Chakra Gauge",nameof(ChakraGaugeTracker))
    };

    public MNKModule(TrackerManager trackerManager, TrackerConfig[] trackerConfigList) : base(trackerManager, trackerConfigList) { }

    public override void Save()
    {
        Configuration.TrackerConfigs.MNK = SaveOrder;
        Configuration.Save();
    }

    public override void TweakUI(ref UpdateFlags update)
    {
        ToggleControls("Hide Chakra Gauge",ref TweakConfigs.MNKHideChakra, ref update);
        HideWarning(TweakConfigs.MNKHideChakra);

        if (update.HasFlag(UpdateFlags.Save)) ApplyTweaks();
    }

    public override unsafe void ApplyTweaks()
    {
        var chakraGauge = (AddonJobHudMNK1*)GameGui.GetAddonByName("JobHudMNK1");
        if (chakraGauge != null && chakraGauge->GaugeStandard.Container != null)
        {
            var hideChakra = TweakConfigs.MNKHideChakra;
            var simple1 = chakraGauge->JobHud.UseSimpleGauge;
            chakraGauge->GaugeStandard.Container->ToggleVisibility(!hideChakra && !simple1);
            chakraGauge->GaugeSimple.Container->Color.A = (byte)(hideChakra || !simple1 ? 0 : 255);
        }
    }
}

public partial class TweakConfigs
{
    public bool MNKHideChakra;
}
