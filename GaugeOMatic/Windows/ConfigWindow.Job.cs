using Dalamud.Interface;
using GaugeOMatic.JobModules;
using ImGuiNET;
using System;
using System.Numerics;
using static Dalamud.Interface.Utility.ImGuiHelpers;
using static GaugeOMatic.Utility.ImGuiHelpers;
using static GaugeOMatic.Windows.UpdateFlags;

namespace GaugeOMatic.Windows;

public partial class ConfigWindow
{
    public static void DrawJobModuleTab(JobModule jobModule)
    {
        UpdateFlags update = 0;

        DrawTrackerTable(jobModule, ref update);

        ImGui.Separator();
        ImGui.Spacing();

        if (ImGui.BeginTable($"{jobModule.Abbr}AddTrackers", 1,ImGuiTableFlags.SizingFixedFit))
        {
            ImGui.TableSetupColumn("GAUGE TWEAKS",ImGuiTableColumnFlags.WidthFixed,500f * GlobalScale);
            ImGui.TableSetupColumn("QUICK ADD");

            ImGui.TableNextRow();
            TableHeadersRowNoHover(new(1, 1, 1, 0.6f));

            ImGui.TableNextRow();
            ImGui.TableNextColumn();

            jobModule.TweakUI(ref update);

            ImGui.EndTable();
        }

        if (update.HasFlag(Rebuild)) jobModule.RebuildTrackerList();
        else if (update.HasFlag(Reset)) jobModule.ResetWidgets();
        else if (update.HasFlag(SoftReset)) jobModule.SoftReset();

        if (update.HasFlag(Save)) jobModule.Save();
    }

    private static void DrawTrackerTable(JobModule jobModule, ref UpdateFlags update)
    {
        if (ImGui.BeginTable($"{jobModule.Abbr}TrackerTable", 9, ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.ScrollY, new Vector2(1000f, 250f) * GlobalScale))
        {
            ImGui.TableSetupColumn("");
            ImGui.TableSetupColumn("Tracker");
            ImGui.TableSetupColumn("");
            ImGui.TableSetupColumn("Widget");
            ImGui.TableSetupColumn("");
            ImGui.TableSetupColumn("Pinned to");
            ImGui.TableSetupColumn("Preview");

            TableHeadersRowNoHover(new(1));

            foreach (var tracker in jobModule.DrawOrder) DrawTrackerRow(tracker, ref update);

            ImGui.TableNextRow();
            ImGui.TableNextColumn();
            if (IconButtonWithText("Add", FontAwesomeIcon.Plus, "AddButton")) jobModule.AddBlankTracker();

            ImGui.TableNextColumn();
            if (IconButtonWithText("Presets",FontAwesomeIcon.ObjectGroup,"PresetButton")) GaugeOMatic.PresetWindow.IsOpen = !GaugeOMatic.PresetWindow.IsOpen;


            ImGui.EndTable();
        }
    }

    internal static string WidgetClipboard = "";
    internal static string? WidgetClipType = null;
}

[Flags]
public enum UpdateFlags { Save = 0x1, Reset = 0x2, SoftReset = 0x4, Rebuild = 0x8 }
