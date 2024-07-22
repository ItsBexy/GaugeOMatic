using CustomNodes;
using Dalamud.Interface;
using GaugeOMatic.CustomNodes.Animation;
using GaugeOMatic.Trackers;
using GaugeOMatic.Windows;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Numerics;
using static CustomNodes.CustomNodeManager;
using static Dalamud.Interface.FontAwesomeIcon;
using static FFXIVClientStructs.FFXIV.Component.GUI.AlignmentType;
using static FFXIVClientStructs.FFXIV.Component.GUI.FontType;
using static GaugeOMatic.CustomNodes.Animation.KeyFrame;
using static GaugeOMatic.CustomNodes.Animation.Tween.EaseType;
using static GaugeOMatic.Utility.Color;
using static GaugeOMatic.Widgets.Common.CommonParts;
using static GaugeOMatic.Widgets.GaugeBarWidgetConfig;
using static GaugeOMatic.Widgets.LabelTextProps;
using static GaugeOMatic.Widgets.MilestoneType;
using static GaugeOMatic.Widgets.NumTextProps;
using static GaugeOMatic.Widgets.SoulBar;
using static GaugeOMatic.Widgets.WidgetTags;
using static GaugeOMatic.Widgets.WidgetUI;
using static System.Math;

#pragma warning disable CS8618

namespace GaugeOMatic.Widgets;

public sealed unsafe class SoulBar : GaugeBarWidget
{
    public SoulBar(Tracker tracker) : base(tracker) { }

    public override WidgetInfo WidgetInfo => GetWidgetInfo;

    public static WidgetInfo GetWidgetInfo => new()
    {
        DisplayName = "Soul Bar",
        Author = "ItsBexy",
        Description = "A recreation of Reaper's Soul gauge bar",
        WidgetTags = GaugeBar | Replica
    };

    public override CustomPartsList[] PartsLists { get; } = { RPR0 };

    #region Nodes

    public CustomNode BarFrame;
    public CustomNode Bar;
    public CustomNode Frame;
    public CustomNode Corners;
    public CustomNode MidMarkers;

    public LabelTextNode LabelTextNode;

    public CustomNode Backdrop;
    public CustomNode GlowFrame;
    public CustomNode TickMark;
    public CustomNode TickGradient;
    public CustomNode TickLine;
    public CustomNode TickDot;

    public override CustomNode BuildContainer()
    {
        Frame = NineGridFromPart(0, 11, 5, 5, 5, 5).SetHeight(20).SetOrigin(10, 10);
        Bar = BuildBar();
        Corners = NineGridFromPart(0, 3, 14, 14, 14, 14).SetHeight(30).SetY(-5).SetOrigin(15, 15);
        MidMarkers = NineGridFromPart(0, 2, 14, 0, 14, 0).SetHeight(30).SetY(-5).SetX(-4);
        NumTextNode = new();
        LabelTextNode = new(Config.LabelTextProps.Text, Tracker.DisplayName);
        LabelTextNode.SetWidth(180);

        BarFrame = new(CreateResNode(), Frame, Bar, Corners, MidMarkers, LabelTextNode, NumTextNode);

        return new CustomNode(CreateResNode(), BarFrame).SetOrigin(28, 12);
    }

    private CustomNode BuildBar()
    {
        Backdrop = NineGridFromPart(0, 7, 0, 10, 0, 10).SetHeight(12);
        Drain = NineGridFromPart(0, 12).SetPos(1, 1).SetHeight(10);
        Gain = NineGridFromPart(0, 12).SetPos(1, 1).SetHeight(10);
        Main = ImageNodeFromPart(0, 9).SetPos(1, 1).SetHeight(10).SetImageWrap(1);

        GlowFrame = NineGridFromPart(0, 14, 7, 7, 7, 7).SetHeight(20).SetPos(-6, -4).SetAlpha(0);
        TickMark = BuildTickMark();

        return new CustomNode(CreateResNode(), Backdrop, Drain, Gain, Main, GlowFrame, TickMark).SetY(3.5f).SetOrigin(6, 6);
    }

