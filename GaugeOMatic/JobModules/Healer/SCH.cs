using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;
using GaugeOMatic.Trackers;
using GaugeOMatic.Windows;
using System.Collections.Generic;
using static GaugeOMatic.GameData.JobData;
using static GaugeOMatic.GameData.JobData.Job;
using static GaugeOMatic.GameData.JobData.Role;
using static GaugeOMatic.GaugeOMatic.Service;
using static GaugeOMatic.JobModules.TweakUI;
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
        new("Aetherflow Gauge",nameof(AetherflowSCHGaugeTracker)),
        new("Fae Aether",nameof(FaerieGaugeTracker)),
        new("Seraph Timer",nameof(SeraphTracker))
    };

    public SCHModule(TrackerManager trackerManager, TrackerConfig[] trackerConfigList) : base(trackerManager, trackerConfigList) { }

    public override void Save()
    {
        Configuration.TrackerConfigs.SCH = SaveOrder;
        Configuration.Save();
    }

    public override void TweakUI(ref UpdateFlags update)
    {
        var hideAetherflow = TweakConfigs.SCHHideAetherflow;
        var hideFaerie = TweakConfigs.SCHHideFaerie;
        var schDissTimer = TweakConfigs.SCHDissHideText;

        if (Bool1("Hide Aetherflow Gauge", ref hideAetherflow, ref update)) TweakConfigs.SCHHideAetherflow = hideAetherflow;
        HideWarning(hideAetherflow);
        if (Bool1("Hide Faerie Gauge", ref hideFaerie, ref update)) TweakConfigs.SCHHideFaerie = hideFaerie;
        HideWarning(hideFaerie);
        if (Bool1("Hide Fae Aether value while faerie-less", ref schDissTimer, ref update)) TweakConfigs.SCHDissHideText = schDissTimer;

        if (update.HasFlag(UpdateFlags.Save)) ApplyTweaks();
    }

    public override unsafe void ApplyTweaks()
    {
        var aetherflowGauge = (AddonJobHudACN0*)GameGui.GetAddonByName("JobHudACN0");
        if (aetherflowGauge != null && aetherflowGauge->GaugeStandard.AetherflowStacksSpan[0].StackContainer != null)
        {
            var hideAetherflow = TweakConfigs.SCHHideAetherflow;
            var simple0 = aetherflowGauge->JobHud.UseSimpleGauge;
            ((AtkUnitBase*)aetherflowGauge)->GetNodeById(2)->ToggleVisibility(!hideAetherflow && !simple0);
            ((AtkUnitBase*)aetherflowGauge)->GetNodeById(7)->ToggleVisibility(!hideAetherflow && simple0);
        }

        var faerieGauge = (AddonJobHudSCH0*)GameGui.GetAddonByName("JobHudSCH0");
        if (faerieGauge != null && faerieGauge->GaugeStandard.Container != null)
        {
            var hideFaerie = TweakConfigs.SCHHideFaerie;
            var simple1 = faerieGauge->JobHud.UseSimpleGauge;
            faerieGauge->GaugeStandard.Container->ToggleVisibility(!hideFaerie && !simple1);
            faerieGauge->GaugeSimple.Container->ToggleVisibility(!hideFaerie && simple1);

            var summoned = faerieGauge->DataCurrent.Prerequisites[2] > 0;
            faerieGauge->GaugeStandard.FaeGaugeTextContainer->ToggleVisibility(!TweakConfigs.SCHDissHideText || summoned);
            faerieGauge->GaugeSimple.FaeValueDisplay->OwnerNode->ToggleVisibility(!TweakConfigs.SCHDissHideText || summoned);
        }
    }
}

public partial class TweakConfigs
{
    public bool SCHHideAetherflow { get; set; }
    public bool SCHHideFaerie { get; set; }
    public bool SCHDissHideText { get; set; }
}
