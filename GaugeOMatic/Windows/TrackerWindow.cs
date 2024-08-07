using Dalamud.Interface;
using Dalamud.Interface.Windowing;
using GaugeOMatic.Config;
using GaugeOMatic.Trackers;
using GaugeOMatic.Utility;
using GaugeOMatic.Widgets;
using ImGuiNET;
using System;
using static Dalamud.Interface.Utility.ImGuiHelpers;
using static GaugeOMatic.GameData.JobData;
using static GaugeOMatic.Trackers.Tracker;
using static GaugeOMatic.Trackers.Tracker.UpdateFlags;
using static GaugeOMatic.Widgets.WidgetUI;

namespace GaugeOMatic.Windows;

public class TrackerWindow : Window, IDisposable
{
    public Configuration Configuration;
    public Tracker Tracker;
    public Widget? Widget;
    public string Hash => Tracker.GetHashCode()+"-"+Widget?.GetHashCode();

    public TrackerWindow(Tracker tracker, Widget widget, Configuration configuration, string name) : base(name)
    {
        Tracker = tracker;
        Widget = widget;
        Configuration = configuration;
        Collapsed = false;
        Flags = ImGuiWindowFlags.NoCollapse;
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new(300f, 320f),
            MaximumSize = new(1100f)
        };
    }

    public override void Draw()
    {
        UpdateFlags update = 0;
        var widgetConfig = Tracker.WidgetConfig;
        if (!Tracker.Available || Widget == null) IsOpen = false;

        HeaderTable(ref update);

        WidgetOptionTable(widgetConfig, ref update);

        if (update.HasFlag(Save)) Configuration.Save();
        if (update.HasFlag(Reset)) Tracker.JobModule.ResetWidgets();
    }

    private void HeaderTable(ref UpdateFlags update)
    {
        if (!ImGui.BeginTable("TrackerHeaderTable" + Hash, 2, ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.PadOuterX)) return;

        ImGui.TableSetupColumn("Labels", ImGuiTableColumnFlags.WidthFixed, 60f * GlobalScale);

        ImGui.TableNextRow();
        ImGui.TableNextColumn();
        ImGuiHelpy.TextRightAligned("Widget");
        ImGui.TableNextColumn();
        Tracker.WidgetMenuWindow.Draw("[Select Widget]", 182f, ref update);

        ImGui.TableNextRow();
        ImGui.TableNextColumn();
        ImGuiHelpy.TextRightAligned("Pinned to:");
        ImGui.TableNextColumn();
        if (Tracker.AddonDropdown.Draw($"AddonSelect{GetHashCode()}", 182f))
        {
            Tracker.AddonName = Tracker.AddonDropdown.CurrentSelection;
            update |= Reset | Save;
        }


        PreviewControls();

        ImGui.EndTable();

    }

    public void PreviewControls()
    {
        ImGui.TableNextRow();
        ImGui.TableNextColumn();
        ImGuiHelpy.TextRightAligned("Test");
        var preview = Tracker.TrackerConfig.Preview;
        var previewValue = Tracker.TrackerConfig.PreviewValue;
        ImGui.TableNextColumn();
        if (ImGui.Checkbox($"##Preview{Hash}", ref preview))
        {
            Tracker.TrackerConfig.Preview = preview;
            Tracker.Widget?.ApplyDisplayRules();
        }
        ImGui.SameLine();

        if (preview)
        {
            ImGui.SetNextItemWidth(153f * GlobalScale);
            if (ImGui.SliderFloat($"##PreviewSlider{Tracker.GetHashCode()}", ref previewValue, 0, 1f,""))
                Tracker.TrackerConfig.PreviewValue = previewValue;
        }
    }

    private void WidgetOptionTable(WidgetConfig widgetConfig, ref UpdateFlags update)
    {
        if (ImGui.BeginTable("TrackerWidgetOptionTable" + Hash, 2, ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.PadOuterX))
        {
            ImGui.TableSetupColumn("Labels");


            ImGuiHelpy.TableSeparator(2);

            ImGui.TableNextRow();
            ImGui.TableNextColumn();
            ImGui.TextColored(new(1, 1, 1, 0.3f), "Widget Settings");
            ImGui.TableNextColumn();

            ImGui.SetCursorPosX(ImGui.GetCursorPosX() + ImGui.GetColumnWidth() - (80 * GlobalScale));
            if (ImGuiHelpy.IconButtonWithText("Default", FontAwesomeIcon.UndoAlt, $"##{Hash}Default", 80f))
            {
                Widget?.ResetConfigs();
                Widget?.ApplyConfigs();
                Tracker.UpdateTracker();
                update |= Save;
            }

            if (ImGui.IsItemHovered())
                ImGui.SetTooltip(
                    $"This will reset to the defaults for {Widget?.WidgetInfo.DisplayName}.\nTo restore a particular preset for this tracker instead, use the Presets window.");

            ImGui.Spacing();
            ImGui.Spacing();

            Widget?.DrawUI(ref widgetConfig, ref update);

            ImGuiHelpy.TableSeparator(2);

            DisplayRuleTable(ref update);

            ImGui.EndTable();
        }
    }

    private void DisplayRuleTable(ref UpdateFlags update)
    {
        ImGui.TableNextRow();
        ImGui.TableNextColumn();
        ImGui.TextColored(new(1, 1, 1, 0.3f), "Display Rules");
        ImGui.TableNextColumn();

        var cond4 = RadioControls("Visibility", ref Tracker.TrackerConfig.HideOutsideCombatDuty, new() { false, true }, new() { "Anytime", "Combat / Duty Only" }, ref update);

        var cond1 = ToggleControls("Set Level Range", ref Tracker.TrackerConfig.LimitLevelRange, ref update);

        var cond2 = false;
        var cond3 = false;
        if (Tracker.TrackerConfig.LimitLevelRange)
        {
            var min = Tracker.TrackerConfig.LevelMin ?? 1;
            var max = Tracker.TrackerConfig.LevelMax ?? LevelCap;
            cond2 = IntControls("Minimum Level", ref min, 1, max, 1, ref update);
            cond3 = IntControls("Maximum Level", ref max, min, LevelCap, 1, ref update);

            if (cond2 || cond3)
            {
                Tracker.TrackerConfig.LevelMin = min;
                Tracker.TrackerConfig.LevelMax = max;
            }
        }
        else
        {
            if (Tracker.TrackerConfig.LevelMin == 1 && Tracker.TrackerConfig.LevelMax == LevelCap)
            {
                Tracker.TrackerConfig.LevelMin = null;
                Tracker.TrackerConfig.LevelMax = null;
            }
        }


        if (cond1 || cond2 || cond3 || cond4) Tracker.Widget?.ApplyDisplayRules();
    }

    public void Dispose() { }
}
