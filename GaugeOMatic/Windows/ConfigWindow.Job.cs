using Dalamud.Interface;
using GaugeOMatic.JobModules;
using ImGuiNET;
using System.Collections.Generic;
using static GaugeOMatic.GameData.JobData;
using static GaugeOMatic.Utility.ImGuiHelpy;
using static GaugeOMatic.Widgets.WidgetUI;
using static GaugeOMatic.Widgets.WidgetUI.UpdateFlags;
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
            ImGui.TableSetupColumn("Labels",ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn("Options", ImGuiTableColumnFlags.WidthStretch);

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

            foreach (var tracker in jobModule.DrawOrder) DrawTrackerRow(tracker);

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

