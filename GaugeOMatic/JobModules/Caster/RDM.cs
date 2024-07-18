using FFXIVClientStructs.FFXIV.Client.UI;
using GaugeOMatic.Trackers;
using GaugeOMatic.Widgets;
using GaugeOMatic.Windows;
using System;
using System.Collections.Generic;
using FFXIVClientStructs.FFXIV.Component.GUI;
using static GaugeOMatic.GameData.JobData;
using static GaugeOMatic.GameData.JobData.Job;
using static GaugeOMatic.GameData.JobData.Role;
using static GaugeOMatic.GameData.StatusData;
using static GaugeOMatic.JobModules.Tweaks;
using static GaugeOMatic.JobModules.Tweaks.TweakUI;
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

    public override List<MenuOption> JobGaugeMenu { get; } = new() {
        new("Black Mana", nameof(BlackManaTracker)),
        new("White Mana", nameof(WhiteManaTracker)),
        new("Mana Stacks", nameof(ManaStackTracker)),
        new("Balance Crystal", nameof(BalanceCrystalTracker))
    };

    public RDMModule(TrackerManager trackerManager, TrackerConfig[] trackerConfigList) : base(trackerManager, trackerConfigList, "JobHudRDM0") { }

    public override void Save()
    {
        Configuration.TrackerConfigs.RDM = SaveOrder;
        Configuration.Save();
    }

    public override void TweakUI(ref UpdateFlags update)
    {
        WidgetUI.ToggleControls("Hide Balance Gauge", ref TweakConfigs.RDMHide0, ref update);
        HideWarning(TweakConfigs.RDMHide0);
        WidgetUI.ToggleControls("Magicked Swordplay Cue", ref TweakConfigs.RDM0SwordplayCue, ref update);
    }

    public bool SwordplayStatePrev;
    public bool SwordplayStateCurrent;

    public override unsafe void ApplyTweaks0(IntPtr gaugeAddon)
    {
        var gauge = (AddonJobHudRDM0*)gaugeAddon;
        ApplySwordplayCueTweak(gauge);
        VisibilityTweak(TweakConfigs.RDMHide0, gauge->UseSimpleGauge, gauge->GaugeStandard.Container, gauge->GaugeSimple.Container);
    }

    private unsafe void ApplySwordplayCueTweak(AddonJobHudRDM0* gauge)
    {
        SwordplayStatePrev = SwordplayStateCurrent;
        SwordplayStateCurrent = TweakConfigs.RDM0SwordplayCue &&
                                Statuses[3875].TryGetStatus(out var buff) &&
                                buff?.StackCount == 3;

        if (SwordplayStateCurrent)
        {
            if (!SwordplayStatePrev) UIModule.PlaySound(78);
            if (gauge->DataCurrent.BlackMana < 50 || gauge->DataCurrent.WhiteMana < 50) JobUiData->SetValue(3, 0, true);
        }
    }
}

public partial class TweakConfigs
{
    public bool RDM0SwordplayCue;
    public bool RDMHide0;
}
