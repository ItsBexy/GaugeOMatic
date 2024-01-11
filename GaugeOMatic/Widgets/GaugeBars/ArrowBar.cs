using GaugeOMatic.Trackers;
using GaugeOMatic.Windows;
using Newtonsoft.Json;
using System;
using System.Numerics;
using static CustomNodes.CustomNodeManager;
using static CustomNodes.CustomNodeManager.Tween;
using static FFXIVClientStructs.FFXIV.Component.GUI.AlignmentType;
using static FFXIVClientStructs.FFXIV.Component.GUI.FontType;
using static GaugeOMatic.Utility.Color;
using static GaugeOMatic.Widgets.ArrowBar;
using static GaugeOMatic.Widgets.GaugeBarWidget.DrainGainType;
using static GaugeOMatic.Widgets.LabelTextProps;
using static GaugeOMatic.Widgets.NumTextProps;
using static GaugeOMatic.Widgets.WidgetTags;
using static GaugeOMatic.Widgets.WidgetUI;

namespace GaugeOMatic.Widgets;

public sealed unsafe class ArrowBar : GaugeBarWidget
{
    public override WidgetInfo WidgetInfo => GetWidgetInfo;

    public static WidgetInfo GetWidgetInfo => new() 
    {
        DisplayName = "Arrow Bar",
        Author = "ItsBexy",
        Description = "A Gauge Bar shaped like an arrow.",
        WidgetTags = GaugeBar
    };

    public override CustomPartsList[] PartsLists { get; } = {
        new ("ui/uld/JobHudBRD0.tex",
             new(1,366,188,34),
             new(1,419,151,6),
             new(59,427,93,6),
             new(215,211,15,43),
             new(280,150,21,44)) 
    };

    #region Nodes

    public CustomNode Bar;
    public CustomNode Frame;
    public CustomNode BarFrame;
    public CustomNode Tick;
    public CustomNode LabelTextNode;
    public override CustomNode NumTextNode { get; set; }

    public CustomNode Backdrop;
    public override CustomNode Drain { get; set; }
    public override CustomNode Gain { get; set; }
    public override CustomNode Main { get; set; }

    public override CustomNode BuildRoot()
    {
        Bar = BuildBar().SetPos(19, 13);
        Frame = NineGridFromPart(0, 0, 13, 38, 13, 56).SetSize(188, 34);
        BarFrame = new CustomNode(CreateResNode(), Bar, Frame).SetOrigin(0,17);
        LabelTextNode = CreateLabelTextNode(Config.LabelText.Text, Tracker.DisplayName);
        NumTextNode = CreateNumTextNode();

        AnimateTickmark();
        AnimateBarPulse();

        return new CustomNode(CreateResNode(), BarFrame, LabelTextNode, NumTextNode).SetOrigin(28, 12);
    }

    private CustomNode BuildBar()
    {
        Tick = ImageNodeFromPart(0, 3).SetImageFlag(32).SetOrigin(7.5f, 21.5f).SetScale(0.5f, 0.4f).SetY(-20f);
        Backdrop = NineGridFromPart(0, 1).SetNineGridOffset(new(0, 2, 0, 2)).SetSize(150, 8);
        Drain = NineGridFromPart(0, 2, 0, 2, 0, 2).SetSize(0, 8).SetPos(0, 1);
        Gain = NineGridFromPart(0, 2, 0, 2, 0, 2).SetSize(0, 8).SetPos(0, 1);
        Main = NineGridFromPart(0, 2, 0, 2, 0, 2).SetSize(0, 8).SetPos(0, 1);

        return new CustomNode(CreateResNode(), Backdrop, Drain, Gain, Main,Tick);
    }

    #endregion

    #region Animations

    public void CollapseBar(int kf1, int kf2)
    {
        var halfWidth = Config.Width / 2;

        Tweens.Add(new(Frame,
                       new(0) { X = -halfWidth - 19, Width = Config.Width + 38, AddRGB = 0, Alpha = 255, Height = 34 },
                       new(kf1) { X = halfWidth - 19, Width = 68, AddRGB = 50, Alpha = 255, Height = 34 },
                       new(kf2) { X = halfWidth - 19, Width = 68, AddRGB = 255, Alpha = 0, Height = 26 })
                       { Ease = Eases.SinInOut });

        Bar.SetOrigin(Config.Width, 4);

        Tweens.Add(new(Bar,
                       new(0) {X=-halfWidth, Alpha = 255, ScaleX=1,ScaleY=1 },
                       new(kf1) { X = -halfWidth+19, Alpha = 255, ScaleX = 30f/Config.Width, ScaleY = 1 },
                       new(kf2) { X = -halfWidth + 19, Alpha = 128, ScaleX = 30f / Config.Width, ScaleY = 0 })
                       { Ease = Eases.SinInOut });

        Tweens.Add(new(BarFrame,
                       new(0) { Y = 0},
                       new(kf1) { Y = 0 },
                       new(kf2) { Y = 4 })
                       { Ease = Eases.SinInOut });

        Tweens.Add(new(LabelTextNode, new(0) { Alpha = 255 }, new(kf1) { Alpha = 0 }));
        Tweens.Add(new(NumTextNode, new(0) { Alpha = 255 }, new(kf2) { Alpha = 0 }));
    }

