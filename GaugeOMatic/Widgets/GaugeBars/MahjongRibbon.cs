using CustomNodes;
using GaugeOMatic.CustomNodes.Animation;
using GaugeOMatic.Trackers;
using GaugeOMatic.Windows;
using Newtonsoft.Json;
using System.Numerics;
using static CustomNodes.CustomNodeManager;
using static FFXIVClientStructs.FFXIV.Component.GUI.AlignmentType;
using static FFXIVClientStructs.FFXIV.Component.GUI.FontType;
using static GaugeOMatic.CustomNodes.Animation.KeyFrame;
using static GaugeOMatic.CustomNodes.Animation.Tween.EaseType;
using static GaugeOMatic.Utility.Color;
using static GaugeOMatic.Widgets.GaugeBarWidgetConfig;
using static GaugeOMatic.Widgets.LabelTextProps;
using static GaugeOMatic.Widgets.MahjongRibbon;
using static GaugeOMatic.Widgets.NumTextProps;
using static GaugeOMatic.Widgets.WidgetTags;
using static GaugeOMatic.Widgets.WidgetUI;
using static System.Math;

#pragma warning disable CS8618

namespace GaugeOMatic.Widgets;

public sealed unsafe class MahjongRibbon : GaugeBarWidget
{
    public MahjongRibbon(Tracker tracker) : base(tracker) { }

    public override WidgetInfo WidgetInfo => GetWidgetInfo;

    public static WidgetInfo GetWidgetInfo => new()
    {
        DisplayName = "Mahjong Ribbon",
        Author = "ItsBexy",
        Description = "A gauge bar based on parts of the Mahjong UI.",
        WidgetTags = GaugeBar
    };

    public override CustomPartsList[] PartsLists { get; } = {
        new ("ui/uld/emjintroparts08.tex",
             new Vector4(0, 17, 720, 32),
             new Vector4(0, 17, 720, 32) ),
        new ("ui/uld/emjintroparts03.tex", new Vector4(0, 0, 64, 64)),
        new ("ui/uld/JobHudNIN0.tex", new Vector4(256, 152, 20, 88))
    };

    #region Nodes

    public CustomNode Bar;
    public CustomNode Frame;
    public LabelTextNode LabelTextNode;
    public CustomNode Contents;

    public CustomNode Backdrop;

    public CustomNode TickWrapper;
    public CustomNode Tick;

    public override CustomNode BuildContainer()
    {
        Bar = BuildBar();
        Frame = NineGridFromPart(1, 0, 28, 28, 28, 28).SetSize(0, 32);
        LabelTextNode = new(Config.LabelText.Text, Tracker.DisplayName);
        NumTextNode = new();

        Tick = ImageNodeFromPart(2, 0).SetAlpha(true).SetOrigin(20, 44).SetPos(0, -27.5f).SetScale(0.8f, 0.55f).SetImageFlag(32);
        TickWrapper = new CustomNode(CreateResNode(), Tick).SetX(Config.Width/-2);

        Animator += new Tween(Tick,
                              new(0) { ScaleY = 0.54f, ScaleX = 0.81f },
                              new(300) { ScaleY = 0.56f, ScaleX = 0.79f },
                              new(600) { ScaleY = 0.54f, ScaleX = 0.81f })
                              { Repeat = true, Ease = SinInOut };

        Contents = new(CreateResNode(), Bar, Frame, TickWrapper);

        return new CustomNode(CreateResNode(), Contents, LabelTextNode, NumTextNode).SetOrigin(0, 32);
    }

    private CustomNode BuildBar()
    {
        Backdrop = ImageNodeFromPart(0, 1).SetSize(2880, 50).SetImageWrap(3).SetSize(0, 32);
        Drain = ImageNodeFromPart(0, 0).SetSize(2880, 50).SetImageWrap(3).SetSize(0, 32).SetOrigin(0, 16);
        Gain = ImageNodeFromPart(0, 0).SetSize(2880, 50).SetImageWrap(3).SetSize(0, 32).SetOrigin(0, 16);
        Main = ImageNodeFromPart(0, 0).SetSize(2880, 50).SetImageWrap(3).SetSize(0, 32).SetOrigin(0, 16);

        return new CustomNode(CreateResNode(), Backdrop, Drain, Gain, Main).SetPos(0, 0).SetSize(Config.Width, 32);
    }

