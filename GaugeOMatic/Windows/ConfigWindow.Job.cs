using Dalamud.Interface.Utility.Raii;
using GaugeOMatic.JobModules;
using GaugeOMatic.Trackers;
using GaugeOMatic.Utility;
using Dalamud.Interface.Components;
using GaugeOMatic.Widgets;
using ImGuiNET;
using System.Collections.Generic;
using System.Numerics;
using static CustomNodes.CustomNode;
using static Dalamud.Interface.FontAwesomeIcon;
using static Dalamud.Interface.Utility.ImGuiHelpers;
using static GaugeOMatic.GameData.JobData;
using static GaugeOMatic.Utility.ImGuiHelpy;
using static GaugeOMatic.Widgets.Common.WidgetUI;
using static GaugeOMatic.Widgets.Common.WidgetUI.UpdateFlags;
using static ImGuiNET.ImGuiKey;
using static ImGuiNET.ImGuiMouseButton;
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
        using (var tb = ImRaii.TabBar(jobModule.Abbr + "Tabs"))
        {
            if (tb.Success)
            {
                TrackerTab(jobModule);
                TweakTab(jobModule);
            }
        }

        HandleDrag();

        if (UpdateFlag.HasFlag(Rebuild)) jobModule.RebuildTrackerList();
        else if (UpdateFlag.HasFlag(Reset)) jobModule.ResetWidgets();
        else if (UpdateFlag.HasFlag(SoftReset)) jobModule.SoftReset();

        if (UpdateFlag.HasFlag(UpdateFlags.Save)) jobModule.Save();
    }

    private static void TweakTab(JobModule jobModule)
    {
        using var ti = ImRaii.TabItem($"Tweaks##{jobModule.Abbr}TweaksTab");
        if (ti)
        {
            JobModuleTab = JobModuleTabs.Tweaks;

            using var table = ImRaii.Table($"{jobModule.Abbr}TweaksTable", 2, SizingFixedFit);
            if (table.Success) {
                ImGui.TableSetupColumn("Labels", WidthFixed, 200 * GlobalScale);
                ImGui.TableSetupColumn("Options", WidthStretch);

                jobModule.TweakUI();
            }
        }
    }

    private static void TrackerTab(JobModule jobModule)
    {
        using var ti = ImRaii.TabItem("Trackers");

        if (ti)
        {
            JobModuleTab = JobModuleTabs.Trackers;

            using (var table = ImRaii.Table($"{jobModule.Abbr}TrackerTable", 7, SizingFixedFit))
            {
                if (table.Success) {
                    ImGui.TableSetupColumn("");
                    ImGui.TableSetupColumn("Tracker");
                    ImGui.TableSetupColumn("");
                    ImGui.TableSetupColumn("Widget");
                    ImGui.TableSetupColumn("");
                    ImGui.TableSetupColumn("Pinned to");
                    ImGui.TableSetupColumn("Test");

                    TableHeadersRowNoHover(new(1));

                    var hoveringOther = DragTarget != null;
                    var hoverBounds = hoveringOther ? DragTarget?.GetBounds() : null;

                    foreach (var tracker in jobModule.DrawOrder)
                    {
                        var hovered = HoverCheck(ref hoveringOther, ref hoverBounds, tracker.Widget?.GetBounds(), tracker);
                        DrawTrackerRow(tracker, hovered);
                    }

                    hoverBounds?.Draw(new Color.ColorRGB(0, 255, 100).ToABGR, 2);

                    ImGui.TableNextRow();
                    ImGui.TableNextColumn();
                    if (ImGuiComponents.IconButtonWithText(Plus, "Add##addBlank")) jobModule.AddBlankTracker();

                    ImGui.TableNextColumn();
                    if (ImGuiComponents.IconButtonWithText(ObjectGroup, "Presets##openPresets")) GaugeOMatic.PresetWindow.IsOpen = !GaugeOMatic.PresetWindow.IsOpen;
                }
            }

            ImGui.Spacing();
            ImGui.Spacing();
            ImGui.Spacing();
            WriteIcon(ArrowsUpDownLeftRight, null, new(255, 255, 255, 128));
            ImGui.TextDisabled("Shift + Click & Drag to reposition widgets");

            WriteIcon(ExpandAlt, null, new(255, 255, 255, 128));
            ImGui.TextDisabled("Shift + Scroll to resize widgets");
        }
    }

    private static bool HoverCheck(ref bool hoveringOther, ref Bounds? hoverBounds, Bounds? bounds, Tracker tracker)
    {
        if (!ImGui.IsKeyDown(ModShift)) return false;

        var hoveringThis = false;

        if (!hoveringOther && bounds?.ContainsCursor() == true)
        {
            ImGui.SetNextFrameWantCaptureMouse(true);
            hoveringThis = true;
            hoverBounds = bounds;

            var wheel = ImGui.GetIO().MouseWheel;
            if (wheel != 0)
            {
                tracker.Widget?.ChangeScale(wheel);
                tracker.WriteWidgetConfig();
                UpdateFlag |= UpdateFlags.Save;
            }

            if (ImGui.IsMouseClicked(Left))
            {
                Dragging = true;
                DragStart = tracker.Widget?.Config.Position;
                DragTarget = tracker.Widget;
            }

            hoveringOther = true;
        }
        else bounds?.Draw(0xaaffffff);

        return hoveringThis;
    }

    internal static bool Dragging;
    internal static Vector2? DragStart;
    internal static Widget? DragTarget;

    private static void HandleDrag()
    {
        if (!Dragging) return;

        ImGui.SetNextFrameWantCaptureMouse(true);
        var click = ImGui.IsMouseDown(Left);
        var shift = ImGui.IsKeyDown(ModShift);

        var dragValid = DragTarget != null && DragStart != null;

        if (click)
        {
            if (dragValid)
            {
                var scale = DragTarget!.WidgetRoot.GetAbsoluteScale();
                var delta = ImGui.GetMouseDragDelta();
                DragTarget.Config.Position = DragStart!.Value + (shift ? delta / scale : new(0));
                DragTarget.Tracker.WriteWidgetConfig();
            }
        }
        else
        {
            if (shift)
            {
                DragTarget?.Tracker.WriteWidgetConfig();
                UpdateFlag |= UpdateFlags.Save;
            }

            Dragging = false;
            DragTarget = null;
            DragStart = null;
        }
    }

    internal static string WidgetClipboard = "";
    internal static string? WidgetClipType = null;
}

