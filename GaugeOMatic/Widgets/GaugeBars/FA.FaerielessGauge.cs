using CustomNodes;
using GaugeOMatic.CustomNodes.Animation;
using GaugeOMatic.Trackers;
using Newtonsoft.Json;
using System.Numerics;
using GaugeOMatic.Widgets.Common;
using static CustomNodes.CustomNodeManager;
using static FFXIVClientStructs.FFXIV.Component.GUI.AlignmentType;
using static FFXIVClientStructs.FFXIV.Component.GUI.FontType;
using static GaugeOMatic.Utility.Color;
using static GaugeOMatic.Widgets.Common.CommonParts;
using static GaugeOMatic.Widgets.FaerieLess;
using static GaugeOMatic.Widgets.Common.NumTextProps;
using static GaugeOMatic.Widgets.WidgetTags;
using static GaugeOMatic.Widgets.Common.WidgetUI;
using static GaugeOMatic.Widgets.Common.WidgetUI.WidgetUiTab;

#pragma warning disable CS8618

namespace GaugeOMatic.Widgets;

[WidgetName("Faerie Bar")]
[WidgetDescription("A slightly curved gauge bar shaped like the Faerie Gauge, but without the decorations.")]
[WidgetAuthor("ItsBexy")]
[WidgetTags(GaugeBar | Replica | MultiComponent)]
[MultiCompData("FA", "Faerie Gauge Replica", 2)]
public sealed unsafe class FaerieLess(Tracker tracker) : GaugeBarWidget(tracker)
{
    public override CustomPartsList[] PartsLists { get; } = [SCH1];

    #region Nodes

    public CustomNode Backdrop;

    public override CustomNode BuildContainer()
    {
        Backdrop = ImageNodeFromPart(0, 2).SetImageWrap(1).SetRGBA(Config.Background);
        Drain = ImageNodeFromPart(0, 1).SetWidth(0).SetImageWrap(1).SetRGBA(Config.DrainColor);
        Gain = ImageNodeFromPart(0, 1).SetWidth(0).SetImageWrap(1).SetRGBA(Config.GainColor);
        Main = ImageNodeFromPart(0, 1).SetWidth(0).SetImageWrap(1).SetRGBA(Config.MainColor);
        NumTextNode = new();

        return new(CreateResNode(), Backdrop, Drain, Gain, Main, NumTextNode);
    }

    #endregion

    #region Animations

    public static KeyFrame[] BarTimeline => [new(0) { Width = 0 }, new(1) { Width = 174 }];

    #endregion

    #region Configs

    public sealed class FaerieLessConfig : GaugeBarWidgetConfig
    {
        public new Vector2 Scale = new(1, 1);
        public float Angle;
        public ColorRGB MainColor = new(255, 255, 255);
        public ColorRGB Background = new(255, 255, 255);
        public ColorRGB GainColor = new(51, 213, 207);
        public ColorRGB DrainColor = new(113, 5, 70);
        public bool Mirror;

        protected override NumTextProps NumTextDefault => new(enabled: true,
                                                              position: new(0, 0),
                                                              color: 0xffffffff,
                                                              edgeColor: 0x288246ff,
                                                              showBg: false,
                                                              bgColor: new(0),
                                                              font: MiedingerMed,
                                                              fontSize: 18,
                                                              align: Right,
                                                              invert: false);

        public FaerieLessConfig(WidgetConfig widgetConfig) : base(widgetConfig.FaerieLessCfg)
        {
            var config = widgetConfig.FaerieLessCfg;

            if (config == null) return;

            Scale = config.Scale;
            Angle = config.Angle;
            MainColor = config.MainColor;
            Background = config.Background;
            GainColor = config.GainColor;
            DrainColor = config.DrainColor;
            Mirror = config.Mirror;
        }

        public override Vector2 DefaultPosition { get; } = new(50, 28);

        public FaerieLessConfig() { }
    }

    private FaerieLessConfig config;

    public override FaerieLessConfig Config => config;

    public override void InitConfigs()
    {
        config = new(Tracker.WidgetConfig);
        if (Tracker.WidgetConfig.FaerieLessCfg == null && ShouldInvertByDefault) Config.Invert = true;
    }

    public override void ResetConfigs() => config = new();

    public override void ApplyConfigs()
    {
        base.ApplyConfigs();
        WidgetContainer.SetScale(Config.Scale.X, Config.Scale.Y * (Config.Mirror ? -1f : 1f))
                       .SetRotation(Config.Angle,true);

        Backdrop.SetRGBA(Config.Background);
        Main.SetRGBA(Config.MainColor).DefineTimeline(BarTimeline);
        Gain.SetRGBA(Config.GainColor).DefineTimeline(BarTimeline);
        Drain.SetRGBA(Config.DrainColor).DefineTimeline(BarTimeline);

        NumTextNode.ApplyProps(Config.NumTextProps, new(109, 40));
    }

    public override void DrawUI()
    {
        switch (UiTab)
        {
            case Layout:
                PositionControls("Position", ref Config.Position);
                ScaleControls("Scale", ref Config.Scale);
                AngleControls("Angle", ref Config.Angle);
                ToggleControls("Mirror", ref Config.Mirror);
                break;
            case Colors:
                ColorPickerRGBA("Backdrop", ref Config.Background);
                ColorPickerRGBA("Main Bar", ref Config.MainColor);
                ColorPickerRGBA("Gain", ref Config.GainColor);
                ColorPickerRGBA("Drain", ref Config.DrainColor);
                break;
            case Behavior:
                SplitChargeControls(ref Config.SplitCharges, Tracker.RefType, Tracker.CurrentData.MaxCount);
                ToggleControls("Invert Fill", ref Config.Invert);
                break;
            case Text:
                NumTextControls($"{Tracker.TermGauge} Text", ref Config.NumTextProps);
                break;
            default:
                break;
        }
    }

    public override void ChangeScale(float amt)
    {
        var y = Config.Scale.Y;
        var x = Config.Scale.X;

        var a1 = (y + x) / 2f;
        var a2 = a1 + (0.05f * amt);

        Config.Scale = new(x / a1 * a2, y / a1 * a2);
    }

    #endregion
}

public partial class WidgetConfig
{
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] public FaerieLessConfig? FaerieLessCfg { get; set; }
}
