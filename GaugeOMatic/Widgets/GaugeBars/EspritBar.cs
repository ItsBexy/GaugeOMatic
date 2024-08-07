using CustomNodes;
using GaugeOMatic.CustomNodes.Animation;
using GaugeOMatic.Trackers;
using Newtonsoft.Json;
using System.ComponentModel;
using System.Numerics;
using static CustomNodes.CustomNodeManager;
using static Dalamud.Interface.FontAwesomeIcon;
using static FFXIVClientStructs.FFXIV.Component.GUI.AlignmentType;
using static FFXIVClientStructs.FFXIV.Component.GUI.FontType;
using static GaugeOMatic.CustomNodes.Animation.KeyFrame;
using static GaugeOMatic.CustomNodes.Animation.Tween.EaseType;
using static GaugeOMatic.Trackers.Tracker;
using static GaugeOMatic.Utility.Color;
using static GaugeOMatic.Widgets.Common.CommonParts;
using static GaugeOMatic.Widgets.EspritBar;
using static GaugeOMatic.Widgets.GaugeBarWidgetConfig;
using static GaugeOMatic.Widgets.MilestoneType;
using static GaugeOMatic.Widgets.NumTextProps;
using static GaugeOMatic.Widgets.WidgetInfo;
using static GaugeOMatic.Widgets.WidgetTags;
using static GaugeOMatic.Widgets.WidgetUI;
using static GaugeOMatic.Widgets.WidgetUI.WidgetUiTab;

#pragma warning disable CS8618

namespace GaugeOMatic.Widgets;

public sealed unsafe class EspritBar : GaugeBarWidget
{
    public EspritBar(Tracker tracker) : base(tracker) { }

    public override WidgetInfo WidgetInfo => GetWidgetInfo;

    public static WidgetInfo GetWidgetInfo { get; } = new()
    {
        DisplayName = "Esprit Bar",
        Author = "ItsBexy",
        Description = "A curved bar based on DNC's Esprit Gauge",
        WidgetTags = GaugeBar | HasAddonRestrictions,
        RestrictedAddons = ClipConflictAddons
    };

    public override CustomPartsList[] PartsLists { get; } = { DNC1, CircleMask};

    #region Nodes

    public CustomNode Backdrop;
    public CustomNode FanPlate;
    public CustomNode BarContents;
    public CustomNode FrameCover;
    public CustomNode Frame;
    public CustomNode FanClip;
    public CustomNode FillNodes;
    public CustomNode Fan;

    public CustomNode MainContainer;
    public CustomNode DrainContainer;
    public CustomNode GainContainer;
    public CustomNode MainMask;
    public CustomNode DrainMask;
    public CustomNode GainMask;

    public override CustomNode BuildContainer()
    {
        Fan = BuildFan();
        NumTextNode = new();
        NumTextNode.SetAlpha(255);
        return new CustomNode(CreateResNode(), Fan, NumTextNode).SetSize(200, 128).SetOrigin(100, 106);
    }

    private CustomNode BuildFan()
    {
        FanPlate = ImageNodeFromPart(0, 3).SetImageWrap(3).SetPos(16, 22).SetSize(168, 100);
        BarContents = BuildBarContents();
        FanClip = ImageNodeFromPart(0, 5).SetPos(76, 90).SetSize(48, 32).SetImageWrap(1).SetOrigin(24, 32);

        return new CustomNode(CreateResNode(), FanPlate, BarContents, FanClip).SetOrigin(100, 106).SetAlpha(255);
    }

    private CustomNode BuildBarContents()
    {
        Backdrop = ImageNodeFromPart(0, 1).SetPos(1, 3).SetSize(166, 74).SetImageWrap(2).SetOrigin(84, 0);
        FillNodes = BuildFillNodes();
        FrameCover = ImageNodeFromPart(0, 11).SetSize(168, 80).SetImageWrap(3).Hide(); // dunno whether/when to bother displaying this
        Frame = ImageNodeFromPart(0, 4).SetSize(168, 80).SetImageWrap(3);

        return new CustomNode(CreateResNode(), Backdrop, FillNodes, FrameCover, Frame).SetPos(16, 22).SetSize(168, 80);
    }

