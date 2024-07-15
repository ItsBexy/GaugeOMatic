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
        ToggleControls("Hide Arcana Gauge", ref TweakConfigs.ASTHide0, ref update);
        HideWarning(TweakConfigs.ASTHide0);

        if (update.HasFlag(UpdateFlags.Save)) ApplyTweaks0();
    }

    public override unsafe void ApplyTweaks0()
    {
        var arcanaGauge = (AddonJobHudAST0*)GameGui.GetAddonByName("JobHudAST0");
        if (arcanaGauge != null)
        {
            var hide0 = TweakConfigs.ASTHide0;
            var simple0 = ((AddonJobHud*)arcanaGauge)->UseSimpleGauge;
            arcanaGauge->GaugeStandard.Container->ToggleVisibility(!hide0 && !simple0);
            arcanaGauge->GaugeSimple.Container->ToggleVisibility(!hide0 && simple0);
        }
    }

}
public partial class TweakConfigs
{
   public bool ASTHide0;
}
