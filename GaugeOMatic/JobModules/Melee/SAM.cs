using FFXIVClientStructs.FFXIV.Client.UI;
using GaugeOMatic.Trackers;
using GaugeOMatic.Windows.Dropdowns;
using System;
using System.Collections.Generic;
using System.Numerics;
using static GaugeOMatic.GameData.JobData;
using static GaugeOMatic.GameData.JobData.Job;
using static GaugeOMatic.GameData.JobData.Role;
using static GaugeOMatic.JobModules.Tweaks;
using static GaugeOMatic.Trackers.Tracker;
using static GaugeOMatic.Widgets.WidgetUI;
using static GaugeOMatic.Windows.Dropdowns.TrackerDropdown;

namespace GaugeOMatic.JobModules;

public class SAMModule : JobModule
{
    public override Job Job => SAM;
    public override Job Class => Job.None;
    public override Role Role => Melee;
    public override List<AddonOption> AddonOptions => new()
    {
        new("JobHudSAM0", "Kenki Gauge"),
        new("JobHudSAM1", "Sen Gauge"),
        new("_ParameterWidget", "Parameter Bar")
    };

    public override List<MenuOption> JobGaugeMenu { get; } = new() {
        new("Kenki Gauge", nameof(KenkiGaugeTracker)),
        new("Meditation Gauge", nameof(MeditationGaugeTracker)),
        new("Sen Gauge - Seal Count", nameof(SenSealTracker)),
        new("Sen Gauge - Setsu Seal", nameof(SenGaugeSetsuTracker)),
        new("Sen Gauge - Getsu Seal", nameof(SenGaugeGetsuTracker)),
        new("Sen Gauge - Ka Seal", nameof(SenGaugeKaTracker))
    };

    public SAMModule(TrackerManager trackerManager, TrackerConfig[] trackerConfigList) : base(trackerManager, trackerConfigList, "JobHudSAM0", "JobHudSAM1") { }

    public override void Save()
    {
        Configuration.TrackerConfigs.SAM = SaveOrder;
        Configuration.Save();
    }

    public override void TweakUI(ref UpdateFlags update)
    {
        Heading("Kenki Gauge");
        ToggleControls("Hide Kenki Bar", ref TweakConfigs.SAMHide0Kenki, ref update);

        ToggleControls("Hide Meditation Stacks", ref TweakConfigs.SAMHide0Meditation, ref update);

        Heading("Sen Gauge");
        ToggleControls("Hide Sen Gauge", ref TweakConfigs.SAMHide1, ref update);

        Heading("Reposition Seals");
        PositionControls("Setsu", ref TweakConfigs.SAMSealPosSetsu, ref update);
        PositionControls("Getsu", ref TweakConfigs.SAMSealPosGetsu, ref update);
        PositionControls("Ka", ref TweakConfigs.SAMSealPosKa, ref update);
    }

    public override unsafe void ApplyTweaks0(IntPtr gaugeAddon)
    {
        var gauge = (AddonJobHudSAM0*)gaugeAddon;

        VisibilityTweak(TweakConfigs.SAMHide0Kenki, gauge->UseSimpleGauge, gauge->GaugeStandard.KenkiContainer, gauge->GaugeSimple.KenkiContainer);
        VisibilityTweak(TweakConfigs.SAMHide0Meditation, gauge->UseSimpleGauge, gauge->GaugeStandard.MeditationContainer, gauge->GaugeSimple.MeditationContainer);
    }

    public override unsafe void ApplyTweaks1(IntPtr gaugeAddon)
    {
        var gauge = (AddonJobHudSAM1*)gaugeAddon;
        var simple1 = gauge->UseSimpleGauge;
        VisibilityTweak(TweakConfigs.SAMHide1, simple1, gauge->GaugeStandard.Container, gauge->GaugeSimple.Container);

        if (gauge != null && gauge->GaugeStandard.Container != null)
        {
            var setsuPos = TweakConfigs.SAMSealPosSetsu;
            var getsuPos = TweakConfigs.SAMSealPosGetsu;
            var kaPos = TweakConfigs.SAMSealPosKa;
            if (!simple1)
            {
                gauge->GaugeStandard.SetsuNode->SetPositionFloat(setsuPos.X + 44, setsuPos.Y + 4);
                gauge->GaugeStandard.GetsuNode->SetPositionFloat(getsuPos.X, getsuPos.Y + 76);
                gauge->GaugeStandard.KaNode->SetPositionFloat(kaPos.X + 90, kaPos.Y + 73);
            }
            else
            {
                gauge->GaugeSimple.SetsuNode->SetPositionFloat(setsuPos.X + 0, setsuPos.Y);
                gauge->GaugeSimple.GetsuNode->SetPositionFloat(getsuPos.X + 19, getsuPos.Y);
                gauge->GaugeSimple.KaNode->SetPositionFloat(kaPos.X + 38, kaPos.Y);
            }
        }
    }
}

public partial class TweakConfigs
{
    public bool SAMHide0Kenki;
    public bool SAMHide0Meditation;
    public bool SAMHide1;
    public Vector2 SAMSealPosSetsu;
    public Vector2 SAMSealPosGetsu;
    public Vector2 SAMSealPosKa;
}
