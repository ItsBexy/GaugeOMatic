using FFXIVClientStructs.FFXIV.Client.UI;
using GaugeOMatic.Trackers;
using GaugeOMatic.Windows.Dropdowns;
using System;
using System.Collections.Generic;
using static GaugeOMatic.GameData.JobData;
using static GaugeOMatic.GameData.JobData.Job;
using static GaugeOMatic.GameData.JobData.Role;
using static GaugeOMatic.JobModules.Tweaks;
using static GaugeOMatic.Widgets.WidgetUI;
using static GaugeOMatic.Windows.Dropdowns.TrackerDropdown;

namespace GaugeOMatic.JobModules;

public class ASTModule(TrackerManager trackerManager, TrackerConfig[] trackerConfigList)
    : JobModule(trackerManager, trackerConfigList, "JobHudAST0")
{
    public override Job Job => AST;
    public override Job Class => Job.None;
    public override Role Role => Healer;
    public override List<AddonOption> AddonOptions =>
    [
        new("JobHudAST0", "Arcana Gauge"),
        new("_ParameterWidget", "Parameter Bar")
    ];

    public override void Save()
    {
        Configuration.TrackerConfigs.AST = SaveOrder;
        Configuration.Save();
    }

    public override List<MenuOption> JobGaugeMenu { get; } = [];

    public override void TweakUI()
    {
        Heading("Arcana Gauge");
        ToggleControls("Hide Arcana Gauge", ref TweakConfigs.ASTHide0);
    }

    public override unsafe void ApplyTweaks0(IntPtr gaugeAddon)
    {
        var gauge = (AddonJobHudAST0*)gaugeAddon;
        VisibilityTweak(TweakConfigs.ASTHide0, gauge->UseSimpleGauge, gauge->GaugeStandard.Container, gauge->GaugeSimple.Container);
    }

}

public partial class TweakConfigs
{
    public bool ASTHide0;
}
