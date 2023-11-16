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

public class SMNModule : JobModule
{
    public sealed override Job Job => SMN;
    public override Job Class => ACN;
    public override Role Role => Caster;
    public override List<AddonOption> AddonOptions => new()
    {
        new("JobHudSMN0", "Aetherflow Gauge"),
        new("JobHudSMN1", "Trance Gauge"),
        new("_ParameterWidget", "Parameter Bar")
    };

    public override List<MenuOption> JobGaugeMenu => new()
    {
        new("Aetherflow Gauge",nameof(AetherflowSMNGaugeTracker))
    };

    public SMNModule(TrackerManager trackerManager, TrackerConfig[] trackerConfigList) : base(trackerManager, trackerConfigList) { }

    public override void Save()
    {
        Configuration.TrackerConfigs.SMN = SaveOrder;
        Configuration.Save();
    }

    public override void TweakUI(ref UpdateFlags update)
    {
        var hideAetherflow = TweakConfigs.SMNHideAetherflow;

        if (Bool1("Hide Aetherflow Gauge", ref hideAetherflow, ref update)) TweakConfigs.SMNHideAetherflow = hideAetherflow;
        HideWarning(hideAetherflow);

        if (update.HasFlag(UpdateFlags.Save)) ApplyTweaks();
    }

    public override unsafe void ApplyTweaks()
    {
        var aetherflowGauge = (AddonJobHudSMN0*)GameGui.GetAddonByName("JobHudSMN0");
        if (aetherflowGauge != null && aetherflowGauge->GaugeStandard.Stack1 != null)
        {
            var hideAetherflow = TweakConfigs.SMNHideAetherflow;
            var simple0 = aetherflowGauge->JobHud.UseSimpleGauge;
            ((AtkUnitBase*)aetherflowGauge)->GetNodeById(2)->ToggleVisibility(!hideAetherflow && !simple0);
            ((AtkUnitBase*)aetherflowGauge)->GetNodeById(11)->ToggleVisibility(!hideAetherflow && simple0);
        }
    }
}

public partial class TweakConfigs
{
    public bool SMNHideAetherflow { get; set; }
}
