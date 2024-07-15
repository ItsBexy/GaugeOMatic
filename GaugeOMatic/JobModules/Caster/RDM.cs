using FFXIVClientStructs.FFXIV.Client.UI;
using GaugeOMatic.Trackers;
using GaugeOMatic.Widgets;
using GaugeOMatic.Windows;
using System.Collections.Generic;
using static GaugeOMatic.GameData.JobData;
using static GaugeOMatic.GameData.JobData.Job;
using static GaugeOMatic.GameData.JobData.Role;
using static GaugeOMatic.GameData.StatusData;
using static GaugeOMatic.Windows.ItemRefMenu;
using static GaugeOMatic.JobModules.TweakUI;

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

        if (update.HasFlag(UpdateFlags.Save)) ApplyTweaks0();
    }

    public bool SwordplayStatePrev;
    public bool SwordplayStateCurrent;
    public override unsafe void ApplyTweaks0()
    {
        var balanceGauge = (AddonJobHudRDM0*)GameGui.GetAddonByName("JobHudRDM0");
        ApplySwordplayCueTweak(balanceGauge);

        if (balanceGauge != null && balanceGauge->GaugeStandard.Container != null)
        {
            var hide0 = TweakConfigs.RDMHide0;
            var simple = ((AddonJobHud*)balanceGauge)->UseSimpleGauge;
            balanceGauge->GaugeStandard.Container->Color.A = (byte)(hide0 || simple ? 0 : 255);
            balanceGauge->GaugeSimple.Container->Color.A = (byte)(hide0 || !simple ? 0 : 255);
        }
    }

    private unsafe void ApplySwordplayCueTweak(AddonJobHudRDM0* balanceGauge)
    {
        SwordplayStatePrev = SwordplayStateCurrent;
        SwordplayStateCurrent = TweakConfigs.RDM0SwordplayCue &&
                                Statuses[3875].TryGetStatus(out var buff) &&
                                buff?.StackCount == 3;

        if (SwordplayStateCurrent)
        {
            if (!SwordplayStatePrev) UIModule.PlaySound(78);

            if (balanceGauge->DataCurrent.BlackMana < 50 || balanceGauge->DataCurrent.WhiteMana < 50)
            {
                UIModule.Instance()->GetRaptureAtkModule()->GetNumberArrayData(86)->SetValue(3, 0, true);
            }
        }
    }
}

public partial class TweakConfigs
{
    public bool RDM0SwordplayCue;
    public bool RDMHide0;
}
