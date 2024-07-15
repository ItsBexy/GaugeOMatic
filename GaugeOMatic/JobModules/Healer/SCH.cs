using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;
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

public class SCHModule : JobModule
{
    public override Job Job => SCH;
    public override Job Class => ACN;
    public override Role Role => Healer;
    public override List<AddonOption> AddonOptions => new()
    {
        new("JobHudACN0", "Aetherflow Gauge"),
        new("JobHudSCH0", "Faerie Gauge"),
        new("_ParameterWidget", "Parameter Bar")
    };

    public override List<MenuOption> JobGaugeMenu => new()
    {
        new("Aetherflow Gauge", nameof(AetherflowSCHGaugeTracker)),
        new("Fae Aether", nameof(FaerieGaugeTracker)),
        new("Seraph Timer", nameof(SeraphTracker))
    };

    public SCHModule(TrackerManager trackerManager, TrackerConfig[] trackerConfigList) : base(trackerManager, trackerConfigList, "JobHudACN0", "JobHudSCH0") { }

    public override void Save()
    {
        Configuration.TrackerConfigs.SCH = SaveOrder;
        Configuration.Save();
    }

    public override void TweakUI(ref UpdateFlags update)
    {
        ToggleControls("Hide Aetherflow Gauge", ref TweakConfigs.SCHHide0, ref update);
        HideWarning(TweakConfigs.SCHHide0);
        ToggleControls("Hide Faerie Gauge", ref TweakConfigs.SCHHide1, ref update);
        HideWarning(TweakConfigs.SCHHide1);
        ToggleControls("Hide Fae Aether value\nwhile faerie-less", ref TweakConfigs.SCHDissHideText, ref update);

        if (update.HasFlag(UpdateFlags.Save))
        {
            ApplyTweaks0();
            ApplyTweaks1();
        }
    }

    public override unsafe void ApplyTweaks0()
    {
        var aetherflowGauge = (AddonJobHudACN0*)GameGui.GetAddonByName("JobHudACN0");
        if (aetherflowGauge != null)
        {
            var hide0 = TweakConfigs.SCHHide0;
            var simple0 = ((AddonJobHud*)aetherflowGauge)->UseSimpleGauge;
            ((AtkUnitBase*)aetherflowGauge)->GetNodeById(2)->ToggleVisibility(!hide0 && !simple0);
            ((AtkUnitBase*)aetherflowGauge)->GetNodeById(7)->ToggleVisibility(!hide0 && simple0);
        }
    }

    public override unsafe void ApplyTweaks1()
    {
        var faerieGauge = (AddonJobHudSCH0*)GameGui.GetAddonByName("JobHudSCH0");
        if (faerieGauge != null && faerieGauge->GaugeStandard.Container != null)
        {
            var summoned = faerieGauge->DataCurrent.FaerieSummoned;
            faerieGauge->GaugeStandard.FaeGaugeTextContainer->ToggleVisibility(!TweakConfigs.SCHDissHideText || summoned);
            faerieGauge->GaugeSimple.FaeValueDisplay->OwnerNode->ToggleVisibility(!TweakConfigs.SCHDissHideText || summoned);

            var hide1 = TweakConfigs.SCHHide1;
            var simple1 = ((AddonJobHud*)faerieGauge)->UseSimpleGauge;
            faerieGauge->GaugeStandard.Container->ToggleVisibility(!hide1 && !simple1);
            faerieGauge->GaugeSimple.Container->ToggleVisibility(!hide1 && simple1);
        }
    }
}

public partial class TweakConfigs
{
    public bool SCHHide0;
    public bool SCHHide1;
    public bool SCHDissHideText;
}
