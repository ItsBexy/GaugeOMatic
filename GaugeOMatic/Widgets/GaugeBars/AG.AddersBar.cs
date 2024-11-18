using CustomNodes;
using Dalamud.Interface;
using GaugeOMatic.CustomNodes.Animation;
using GaugeOMatic.Trackers;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel;
using System.Numerics;
using GaugeOMatic.Widgets.Common;
using static CustomNodes.CustomNode;
using static CustomNodes.CustomNode.CustomNodeFlags;
using static CustomNodes.CustomNodeManager;
using static Dalamud.Interface.FontAwesomeIcon;
using static FFXIVClientStructs.FFXIV.Component.GUI.AlignmentType;
using static FFXIVClientStructs.FFXIV.Component.GUI.FontType;
using static GaugeOMatic.CustomNodes.Animation.KeyFrame;
using static GaugeOMatic.CustomNodes.Animation.Tween.EaseType;
using static GaugeOMatic.Utility.Color;
using static GaugeOMatic.Widgets.AddersBar;
using static GaugeOMatic.Widgets.GaugeBarWidgetConfig;
using static GaugeOMatic.Widgets.Common.LabelTextProps;
using static GaugeOMatic.Widgets.NumTextProps;
using static GaugeOMatic.Widgets.WidgetTags;
using static GaugeOMatic.Widgets.WidgetUI;
using static GaugeOMatic.Widgets.WidgetUI.WidgetUiTab;
using static System.Math;
// ReSharper disable SuggestBaseTypeForParameter

#pragma warning disable CS8618

namespace GaugeOMatic.Widgets;

[WidgetName("Addersgall Bar")]
[WidgetDescription("A recreation of Sage's Addersgall Gauge Bar.")]
[WidgetAuthor("ItsBexy")]
[WidgetTags(GaugeBar | Replica | MultiComponent)]
[MultiCompData("AG", "Addersgall Gauge Replica", 2)]
public sealed unsafe class AddersBar(Tracker tracker) : GaugeBarWidget(tracker)
{
    public override CustomPartsList[] PartsLists { get; } =
    [
        new ("ui/uld/JobHudGFF1.tex",
             new(0, 24, 200, 32), // frame
             new(0, 0, 200, 24),
             new(89, 100, 8, 24),
             new(99, 100, 30, 24), // bar overlay
             new(130, 100, 22, 24),
             new(152, 100, 22, 24),
             new(174, 100, 22, 24),
             new(178, 124, 22, 24),
             new(1, 124, 142, 24), // bar fill
             new(124, 80, 20, 20),
             new(144, 80, 20, 20),
             new(164, 56, 32, 32),
             new(176, 88, 24, 12),
             new(164, 88, 12, 12))
    ];

    #region Nodes

    public CustomNode BarFrame;
    public CustomNode Bar;
    public CustomNode Frame;
    public CustomNode Plate;
    public LabelTextNode LabelTextNode;
    public CustomNode Backdrop;
    public CustomNode MainOverlay;
    public CustomNode Sparkles;

    public override Bounds GetBounds() => new(Frame, Config.ShowPlate ? Plate : Frame);

    public override CustomNode BuildContainer()
    {
        Bar = BuildBar();
        Plate = ImageNodeFromPart(0, 1).SetPos(-100, -24).SetOrigin(100, 24).RemoveFlags(SetVisByAlpha);
        Frame = NineGridFromPart(0, 0, 0, 32, 0, 32);
        LabelTextNode = new(Config.LabelTextProps.Text, Tracker.DisplayAttr.Name);
        LabelTextNode.SetWidth(144);

        NumTextNode = new();

        BarFrame = new CustomNode(CreateResNode(), Plate, Bar, Frame).SetOrigin(0, 11);

        return new CustomNode(CreateResNode(), BarFrame, LabelTextNode, NumTextNode).SetOrigin(0, 16f);
    }

