using Dalamud.Interface;
using FFXIVClientStructs.FFXIV.Component.GUI;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using static Dalamud.Interface.Utility.ImGuiHelpers;
using static GaugeOMatic.Trackers.Tracker;
using static GaugeOMatic.Trackers.Tracker.UpdateFlags;
using static GaugeOMatic.Utility.Color;
using static GaugeOMatic.Utility.ImGuiHelpy;
using static ImGuiNET.ImGuiCol;
using static ImGuiNET.ImGuiColorEditFlags;

namespace GaugeOMatic.Widgets;

[SuppressMessage("ReSharper", "UnusedMethodReturnValue.Global")]
internal static class WidgetUI
{
    public static void LabelColumn(string label)
    {
        ImGui.TableNextRow();
        ImGui.TableNextColumn();
        TextRightAligned(label.IndexOf('#') > 0 ? label[..label.IndexOf('#')] : label);
        ImGui.TableNextColumn();
    }

    public static bool ToggleControls(string label, ref bool b, ref UpdateFlags update)
    {
        LabelColumn(label);

        if (ImGui.Checkbox($"##Bool{label}", ref b)) {
            update |= Save;
            return true;
        }
        return false;
    }

    public static bool ToggleControls(string label, ref List<bool> bools, List<string> boolNames, ref UpdateFlags update)
    {
        LabelColumn(label);

        var ret = false;
        for (var i = 0; i < bools.Count; i++)
        {
            var b = bools[i];
            if (ImGui.Checkbox($"{boolNames[i]}##Bool{i}{label}", ref b))
            {
                update |= Save;
                bools[i] = b;
                ret = true;
            }
        }
        return ret;
    }

    public static bool IntControls(string label, ref int i, int min, int max, int step, ref UpdateFlags update)
    {
        LabelColumn(label);

        if (IntInputDrag(label, ref i, min, max, step))
        {
            update |= Save;
            return true;
        }
        return false;
    }

    public static bool IntControls(string label, ref byte b, int min, int max, int step, ref UpdateFlags update)
    {
        var i = (int)b;
        if (IntControls(label, ref i, min, max, step, ref update))
        {
            b = (byte)i;
            return true;
        }
        return false;
    }

    public static bool FloatControls(string label, ref float f, float min, float max, float step, ref UpdateFlags update, string format = "%.2f")
    {
        LabelColumn(label);

        if (FloatInputDrag(label, ref f, min, max, step, format: format))
        {
            update |= Save;
            return true;
        }
        return false;
    }

    public static void AngleControls(string label, ref float angle, ref UpdateFlags update)
    {
        LabelColumn(label);
        if (FloatInputDrag(label, ref angle, -1000, 1000, 1f, format: "%.2f"))
        {
            while (angle < -180) angle += 360;
            while (angle > 180) angle -= 360;
            update |= Save;
        }
    }

    public static bool PercentControls(string label, ref float p, ref UpdateFlags update)
    {
        var ret = false;
        var percent = p * 100f;
        if (FloatControls(label, ref percent, 0, 100, 1f, ref update, "%.0f%%"))
        {
            p = percent / 100f;
            ret = true;
        }

        return ret;
    }

    public static void ComboControls(string label, ref FontType val, List<FontType> options, List<string> optionNames, ref UpdateFlags update)
    {
        LabelColumn(label);

        ImGui.SetNextItemWidth(142 * GlobalScale);
        var i = options.IndexOf(val);
        if (ImGui.Combo($"##{label}", ref i, optionNames.ToArray(), optionNames.Count))
        {
            update |= Save;
            val = options[i];
        }
    }

    public static bool StringControls(string label, ref string str, string hintText, ref UpdateFlags update)
    {
        LabelColumn(label);

        ImGui.SetNextItemWidth(142f * GlobalScale);
        if (ImGui.InputTextWithHint($"##{label}", hintText, ref str, 40u))
        {
            update |= Save;
            return true;
        }
        return false;
    }

    public const ImGuiColorEditFlags PickerFlags = DisplayHex | AlphaBar;

    public static bool ColorPickerRGBA(string label, ref Vector4 v4, ref UpdateFlags update)
    {
        LabelColumn(label);

        ImGui.SetNextItemWidth(142f * GlobalScale);

        if (ImGui.ColorEdit4($"##{label}RGBA", ref v4, PickerFlags))
        {
            update |= Save;
            return true;
        }
        return false;
    }

    public static bool ColorPickerRGB(string label, ref Vector3 v3, ref UpdateFlags update)
    {
        LabelColumn(label);

        ImGui.SetNextItemWidth(142f * GlobalScale);

        if (ImGui.ColorEdit3($"##{label}RGB", ref v3, PickerFlags))
        {
            update |= Save;
            return true;
        }
        return false;
    }

    public static bool ColorPickerRGB<T>(string label, ref T color, ref UpdateFlags update) where T : IColor
    {
        var v3 = color.AsVec3();
        if (ColorPickerRGB(label, ref v3, ref update))
        {
            color = (T)color.FromVec3(v3);
            return true;
        }
        return false;
    }

    public static bool ColorPickerRGBA<T>(string label, ref T color, ref UpdateFlags update) where T : IColor
    {
        var v4 = color.AsVec4();
        if (ColorPickerRGBA(label, ref v4, ref update))
        {
            color = (T)color.FromVec4(v4);
            return true;
        }
        return false;
    }

