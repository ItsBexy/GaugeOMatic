using CustomNodes;
using Dalamud.Interface;
using GaugeOMatic.CustomNodes.Animation;
using GaugeOMatic.Trackers;
using GaugeOMatic.Windows;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Numerics;
using static CustomNodes.CustomNodeManager;
using static Dalamud.Interface.FontAwesomeIcon;
using static FFXIVClientStructs.FFXIV.Component.GUI.AlignmentType;
using static FFXIVClientStructs.FFXIV.Component.GUI.FontType;
using static GaugeOMatic.CustomNodes.Animation.KeyFrame;
using static GaugeOMatic.CustomNodes.Animation.Tween.Eases;
using static GaugeOMatic.Utility.Color;
using static GaugeOMatic.Widgets.GaugeBarWidgetConfig;
using static GaugeOMatic.Widgets.LabelTextProps;
using static GaugeOMatic.Widgets.MilestoneType;
using static GaugeOMatic.Widgets.NumTextProps;
using static GaugeOMatic.Widgets.BeastBar;
using static GaugeOMatic.Widgets.WidgetTags;
using static GaugeOMatic.Widgets.WidgetUI;
using static System.Math;
using static GaugeOMatic.Widgets.Common.CommonParts;

#pragma warning disable CS8618

namespace GaugeOMatic.Widgets;

public sealed unsafe class BeastBar : GaugeBarWidget
{
    public BeastBar(Tracker tracker) : base(tracker) { }

    public override WidgetInfo WidgetInfo => GetWidgetInfo;

    public static WidgetInfo GetWidgetInfo =>
        new()
        {
            DisplayName = "Beast Bar",
            Author = "ItsBexy",
            Description = "A recreation of Warrior's Beast Gauge Bar",
            WidgetTags = GaugeBar | Replica
        };

    public override CustomPartsList[] PartsLists { get; } = { WAR0Parts };

    #region Nodes

    public CustomNode Contents;
    public CustomNode Bar;
    public CustomNode Backdrop;
    public CustomNode Frame;
    public CustomNode Glow;
    public LabelTextNode LabelTextNode;

    public override CustomNode BuildRoot()
    {
        NumTextNode = new();
        LabelTextNode = new(Config.LabelTextProps.Text, Tracker.DisplayName);

        Backdrop = NineGridFromPart(0, 2, 3, 3, 3, 3).SetSize(172, 22);
        Drain = NineGridFromPart(0, 1, 3, 3, 3, 3).SetAddRGB(-210, -210, 180).SetSize(0, 22).DefineTimeline(BarTimeline);
        Gain = NineGridFromPart(0, 1, 3, 3, 3, 3).SetAddRGB(100, 100, 100).SetSize(0, 22).DefineTimeline(BarTimeline);
        Main = NineGridFromPart(0, 1, 3, 3, 3, 3).SetSize(0, 22).DefineTimeline(BarTimeline);

        Bar = new CustomNode(CreateResNode(), Backdrop, Drain, Gain, Main).SetPos(36, 80).SetSize(172, 22).SetOrigin(86, 11);
        Frame = NineGridFromPart(0, 0, 20, 47, 32, 47).SetY(64);
        Glow = ImageNodeFromPart(0, 5).SetImageFlag(32).SetScale(1.14f).SetOrigin(108, 32).SetPos(12, 64).SetAlpha(0);

        Contents = new CustomNode(CreateResNode(), Bar, Frame, Glow).SetSize(242, 170).SetOrigin(122, 91);

        return new CustomNode(CreateResNode(), Contents, LabelTextNode, NumTextNode).SetOrigin(0, 60);
    }

    #endregion

    #region Animations

    public static KeyFrame[] BarTimeline => new KeyFrame[] { new(0) { Width = 6.2818f },
    new(1) { Width = 172 }};

    public void CollapseBar(int kf1, int kf2)
    {
        Animator -= "Expand";
        Animator += new Tween[]
        {
            new(Frame,
                new(0) { Height = 66, Y = 64, X = 0, Width = 242, AddRGB = 0, Alpha = 255 },
                new(kf1) { Height = 52, Y = 71, X = 0, Width = 242, AddRGB = Config.GainColor / 2, Alpha = 200 },
                new(kf2) { Height = 52, Y = 71, X = 23.5f, Width = 200, AddRGB = Config.GainColor, Alpha = 0 }) { Label = "Collapse" },
            new(Bar,
                new(0) { ScaleY = 1, Alpha = 255 },
                new(kf1) { ScaleY = 0.2727273F, Alpha = 0 })
                { Label = "Collapse" }
        };
    }

