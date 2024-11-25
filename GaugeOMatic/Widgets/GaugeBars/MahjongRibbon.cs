using CustomNodes;
using GaugeOMatic.CustomNodes.Animation;
using GaugeOMatic.Trackers;
using Newtonsoft.Json;
using System.ComponentModel;
using System.Numerics;
using GaugeOMatic.Widgets.Common;
using static CustomNodes.CustomNode;
using static CustomNodes.CustomNodeManager;
using static FFXIVClientStructs.FFXIV.Component.GUI.AlignmentType;
using static FFXIVClientStructs.FFXIV.Component.GUI.FontType;
using static GaugeOMatic.CustomNodes.Animation.KeyFrame;
using static GaugeOMatic.CustomNodes.Animation.Tween.EaseType;
using static GaugeOMatic.Utility.Color;
using static GaugeOMatic.Widgets.Common.LabelTextProps;
using static GaugeOMatic.Widgets.MahjongRibbon;
using static GaugeOMatic.Widgets.Common.NumTextProps;
using static GaugeOMatic.Widgets.WidgetTags;
using static GaugeOMatic.Widgets.Common.WidgetUI;
using static GaugeOMatic.Widgets.Common.WidgetUI.WidgetUiTab;
using static System.Math;

#pragma warning disable CS8618

namespace GaugeOMatic.Widgets;

[WidgetName("Mahjong Ribbon")]
[WidgetDescription("A gauge bar based on parts of the Mahjong UI.")]
[WidgetAuthor("ItsBexy")]
[WidgetTags(GaugeBar)]
public sealed unsafe class MahjongRibbon(Tracker tracker) : GaugeBarWidget(tracker)
{
    public override CustomPartsList[] PartsLists { get; } =
    [
        new ("ui/uld/emjintroparts08.tex",
             new Vector4(0, 17, 720, 32),
             new Vector4(0, 17, 720, 32) ),
        new ("ui/uld/emjintroparts03.tex", new Vector4(0, 0, 64, 64)),
        new ("ui/uld/JobHudNIN0.tex", new Vector4(256, 152, 20, 88))
    ];

    #region Nodes

    public CustomNode Bar;
    public CustomNode Frame;
    public LabelTextNode LabelTextNode;
    public CustomNode Contents;
    public CustomNode Backdrop;
    public CustomNode TickWrapper;
    public CustomNode Tick;

    public override Bounds GetBounds() => Frame;

