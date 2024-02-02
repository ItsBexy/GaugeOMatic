using CustomNodes;
using Dalamud.Interface;
using FFXIVClientStructs.FFXIV.Component.GUI;
using GaugeOMatic.Windows;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Numerics;
using static CustomNodes.CustomNode.CustomNodeFlags;
using static CustomNodes.CustomNodeManager;
using static Dalamud.Interface.FontAwesomeIcon;
using static FFXIVClientStructs.FFXIV.Component.GUI.AlignmentType;
using static FFXIVClientStructs.FFXIV.Component.GUI.FontType;
using static GaugeOMatic.Utility.Color;
using static GaugeOMatic.Utility.ImGuiHelpy;
using static GaugeOMatic.Widgets.WidgetUI;
using FontType = FFXIVClientStructs.FFXIV.Component.GUI.FontType;

namespace GaugeOMatic.Widgets;

public class LabelTextNode : CustomNode
{
    public LabelTextProps Props { get; set; }
    public string Fallback { get; set; }
    public static unsafe implicit operator AtkResNode*(LabelTextNode l) => l.Node;

    public unsafe LabelTextNode(string label, string fallback)
    {
        Fallback = fallback;
        Node = (AtkResNode*)CreateTextNode(label.Length > 0 ? label : fallback, 20, 52);
        SetTextColor(0xffffffff, 0x000000ff);
        RemoveFlags(SetVisByAlpha);

        Children = Array.Empty<CustomNode>();
    }

    public LabelTextNode SetLabelText(string text)
    {
        SetText(text.Length > 0 ? text : Fallback);
        return this;
    }

    public LabelTextNode ApplyProps(LabelTextProps props, Vector2? posAdjust = null)
    {
        Props = props;
        this.SetLabelText(Props.Text)
            .SetTextColor(Props.Color, Props.EdgeColor)
            .SetTextFont(Props.Font)
            .SetTextAlign(Props.Align)
            .SetTextSize(Props.FontSize)
            .SetPos(Props.Position + (posAdjust ?? new(0)))
            .SetVis(Props.Enabled);
        return this;
    }
}

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

    public static void LabelTextControls(string label, ref LabelTextProps configVal, string hintText, ref UpdateFlags update)
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
