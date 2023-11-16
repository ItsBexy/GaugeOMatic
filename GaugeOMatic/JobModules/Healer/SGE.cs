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
        new("Eukrasia",nameof(EukrasiaTracker)),
        new("Addersgall Gauge",nameof(AddersgallTracker)),
        new("Addersting Counter",nameof(AdderstingTracker))
    };

    public SGEModule(TrackerManager trackerManager, TrackerConfig[] trackerConfigList) : base(trackerManager, trackerConfigList) { }

    public override void Save()
    {
        Configuration.TrackerConfigs.SGE = SaveOrder;
        Configuration.Save();
    }

    public override void TweakUI(ref UpdateFlags update)
    {
        var hideEukrasia = TweakConfigs.SGEHideEukrasia;
        var hideAddersgall = TweakConfigs.SGEHideAddersgall;

        if (Bool1("Hide Eukrasia", ref hideEukrasia, ref update)) TweakConfigs.SGEHideEukrasia = hideEukrasia;
        HideWarning(hideEukrasia);
        if (Bool1("Hide Addersgall Gauge", ref hideAddersgall, ref update)) TweakConfigs.SGEHideAddersgall = hideAddersgall;
        HideWarning(hideAddersgall);

        if (update.HasFlag(UpdateFlags.Save)) ApplyTweaks();
    }

    public override unsafe void ApplyTweaks()
    {
        var eukrasiaGauge = (AddonJobHudGFF0*)GameGui.GetAddonByName("JobHudGFF0");
        if (eukrasiaGauge != null && eukrasiaGauge->GaugeStandard.Container != null)
        {
            var hideEukrasia = TweakConfigs.SGEHideEukrasia;
            var simple0 = eukrasiaGauge->JobHud.UseSimpleGauge;
            eukrasiaGauge->GaugeStandard.Container->Color.A = (byte)(hideEukrasia || simple0 ? 0 : 255);
            eukrasiaGauge->GaugeSimple.Container->Color.A = (byte)(hideEukrasia || !simple0 ? 0 : 255);
        }

        var addersgallGauge = (AddonJobHudGFF1*)GameGui.GetAddonByName("JobHudGFF1");
        if (addersgallGauge != null && addersgallGauge->GaugeStandard.Container != null)
        {
            var hideAddersgall = TweakConfigs.SGEHideAddersgall;
            var simple1 = addersgallGauge->JobHud.UseSimpleGauge;
            addersgallGauge->GaugeStandard.Container->Color.A = (byte)(hideAddersgall || simple1 ? 0 : 255);
            ((AtkUnitBase*)addersgallGauge)->GetNodeById(25)->Color.A = (byte)(hideAddersgall || !simple1 ? 0 : 255);
        }
    }
}

public partial class TweakConfigs
{
    public bool SGEHideEukrasia { get; set; }
    public bool SGEHideAddersgall { get; set; }
}