    #endregion

    #region Animations

    public KeyFrame[] BarTimeline => new KeyFrame[] { new(0) { Width = 0 }, new(1) { Width = Config.Width }};

    public override void HideBar(bool instant = false)
    {
        var halfWidth = Config.Width / 2;
        var kf = instant ? new[] { 0, 0, 0, 0 } : new[] { 0, 350, 450,200 };

        Animator += new Tween[]
        {
            new(Frame,
                new(kf[0]) { X = -halfWidth, Width = Config.Width, Height = 32, AddRGB = 0, Alpha = 255, Y = 0 },
                new(kf[1]) { X = 0, Width = 0, Height = 32, AddRGB = 50, Alpha = 255, Y = 0 },
                new(kf[2]) { X = 0, Width = 0, Height = 32, AddRGB = 255, Alpha = 0, Y = 16 })
                { Ease = SinInOut },
            new(Bar,
                new(kf[0]) { X = -halfWidth, Alpha = 255, Y = 0, ScaleX = 1, Height = 32 },
                new((int)(kf[1]*0.9f)) { X = 0 , Alpha = 128, Y = 0, ScaleX = 0, Height = 32 },
                new(kf[2]) { X = 0 , Alpha = 0, Y = 16, ScaleX = 0, Height = 0 })
                { Ease = SinInOut },
            new(TickWrapper,
                new(kf[0]) { Alpha = 255, X = -halfWidth, ScaleX = 1 },
                new(kf[1]) { Alpha = 128, X = 0, ScaleX = 0 },
                new(kf[2]) { Alpha = 0, X = 0, ScaleX = 0 }),
            new(LabelTextNode, Visible[kf[0]], Hidden[kf[1]]),
            new(NumTextNode, Visible[kf[0]], Hidden[kf[2]]),
            new(Tick, Visible[kf[0]], Hidden[kf[3]])
        };

        StopBackdropTween();
    }

    public override void RevealBar(bool instant = false)
    {
        var halfWidth = Config.Width / 2;
        var kf = instant ? new[] { 0, 0, 0, 0 } : new[] { 0, 100, 350, 200 };

        Animator += new Tween[]
        {
            new(Frame,
                new(kf[0]) { Alpha = 0, Y = 16, X = -16, Width = 32, Height = 0, AddRGB = new(200) },
                new(kf[1]) { Alpha = 255, Y = 0, X = -16, Width = 32, Height = 32, AddRGB = new(255) },
                new(kf[2]) { Alpha = 255, Y = 0, X = -halfWidth, Height = 32, Width = Config.Width, AddRGB = 0 })
                { Ease = SinInOut },

            new(Bar,
                new(kf[0]) { Alpha = 0, Y = 16, X = -16, ScaleY = 0, ScaleX = 32f/Config.Width, Height = 0 },
                new(kf[1]) { Alpha = 255, Y = 0, X = -16 , ScaleY = 1, ScaleX = 32f / Config.Width, Height = 32 },
                new(kf[2]) { Alpha = 255, Y = 0, X = -halfWidth, ScaleX = 1, Height = 32 })
                { Ease = SinInOut },

            new(Backdrop,
                new(kf[0]) { X = 0, Height = 0, Alpha = 255 },
                new(kf[1]) { X = 0, Height = 32, Alpha = Config.Background.A },
                new(kf[2]) { X = 0, Alpha = Config.Background.A })
                { Ease = SinInOut },

            new(TickWrapper,
                new(kf[0]) { Alpha = 0, X = -16, ScaleX = 32f / Config.Width },
                new(kf[1]) { Alpha = 0, X = -16, ScaleX = 32f / Config.Width },
                new(kf[2]) { Alpha = 255, X = -halfWidth, ScaleX = 1 }),

            new(LabelTextNode, Hidden[kf[0]], Hidden[kf[1]], Visible[kf[2]]),

            new(NumTextNode, Hidden[kf[0]], Hidden[kf[1]], Visible[kf[2]]),

            new(Tick, Hidden[kf[0]], Visible[kf[3]])
        };

        StartBackdropTween();
    }