    // ReSharper disable once UnusedMember.Global
    public static bool TriColorPicker(string label, ref ColorSet colorSet, ref UpdateFlags update)
    {
        Vector4 baseColor = colorSet.Base;
        Vector3 add = colorSet.Add;
        Vector3 multiply = colorSet.Multiply;

        LabelColumn(label);

        ImGui.SetNextItemWidth(92f * GlobalScale);

        if (ImGui.ColorEdit4($"##{label}RGB", ref baseColor, PickerFlags)) update |= Save;
        SameLineSquished();
        if (ImGui.ColorEdit3($"##{label}AddRGB", ref add, PickerFlags | NoInputs)) update |= Save;
        SameLineSquished();
        if (ImGui.ColorEdit3($"##{label}MultiplyRGB", ref multiply, PickerFlags | NoInputs)) update |= Save;


        if (update.HasFlag(Save))
        {
            colorSet = new(baseColor, add, multiply);
            return true;
        }
        return false;
    }

    public static bool RadioControls<T>(
        string label, ref T val, List<T> options, List<string> names, ref UpdateFlags update, bool sameLine = false)
    {
        LabelColumn(label);

        var ret = false;
        for (var i = 0; i < options.Count; i++)
        {
            var option = options[i];
            var name = names[i];
            if (i > 0 && sameLine) ImGui.SameLine();
            if (ImGui.RadioButton($"{name}##{label}{option}", val is not null && val.Equals(option)))
            {
                val = option;
                update |= Save;
                ret = true;
            }
        }
        return ret;
    }

    public static bool RadioIcons<T>(
        string label, ref T val, List<T> options, List<FontAwesomeIcon> icons, ref UpdateFlags update)
    {
        LabelColumn(label);

        var buttonColor = GetStyleColorVec4(Button);
        var activeColor = GetStyleColorVec4(ButtonActive);

        var ret = false;
        for (var i = 0; i < options.Count; i++)
        {
            var option = options[i];
            var icon = icons[i];

            if (i > 0) SameLineSquished();

            if (IconButton($"{label}{option}{i}", icon, 16f, val is not null && val.Equals(option) ? activeColor : buttonColor)) {
                val = option;
                update |= Save;
                ret = true;
            }
        }

        return ret;
    }

    public static bool PositionControls(string label, ref Vector2 pos, ref UpdateFlags update) => ControlXY(label, ref pos, -2560, 2560, 1, ref update, new() { FontAwesomeIcon.ChevronLeft, FontAwesomeIcon.ChevronRight, FontAwesomeIcon.ChevronUp, FontAwesomeIcon.ChevronDown });
    public static bool ScaleControls(string label, ref Vector2 scale, ref UpdateFlags update) => ControlXY(label, ref scale, 0, 10, 0.05f, ref update);
    public static bool ScaleControls(string label, ref float scale, ref UpdateFlags update) => FloatControls(label, ref scale, 0, 10, 0.05f, ref update);

    private static bool IntInputDrag(string label, ref int val, int min, int max, int step = 1, FontAwesomeIcon icon1 = FontAwesomeIcon.Minus, FontAwesomeIcon icon2 = FontAwesomeIcon.Plus)
    {
        ImGui.SetNextItemWidth(90f * GlobalScale);
        var input1 = ImGui.DragInt($"##{label}Drag", ref val, step, min, max);
        if (input1) val = Math.Clamp(val, min, max);


        ImGui.PushButtonRepeat(true);

        SameLineSquished();
        var input2 = IconButton($"##{label}ButtonDown", icon1);
        if (input2) val = Math.Clamp(val - step, min, max);

        SameLineSquished();
        var input3 = IconButton($"##{label}ButtonUp", icon2);
        if (input3) val = Math.Clamp(val + step, min, max);

        ImGui.PopButtonRepeat();

        return input1 || input2 || input3;
    }

    public static bool FloatInputDrag(
        string label, ref float val, float min, float max, float step = 0.05f, FontAwesomeIcon icon1 = FontAwesomeIcon.Minus,
        FontAwesomeIcon icon2 = FontAwesomeIcon.Plus, string format = "%.2f")
    {
        ImGui.SetNextItemWidth(90f * GlobalScale);

        var input1 = ImGui.DragFloat($"##{label}Drag", ref val, step, min, max, format);

        ImGui.PushButtonRepeat(true);
        SameLineSquished();
        var input2 = IconButton($"##{label}ButtonDown", icon1);
        if (input2) val = Math.Clamp(val - step, min, max);
        SameLineSquished();
        var input3 = IconButton($"##{label}ButtonUp", icon2);
        if (input3) val = Math.Clamp(val + step, min, max);
        ImGui.PopButtonRepeat();

        return input1 || input2 || input3;
    }

    public static bool ControlXY(string label, ref Vector2 xy, float min, float max, float step, ref UpdateFlags update, List<FontAwesomeIcon>? icons = null)
    {
        LabelColumn(label);

        var x = xy.X;
        var y = xy.Y;

        var inputX = FloatInputDrag($"{label}X", ref x, min, max, step, icons?[0] ?? FontAwesomeIcon.Minus, icons?[1] ?? FontAwesomeIcon.Plus);
        var inputY = FloatInputDrag($"{label}Y", ref y, min, max, step, icons?[2] ?? FontAwesomeIcon.Minus, icons?[3] ?? FontAwesomeIcon.Plus);

        if (inputX || inputY)
        {
            xy = new(x, y);
            update |= Save;
            return true;
        }
        return false;
    }

    public static void Heading(string headingText)
    {
        ImGui.TableNextRow();
        ImGui.TableNextColumn();

        ImGui.PushStyleColor(Text, new Vector4(1, 1, 1, 0.3f));
        ImGui.Text(headingText);
        ImGui.PopStyleColor();
    }
}
