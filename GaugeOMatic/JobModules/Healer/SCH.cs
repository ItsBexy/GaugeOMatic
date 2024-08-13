using CustomNodes;
using FFXIVClientStructs.FFXIV.Client.UI;
using GaugeOMatic.Trackers;
using GaugeOMatic.Windows.Dropdowns;
using System;
using System.Collections.Generic;
using static GaugeOMatic.GameData.JobData;
using static GaugeOMatic.GameData.JobData.Job;
using static GaugeOMatic.GameData.JobData.Role;
using static GaugeOMatic.GameData.StatusRef;
using static GaugeOMatic.GameData.StatusRef.StatusActor;
using static GaugeOMatic.JobModules.Tweaks;
using static GaugeOMatic.Trackers.Tracker;
using static GaugeOMatic.Widgets.WidgetUI;
using static GaugeOMatic.Windows.Dropdowns.TrackerDropdown;

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

        if (!TweakConfigs.SCHHide1)
            RadioControls("While Faerieless: ", ref TweakConfigs.SCH1FaerieLess,
                          new() { 0, 1, 2 },
                          new() { "Show Gauge Value", "Hide Gauge Value", "Show Dissipation Timer" }, ref update);
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

        FaerielessTweak(gauge);
    }

    private unsafe void FaerielessTweak(AddonJobHudSCH0* gauge)
    {
        if (gauge == null || gauge->GaugeStandard.FaeGaugeTextContainer == null) return;
        var summoned = gauge->DataCurrent.FaerieSummoned;
        bool show;
        int dissTimer;

        switch (TweakConfigs.SCH1FaerieLess)
        {
            case 1:
                show = summoned;
                dissTimer = 0;
                break;
            case 2:
                show = StatusData[791].TryGetStatus(out var buff, Self) || summoned;
                dissTimer = (int)Math.Abs(buff?.RemainingTime??0);
                break;
            default:
                show = true;
                dissTimer = 0;
                break;
        }

        ((CustomNode)gauge->GaugeStandard.FaeGaugeTextContainer).SetVis(show);
        ((CustomNode)gauge->GaugeSimple.FaeValueDisplay->OwnerNode).SetVis(show);

        var textStandard = (CustomNode)gauge->GaugeStandard.FaeGaugeText;
        var textSimple = (CustomNode)gauge->GaugeSimple.FaeValueDisplay->AtkTextNode;
        if (dissTimer > 0)
        {
            textStandard.SetText(dissTimer.ToString()).SetTextColor(0xD7D7D7FF,0x316381FF);
            textSimple.SetText(dissTimer.ToString()).SetTextColor(0xD7D7D7FF, 0x316381FF);
        }
        else
        {
            textStandard.SetTextColor(0xffffffff, 0x288246ff);
            textSimple.SetTextColor(0xffffffff, 0x9d835bff);
        }
    }
}

public partial class TweakConfigs
{
    public bool SCHHide0;
    public uint SCH1FaerieLess;
    public bool SCHHide1;
    public bool SCHDissHideText;
}
