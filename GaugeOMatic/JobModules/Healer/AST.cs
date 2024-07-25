using FFXIVClientStructs.FFXIV.Client.UI;
using GaugeOMatic.Trackers;
using GaugeOMatic.Windows;
using System;
using System.Collections.Generic;
using static GaugeOMatic.GameData.JobData;
using static GaugeOMatic.GameData.JobData.Job;
using static GaugeOMatic.GameData.JobData.Role;
using static GaugeOMatic.JobModules.Tweaks;
using static GaugeOMatic.JobModules.Tweaks.TweakUI;
using static GaugeOMatic.Widgets.WidgetUI;
using static GaugeOMatic.Windows.ItemRefMenu;

namespace GaugeOMatic.JobModules;

public class ASTModule : JobModule
{
    public override Job Job => AST;
    public override Job Class => Job.None;
    public override Role Role => Healer;
    public override List<AddonOption> AddonOptions => new()
    {
        new("JobHudAST0", "Arcana Gauge"),
        new("_ParameterWidget", "Parameter Bar")
    };

    public ASTModule(TrackerManager trackerManager, TrackerConfig[] trackerConfigList) : base(trackerManager, trackerConfigList, "JobHudAST0") { }

    public override void Save()
    {
        Configuration.TrackerConfigs.AST = SaveOrder;
        Configuration.Save();
    }

    public override List<MenuOption> JobGaugeMenu { get; } = new();

    public override void TweakUI(ref UpdateFlags update)
    {
        Heading("Arcana Gauge");
        ToggleControls("Hide Arcana Gauge", ref TweakConfigs.ASTHide0, ref update);
        HideInfo(TweakConfigs.ASTHide0);
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
