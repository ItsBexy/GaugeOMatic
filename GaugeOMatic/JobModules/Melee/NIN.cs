using CustomNodes;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;
using GaugeOMatic.GameData;
using GaugeOMatic.Trackers;
using GaugeOMatic.Windows.Dropdowns;
using ImGuiNET;
using System;
using System.Collections.Generic;
using Dalamud.Interface;
using Dalamud.Interface.Components;
using static GaugeOMatic.GameData.JobData;
using static GaugeOMatic.GameData.JobData.Job;
using static GaugeOMatic.GameData.JobData.Role;
using static GaugeOMatic.GameData.StatusRef.StatusActor;
using static GaugeOMatic.JobModules.Tweaks;
using static GaugeOMatic.Utility.Color;
using static GaugeOMatic.Widgets.Common.WidgetUI;
using static GaugeOMatic.Windows.Dropdowns.TrackerDropdown;

namespace GaugeOMatic.JobModules;

public class NINModule(TrackerManager trackerManager, TrackerConfig[] trackerConfigList)
    : JobModule(trackerManager, trackerConfigList, "JobHudNIN0", "JobHudNIN1v70")
{
    public override Job Job => NIN;
    public override Job Class => ROG;
    public override Role Role => Melee;
    public override List<AddonOption> AddonOptions =>
    [
        new("JobHudNIN0", "Ninki Gauge"),
        new("JobHudNIN1v70", "Kazematoi"),
        new("_ParameterWidget", "Parameter Bar")
    ];

    public override List<MenuOption> JobGaugeMenu { get; } =
    [
        new("Ninki Gauge", nameof(NinkiGaugeTracker)),
        new("Kazematoi Stacks", nameof(KazematoiTracker))
    ];

    public override void Save()
    {
        Configuration.TrackerConfigs.NIN = SaveOrder;
        Configuration.Save();
    }

    public override unsafe void TweakUI()
    {
        Heading("Ninki Gauge");
        ToggleControls("Hide Ninki Gauge", ref TweakConfigs.NINHide0);

        LabelColumn("Higi Indicator");
        if (ImGui.Checkbox("##BoolChange color under Higi", ref TweakConfigs.NIN0HigiRecolor)) UpdateFlag |= UpdateFlags.Save;
        if (TweakConfigs.NIN0HigiRecolor)
        {
            ImGuiComponents.HelpMarker("Changes the color of the gauge while the Higi buff is active.",FontAwesomeIcon.QuestionCircle);

            ImGui.SameLine();
            ImGui.Text("Test");
            ImGui.SameLine();
            if (ImGui.Checkbox("##TweakPreview", ref TweakConfigs.Preview)) UpdateFlag |= UpdateFlags.Save;

            var gauge0 = (AddonJobHudNIN0*)GameGui.GetAddonByName("JobHudNIN0");
            if (gauge0 != null && gauge0->UseSimpleGauge)
            {
                ColorPickerRGB("Fill Color", ref TweakConfigs.NIN0HigiColor3);
                ColorPickerRGB("Frame Tint", ref TweakConfigs.NIN0HigiColor4);
            }
            else
            {
                ColorPickerRGB("Glow Color", ref TweakConfigs.NIN0HigiColor1);
                ColorPickerRGB("Scroll Tint", ref TweakConfigs.NIN0HigiColor2);
            }
        }

        Heading("Kazematoi");
        ToggleControls("Hide Kazematoi", ref TweakConfigs.NINHide1);
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

        if (TweakConfigs.NIN0HigiRecolor && (TweakConfigs.ShowPreviews || ((StatusRef)3850).TryGetStatus(Self)))
        {
            if (TweakConfigs.ShowPreviews) JobUiData->SetValue(0, 100, true);

            var fillColorH = TweakConfigs.NIN0HigiColor1 + new AddRGB(0, 70, -100);
            var fillColorV = TweakConfigs.NIN0HigiColor1 + new AddRGB(100);
            var scrollColor = TweakConfigs.NIN0HigiColor2;

            hGaugeNode.SetKeyFrameAddRGB(fillColorH, (1, 1), (2, 0));
            vGaugeNode.SetAddRGB(fillColorV);

            scrollNode.SetKeyFrameAddRGB(scrollColor, (0, 0), (1, 0), (1, 2), (2, 0), (2, 2))
                      .SetKeyFrameAddRGB(scrollColor + new AddRGB(100, 0, 0), 1, 1)
                      .SetKeyFrameAddRGB(scrollColor + new AddRGB(30, 0, 20), 2, 1)
                      .SetAddRGB(scrollColor);

            if (gauge->DataCurrent.NinkiValue >= 50) simpleBarFill.SetAddRGB(TweakConfigs.NIN0HigiColor3);

            simpleBarFill.SetKeyFrameAddRGB(TweakConfigs.NIN0HigiColor3, (1, 0));
            simpleFrame.SetAddRGB(TweakConfigs.NIN0HigiColor4);
            borderTop.SetAddRGB(TweakConfigs.NIN0HigiColor1);
            borderBottom.SetAddRGB(TweakConfigs.NIN0HigiColor1);
        }
        else
        {
            hGaugeNode.SetKeyFrameAddRGB(new(200, -20, -250), (1, 1), (2, 0));
            vGaugeNode.SetAddRGB(150, 20, -100);

            scrollNode.SetKeyFrameAddRGB(new(0), (0, 0), (1, 0), (1, 2), (2, 0), (2, 2))
                      .SetKeyFrameAddRGB(new(100, 0, 0), 1, 1)
                      .SetKeyFrameAddRGB(new(30, 0, 20), 2, 1)
                      .SetAddRGB(0);

            if (gauge->DataCurrent.NinkiValue >= 50) simpleBarFill.SetAddRGB(150, 0, 0);

            simpleBarFill.SetKeyFrameAddRGB(new(150, 0, 0), 1, 0);
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
