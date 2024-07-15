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

public class MNKModule : JobModule
{
    public override Job Job => MNK;
    public override Job Class => PGL;
    public override Role Role => Melee;
    public override List<AddonOption> AddonOptions => new()
    {
        new("JobHudMNK0", "Beast Chakra Gauge"),
        new("JobHudMNK1", "Chakra Gauge"),
        new("_ParameterWidget", "Parameter Bar")
    };

    public override List<MenuOption> JobGaugeMenu { get; } = new()
    {
        new("Chakra Gauge", nameof(ChakraGaugeTracker))
    };

    public MNKModule(TrackerManager trackerManager, TrackerConfig[] trackerConfigList) : base(trackerManager, trackerConfigList, "JobHudMNK0", "JobHudMNK1") { }

    public override void Save()
    {
        Configuration.TrackerConfigs.MNK = SaveOrder;
        Configuration.Save();
    }

    public override void TweakUI(ref UpdateFlags update)
    {
        ToggleControls("Hide Beast Chakra Gauge", ref TweakConfigs.MNKHide0, ref update);
        HideWarning(TweakConfigs.MNKHide0);
        ToggleControls("Hide Chakra Gauge", ref TweakConfigs.MNKHide1, ref update);
        HideWarning(TweakConfigs.MNKHide1);

        if (update.HasFlag(UpdateFlags.Save))
        {
            ApplyTweaks0();
            ApplyTweaks1();
        }
    }

    public override unsafe void ApplyTweaks0()
    {
        var beastChakraGauge = (AddonJobHudMNK0*)GameGui.GetAddonByName("JobHudMNK0");
        if (beastChakraGauge != null && beastChakraGauge->GaugeStandard.Container != null)
        {
            var hide1 = TweakConfigs.MNKHide0;
            var simple1 = ((AddonJobHud*)beastChakraGauge)->UseSimpleGauge;
            beastChakraGauge->GaugeStandard.Container->ToggleVisibility(!hide1 && !simple1);
            beastChakraGauge->GaugeSimple.Container->Color.A = (byte)(hide1 || !simple1 ? 0 : 255);
        }
    }

    public override unsafe void ApplyTweaks1()
    {
        var chakraGauge = (AddonJobHudMNK1*)GameGui.GetAddonByName("JobHudMNK1");
        if (chakraGauge != null && chakraGauge->GaugeStandard.Container != null)
        {
            var hide1 = TweakConfigs.MNKHide1;
            var simple1 = ((AddonJobHud*)chakraGauge)->UseSimpleGauge;
            chakraGauge->GaugeStandard.Container->ToggleVisibility(!hide1 && !simple1);
            chakraGauge->GaugeSimple.Container->Color.A = (byte)(hide1 || !simple1 ? 0 : 255);
        }
    }
}

public partial class TweakConfigs
{
    public bool MNKHide1;
    public bool MNKHide0;
}
