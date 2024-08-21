using CustomNodes;
using Dalamud.Interface;
using FFXIVClientStructs.FFXIV.Component.GUI;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Numerics;
using static CustomNodes.CustomNode.CustomNodeFlags;
using static CustomNodes.CustomNodeManager;
using static Dalamud.Interface.FontAwesomeIcon;
using static FFXIVClientStructs.FFXIV.Component.GUI.AlignmentType;
using static FFXIVClientStructs.FFXIV.Component.GUI.FontType;
using static GaugeOMatic.Utility.Color;
using static GaugeOMatic.Widgets.WidgetUI;
using static Newtonsoft.Json.DefaultValueHandling;

namespace GaugeOMatic.Widgets.Common;

public class LabelTextNode : CustomNode
{
    public LabelTextProps Props { get; set; }
    public string Fallback { get; set; }
    public static unsafe implicit operator AtkResNode*(LabelTextNode l) => l.Node;

    public unsafe LabelTextNode(string label, string fallback)
    {
        Fallback = fallback;
        Node = (AtkResNode*)CreateTextNode(label.Length > 0 ? label : fallback, 20, 52);
        SetText(label);
        SetTextColor(0xffffffff, 0x000000ff);
        RemoveFlags(SetVisByAlpha);

        Children = [];
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
    internal static List<AlignmentType> AlignList = [Left, Center, Right];
    internal static List<FontAwesomeIcon> AlignIcons = [AlignLeft, AlignCenter, AlignRight];
    internal static List<FontType> FontList = [Axis, MiedingerMed, TrumpGothic, Jupiter];
    internal static List<string> FontNames = ["Axis", "Miedinger Med", "Trump Gothic", "Jupiter"];

    public string Text;
    [JsonProperty(DefaultValueHandling = Include)] public bool Enabled = false;
    [JsonProperty(DefaultValueHandling = Include)] public Vector2 Position;
    public ColorRGB Color = new(255, 255, 255);
    public ColorRGB EdgeColor = new(0, 0, 0);
    [JsonProperty(DefaultValueHandling = Include)] public FontType Font = Miedinger;
    [JsonProperty(DefaultValueHandling = Include)] public byte FontSize = 18;
    [JsonProperty(DefaultValueHandling = Include)] public AlignmentType Align = Center;

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

    public static void LabelTextControls(string label, ref LabelTextProps configVal, string hintText)
    {
        var labelTextProps = configVal;

        ToggleControls(label, ref labelTextProps.Enabled);
        if (labelTextProps.Enabled)
        {
            var text = labelTextProps.Text;

            if (StringControls("Override Text", ref text, hintText)) labelTextProps.Text = text;

            PositionControls($"Position##{label}Pos", ref labelTextProps.Position);
            ColorPickerRGBA($"Color##{label}color", ref labelTextProps.Color);
            ColorPickerRGBA($"Edge Color##{label}edgeColor", ref labelTextProps.EdgeColor);

            ComboControls($"Font##{label}font", ref labelTextProps.Font, FontList, FontNames);

            RadioIcons($"Alignment##{label}align", ref labelTextProps.Align, AlignList, AlignIcons);
            IntControls($"Font Size##{label}fontSize", ref labelTextProps.FontSize, 1, 100, 1);
        }

        if (UpdateFlag.HasFlag(UpdateFlags.Save)) configVal = labelTextProps;
    }
}
