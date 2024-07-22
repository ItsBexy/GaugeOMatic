using CustomNodes;
using GaugeOMatic.CustomNodes.Animation;
using GaugeOMatic.Trackers;
using GaugeOMatic.Windows;
using Newtonsoft.Json;
using System.Numerics;
using static CustomNodes.CustomNodeManager;
using static FFXIVClientStructs.FFXIV.Component.GUI.AlignmentType;
using static FFXIVClientStructs.FFXIV.Component.GUI.FontType;
using static GaugeOMatic.CustomNodes.Animation.Tween.EaseType;
using static GaugeOMatic.Utility.Color;
using static GaugeOMatic.Widgets.BatteryReplica;
using static GaugeOMatic.Widgets.Common.CommonParts;
using static GaugeOMatic.Widgets.GaugeBarWidgetConfig;
using static GaugeOMatic.Widgets.NumTextProps;
using static GaugeOMatic.Widgets.WidgetTags;
using static GaugeOMatic.Widgets.WidgetUI;
#pragma warning disable CS8618

namespace GaugeOMatic.Widgets;
public unsafe class BatteryReplica : GaugeBarWidget
{
    public BatteryReplica(Tracker tracker) : base(tracker)
    {
        SharedEvents.Add("StartBatteryGlow", args => BeginOverlayPulse(args?.AddRGB ?? new(0)));
        SharedEvents.Add("StopBatteryGlow", _ => EndOverlayPulse());
    }

    public override WidgetInfo WidgetInfo => GetWidgetInfo;

    public static WidgetInfo GetWidgetInfo => new()
    {
        DisplayName = "Battery Gauge",
        Author = "ItsBexy",
        Description = "A gauge bar based on Machinist's Battery Gauge",
        WidgetTags = GaugeBar | Replica | MultiComponent,
        MultiCompData = new("HT", "Heat Gauge Replica", 3)
    };

    public override CustomPartsList[] PartsLists { get; } = { MCH0 };

    #region Nodes

    public CustomNode Barrel;
    public CustomNode BatteryClock;
    public CustomNode ClockFace;
    public NumTextNode TabTextNode;
    public CustomNode Contents;

    public CustomNode Bar;
    public CustomNode Backdrop;

    public override CustomNode BuildContainer()
    {
        TabTextNode = new();
        TabTextNode.SetAlpha(0);

        Barrel = NineGridFromPart(0, 3, 0, 84, 0, 115).SetPos(31, 0);

        ClockFace = ImageNodeFromPart(0, 5);
        BatteryClock = new CustomNode(CreateResNode(), ClockFace).SetPos(-1, 6).SetOrigin(35, 30); //THERE'S MORE WE'LL DO IT LATER

        Bar = BuildBar();
        NumTextNode = new();

        Contents = new CustomNode(CreateResNode(), Barrel, BatteryClock, Bar).SetOrigin(34, 36);

        return new CustomNode(CreateResNode(),
                              Contents,
                              NumTextNode).SetSize(234, 94);
    }

    private CustomNode BuildBar()
    {
        Backdrop = NineGridFromPart(0, 10, 0, 18, 0, 18);
        Drain = NineGridFromPart(0, 12, 0, 18, 0, 18).SetAddRGB(-100).SetOrigin(10, 0).SetWidth(0);
        Gain = NineGridFromPart(0, 12, 0, 18, 0, 18).SetOrigin(10, 0).SetWidth(0).SetAddRGB(255);
        Main = NineGridFromPart(0, 12, 0, 18, 0, 18).SetOrigin(10, 0);

        return new CustomNode(CreateResNode(), Backdrop, Drain, Gain, Main).SetPos(61, 20).SetSize(148, 36);
    }

    #endregion

    #region Animations

    public virtual KeyFrame[] BarTimeline => new KeyFrame[] { new(0) { Width = 20 }, new(1) { Width = Config.Width }};

    protected override void StartMilestoneAnim()
    {
        Animator -= "BarPulse";
        StartPulse(Barrel);
        StartPulse(BatteryClock);
    }

