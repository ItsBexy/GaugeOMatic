using CustomNodes;
using FFXIVClientStructs.FFXIV.Component.GUI;
using GaugeOMatic.CustomNodes.Animation;
using GaugeOMatic.Trackers;
using GaugeOMatic.Windows;
using Newtonsoft.Json;
using System.Numerics;
using static CustomNodes.CustomNodeManager;
using static Dalamud.Interface.FontAwesomeIcon;
using static FFXIVClientStructs.FFXIV.Component.GUI.AlignmentType;
using static FFXIVClientStructs.FFXIV.Component.GUI.FontType;
using static GaugeOMatic.CustomNodes.Animation.KeyFrame;
using static GaugeOMatic.CustomNodes.Animation.Tween.Eases;
using static GaugeOMatic.Utility.Color;
using static GaugeOMatic.Widgets.EspritBar;
using static GaugeOMatic.Widgets.GaugeBarWidgetConfig;
using static GaugeOMatic.Widgets.MilestoneType;
using static GaugeOMatic.Widgets.NumTextProps;
using static GaugeOMatic.Widgets.WidgetTags;
using static GaugeOMatic.Widgets.WidgetUI;
using static System.Math;

#pragma warning disable CS8618

namespace GaugeOMatic.Widgets;

public sealed unsafe class EspritBar : GaugeBarWidget
{
    public EspritBar(Tracker tracker) : base(tracker) { }

    public override WidgetInfo WidgetInfo => GetWidgetInfo;

    public static WidgetInfo GetWidgetInfo => new() 
    {
        DisplayName = "Esprit Bar",
        Author = "ItsBexy",
        Description = "A curved bar based on DNC's Esprit Gauge",
        WidgetTags = GaugeBar
    };

    public override CustomPartsList[] PartsLists { get; } = {
        new ("ui/uld/JobHudDNC1.tex",
             new(0,0,168,76),    // 0  bar
             new(1,77,166,74),   // 1  backdrop
             new(168,0,48,108),  // 2  feather
             new(216,0,84,100),  // 3  half fan
             new(216,100,84,80), // 4  half frame
             new(168,108,48,32), // 5  corner clip thingy
             new(168,140,48,36), // 6  number bg
             new(2,160,76,60),   // 7  feather glow
             new(80,156,54,40),  // 8  spotlights
             new(79,198,54,40),  // 9  streaks
             new(132,153,20,20), // 10 star
             new(216,180,84,80)  // 11 half frame cover
            )};

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

    public override CustomNode BuildRoot()
    {
        Fan = BuildFan();
        NumTextNode = new();
        NumTextNode.SetAlpha(0);
        return new CustomNode(CreateResNode(), Fan, NumTextNode).SetSize(200,128).SetOrigin(100,113);
    }

    private CustomNode BuildFan()
    {
        FanPlate = ImageNodeFromPart(0, 3).SetImageWrap(3).SetPos(16, 22).SetSize(168, 100);
        BarContents = BuildBarContents();
        FanClip = ImageNodeFromPart(0, 5).SetPos(76, 90).SetSize(48, 32).SetImageWrap(1).SetOrigin(24,32);

        return new CustomNode(CreateResNode(), FanPlate, BarContents, FanClip).SetOrigin(100, 113).SetAlpha(0);
    }

    private CustomNode BuildBarContents()
    {
        Backdrop = ImageNodeFromPart(0, 1).SetPos(1, 3).SetSize(166, 74).SetImageWrap(2).SetOrigin(84, 0);
        FillNodes = BuildFillNodes();
        FrameCover = ImageNodeFromPart(0, 11).SetSize(168, 80).SetImageWrap(3).Hide(); // dunno whether/when to bother displaying this
        Frame = ImageNodeFromPart(0, 4).SetSize(168, 80).SetImageWrap(3);

        return new CustomNode(CreateResNode(), Backdrop, FillNodes, FrameCover, Frame).SetPos(16,22).SetSize(168,80);
    }

