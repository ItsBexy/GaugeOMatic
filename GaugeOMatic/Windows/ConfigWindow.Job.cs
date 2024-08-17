using Dalamud.Interface;
using GaugeOMatic.JobModules;
using GaugeOMatic.Trackers;
using GaugeOMatic.Utility;
using GaugeOMatic.Widgets;
using ImGuiNET;
using System.Collections.Generic;
using static CustomNodes.CustomNode;
using static Dalamud.Interface.Utility.ImGuiHelpers;
using static GaugeOMatic.GameData.JobData;
using static GaugeOMatic.Utility.ImGuiHelpy;
using static GaugeOMatic.Widgets.WidgetUI;
using static GaugeOMatic.Widgets.WidgetUI.UpdateFlags;
using static ImGuiNET.ImGuiTableColumnFlags;
using static ImGuiNET.ImGuiTableFlags;

namespace GaugeOMatic.Windows;

public partial class ConfigWindow
{
    public static JobModule? GetModuleForTab(Job jobTab, List<JobModule> jobModules) => jobModules.Find(g => g.Job == jobTab);

    public enum JobModuleTabs
    {
        Trackers = 0,
        Tweaks = 1
    }

    public static JobModuleTabs JobModuleTab { get; set; } = JobModuleTabs.Trackers;

    public static void DrawJobModuleTab(JobModule jobModule)
    {
        UpdateFlag = 0;
        if (ImGui.BeginTabBar(jobModule.Abbr+"Tabs"))
        {
            TrackerTab(jobModule);
            TweakTab(jobModule);
            ImGui.EndTabBar();
        }

        WidgetDragCheck();

        if (UpdateFlag.HasFlag(Rebuild)) jobModule.RebuildTrackerList();
        else if (UpdateFlag.HasFlag(Reset)) jobModule.ResetWidgets();
        else if (UpdateFlag.HasFlag(SoftReset)) jobModule.SoftReset();

        if (UpdateFlag.HasFlag(Save)) jobModule.Save();
    }

    private static void TweakTab(JobModule jobModule)
    {
        if (!ImGui.BeginTabItem($"Tweaks##{jobModule.Abbr}TweaksTab")) return;

        JobModuleTab = JobModuleTabs.Tweaks;

        if (ImGui.BeginTable($"{jobModule.Abbr}TweaksTable", 2, SizingFixedFit))
        {
            ImGui.TableSetupColumn("Labels",WidthFixed,200*GlobalScale);
            ImGui.TableSetupColumn("Options", WidthStretch);

            jobModule.TweakUI();

            ImGui.EndTable();
        }
    }

    private static void TrackerTab(JobModule jobModule)
    {
        if (!ImGui.BeginTabItem("Trackers")) return;

        JobModuleTab = JobModuleTabs.Trackers;

        if (ImGui.BeginTable($"{jobModule.Abbr}TrackerTable", 7, SizingFixedFit))
        {
            ImGui.TableSetupColumn("");
            ImGui.TableSetupColumn("Tracker");
            ImGui.TableSetupColumn("");
            ImGui.TableSetupColumn("Widget");
            ImGui.TableSetupColumn("");
            ImGui.TableSetupColumn("Pinned to");
            ImGui.TableSetupColumn("Test");

            TableHeadersRowNoHover(new(1));

            DrawTrackerRows(jobModule);

            ImGui.TableNextRow();
            ImGui.TableNextColumn();
            if (IconButtonWithText("Add", FontAwesomeIcon.Plus, "AddButton")) jobModule.AddBlankTracker();

            ImGui.TableNextColumn();
            if (IconButtonWithText("Presets", FontAwesomeIcon.ObjectGroup,"PresetButton")) GaugeOMatic.PresetWindow.IsOpen = !GaugeOMatic.PresetWindow.IsOpen;

            ImGui.EndTable();
        }

        ImGui.Spacing();
        ImGui.Spacing();
        ImGui.Spacing();
        WriteIcon(FontAwesomeIcon.ArrowsUpDownLeftRight,null,new(255,255,255,128));
        ImGui.TextDisabled("Press Shift to click and drag widgets onscreen");

        ImGui.EndTabItem();
    }

    private static void DrawTrackerRows(JobModule jobModule)
    {
        var hoveringOther = false;
        Bounds? hoverBounds = null;

        if (DragTarget != null)
        {
            hoveringOther = true;
            hoverBounds = DragTarget.GetBounds();
        }

        foreach (var tracker in jobModule.DrawOrder)
        {
            DrawTrackerRow(tracker, HoverCheck(ref hoveringOther, ref hoverBounds, tracker.Widget?.GetBounds(), tracker));
        }

        hoverBounds?.Draw(new Color.ColorRGB(0, 255, 100).ToABGR, 2);
    }

    private static bool HoverCheck(ref bool hoveringOther, ref Bounds? hoverBounds, Bounds? bounds, Tracker tracker)
    {
        var hoveringThis = false;
        if (ImGui.IsKeyDown(ImGuiKey.ModShift))
        {
            if (hoveringOther || bounds?.ContainsCursor() != true)
            {
                bounds?.Draw(0xaaffffff);
            }
            else
            {
                ImGui.SetNextFrameWantCaptureMouse(true);
                hoveringThis = true;
                hoverBounds = bounds;

                if (ImGui.IsMouseClicked(ImGuiMouseButton.Left))
                {
                    Dragging = true;
                    DragStart = tracker.Widget?.GetConfig.Position;
                    DragTarget = tracker.Widget;
                }

                hoveringOther = true;
            }
        }

        return hoveringThis;
    }

    public static bool Dragging;
    public static System.Numerics.Vector2? DragStart;
    public static Widget? DragTarget;

    private static void WidgetDragCheck()
    {
        if (!Dragging) return;

        ImGui.SetNextFrameWantCaptureMouse(true);
        var click = ImGui.IsMouseDown(ImGuiMouseButton.Left);
        var shift = ImGui.IsKeyDown(ImGuiKey.ModShift);

        var dragValid = DragTarget != null && DragStart != null;

        if (click)
        {
            if (dragValid)
            {
                var scale = DragTarget!.WidgetRoot.GetAbsoluteScale();
                var delta = ImGui.GetMouseDragDelta();
                DragTarget.GetConfig.Position = DragStart!.Value + (shift ? delta / scale : new(0));
                DragTarget.ApplyConfigs();
            }
        }
        else
        {
            if (shift)
            {
                DragTarget?.GetConfig.WriteToTracker(DragTarget.Tracker);
                UpdateFlag |= Save;
            }

            Dragging = false;
            DragTarget = null;
            DragStart = null;
        }
    }

    internal static string WidgetClipboard = "";
    internal static string? WidgetClipType = null;
}

