using GaugeOMatic.Trackers;
using GaugeOMatic.Windows;
using System.Collections.Generic;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;
using static GaugeOMatic.GameData.JobData;
using static GaugeOMatic.GameData.JobData.Job;
using static GaugeOMatic.GameData.JobData.Role;
using static GaugeOMatic.Windows.ItemRefMenu;
using static GaugeOMatic.JobModules.TweakUI;
using static GaugeOMatic.Widgets.WidgetUI;

namespace GaugeOMatic.JobModules;

public class NINModule : JobModule
{
    public override Job Job => NIN;
    public override Job Class => ROG;
    public override Role Role => Melee;
    public override List<AddonOption> AddonOptions => new()
    {
        new("JobHudNIN0", "Ninki Gauge"),
        new("JobHudNIN1v70", "Kazematoi"),
        new("_ParameterWidget", "Parameter Bar")
    };

    public override List<MenuOption> JobGaugeMenu { get; } = new() {
        new("Ninki Gauge", nameof(NinkiGaugeTracker))
    };

    public NINModule(TrackerManager trackerManager, TrackerConfig[] trackerConfigList) : base(trackerManager, trackerConfigList, "JobHudNIN0", "JobHudNIN1v70") { }

    public override void Save()
    {
        Configuration.TrackerConfigs.NIN = SaveOrder;
        Configuration.Save();
    }

    public override void TweakUI(ref UpdateFlags update)
    {
        ToggleControls("Hide Ninki Gauge", ref TweakConfigs.NINHide0, ref update);
        HideWarning(TweakConfigs.NINHide0);
        ToggleControls("Hide Kazematoi", ref TweakConfigs.NINHide1, ref update);
        HideWarning(TweakConfigs.NINHide1);

        if (update.HasFlag(UpdateFlags.Save))
        {
            ApplyTweaks0();
            ApplyTweaks1();
        }
    }

    public override unsafe void ApplyTweaks0()
    {
        var ninkiGauge = (AddonJobHudNIN0*)GameGui.GetAddonByName("JobHudNIN0");
        if (ninkiGauge != null && ninkiGauge->GaugeStandard.Container != null)
        {
            var hide0 = TweakConfigs.NINHide0;
            var simple0 = ((AddonJobHud*)ninkiGauge)->UseSimpleGauge;
            ninkiGauge->GaugeStandard.Container->Color.A = (byte)(hide0 || simple0 ? 0 : 255);
            ninkiGauge->GaugeSimple.Container->Color.A = (byte)(hide0 || !simple0 ? 0 : 255);
        }
    }

    public override unsafe void ApplyTweaks1()
    {
        var kazematoiGauge = (AddonJobHudNIN1*)GameGui.GetAddonByName("JobHudNIN1v70");
        if (kazematoiGauge != null && kazematoiGauge->GaugeStandard.Container != null)
        {
            var hide0 = TweakConfigs.NINHide1;
            var simple0 = ((AddonJobHud*)kazematoiGauge)->UseSimpleGauge;
            kazematoiGauge->GaugeStandard.Container->Color.A = (byte)(hide0 || simple0 ? 0 : 255);
            ((AtkUnitBase*)kazematoiGauge)->GetNodeById(17)->SetAlpha((byte)(hide0 || !simple0 ? 0 : 255));
        }
    }
}

public partial class TweakConfigs
{
    public bool NINHide0;
    public bool NINHide1;
}