    #endregion

    #region UpdateFuncs

    public override void OnDecreaseToMin() { if (Config.HideEmpty) HideBar(); }
    public override void OnIncreaseFromMin() { if (Config.HideEmpty) RevealBar(); }

    public override void OnIncreaseToMax() { if (Config.HideFull) HideBar(); }
    public override void OnDecreaseFromMax() { if (Config.HideFull) RevealBar(); }

    public override void PlaceTickMark(float prog)
    {
        var wid = Max(Drain.Width, Main.Width);
        Tick.SetPos(wid - 20, -27.5f)
            .SetAlpha(prog switch {
                < 0.025f => (byte)(prog * 10200F),
                > 0.975f => (byte)((1 - prog) * 10200F),
                _ => 255
            });
    }

    public override void OnFirstRun(float prog)
    {
        base.OnFirstRun(prog);
        Tick.SetAlpha(prog > 0);
        if (prog > 0) { StartBackdropTween(); }
    }

    private void StopBackdropTween()
    {
        Animator -= "ScrollAni";
        Animator += new Tween[]
        {
            new(Main,
                new(0, Main),
                new(200) { PartCoords = new(720, 17, 720, 32) })
                { Ease = SinInOut, Label = "ScrollAni" },
            new(Backdrop,
                new(0, Backdrop),
                new(200) { PartCoords = new(0, 17, 720, 32) })
                { Ease = SinInOut, Label = "ScrollAni" }
        };
    }

    private void StartBackdropTween()
    {
        Animator -= "ScrollAni";
        Animator += new Tween[]
        {
            new(Main,
                new(0) { PartCoords = new(0, 17, 720, 32) },
                new(5000) { PartCoords = new(720, 17, 720, 32) })
                { Repeat = true, Label = "ScrollAni" },
            new(Backdrop,
                new(0) { PartCoords = new(720, 17, 720, 32) },
                new(10000) { PartCoords = new(0, 17, 720, 32) })
                { Repeat = true, Label = "ScrollAni" }
        };
    }

    public override void PostUpdate(float prog)
    {
        if (Tracker.CurrentData.HasLabelOverride) LabelTextNode.SetLabelText(Tracker.CurrentData.LabelOverride ?? " ");
    }

    #endregion

    #region Configs

    public sealed class MahjongRibbonConfig : GaugeBarWidgetConfig
    {
        public Vector2 Position = new(0, -27);
        public float Scale = 1;
        public float Width = 144;
        public float Angle;

        public AddRGB Background = "0x777777FF";
        public AddRGB MainColor = "0x987B7BFF";
        public AddRGB GainColor = "0xDE56B2A0";
        public AddRGB DrainColor = "0x8069B2A0";
        public AddRGB TickColor = "0xD36E27FF";

        public LabelTextProps LabelText = new(string.Empty, false, new(0, 0), new(255), 0x8E6A0CFF, Jupiter, 20, Left);
        protected override NumTextProps NumTextDefault => new(enabled:   true,
                                                              position:  new(0),
                                                              color:     new(255),
                                                              edgeColor: new(0),
                                                              showBg:    false,
                                                              bgColor:   new(0),
                                                              font:      MiedingerMed,
                                                              fontSize:  18,
                                                              align:     Center,
                                                              invert:    false);

        public MahjongRibbonConfig(WidgetConfig widgetConfig) : base(widgetConfig.MahjongRibbonCfg)
        {
            var config = widgetConfig.MahjongRibbonCfg;

            if (config == null) return;

            Position = config.Position;
            Scale = config.Scale;
            Width = config.Width;
            Angle = config.Angle;

            Background = config.Background;
            MainColor = config.MainColor;
            GainColor = config.GainColor;
            DrainColor = config.DrainColor;
            LabelText = config.LabelText;
        }

