using FFXIVClientStructs.FFXIV.Client.UI;
using GaugeOMatic.Trackers;
using GaugeOMatic.Windows.Dropdowns;
using System;
using System.Collections.Generic;
using System.Numerics;
using static GaugeOMatic.GameData.JobData;
using static GaugeOMatic.GameData.JobData.Job;
using static GaugeOMatic.GameData.JobData.Role;
using static GaugeOMatic.JobModules.Tweaks;
using static GaugeOMatic.Widgets.WidgetUI;
using static GaugeOMatic.Windows.Dropdowns.TrackerDropdown;

namespace GaugeOMatic.JobModules;

public class BRDModule(TrackerManager trackerManager, TrackerConfig[] trackerConfigList)
    : JobModule(trackerManager, trackerConfigList, "JobHudBRD0")
{
    public override Job Job => BRD;
    public override Job Class => ARC;
    public override Role Role => Ranged;
    public override List<AddonOption> AddonOptions =>
    [
        new("JobHudBRD0", "Song Gauge"),
        new("_ParameterWidget", "Parameter Bar")
    ];

    public override List<MenuOption> JobGaugeMenu { get; } = [new("Soul Voice Gauge", nameof(SoulVoiceGaugeTracker))];

    public override void Save()
    {
        Configuration.TrackerConfigs.BRD = SaveOrder;
        Configuration.Save();
    }

    public override void TweakUI()
    {
        Heading("Song Gauge");
        ToggleControls("Hide Song Gauge", ref TweakConfigs.BRDHide0Song);

        if (!TweakConfigs.BRDHide0Song)
        {
            ToggleControls("Hide Soul Voice Gauge", ref TweakConfigs.BRDHide0SoulVoice);

            if (!TweakConfigs.BRDHide0SoulVoice)
            {
                PositionControls("Move Soul Voice Gauge", ref TweakConfigs.BRDSoulVoicePos);
            }
        }
    }

    public override unsafe void ApplyTweaks0(IntPtr gaugeAddon)
    {
        var gauge = (AddonJobHudBRD0*)gaugeAddon;
        VisibilityTweak(TweakConfigs.BRDHide0Song, gauge->UseSimpleGauge, gauge->GaugeStandard.Container, gauge->GaugeSimple.Container);
        VisibilityTweak(TweakConfigs.BRDHide0SoulVoice, gauge->UseSimpleGauge, gauge->GaugeStandard.SoulVoiceContainer, gauge->GaugeSimple.SoulVoiceContainer);

        if (gauge != null && gauge->GaugeStandard.Container != null)
        {
            var soulVoicePos = TweakConfigs.BRDSoulVoicePos;
            gauge->GaugeStandard.SoulVoiceContainer->SetPositionFloat(soulVoicePos.X + 16, soulVoicePos.Y + 98);
            gauge->GaugeSimple.SoulVoiceContainer->SetPositionFloat(soulVoicePos.X + 10, soulVoicePos.Y + 39);
        }
    }

    public static float BloodletterFix() => ClientState.LocalPlayer?.Level < 84 ? 30f : 45f;
}

public partial class TweakConfigs
{
    public bool BRDHide0Song;
    public bool BRDHide0SoulVoice;
    public Vector2 BRDSoulVoicePos;
}
