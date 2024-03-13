using Dalamud.Interface;
using GaugeOMatic.JobModules;
using GaugeOMatic.Widgets;
using ImGuiNET;
using System;
using static GaugeOMatic.Utility.ImGuiHelpy;
using static GaugeOMatic.Windows.UpdateFlags;
using static ImGuiNET.ImGuiTableFlags;

namespace GaugeOMatic.Windows;

public partial class ConfigWindow
{
    public static void DrawJobModuleTab(JobModule jobModule)
    {
        UpdateFlags update = 0;
        if (ImGui.BeginTabBar(jobModule.Abbr+"Tabs"))
        {
            TrackerTab(jobModule, ref update);
            TweakTab(jobModule, ref update);

            ImGui.EndTabBar();
        }

        if (update.HasFlag(Rebuild)) jobModule.RebuildTrackerList();
        else if (update.HasFlag(Reset)) jobModule.ResetWidgets();
        else if (update.HasFlag(SoftReset)) jobModule.SoftReset();

        if (update.HasFlag(Save)) jobModule.Save();
    }

    private static void TweakTab(JobModule jobModule, ref UpdateFlags update)
    {
        if (!ImGui.BeginTabItem($"Tweaks##{jobModule.Abbr}TweaksTab")) return;

        if (ImGui.BeginTable($"{jobModule.Abbr}TweaksTable", 2, SizingFixedFit))
        {
            ImGui.TableSetupColumn("Labels", default, 150f);
            ImGui.TableSetupColumn("Options");

            WidgetUI.Heading("Job Gauge Tweaks");

            jobModule.TweakUI(ref update);

            ImGui.EndTable();
        }
    }

    private static void TrackerTab(JobModule jobModule, ref UpdateFlags update)
    {
        if (!ImGui.BeginTabItem("Trackers")) return;

        if (ImGui.BeginTable($"{jobModule.Abbr}TrackerTable", 9, SizingFixedFit))
        {
            ImGui.TableSetupColumn("");
            ImGui.TableSetupColumn("Tracker");
            ImGui.TableSetupColumn("");
            ImGui.TableSetupColumn("Widget");
            ImGui.TableSetupColumn("");
            ImGui.TableSetupColumn("Pinned to");
            ImGui.TableSetupColumn("Test");

            TableHeadersRowNoHover(new(1));

            foreach (var tracker in jobModule.DrawOrder) DrawTrackerRow(tracker, ref update);

            ImGui.TableNextRow();
            ImGui.TableNextColumn();
            if (IconButtonWithText("Add", FontAwesomeIcon.Plus, "AddButton")) jobModule.AddBlankTracker();

            ImGui.TableNextColumn();
            if (IconButtonWithText("Presets", FontAwesomeIcon.ObjectGroup,"PresetButton")) GaugeOMatic.PresetWindow.IsOpen = !GaugeOMatic.PresetWindow.IsOpen;

            ImGui.EndTable();
        }

        ImGui.EndTabItem();
    }

    internal static string WidgetClipboard = "";
    internal static string? WidgetClipType = null;
}

[Flags]
public enum UpdateFlags { Save = 0x1, Reset = 0x2, SoftReset = 0x4, Rebuild = 0x8 }
