using CustomNodes;
using FFXIVClientStructs.FFXIV.Client.UI;
using GaugeOMatic.Trackers;
using GaugeOMatic.Windows.Dropdowns;
using ImGuiNET;
using System;
using System.Collections.Generic;
using FFXIVClientStructs.FFXIV.Component.GUI;
using static GaugeOMatic.GameData.JobData;
using static GaugeOMatic.GameData.JobData.Job;
using static GaugeOMatic.GameData.JobData.Role;
using static GaugeOMatic.JobModules.Tweaks;
using static GaugeOMatic.Trackers.Tracker;
using static GaugeOMatic.Utility.Color;
using static GaugeOMatic.Widgets.WidgetUI;
using static GaugeOMatic.Windows.Dropdowns.TrackerDropdown;

namespace GaugeOMatic.JobModules;

public class BLMModule : JobModule
{
    public override Job Job => BLM;
    public override Job Class => THM;
    public override Role Role => Caster;

    public override List<AddonOption> AddonOptions => new()
    {
        new("JobHudBLM0", "Elemental Gauge"),
        new("_ParameterWidget", "Parameter Bar")
    };

    public BLMModule(TrackerManager trackerManager, TrackerConfig[] trackerConfigList) : base(trackerManager, trackerConfigList, "JobHudBLM0", "JobHudBLM1") { }

    public override void Save()
    {
        Configuration.TrackerConfigs.BLM = SaveOrder;
        Configuration.Save();
    }

    public override List<MenuOption> JobGaugeMenu { get; } = new()
    {
        new("Element Status", nameof(ElementTracker)),
        new("Astral Fire", nameof(AstralFireTracker)),
        new("Umbral Ice", nameof(UmbralIceTracker)),
        new("Enochian / Polyglot", nameof(EnochianTracker)),
        new("Umbral Hearts", nameof(UmbralHeartTracker)),
        new("Paradox", nameof(ParadoxTracker)),
        new("Astral Soul Stacks", nameof(AstralSoulTracker))
    };

    public override void TweakUI(ref UpdateFlags update)
    {
        Heading("Elemental Gauge");
        ToggleControls("Hide Elemental Gauge", ref TweakConfigs.BLMHide0, ref update);
        ToggleControls("Color MP bar by element", ref TweakConfigs.BLM0MpColor, ref update);
        if (TweakConfigs.BLM0MpColor)
        {
            Info("Changes the color of the MP bar to match your current element"); ImGui.SameLine();
            ImGui.Text("Test");
            ImGui.SameLine();
            if (ImGui.Checkbox("##TweakPreview", ref TweakConfigs.Preview))
            {
                TweakConfigs.TestColor = TweakConfigs.BLM0MpFire;
                update |= UpdateFlags.Save;
            }
            if (ColorPickerRGB("Astral Fire", ref TweakConfigs.BLM0MpFire, ref update) || ImGui.IsItemHovered()) TweakConfigs.TestColor = TweakConfigs.BLM0MpFire;
            if (ColorPickerRGB("Umbral Ice", ref TweakConfigs.BLM0MpIce, ref update) || ImGui.IsItemHovered()) TweakConfigs.TestColor = TweakConfigs.BLM0MpIce;
            if (ColorPickerRGB("None", ref TweakConfigs.BLM0MpNone, ref update) || ImGui.IsItemHovered()) TweakConfigs.TestColor = TweakConfigs.BLM0MpNone;

        }

        Heading("Astral Gauge");
        ToggleControls("Hide Astral Gauge", ref TweakConfigs.BLMHide1, ref update);
    }

    public override unsafe void ApplyTweaks0(IntPtr gaugeAddon)
    {
        var gauge = (AddonJobHudBLM0*)gaugeAddon;
        VisibilityTweak(TweakConfigs.BLMHide0, ((AddonJobHud*)gauge)->UseSimpleGauge, gauge->GaugeStandard.Container, gauge->GaugeSimple.Container);

        MpBarTweak(TweakConfigs.BLM0MpColor, gauge->DataCurrent.ElementActive, gauge->DataCurrent.ElementStacks);
    }

    private unsafe void MpBarTweak(bool tweakActive, bool elementActive = false, int elementStacks = 0)
    {
        var addon = (AtkUnitBase*)GameGui.GetAddonByName("_ParameterWidget");
        if (addon == null) return;

        AddonIndex addonIndex = addon;

        var offset = tweakActive ? addonIndex[4u, 5u].Add : 0;
        var color = tweakActive
                        ? TweakConfigs.ShowPreviews
                              ? TweakConfigs.TestColor ?? TweakConfigs.BLM0MpFire
                              : elementActive
                                  ? elementStacks > 0 ? TweakConfigs.BLM0MpFire : TweakConfigs.BLM0MpIce
                                  : TweakConfigs.BLM0MpNone
                        : 0;

        var componentColor = color - offset;
        var inverse = componentColor * -1;

        // don't feel like fighting with the SimpleTweak or implementing IPC,
        // so we're just going to offset the colors of every other node in the MP bar lmao
        addonIndex[4u].SetAddRGB(componentColor);
        addonIndex[4u, 8u].SetAddRGB(inverse);
        addonIndex[4u, 7u].SetAddRGB(inverse);
        addonIndex[4u, 6u].SetAddRGB(inverse);
        addonIndex[4u, 4u].SetAddRGB(inverse);
        addonIndex[4u, 3u].SetAddRGB(inverse);
        addonIndex[4u, 2u].SetAddRGB(inverse);
    }

    public override unsafe void ApplyTweaks1(IntPtr gaugeAddon)
    {
        var gauge = (AddonJobHudBLM1*)gaugeAddon;
        VisibilityTweak(TweakConfigs.BLMHide1, gauge->UseSimpleGauge, gauge->GaugeStandard.Container, gauge->GetNodeById(15));
    }

    public override void RevertTweaks()
    {
        MpBarTweak(false);
    }
}

public partial class TweakConfigs
{
    public bool BLMHide0;
    public bool BLM0MpColor;
    public AddRGB BLM0MpFire = new(186, 0, -45);
    public AddRGB BLM0MpIce = new(0, 140, 210);
    public AddRGB BLM0MpNone = new(187, 128, 158);

    public bool BLMHide1;
}
