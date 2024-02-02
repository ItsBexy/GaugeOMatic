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
using static GaugeOMatic.CustomNodes.Animation.Tween.Eases;
using static GaugeOMatic.Utility.Color;
using static GaugeOMatic.Widgets.GaugeBarWidgetConfig;
using static GaugeOMatic.Widgets.LabelTextProps;
using static GaugeOMatic.Widgets.NumTextProps;
using static GaugeOMatic.Widgets.SimpleBar;
using static GaugeOMatic.Widgets.WidgetTags;
using static GaugeOMatic.Widgets.WidgetUI;
using static System.Math;

#pragma warning disable CS8618

namespace GaugeOMatic.Widgets;

public sealed unsafe class SimpleBar : GaugeBarWidget
{
    public SimpleBar(Tracker tracker) : base(tracker) { }

    public override WidgetInfo WidgetInfo => GetWidgetInfo;

    public static WidgetInfo GetWidgetInfo => new() 
    {
        DisplayName = "Simple Bar",
        Author = "ItsBexy",
        Description = "A bar in the style of the Simple job gauges.",
        WidgetTags = GaugeBar
    };

    public override CustomPartsList[] PartsLists { get; } = {
        new ("ui/uld/Parameter_Gauge.tex",
             new(0, 0, 160, 20),  // 0 frame
             new(0, 20, 160, 20), // 1 glow
             new(0, 40, 160, 20), // 2 bw bar
             new(0, 60, 160, 20), // 3 blue bar
             new(0, 80, 160, 20), // 4 orange bar
             new(0, 100, 160, 20) // 5 brown bar
        )
    };

    #region Nodes

    public CustomNode BarFrame;
    public CustomNode Bar;
    public CustomNode Frame;
    public CustomNode Backdrop;
    public LabelTextNode LabelTextNode;

    public override CustomNode BuildRoot()
    {
        Drain = NineGridFromPart(0, 4, 0, 10, 0, 10);
        Gain = NineGridFromPart(0, 3, 0, 10, 0, 10);
        Main = NineGridFromPart(0, 2, 0, 10, 0, 10);
        Backdrop = NineGridFromPart(0, 5, 0, 10, 0, 10);
        Frame = NineGridFromPart(0, 0, 0, 15, 0, 15);
        NumTextNode = new();
        LabelTextNode = new(Config.LabelTextProps.Text, Tracker.DisplayName);

        Bar = new(CreateResNode(), Drain, Gain, Main);
        BarFrame = new(CreateResNode(), Backdrop, Bar, Frame);

        return new CustomNode(CreateResNode(), NumTextNode, BarFrame, LabelTextNode).SetOrigin(0, 10);
    }


    #endregion

    #region Animations

    public KeyFrame[] BarTimeline => new KeyFrame[] { new(0) { Width = 13 }, new(1) { Width = Config.Width }};

    public void HideBar(int time)
    {
        Animator -= "Fade";
        Animator += new Tween(WidgetRoot, new(0, WidgetRoot), Hidden[time]) { Label ="Fade", Ease = SinInOut };

    }

    public void ShowBar(int time)
    {
        Animator -= "Fade";
        Animator += new Tween(WidgetRoot, new(0, WidgetRoot), Visible[time]) { Label = "Fade", Ease = SinInOut };
    }

    #endregion

    #region UpdateFuncs

    public override void OnDecreaseToMin() { if (Config.HideEmpty) HideBar(250); }

    public override void OnIncreaseFromMin() { if (Config.HideEmpty) ShowBar(250); }

    protected override void StartMilestoneAnim()
    {
        Animator -= "BarPulse";
        var colorFrame1 = new KeyFrame { AddRGB = Config.PulseColor  };
        var colorFrame2 = new KeyFrame { AddRGB = Config.PulseColor2 };
        Animator += new Tween(Main, 
                              colorFrame1[0],
                              colorFrame2[800],
                              colorFrame1[1600]) 
                              { Ease = SinInOut, Repeat = true, Label = "BarPulse" };

    }

    protected override void StopMilestoneAnim()
    {
        Animator -= "BarPulse";
        Main.SetAddRGB(Config.MainColor);
    }

    #endregion

    #region Configs

    public sealed class SimpleBarConfig : GaugeBarWidgetConfig
    {
        public Vector2 Position;
        public float Scale = 1;
        public float Width = 160;
        public float Angle;

        public AddRGB BGColor = new(0, 0, 0, 229);
        public ColorRGB FrameColor = new(100, 100, 100);
        public AddRGB MainColor = "0xaaaaaaff";
        public AddRGB GainColor = new(-96, 100, 123);
        public AddRGB DrainColor = new(127, 42, -126);