    public void ExpandBar(int kf1, int kf2)
    {
        Animator -= "Collapse";
        Animator += new Tween[]
        {
            new(Frame,
                new(0) { Height = 52, Y = 71, X = 23.5f, Width = 200, AddRGB = Config.GainColor, Alpha = 0 },
                new(kf1) { Height = 52, Y = 71, X = 0, Width = 242, AddRGB = Config.GainColor / 2, Alpha = 200 },
                new(kf2) { Height = 66, Y = 64, X = 0, Width = 242, AddRGB = 0, Alpha = 255 })
                { Label ="Expand" },
            new(Bar,
                new(0) { ScaleY = 0.2727273F, Alpha = 0 },
                new(kf1) { ScaleY = 0.2727273F, Alpha = 0 },
                new(kf2) { ScaleY = 1, Alpha = 255 })
                { Label ="Expand" }
        };
    }

    #endregion

    #region UpdateFuncs

    public override void OnDecreaseToMin()
    {
        if (Config.HideEmpty)
            CollapseBar(70, 140);
    }

    public override void OnIncrease(float prog, float prevProg) { }
    public override void OnDecrease(float prog, float prevProg) { }

    public override void OnIncreaseFromMin()
    {
        if (Config.HideEmpty)
            ExpandBar(70, 140);
    }

    public override void OnFirstRun(float prog)
    {
        base.OnFirstRun(prog);
        if (prog <= 0 && Config.HideEmpty)
            CollapseBar(0, 0);
    }

    protected override void StartMilestoneAnim()
    {
        Animator -= "BarPulse";

        var colorOffset = new AddRGB(-110, -2, 120);
        Animator += new Tween[]
        {
            new(Main,
                new(0) { AddRGB = Config.PulseColor + colorOffset },
                new(325) { AddRGB = Config.PulseColor2 + colorOffset },
                new(1140) { AddRGB = Config.PulseColor + colorOffset })
                { Ease = SinInOut, Repeat = true, Label = "BarPulse" },
            new(Glow,
                Hidden[0],
                new(325) { Alpha = 102 },
                Hidden[675],
                Hidden[1140])
                { Ease = SinInOut, Repeat = true, Label = "BarPulse" }
        };
    }

    protected override void StopMilestoneAnim()
    {
        Animator -= "BarPulse";
        Main.SetAddRGB(Config.MainColor + new AddRGB(-110, -2, 120));
        Animator += new Tween(Glow,
                              new(0, Glow),
                              Hidden[325]);
    }

    public override void PostUpdate(float prog, float prevProg)
    {
        if (Tracker.CurrentData.HasLabelOverride) LabelTextNode.SetLabelText(Tracker.CurrentData.LabelOverride ?? " ");
    }

    #endregion

    #region Configs

    public sealed class BeastBarConfig : GaugeBarWidgetConfig
    {
        public Vector2 Position;
        public float Scale = 1;
        public float Angle;

        public AddRGB BGColor = new(0, 0, 0);
        public ColorRGB FrameColor = new(100, 100, 100);
        public AddRGB MainColor = new(110, 2, -120);
        public AddRGB GainColor = new(210, 102, -20);
        public AddRGB DrainColor = new(-100, -208, 60);

        public AddRGB PulseColor = new(110, 2, -120);
        public AddRGB PulseColor2 = new(110, 2, -120);
        public AddRGB PulseColor3 = new(127, 46, -21);

        public bool Mirror;

        public LabelTextProps LabelTextProps = new(string.Empty, false, new(0, 0), "0xFF9C47FF", "0x360000FF", Jupiter, 22, Center);

        protected override NumTextProps NumTextDefault => new(enabled: true,
                                                              position: new(26, 0),
                                                              color: new(255),
                                                              edgeColor: new(0),
                                                              showBg: true,
                                                              bgColor: new(0, 0, 0, 0xcc),
                                                              font: MiedingerMed,
                                                              fontSize: 18,
                                                              align: Right,
                                                              invert: false);