    private void StartPulse(CustomNode target) =>
        Animator += new Tween(target,
                              new(0) { AddRGB = 0, MultRGB = new(90) },
                              new(400) { AddRGB = 30, MultRGB = new(100) },
                              new(800) { AddRGB = 0, MultRGB = new(90) })
                              { Ease = SinInOut, Repeat = true, Label = "BarPulse" };

    protected override void StopMilestoneAnim()
    {
        Animator -= "BarPulse";

        StopPulse(Barrel);
        StopPulse(BatteryClock);
    }

    private void StopPulse(CustomNode target) =>
        Animator += new Tween(target,
                              new(0, target),
                              new(400) { AddRGB = 0, MultRGB = new(100) })
                              { Ease = SinInOut, Label = "BarPulse" };

    private void EndOverlayPulse()
    {
        Animator -= "OverlayPulse";
        Animator += new Tween[]
        {
            new(ClockFace,
                new(0, ClockFace),
                new(400) { AddRGB = new(0) })
                { Ease = SinInOut, Label = "OverlayPulse" },
            new(Barrel,
                new(0, ClockFace),
                new(400) { AddRGB = new(0) })
                { Ease = SinInOut, Label = "OverlayPulse" }
        };
    }

    private void BeginOverlayPulse(AddRGB addRGB)
    {
        Animator -= "OverlayPulse";
        Animator += new Tween(ClockFace,
                              new(0) { AddRGB = new(0) },
                              new(400) { AddRGB = addRGB },
                              new(780) { AddRGB = new(0) })
                              { Repeat = true, Ease = SinInOut, Label = "OverlayPulse" };
        Animator += new Tween(Barrel,
                              new(0) { AddRGB = new(0) },
                              new(400) { AddRGB = addRGB },
                              new(780) { AddRGB = new(0) })
                              { Repeat = true, Ease = SinInOut, Label = "OverlayPulse" };
    }

    #endregion

    #region UpdateFuncs

    public override string SharedEventGroup => "HeatGauge";

    public override void PostUpdate(float prog, float prevProg)
    {
        Main.SetAlpha(Main.Width > 20);

        var orange = Config.BaseColor == 12;
        var gainA = orange ? Config.GainColorOrange.A : Config.GainColorBlue.A;
        var drainA = orange ? Config.DrainColorOrange.A : Config.DrainColorBlue.A;

        var gainDiff = Gain.Width - Main.Width;
        var drainDiff = Drain.Width - Main.Width;

        Gain.SetAlpha(gainDiff switch
        {
            <= 0 => 0,
            >= 25 => gainA,
            _ => (byte)(gainDiff * (gainA/25f))
        });

        Drain.SetAlpha(drainDiff switch
        {
            <= 0 => 0,
            >= 25 => drainA,
            _ => (byte)(drainDiff * (drainA / 25f))
        });
    }

    #endregion

    #region Configs

    public sealed class BatteryReplicaConfig : GaugeBarWidgetConfig
    {
        public Vector2 Position;
        public float Scale = 1;
        public float Width = 148;
        public float Angle;

        public ushort BaseColor = 11;
        public AddRGB BackdropColor;

        public AddRGB MainColorOrange = new(91, 52,-27);
        public AddRGB GainColorOrange = new(346, 307, 228);
        public AddRGB DrainColorOrange = new(-9,-48,-127);

        public AddRGB MainColorBlue = new(-38, 100, 116);
        public AddRGB GainColorBlue = new(217, 355, 371);
        public AddRGB DrainColorBlue = new(-138, 0, 16);

        protected override NumTextProps NumTextDefault => new(enabled: true,
                                                              position: new(0),
                                                              color: new(255),
                                                              edgeColor: "0x9d835bff",
                                                              showBg: true,
                                                              bgColor: new(0),
                                                              fontSize: 18,
                                                              invert: false,
                                                              showZero: false,
                                                              precision: 0,
                                                              font: MiedingerMed,
                                                              align: Right);