    private CustomNode BuildTickMark()
    {
        TickGradient = ImageNodeFromPart(0, 6).SetPos(-16, 0).SetOrigin(18, 6).SetAddRGB(200, -200, -100).SetImageWrap(2).SetImageFlag(32);
        TickLine = ImageNodeFromPart(0, 13).SetPos(-6, 0).SetOrigin(5, 6).SetImageWrap(2).SetImageFlag(32);
        TickDot = ImageNodeFromPart(0, 10).SetPos(-7, -2).SetOrigin(7, 8).SetAlpha(0xCC).SetImageWrap(2).SetImageFlag(32);

        Animator += new Tween(TickGradient,
                              new(0) { X = -18, Alpha = 42 },
                              new(390) { X = -17, Alpha = 50 },
                              new(790) { X = -15, Alpha = 0 })
                              { Repeat = true };

        Animator += new Tween(TickLine,
                              new(0) { Scale = 1f },
                              new(450) { Scale = 1.2f },
                              new(800) { Scale = 1f })
                              { Repeat = true, Ease = SinInOut };

        Animator += new Tween(TickDot,
                              new(0) { Scale = 0.5f },
                              new(350) { Scale = 0.7f },
                              new(800) { Scale = 0.5f })
                              { Repeat = true, Ease = SinInOut };

        return new CustomNode(CreateResNode(), TickGradient, TickLine, TickDot).SetOrigin(0, 6);
    }

    #endregion

    #region Animations

    public KeyFrame[] BarTimeline => new KeyFrame[] { new(0) { Width = 0 }, new(1) { Width = Config.Width }};

    public void CollapseBar(int kf1, int kf2, int kf3)
    {
        Animator -= "Expand";
        var bgWidth = Config.Width + 2;
        var frameWidth = Config.Width + 10;
        var cornerWidth = Config.Width + 20;
        var flipFactor = Config.Mirror ? -1 : 1;

        Animator += new Tween[]
        {
            new(Frame,
                new(0) { Width = frameWidth, Height = 20, X = -frameWidth / 2, Y = 0, Alpha = 255, Rotation = 0, AddRGB = 0 },
                new(kf1) { Width = 20, Height = 20, X = -10, Y = 0, Alpha = 255, Rotation = 0, AddRGB = 10 },
                new(kf2) { Width = 20, Height = 20, X = -10, Y = 0, Alpha = 255, Rotation = 0.785398163397448f, AddRGB = 50 },
                new(kf3) { Width = 0, Height = 0, X = -10, Y = 14.1421356f, Alpha = 0, Rotation = 0.785398163397448f, AddRGB = 120 })
                { Ease = SinInOut, Label = "Collapse" },
            new(Corners,
                new(0) { Width = cornerWidth, Height = 30, X = -cornerWidth / 2, Y = -5, Alpha = 255, Rotation = 0, AddRGB = 0 },
                new(kf1) { Width = 30, Height = 30, X = -15, Y = -5, Alpha = 255, Rotation = 0, AddRGB = 10 },
                new(kf2) { Width = 30, Height = 30, X = -15, Y = -5, Alpha = 255, Rotation = 0.785398163397448f, AddRGB = 50 },
                new(kf3 + (kf3 > 0 ? 150 : 0)) { Width = 0, Height = 0, X = -15, Y = 21.2132f, Alpha = 0, Rotation = 0.785398163397448f, AddRGB = 120 })
                { Ease = SinInOut, Label = "Collapse" },
            new(Bar,
                new(0) { ScaleX = flipFactor, Alpha = 255 },
                new(kf1) { ScaleX = 12 / bgWidth * flipFactor, Alpha = 0 },
                new(kf2) { ScaleX = 12 / bgWidth * flipFactor, Alpha = 0 })
                { Ease = SinInOut, Label = "Collapse" },
            new(MidMarkers,
                new(0) { Alpha = 255, X = MidMarkerX() },
                new(kf1) { Alpha = 0, X = -4 })
                { Ease = SinInOut, Label = "Collapse" },
            new(LabelTextNode,
                Visible[0],
                Hidden[kf2])
                { Ease = SinInOut, Label = "Collapse" },
            new(NumTextNode,
                Visible[0],
                Hidden[kf2])
                { Ease = SinInOut, Label = "Collapse" }
        };
    }

