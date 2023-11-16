using FFXIVClientStructs.FFXIV.Client.UI;
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

public class RDMModule : JobModule
{
    public override Job Job => RDM;
    public override Job Class => Job.None;
    public override Role Role => Caster;
    public override List<AddonOption> AddonOptions => new()
    {
        new("JobHudRDM0", "Balance Gauge"),
        new("_ParameterWidget", "Parameter Bar")
    };

    public override List<MenuOption> JobGaugeMenu { get; } = new()    {
        new("Black Mana",nameof(BlackManaTracker)),
        new("White Mana",nameof(WhiteManaTracker)),
        new("Mana Stacks",nameof(ManaStackTracker)),
        new("Balance Crystal",nameof(BalanceCrystalTracker))
    };

    public RDMModule(TrackerManager trackerManager, TrackerConfig[] trackerConfigList) : base(trackerManager, trackerConfigList) { }

    public override void Save()
    {
        Configuration.TrackerConfigs.RDM = SaveOrder;
        Configuration.Save();
    }

    public override void TweakUI(ref UpdateFlags update)
    {
        var hideAll = TweakConfigs.RDMHideAll;

        if (Bool1("Hide Balance Gauge", ref hideAll, ref update)) TweakConfigs.RDMHideAll = hideAll;
        HideWarning(hideAll);

        if (update.HasFlag(UpdateFlags.Save)) ApplyTweaks();
    }

    public override unsafe void ApplyTweaks()
    {
        var balanceGauge = (AddonJobHudRDM0*)GameGui.GetAddonByName("JobHudRDM0");
        if (balanceGauge != null && balanceGauge->GaugeStandard.Container != null)
        {
            var hideAll = TweakConfigs.RDMHideAll;
            var simple = balanceGauge->JobHud.UseSimpleGauge;
            balanceGauge->GaugeStandard.Container->Color.A = (byte)(hideAll || simple ? 0 : 255);
            balanceGauge->GaugeSimple.Container->Color.A = (byte)(hideAll || !simple ? 0 : 255);
        }
    }
}

public partial class TweakConfigs
{
    public bool RDMHideAll { get; set; }
}