    private CustomNode BuildFillNodes()
    {
        CustomNode FillNode() => ImageNodeFromPart(0, 0).SetOrigin(84, 82)
                                                        .SetRotation(-151,true)
                                                        .SetDrawFlags(0xC)
                                                        .DefineTimeline(BarTimeline);

        static CustomNode FillContainer(CustomNode node) =>
            new CustomNode(CreateResNode(), node)
                .SetSize(168, 70)
                .SetNodeFlags(NodeFlags.Clip)
                .SetDrawFlags(0x200)
                .SetOrigin(84,70);

        Drain = FillNode();
        Gain = FillNode();
        Main = FillNode();

        MainContainer = FillContainer(Main);
        DrainContainer = FillContainer(Drain);
        GainContainer = FillContainer(Gain);

        return new CustomNode(CreateResNode(), DrainContainer, GainContainer, MainContainer).SetPos(0,1).SetSize(168,70).SetOrigin(0,-1);
    }

    #endregion

    #region Animations

    public static KeyFrame[] BarTimeline => new KeyFrame[] { new(0) { Rotation = -2.6542183675969f + 0.01f}, new(1) { Rotation = -0.05f } };

    protected override void StartMilestoneAnim()
    {
        Animator -= "BarPulse";
        Animator += new Tween(MainContainer,
                                  new(0) {AddRGB = Config.PulseColor2 - Config.MainColor },
                                  new(800) {AddRGB = Config.PulseColor - Config.MainColor},
                                  new(1600) { AddRGB = Config.PulseColor2 - Config.MainColor }) 
                                  { Ease = SinInOut, Repeat = true, Label = "BarPulse" };
    }

    protected override void StopMilestoneAnim()
    {
        Animator -= "BarPulse";
        MainContainer.SetAddRGB(0);
    }

