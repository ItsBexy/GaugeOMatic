using GaugeOMatic.Trackers;
using GaugeOMatic.Windows;
using System.Collections.Generic;
using FFXIVClientStructs.FFXIV.Client.UI;
using static GaugeOMatic.GameData.JobData;
using static GaugeOMatic.GameData.JobData.Job;
using static GaugeOMatic.GameData.JobData.Role;
using static GaugeOMatic.Windows.ItemRefMenu;
using static GaugeOMatic.JobModules.TweakUI;
using static GaugeOMatic.Widgets.WidgetUI;

namespace GaugeOMatic.JobModules;

public class RPRModule : JobModule
{
    public override Job Job => RPR;
    public override Job Class => Job.None;
    public override Role Role => Melee;
    public override List<AddonOption> AddonOptions => new()
    {
        new("JobHudRRP0", "Soul Gauge"),
        new("JobHudRRP1", "Death Gauge"),
        new("_ParameterWidget", "Parameter Bar")
    };

    public override List<MenuOption> JobGaugeMenu { get; } = new() {
        new("Soul Gauge", nameof(SoulGaugeTracker)),
        new("Shroud Gauge", nameof(ShroudGaugeTracker)),
        new("Lemure Shroud", nameof(LemureShroudTracker)),
        new("Void Shroud", nameof(VoidShroudTracker))
    };

    public RPRModule(TrackerManager trackerManager, TrackerConfig[] trackerConfigList) : base(trackerManager, trackerConfigList, "JobHudRRP0", "JobHudRRP1") { }

    public override void Save()
    {
        Configuration.TrackerConfigs.RPR = SaveOrder;
        Configuration.Save();
    }


    public override void TweakUI(ref UpdateFlags update)
    {
        ToggleControls("Hide Soul Gauge", ref TweakConfigs.RPRHide0, ref update);
        HideWarning(TweakConfigs.RPRHide0);
        ToggleControls("Hide Death Gauge", ref TweakConfigs.RPRHide1, ref update);
        HideWarning(TweakConfigs.RPRHide1);

        if (update.HasFlag(UpdateFlags.Save))
        {
            ApplyTweaks0();
            ApplyTweaks1();
        }
    }

    public override unsafe void ApplyTweaks0()
    {
        var soulGauge = (AddonJobHudRRP0*)GameGui.GetAddonByName("JobHudRRP0");
        if (soulGauge != null && soulGauge->GaugeStandard.Container != null)
        {
            var hideSoul = TweakConfigs.RPRHide0;
            var simple0 = ((AddonJobHud*)soulGauge)->UseSimpleGauge;
            soulGauge->GaugeStandard.Container->Color.A = (byte)(hideSoul || simple0 ? 0 : 255);
            soulGauge->GaugeSimple.Container->Color.A = (byte)(hideSoul || !simple0 ? 0 : 255);
        }
    }

    public override unsafe void ApplyTweaks1()
    {
        var deathGauge = (AddonJobHudRRP1*)GameGui.GetAddonByName("JobHudRRP1");
        if (deathGauge != null && deathGauge->GaugeStandard.Container != null)
        {
            var hideDeath = TweakConfigs.RPRHide1;
            var simple1 = ((AddonJobHud*)deathGauge)->UseSimpleGauge;
            deathGauge->GaugeStandard.Container->Color.A = (byte)(hideDeath || simple1 ? 0 : 255);
            deathGauge->GaugeSimple.Container->Color.A = (byte)(hideDeath || !simple1 ? 0 : 255);
        }
    }
}

public partial class TweakConfigs
{
    public bool RPRHide0;
    public bool RPRHide1;
}
