using FFXIVClientStructs.FFXIV.Client.UI;
using GaugeOMatic.Trackers;
using GaugeOMatic.Windows;
using System.Collections.Generic;
using System.Numerics;
using static GaugeOMatic.GameData.JobData;
using static GaugeOMatic.GameData.JobData.Job;
using static GaugeOMatic.GameData.JobData.Role;
using static GaugeOMatic.JobModules.TweakUI;
using static GaugeOMatic.Widgets.WidgetUI;
using static GaugeOMatic.Windows.ItemRefMenu;

namespace GaugeOMatic.JobModules;

public class SAMModule : JobModule
{
    public override Job Job => SAM;
    public override Job Class => Job.None;
    public override Role Role => Melee;
    public override List<AddonOption> AddonOptions => new()
    {
        new("JobHudSAM0", "Kenki Gauge"),
        new("JobHudSAM1", "Sen Gauge"),
        new("_ParameterWidget", "Parameter Bar")
    };

    public override List<MenuOption> JobGaugeMenu { get; } = new() {
        new("Kenki Gauge", nameof(KenkiGaugeTracker)),
        new("Meditation Gauge", nameof(MeditationGaugeTracker)),
        new("Sen Gauge - Seal Count", nameof(SenSealTracker)),
        new("Sen Gauge - Setsu Seal", nameof(SenGaugeSetsuTracker)),
        new("Sen Gauge - Getsu Seal", nameof(SenGaugeGetsuTracker)),
        new("Sen Gauge - Ka Seal", nameof(SenGaugeKaTracker))
    };

    public SAMModule(TrackerManager trackerManager, TrackerConfig[] trackerConfigList) : base(trackerManager, trackerConfigList) { }

    public override void Save()
    {
        Configuration.TrackerConfigs.SAM = SaveOrder;
        Configuration.Save();
    }

    public override void TweakUI(ref UpdateFlags update)
    {
        ToggleControls("Hide Kenki Gauge", ref TweakConfigs.SAMHideKenki, ref update);
        HideWarning(TweakConfigs.SAMHideKenki);

        ToggleControls("Hide Meditation Gauge", ref TweakConfigs.SAMHideMeditation, ref update);
        HideWarning(TweakConfigs.SAMHideMeditation);

        ToggleControls("Hide Sen Gauge", ref TweakConfigs.SAMHideSen, ref update);
        HideWarning(TweakConfigs.SAMHideSen);

        if (!TweakConfigs.SAMHideSen)
        {
            Heading("Reposition Seals");
            PositionControls("Setsu", ref TweakConfigs.SAMSealPosSetsu, ref update);
            PositionControls("Getsu", ref TweakConfigs.SAMSealPosGetsu, ref update);
            PositionControls("Ka", ref TweakConfigs.SAMSealPosKa, ref update);
        }

        if (update.HasFlag(UpdateFlags.Save)) ApplyTweaks();
    }

    public override unsafe void ApplyTweaks()
    {
        var kenkiGauge = (AddonJobHudSAM0*)GameGui.GetAddonByName("JobHudSAM0");
        if (kenkiGauge != null && kenkiGauge->GaugeStandard.KenkiContainer != null)
        {
            var simple0 = kenkiGauge->JobHud.UseSimpleGauge;
            var hideKenki = TweakConfigs.SAMHideKenki;
            var hideMeditation = TweakConfigs.SAMHideMeditation;

            kenkiGauge->GaugeStandard.KenkiContainer->Color.A = (byte)(hideKenki || simple0 ? 0 : 255);
            kenkiGauge->GaugeStandard.MeditationContainer->Color.A = (byte)(hideMeditation || simple0 ? 0 : 255);

            kenkiGauge->GaugeSimple.KenkiContainer->Color.A = (byte)(hideKenki || !simple0 ? 0 : 255);
            kenkiGauge->GaugeSimple.MeditationContainer->Color.A = (byte)(hideMeditation || !simple0 ? 0 : 255);
        }

        var senGauge = (AddonJobHudSAM1*)GameGui.GetAddonByName("JobHudSAM1");
        if (senGauge != null && senGauge->GaugeStandard.Container != null)
        {
            var hideSen = TweakConfigs.SAMHideSen;
            var simple1 = senGauge->JobHud.UseSimpleGauge;
            senGauge->GaugeStandard.Container->ToggleVisibility(!hideSen && !simple1);
            senGauge->GaugeSimple.Container->Color.A = (byte)(hideSen || !simple1 ? 0 : 255);

            if (!hideSen)
            {
                var setsuPos = TweakConfigs.SAMSealPosSetsu;
                var getsuPos = TweakConfigs.SAMSealPosGetsu;
                var kaPos = TweakConfigs.SAMSealPosKa;
                if (!simple1)
                {
                    senGauge->GaugeStandard.SetsuNode->SetPositionFloat(setsuPos.X + 44, setsuPos.Y + 4);
                    senGauge->GaugeStandard.GetsuNode->SetPositionFloat(getsuPos.X, getsuPos.Y + 76);
                    senGauge->GaugeStandard.KaNode->SetPositionFloat(kaPos.X + 90, kaPos.Y + 73);
                }
                else
                {
                    senGauge->GaugeSimple.SetsuNode->SetPositionFloat(setsuPos.X + 0, setsuPos.Y);
                    senGauge->GaugeSimple.GetsuNode->SetPositionFloat(getsuPos.X + 19, getsuPos.Y);
                    senGauge->GaugeSimple.KaNode->SetPositionFloat(kaPos.X + 38, kaPos.Y);
                }
            }
        }
    }
}

public partial class TweakConfigs
{
    public bool SAMHideKenki;
    public bool SAMHideMeditation;
    public bool SAMHideSen;
    public Vector2 SAMSealPosSetsu;
    public Vector2 SAMSealPosGetsu;
    public Vector2 SAMSealPosKa;
}