    private CustomNode BuildBar()
    {
        Backdrop = NineGridFromPart(0, 2).SetNineGridOffset(new(0, 2, 0, 2));
        Drain = NineGridFromPart(0, 8, 0, 12, 0, 12).SetWidth(0);
        Gain = NineGridFromPart(0, 8, 0, 12, 0, 12).SetWidth(0);
        Main = NineGridFromPart(0, 8, 0, 12, 0, 12).SetWidth(0);
        MainOverlay = NineGridFromPart(0, 3).SetNineGridBlend(2);
        Sparkles = BuildSparkles();

        return new CustomNode(CreateResNode(), Backdrop, Drain, Gain, Main, MainOverlay, Sparkles).SetPos(0, 0);
    }

    private CustomNode BuildSparkles()
    {
        var sparkleNodes = new[]
        {
            ImageNodeFromPart(0, 4).SetScale(1, 0.95f).SetAddRGB(-100, 0, 0),
            ImageNodeFromPart(0, 13).SetOrigin(6, 6).SetPos(-10, 10).SetAddRGB(-100, 0, 0),
            ImageNodeFromPart(0, 13).SetOrigin(6, 6).SetPos(2.25f, 6).SetAddRGB(-100, 0, 0),
            ImageNodeFromPart(0, 13).SetOrigin(6, 6).SetPos(4, 9).SetAddRGB(-100, 0, 0),
            ImageNodeFromPart(0, 13).SetOrigin(6, 6).SetPos(2, 3).SetAddRGB(-100, 0, 0),
            ImageNodeFromPart(0, 13).SetOrigin(6, 6).SetPos(4, 0).SetAddRGB(-100, 0, 0),
            ImageNodeFromPart(0, 5).SetPos(0, 0)
        };

        AnimateSparkles(sparkleNodes);

        return new CustomNode(CreateResNode(), sparkleNodes).SetSize(22, 24).SetPos(0, 0);
    }

    #endregion

    #region Animations

    public KeyFrame[] BarTimeline => [new(0) { Width = 0 }, new(1) { Width = Config.Width }];

    public override void HideBar(bool instant = false)
    {
        var frameWidth = Config.Width + 56;
        var kf = instant ? [0, 0, 0] : new[] { 0, 250, 350 };

        Animator -= "Expand";
        Animator +=
        [
            new(BarFrame,
                new(kf[0]) { Y = 0 },
                new(kf[1]) { Y = 1 },
                new(kf[2]) { Y = 10 })
                { Ease = SinInOut, Label = "Collapse" },

            new(Frame,
                new(kf[0]) { X = frameWidth / -2f, Width = frameWidth, AddRGB = 0, Alpha = 255 },
                new(kf[1]) { X = -28, Width = 56, AddRGB = 50, Alpha = 255 },
                new(kf[2]) { X = -28, Width = 56, AddRGB = 255, Alpha = 0 })
                { Ease = SinInOut, Label = "Collapse" },

            new(Plate,
                new(kf[0]) { Alpha = 255, AddRGB = 0, ScaleY = 1, ScaleX = 1, Y = -24 },
                new(kf[1]) { Alpha = 255, AddRGB = 255, ScaleY = 0, ScaleX = 0.1f, Y = -20 },
                new(kf[2]) { Alpha = 0, AddRGB = 255, ScaleY = 0, ScaleX = 0, Y = -20 })
                { Ease = SinInOut, Label = "Collapse" },

            new(Bar,
                new(kf[0]) { ScaleX = Config.Mirror ? -1 : 1, Alpha = 255 },
                new(kf[1]) { ScaleX = 0, Alpha = 128 },
                new(kf[2]) { ScaleX = 0, Alpha = 128 })
                { Ease = SinInOut, Label = "Collapse" },

            new(Sparkles, Visible[kf[0]], new((int)(kf[1] * 0.6f)) { Alpha = 0 }) { Label = "Collapse" },
            new(LabelTextNode, Visible[kf[0]], Hidden[kf[1]]) { Label = "Collapse" },
            new(NumTextNode, Visible[kf[0]], Hidden[kf[2]]) { Label = "Collapse" }
        ];
    }

