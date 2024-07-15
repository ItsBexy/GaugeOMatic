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

        if (update.HasFlag(UpdateFlags.Save)) ApplyTweaks0();
    }

    public override unsafe void ApplyTweaks0()
    {
        var songGauge = (AddonJobHudBRD0*)GameGui.GetAddonByName("JobHudBRD0");
        if (songGauge != null && songGauge->GaugeStandard.Container != null)
        {
            var simple0 = ((AddonJobHud*)songGauge)->UseSimpleGauge;

            var hideSong = TweakConfigs.BRDHide0Song;
            songGauge->GaugeStandard.Container->Color.A = (byte)(hideSong || simple0 ? 0 : 255);
            songGauge->GaugeSimple.Container->Color.A = (byte)(hideSong || !simple0 ? 0 : 255);

            var hideSoulVoice = TweakConfigs.BRDHide0SoulVoice;
            songGauge->GaugeStandard.SoulVoiceContainer->Color.A = (byte)(hideSoulVoice || simple0 ? 0 : 255);
            songGauge->GaugeSimple.SoulVoiceContainer->Color.A = (byte)(hideSoulVoice || !simple0 ? 0 : 255);

            var soulVoicePos = TweakConfigs.BRDSoulVoicePos;
            songGauge->GaugeStandard.SoulVoiceContainer->SetPositionFloat(soulVoicePos.X+16, soulVoicePos.Y+98);
            songGauge->GaugeSimple.SoulVoiceContainer->SetPositionFloat(soulVoicePos.X + 10, soulVoicePos.Y + 39);
        }
    }
}

public partial class TweakConfigs
{
    public bool BRDHide0Song;
    public bool BRDHide0SoulVoice;
    public Vector2 BRDSoulVoicePos;
}
