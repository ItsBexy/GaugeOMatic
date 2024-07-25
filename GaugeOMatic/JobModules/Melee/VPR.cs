using CustomNodes;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;
using GaugeOMatic.Trackers;
using GaugeOMatic.Windows;
using ImGuiNET;
using System;
using System.Collections.Generic;
using static GaugeOMatic.GameData.ActionData;
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

public class VPRModule : JobModule
{
    public override Job Job => VPR;
    public override Job Class => Job.None;
    public override Role Role => Melee;

    public override List<AddonOption> AddonOptions => new()
    {
        new("JobHudRDB1", "Serpent Offerings Gauge"),
        new("JobHudRDB0", "Vipersight"),
        new("_ParameterWidget", "Parameter Bar")
    };

    public override List<MenuOption> JobGaugeMenu { get; } = new()
    {
        new("Rattling Coils", nameof(RattlingCoilTracker)),
        new("Serpent Offerings Gauge", nameof(SerpentGaugeTracker)),
        new("Anguine Tribute", nameof(AnguineTributeTracker))
    };

    public VPRModule(TrackerManager trackerManager, TrackerConfig[] trackerConfigList) : base(
        trackerManager, trackerConfigList, "JobHudRDB0", "JobHudRDB1") { }

    public override void Save()
    {
        Configuration.TrackerConfigs.VPR = SaveOrder;
        Configuration.Save();
    }


    public override void TweakUI(ref UpdateFlags update)
    {

        Heading("Vipersight");
        ToggleControls("Hide Vipersight", ref TweakConfigs.VPRHide0, ref update);
        HideInfo(TweakConfigs.VPRHide0);

        if (!TweakConfigs.VPRHide0)
        {
            LabelColumn("Color-Code Vipersight");
            if (ImGui.Checkbox("##BoolColor-Code Vipersight", ref TweakConfigs.VPR0ColorCode)) update |= UpdateFlags.Save;
            if (TweakConfigs.VPR0ColorCode)
            {
                ImGui.SameLine();
                ImGui.Text("Test");
                ImGui.SameLine();
                if (ImGui.Checkbox("##TweakPreview", ref TweakConfigs.Preview)) update |= UpdateFlags.Save;
            }

            if (TweakConfigs.VPR0ColorCode)
            {
                ColorPickerRGB("Flank Venom##VPR0Flank", ref TweakConfigs.VPR0ColorFlank, ref update);
                if (ImGui.IsItemHovered()) TweakConfigs.TestColor = TweakConfigs.VPR0ColorFlank;

                ColorPickerRGB("Hind Venom##VPR0Rear", ref TweakConfigs.VPR0ColorRear, ref update);
                if (ImGui.IsItemHovered()) TweakConfigs.TestColor = TweakConfigs.VPR0ColorRear;

                ColorPickerRGB("Neutral / True North##VPR0Neutral", ref TweakConfigs.VPR0ColorNeutral, ref update);
                if (ImGui.IsItemHovered()) TweakConfigs.TestColor = TweakConfigs.VPR0ColorNeutral;

            }
        }

        Heading("Serpent Offerings Gauge");
        ToggleControls("Hide Serpent Offerings Gauge", ref TweakConfigs.VPRHide1, ref update);
        HideInfo(TweakConfigs.VPRHide1);

        ToggleControls("Ready to Reawaken Cue", ref TweakConfigs.VPR1ReawakenCue, ref update);
    }

    public override unsafe void ApplyTweaks0(IntPtr gaugeAddon)
    {
        var gauge = (AddonJobHudRDB0*)gaugeAddon;
        var gaugeIndex = (AddonIndex)gaugeAddon;
        VisibilityTweak(TweakConfigs.VPRHide0, gauge->UseSimpleGauge, gaugeIndex[2u], gaugeIndex[10u]);
        ApplyColorCodeTweak(gauge);
    }

    public override unsafe void ApplyTweaks1(IntPtr gaugeAddon)
    {
        var gauge = (AddonJobHudRDB1*)gaugeAddon;
        var gaugeIndex = (AddonIndex)gaugeAddon;
        VisibilityTweak(TweakConfigs.VPRHide1, gauge->UseSimpleGauge, gaugeIndex[13u], gaugeIndex[2u]);
        ApplyReawakenCueTweak(gauge);
    }

