using CustomNodes;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;
using GaugeOMatic.GameData;
using GaugeOMatic.Trackers;
using GaugeOMatic.Windows;
using ImGuiNET;
using System;
using System.Collections.Generic;
using static GaugeOMatic.GameData.JobData;
using static GaugeOMatic.GameData.JobData.Job;
using static GaugeOMatic.GameData.JobData.Role;
using static GaugeOMatic.GameData.StatusRef;
using static GaugeOMatic.JobModules.Tweaks;
using static GaugeOMatic.JobModules.Tweaks.TweakUI;
using static GaugeOMatic.Trackers.Tracker;
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
        trackerManager, trackerConfigList, "JobHudRDB0", "JobHudRDB1")
    { }

    public override void Save()
    {
        Configuration.TrackerConfigs.VPR = SaveOrder;
        Configuration.Save();
    }

    public override void TweakUI(ref UpdateFlags update)
    {
        Heading("Vipersight");
        ToggleControls("Hide Vipersight", ref TweakConfigs.VPRHide0, ref update);
        if (!TweakConfigs.VPRHide0)
        {
            ToggleControls("Mirror Highlights", ref TweakConfigs.VPR0Mirror, ref update);
            Info("Reverses the left/right position of the gauge highlights.\n" +
                 "Useful if you keep your Steel Fangs button on the right-hand side.");

            LabelColumn("Color-Code Highlights");

            if (ImGui.Checkbox("##BoolColor-Code Vipersight", ref TweakConfigs.VPR0ColorCode)) update |= UpdateFlags.Save;
            Info("Changes the gauge's highlight color to indicate your next positional.");

            if (TweakConfigs.VPR0ColorCode)
            {
                ImGui.SameLine();
                ImGui.Text("Test");
                ImGui.SameLine();
                if (ImGui.Checkbox("##TweakPreview", ref TweakConfigs.Preview)) update |= UpdateFlags.Save;

                RadioControls("Apply to:", ref TweakConfigs.VPR0ColorAll, new() { false, true }, new() { "3rd Step Only", "All Steps" }, ref update, true);

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

        ToggleControls("Ready to Reawaken Cue", ref TweakConfigs.VPR1ReawakenCue, ref update);
        Info("Cues the gauge to become highlighted after pressing\nSerpent's Ire and gaining Ready to Reawaken");
    }

    public override unsafe void ApplyTweaks0(IntPtr gaugeAddon)
    {
        var gauge = (AddonJobHudRDB0*)gaugeAddon;
        var gaugeIndex = (AddonIndex)gaugeAddon;
        VisibilityTweak(TweakConfigs.VPRHide0, gauge->UseSimpleGauge, gaugeIndex[2u], gaugeIndex[10u]);
        ApplyColorCodeTweak(gauge);

        gaugeIndex[2u].SetOrigin(167,0).SetScaleX(TweakConfigs.VPR0Mirror ? -1 : 1);
        gaugeIndex[9u].SetOrigin(52, 0).SetScaleX(TweakConfigs.VPR0Mirror ? -1 : 1);
        gaugeIndex[3u].SetOrigin(38, 0).SetScaleX(TweakConfigs.VPR0Mirror ? -1 : 1);

        gaugeIndex[16u].SetOrigin(97, 0).SetScaleX(TweakConfigs.VPR0Mirror ? -1 : 1);
        gaugeIndex[18u].SetX(TweakConfigs.VPR0Mirror?-194:0).SetScaleX(TweakConfigs.VPR0Mirror ? 1 : -1);

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
        if (TweakConfigs.VPR1ReawakenCue && ((StatusRef)3671).TryGetStatus() && serpentGauge->DataCurrent.GaugeValue < 50)
            UIModule.Instance()->GetRaptureAtkModule()->GetNumberArrayData(86)->SetValue(9, 0, true);
    }

    private static ActionRef ReavingFangs => 34609;
    private static ActionRef FlankAction1 = 34610;
    private static ActionRef FlankAction2 = 34611;
    private static ActionRef HindAction1 = 34612;
    private static ActionRef HindAction2 = 34613;
    private static bool FlankAnts => FlankAction1.HasAnts() || FlankAction2.HasAnts();
    private static bool HindAnts => HindAction1.HasAnts() || HindAction2.HasAnts();

    private unsafe void ApplyColorCodeTweak(AddonJobHudRDB0* vipersight)
    {
        var neutral = TweakConfigs.VPR0ColorNeutral;
        var flank = TweakConfigs.VPR0ColorFlank;
        var rear = TweakConfigs.VPR0ColorRear;
        var gaugeIndex = (AddonIndex)(AtkUnitBase*)vipersight;

        if (vipersight != null && vipersight->GaugeStandard.ViperBlades != null && vipersight->GaugeStandard.ViperBlades->LeftBlade != null)
        {
            if (TweakConfigs.VPR0ColorCode)
            {
                AddRGB appliedColor;
                if (TweakConfigs.ShowPreviews)
                {
                    JobUiData->SetValue(2, 1, true);
                    JobUiData->SetValue(3, 1, true);

                    appliedColor = TweakConfigs.TestColor == neutral ? neutral :
                                   TweakConfigs.TestColor == flank ? flank :
                                   rear;
                }
                else
                {
                    appliedColor = (!TweakConfigs.VPR0ColorAll && vipersight->DataCurrent.ComboStep < 2) || StatusData[1250].TryGetStatus()
                                       ? neutral
                                       : ReavingFangs.GetAdjustedId()
                                           switch // check what state the Dread Fangs button is in
                                           {
                                               34611 => flank, // flank positional finishers are up
                                               34613 => rear,  // rear positional finishers are up
                                               34609 when FlankAnts => flank, // swiftskin is up, a flank positional has ants
                                               34609 when HindAnts => rear, // swiftskin is up, a rear positional has ants
                                               _ => neutral
                                           };
                }

                RecolorStandard(appliedColor);
                RecolorSimple(appliedColor);
            }
            else
            {
                RevertStandard();
                RevertSimple();
            }
        }

        void RecolorStandard(AddRGB color)
        {
            var offset = new AddRGB(-255, 0, 0);
            gaugeIndex[7u, 5u].SetKeyFrameAddRGB(color + offset, (0, 0), (1, 0), (2, 0));
            gaugeIndex[7u, 4u].SetKeyFrameAddRGB(color + offset, 0)
                              .SetKeyFrameAddRGB(color + offset, 1)
                              .SetKeyFrameAddRGB(color + offset, 2);
            gaugeIndex[7u, 3u].SetKeyFrameAddRGB(color, 0)
                              .SetKeyFrameAddRGB(color, 1)
                              .SetKeyFrameAddRGB(color, 2);

        }

        void RevertStandard()
        {
            gaugeIndex[7u, 5u].SetKeyFrameAddRGB(new(200, 200, 50), 0, 0)
                              .SetKeyFrameAddRGB(new(0, 0, 0), 1, 0)
                              .SetKeyFrameAddRGB(new(-255, 150, 255), 2, 0);

            gaugeIndex[7u, 4u].SetKeyFrameAddRGB(new(200, 200, 50), 0)
                              .SetKeyFrameAddRGB(new(0, 0, 0), 1)
                              .SetKeyFrameAddRGB(new(-255, 150, 255), 2);

            gaugeIndex[7u, 3u].SetKeyFrameAddRGB(new(200, 200, 50), 0)
                              .SetKeyFrameAddRGB(new(200, -50, -150), 1)
                              .SetKeyFrameAddRGB(new(-255, 100, 200), 2);
        }

        void RecolorSimple(AddRGB color)
        {
            gaugeIndex[16u, 3u].SetKeyFrameAddRGB(color, 0)
                               .SetKeyFrameAddRGB(color, 1)
                               .SetKeyFrameAddRGB(color, 2);

            gaugeIndex[16u, 4u].SetKeyFrameAddRGB(color, 1, 1)
                               .SetKeyFrameAddRGB(color, 2, 1)
                               .SetKeyFrameAddRGB(color, 3, 1);
        }

        void RevertSimple()
        {
            gaugeIndex[16u, 3u].SetKeyFrameAddRGB(new(0, -200, -200), 0)
                               .SetKeyFrameAddRGB(new(-200, -200, 0), 1)
                               .SetKeyFrameAddRGB(new(-200, -200, 0), 2);

            gaugeIndex[16u, 4u].SetKeyFrameAddRGB(new(255, 0, 0), 1, 1)
                               .SetKeyFrameAddRGB(new(0, 50, 255), 2, 1)
                               .SetKeyFrameAddRGB(new(0, 50, 255), 3, 1);
        }
    }

}


public partial class TweakConfigs
{
    public bool VPRHide0;
    public bool VPR0ColorCode;
    public AddRGB VPR0ColorRear = new(251, -134, 31);
    public AddRGB VPR0ColorFlank = new(-150, 255, 140);
    public AddRGB VPR0ColorNeutral = new(255, 180, 80);
    public bool VPR0ColorAll = true;
    public bool VPR0Mirror;

    public bool VPRHide1;
    public bool VPR1ReawakenCue;
}
