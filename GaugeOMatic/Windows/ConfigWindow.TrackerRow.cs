using GaugeOMatic.Trackers;
using ImGuiNET;
using System;
using System.Numerics;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Components;
using static Dalamud.Interface.FontAwesomeIcon;
using static Dalamud.Interface.Utility.ImGuiHelpers;
using static GaugeOMatic.Utility.Color;
using static GaugeOMatic.Utility.ImGuiHelpy;
using static GaugeOMatic.Widgets.Common.WidgetUI;
using static GaugeOMatic.Widgets.Common.WidgetUI.UpdateFlags;
using static ImGuiNET.ImGuiKey;

namespace GaugeOMatic.Windows;

public partial class ConfigWindow
{
    private static void DrawTrackerRow(Tracker tracker, bool hovering = false)
    {
        var hash = tracker.GetHashCode();
        var trackerConfig = tracker.TrackerConfig;
        var index = trackerConfig.Index;

        using (ImRaii.PushColor(ImGuiCol.Text, hovering ? new ColorRGB(0, 255, 100) : new Vector4(1, 1, 1, 1)))
        {
            ImGui.TableNextRow();

            ImGui.TableNextColumn();

            DeleteButton(tracker, hash);
            ImGui.SameLine(0,3);
            EnabledCheckbox(tracker, hash);

            ImGui.TableNextColumn();

            var attr = tracker.DisplayAttr;
            DrawGameIcon(attr.GameIcon, 22f, trackerConfig.Enabled);

            if (ImGui.IsItemHovered()) trackerConfig.DrawTooltip();

            tracker.TrackerDropdown.Draw("[ Track... ]", 180f);

            ImGui.TableNextColumn();
            LayerControls(tracker, hash, index);

            ImGui.TableNextColumn();
            tracker.WidgetMenuTable.Draw("[Select Widget]", 200f);

            ImGui.TableNextColumn();
            WidgetControls(tracker, hash, index);

            ImGui.TableNextColumn();
            AddonDropdown(tracker, hash);

            ImGui.TableNextColumn();
            PreviewControls(tracker, hash);
        }

        if (UpdateFlag.HasFlag(UpdateFlags.Save)) trackerConfig.DisplayAttr = null;


    }

    private static void LayerControls(Tracker tracker, int hash, int index)
    {
        BumpUpButton(tracker, hash, index);
        ImGui.SameLine(0,3);
        BumpDownButton(tracker, hash, index);
        ImGui.SameLine(0,3);
    }

    private static void BumpUpButton(Tracker tracker, int hash, int index)
    {
        if (index == 0) IconButtonDisabled($"BumpUp{hash}", ChevronUp);
        else if (IconButton($"BumpUp{hash}", ChevronUp))
        {
            var swapWith = tracker.JobModule.TrackerList.Find(t => t.TrackerConfig.Index == index - 1);
            if (swapWith == null) return;

            tracker.TrackerConfig.Index--;
            swapWith.TrackerConfig.Index++;
            UpdateFlag |= SoftReset | UpdateFlags.Save;
        }
    }

    private static void BumpDownButton(Tracker tracker, int hash, int index)
    {
        if (index == tracker.JobModule.TrackerList.Count - 1) IconButtonDisabled($"BumpDown{hash}", ChevronDown);
        else if (IconButton($"BumpDown{hash}", ChevronDown))
        {
            var swapWith = tracker.JobModule.TrackerList.Find(t => t.TrackerConfig.Index == index + 1);
            if (swapWith == null) return;

            tracker.TrackerConfig.Index++;
            swapWith.TrackerConfig.Index--;
            UpdateFlag |= SoftReset | UpdateFlags.Save;
        }
    }

    private static void WidgetControls(Tracker tracker, int hash, int index)
    {
        SettingsButton();
        ImGui.SameLine(0,3);
        CopyWidgetButton(tracker, hash);
        ImGui.SameLine(0,3);
        PasteWidgetButton(tracker, hash);
        return;

        void SettingsButton()
        {
            if (!tracker.Available) IconButtonDisabled($"Settings{hash}", Cog);
            else if (IconButton($"Settings{hash}", Cog) && tracker.Window != null)
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
        var sizeY = tracker.Window?.Size?.Y ?? workSize.Y / 2f;
        var y = Math.Clamp(ConfigWindowPos.Y + ConfigWindowSize.Y, 0, workSize.Y - sizeY);
        return new(x, y);
    }

    private static void CopyWidgetButton(Tracker tracker, int hash)
    {
        if (IconButton($"CopyWidget{hash}", Copy))
        {
            tracker.TrackerConfig.CleanUp();
            WidgetClipType = tracker.WidgetType;
            WidgetClipboard = tracker.WidgetConfig;
            ImGui.SetClipboardText(WidgetClipboard);
        }

        if (ImGui.IsItemHovered()) ImGui.SetTooltip("Copy Widget Settings");

    }

    private static void PasteWidgetButton(Tracker tracker, int hash)
    {
        if (!string.IsNullOrEmpty(WidgetClipType) && tracker.WidgetMenuTable.AvailableWidgets.ContainsKey(WidgetClipType))
        {
            if (IconButton($"PasteWidget{hash}", PaintRoller))
            {
                tracker.WidgetConfig = WidgetClipboard!;
                UpdateFlag |= Reset | UpdateFlags.Save;
            }

            if (ImGui.IsItemHovered()) ImGui.SetTooltip("Paste Copied Settings");
        }
        else IconButtonDisabled(PaintRoller);
    }

    private static void AddonDropdown(Tracker tracker, int hash)
    {
        if (!tracker.AddonDropdown.Draw($"AddonSelect{hash}", 120f)) return;

        tracker.AddonName = tracker.AddonDropdown.CurrentSelection;
        UpdateFlag |= Reset | UpdateFlags.Save;
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
            using (ImRaii.PushColor(ImGuiCol.Button, 0)
                         .Push(ImGuiCol.ButtonActive, 0)
                         .Push(ImGuiCol.ButtonHovered, 0))
            {
                ImGui.Button("", new(120f, 0));
            }
        }
    }

    private static void DeleteButton(Tracker tracker, int hash)
    {
        var shift = ImGui.IsKeyDown(ModShift);
        if (shift)
        {
            using (ImRaii.PushColor(ImGuiCol.Text, 0xffffffff))
            {
                if (ImGuiComponents.IconButton($"Delete{hash}", TrashAlt, (ColorRGB)0xb9222aff, (ColorRGB)0xf87942ff, (ColorRGB)0xd75440ff)) tracker.JobModule.RemoveTracker(tracker);
            }
        }
        else
        {
            IconButtonDisabled($"Delete{hash}", TrashAlt);
        }

        if (ImGui.IsItemHovered())
        {
            ImGui.SetTooltip("Shift-Click to Delete");
            if (shift) tracker.Widget?.DrawBounds(new ColorRGB(255, 0, 0).ToABGR, 2);
        }
    }

    private static void EnabledCheckbox(Tracker tracker, int hash)
    {
        var enabled = tracker.TrackerConfig.Enabled;
        if (ImGui.Checkbox($"##Enabled{hash}", ref enabled))
        {
            tracker.TrackerConfig.Enabled = enabled;
            UpdateFlag |= Reset | UpdateFlags.Save;
        }
        if (ImGui.IsItemHovered()) ImGui.SetTooltip("Enable/Disable");
    }
}
