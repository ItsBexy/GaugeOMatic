using CustomNodes;
using Dalamud.Interface;
using FFXIVClientStructs.FFXIV.Component.GUI;
using GaugeOMatic.Utility;
using GaugeOMatic.Windows;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Numerics;
using GaugeOMatic.Widgets.Common;
using static CustomNodes.CustomNode.CustomNodeFlags;
using static CustomNodes.CustomNodeManager;
using static FFXIVClientStructs.FFXIV.Component.GUI.AlignmentType;
using static FFXIVClientStructs.FFXIV.Component.GUI.FontType;
using static GaugeOMatic.Utility.Color;
using static GaugeOMatic.Widgets.WidgetUI;

namespace GaugeOMatic.Widgets;

public class NumTextNode : CustomNode
{
    public NumTextProps Props { get; set; } = new();
    public CustomNode BgNode;
    public static unsafe implicit operator AtkResNode*(NumTextNode n) => n.Node;

    public unsafe NumTextNode()
    {
        BgNode = new CustomNode(CreateNineGridNode(CommonParts.BgPart, 0)).SetNineGridOffset(0, 21, 0, 21)
                                                                 .SetSize(65, 40)
                                                                 .SetPos(-17, -21)
                                                                 .SetOrigin(16, 20)
                                                                 .Hide();

        Node = (AtkResNode*)CreateTextNode(" ", 18, 20);
        Children = new[] { BgNode };
        RemoveFlags(SetVisByAlpha);
    }

    public void ApplyProps(NumTextProps props, Vector2? posAdjust = null)
    {
        Props = props;
        this.SetTextColor(Props.Color, Props.EdgeColor)
            .SetTextFont(Props.Font)
            .SetTextAlign(Props.Align)
            .SetWidth(Props.FontSize * 6f)
            .SetTextSize(Props.FontSize)
            .SetPos(Props.Position + (posAdjust??new(0)) + new Vector2(-Props.FontSize*3f, 0))
            .SetVis(Props.Enabled);

        BgNode.SetScale(Props.FontSize / 20f)
              .SetAddRGB(Props.BgColor, true);
    }

    public void UpdateValue(float current, float max)
    {
        static string PrecisionString(int p) => p <= 0 ? "0" : "0.".PadRight(p + 2, '0');

        var text = Props.ShowZero ? PrecisionString(0) : " ";
        if (Props.Enabled && current != 0)
        {
            if (Props.Precision == 0) current = (float)Math.Ceiling(current);

            var numText = ((double)(Props.Invert ? max - current : current)).ToString(PrecisionString(Props.Precision));
            if (numText != "0") text = numText;
        }

        SetText(text);
        UpdateBg(text);
    }

    public unsafe void UpdateBg(string text)
    {
        if (!Props.ShowBg || text == " ") BgNode.SetWidth(0).Hide();
        else
        {
            var sizeRatio = Props.FontSize / 20f;
            var bgWidth = (int)(GetTextDrawSize().X / sizeRatio) + 36;

            BgNode.Show().SetWidth(bgWidth);
            switch (Props.Align)
            {
                case Left:

                    SetScaleX(Node->ScaleY);
                    BgNode.SetX((bgWidth * 0.005f) - 16.945f)
                          .SetOrigin((bgWidth * 0.0011f) + 16.31f, 20)
                          .SetWidth(bgWidth);
                    break;
                case Right:

                    SetScaleX(-Node->ScaleY);
                    BgNode.SetX((bgWidth * 0.005f) - 16.945f - (122f * sizeRatio))
                          .SetOrigin((bgWidth * 0.0011f) + 16.31f, 20);
                    break;
                default:
                    SetScaleX(Node->ScaleY);
                    BgNode.SetX((bgWidth * -0.4737f) + 58.997f)
                          .SetOrigin((bgWidth * 0.4729f) - 59.204f, 20);
                    break;
            }

             

            BgNode.SetHeight(Props.Font == Jupiter ? 44 : 40);
            BgNode.SetY(Props.Font == Miedinger ? -22f : -21);
        }
    }
}

public struct NumTextProps
{
    internal static List<AlignmentType> AlignList = new() { Left, Center, Right };
    internal static List<FontAwesomeIcon> AlignIcons = new() { FontAwesomeIcon.AlignLeft, FontAwesomeIcon.AlignCenter, FontAwesomeIcon.AlignRight };
    internal static List<FontType> FontList = new() { Axis, MiedingerMed, Miedinger, TrumpGothic, Jupiter };
    internal static List<string> FontNames = new() { "Axis", "Miedinger Med", "Miedinger", "Trump Gothic", "Jupiter" };

    public bool Enabled;
    public Vector2 Position;
    public ColorRGB Color = new(255);
    public ColorRGB EdgeColor = new(0);
    public bool ShowBg = false;
    public AddRGB BgColor = new(0, 0, 0);
    public byte FontSize = 18;
    public bool Invert = false;
    public bool ShowZero = false;
    public int Precision = 0;
    public FontType Font = MiedingerMed;
    public AlignmentType Align = Center;

    public NumTextProps(bool enabled, Vector2 position, ColorRGB color, ColorRGB edgeColor, bool showBg, AddRGB bgColor, FontType font, byte fontSize, AlignmentType align, bool invert, int precision = 0, bool showZero = false)
    {
        Enabled = enabled;
        Position = position;
        Color = color;
        EdgeColor = edgeColor;
        ShowBg = showBg;
        BgColor = bgColor;
        FontSize = fontSize;
        Invert = invert;
        ShowZero = showZero;
        Precision = precision;
        Font = font;
        Align = align;
    }

    public NumTextProps()
    {
        Enabled = false;
        Position = new(0);
        Color = new(255, 255, 255);
        EdgeColor = new(0, 0, 0);
        ShowBg = false;
        BgColor = new(0);
        FontSize = 18;
        Invert = false;
        ShowZero = false;
        Precision = 0;
        Font = MiedingerMed;
        Align = Center;
    }

    public static void NumTextControls(string label, ref NumTextProps configVal, ref UpdateFlags update)
    {
        var numTextProps = configVal;
        ImGuiHelpy.TableSeparator(2);

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
            ToggleControls("Backdrop", ref numTextProps.ShowBg, ref update);
            if (numTextProps.ShowBg) ColorPickerRGBA($"Backdrop Color##{label}bgColor", ref numTextProps.BgColor, ref update);

            ComboControls($"Font##{label}font", ref numTextProps.Font, FontList, FontNames, ref update);
            RadioIcons($"Alignment##{label}align", ref numTextProps.Align, AlignList, AlignIcons, ref update);
            IntControls($"Font Size##{label}fontSize", ref numTextProps.FontSize, 1, 100, 1, ref update);

            RadioControls("Precision ", ref numTextProps.Precision, new() { 0, 1, 2 }, new() { "0", "1", "2" }, ref update, true);
            ToggleControls("Invert Value ", ref numTextProps.Invert, ref update);
            ToggleControls("Show Zero ", ref numTextProps.ShowZero, ref update);
            ImGui.TreePop();
        }

        if (update.HasFlag(UpdateFlags.Save)) configVal = numTextProps;
    }
}