    public void ExpandBar(int kf1, int kf2, int kf3)
    {
        Animator -= "Collapse";
        var frameWidth = Config.Width + 10;
        var bgWidth = Config.Width + 2;
        var cornerWidth = Config.Width + 20;
        var flipFactor = Config.Mirror ? -1 : 1;

        Animator += new Tween[]
        {
            new(Frame,
                new(0) { Width = 0, Height = 0, X = -10, Y = 14.1421356f, Alpha = 0, Rotation = 0.785398163397448f, AddRGB = 120 },
                new(kf1) { Width = 20, Height = 20, X = -10, Y = 0, Alpha = 255, Rotation = 0.785398163397448f, AddRGB = 50 },
                new(kf2) { Width = 20, Height = 20, X = -10, Y = 0, Alpha = 255, Rotation = 0, AddRGB = 10 },
                new(kf3) { Width = frameWidth, Height = 20, X = -frameWidth / 2, Y = 0, Alpha = 255, Rotation = 0, AddRGB = 0 })
                { Ease = SinInOut, Label = "Expand" },
            new(Corners,
                new(0) { Width = 0, Height = 0, X = -15, Y = 21.2132f, Alpha = 0, Rotation = 0.785398163397448f, AddRGB = 120 },
                new(kf1) { Width = 30, Height = 30, X = -15, Y = -5, Alpha = 255, Rotation = 0.785398163397448f, AddRGB = 50 },
                new(kf2) { Width = 30, Height = 30, X = -15, Y = -5, Alpha = 255, Rotation = 0, AddRGB = 10 },
                new(kf3) { Width = cornerWidth, Height = 30, X = -cornerWidth / 2, Y = -5, Alpha = 255, Rotation = 0, AddRGB = 0 })
                { Ease = SinInOut, Label = "Expand" },
            new(Bar,
                new(0) { ScaleX = 12 / bgWidth * flipFactor, Alpha = 0 },
                new(kf1) { ScaleX = 12 / bgWidth * flipFactor, Alpha = 0 },
                new(kf2) { ScaleX = 12 / bgWidth * flipFactor, Alpha = 0 },
                new(kf3) { ScaleX = flipFactor, Alpha = 255 })
                { Ease = SinInOut, Label = "Expand" },
            new(MidMarkers,
                new(0) { Alpha = 0, X = -4 },
                new(kf2) { Alpha = 0, X = -4 },
                new(kf3) { Alpha = 255, X = MidMarkerX() })
                { Ease = SinInOut, Label = "Expand" },
            new(LabelTextNode,
                Hidden[0],
                Visible[kf2])
                { Ease = SinInOut, Label = "Expand" },
            new(NumTextNode,
                Hidden[0],
                Visible[kf2])
                { Ease = SinInOut, Label = "Expand" }
        };

    }

    #endregion

    #region UpdateFuncs

    public override void OnDecreaseToMin()
    {
        TickMark.SetAlpha(0);
        if (Config.HideEmpty) CollapseBar(250, 350, 450);
    }

    public override void OnIncreaseFromMin()
    {
        TickMark.SetAlpha(255);
        if (Config.HideEmpty) ExpandBar(100, 170, 400);
    }

    public override void OnFirstRun(float prog)
    {
        base.OnFirstRun(prog);
        TickMark.SetAlpha(prog > 0);
        if (prog <= 0 && Config.HideEmpty) CollapseBar(0, 0, 0);
    }

