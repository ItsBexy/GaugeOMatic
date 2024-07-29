using CustomNodes;
using FFXIVClientStructs.FFXIV.Client.UI;
using GaugeOMatic.Trackers;
using GaugeOMatic.Windows;
using System;
using System.Collections.Generic;
using static GaugeOMatic.GameData.JobData;
using static GaugeOMatic.GameData.JobData.Job;
using static GaugeOMatic.GameData.JobData.Role;
using static GaugeOMatic.JobModules.Tweaks;
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
        Heading("Aetherflow Gauge");
        ToggleControls("Hide Aetherflow Gauge", ref TweakConfigs.SCHHide0, ref update);

        Heading("Faerie Gauge");
        ToggleControls("Hide Faerie Gauge", ref TweakConfigs.SCHHide1, ref update);
        //todo: update this tweak to allow showing the Dissipation timer instead
        ToggleControls("Hide Fae Aether value while faerie-less", ref TweakConfigs.SCHDissHideText, ref update);
    }

    public override unsafe void ApplyTweaks0(IntPtr gaugeAddon)
    {
        var gauge = (AddonJobHudACN0*)gaugeAddon;
        var gaugeIndex = (AddonIndex)gaugeAddon;
        VisibilityTweak(TweakConfigs.SCHHide0, gauge->UseSimpleGauge, gaugeIndex[2u], gaugeIndex[7u]);
    }

    public override unsafe void ApplyTweaks1(IntPtr gaugeAddon)
    {
        var gauge = (AddonJobHudSCH0*)gaugeAddon;
        VisibilityTweak(TweakConfigs.SCHHide1, gauge->UseSimpleGauge, gauge->GaugeStandard.Container, gauge->GaugeSimple.Container);

        if (gauge != null && gauge->GaugeStandard.Container != null)
        {
            var summoned = gauge->DataCurrent.FaerieSummoned;
            gauge->GaugeStandard.FaeGaugeTextContainer->ToggleVisibility(!TweakConfigs.SCHDissHideText || summoned);
            gauge->GaugeSimple.FaeValueDisplay->OwnerNode->ToggleVisibility(!TweakConfigs.SCHDissHideText || summoned);
        }
    }
}

public partial class TweakConfigs
{
    public bool SCHHide0;
    public bool SCHHide1;
    public bool SCHDissHideText;
}