    private CustomNode BuildFillNodes()
    {
        CustomNode FillNode() => ImageNodeFromPart(0, 0).SetPos(1,0)
                                                        .SetOrigin(83, 82)
                                                        .SetRotation(-151, true)
                                                        .DefineTimeline(BarTimeline);

        CustomNode MaskNode() => ClippingMaskFromPart(1,0).SetSize(176,176).SetPos(-4,-88).SetOrigin(88,176).SetScale(2,1);

        static CustomNode FillContainer(CustomNode fill,CustomNode mask) =>
            new CustomNode(CreateResNode(), fill, mask)
                .SetSize(168, 70)
                .SetOrigin(84, 70);

        Drain = FillNode();
        Gain = FillNode();
        Main = FillNode();

        DrainMask = MaskNode();
        GainMask = MaskNode();
        MainMask = MaskNode();

        MainContainer = FillContainer(Main,MainMask);
        DrainContainer = FillContainer(Drain,DrainMask);
        GainContainer = FillContainer(Gain,GainMask);

        return new CustomNode(CreateResNode(), DrainContainer, GainContainer, MainContainer).SetPos(0, 1).SetSize(168, 70).SetOrigin(0,-1);
    }

    #endregion

    #region Animations

    public static KeyFrame[] BarTimeline => new KeyFrame[] { new(0) { Rotation = -2.6542183675969f + 0.01f }, new(1) { Rotation = -0.05f }};

    protected override void StartMilestoneAnim()
    {
        Animator -= "BarPulse";
        Animator += new Tween(MainContainer,
                                  new(0) { AddRGB = Config.PulseColor2 - Config.MainColor },
                                  new(800) { AddRGB = Config.PulseColor - Config.MainColor },
                                  new(1600) { AddRGB = Config.PulseColor2 - Config.MainColor })
                                  { Ease = SinInOut, Repeat = true, Label = "BarPulse" };
    }

    protected override void StopMilestoneAnim()
    {
        Animator -= "BarPulse";
        MainContainer.SetAddRGB(0);
    }

    public override void RevealBar(bool instant = false)
    {
        Animator -= "Hide";
        if (instant)
        {
            Fan.SetAlpha(255);
            NumTextNode.SetAlpha(255);
        }
        else
        {
            Animator += new Tween[]
            {
                new(Fan,
                    new(0) { Alpha = 0, ScaleY = 1.2f, ScaleX = Config.Clockwise ? 1.2f : -1.2f },
                    new(150) { Alpha = 255, ScaleY = 1, ScaleX = Config.Clockwise ? 1f : -1f })
                    { Ease = SinInOut, Label = "Show" },
                new(NumTextNode,
                    Hidden[0],
                    Visible[180])
                    { Ease = SinInOut, Label = "Show" }
            };

        }
    }

    public override void HideBar(bool instant = false)
    {
        Animator -= "Show";
        if (instant)
        {
            Fan.SetAlpha(0);
            NumTextNode.SetAlpha(0);
        }
        else
        {
            Animator += new Tween[]
            {
                new(Fan,
                    new(0) { Alpha = 255, ScaleY = 1, ScaleX = Config.Clockwise ? 1f : -1f },
                    new(150) { Alpha = 0, ScaleY = 0.8f, ScaleX = Config.Clockwise ? 0.8f : -0.8f })
                    { Ease = SinInOut, Label ="Hide" },
                new(NumTextNode,
                    Visible[0],
                    Hidden[120])
                    { Ease = SinInOut, Label = "Hide" }
            };
        }
    }

    #endregion

    #region UpdateFuncs

    public override void OnDecreaseToMin() { if (Config.HideEmpty) HideBar(); }
    public override void OnIncreaseFromMin() { if (Config.HideEmpty) RevealBar(); }

    public override void OnIncreaseToMax() { if (Config.HideFull) HideBar(); }
    public override void OnDecreaseFromMax() { if (Config.HideFull) RevealBar(); }

    #endregion

    #region Configs

    public sealed class EspritBarConfig : GaugeBarWidgetConfig
    {
        public Vector2 Position;
        [DefaultValue(1f)] public float Scale = 1;
        [DefaultValue(true)] public bool ShowPlate = true;
        public float Angle;
        [DefaultValue(true)] public bool Clockwise = true;

        public AddRGB Backdrop = new(0, 0, 0);
        public ColorRGB FrameColor = new(100, 100, 100);
        public AddRGB MainColor = new(120, 0, -190);
        public AddRGB GainColor = new(-80, 30, 160);
        public AddRGB DrainColor = new(-50, -140, -70);
        public AddRGB PulseColor = new(280, 100, -110);
        public AddRGB PulseColor2 = new(0,-120,-255);