    protected override void StartMilestoneAnim()
    {
        Animator -= "BarPulse";

        var red = Config.BaseColor == 0;
        var pulse1 = red ? Config.Pulse1Red + new AddRGB(-57, 115, 96) : Config.Pulse1Teal + new AddRGB(128, -74, -71);
        var pulse2 = red ? Config.Pulse2Red + new AddRGB(-57, 115, 96) : Config.Pulse2Teal + new AddRGB(128, -74, -71);

        Animator += new Tween[]
        {
            new(GlowFrame,
                new(0) { Alpha = 76 },
                Visible[500],
                new(1000) { Alpha = 76 })
                { Ease = SinInOut, Repeat = true, Label = "BarPulse" },
            new(Main,
                new(0) { AddRGB = pulse1 },
                new(500) { AddRGB = pulse2 },
                new(1000) { AddRGB = pulse1 })
                { Ease = SinInOut, Repeat = true, Label = "BarPulse" }
        };
    }

    protected override void StopMilestoneAnim()
    {
        Animator -= "BarPulse";
        Animator += new Tween(GlowFrame,
                                  new(0, GlowFrame),
                                  Hidden[500])
                                  { Ease = SinInOut, Label = "BarPulse" };

        var red = Config.BaseColor == 0;
        Main.SetAddRGB(red ? Config.MainColorRed + new AddRGB(-57, 115, 96) : Config.MainColorTeal + new AddRGB(128, -74, -71));
    }

    public override void PlaceTickMark(float prog) => TickMark.SetX(Main.Width + prog + 1);

    public override void PostUpdate(float prog, float prevProg)
    {
        if (Tracker.CurrentData.HasLabelOverride) LabelTextNode.SetLabelText(Tracker.CurrentData.LabelOverride ?? " ");
    }

    #endregion

    #region Configs

    public sealed class SoulBarConfig : GaugeBarWidgetConfig
    {
        public Vector2 Position;
        public float Scale = 1;
        public float Width = 180;
        public float Angle;

        public int BaseColor;
        public AddRGB BGColor = new(0, 0, 0, 250);
        public ColorRGB FrameColor = new(100, 100, 100);

        public AddRGB MainColorRed = new(57, -115, -96);
        public AddRGB GainColorRed = new(200, 80, 150);
        public AddRGB DrainColorRed = new(-50, -50, 20);
        public AddRGB TickRed = new(140, -133, -67);
        public AddRGB GlowRed = new(150, -50, -30);

        public AddRGB Pulse1Red = new(57, -115, -96);
        public AddRGB Pulse2Red = new(57, -115, -96);

        public AddRGB MainColorTeal = new(-128, 74, 71);
        public AddRGB GainColorTeal = new(0, 150, 150);
        public AddRGB DrainColorTeal = new(-50, -50, 20);
        public AddRGB TickTeal = new(-200, -50, 200);
        public AddRGB GlowTeal = new(-50, 50, 80);
        public AddRGB Pulse1Teal = new(-128, 74, 71);
        public AddRGB Pulse2Teal = new(-128, 74, 71);

        public bool Mirror;

        public LabelTextProps LabelTextProps = new(string.Empty, false, new(0, 0), new(255), new(0), Jupiter, 18, Left);
        protected override NumTextProps NumTextDefault => new(enabled: true,
                                                              position: new(0, 0),
                                                              color: new(255),
                                                              edgeColor: "0x9D835BFF",
                                                              showBg: true,
                                                              bgColor: new(0),
                                                              font: MiedingerMed,
                                                              fontSize: 14,
                                                              align: Left,
                                                              invert: false);

        public SoulBarConfig(WidgetConfig widgetConfig) : base(widgetConfig.SoulBarCfg)
        {
            var config = widgetConfig.SoulBarCfg;
            if (config == null) return;

            Position = config.Position;
            Scale = config.Scale;
            Width = config.Width;
            Angle = config.Angle;

            BGColor = config.BGColor;
            FrameColor = config.FrameColor;

            BaseColor = config.BaseColor;

            MainColorTeal = config.MainColorTeal;
            GainColorTeal = config.GainColorTeal;
            DrainColorTeal = config.DrainColorTeal;
            GlowTeal = config.GlowTeal;
            Pulse1Teal = config.Pulse1Teal;
            Pulse2Teal = config.Pulse2Teal;

            MainColorRed = config.MainColorRed;
            GainColorRed = config.GainColorRed;
            DrainColorRed = config.DrainColorRed;
            GlowRed = config.GlowRed;
            Pulse1Red = config.Pulse1Red;
            Pulse2Red = config.Pulse2Red;

            Mirror = config.Mirror;

            LabelTextProps = config.LabelTextProps;
        }

