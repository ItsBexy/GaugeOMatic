using GaugeOMatic.Trackers;
using GaugeOMatic.Windows;
using System.Collections.Generic;
using System.Numerics;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;
using static GaugeOMatic.GameData.JobData;
using static GaugeOMatic.GameData.JobData.Job;
using static GaugeOMatic.GameData.JobData.Role;
using static GaugeOMatic.Widgets.WidgetUI;
using static GaugeOMatic.Windows.ItemRefMenu;
using static GaugeOMatic.JobModules.TweakUI;

namespace GaugeOMatic.JobModules;

public class PCTModule : JobModule
{
    public override Job Job => PCT;
    public override Job Class => Job.None;
    public override Role Role => Caster;
    public override List<AddonOption> AddonOptions => new()
    {
        new("JobHudRPM0", "Canvases"),
        new("JobHudRPM1", "Palette Gauge"),
        new("_ParameterWidget", "Parameter Bar")
    };

    public override List<MenuOption> JobGaugeMenu { get; } = new() {
        new("Creature Motif Deadline", nameof(CreatureMotifDeadline)),
        new("Weapon Motif Deadline", nameof(WeaponMotifDeadline)),
        new("Landscape Motif Deadline", nameof(LandscapeMotifDeadline))
    };

    public PCTModule(TrackerManager trackerManager, TrackerConfig[] trackerConfigList) : base(trackerManager, trackerConfigList, "JobHudRPM0", "JobHudRPM1") { }

    public override void Save()
    {
        Configuration.TrackerConfigs.PCT = SaveOrder;
        Configuration.Save();
    }

    public override void TweakUI(ref UpdateFlags update)
    {
        ToggleControls("Hide Palette Gauge", ref TweakConfigs.PCTHide1, ref update);
        HideWarning(TweakConfigs.PCTHide1);
        ToggleControls("Hide Canvases", ref TweakConfigs.PCTHide0, ref update);
        HideWarning(TweakConfigs.PCTHide0);

        if (!TweakConfigs.PCTHide0)
        {
            ToggleControls("Hide Easels", ref TweakConfigs.PCTHide0Easels, ref update);

            Heading("Reposition Canvases");
            PositionControls("Creature", ref TweakConfigs.PCT0CanvasPosCreature, ref update);
            PositionControls("Weapon", ref TweakConfigs.PCT0CanvasPosWeapon, ref update);
            PositionControls("Landscape", ref TweakConfigs.PCT0CanvasPosLandscape, ref update);
        }

        if (update.HasFlag(UpdateFlags.Save))
        {
            ApplyTweaks0();
            ApplyTweaks1();
        }
    }

    public override unsafe void ApplyTweaks0() {
        var canvasGauge = (AddonJobHudRPM0*)GameGui.GetAddonByName("JobHudRPM0");

        if (canvasGauge != null)
        {
            var hide0 = TweakConfigs.PCTHide0;
            var simple = ((AddonJobHud*)canvasGauge)->UseSimpleGauge;
            canvasGauge->GaugeStandard.Container->Color.A = (byte)(hide0 || simple ? 0 : 255);
            canvasGauge->GaugeSimple.Container->Color.A = (byte)(hide0 || !simple ? 0 : 255);

            ((AtkUnitBase*)canvasGauge)->GetNodeById(3)->SetPositionFloat(TweakConfigs.PCT0CanvasPosCreature.X, TweakConfigs.PCT0CanvasPosCreature.Y);
            ((AtkUnitBase*)canvasGauge)->GetNodeById(19)->SetPositionFloat(TweakConfigs.PCT0CanvasPosWeapon.X + 102, TweakConfigs.PCT0CanvasPosWeapon.Y);
            ((AtkUnitBase*)canvasGauge)->GetNodeById(23)->SetPositionFloat(TweakConfigs.PCT0CanvasPosLandscape.X + 204, TweakConfigs.PCT0CanvasPosLandscape.Y);

            ((AtkUnitBase*)canvasGauge)->GetNodeById(28)->SetPositionFloat(TweakConfigs.PCT0CanvasPosCreature.X, TweakConfigs.PCT0CanvasPosCreature.Y);
            ((AtkUnitBase*)canvasGauge)->GetNodeById(38)->SetPositionFloat(TweakConfigs.PCT0CanvasPosWeapon.X + 78, TweakConfigs.PCT0CanvasPosWeapon.Y);
            ((AtkUnitBase*)canvasGauge)->GetNodeById(41)->SetPositionFloat(TweakConfigs.PCT0CanvasPosLandscape.X + 156, TweakConfigs.PCT0CanvasPosLandscape.Y);

            var hideEasels = TweakConfigs.PCTHide0Easels;
            ((AtkUnitBase*)canvasGauge)->GetNodeById(10)->ToggleVisibility(!hideEasels);
            ((AtkUnitBase*)canvasGauge)->GetNodeById(20)->ToggleVisibility(!hideEasels);
            ((AtkUnitBase*)canvasGauge)->GetNodeById(24)->ToggleVisibility(!hideEasels);
        }
    }

    public override unsafe void ApplyTweaks1()
    {
        var paletteGauge = (AddonJobHudRPM1*)GameGui.GetAddonByName("JobHudRPM1");

        if (paletteGauge != null)
        {
            var hide1 = TweakConfigs.PCTHide1;
            var simple = ((AddonJobHud*)paletteGauge)->UseSimpleGauge;
            ((AtkUnitBase*)paletteGauge)->GetNodeById(2)->Color.A = (byte)(hide1 || simple ? 0 : 255);
            ((AtkUnitBase*)paletteGauge)->GetNodeById(28)->Color.A = (byte)(hide1 || !simple ? 0 : 255);
        }
    }
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