    public override CustomNode BuildContainer()
    {
        Bar = BuildBar();
        Frame = NineGridFromPart(1, 0, 28, 28, 28, 28).SetSize(0, 32);
        LabelTextNode = new(Config.LabelText.Text, Tracker.DisplayAttr.Name);
        NumTextNode = new();

        Tick = ImageNodeFromPart(2, 0).SetAlpha(true).SetOrigin(20, 44).SetPos(0, -27.5f).SetScale(0.8f, 0.55f).SetImageFlag(32);
        TickWrapper = new CustomNode(CreateResNode(), Tick).SetX(Config.Width / -2);

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

    public KeyFrame[] BarTimeline => [new(0) { Width = 0 }, new(1) { Width = Config.Width }];

    public override void HideBar(bool instant = false)
    {
        base.HideBar(instant);
        var halfWidth = Config.Width / 2;
        var kf = instant ? [0, 0, 0, 0] : new[] { 0, 350, 450, 200 };

        Animator +=
        [
            new(Frame,
                new(kf[0]) { X = -halfWidth, Width = Config.Width, Height = 32, AddRGB = 0, Alpha = 255, Y = 0 },
                new(kf[1]) { X = 0, Width = 0, Height = 32, AddRGB = 50, Alpha = 255, Y = 0 },
                new(kf[2]) { X = 0, Width = 0, Height = 32, AddRGB = 255, Alpha = 0, Y = 16 })
                { Ease = SinInOut, Label = "ShowHide"},
            new(Bar,
                new(kf[0]) { X = -halfWidth, Alpha = 255, Y = 0, ScaleX = 1, Height = 32 },
                new((int)(kf[1]*0.9f)) { X = 0 , Alpha = 128, Y = 0, ScaleX = 0, Height = 32 },
                new(kf[2]) { X = 0 , Alpha = 0, Y = 16, ScaleX = 0, Height = 0 })
                { Ease = SinInOut, Label = "ShowHide" },
            new(TickWrapper,
                new(kf[0]) { Alpha = 255, X = -halfWidth, ScaleX = 1 },
                new(kf[1]) { Alpha = 128, X = 0, ScaleX = 0 },
                new(kf[2]) { Alpha = 0, X = 0, ScaleX = 0 }){ Label = "ShowHide" },
            new(LabelTextNode, Visible[kf[0]], Hidden[kf[1]]){ Label = "ShowHide" },
            new(NumTextNode, Visible[kf[0]], Hidden[kf[2]]){ Label = "ShowHide" },
            new(Tick, Visible[kf[0]], Hidden[kf[3]]){ Label = "ShowHide" }
        ];

        StopBackdropTween();
    }

    public override void RevealBar(bool instant = false)
    {
        base.RevealBar(instant);
        var halfWidth = Config.Width / 2;
        var kf = instant ? [0, 0, 0, 0] : new[] { 0, 100, 350, 200 };

        Animator +=
        [
            new(Frame,
                new(kf[0]) { Alpha = 0, Y = 16, X = -16, Width = 32, Height = 0, AddRGB = new(200) },
                new(kf[1]) { Alpha = 255, Y = 0, X = -16, Width = 32, Height = 32, AddRGB = new(255) },
                new(kf[2]) { Alpha = 255, Y = 0, X = -halfWidth, Height = 32, Width = Config.Width, AddRGB = 0 })
                { Ease = SinInOut, Label = "ShowHide" },

            new(Bar,
                new(kf[0]) { Alpha = 0, Y = 16, X = -16, ScaleY = 0, ScaleX = 32f/Config.Width, Height = 0 },
                new(kf[1]) { Alpha = 255, Y = 0, X = -16 , ScaleY = 1, ScaleX = 32f / Config.Width, Height = 32 },
                new(kf[2]) { Alpha = 255, Y = 0, X = -halfWidth, ScaleX = 1, Height = 32 })
                { Ease = SinInOut, Label = "ShowHide" },

            new(Backdrop,
                new(kf[0]) { X = 0, Height = 0, Alpha = 255 },
                new(kf[1]) { X = 0, Height = 32, Alpha = Config.Background.A },
                new(kf[2]) { X = 0, Alpha = Config.Background.A })
                { Ease = SinInOut, Label = "ShowHide" },

            new(TickWrapper,
                new(kf[0]) { Alpha = 0, X = -16, ScaleX = 32f / Config.Width },
                new(kf[1]) { Alpha = 0, X = -16, ScaleX = 32f / Config.Width },
                new(kf[2]) { Alpha = 255, X = -halfWidth, ScaleX = 1 }){ Label = "ShowHide" },

            new(LabelTextNode, Hidden[kf[0]], Hidden[kf[1]], Visible[kf[2]]){ Label = "ShowHide" },

            new(NumTextNode, Hidden[kf[0]], Hidden[kf[1]], Visible[kf[2]]){ Label = "ShowHide" },

            new(Tick, Hidden[kf[0]], Visible[kf[3]]){ Label = "ShowHide" }
        ];

        StartBackdropTween();
    }

    #endregion

    #region UpdateFuncs

    public override void PlaceTickMark(float prog)
    {
        var wid = Max(Drain.Width, Main.Width);
        Tick.SetPos(wid - 20, -27.5f)
            .SetAlpha(prog switch
            {
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
        Animator +=
        [
            new(Main,
                new(0, Main),
                new(200) { PartCoords = new(720, 17, 720, 32) })
                { Ease = SinInOut, Label = "ScrollAni" },
            new(Backdrop,
                new(0, Backdrop),
                new(200) { PartCoords = new(0, 17, 720, 32) })
                { Ease = SinInOut, Label = "ScrollAni" }
        ];
    }

    private void StartBackdropTween()
    {
        Animator -= "ScrollAni";
        Animator +=
        [
            new(Main,
                new(0) { PartCoords = new(0, 17, 720, 32) },
                new(5000) { PartCoords = new(720, 17, 720, 32) })
                { Repeat = true, Label = "ScrollAni" },
            new(Backdrop,
                new(0) { PartCoords = new(720, 17, 720, 32) },
                new(10000) { PartCoords = new(0, 17, 720, 32) })
                { Repeat = true, Label = "ScrollAni" }
        ];
    }

    public override void PostUpdate(float prog)
    {
        if (Tracker.CurrentData.HasLabelOverride) LabelTextNode.SetLabelText(Tracker.CurrentData.LabelOverride ?? " ");
    }

    #endregion

    #region Configs

    public sealed class MahjongRibbonConfig : GaugeBarWidgetConfig
    {
        [DefaultValue(144)] public float Width = 144;
        public float Angle;

        public AddRGB Background = "0x777777FF";
        public AddRGB MainColor = "0x987B7BFF";
        public AddRGB GainColor = "0xDE56B2A0";
        public AddRGB DrainColor = "0x8069B2A0";
        public AddRGB TickColor = "0xD36E27FF";

        public LabelTextProps LabelText = new(string.Empty, false, new(0, 0), new(255), 0x8E6A0CFF, Jupiter, 20, Left);
        protected override NumTextProps NumTextDefault => new(enabled: true,
                                                              position: new(0),
                                                              color: new(255),
                                                              edgeColor: new(0),
                                                              showBg: false,
                                                              bgColor: new(0),
                                                              font: MiedingerMed,
                                                              fontSize: 18,
                                                              align: Center,
                                                              invert: false);

        public MahjongRibbonConfig(WidgetConfig widgetConfig) : base(widgetConfig.MahjongRibbonCfg)
        {
            var config = widgetConfig.MahjongRibbonCfg;

            if (config == null) return;

            Width = config.Width;
            Angle = config.Angle;

            Background = config.Background;
            MainColor = config.MainColor;
            GainColor = config.GainColor;
            DrainColor = config.DrainColor;
            LabelText = config.LabelText;
        }

        public override Vector2 DefaultPosition { get; } = new(0, -27);

        public MahjongRibbonConfig() { }
    }

    private MahjongRibbonConfig config;

    public override MahjongRibbonConfig Config => config;

    public override void InitConfigs()
    {
        config = new(Tracker.WidgetConfig);
        if (Tracker.WidgetConfig.MahjongRibbonCfg == null && ShouldInvertByDefault) { Config.Invert = true; }
    }

    public override void ResetConfigs() => config = new();

    public Vector2 PosAdjust = new(0, -32);
    public override void ApplyConfigs()
    {
        base.ApplyConfigs();
        WidgetContainer.SetPos(Config.Position + PosAdjust)
                       .SetWidth(Config.Width);

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
             .SetOrigin(Config.Width / 2f, 16)
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
        NumTextNode.SetOrigin((NumTextNode.Width / 2f) + 1, (NumTextNode.Height / 2f) - 1);

    }

    public override void DrawUI()
    {
        base.DrawUI();
        switch (UiTab)
        {
            case Layout:
                FloatControls("Width", ref Config.Width, 64, 1440, 1);
                AngleControls("Angle", ref Config.Angle);
                break;
            case Colors:
                ColorPickerRGBA("Backdrop", ref Config.Background);
                ColorPickerRGBA("Main Bar", ref Config.MainColor);
                ColorPickerRGBA("Gain", ref Config.GainColor);
                ColorPickerRGBA("Drain", ref Config.DrainColor);
                ColorPickerRGB("Tick Color", ref Config.TickColor);
                break;
            case Behavior:
                SplitChargeControls(ref Config.SplitCharges, Tracker.RefType, Tracker.CurrentData.MaxCount);
                ToggleControls("Invert Fill", ref Config.Invert);
                HideControls("Collapse Empty", "Collapse Full");
                break;
            case Text:
                NumTextControls($"{Tracker.TermGauge} Text", ref Config.NumTextProps, true);
                LabelTextControls("Label Text", ref Config.LabelText, Tracker.DisplayAttr.Name);
                break;
            default:
                break;
        }
    }

    #endregion
}

public partial class WidgetConfig
{
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] public MahjongRibbonConfig? MahjongRibbonCfg { get; set; }
}
