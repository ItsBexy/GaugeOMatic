using GaugeOMatic.Trackers;
using GaugeOMatic.Windows;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Numerics;
using static CustomNodes.CustomNodeManager;
using static FFXIVClientStructs.FFXIV.Component.GUI.AlignmentType;
using static FFXIVClientStructs.FFXIV.Component.GUI.FontType;
using static GaugeOMatic.Utility.Color;
using static GaugeOMatic.Utility.MiscMath;
using static GaugeOMatic.Widgets.GaugeBarWidget;
using static GaugeOMatic.Widgets.NinkiBorders;
using static GaugeOMatic.Widgets.NumTextProps;
using static GaugeOMatic.Widgets.WidgetTags;
using static GaugeOMatic.Widgets.WidgetUI;
using static GaugeOMatic.Windows.UpdateFlags;

namespace GaugeOMatic.Widgets;

public sealed unsafe class NinkiBorders : Widget
{
    public override WidgetInfo WidgetInfo => GetWidgetInfo;

    public static WidgetInfo GetWidgetInfo => new()
    {
        DisplayName = "Ninki Borders",
        Author = "ItsBexy",
        Description = "A set of gauge bars fitted over the top/bottom borders of the Ninki Gauge (or a replica of it). A tracker can use both bars, or just one.",
        WidgetTags = GaugeBar | MultiComponent,
        KeyText = "NK3"
    };

    public override CustomPartsList[] PartsLists { get; } = {
        new ("ui/uld/JobHudNIN0.tex",
             new(0, 252, 208, 20),
             new(0, 272, 208, 16),
             new(256, 152, 20, 88),
             new(0, 196, 196, 56))
    };

    #region Nodes

    public CustomNode BorderTop;
    public CustomNode BorderBottom;
    public CustomNode TickTop;
    public CustomNode TickBottom;
    public CustomNode Shine;
    public CustomNode Calligraphy;
    public CustomNode NumTextNode;

    public override CustomNode BuildRoot()
    {
        BorderTop = ImageNodeFromPart(0, 0).SetPos(11,19).SetAlpha(255).SetImageFlag(32).SetImageWrap(1);
        BorderBottom = ImageNodeFromPart(0, 1).SetPos(11,77).SetAlpha(255).SetImageFlag(32).SetImageWrap(1);
        TickTop = ImageNodeFromPart(0, 2).SetPos(0,-16).SetOrigin(22,43.5f).SetScale(0.5f,0.15f);
        TickBottom = ImageNodeFromPart(0, 2).SetPos(0,38).SetOrigin(22,43.5f).SetScale(0.5f,0.15f);
        Shine = ImageNodeFromPart(0, 2).SetAlpha(0).SetOrigin(22, 43.5f).SetImageFlag(32);
        Calligraphy = ImageNodeFromPart(0, 3).SetPos(23, 29).SetOrigin(98, 28).SetImageFlag(32).SetAlpha(0);
        NumTextNode = CreateNumTextNode();

        return new(CreateResNode(), BorderTop, BorderBottom, TickTop, TickBottom, Shine, Calligraphy, NumTextNode);
    }

    #endregion

    #region Animations

    private void AppearAnim()
    {
        Tweens.Add(new(Calligraphy,
                       new(0) { Scale = 1, Alpha = 70 },
                       new(100) { Scale = 1, Alpha = 160 },
                       new(300) { Scale = 1.5f, Alpha = 0 }));

        Tweens.Add(new(Shine,
                       new(0) { X = 11, Y = 14, Alpha = 76 },
                       new(200) { X = 190, Y = 10, Alpha = 76 },
                       new(400) { X = 300, Y = 10, Alpha = 0 }));

        Tweens.Add(new(Shine,
                       new(0) { ScaleX = 1.15f, ScaleY = 1.5f, Rotation = 0 },
                       new(100) { ScaleX = 3f, ScaleY = 1.2f, Rotation = -0.065f },
                       new(400) { ScaleX = 0.6f, ScaleY = 2f, Rotation = 0 }));
    }

    #endregion

    #region UpdateFuncs

    public override string? SharedEventGroup => null;

    public static float CalcTickY(float prog) => PolyCalc(prog, -14.8249956874245, 38.6612557476222, -102.819457745683, 62.3102095019424);

