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

public class SGEModule : JobModule
{
    public override Job Job => SGE;
    public override Job Class => Job.None;
    public override Role Role => Healer;
    public override List<AddonOption> AddonOptions => new()
    {
        new("JobHudGFF0", "Eukrasia"),
        new("JobHudGFF1", "Addersgall Gauge"),
        new("_ParameterWidget", "Parameter Bar")
    };

    public override List<MenuOption> JobGaugeMenu => new()
    {
        new("Eukrasia", nameof(EukrasiaTracker)),
        new("Addersgall Gauge", nameof(AddersgallTracker)),
        new("Addersting Counter", nameof(AdderstingTracker))
    };

    public SGEModule(TrackerManager trackerManager, TrackerConfig[] trackerConfigList) : base(trackerManager, trackerConfigList, "JobHudGFF0", "JobHudGFF1") { }

    public override void Save()
    {
        Configuration.TrackerConfigs.SGE = SaveOrder;
        Configuration.Save();
    }

    public override void TweakUI(ref UpdateFlags update)
    {
        Heading("Eukrasia");
        ToggleControls("Hide Eukrasia", ref TweakConfigs.SGEHide0, ref update);

        Heading("Addersgall Gauge");
        ToggleControls("Hide Addersgall Gauge", ref TweakConfigs.SGEHide1, ref update);
    }

    public override unsafe void ApplyTweaks0(IntPtr gaugeAddon)
    {
        var gauge = (AddonJobHudGFF0*)gaugeAddon;
        VisibilityTweak(TweakConfigs.SGEHide0, gauge->UseSimpleGauge, gauge->GaugeStandard.Container, gauge->GaugeSimple.Container);
    }

    public override unsafe void ApplyTweaks1(IntPtr gaugeAddon)
    {
        var gauge = (AddonJobHudGFF1*)gaugeAddon;
        VisibilityTweak(TweakConfigs.SGEHide1, gauge->UseSimpleGauge, gauge->GaugeStandard.Container, gauge->GetNodeById(25));
    }
}

public partial class TweakConfigs
{
    public bool SGEHide0;
    public bool SGEHide1;
}
