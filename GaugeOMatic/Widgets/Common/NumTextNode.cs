using CustomNodes;
using Dalamud.Interface;
using FFXIVClientStructs.FFXIV.Component.GUI;
using GaugeOMatic.Utility;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Numerics;
using static CustomNodes.CustomNode.CustomNodeFlags;
using static CustomNodes.CustomNodeManager;
using static FFXIVClientStructs.FFXIV.Component.GUI.AlignmentType;
using static FFXIVClientStructs.FFXIV.Component.GUI.FontType;
using static GaugeOMatic.Utility.Color;
using static GaugeOMatic.Widgets.Common.CommonParts;
using static GaugeOMatic.Widgets.WidgetUI;
using static GaugeOMatic.Widgets.WidgetUI.UpdateFlags;
using static Newtonsoft.Json.DefaultValueHandling;

namespace GaugeOMatic.Widgets;

public class NumTextNode : CustomNode
{
    public NumTextProps Props { get; set; } = new();
    public CustomNode BgNode;
    public static unsafe implicit operator AtkResNode*(NumTextNode n) => n.Node;

    public unsafe NumTextNode()
    {
        BgNode = new CustomNode(CreateNineGridNode(BgPart, 0)).SetNineGridOffset(0, 21, 0, 21)
                                                                 .SetSize(65, 40)
                                                                 .SetPos(-17, -21)
                                                                 .SetOrigin(16, 20)
                                                                 .Hide();

        Node = (AtkResNode*)CreateTextNode(" ", 18, 20);
        SetText(" ");
        Children = [BgNode];
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
            .SetPos(Props.Position + (posAdjust ?? new(0)) + new Vector2(-Props.FontSize * 3f, 0))
            .SetVis(Props.Enabled);

        BgNode.SetScale(Props.FontSize / 20f)
              .SetAddRGB(Props.BgColor, true);
    }

    public void UpdateValue(float current, float max)
    {
        var text = Props.ShowZero ? PrecisionString(0) : " ";
        if (Props.Enabled && current != 0)
        {
            if (Props.Precision == 0) current = (float)Math.Ceiling(current);

            var numText = ((double)(Props.Invert ? max - current : current)).ToString(PrecisionString(Props.Precision));
            if (numText != "0") text = numText;
        }

        SetText(text);
        UpdateBg(text);
        return;

        static string PrecisionString(int p) => p <= 0 ? "0" : "0.".PadRight(p + 2, '0');
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
    internal static List<AlignmentType> AlignList = [Left, Center, Right];
    internal static List<FontAwesomeIcon> AlignIcons =
        [FontAwesomeIcon.AlignLeft, FontAwesomeIcon.AlignCenter, FontAwesomeIcon.AlignRight];
    internal static List<FontType> FontList = [Axis, MiedingerMed, Miedinger, TrumpGothic, Jupiter];
    internal static List<string> FontNames = ["Axis", "Miedinger Med", "Miedinger", "Trump Gothic", "Jupiter"];

    [JsonProperty(DefaultValueHandling = Include)] public bool Enabled;
    [JsonProperty(DefaultValueHandling = Include)] public Vector2 Position;
    public ColorRGB Color = new(255);
    public ColorRGB EdgeColor = new(0);
    [JsonProperty(DefaultValueHandling = Include)] public bool ShowBg = false;
    public AddRGB BgColor = new(0, 0, 0);
    public byte FontSize = 18;
    [JsonProperty(DefaultValueHandling = Include)] public bool Invert = false;
    [JsonProperty(DefaultValueHandling = Include)] public bool ShowZero = false;
    [JsonProperty(DefaultValueHandling = Include)] public int Precision = 0;
    [JsonProperty(DefaultValueHandling = Include)] public FontType Font = MiedingerMed;
    [JsonProperty(DefaultValueHandling = Include)] public AlignmentType Align = Center;

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

    public static void NumTextControls(string label, ref NumTextProps configVal, bool separatorAfter = false)
    {
        var numTextProps = configVal;
        ToggleControls(label, ref numTextProps.Enabled);

        if (numTextProps.Enabled)
        {
            PositionControls("Position", ref numTextProps.Position);
            ColorPickerRGBA($"Color##{label}color", ref numTextProps.Color);
            ColorPickerRGBA($"Edge Color##{label}edgeColor", ref numTextProps.EdgeColor);
            ToggleControls("Backdrop", ref numTextProps.ShowBg);
            if (numTextProps.ShowBg) ColorPickerRGBA($"Backdrop Color##{label}bgColor", ref numTextProps.BgColor);

            ComboControls($"Font##{label}font", ref numTextProps.Font, FontList, FontNames);
            RadioIcons($"Alignment##{label}align", ref numTextProps.Align, AlignList, AlignIcons);
            IntControls($"Font Size##{label}fontSize", ref numTextProps.FontSize, 1, 100, 1);

            RadioControls("Precision ", ref numTextProps.Precision, [0, 1, 2], ["0", "1", "2"], true);
            ToggleControls("Invert Value ", ref numTextProps.Invert);
            ToggleControls("Show Zero ", ref numTextProps.ShowZero);
            if (separatorAfter) ImGuiHelpy.TableSeparator(2);
        }

        if (UpdateFlag.HasFlag(Save)) configVal = numTextProps;
    }
}
