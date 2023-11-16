using Dalamud.Interface;
using FFXIVClientStructs.FFXIV.Component.GUI;
using GaugeOMatic.Windows;
using ImGuiNET;
using System.Collections.Generic;
using System.Numerics;
using static CustomNodes.CustomNodeManager;
using static Dalamud.Interface.FontAwesomeIcon;
using static FFXIVClientStructs.FFXIV.Component.GUI.AlignmentType;
using static FFXIVClientStructs.FFXIV.Component.GUI.FontType;
using static GaugeOMatic.Utility.Color;
using static GaugeOMatic.Utility.ImGuiHelpers;
using static GaugeOMatic.Widgets.WidgetUI;

namespace GaugeOMatic.Widgets;

public struct LabelTextProps
{
    internal static List<AlignmentType> AlignList = new() { Left, Center, Right };
    internal static List<FontAwesomeIcon> AlignIcons = new() { AlignLeft, AlignCenter, AlignRight };
    internal static List<FontType> FontList = new() { Axis, MiedingerMed, TrumpGothic, Jupiter };
    internal static List<string> FontNames = new() { "Axis", "Miedinger Med", "Trump Gothic", "Jupiter" };

    public string Text;
    public bool Enabled = false;
    public Vector2 Position;
    public ColorRGB Color = new(255, 255, 255);
    public ColorRGB EdgeColor = new(0, 0, 0);
    public FontType Font = Miedinger;
    public byte FontSize = 18;
    public AlignmentType Align = Center;

    public LabelTextProps(string text, bool enabled, Vector2 position, ColorRGB color, ColorRGB edgeColor, FontType font, byte fontSize, AlignmentType align)
    {
        Text = text;
        Enabled = enabled;
        Position = position;
        Color = color;
        EdgeColor = edgeColor;
        Font = font;
        FontSize = fontSize;
        Align = align;
    }

    public static unsafe CustomNode CreateLabelTextNode(string label, string fallback) => new CustomNode(CreateTextNode(label.Length > 0 ? label : fallback, 20, 52)).SetTextColor(0xffffffff, new(0, 0, 0));

    public readonly void ApplyTo(CustomNode labelTextNode) => labelTextNode.SetTextColor(Color, EdgeColor)
                                                                           .SetTextFont(Font)
                                                                           .SetTextAlign(Align)
                                                                           .SetTextSize(FontSize)
                                                                           .SetPos(Position)
                                                                           .SetVis(Enabled);

    public readonly void ApplyTo(CustomNode labelTextNode, Vector2 posAdjust)
    {
        ApplyTo(labelTextNode);
        labelTextNode.SetPos(Position + posAdjust);
    }

    public static void LabelTextControls2(string label, ref LabelTextProps configVal, string hintText, ref UpdateFlags update)
    {
        var labelTextProps = configVal;
        TableSeparator(2);

        ImGui.TableNextRow();
        ImGui.TableNextColumn();

        var treeNode = ImGui.TreeNodeEx($"{label}##{label}treeRow");

        var enabled = labelTextProps.Enabled;
        ImGui.TableNextColumn();
        if (ImGui.Checkbox($"##{label}Enabled", ref enabled))
        {
            labelTextProps.Enabled = enabled;
            update |= UpdateFlags.Save;
        }

        if (treeNode)
        {
            var text = labelTextProps.Text;

            if (StringControls("Override Text", ref text, hintText, ref update)) labelTextProps.Text = text;

            PositionControls($"Position##{label}Pos", ref labelTextProps.Position, ref update);
            ColorPickerRGBA($"Color##{label}color", ref labelTextProps.Color, ref update);
            ColorPickerRGBA($"Edge Color##{label}edgeColor", ref labelTextProps.EdgeColor, ref update);

            ComboControls($"Font##{label}font", ref labelTextProps.Font, FontList, FontNames, ref update);

            RadioIcons($"Alignment##{label}align", ref labelTextProps.Align, AlignList, AlignIcons, ref update);
            IntControls($"Font Size##{label}fontSize", ref labelTextProps.FontSize, 1, 100, 1, ref update);

            ImGui.TreePop();
        }

        if (update.HasFlag(UpdateFlags.Save)) configVal = labelTextProps;
    }
}