        public AddRGB PulseColor = "0xE39A63ff";
        public AddRGB PulseColor2 = "0xD16347FF";
        
        public LabelTextProps LabelTextProps = new(string.Empty, false, new(0, 0), new(255), new(0), MiedingerMed, 18, Center);
        protected override NumTextProps NumTextDefault => new(enabled:   true, 
                                                              position:  new(0, 0), 
                                                              color:     new(255), 
                                                              edgeColor: "0x9f835bff", 
                                                              showBg:    true, 
                                                              bgColor:   new(0), 
                                                              font:      MiedingerMed, 
                                                              fontSize:  18, 
                                                              align:     Right, 
                                                              showZero:  true,
                                                              invert:    false);

        public SimpleBarConfig(WidgetConfig widgetConfig) : base(widgetConfig.SimpleBarCfg)
        {
            var config = widgetConfig.SimpleBarCfg;

            if (config == null) return;

            Position = config.Position;
            Scale = config.Scale;
            Width = config.Width;
            Angle = config.Angle;

            BGColor = config.BGColor;
            FrameColor = config.FrameColor;
            MainColor = config.MainColor;
            GainColor = config.GainColor;
            DrainColor = config.DrainColor;

            PulseColor = config.PulseColor;
            PulseColor2 = config.PulseColor2;

            LabelTextProps = config.LabelTextProps;
        }

        public SimpleBarConfig() { }
    }

    public override GaugeBarWidgetConfig GetConfig => Config;

    public SimpleBarConfig Config;

    public override void InitConfigs()
    {
        Config = new(Tracker.WidgetConfig);
        if (Tracker.WidgetConfig.SimpleBarCfg == null && Tracker.RefType == RefType.Action) { Config.Invert = true; }
    }

    public override void ResetConfigs() => Config = new();

    public override void ApplyConfigs()
    {
        WidgetRoot.SetPos(Config.Position + new Vector2(48, 27))
                  .SetScale(Config.Scale);

        var flipFactor = Abs(Config.Angle) > 90 ? -1:1;

        Frame.SetWidth(Config.Width)
             .SetOrigin(Config.Width/2, 10)
             .SetScaleX(flipFactor)
             .SetMultiply(Config.FrameColor);

        Backdrop.SetWidth(Config.Width).SetAddRGB(Config.BGColor, true);
        BarFrame.SetX(Config.Width / -2)
                .SetOrigin(Config.Width / 2, 10)
                .SetRotation(Config.Angle, true)
                .SetScaleY(flipFactor);

        var prog = CalcProg();
        HandleMilestone(prog, true);
        Main.SetMultiply(80, 75, 80).DefineTimeline(BarTimeline).SetProgress(prog);
        Gain.SetAddRGB(Config.GainColor + new AddRGB(96, -100, -123)).SetWidth(0).DefineTimeline(BarTimeline);
        Drain.SetAddRGB(Config.DrainColor + new AddRGB(-127, -42, 126)).SetWidth(0).DefineTimeline(BarTimeline);


        NumTextNode.ApplyProps(Config.NumTextProps, new((Config.Width/2)-71, 26));
        LabelTextNode.ApplyProps(Config.LabelTextProps, new(Config.Width / -2, -7))
                     .SetWidth(Config.Width);
    }

    public override void DrawUI(ref WidgetConfig widgetConfig, ref UpdateFlags update)
    {
        Heading("Layout");

        PositionControls("Position", ref Config.Position, ref update);
        ScaleControls("Scale", ref Config.Scale, ref update);
        FloatControls("Width", ref Config.Width, 50, 2000, 1, ref update);
        FloatControls("Angle", ref Config.Angle, -180, 180, 1f, ref update);

        Heading("Colors");

        ColorPickerRGBA("Backdrop", ref Config.BGColor, ref update);
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
        LabelTextControls("Label Text", ref Config.LabelTextProps, Tracker.DisplayName, ref update);

        if (update.HasFlag(UpdateFlags.Save)) ApplyConfigs();
        widgetConfig.SimpleBarCfg = Config;
    }

    private void HideCheck(bool hide)
    {
        if (Tracker.CurrentData.GaugeValue == 0 || (Config.Invert && Abs(Tracker.CurrentData.GaugeValue - Tracker.CurrentData.MaxGauge) < 0.01f))
        {
            if (hide) HideBar(250);
            else ShowBar(250);
        }
    }

    #endregion
}

public partial class WidgetConfig
{
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] public SimpleBarConfig? SimpleBarCfg { get; set; }
}