    private unsafe void ApplyReawakenCueTweak(AddonJobHudRDB1* serpentGauge)
    {
        if (TweakConfigs.VPR1ReawakenCue && Statuses[3671].TryGetStatus() && serpentGauge->DataCurrent.GaugeValue < 50)
            UIModule.Instance()->GetRaptureAtkModule()->GetNumberArrayData(86)->SetValue(9, 0, true);
    }

    private unsafe void ApplyColorCodeTweak(AddonJobHudRDB0* vipersight)
    {
        var neutral = TweakConfigs.VPR0ColorNeutral;
        var flank = TweakConfigs.VPR0ColorFlank;
        var rear = TweakConfigs.VPR0ColorRear;

        if (vipersight != null && vipersight->GaugeStandard.ViperBlades != null && vipersight->GaugeStandard.ViperBlades->LeftBlade != null)
        {
            var gaugeIndex = (AddonIndex)(AtkUnitBase*)vipersight;
            var leftGlowWrapper = gaugeIndex[7u,2u];
            var rightGlowWrapper = gaugeIndex[8u,2u];

            if (TweakConfigs.VPR0ColorCode)
            {
                var (leftColor, rightColor, simpleColor) = TweakConfigs.ShowPreviews ? GetPreviewColors() : GetColors();
                var colorOffset = vipersight->DataCurrent.ComboStep == 2 ? new AddRGB(0, -150, -255) : new AddRGB(-255, 0, 0);

                leftGlowWrapper.SetAddRGB(colorOffset + leftColor);
                rightGlowWrapper.SetAddRGB(colorOffset + rightColor);
                RecolorSimpleGlow(simpleColor);
            }
            else
            {
                leftGlowWrapper.SetAddRGB(0);
                rightGlowWrapper.SetAddRGB(0);
                RevertSimpleGlow();
            }
        }

        (AddRGB leftColor, AddRGB rightColor, AddRGB simpleColor) GetColors()
        {
            var appliedColor = Statuses[1250].TryGetStatus()
                                   ? neutral
                                   : ActionManager->GetAdjustedActionId(34607)
                                       switch // check what state the Dread Fangs button is in
                                       {
                                           34611 => flank, // flank positional finishers are up
                                           34613 => rear,  // rear positional finishers are up
                                           34609 when CheckForAnts(34610, 34611) => flank, // swiftskin is up, a flank positional has ants
                                           34609 when CheckForAnts(34612, 34613) => rear, // swiftskin is up, a rear positional has ants
                                           _ => neutral
                                       };

            return (appliedColor, appliedColor, appliedColor);
        }

        (AddRGB leftColor, AddRGB rightColor, AddRGB simpleColor) GetPreviewColors()
        {
            JobUiData->SetValue(2, 1, true);
            JobUiData->SetValue(3, 1, true);

            var useNeutral = TweakConfigs.TestColor == neutral;

            return (useNeutral ? neutral : flank,
                       useNeutral ? neutral : rear,
                       TweakConfigs.TestColor ?? neutral);
        }

        void RecolorSimpleGlow(AddRGB color) => SetSimpleGlow(color, color, color, color);
        void RevertSimpleGlow() => SetSimpleGlow(new(0, -200, -200), new(-200, -200, 0), new(255, 0, 0), new(0, 50, 255));

        void SetSimpleGlow(AddRGB color1, AddRGB color2, AddRGB color3, AddRGB color4)
        {
            var gaugeIndex = (AddonIndex)(AtkUnitBase*)vipersight;

            var leftGlowSimple = gaugeIndex[16u,3u];
            var leftFrameSimple = gaugeIndex[16u,4u];

            leftGlowSimple.SetKeyFrameAddRGB(color1, (0, 4, 0), (0, 4, 1), (0, 4, 2));
            leftGlowSimple.SetKeyFrameAddRGB(color2, (1, 4, 0), (1, 4, 1), (1, 4, 2));

            leftFrameSimple.SetKeyFrameAddRGB(color3, 1, 4, 1);
            leftFrameSimple.SetKeyFrameAddRGB(color4, 2, 4, 1);
        }
    }
}


public partial class TweakConfigs
{
    public bool VPRHide0;
    public bool VPR0ColorCode;
    public AddRGB VPR0ColorRear = new(251,-134,31);
    public AddRGB VPR0ColorFlank = new(-150,255,140);
    public AddRGB VPR0ColorNeutral = new(255, 180, 80);

    public bool VPRHide1;
    public bool VPR1ReawakenCue;
}
