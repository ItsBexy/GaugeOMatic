using Dalamud.Interface;
using Dalamud.Interface.Components;
using GaugeOMatic.Trackers;
using ImGuiNET;
using System;
using System.Numerics;
using static Dalamud.Interface.Utility.ImGuiHelpers;
using static GaugeOMatic.Trackers.Tracker;
using static GaugeOMatic.Trackers.Tracker.UpdateFlags;
using static GaugeOMatic.Utility.Color;
using static GaugeOMatic.Utility.ImGuiHelpy;

namespace GaugeOMatic.Windows;

public partial class ConfigWindow
{
    private static void DrawTrackerRow(Tracker tracker, ref UpdateFlags update)
    {
        var hash = tracker.GetHashCode();
        var index = tracker.TrackerConfig.Index;

        ImGui.TableNextRow();

        ImGui.TableNextColumn();

        DeleteButton(tracker, hash);
        SameLineSquished();
        EnabledCheckbox(tracker, hash, ref update);

        ImGui.TableNextColumn();

        DrawGameIcon(tracker.GameIcon, 22f, tracker.TrackerConfig.Enabled);

        if (ImGui.IsItemHovered()) tracker.DrawTooltip();

        tracker.ItemRefMenu.Draw("[ Track... ]", 180f, ref update);

        ImGui.TableNextColumn();
        LayerControls(tracker, hash, index, ref update);

        ImGui.TableNextColumn();
        tracker.WidgetMenuTable.Draw("[Select Widget]", 200f, ref update);

        ImGui.TableNextColumn();
        WidgetControls(tracker, hash, index, ref update);

        ImGui.TableNextColumn();
        AddonDropdown(tracker, hash, ref update);

        ImGui.TableNextColumn();
        PreviewControls(tracker, hash);
    }

    private static void LayerControls(Tracker tracker, int hash, int index, ref UpdateFlags update)
    {
        BumpUpButton(tracker, hash, index, ref update);
        SameLineSquished();
        BumpDownButton(tracker, hash, index, ref update);
        SameLineSquished();
    }

    private static void BumpUpButton(Tracker tracker, int hash, int index, ref UpdateFlags update)
    {
        if (index == 0) IconButtonDisabled($"BumpUp{hash}", FontAwesomeIcon.ChevronUp);
        else if (ImGuiComponents.IconButton($"BumpUp{hash}", FontAwesomeIcon.ChevronUp))
        {
            var swapWith = tracker.JobModule.TrackerList.Find(t => t.TrackerConfig.Index == index - 1);
            if (swapWith == null) return;

            tracker.TrackerConfig.Index--;
            swapWith.TrackerConfig.Index++;
            update |= SoftReset | Save;
        }
    }

    private static void BumpDownButton(Tracker tracker, int hash, int index, ref UpdateFlags update)
    {
        if (index == tracker.JobModule.TrackerList.Count - 1) IconButtonDisabled($"BumpDown{hash}", FontAwesomeIcon.ChevronDown);
        else if (ImGuiComponents.IconButton($"BumpDown{hash}", FontAwesomeIcon.ChevronDown))
        {
            var swapWith = tracker.JobModule.TrackerList.Find(t => t.TrackerConfig.Index == index + 1);
            if (swapWith == null) return;

            tracker.TrackerConfig.Index++;
            swapWith.TrackerConfig.Index--;
            update |= SoftReset | Save;
        }
    }

    private static void WidgetControls(Tracker tracker, int hash, int index, ref UpdateFlags update)
    {
        SettingsButton();
        SameLineSquished();
        CopyWidgetButton(tracker, hash);
        SameLineSquished();
        PasteWidgetButton(tracker, hash, ref update);
        void SettingsButton()
        {
            if (!tracker.Available) IconButtonDisabled($"Settings{hash}", FontAwesomeIcon.Cog);
            else if (ImGuiComponents.IconButton($"Settings{hash}", FontAwesomeIcon.Cog) && tracker.Window != null)
            {
                tracker.Window.PositionCondition = ImGuiCond.FirstUseEver;
                tracker.Window.IsOpen = !tracker.Window.IsOpen;
                tracker.Window.Position = FindWindowPosition(tracker, index);
            }

            if (ImGui.IsItemHovered()) ImGui.SetTooltip("Widget Settings");
        }
    }