        public SoulBarConfig() => MilestoneType = Above;
    }

    public override GaugeBarWidgetConfig GetConfig => Config;

    public SoulBarConfig Config;

    public override void InitConfigs()
    {
        Config = new(Tracker.WidgetConfig);
        if (Tracker.WidgetConfig.SoulBarCfg == null)
        {
            Config.MilestoneType = Above;
            if (Tracker.RefType == RefType.Action)
            {
                Config.Invert = true;
            }
        }
    }

    public override void ResetConfigs() => Config = new();

    public override void ApplyConfigs()
    {
        WidgetContainer.SetPos(Config.Position)
                  .SetOrigin(0, 16f)
                  .SetScale(Config.Scale)
                  .SetRotation(Config.Angle, true);

        NumTextNode.SetRotation(-Config.Angle, true);

        var bgWidth = Config.Width + 2;
        var frameWidth = Config.Width + 10;
        var cornerWidth = Config.Width + 20;

        Frame.SetWidth(frameWidth).SetX(-frameWidth / 2).SetMultiply(Config.FrameColor);
        Corners.SetWidth(cornerWidth).SetX(-cornerWidth / 2).SetMultiply(Config.FrameColor);
        Bar.SetWidth(bgWidth).SetX(-bgWidth / 2).SetOrigin(bgWidth / 2, 0).SetScaleX(Config.Mirror ? -1 : 1);
        Backdrop.SetWidth(bgWidth);

        MidMarkers.SetX(MidMarkerX())
                  .SetVis(Config.MilestoneType != MilestoneType.None && Config.Milestone is >= 0.05f and <= 0.95f)
                  .SetAlpha(Config.HideEmpty == false || Tracker.CurrentData.GaugeValue > 0);

        var red = Config.BaseColor == 0;
        GlowFrame.SetWidth(frameWidth).SetAddRGB(red ? Config.GlowRed : Config.GlowTeal);

        Drain.SetAddRGB(red ? Config.DrainColorRed : Config.DrainColorTeal).SetWidth(0)
             .DefineTimeline(BarTimeline);
        Gain.SetAddRGB(red ? Config.GainColorRed : Config.GainColorTeal).SetWidth(0)
            .DefineTimeline(BarTimeline);
        Main.SetAddRGB(red ? Config.MainColorRed + new AddRGB(-57, 115, 96) : Config.MainColorTeal + new AddRGB(128, -74, -71))
            .SetPartId(red ? 8 : 9)
            .DefineTimeline(BarTimeline)
            .SetProgress(CalcProg());

        TickGradient.SetAddRGB(red ? Config.TickRed : Config.TickTeal);
        TickLine.SetAddRGB(red ? Config.TickRed : Config.TickTeal);
        TickDot.SetAddRGB(red ? Config.TickRed : Config.TickTeal);

        LabelTextNode.ApplyProps(Config.LabelTextProps, new(-Config.Width / 2,-7f))
                     .SetWidth(Config.Width);

        PartsLists[0].AtkUldPartsList->Parts[8].Width = (ushort)Clamp(Config.Width, 1, 180);
        PartsLists[0].AtkUldPartsList->Parts[8].U = (ushort)Clamp(182 - Config.Width, 2, 181);
        PartsLists[0].AtkUldPartsList->Parts[9].Width = (ushort)Clamp(Config.Width, 1, 180);
        PartsLists[0].AtkUldPartsList->Parts[9].U = (ushort)Clamp(182 - Config.Width, 2, 181);

        NumTextNode.ApplyProps(Config.NumTextProps, new Vector2((Config.Width / 2) + 59, 10.5f));

        HandleMilestone(CalcProg(), true);
    }