    public override void RevealBar(bool instant = false)
    {
        var frameWidth = Config.Width + 56;

        var kf = instant ? [0, 0, 0] : new[] { 0, 100, 350 };

        Animator -= "Collapse";
        BarFrame.SetY(0);

        Animator +=
        [
            new(Frame,
                new(kf[0]) { Alpha = 0, X = -28, Width = 56, AddRGB = 200 },
                new(kf[1]) { Alpha = 255, X = -28, Width = 56, AddRGB = 255 },
                new(kf[2]) { Alpha = 255, X = frameWidth / -2f, Width = frameWidth, AddRGB = 0 })
                { Ease = SinInOut, Label = "Expand" },
            new(Plate,
                new(kf[0]) { Alpha = 0, AddRGB = 200, ScaleY = 0, ScaleX = 0, Y = -20 },
                new(kf[1]) { Alpha = 0, AddRGB = 255, ScaleY = 0, ScaleX = 0, Y = -20 },
                new(kf[2]) { Alpha = 255, AddRGB = 0, ScaleY = 1, ScaleX = 1, Y = -24 })
                { Ease = SinInOut, Label = "Expand" },
            new(Bar,
                new(kf[0]) { Alpha = 0, ScaleX = 0 },
                new(kf[1]) { Alpha = 128, ScaleX = 0 },
                new(kf[2]) { Alpha = 255, ScaleX = Config.Mirror ? -1 : 1 })
                { Ease = SinInOut, Label = "Expand" },
            new(Sparkles, Hidden[kf[0]], Hidden[200], Visible[kf[2]]) { Label = "Expand" },
            new(LabelTextNode, Hidden[kf[0]], Hidden[kf[1]], Visible[kf[2]]) { Label = "Expand" },
            new(NumTextNode, Hidden[kf[0]], Hidden[kf[1]], Visible[kf[2]]) { Label = "Expand" }
        ];
    }

    private void AnimateSparkles(CustomNode[] sparkleNodes)
    {
        Animator += new Tween(sparkleNodes[0],
                                 Visible[0],
                                 new(300) { Alpha = 204 },
                                 Visible[600])
        { Repeat = true };

        for (var i = 1; i <= 5; i++) sparkleNodes[i].SetImageFlag(32).SetImageWrap(2);

        Animator +=
        [
            new(sparkleNodes[1],
                new(0) { X = 6, Scale = 1, Alpha = 0 },
                new(199) { X = 6, Scale = 1, Alpha = 0 },
                new(200) { X = 6, Scale = 1, Alpha = 255 },
                new(600) { X = -10, Scale = 0.4f, Alpha = 0 }) { Repeat = true },

            new(sparkleNodes[2],
                new(0) { X = 4, Scale = 1, Alpha = 0 },
                new(269) { X = 4, Scale = 1, Alpha = 0 },
                new(270) { X = 4, Scale = 1, Alpha = 255 },
                new(630) { X = -4, Scale = 0.4f, Alpha = 0 }) { Repeat = true },

            new(sparkleNodes[3],
                new(0) { X = 6, Y = 7, Scale = 0.88f, Alpha = 255 },
                new(307) { X = 1, Y = 7, Scale = 0.88f, Alpha = 0 },
                new(308) { X = 4, Y = 8, Scale = 1, Alpha = 255 },
                new(615) { X = -12, Y = 8, Scale = 0.4f, Alpha = 0 }) { Repeat = true },

            new(sparkleNodes[4],
                new(0) { X = 3.75f, Y = 2, Scale = 0.4f, Alpha = 255 },
                new(244) { X = -10, Y = 2, Scale = 0.4f, Alpha = 0 },
                new(245) { X = 3.75f, Y = 4, Scale = 1, Alpha = 255 },
                new(590) { X = -6, Y = 4, Scale = 0.4f, Alpha = 0 }) { Repeat = true },

            new(sparkleNodes[5],
                new(0) { X = 3.75f, Y = 0, Scale = 1f, Alpha = 255 },
                new(360) { X = -10, Y = 0, Scale = 0.4f, Alpha = 0 },
                new(361) { X = 3.5f, Y = 0, Scale = 1, Alpha = 255 },
                new(605) { X = -7.5f, Y = 0, Scale = 0.4f, Alpha = 0 }) { Repeat = true },

            new(sparkleNodes[6],
                new(0) { PartId = 5 },
                new(350) { PartId = 7 }) { Repeat = true }
        ];

    }

    #endregion

    #region UpdateFuncs

    public override void OnIncreaseToMax() { if (Config.HideFull) HideBar(); }