    private static Vector2 FindWindowPosition(Tracker tracker, int index)
    {
        var lastIndex = tracker.JobModule.TrackerConfigList.Length - 1;

        var workSize = ImGui.GetMainViewport().WorkSize;
        var minX = ConfigWindowPos.X;
        var maxX = minX + (ConfigWindowSize.X - 300f);
        var spanX = maxX - minX;

        var x = lastIndex == 0 ? minX : ((float)index / lastIndex * spanX) + minX;
        var sizeY = tracker.Window?.Size?.Y ?? workSize.Y/2f;
        var y = Math.Clamp(ConfigWindowPos.Y + ConfigWindowSize.Y, 0, workSize.Y - sizeY);
        return new(x, y);
    }

    private static void CopyWidgetButton(Tracker tracker, int hash)
    {
        if (ImGuiComponents.IconButton($"CopyWidget{hash}", FontAwesomeIcon.Copy))
        {
            tracker.TrackerConfig.CleanUp();
            WidgetClipType = tracker.WidgetType;
            WidgetClipboard = tracker.WidgetConfig;
            ImGui.SetClipboardText(WidgetClipboard);
        }

        if (ImGui.IsItemHovered()) ImGui.SetTooltip("Copy Widget Settings");

    }

    private static void PasteWidgetButton(Tracker tracker, int hash, ref UpdateFlags update)
    {
        if (!string.IsNullOrEmpty(WidgetClipType) && tracker.WidgetMenuTable.AvailableWidgets.ContainsKey(WidgetClipType))
        {
            if (ImGuiComponents.IconButton($"PasteWidget{hash}", FontAwesomeIcon.PaintRoller))
            {
                tracker.WidgetConfig = WidgetClipboard!;
                update |= Reset | Save;
            }

            if (ImGui.IsItemHovered()) ImGui.SetTooltip("Paste Copied Settings");
        }
        else IconButtonDisabled(FontAwesomeIcon.PaintRoller);
    }

    private static void AddonDropdown(Tracker tracker, int hash, ref UpdateFlags update)
    {
        if (!tracker.AddonDropdown.Draw($"AddonSelect{hash}", 120f)) return;

        tracker.AddonName = tracker.AddonDropdown.CurrentSelection;
        update |= Reset | Save;
    }

    private static void PreviewControls(Tracker tracker, int hash)
    {
        var preview = tracker.TrackerConfig.Preview;

        if (ImGui.Checkbox($"##Preview{hash}", ref preview))
        {
            tracker.TrackerConfig.Preview = tracker.Available && preview;
            tracker.Widget?.ApplyDisplayRules();
        }

        ImGui.SameLine();
        ImGui.SetNextItemWidth(120f * GlobalScale);
        if (tracker.Available && preview)
        {
            var previewValue = tracker.TrackerConfig.PreviewValue;
            if (ImGui.SliderFloat($"##PreviewSlider{hash}", ref previewValue, 0, 1f, "")) tracker.TrackerConfig.PreviewValue = previewValue;
        }
        else
        {
            ImGui.PushStyleColor(ImGuiCol.Button, 0);
            ImGui.PushStyleColor(ImGuiCol.ButtonActive, 0);
            ImGui.PushStyleColor(ImGuiCol.ButtonHovered, 0);
            ImGui.Button("", new(120f, 0));
            ImGui.PopStyleColor(3);
        }
    }

    private static void DeleteButton(Tracker tracker, int hash)
    {
        if (!ImGui.IsKeyDown(ImGuiKey.ModShift)) IconButtonDisabled($"Delete{hash}", FontAwesomeIcon.TrashAlt);
        else if (ImGuiComponents.IconButton($"Delete{hash}", FontAwesomeIcon.TrashAlt, (ColorRGB)0xb9222aff, (ColorRGB)0xf87942ff, (ColorRGB)0xd75440ff)) tracker.JobModule.RemoveTracker(tracker);
        if (ImGui.IsItemHovered()) ImGui.SetTooltip("Shift-Click to Delete");
    }

    private static void EnabledCheckbox(Tracker tracker, int hash, ref UpdateFlags update)
    {
        var enabled = tracker.TrackerConfig.Enabled;
        if (ImGui.Checkbox($"##Enabled{hash}", ref enabled))
        {
            tracker.TrackerConfig.Enabled = enabled;
            update |= Reset | Save;
        }
        if (ImGui.IsItemHovered()) ImGui.SetTooltip("Enable/Disable");
    }
}