        public LabelTextProps LabelText = new(string.Empty, false, new(0, 32), new(255), 0x8E6A0CFF, Jupiter, 16, Left);
        protected override NumTextProps NumTextDefault => new(enabled: true,
                                                              position: new(0, 0),
                                                              color: new(255),
                                                              edgeColor: new(157, 131, 91),
                                                              showBg: true,
                                                              bgColor: new(0),
                                                              font: MiedingerMed,
                                                              fontSize: 18,
                                                              align: Center,
                                                              invert: false,
                                                              precision: 0,
                                                              showZero: true);

        public EspritBarConfig(WidgetConfig widgetConfig) : base(widgetConfig.EspritBarCfg)
        {
            var config = widgetConfig.EspritBarCfg;

            if (config == null) return;

            Position = config.Position;
            Scale = config.Scale;
            ShowPlate = config.ShowPlate;
            Clockwise = config.Clockwise;
            Angle = config.Angle;

            Backdrop = config.Backdrop;
            FrameColor = config.FrameColor;
            MainColor = config.MainColor;
            GainColor = config.GainColor;
            DrainColor = config.DrainColor;
            PulseColor = config.PulseColor;
            PulseColor2 = config.PulseColor2;

            LabelText = config.LabelText;
        }

        public EspritBarConfig() => MilestoneType = Above;
    }

    public override GaugeBarWidgetConfig GetConfig => Config;

    public EspritBarConfig Config;

    public override void InitConfigs()
    {
        Config = new(Tracker.WidgetConfig);
        if (Tracker.WidgetConfig.EspritBarCfg == null)
        {
            Config.MilestoneType = Above;
            if (ShouldInvertByDefault) Config.Invert = true;
        }
    }

    public override void ResetConfigs() => Config = new();

    public override void ApplyConfigs()
    {
        var flipFactor = Config.Clockwise ? 1 : -1;

        WidgetContainer.SetPos(Config.Position)
                  .SetScale(Config.Scale);

        Fan.SetScaleX(flipFactor)
           .SetRotation(Config.Angle,true);

        FanPlate.SetVis(Config.ShowPlate)
                .SetMultiply(Config.FrameColor);

        FanClip.SetVis(Config.ShowPlate)
               .SetMultiply(Config.FrameColor)
               .SetScaleX(flipFactor);

        Frame.SetMultiply(Config.FrameColor);

        Backdrop.SetAddRGB(Config.Backdrop, true);

        Main.SetAddRGB(Config.MainColor);
        Drain.SetAddRGB(Config.DrainColor);
        Gain.SetAddRGB(Config.GainColor);

        HandleMilestone(CalcProg(), true);

        NumTextNode.ApplyProps(Config.NumTextProps, new(100, 62));
    }

    public override void DrawUI(ref WidgetConfig widgetConfig, ref UpdateFlags update)
    {
        switch (UiTab)
        {
            case Layout:
                PositionControls("Position", ref Config.Position, ref update);
                ScaleControls("Scale", ref Config.Scale, ref update);
                ToggleControls("Fan Plate", ref Config.ShowPlate, ref update);
                AngleControls("Angle", ref Config.Angle, ref update);
                RadioIcons("Direction", ref Config.Clockwise, new() { true, false }, new() { RedoAlt, UndoAlt }, ref update);
                break;
            case Colors:

                ColorPickerRGBA("Backdrop", ref Config.Backdrop, ref update);
                ColorPickerRGB("Frame Tint", ref Config.FrameColor, ref update);
                ColorPickerRGBA("Main Bar", ref Config.MainColor, ref update);
                ColorPickerRGBA("Gain", ref Config.GainColor, ref update);
                ColorPickerRGBA("Drain", ref Config.DrainColor, ref update);
                break;
            case Behavior:
                SplitChargeControls(ref Config.SplitCharges, Tracker.RefType, Tracker.CurrentData.MaxCount, ref update);
                ToggleControls("Invert Fill", ref Config.Invert, ref update);
                HideControls(ref Config.HideEmpty, ref Config.HideFull, EmptyCheck, FullCheck, ref update);
                MilestoneControls("Pulse", ref Config.MilestoneType, ref Config.Milestone, ref update);
                if (Config.MilestoneType > 0)
                {
                    ColorPickerRGB("Pulse Colors", ref Config.PulseColor, ref update);
                    ColorPickerRGB(" ##Pulse2", ref Config.PulseColor2, ref update);
                }
                break;
            case Text:
                NumTextControls($"{Tracker.TermGauge} Text", ref Config.NumTextProps, ref update);
                break;
            default:
                break;
        }

        if (update.HasFlag(UpdateFlags.Save)) ApplyConfigs();
        widgetConfig.EspritBarCfg = Config;
    }

    #endregion
}

public partial class WidgetConfig
{
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] public EspritBarConfig? EspritBarCfg { get; set; }
}