    private void ShowBar()
    {
        Animator -= "Hide";
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

    private void HideBar()
    {
        Animator -= "Show";
        Animator += new Tween[]
        {
            new(Fan,
                new(0) { Alpha = 255, ScaleY = 1, ScaleX = Config.Clockwise ? 1f : -1f },
                new(150) { Alpha = 0, ScaleY = 0.8f, ScaleX = Config.Clockwise ? 0.8f : -0.8f })
                { Ease = SinInOut,Label="Hide" },
            new(NumTextNode,
                Visible[0],
                Hidden[120])
                { Ease = SinInOut, Label = "Hide" }
        };
    }

    #endregion

    #region UpdateFuncs

    public override void OnIncreaseFromMin() { if (Config.HideEmpty) ShowBar(); }
    public override void OnDecreaseToMin() { if (Config.HideEmpty) HideBar(); }

    public override void OnFirstRun(float prog)
    {
        base.OnFirstRun(prog);

        if (Config.HideEmpty && prog == 0)
        {
            Fan.SetAlpha(0);
            NumTextNode.SetAlpha(0);
        }
        else
        {
            Fan.SetAlpha(255);
            NumTextNode.SetAlpha(255);
        }
    }

    #endregion

    #region Configs

    public sealed class EspritBarConfig : GaugeBarWidgetConfig
    {

        public Vector2 Position;
        public float Scale = 1;
        public bool ShowPlate = true;

        public int Angle;
        public bool Clockwise = true;

        public AddRGB Backdrop = new(0, 0, 0);
        public ColorRGB FrameColor = new(100, 100, 100);
        public AddRGB MainColor = new(120, 0, -190);
        public AddRGB GainColor = new(-80, 30, 160);
        public AddRGB DrainColor = new(-50, -140, -70);
        public AddRGB PulseColor = new(280, 100, -110);
        public AddRGB PulseColor2 = new(0,-120,-255);

        public LabelTextProps LabelText = new(string.Empty, false, new(0, 32), new(255), 0x8E6A0CFF, Jupiter, 16, Left);
        protected override NumTextProps NumTextDefault => new(enabled: true,
                                                              position: new(0,0), 
                                                              color: new(255),
                                                              edgeColor: new(157,131,91), 
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
            if (Tracker.RefType == RefType.Action)
            {
                Config.Invert = true;
            }
        }
    }

    public override void ResetConfigs() => Config = new();

    public override void ApplyConfigs()
    {
        var flipFactor = Config.Clockwise ? 1 : -1;
        var containerSize = Config.Angle is 0 or 180 ? new Vector2(168, 70) : new(70, 168);
        var offsetX = Config.Clockwise == (Config.Angle >= 180) ? 168 : 0;
        var offsetY = Config.Angle is 90 or 180 ? 70 : 0;
        var textOffset = Config.Angle switch
        {
            90 => new Vector2(131, 113),
            180 => new(100, 164),
            270 => new(67, 113),
            _ => new (100, 62)
        };

        WidgetRoot.SetPos(Config.Position)
                  .SetScale(Config.Scale);

        Fan.SetScaleX(flipFactor)
           .SetRotation(Config.Angle);

        FanPlate.SetVis(Config.ShowPlate)
                .SetMultiply(Config.FrameColor);

        FanClip.SetVis(Config.ShowPlate)
               .SetMultiply(Config.FrameColor)
               .SetScaleX(flipFactor);

        Frame.SetMultiply(Config.FrameColor);

        Backdrop.SetAddRGB(Config.Backdrop,true);


        MainContainer.SetPos(offsetX, offsetY).SetSize(containerSize);
        DrainContainer.SetPos(offsetX, offsetY).SetSize(containerSize);
        GainContainer.SetPos(offsetX, offsetY).SetSize(containerSize);

        Main.SetAddRGB(Config.MainColor).SetPos(-offsetX, -offsetY);
        Drain.SetAddRGB(Config.DrainColor).SetPos(-offsetX, -offsetY);
        Gain.SetAddRGB(Config.GainColor).SetPos(-offsetX, -offsetY);

        HandleMilestone(CalcProg(),true);

        NumTextNode.ApplyProps(Config.NumTextProps,textOffset);
    }

    public override void DrawUI(ref WidgetConfig widgetConfig, ref UpdateFlags update)
    {
        Heading("Layout");

        PositionControls("Position", ref Config.Position, ref update);
        ScaleControls("Scale", ref Config.Scale, ref update);
        ToggleControls("Fan Plate", ref Config.ShowPlate, ref update);

        RadioIcons("Angle", ref Config.Angle, new() { 0,90,180,270 }, new() { ChevronUp, ChevronRight, ChevronDown, ChevronLeft }, ref update);
        RadioIcons("Direction",ref Config.Clockwise,new() {true,false}, new() { RedoAlt,UndoAlt}, ref update);

        Heading("Colors");

        ColorPickerRGBA("Backdrop", ref Config.Backdrop, ref update);
        ColorPickerRGB("Frame Tint", ref Config.FrameColor, ref update);
        ColorPickerRGBA("Main Bar", ref Config.MainColor, ref update);
        ColorPickerRGBA("Gain", ref Config.GainColor, ref update);
        ColorPickerRGBA("Drain", ref Config.DrainColor, ref update);

        if (Config.MilestoneType > 0)
        {
            ColorPickerRGB("Pulse Colors", ref Config.PulseColor, ref update);
            ColorPickerRGB(" ##Pulse2", ref Config.PulseColor2, ref update);
        }

        Heading("Behavior");

        SplitChargeControls(ref Config.SplitCharges, Tracker.RefType, Tracker.CurrentData.MaxCount, ref update);
        ToggleControls("Invert Fill", ref Config.Invert, ref update);
        if (ToggleControls("Hide Empty", ref Config.HideEmpty, ref update)) HideCheck(Config.HideEmpty);
        MilestoneControls("Pulse", ref Config.MilestoneType, ref Config.Milestone, ref update);

        NumTextControls($"{Tracker.TermGauge} Text", ref Config.NumTextProps, ref update);

        if (update.HasFlag(UpdateFlags.Save)) ApplyConfigs();
        widgetConfig.EspritBarCfg = Config;
    }

    public void HideCheck(bool hideEmpty)
    {
        if (Tracker.CurrentData.GaugeValue == 0 || (Config.Invert && Abs(Tracker.CurrentData.GaugeValue - Tracker.CurrentData.MaxGauge) < 0.01f))
        {
            if (hideEmpty) HideBar();
            else ShowBar();
        }
    }

    #endregion
}

public partial class WidgetConfig
{
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] public EspritBarConfig? EspritBarCfg { get; set; }
}
