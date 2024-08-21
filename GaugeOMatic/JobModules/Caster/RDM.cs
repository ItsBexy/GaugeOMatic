using FFXIVClientStructs.FFXIV.Client.UI;
using GaugeOMatic.Trackers;
using GaugeOMatic.Windows.Dropdowns;
using System;
using System.Collections.Generic;
using static FFXIVClientStructs.FFXIV.Client.UI.UIModule;
using static GaugeOMatic.GameData.JobData;
using static GaugeOMatic.GameData.JobData.Job;
using static GaugeOMatic.GameData.JobData.Role;
using static GaugeOMatic.GameData.StatusRef;
using static GaugeOMatic.GameData.StatusRef.StatusActor;
using static GaugeOMatic.JobModules.Tweaks;
using static GaugeOMatic.Widgets.WidgetUI;
using static GaugeOMatic.Windows.Dropdowns.TrackerDropdown;

namespace GaugeOMatic.JobModules;

public class RDMModule(TrackerManager trackerManager, TrackerConfig[] trackerConfigList)
    : JobModule(trackerManager, trackerConfigList, "JobHudRDM0")
{
    public override Job Job => RDM;
    public override Job Class => Job.None;
    public override Role Role => Caster;
    public override List<AddonOption> AddonOptions =>
    [
        new("JobHudRDM0", "Balance Gauge"),
        new("_ParameterWidget", "Parameter Bar")
    ];

    public override List<MenuOption> JobGaugeMenu { get; } =
    [
        new("Black Mana", nameof(BlackManaTracker)),
        new("White Mana", nameof(WhiteManaTracker)),
        new("Mana Stacks", nameof(ManaStackTracker)),
        new("Balance Crystal", nameof(BalanceCrystalTracker))
    ];

    public override void Save()
    {
        Configuration.TrackerConfigs.RDM = SaveOrder;
        Configuration.Save();
    }

    public override void TweakUI()
    {
        Heading("Balance Gauge");
        ToggleControls("Hide Balance Gauge", ref TweakConfigs.RDMHide0);
        ToggleControls("Magicked Swordplay Cue", ref TweakConfigs.RDM0SwordplayCue);
        Info("Cues the gauge to become highlighted after pressing\nManification and gaining the Magicked Swordplay buff");
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
                                StatusData[3875].TryGetStatus(out var buff, Self) &&
                                buff?.StackCount == 3;

        if (SwordplayStateCurrent)
        {
            if (!SwordplayStatePrev) PlaySound(78);
            if (gauge->DataCurrent.BlackMana < 50 || gauge->DataCurrent.WhiteMana < 50) JobUiData->SetValue(3, 0, true);
        }
    }
}

public partial class TweakConfigs
{
    public bool RDM0SwordplayCue;
    public bool RDMHide0;
}