    public float MidMarkerX() => (((Config.Mirror ? 1 - Config.Milestone : Config.Milestone) - 0.5f) * Config.Width) - 4;

    public override void DrawUI(ref WidgetConfig widgetConfig, ref UpdateFlags update)
    {
        Heading("Layout");

        PositionControls("Position", ref Config.Position, ref update);
        ScaleControls("Scale", ref Config.Scale, ref update);
        FloatControls("Width", ref Config.Width, 28, 180, 1, ref update);
        AngleControls("Angle", ref Config.Angle, ref update);
        RadioIcons("Fill Direction", ref Config.Mirror, new() { false, true }, ArrowIcons, ref update);

        Heading("Colors");

        ColorPickerRGB("Frame Tint", ref Config.FrameColor, ref update);
        RadioControls("Base Texture", ref Config.BaseColor, new() { 0, 1 }, new() { "Red", "Teal" }, ref update, true);

        if (Config.BaseColor == 0)
        {
            ColorPickerRGBA("Main Bar", ref Config.MainColorRed, ref update);
            ColorPickerRGBA("Gain", ref Config.GainColorRed, ref update);
            ColorPickerRGBA("Drain", ref Config.DrainColorRed, ref update);
            ColorPickerRGBA("Tick", ref Config.TickRed, ref update);
            if (Config.MilestoneType > 0)
            {
                ColorPickerRGBA("Pulse Colors##Pulse1", ref Config.Pulse1Red, ref update);
                ColorPickerRGBA(" ##Pulse2", ref Config.Pulse2Red, ref update);
                ColorPickerRGBA(" ##Pulse3", ref Config.GlowRed, ref update);
            }
        }
        else
        {
            ColorPickerRGBA("Main Bar", ref Config.MainColorTeal, ref update);
            ColorPickerRGBA("Gain", ref Config.GainColorTeal, ref update);
            ColorPickerRGBA("Drain", ref Config.DrainColorTeal, ref update);
            ColorPickerRGBA("Tick", ref Config.TickTeal, ref update);
            if (Config.MilestoneType > 0)
            {
                ColorPickerRGBA("Pulse Colors##Pulse1", ref Config.Pulse1Teal, ref update);
                ColorPickerRGBA(" ##Pulse2", ref Config.Pulse2Teal, ref update);
                ColorPickerRGBA(" ##Pulse3", ref Config.GlowTeal, ref update);
            }
        }

        Heading("Behavior");

        SplitChargeControls(ref Config.SplitCharges, Tracker.RefType, Tracker.CurrentData.MaxCount, ref update);
        ToggleControls("Invert Fill", ref Config.Invert, ref update);
        if (ToggleControls("Collapse Empty", ref Config.HideEmpty, ref update)) CollapseCheck(Config.HideEmpty);

        MilestoneControls("Pulse", ref Config.MilestoneType, ref Config.Milestone, ref update);

        NumTextControls($"{Tracker.TermGauge} Text", ref Config.NumTextProps, ref update);
        LabelTextControls("Label Text", ref Config.LabelTextProps, Tracker.DisplayName, ref update);

        if (update.HasFlag(UpdateFlags.Save)) ApplyConfigs();
        widgetConfig.SoulBarCfg = Config;
    }

    private List<FontAwesomeIcon> ArrowIcons =>
        Abs(Config.Angle) > 135 ? new() { ArrowLeft, ArrowRight } :
        Config.Angle > 45 ? new() { ArrowDown, ArrowUp } :
        Config.Angle < -45 ? new() { ArrowUp, ArrowDown } :
        new() { ArrowRight, ArrowLeft };

    private void CollapseCheck(bool collapse)
    {
        if (Tracker.CurrentData.GaugeValue == 0 || (Config.Invert && Abs(Tracker.CurrentData.GaugeValue - Tracker.CurrentData.MaxGauge) < 0.01f))
        {
            if (collapse) CollapseBar(250, 350, 450);
            else ExpandBar(100, 170, 400);
        }
    }

    #endregion
}

public partial class WidgetConfig
{
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] public SoulBarConfig? SoulBarCfg { get; set; }
}