    public void ExpandBar(int kf1, int kf2)
    {
        var halfWidth = Config.Width / 2;

        Tweens.Add(new(Frame,
                       new(0) { Alpha = 0, X = -halfWidth - 69, Width = 68, AddRGB = new(200), Height = 26 },
                       new(kf1) { Alpha = 255, X = -halfWidth - 69, Width = 68, AddRGB = new(255), Height = 34 },
                       new(kf2) { Alpha = 255, X = -halfWidth - 19, Width = Config.Width + 38, AddRGB = 0, Height = 34 })
                       { Ease = Eases.SinInOut });

        Bar.SetOrigin(0, 4);

        Tweens.Add(new(Bar,
                       new(0) { X = -halfWidth - 50, Alpha = 0, ScaleX = 30f / Config.Width, ScaleY = 0 },
                       new(kf1) { X = -halfWidth - 50, Alpha = 255, ScaleX = 30f / Config.Width, ScaleY = 1 },
                       new(kf2) { X = -halfWidth, Alpha = 255, ScaleX=1, ScaleY = 1 })
                       { Ease = Eases.SinInOut });

        Tweens.Add(new(BarFrame,
                       new(0) { Y = 4 },
                       new(kf1) { Y = 0 },
                       new(kf2) { Y = 0 })
                       { Ease = Eases.SinInOut });

        Tweens.Add(new(LabelTextNode, new(0) { Alpha = 0 }, new(kf1) { Alpha = 0 }, new(kf2) { Alpha = 255 }));
        Tweens.Add(new(NumTextNode, new(0) { Alpha = 0 }, new(kf1) { Alpha = 0 }, new(kf2) { Alpha = 255 }));
    }

    private void AnimateBarPulse()
    {
        Tweens.Add(new(Main,
                       new(0) { AddRGB = Config.MainColor + new AddRGB(-10) },
                       new(600) { AddRGB = Config.MainColor + new AddRGB(10) },
                       new(1200) { AddRGB = Config.MainColor + new AddRGB(-10) })
                       { Ease = Eases.SinInOut, Repeat = true });
    }

    private void AnimateTickmark()
    {
        Tweens.Add(new(Tick,
                       new(0) { ScaleX = 0.4f, ScaleY = 0.2f },
                       new(300) { ScaleX = 0.6f, ScaleY = 0.2f },
                       new(600) { ScaleX = 0.4f, ScaleY = 0.2f })
                       { Ease = Eases.SinInOut, Repeat = true });
    }

    #endregion

    #region UpdateFuncs

    public override string? SharedEventGroup => null;

    public override void OnDecreaseToMin(float prog, float prevProg) { if (Config.Collapse) CollapseBar(250, 350); }
    public override void OnIncreaseFromMin(float prog, float prevProg) { if (Config.Collapse) ExpandBar(100, 350); }

    public override void OnFirstRun(float prog)
    {
        var curWid = CalcBarProperty(prog);
        Main.SetWidth(curWid);
        Gain.SetWidth(curWid);
        Drain.SetWidth(curWid);
        if (prog == 0 && Config.Collapse) CollapseBar(0, 0);
    }

    public override void PlaceTickMark(float prog)
    {
        Tick.SetPos(Math.Clamp((float)Main.Node->Width-8, 0, Backdrop.Node->Width), -18f)
            .SetVis(prog > 0);
    }

    public override DrainGainType DGType => Width;
    public override float CalcBarProperty(float prog) => Math.Clamp(prog, 0f, 1f) * Config.Width;

    #endregion

    #region Configs

    public sealed class ArrowBarConfig : GaugeBarWidgetConfig
    {
        public Vector2 Position = new(0, -27);
        public float Scale = 1;
        public float Width = 150;
        public float Angle;

        public AddRGB Background = new(0, 0, 0, 229);
        public ColorRGB FrameColor = new(100, 100, 100);
        public AddRGB MainColor = new(-136, -17, 10);
        public AddRGB GainColor = new(5, 155, 93);
        public AddRGB DrainColor = new(-107, -159, -111);
      
