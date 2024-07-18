using CustomNodes;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;
using GaugeOMatic.GameData;
using GaugeOMatic.Trackers;
using GaugeOMatic.Windows;
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

    public override List<MenuOption> JobGaugeMenu { get; } = new() {
        new("Rattling Coils", nameof(RattlingCoilTracker)),
        new("Serpent Offerings Gauge", nameof(SerpentGaugeTracker)),
        new("Anguine Tribute", nameof(AnguineTributeTracker))
    };

    public VPRModule(TrackerManager trackerManager, TrackerConfig[] trackerConfigList) : base(trackerManager, trackerConfigList, "JobHudRDB0", "JobHudRDB1") { }

    public override void Save()
    {
        Configuration.TrackerConfigs.VPR = SaveOrder;
        Configuration.Save();
    }

    public override void TweakUI(ref UpdateFlags update)
    {
        ToggleControls("Hide Vipersight", ref TweakConfigs.VPRHide0, ref update);
        HideWarning(TweakConfigs.VPRHide0);
        ToggleControls("Hide Serpent Offerings Gauge", ref TweakConfigs.VPRHide1, ref update);
        HideWarning(TweakConfigs.VPRHide1);

        ToggleControls("Ready to Reawaken Cue", ref TweakConfigs.VPR1ReawakenCue, ref update);

        if (!TweakConfigs.VPRHide0)
        {
            ToggleControls("Color-Code Vipersight", ref TweakConfigs.VPR0ColorCode, ref update);

            if (TweakConfigs.VPR0ColorCode)
            {
                ColorPickerRGB("Hind Venom##VPR0Rear", ref TweakConfigs.VPR0ColorRear, ref update);
                ColorPickerRGB("Flank Venom##VPR0Flank", ref TweakConfigs.VPR0ColorFlank, ref update);
                ColorPickerRGB("Neutral / True North##VPR0Neutral", ref TweakConfigs.VPR0ColorNeutral, ref update);
            }
        }
    }

    public override unsafe void ApplyTweaks0(IntPtr gaugeAddon)
    {
        var gauge = (AddonJobHudRDB0*)gaugeAddon;
        VisibilityTweak(TweakConfigs.VPRHide0, gauge->UseSimpleGauge, gauge->GetNodeById(2), gauge->GetNodeById(10));
        ApplyColorCodeTweak(gauge);
    }

    public override unsafe void ApplyTweaks1(IntPtr gaugeAddon)
    {
        var gauge = (AddonJobHudRDB1*)gaugeAddon;
        VisibilityTweak(TweakConfigs.VPRHide1, gauge->UseSimpleGauge, gauge->GetNodeById(13), gauge->GetNodeById(2));
        ApplyReawakenCueTweak(gauge);
    }

    private unsafe void ApplyReawakenCueTweak(AddonJobHudRDB1* serpentGauge)
    {
        if (TweakConfigs.VPR1ReawakenCue && Statuses[3671].TryGetStatus() && serpentGauge->DataCurrent.GaugeValue < 50)
            UIModule.Instance()->GetRaptureAtkModule()->GetNumberArrayData(86)->SetValue(9, 0, true);
    }

    private unsafe void ApplyColorCodeTweak(AddonJobHudRDB0* vipersight)
    {
        if (vipersight != null && vipersight->GaugeStandard.ViperBlades != null && vipersight->GaugeStandard.ViperBlades->LeftBlade != null)
        {
            var leftGlowWrapper = new CustomNode(vipersight->GaugeStandard.ViperBlades->LeftBlade->UldManager.NodeList[6]);
            var rightGlowWrapper = new CustomNode(vipersight->GaugeStandard.ViperBlades->RightBlade->UldManager.NodeList[6]);

            if (TweakConfigs.VPR0ColorCode)
            {
                var colorOffset = vipersight->DataCurrent.ComboStep == 2
                                      ? new AddRGB(0, -150, -255)
                                      : new AddRGB(-255, 0, 0);

                var actionManager = ActionData.ActionManager;


                var flankAnts = ActionData.CheckForAnts(34610, 34611);
                var hindAnts = ActionData.CheckForAnts(34612, 34613);

                var appliedColor = Statuses[1250].TryGetStatus()
                                       ? TweakConfigs.VPR0ColorNeutral
                                       : actionManager->GetAdjustedActionId(34607)
                                           switch // check what state the Dread Fangs button is in
                                           {
                                               34611 => TweakConfigs.VPR0ColorFlank, // flank positional finishers are up
                                               34613 => TweakConfigs.VPR0ColorRear,  // rear positional finishers are up
                                               34609 when flankAnts => TweakConfigs
                                                   .VPR0ColorFlank, // swiftskin is up, a flank positional has ants
                                               34609 when hindAnts => TweakConfigs
                                                   .VPR0ColorRear, // swiftskin is up, a rear positional has ants
                                               _ => TweakConfigs.VPR0ColorNeutral
                                           };

                var adjustedColor = colorOffset + appliedColor;
                leftGlowWrapper.SetAddRGB(adjustedColor);
                rightGlowWrapper.SetAddRGB(adjustedColor);

                UpdateSimpleGlow(appliedColor);
            }
            else
            {
                leftGlowWrapper.SetAddRGB(0);
                rightGlowWrapper.SetAddRGB(0);

                RevertSimpleGlow();
            }
        }

        //todo: simplify/combine these two functions
        void UpdateSimpleGlow(AddRGB appliedColor)
        {
            void UpdateKeyFrame(AtkTimelineAnimation atkTimelineAnimation, int frameId) =>
                atkTimelineAnimation.KeyGroups[4].KeyFrames[frameId].Value.NodeTint.AddRGBBitfield =
                    appliedColor.ToBitField();

            var leftGlowSimple = (AtkNineGridNode*)vipersight->GaugeSimple.ViperBlades->LeftBar->UldManager.NodeList[5];
            var leftFrameSimple = (AtkNineGridNode*)vipersight->GaugeSimple.ViperBlades->LeftBar->UldManager.NodeList[4];

            var glowAnim0 = leftGlowSimple->Timeline->Resource->Animations[0];
            var glowAnim1 = leftGlowSimple->Timeline->Resource->Animations[1];
            var frameAnim1 = leftFrameSimple->Timeline->Resource->Animations[1];
            var frameAnim2 = leftFrameSimple->Timeline->Resource->Animations[2];

            UpdateKeyFrame(glowAnim0, 0);
            UpdateKeyFrame(glowAnim0, 1);
            UpdateKeyFrame(glowAnim0, 2);

            UpdateKeyFrame(glowAnim1, 0);
            UpdateKeyFrame(glowAnim1, 1);
            UpdateKeyFrame(glowAnim1, 2);

            UpdateKeyFrame(frameAnim1, 1);
            UpdateKeyFrame(frameAnim2, 1);
        }

        void RevertSimpleGlow()
        {
            static void UpdateKeyFrame(AtkTimelineAnimation atkTimelineAnimation, int frameId, AddRGB appliedColor) => atkTimelineAnimation.KeyGroups[4].KeyFrames[frameId].Value.NodeTint.AddRGBBitfield = appliedColor.ToBitField();

            var leftGlowSimple = (AtkNineGridNode*)vipersight->GaugeSimple.ViperBlades->LeftBar->UldManager.NodeList[5];
            var leftFrameSimple = (AtkNineGridNode*)vipersight->GaugeSimple.ViperBlades->LeftBar->UldManager.NodeList[4];

            var glowAnim0 = leftGlowSimple->Timeline->Resource->Animations[0];
            var glowAnim1 = leftGlowSimple->Timeline->Resource->Animations[1];
            var frameAnim1 = leftFrameSimple->Timeline->Resource->Animations[1];
            var frameAnim2 = leftFrameSimple->Timeline->Resource->Animations[2];

            UpdateKeyFrame(glowAnim0, 0, new(0, -200, -200));
            UpdateKeyFrame(glowAnim0, 1, new(0, -200, -200));
            UpdateKeyFrame(glowAnim0, 2, new(0, -200, -200));

            UpdateKeyFrame(glowAnim1, 0, new(-200, -200, 0));
            UpdateKeyFrame(glowAnim1, 1, new(-200, -200, 0));
            UpdateKeyFrame(glowAnim1, 2, new(-200, -200, 0));

            UpdateKeyFrame(frameAnim1, 1, new(255, 0, 0));
            UpdateKeyFrame(frameAnim2, 1, new(0, 50, 255));
        }
    }
}

public partial class TweakConfigs
{
    public bool VPRHide0;
    public bool VPRHide1;

    public bool VPR0ColorCode;
    public AddRGB VPR0ColorRear = new(251,-134,31);
    public AddRGB VPR0ColorFlank = new(-150,255,140);
    public AddRGB VPR0ColorNeutral = new(255, 180, 80);

    public bool VPR1ReawakenCue;
}
