using CustomNodes;
using GaugeOMatic.CustomNodes.Animation;
using GaugeOMatic.Trackers;
using Newtonsoft.Json;
using System.Numerics;
using GaugeOMatic.Widgets.Common;
using static CustomNodes.CustomNode;
using static CustomNodes.CustomNodeManager;
using static FFXIVClientStructs.FFXIV.Component.GUI.AlignmentType;
using static FFXIVClientStructs.FFXIV.Component.GUI.FontType;
using static GaugeOMatic.Utility.Color;
using static GaugeOMatic.Utility.MiscMath;
using static GaugeOMatic.Widgets.NinkiOverlay;
using static GaugeOMatic.Widgets.Common.NumTextProps;
using static GaugeOMatic.Widgets.WidgetTags;
using static GaugeOMatic.Widgets.Common.WidgetUI;
using static GaugeOMatic.Widgets.Common.WidgetUI.WidgetUiTab;

#pragma warning disable CS8618

namespace GaugeOMatic.Widgets;

[WidgetName("Ninki Overlay")]
[WidgetDescription("A glowing gauge bar aura fitted over the shape of the Ninki Gauge (or a replica of it).")]
[WidgetAuthor("ItsBexy")]
[WidgetTags(GaugeBar | MultiComponent)]
[MultiCompData("NK", "Ninki Gauge Replica", 2)]
public sealed unsafe class NinkiOverlay(Tracker tracker) : GaugeBarWidget(tracker)
{
    public override CustomPartsList[] PartsLists { get; } =
    [
        new ("ui/uld/JobHudNIN0.tex",
             new(0, 0, 256, 100),
             new(256, 152, 20, 88),
             new(0, 196, 196, 56))
    ];

    #region Nodes

    public CustomNode Tick;
    public CustomNode Shine;
    public CustomNode Calligraphy;

    public override Bounds GetBounds() => Main;

    public override CustomNode BuildContainer()
    {
        Main = ImageNodeFromPart(0, 0).SetAlpha(0).SetImageWrap(1).SetImageFlag(32).DefineTimeline(BarTimeline);
        Tick = ImageNodeFromPart(0, 1).SetAlpha(0).SetOrigin(22, 43.5f).SetImageFlag(32).DefineTimeline(TickTimeline);
        Shine = ImageNodeFromPart(0, 1).SetAlpha(0).SetOrigin(22, 43.5f).SetImageFlag(32);
        Calligraphy = ImageNodeFromPart(0, 2).SetPos(23, 29).SetOrigin(98, 28).SetImageFlag(32).SetRGBA(new(1, 1, 1, 0));
        NumTextNode = new();

        return new(CreateResNode(), Main, Tick, Shine, Calligraphy, NumTextNode);
    }

    #endregion

    #region Animations

    public KeyFrame[] BarTimeline =>
    [
        new(0) { Width = 18, Alpha = 0 },
        new(10) { Width = 28, Alpha = Config.ScrollColor.A },
        new(219) { Width = 237, Alpha = Config.ScrollColor.A },
        new(229) { Width = 247, Alpha = Config.ScrollColor.A/2f }
    ];
    public static KeyFrame[] TickTimeline =>
    [
        new(0) { X = -1, Alpha = 0 },
        new(10) { X = 9, Alpha = 255 },
        new(219) { X = 218, Alpha = 255 },
        new(229) { X = 228, Alpha = 0 }
    ];

    private void AppearAnim() =>
        Animator +=
        [
            new(Calligraphy,
                new(0) { Scale = 1, Alpha = 70 },
                new(100) { Scale = 1, Alpha = 160 },
                new(300) { Scale = 1.5f, Alpha = 0 }),
            new(Shine,
                new(0) { X = 11, Y = 14, Alpha = 76 },
                new(200) { X = 190, Y = 10, Alpha = 76 },
                new(400) { X = 300, Y = 10, Alpha = 0 }),
            new(Shine,
                new(0) { ScaleX = 1.15f, ScaleY = 1.5f, Rotation = 0 },
                new(100) { ScaleX = 3f, ScaleY = 1.2f, Rotation = -0.065f },
                new(400) { ScaleX = 0.6f, ScaleY = 2f, Rotation = 0 })
        ];