        public bool Collapse;
        public LabelTextProps LabelText = new(string.Empty, false, new(0, 32), new(255), 0x8E6A0CFF, Jupiter, 16, Left);
        protected override NumTextProps NumTextDefault => new(true, new(0, 17.5f), new(255), new(0), MiedingerMed, 18, Left, false);

        public ArrowBarConfig(WidgetConfig widgetConfig)
        {
            NumTextProps = NumTextDefault;
            var config = widgetConfig.ArrowBarCfg;

            if (config != null)
            {
                Position = config.Position;
                Scale = config.Scale;
                Width = config.Width;
                Angle = config.Angle;

                Background = config.Background;
                FrameColor = config.FrameColor;
                MainColor = config.MainColor;
                GainColor = config.GainColor;
                DrainColor = config.DrainColor;

                AnimationLength = config.AnimationLength;
                Invert = config.Invert;
                Collapse = config.Collapse;

                NumTextProps = config.NumTextProps;
                LabelText = config.LabelText;
            }
        }

        public ArrowBarConfig()
        {
            NumTextProps = NumTextDefault;
        }
    }

    public override GaugeBarWidgetConfig GetConfig => Config;

    public ArrowBarConfig Config = null!;

    public override void InitConfigs()
    {
        Config = new(Tracker.WidgetConfig);
        if (Tracker.WidgetConfig.ArrowBarCfg == null && Tracker.RefType == RefType.Action) { Config.Invert = true; }
    }

    public override void ResetConfigs() => Config = new();

    public override void ApplyConfigs()
    {
        var frameWidth = Config.Width + 38;
        WidgetRoot.SetPos(Config.Position)
                  .SetScale(Config.Scale);

        BarFrame.SetRotation(Config.Angle * 0.01745329f)
                .SetScaleY(Math.Abs(Config.Angle) > 90 ? -1:1);

        Frame.SetPos(frameWidth / -2, 0)
             .SetWidth(frameWidth)
             .SetMultiply(Config.FrameColor);

        Bar.SetPos(Config.Width / -2, 13);

        Backdrop.SetWidth(Config.Width)
                .SetAddRGB(Config.Background, true);

        Config.LabelText.ApplyTo(LabelTextNode,new((frameWidth / -2f) + 38, 0));
        Config.NumTextProps.ApplyTo(NumTextNode, new(frameWidth / 2f,0));

        LabelTextNode.SetWidth(Math.Max(0, Config.Width - 40));

        Main.SetAddRGB(Config.MainColor)
            .SetWidth(CalcBarProperty(CalcProg()));

        Gain.SetAddRGB(Config.GainColor)
            .SetWidth(0);

        Drain.SetAddRGB(Config.DrainColor)
             .SetWidth(0);
    }

    public override void DrawUI(ref WidgetConfig widgetConfig, ref UpdateFlags update)
    {
        Heading("Layout");

        PositionControls("Position", ref Config.Position, ref update);
        ScaleControls("Scale", ref Config.Scale, ref update);
        FloatControls("Width", ref Config.Width, 30, 2000, 1, ref update);
        FloatControls("Angle", ref Config.Angle,-180,180,1, ref update);

        Heading("Colors");

        ColorPickerRGBA("Backdrop", ref Config.Background, ref update);
        ColorPickerRGB("Frame Tint", ref Config.FrameColor, ref update);
        ColorPickerRGBA("Main Bar", ref Config.MainColor, ref update);
        ColorPickerRGBA("Gain", ref Config.GainColor, ref update);
        ColorPickerRGBA("Drain", ref Config.DrainColor, ref update);

        Heading("Behavior");

        ToggleControls("Invert Fill", ref Config.Invert, ref update);
        if (ToggleControls("Collapse Empty", ref Config.Collapse, ref update)) CollapseCheck();
      //  IntControls("Animation Time", ref Config.AnimationLength, 0, 2000, 50, ref update);

        NumTextControls($"{Tracker.TermGauge} Text", ref Config.NumTextProps, ref update);
        LabelTextControls2("Label Text", ref Config.LabelText, Tracker.DisplayName, ref update);

        if (update.HasFlag(UpdateFlags.Save)) ApplyConfigs();
        widgetConfig.ArrowBarCfg = Config;
    }

    private void CollapseCheck()
    {
        if (Tracker.CurrentData.GaugeValue == 0 || (Config.Invert && Math.Abs(Tracker.CurrentData.GaugeValue - Tracker.CurrentData.MaxGauge) < 0.01f))
        {
            if (Config.Collapse) CollapseBar(250, 350);
            else ExpandBar(50, 150);
        }
    }

    #endregion

    public ArrowBar(Tracker tracker) : base(tracker) { }
}

public partial class WidgetConfig
{
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] public ArrowBarConfig? ArrowBarCfg { get; set; }
}