    public override void Update()
    {
        var current = Tracker.CurrentData.GaugeValue;
        var previous = Tracker.PreviousData.GaugeValue;
        var max = Tracker.CurrentData.MaxGauge;

        var prog = Math.Clamp(current / max, 0f, 1f);
        var prevProg = Math.Clamp(previous / max, 0f, 1f);

        if (prog > 0 && prevProg == 0) AppearAnim();

        var curWid = (ushort)Math.Round((prog * 190f) + 10f);
        var prevWid = (ushort)Math.Round((prevProg * 190f) + 10f);

        NumTextNode.UpdateNumText(Config.NumTextProps, current, max);

        AnimateDrainGain(DrainGainType.Width,ref Tweens, BorderTop, curWid, prevWid, Config.AnimationLength);
        AnimateDrainGain(DrainGainType.Width, ref Tweens, BorderBottom, curWid, prevWid, Config.AnimationLength);

        var tweenWidth = BorderTop.Node->Width;
        var tweenProg = (tweenWidth - 10f) / 190f;

        if (tweenProg > 0)
        {
            var tickY = CalcTickY(tweenProg);
            TickTop.SetPos(tweenWidth - 7, tickY);
            TickBottom.SetPos(tweenWidth - 7, tickY + 54);
        }

        var borderAlpha = Config.BorderColor.A;
        var tickAlpha = Config.TickColor.A;

        switch (tweenProg)
        {
            case < 0.05f:
                BorderTop.Node->Color.A = (byte)(tweenProg / 0.05f * borderAlpha);
                TickTop.Node->Color.A = (byte)(tweenProg / 0.05f * tickAlpha);
                BorderBottom.Node->Color.A = (byte)(tweenProg / 0.05f * borderAlpha);
                TickBottom.Node->Color.A = (byte)(tweenProg / 0.05f * tickAlpha);
                break;
            default:
                BorderTop.Node->Color.A = borderAlpha;
                TickTop.Node->Color.A = tickAlpha;
                BorderBottom.Node->Color.A = borderAlpha;
                TickBottom.Node->Color.A = tickAlpha;
                break;
        }

        RunTweens();
    }

    #endregion

    #region Configs

    public sealed class NinkiBordersConfig : GaugeBarWidgetConfig
    {
        public Vector2 Position = new(0, 0);
        public float Scale = 1;
        public ColorRGB BorderColor = new(255, 90, 0);
        public ColorRGB TickColor = new(255, 195, 144);
        public bool Top = true;
        public bool Bottom = true;
        protected override NumTextProps NumTextDefault => new(true, new(212, 30), new(255, 241, 197), new(110, 25, 0), MiedingerMed, 20, Center, false);

        public NinkiBordersConfig(WidgetConfig widgetConfig)
        {
            NumTextProps = NumTextDefault;
            var config = widgetConfig.NinkiBordersCfg;

            if (config == null) return;

            Position = config.Position;
            Scale = config.Scale;
            BorderColor = config.BorderColor;
            TickColor = config.TickColor;
            Top = config.Top;
            Bottom = config.Bottom;
            NumTextProps = config.NumTextProps;
            AnimationLength = config.AnimationLength;
            Invert = config.Invert;
        }

        public NinkiBordersConfig() { }
    }

    public NinkiBordersConfig Config = null!;

    public override void InitConfigs()
    {
        Config = new(Tracker.WidgetConfig);
        if (Tracker.WidgetConfig.NinkiBordersCfg == null && Tracker.RefType == RefType.Action) { Config.Invert = true; }
    }

    public override void ResetConfigs() => Config = new();

    public override void ApplyConfigs()
    {
        WidgetRoot.SetPos(Config.Position).SetScale(Config.Scale);

        BorderTop.SetVis(Config.Top).SetRGB((Vector4)Config.BorderColor);
        TickTop.SetVis(Config.Top).SetRGB((Vector4)Config.TickColor);

        Shine.SetRGB(Config.TickColor);
        Calligraphy.SetAddRGB((Vector4)Config.TickColor);

        BorderBottom.SetVis(Config.Bottom).SetRGB((Vector4)Config.BorderColor);
        TickBottom.SetVis(Config.Bottom).SetRGB((Vector4)Config.TickColor);

        Config.NumTextProps.ApplyTo(NumTextNode);
    }

    public override void DrawUI(ref WidgetConfig widgetConfig, ref UpdateFlags update)
    {
        Heading("Layout");

        PositionControls("Position", ref Config.Position, ref update);
        ScaleControls("Scale", ref Config.Scale, ref update);
        var borders = new List<bool> { Config.Top, Config.Bottom };
        if (ToggleControls("Show", ref borders, new List<string> { "Top", "Bottom" }, ref update))
        {
            Config.Top = borders[0];
            Config.Bottom = borders[1];
        }

        Heading("Colors");

        ColorPickerRGBA("Border Color", ref Config.BorderColor, ref update);
        ColorPickerRGBA("Tick Color", ref Config.TickColor, ref update);

        Heading("Behavior");

        ToggleControls("Invert Fill", ref Config.Invert, ref update);

      //  IntControls("Animation Time", ref Config.AnimationLength, 0, 2000, 50, ref update);

        NumTextControls($"{Tracker.TermGauge} Text", ref Config.NumTextProps, ref update);

        if (update.HasFlag(Save)) ApplyConfigs();
        widgetConfig.NinkiBordersCfg = Config;
    }

    #endregion

    public NinkiBorders(Tracker tracker) : base(tracker) { }
}

public partial class WidgetConfig
{
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] public NinkiBordersConfig? NinkiBordersCfg { get; set; }
}