    public override void PostUpdate(float prog)
    {
        MainOverlay.SetWidth(Main.Width);

        if (!MilestoneActive)
        {
            prog %= 0.5f;
            Main.SetAddRGB((prog < 0.25f ? new(0, (short)(120f * prog), (short)(160f * prog)) : new AddRGB(0, (short)(60 - (120f * prog)), (short)(80 - (160f * prog)))) + Config.MainColor + (AddRGB)new(116, -3, -30));
        }

        if (Tracker.CurrentData.HasLabelOverride) LabelTextNode.SetLabelText(Tracker.CurrentData.LabelOverride ?? " ");
    }

    protected override void StartMilestoneAnim()
    {
        Animator -= "BarPulse";
        var colorAdjust = new AddRGB(116, -3, -30);
        for (var i = 0; i <= 6; i++) Sparkles[i].SetAddRGB(Config.PulseSparkles);
        Animator += new Tween(Main,
                                  new(0) { AddRGB = Config.PulseColor2 + colorAdjust },
                                  new(800) { AddRGB = Config.PulseColor + colorAdjust },
                                  new(1600) { AddRGB = Config.PulseColor2 + colorAdjust })
        { Ease = SinInOut, Repeat = true, Label = "BarPulse" };
    }

    protected override void StopMilestoneAnim()
    {
        Animator -= "BarPulse";
        Main.SetAddRGB(Config.MainColor + new AddRGB(116, -3, -30));
        for (var i = 0; i <= 6; i++) Sparkles[i].SetAddRGB(Config.SparkleColor);
    }

    public override void PlaceTickMark(float prog) => Sparkles.SetPos(Main.Node->Width - 13f, -0.5f).SetVis(prog > 0);

    #endregion

    #region Configs

    public sealed class AddersBarConfig : GaugeBarWidgetConfig
    {
        [DefaultValue(144)] public float Width = 144;
        public float Angle;
        public bool ShowPlate;

        public AddRGB BGColor = new(0, 0, 0, 229);
        public ColorRGB FrameColor = new(100, 100, 100);
        public AddRGB MainColor = new(-136, -17, 10);
        public AddRGB GainColor = new(5, 155, 93);
        public AddRGB DrainColor = new(-107, -159, -111);
        public AddRGB SparkleColor = new(-100, 0, 0);

        public AddRGB PulseColor = "0x9D1E3FFF";
        public AddRGB PulseColor2 = "0xAD6C5DFF";
        public AddRGB PulseSparkles = "0xB35F2FFF";

        public bool Mirror;

        public LabelTextProps LabelTextProps = new(string.Empty, false, new(0, 20), new(255), new(0), TrumpGothic, 22, Center);
        protected override NumTextProps NumTextDefault => new(enabled: true,
                                                              position: new(0, 0),
                                                              color: new(255),
                                                              edgeColor: new(0),
                                                              showBg: true,
                                                              bgColor: new(0),
                                                              font: MiedingerMed,
                                                              fontSize: 18,
                                                              align: Left,
                                                              invert: false);

        public AddersBarConfig(WidgetConfig widgetConfig) : base(widgetConfig.AddersBarCfg)
        {
            var config = widgetConfig.AddersBarCfg;

            if (config == null) return;

            Width = config.Width;
            Angle = config.Angle;
            ShowPlate = config.ShowPlate;

            BGColor = config.BGColor;
            FrameColor = config.FrameColor;
            MainColor = config.MainColor;
            GainColor = config.GainColor;
            DrainColor = config.DrainColor;
            SparkleColor = config.SparkleColor;

            PulseColor = config.PulseColor;
            PulseColor2 = config.PulseColor2;
            PulseSparkles = config.PulseSparkles;

            Mirror = config.Mirror;

            LabelTextProps = config.LabelTextProps;
        }

        public AddersBarConfig() { }
    }

    private AddersBarConfig config;

    public override AddersBarConfig Config => config;

    public override void InitConfigs()
    {
        config = new(Tracker.WidgetConfig);
        if (Tracker.WidgetConfig.AddersBarCfg == null && ShouldInvertByDefault) Config.Invert = true;
    }

    public override void ResetConfigs() => config = new();

