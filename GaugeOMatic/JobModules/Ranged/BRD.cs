using FFXIVClientStructs.FFXIV.Client.UI;
using GaugeOMatic.Trackers;
using GaugeOMatic.Windows;
using System;
using System.Collections.Generic;
using System.Numerics;
using static GaugeOMatic.GameData.JobData;
using static GaugeOMatic.GameData.JobData.Job;
using static GaugeOMatic.GameData.JobData.Role;
using static GaugeOMatic.JobModules.Tweaks;
using static GaugeOMatic.JobModules.Tweaks.TweakUI;
using static GaugeOMatic.Widgets.WidgetUI;
using static GaugeOMatic.Windows.ItemRefMenu;

namespace GaugeOMatic.JobModules;

public class BRDModule : JobModule
{
    public override Job Job => BRD;
    public override Job Class => ARC;
    public override Role Role => Ranged;
    public override List<AddonOption> AddonOptions => new()
    {
        new("JobHudBRD0", "Song Gauge"),
        new("_ParameterWidget", "Parameter Bar")
    };

    public override List<MenuOption> JobGaugeMenu { get; } = new()
    {
        new ("Soul Voice Gauge", nameof(SoulVoiceGaugeTracker))
    };

    public BRDModule(TrackerManager trackerManager, TrackerConfig[] trackerConfigList) : base(trackerManager, trackerConfigList, "JobHudBRD0") { }

    public override void Save()
    {
        Configuration.TrackerConfigs.BRD = SaveOrder;
        Configuration.Save();
    }

    public override void TweakUI(ref UpdateFlags update)
    {
        ToggleControls("Hide Song Gauge", ref TweakConfigs.BRDHide0Song, ref update);
        HideWarning(TweakConfigs.BRDHide0Song);

        if (!TweakConfigs.BRDHide0Song)
        {
            ToggleControls("Hide Soul Voice Gauge", ref TweakConfigs.BRDHide0SoulVoice, ref update);
            HideWarning(TweakConfigs.BRDHide0SoulVoice);

            if (!TweakConfigs.BRDHide0SoulVoice)
            {
                PositionControls("Move Soul Voice Gauge", ref TweakConfigs.BRDSoulVoicePos, ref update);
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
}

public partial class TweakConfigs
{
    public bool BRDHide0Song;
    public bool BRDHide0SoulVoice;
    public Vector2 BRDSoulVoicePos;
}
