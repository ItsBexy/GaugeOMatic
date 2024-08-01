using Dalamud.Interface;
using GaugeOMatic.JobModules;
using ImGuiNET;
using System.Collections.Generic;
using static GaugeOMatic.GameData.JobData;
using static GaugeOMatic.Trackers.Tracker;
using static GaugeOMatic.Trackers.Tracker.UpdateFlags;
using static GaugeOMatic.Utility.ImGuiHelpy;
using static ImGuiNET.ImGuiTableFlags;

namespace GaugeOMatic.Windows;

public partial class ConfigWindow
{
    public static JobModule? GetModuleForTab(Job jobTab, List<JobModule> jobModules) => jobModules.Find(g => g.Job == jobTab);

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
            ImGui.TableSetupColumn("Labels",ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn("Options", ImGuiTableColumnFlags.WidthStretch);

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

