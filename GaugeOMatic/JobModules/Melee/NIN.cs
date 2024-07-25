using CustomNodes;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;
using GaugeOMatic.Trackers;
using GaugeOMatic.Windows;
using ImGuiNET;
using System;
using System.Collections.Generic;
using static GaugeOMatic.GameData.JobData;
using static GaugeOMatic.GameData.JobData.Job;
using static GaugeOMatic.GameData.JobData.Role;
using static GaugeOMatic.GameData.StatusData;
using static GaugeOMatic.JobModules.Tweaks;
using static GaugeOMatic.JobModules.Tweaks.TweakUI;
using static GaugeOMatic.Utility.Color;
using static GaugeOMatic.Widgets.WidgetUI;
using static GaugeOMatic.Windows.ItemRefMenu;

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
        new("Ninki Gauge", nameof(NinkiGaugeTracker)),
        new("Kazematoi Stacks", nameof(KazematoiTracker))
    };

    public NINModule(TrackerManager trackerManager, TrackerConfig[] trackerConfigList) : base(trackerManager, trackerConfigList, "JobHudNIN0", "JobHudNIN1v70") { }

    public override void Save()
    {
        Configuration.TrackerConfigs.NIN = SaveOrder;
        Configuration.Save();
    }

    public override unsafe void TweakUI(ref UpdateFlags update)
    {
        Heading("Ninki Gauge");
        ToggleControls("Hide Ninki Gauge", ref TweakConfigs.NINHide0, ref update);
        HideInfo(TweakConfigs.NINHide0);

        LabelColumn("Change color under Higi");
        if (ImGui.Checkbox("##BoolChange color under Higi", ref TweakConfigs.NIN0HigiRecolor)) update |= UpdateFlags.Save;
        if (TweakConfigs.NIN0HigiRecolor)
        {
            ImGui.SameLine();
            ImGui.Text("Test");
            ImGui.SameLine();
            if (ImGui.Checkbox("##TweakPreview", ref TweakConfigs.Preview)) update |= UpdateFlags.Save;
        }

        if (TweakConfigs.NIN0HigiRecolor)
        {
            var gauge0 = (AddonJobHudNIN0*)GameGui.GetAddonByName("JobHudNIN0");
            if (gauge0 != null && gauge0->UseSimpleGauge)
            {
                ColorPickerRGB("Fill Color", ref TweakConfigs.NIN0HigiColor3, ref update);
                ColorPickerRGB("Frame Tint", ref TweakConfigs.NIN0HigiColor4, ref update);
            }
            else
            {
                ColorPickerRGB("Glow Color", ref TweakConfigs.NIN0HigiColor1, ref update);
                ColorPickerRGB("Scroll Tint", ref TweakConfigs.NIN0HigiColor2, ref update);
            }
        }

        Heading("Kazematoi");
        ToggleControls("Hide Kazematoi", ref TweakConfigs.NINHide1, ref update);
        HideInfo(TweakConfigs.NINHide1);
    }

    public override unsafe void ApplyTweaks0(IntPtr gaugeAddon)
    {
        var gauge = (AddonJobHudNIN0*)gaugeAddon;
        VisibilityTweak(TweakConfigs.NINHide0, gauge->UseSimpleGauge, gauge->GaugeStandard.Container, gauge->GaugeSimple.Container);

        HigiTweak(gauge);
    }

    private unsafe void HigiTweak(AddonJobHudNIN0* gauge)
    {
        if (gauge == null) return;
        var gaugeIndex = (AddonIndex)(AtkUnitBase*)gauge;

        var hGaugeNode = gaugeIndex[14u];
        var vGaugeNode = gaugeIndex[15u, 3u];
        var scrollNode = gaugeIndex[13u];

        var simpleBarFill = gaugeIndex[19u, 4];
        var simpleFrame = gaugeIndex[19u, 2u];

        var borderTop = gaugeIndex[9u];
        var borderBottom = gaugeIndex[10u];

        if (TweakConfigs.NIN0HigiRecolor && (TweakConfigs.ShowPreviews || Statuses[3850].TryGetStatus()))
        {
            if (TweakConfigs.ShowPreviews) JobUiData->SetValue(0, 100, true);

            var fillColorH = TweakConfigs.NIN0HigiColor1 + new AddRGB(0, 70, -100);
            var fillColorV = TweakConfigs.NIN0HigiColor1 + new AddRGB(100);
            var scrollColor = TweakConfigs.NIN0HigiColor2;

            hGaugeNode.SetKeyFrameAddRGB(fillColorH, (1, 4, 1), (2, 4, 0));
            vGaugeNode.SetAddRGB(fillColorV);

            scrollNode.SetKeyFrameAddRGB(scrollColor, (0, 4, 0), (1, 4, 0), (1, 4, 2), (2, 4, 0), (2, 4, 2));
            scrollNode.SetKeyFrameAddRGB(scrollColor + new AddRGB(100, 0, 0), 1, 4, 1);
            scrollNode.SetKeyFrameAddRGB(scrollColor + new AddRGB(30, 0, 20), 2, 4, 1);
            scrollNode.SetAddRGB(scrollColor);

            if (gauge->DataCurrent.NinkiValue >= 50) simpleBarFill.SetAddRGB(TweakConfigs.NIN0HigiColor3);

            simpleBarFill.SetKeyFrameAddRGB(TweakConfigs.NIN0HigiColor3, (1, 4, 0));
            simpleFrame.SetAddRGB(TweakConfigs.NIN0HigiColor4);
            borderTop.SetAddRGB(TweakConfigs.NIN0HigiColor1);
            borderBottom.SetAddRGB(TweakConfigs.NIN0HigiColor1);
        }
        else
        {
            hGaugeNode.SetKeyFrameAddRGB(new(200, -20, -250), (1, 4, 1), (2, 4, 0));
            vGaugeNode.SetAddRGB(150, 20, -100);

            scrollNode.SetKeyFrameAddRGB(new(0), (0, 4, 0), (1, 4, 0), (1, 4, 2), (2, 4, 0), (2, 4, 2));
            scrollNode.SetKeyFrameAddRGB(new(100, 0, 0), 1, 4, 1);
            scrollNode.SetKeyFrameAddRGB(new(30, 0, 20), 2, 4, 1);
            scrollNode.SetAddRGB(0);

            if (gauge->DataCurrent.NinkiValue >= 50) simpleBarFill.SetAddRGB(150, 0, 0);

            simpleBarFill.SetKeyFrameAddRGB(new(150, 0, 0), (1, 04, 0));
            simpleFrame.SetAddRGB(0);
            borderTop.SetAddRGB(100, -50, -120);
            borderBottom.SetAddRGB(100, -50, -120);
        }
    }

    public override unsafe void ApplyTweaks1(IntPtr gaugeAddon)
    {
        var gauge = (AddonJobHudNIN1*)gaugeAddon;
        var gaugeIndex = (AddonIndex)gaugeAddon;
        VisibilityTweak(TweakConfigs.NINHide1, gauge->UseSimpleGauge, gauge->GaugeStandard.Container, gaugeIndex[17u]);
    }
}

public partial class TweakConfigs
{
    public bool NINHide1;

    public bool NINHide0;
    public bool NIN0HigiRecolor;
    public AddRGB NIN0HigiColor1 = new(255, -185, -205);
    public AddRGB NIN0HigiColor2 = new(0, 0, 50);
    public AddRGB NIN0HigiColor3 = new(171, -107, -46);
    public AddRGB NIN0HigiColor4 = new(-31, -20, 89);
}
