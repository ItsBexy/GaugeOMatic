using FFXIVClientStructs.FFXIV.Client.UI;
using GaugeOMatic.Trackers;
using GaugeOMatic.Windows.Dropdowns;
using System;
using System.Collections.Generic;
using System.Numerics;
using GaugeOMatic.GameData;
using static GaugeOMatic.GameData.JobData;
using static GaugeOMatic.GameData.JobData.Job;
using static GaugeOMatic.GameData.JobData.Role;
using static GaugeOMatic.JobModules.Tweaks;
using static GaugeOMatic.Widgets.Common.WidgetUI;
using static GaugeOMatic.Windows.Dropdowns.TrackerDropdown;

namespace GaugeOMatic.JobModules;

public class PCTModule(TrackerManager trackerManager, TrackerConfig[] trackerConfigList)
    : JobModule(trackerManager, trackerConfigList, "JobHudRPM0", "JobHudRPM1")
{
    public override Job Job => PCT;
    public override Job Class => Job.None;
    public override Role Role => Caster;
    public override List<AddonOption> AddonOptions =>
    [
        new("JobHudRPM0", "Canvases"),
        new("JobHudRPM1", "Palette Gauge"),
        new("_ParameterWidget", "Parameter Bar")
    ];

    public override List<MenuOption> JobGaugeMenu { get; } =
    [
        new("Creature Motif Deadline", nameof(CreatureMotifDeadline)),
        new("Weapon Motif Deadline", nameof(WeaponMotifDeadline)),
        new("Landscape Motif Deadline", nameof(LandscapeMotifDeadline)),
        new("Palette Gauge", nameof(PaletteGaugeTracker))
    ];

    public override void Save()
    {
        Configuration.TrackerConfigs.PCT = SaveOrder;
        Configuration.Save();
    }

    public override void TweakUI()
    {
        Heading("Palette Gauge");
        ToggleControls("Hide Palette Gauge", ref TweakConfigs.PCTHide1);

        Heading("Canvases");
        ToggleControls("Hide Canvases", ref TweakConfigs.PCTHide0);

        if (!TweakConfigs.PCTHide0)
        {
            ToggleControls("Hide Easels", ref TweakConfigs.PCTHide0Easels);

            Heading("Reposition Canvases");
            PositionControls("Creature", ref TweakConfigs.PCT0CanvasPosCreature);
            PositionControls("Weapon", ref TweakConfigs.PCT0CanvasPosWeapon);
            PositionControls("Landscape", ref TweakConfigs.PCT0CanvasPosLandscape);
        }
    }

    public override unsafe void ApplyTweaks0(IntPtr gaugeAddon)
    {
        var gauge = (AddonJobHudRPM0*)gaugeAddon;
        VisibilityTweak(TweakConfigs.PCTHide0, gauge->UseSimpleGauge, gauge->GaugeStandard.Container, gauge->GaugeSimple.Container);

        if (gauge != null && gauge->GaugeStandard.Container != null)
        {
            var hideEasels = TweakConfigs.PCTHide0Easels;

            gauge->GetNodeById(3)->SetPositionFloat(TweakConfigs.PCT0CanvasPosCreature.X, TweakConfigs.PCT0CanvasPosCreature.Y);
            gauge->GetNodeById(19)->SetPositionFloat(TweakConfigs.PCT0CanvasPosWeapon.X + 102, TweakConfigs.PCT0CanvasPosWeapon.Y);
            gauge->GetNodeById(23)->SetPositionFloat(TweakConfigs.PCT0CanvasPosLandscape.X + 204, TweakConfigs.PCT0CanvasPosLandscape.Y);

            gauge->GetNodeById(28)->SetPositionFloat(TweakConfigs.PCT0CanvasPosCreature.X, TweakConfigs.PCT0CanvasPosCreature.Y);
            gauge->GetNodeById(38)->SetPositionFloat(TweakConfigs.PCT0CanvasPosWeapon.X + 78, TweakConfigs.PCT0CanvasPosWeapon.Y);
            gauge->GetNodeById(41)->SetPositionFloat(TweakConfigs.PCT0CanvasPosLandscape.X + 156, TweakConfigs.PCT0CanvasPosLandscape.Y);

            gauge->GetNodeById(10)->ToggleVisibility(!hideEasels);
            gauge->GetNodeById(20)->ToggleVisibility(!hideEasels);
            gauge->GetNodeById(24)->ToggleVisibility(!hideEasels);
        }
    }

    public override unsafe void ApplyTweaks1(IntPtr gaugeAddon)
    {
        var gauge = (AddonJobHudRPM1*)GameGui.GetAddonByName("JobHudRPM1").Address;
        VisibilityTweak(TweakConfigs.PCTHide1, gauge->UseSimpleGauge, gauge->GetNodeById(2), gauge->GetNodeById(28));
    }

    public static float LivingMuseFix() => FrameworkData.LocalPlayer.Lvl < 96 ? 80f : 120f;
}

public partial class TweakConfigs
{
    public bool PCTHide0;
    public bool PCTHide0Easels;
    public bool PCTHide1;
    public Vector2 PCT0CanvasPosCreature;
    public Vector2 PCT0CanvasPosWeapon;
    public Vector2 PCT0CanvasPosLandscape;
}