        public BatteryReplicaConfig(WidgetConfig widgetConfig) : base(widgetConfig.BatteryReplicaCfg)
        {
            var config = widgetConfig.BatteryReplicaCfg;

            if (config == null) return;

            Position = config.Position;
            Scale = config.Scale;
            Width = config.Width;
            Angle = config.Angle;

            BackdropColor = config.BackdropColor;

            MainColorOrange = config.MainColorOrange;
            GainColorOrange = config.GainColorOrange;
            DrainColorOrange = config.DrainColorOrange;

            MainColorBlue = config.MainColorBlue;
            GainColorBlue = config.GainColorBlue;
            DrainColorBlue = config.DrainColorBlue;
        }

        public BatteryReplicaConfig() { }
    }

    public BatteryReplicaConfig Config;
    public override GaugeBarWidgetConfig GetConfig => Config;

    public override void InitConfigs()
    {
        Config = new(Tracker.WidgetConfig);
        if (Tracker.WidgetConfig.BatteryReplicaCfg == null && Tracker.RefType == RefType.Action) { Config.Invert = true; }
    }

    public override void ResetConfigs() => Config = new();

    public override void ApplyConfigs()
    {
        WidgetContainer.SetPos(Config.Position)
                  .SetScale(Config.Scale);

        Contents.SetRotation(Config.Angle, true);
        BatteryClock.SetRotation(-Config.Angle, true);

        Barrel.SetWidth(Config.Width + 52);
        Backdrop.SetWidth(Config.Width);

        var orange = Config.BaseColor == 12;
        AddRGB colorOffset = orange ? new(-91, -52, 27) : new(38, -100, -116);

        Main.SetPartId(Config.BaseColor)
            .SetAddRGB(colorOffset + (orange ? Config.MainColorOrange : Config.MainColorBlue), true)
            .DefineTimeline(BarTimeline)
            .SetWidth(AdjustProg(CalcProg()));

        Drain.SetPartId(Config.BaseColor)
             .SetAddRGB(colorOffset + (orange ? Config.DrainColorOrange : Config.DrainColorBlue), true)
             .DefineTimeline(BarTimeline)
             .SetWidth(0);

        Gain.SetPartId(Config.BaseColor)
            .SetAddRGB(colorOffset + (orange ? Config.GainColorOrange : Config.GainColorBlue), true)
            .DefineTimeline(BarTimeline)
            .SetWidth(0);

        NumTextNode.ApplyProps(Config.NumTextProps, new(Config.Width+6, 74));
    }

    public override void DrawUI(ref WidgetConfig widgetConfig, ref UpdateFlags update)
    {
        Heading("Layout");

        PositionControls("Position", ref Config.Position, ref update);
        ScaleControls("Scale", ref Config.Scale, ref update);
        FloatControls("Width", ref Config.Width, 148, 1000, 1, ref update);
        AngleControls("Angle", ref Config.Angle, ref update);

        Heading("Colors");
        RadioControls("Base Color", ref Config.BaseColor, new() { 11, 12 }, new() {"Blue", "Orange" }, ref update, true);

        if (Config.BaseColor == 12)
        {
            ColorPickerRGBA("Main Bar", ref Config.MainColorOrange, ref update);
            ColorPickerRGBA("Gain", ref Config.GainColorOrange, ref update);
            ColorPickerRGBA("Drain", ref Config.DrainColorOrange, ref update);
        }
        else
        {
            ColorPickerRGBA("Main Bar", ref Config.MainColorBlue, ref update);
            ColorPickerRGBA("Gain", ref Config.GainColorBlue, ref update);
            ColorPickerRGBA("Drain", ref Config.DrainColorBlue, ref update);
        }

        MilestoneControls("Pulse", ref Config.MilestoneType, ref Config.Milestone, ref update);
        NumTextControls($"{Tracker.TermGauge} Text", ref Config.NumTextProps, ref update);

        if (update.HasFlag(UpdateFlags.Save)) ApplyConfigs();
        widgetConfig.BatteryReplicaCfg = Config;
    }

    #endregion
}

public partial class WidgetConfig
{
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] public BatteryReplicaConfig? BatteryReplicaCfg { get; set; }
}