        public MahjongRibbonConfig() { }
    }

    public override GaugeBarWidgetConfig GetConfig => Config;

    public MahjongRibbonConfig Config;

    public override void InitConfigs()
    {
        Config = new(Tracker.WidgetConfig);
        if (Tracker.WidgetConfig.MahjongRibbonCfg == null && Tracker.RefType == RefType.Action) { Config.Invert = true; }
    }

    public override void ResetConfigs() => Config = new();

    public Vector2 PosAdjust = new(0, -32);
    public override void ApplyConfigs()
    {
        WidgetContainer.SetPos(Config.Position + PosAdjust)
                  .SetWidth(Config.Width)
                  .SetScale(Config.Scale);

        Contents.SetRotation(Config.Angle, true).SetOrigin(0, 16);

        Frame.SetPos(Config.Width / -2, 0).SetWidth(Config.Width);
        Bar.SetPos(Config.Width / -2, 0).SetWidth(Config.Width);
        Backdrop.SetWidth(Config.Width);

        Main.SetAddRGB(Config.MainColor, true)
            .SetOrigin(Config.Width / 2f, 16)
            .DefineTimeline(BarTimeline)
            .SetProgress(CalcProg());

        Backdrop.SetAddRGB(Config.Background, true);

        Drain.SetAddRGB(Config.DrainColor, true)
             .SetOrigin(Config.Width/2f, 16)
             .DefineTimeline(BarTimeline)
             .SetWidth(0);

        Gain.SetAddRGB(Config.GainColor, true)
            .SetOrigin(Config.Width / 2f, 16)
            .DefineTimeline(BarTimeline)
            .SetWidth(0);

        TickWrapper.SetX(Config.Width / -2);
        Tick.SetAddRGB(Config.TickColor);

        LabelTextNode.ApplyProps(Config.LabelText, new Vector2(Config.Width / -2, -15));

        NumTextNode.ApplyProps(Config.NumTextProps, new(-1, 17));
        NumTextNode.SetOrigin((NumTextNode.Width / 2) + 1, (NumTextNode.Height / 2) - 1);

    }

    public override void DrawUI(ref WidgetConfig widgetConfig, ref UpdateFlags update)
    {
        Heading("Layout");
        PositionControls("Position", ref Config.Position, ref update);
        ScaleControls("Scale", ref Config.Scale, ref update);
        FloatControls("Width", ref Config.Width, 64, 1440, 1, ref update);
        AngleControls("Angle", ref Config.Angle, ref update);

        Heading("Colors");
        ColorPickerRGBA("Backdrop", ref Config.Background, ref update);
        ColorPickerRGBA("Main Bar", ref Config.MainColor, ref update);
        ColorPickerRGBA("Gain", ref Config.GainColor, ref update);
        ColorPickerRGBA("Drain", ref Config.DrainColor, ref update);
        ColorPickerRGB("Tick Color", ref Config.TickColor, ref update);

        Heading("Behavior");

        SplitChargeControls(ref Config.SplitCharges, Tracker.RefType, Tracker.CurrentData.MaxCount, ref update);
        ToggleControls("Invert Fill", ref Config.Invert, ref update);
        HideControls("Collapse Empty", "Collapse Full", ref Config.HideEmpty, ref Config.HideFull, EmptyCheck, FullCheck, ref update);

        NumTextControls($"{Tracker.TermGauge} Text", ref Config.NumTextProps, ref update);
        LabelTextControls("Label Text", ref Config.LabelText, Tracker.DisplayName, ref update);

        if (update.HasFlag(UpdateFlags.Save)) ApplyConfigs();
        widgetConfig.MahjongRibbonCfg = Config;
    }

    #endregion
}

public partial class WidgetConfig
{
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] public MahjongRibbonConfig? MahjongRibbonCfg { get; set; }
}