    public override void ApplyConfigs()
    {
        var frameWidth = Config.Width + 56;

        WidgetContainer.SetPos(Config.Position + new Vector2(100, 38.5f))
                  .SetScale(Config.Scale);

        Frame.SetWidth(frameWidth)
             .SetX(frameWidth / -2f)
             .SetOrigin(frameWidth / 2f, 16)
             .SetMultiply(Config.FrameColor);

        Plate.SetVis(Config.ShowPlate)
             .SetMultiply(Config.FrameColor);

        BarFrame.SetRotation(Config.Angle * 0.01745329f);

        Bar.SetX(Config.Width / -2)
           .SetOrigin(Config.Width / 2, 0)
           .SetWidth(Config.Width)
           .SetScaleX(Config.Mirror ? -1 : 1);

        Backdrop.SetWidth(Config.Width)
                .SetAddRGB(Config.BGColor, true);

        var colorAdjust = new AddRGB(116, -3, -30);
        var barSize = Tracker.CurrentData.GaugeValue / Tracker.CurrentData.MaxGauge * Config.Width;
        Drain.SetAddRGB(Config.DrainColor + colorAdjust)
             .SetWidth(0)
             .DefineTimeline(BarTimeline);

        Gain.SetAddRGB(Config.GainColor + colorAdjust).SetWidth(0).DefineTimeline(BarTimeline);
        Main.DefineTimeline(BarTimeline).SetProgress(CalcProg());

        for (var i = 0; i <= 6; i++) Sparkles[i].SetAddRGB(Config.SparkleColor);

        HandleMilestone(CalcProg(), true);

        Sparkles.SetX(barSize - 13);

        LabelTextNode.ApplyProps(Config.LabelTextProps, new(Config.Width / -2, 0))
                     .SetWidth(Config.Width);

        NumTextNode.ApplyProps(Config.NumTextProps, new((Config.Width / 2) + 88, 11.5f));
    }

    public override void DrawUI()
    {
        base.DrawUI();
        switch (UiTab)
        {
            case Layout:
                FloatControls("Width", ref Config.Width, Config.ShowPlate ? 144 : 30, 2000, 1);
                AngleControls("Angle", ref Config.Angle);
                RadioIcons("Fill Direction", ref Config.Mirror, [false, true], ArrowIcons);
                ToggleControls("Backplate", ref Config.ShowPlate);
                if (Config.ShowPlate) Config.Width = Max(Config.Width, 144);
                break;
            case Colors:

                ColorPickerRGBA("Backdrop", ref Config.BGColor);
                ColorPickerRGB("Frame Tint", ref Config.FrameColor);
                ColorPickerRGBA("Main Bar", ref Config.MainColor);
                ColorPickerRGBA("Gain", ref Config.GainColor);
                ColorPickerRGBA("Drain", ref Config.DrainColor);
                ColorPickerRGB("Sparkles", ref Config.SparkleColor);
                break;
            case Behavior:
                SplitChargeControls(ref Config.SplitCharges, Tracker.RefType, Tracker.CurrentData.MaxCount);
                ToggleControls("Invert Fill", ref Config.Invert);
                HideControls("Collapse Empty", "Collapse Full");
                MilestoneControls("Pulse", ref Config.MilestoneType, ref Config.Milestone, MaxGauge);
                if (Config.MilestoneType > 0)
                {
                    ColorPickerRGB("Pulse Colors", ref Config.PulseColor);
                    ColorPickerRGB(" ##Pulse2", ref Config.PulseColor2);
                    ColorPickerRGB(" ##Pulse3", ref Config.PulseSparkles);
                }
                break;
            case Text:
                NumTextControls($"{Tracker.TermGauge} Text", ref Config.NumTextProps, true);
                LabelTextControls("Label Text", ref Config.LabelTextProps, Tracker.DisplayAttr.Name);
                break;
            default:
                break;
        }
    }

    private List<FontAwesomeIcon> ArrowIcons =>
        Abs(Config.Angle) > 135 ? [ArrowLeft, ArrowRight] :
        Config.Angle > 45 ? [ArrowDown, ArrowUp] :
        Config.Angle < -45 ? [ArrowUp, ArrowDown] :
        [ArrowRight, ArrowLeft];

    #endregion
}

public partial class WidgetConfig
{
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] public AddersBarConfig? AddersBarCfg { get; set; }
}
