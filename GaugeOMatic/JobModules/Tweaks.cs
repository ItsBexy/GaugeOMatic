using Dalamud.Interface;
using GaugeOMatic.Utility;
using GaugeOMatic.Windows;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Numerics;
using static Dalamud.Interface.FontAwesomeIcon;
using static Dalamud.Interface.Utility.ImGuiHelpers;

namespace GaugeOMatic.JobModules;

public class TweakUI
{
    public static bool Bool1(string label, ref bool b, ref UpdateFlags update)
    {
        if (!ImGui.Checkbox(label, ref b)) return false;
        update |= UpdateFlags.Save;
        return true;
    }

    public static void HideWarning(bool highlight = false)
    {
        const string helpText = "Hiding job gauge elements means you won't see key information!\nMake sure to set up some widgets to replace your gauge with.";

        ImGui.SameLine();
        ImGui.PushFont(UiBuilder.IconFont);
        ImGui.TextColored(highlight ? new(0.9f, 0.71f, 0, 1) : new(1, 1, 1, 0.3f), ExclamationTriangle.ToIconString());
        ImGui.PopFont();
        if (!ImGui.IsItemHovered()) return;
        ImGui.BeginTooltip();
        ImGui.PushTextWrapPos(ImGui.GetFontSize() * 35f);
        ImGui.TextUnformatted(helpText);
        ImGui.PopTextWrapPos();
        ImGui.EndTooltip();
    }

    public static bool PositionControls(string label, ref Vector2 pos, ref UpdateFlags update) => XYControls(label, ref pos, -2560f, 2560f, new List<FontAwesomeIcon> { ChevronLeft, ChevronRight, ChevronUp, ChevronDown }, ref update);

    private static bool XYControls(string label, ref Vector2 xy, float min, float max, IReadOnlyList<FontAwesomeIcon>? icons, ref UpdateFlags update)
    {
        var x = xy.X;
        var y = xy.Y;

        ImGui.Text(label);
        ImGui.SameLine();
        var inputX = FloatInputDrag(label + "X", ref x, min, max, icons?[0] ?? Minus, icons?[1] ?? Plus);

        ImGui.SameLine();
        var inputY = FloatInputDrag(label + "Y", ref y, min, max, icons?[2] ?? Minus, icons?[3] ?? Plus);
        if (inputX || inputY)
        {
            xy = new(x, y);
            update |= UpdateFlags.Save;
            return true;
        }

        return false;
    }

    private static bool FloatInputDrag(string label, ref float f, float min, float max, FontAwesomeIcon icon1, FontAwesomeIcon icon2)
    {
        ImGui.SetNextItemWidth(90f * GlobalScale);

        var input1 = ImGui.DragFloat($"##{label}Drag", ref f, 1f, min, max);
        ImGui.PushButtonRepeat(true);

        ImGuiHelpers.SameLineSquished();
        var input2 = ImGuiHelpers.IconButton($"##{label}ButtonDown", icon1);
        if (input2) f = Math.Clamp(f - 1f, min, max);

        ImGuiHelpers.SameLineSquished();
        var input3 = ImGuiHelpers.IconButton($"##{label}ButtonUp", icon2);
        if (input3) f = Math.Clamp(f + 1f, min, max);

        ImGui.PopButtonRepeat();

        return input1 || input2 || input3;
    }
}