    #endregion

    #region UpdateFuncs

    public static float CalcTickY(float prog) => prog >= 0.845 ? PolyCalc(prog, 674.909344670314, -1438.55803705433, 771.474668979089) : PolyCalc(prog, 11.6767206839834, 52.8756304874818, -160.237711149177, 114.469149751575);

    public static float CalcTickScale(float prog) => prog < 0.845 ? 1 : PolyCalc(prog, -40.6519205182959, 91.1244645648548, -49.4678579360407);

    public int Flippy;
    public override void Update()
    {
        var current = Tracker.CurrentData.GaugeValue;
        var max = Tracker.CurrentData.MaxGauge;
        var prog = CalcProg();
        var prevProg = CalcProg(true);

        if (Config.SplitCharges && Tracker.RefType == RefType.Action) AdjustForCharges(ref current, ref max, ref prog, ref prevProg);
        NumTextNode.UpdateValue(current, max);

        if (prog > 0 && prevProg == 0) AppearAnim();

        NumTextNode.UpdateValue(current, max);

        AnimateDrainGain(prog, prevProg);

        Animator.RunTweens();

        if (Main.Progress > 0) Tick.SetScaleY(CalcTickScale(Main.Progress) * (Flippy > 9 ? -1 : 1));
        Flippy++;
        Flippy %= 20;

        Tick.SetProgress(Main).SetY(CalcTickY(Main.Progress));
    }

    #endregion

    #region Configs

    public sealed class NinkiOverlayConfig : GaugeBarWidgetConfig
    {
        public AddRGB ScrollColor = new(80, 30, -70, 90);
        public ColorRGB TickColor = new(255, 164, 93);
        protected override NumTextProps NumTextDefault => new(enabled: true,
                                                              position: new(227, 58),
                                                              color: new(255, 241, 197),
                                                              edgeColor: new(110, 25, 0),
                                                              showBg: false,
                                                              bgColor: new(0),
                                                              font: MiedingerMed,
                                                              fontSize: 20,
                                                              align: Center,
                                                              invert: false);

        public NinkiOverlayConfig(WidgetConfig widgetConfig) : base(widgetConfig.NinkiOverlayCfg)
        {
            var config = widgetConfig.NinkiOverlayCfg;

            if (config == null) return;

            ScrollColor = config.ScrollColor;
            TickColor = config.TickColor;
        }

        public NinkiOverlayConfig() { }
    }

    private NinkiOverlayConfig config;

    public override NinkiOverlayConfig Config => config;

    public override void InitConfigs()
    {
        config = new(Tracker.WidgetConfig);
        if (Tracker.WidgetConfig.NinkiOverlayCfg == null && ShouldInvertByDefault) { Config.Invert = true; }
    }

    public override void ResetConfigs() => config = new();

    public override void ApplyConfigs()
    {
        base.ApplyConfigs();
        Main.SetAddRGB(Config.ScrollColor).DefineTimeline(BarTimeline).SetProgress(CalcProg());
        Tick.SetRGB(Config.TickColor);
        Shine.SetRGB(Config.TickColor);
        Calligraphy.SetAddRGB((Vector4)Config.TickColor);

        NumTextNode.ApplyProps(Config.NumTextProps);
    }

    public override void DrawUI()
    {
        base.DrawUI();
        switch (UiTab)
        {
            case Colors:
                ColorPickerRGBA("Scroll Color", ref Config.ScrollColor);
                ColorPickerRGBA("Tick Color", ref Config.TickColor);
                break;
            case Behavior:
                ToggleControls("Invert Fill", ref Config.Invert);
                break;
            case Text:
                NumTextControls($"{Tracker.TermGauge} Text", ref Config.NumTextProps);
                break;
            default:
                break;
        }
    }

    #endregion
}

public partial class WidgetConfig
{
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] public NinkiOverlayConfig? NinkiOverlayCfg { get; set; }
}