        public BeastBarConfig(WidgetConfig widgetConfig) : base(widgetConfig.BeastBarCfg)
        {
            var config = widgetConfig.BeastBarCfg;

            if (config == null)
                return;

            Position = config.Position;
            Scale = config.Scale;
            Angle = config.Angle;

            BGColor = config.BGColor;
            FrameColor = config.FrameColor;
            MainColor = config.MainColor;
            GainColor = config.GainColor;
            DrainColor = config.DrainColor;

            PulseColor = config.PulseColor;
            PulseColor2 = config.PulseColor2;
            PulseColor3 = config.PulseColor3;

            Mirror = config.Mirror;

            LabelTextProps = config.LabelTextProps;
        }

        public BeastBarConfig() => MilestoneType = Above;
    }

    public override GaugeBarWidgetConfig GetConfig => Config;

    public BeastBarConfig Config;

    public override void InitConfigs()
    {
        Config = new(Tracker.WidgetConfig);
        if (Tracker.WidgetConfig.BeastBarCfg == null)
        {
            Config.MilestoneType = Above;
            if (Tracker.RefType == RefType.Action)
                Config.Invert = true;
        }
    }

    public override void ResetConfigs() => Config = new();

    public override void ApplyConfigs()
    {
        WidgetRoot.SetPos(Config.Position + new Vector2(-6, 0))
                  .SetScale(Config.Scale);

        Contents.SetRotation(Config.Angle, true);

        Bar.SetScaleX(Abs(Bar.ScaleX) * (Config.Mirror ? -1 : 1));

        var prog = CalcProg();

        var colorOffset = new AddRGB(-110, -2, 120);

        Main.SetAddRGB(Config.MainColor + colorOffset);
        Drain.SetAddRGB(Config.DrainColor + colorOffset).SetWidth(0);
        Gain.SetAddRGB(Config.GainColor + colorOffset).SetWidth(0);
        Backdrop.SetAddRGB(Config.BGColor, true);

        HandleMilestone(prog, true);

        Frame.SetMultiply(Config.FrameColor);
        Glow.SetAddRGB(Config.PulseColor3 + new AddRGB(-127, -46, 21));

        NumTextNode.ApplyProps(Config.NumTextProps, new Vector2(121, 111));

        LabelTextNode.ApplyProps(Config.LabelTextProps, new Vector2(36, 66))
                     .SetWidth(172);
    }

    public override void DrawUI(ref WidgetConfig widgetConfig, ref UpdateFlags update)
    {
        Heading("Layout");

        PositionControls("Position", ref Config.Position, ref update);
        ScaleControls("Scale", ref Config.Scale, ref update);
        AngleControls("Angle", ref Config.Angle, ref update);
        RadioIcons("Fill Direction", ref Config.Mirror, new() { false, true }, ArrowIcons, ref update);

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
            ColorPickerRGB(" ##Pulse3", ref Config.PulseColor3, ref update);
        }

        Heading("Behavior");

        SplitChargeControls(ref Config.SplitCharges, Tracker.RefType, Tracker.CurrentData.MaxCount, ref update);
        ToggleControls("Invert Fill", ref Config.Invert, ref update);
        if (ToggleControls("Collapse Empty", ref Config.HideEmpty, ref update))
            CollapseCheck(Config.HideEmpty);

        MilestoneControls("Pulse", ref Config.MilestoneType, ref Config.Milestone, ref update);

        NumTextControls($"{Tracker.TermGauge} Text", ref Config.NumTextProps, ref update);
        LabelTextControls("Label Text", ref Config.LabelTextProps, Tracker.DisplayName, ref update);

        if (update.HasFlag(UpdateFlags.Save))
            ApplyConfigs();
        widgetConfig.BeastBarCfg = Config;
    }

    private List<FontAwesomeIcon> ArrowIcons =>
        Abs(Config.Angle) > 135 ? new() { ArrowLeft, ArrowRight } :
        Config.Angle > 45 ? new() { ArrowDown, ArrowUp } :
        Config.Angle < -45 ? new() { ArrowUp, ArrowDown } :
                                       new List<FontAwesomeIcon> { ArrowRight, ArrowLeft };

    private void CollapseCheck(bool collapse)
    {
        if (Tracker.CurrentData.GaugeValue == 0 || (Config.Invert && Abs(Tracker.CurrentData.GaugeValue - Tracker.CurrentData.MaxGauge) < 0.01f))
        {
            if (collapse)
                CollapseBar(70, 140);
            else
                ExpandBar(70, 140);
        }
    }

    #endregion
}

public partial class WidgetConfig
{
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] public BeastBarConfig? BeastBarCfg { get; set; }
}