/// <summary>
/// Struct summarizing a timer display.
/// </summary>
public struct NumTextProps
{
    internal static List<AlignmentType> AlignList = new() { Left, Center, Right };
    internal static List<FontAwesomeIcon> AlignIcons = new() { AlignLeft, AlignCenter, AlignRight };
    internal static List<FontType> FontList = new() { Axis, MiedingerMed, Miedinger, TrumpGothic, Jupiter };
    internal static List<string> FontNames = new() { "Axis", "Miedinger Med", "Miedinger", "Trump Gothic", "Jupiter" };

    public bool Enabled;
    public Vector2 Position;
    public ColorRGB Color = new(255,255,255);
    public ColorRGB EdgeColor = new(0,0,0);
    public byte FontSize = 18;
    public bool Invert = false;
    public bool ShowZero = false;
    public int Precision = 0;
    public FontType Font = MiedingerMed;
    public AlignmentType Align = Center;

    public NumTextProps(bool enabled, Vector2 position, ColorRGB color, ColorRGB edgeColor, FontType font, byte fontSize, AlignmentType align, bool invert, int precision = 0, bool showZero = false)
    {
        Enabled = enabled;
        Position = position;
        Color = color;
        EdgeColor = edgeColor;
        Font = font;
        FontSize = fontSize;
        Align = align;
        Invert = invert;
        Precision = precision;
        ShowZero = showZero;
    }

    public NumTextProps()
    {
        Enabled = false;
        Position = new(0);
        Color = new(255, 255, 255);
        EdgeColor = new(0, 0, 0);
        Font = MiedingerMed;
        FontSize = 18;
        Align = Center;
        Invert = false;
        Precision = 0;
        ShowZero = false;
    }

    public static unsafe CustomNode CreateNumTextNode() => new((AtkResNode*)CreateTextNode("0", 18, 20));

    public readonly void ApplyTo(CustomNode numTextNode) => numTextNode.SetTextColor(Color, EdgeColor)
                                                                       .SetTextFont(Font)
                                                                       .SetTextAlign(Align)
                                                                       .SetTextSize(FontSize)
                                                                       .SetPos(Position)
                                                                       .SetVis(Enabled);

    public readonly void ApplyTo(CustomNode numTextNode, Vector2 posAdjust)
    {
       ApplyTo(numTextNode);
       numTextNode.SetPos(Position + posAdjust);
    }

    public static string PrecisionString(int p) => p <= 0 ? "0" : "0.".PadRight(p + 2, '0');

    public static void NumTextControls(string label, ref NumTextProps configVal, ref UpdateFlags update)
    {
        var numTextProps = configVal;
        TableSeparator(2);

        ImGui.TableNextRow();
        ImGui.TableNextColumn();
        var treeNode = ImGui.TreeNodeEx($"{label}##{label}treeRow");

        var enabled = numTextProps.Enabled;
        ImGui.TableNextColumn();
        if (ImGui.Checkbox($"##{label}Enabled", ref enabled))
        {
            numTextProps.Enabled = enabled;
            update |= UpdateFlags.Save;
        }

        if (treeNode)
        {
            PositionControls("Position", ref numTextProps.Position, ref update);
            ColorPickerRGBA($"Color##{label}color", ref numTextProps.Color, ref update);
            ColorPickerRGBA($"Edge Color##{label}edgeColor", ref numTextProps.EdgeColor, ref update);

            ComboControls($"Font##{label}font", ref numTextProps.Font, FontList, FontNames, ref update);
            RadioIcons($"Alignment##{label}align", ref numTextProps.Align, AlignList, AlignIcons, ref update);
            IntControls($"Font Size##{label}fontSize", ref numTextProps.FontSize, 1, 100, 1, ref update);

            RadioControls("Precision ", ref numTextProps.Precision, new() { 0, 1, 2 }, new() { "0", "1", "2" }, ref update,true);
            ToggleControls("Invert Value ", ref numTextProps.Invert, ref update);
            ToggleControls("Show Zero ", ref numTextProps.ShowZero, ref update);
            ImGui.TreePop();
        }

        if (update.HasFlag(UpdateFlags.Save)) configVal = numTextProps;
    }
}
