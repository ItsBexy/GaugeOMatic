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

public class SGEModule : JobModule
{
    public override Job Job => SGE;
    public override Job Class => Job.None;
    public override Role Role => Healer;
    public override List<AddonOption> AddonOptions => new()
    {
        new("JobHudGFF0", "Eukrasia"),
        new("JobHudGFF1", "Addersgall Gauge"),
        new("_ParameterWidget", "Parameter Bar")
    };

    public override List<MenuOption> JobGaugeMenu => new()
    {
        new("Eukrasia", nameof(EukrasiaTracker)),
        new("Addersgall Gauge", nameof(AddersgallTracker)),
        new("Addersting Counter", nameof(AdderstingTracker))
    };

    public SGEModule(TrackerManager trackerManager, TrackerConfig[] trackerConfigList) : base(trackerManager, trackerConfigList, "JobHudGFF0", "JobHudGFF1") { }

    public override void Save()
    {
        Configuration.TrackerConfigs.SGE = SaveOrder;
        Configuration.Save();
    }

    public override void TweakUI(ref UpdateFlags update)
    {
        ToggleControls("Hide Eukrasia", ref TweakConfigs.SGEHide0, ref update);
        HideWarning(TweakConfigs.SGEHide0);
        ToggleControls("Hide Addersgall Gauge", ref TweakConfigs.SGEHide1, ref update);
        HideWarning(TweakConfigs.SGEHide1);

        if (update.HasFlag(UpdateFlags.Save))
        {
            ApplyTweaks0();
            ApplyTweaks1();
        }
    }

    public override unsafe void ApplyTweaks0()
    {
        var eukrasiaGauge = (AddonJobHudGFF0*)GameGui.GetAddonByName("JobHudGFF0");
        if (eukrasiaGauge != null && eukrasiaGauge->GaugeStandard.Container != null)
        {
            var hideEukrasia = TweakConfigs.SGEHide0;
            var simple0 = ((AddonJobHud*)eukrasiaGauge)->UseSimpleGauge;
            eukrasiaGauge->GaugeStandard.Container->Color.A = (byte)(hideEukrasia || simple0 ? 0 : 255);
            eukrasiaGauge->GaugeSimple.Container->Color.A = (byte)(hideEukrasia || !simple0 ? 0 : 255);
        }
    }

    public override unsafe void ApplyTweaks1()
    {

        var addersgallGauge = (AddonJobHudGFF1*)GameGui.GetAddonByName("JobHudGFF1");
        if (addersgallGauge != null && addersgallGauge->GaugeStandard.Container != null)
        {
            var hideAddersgall = TweakConfigs.SGEHide1;
            var simple1 = ((AddonJobHud*)addersgallGauge)->UseSimpleGauge;
            addersgallGauge->GaugeStandard.Container->Color.A = (byte)(hideAddersgall || simple1 ? 0 : 255);
            ((AtkUnitBase*)addersgallGauge)->GetNodeById(25)->Color.A = (byte)(hideAddersgall || !simple1 ? 0 : 255);
        }
    }
}

public partial class TweakConfigs
{
    public bool SGEHide0;
    public bool SGEHide1;
}
