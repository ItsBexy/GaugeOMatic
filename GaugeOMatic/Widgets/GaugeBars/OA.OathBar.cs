using CustomNodes;
using Dalamud.Interface;
using GaugeOMatic.CustomNodes.Animation;
using GaugeOMatic.Trackers;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel;
using System.Numerics;
using static CustomNodes.CustomNode;
using static CustomNodes.CustomNodeManager;
using static Dalamud.Interface.FontAwesomeIcon;
using static FFXIVClientStructs.FFXIV.Component.GUI.AlignmentType;
using static FFXIVClientStructs.FFXIV.Component.GUI.FontType;
using static GaugeOMatic.CustomNodes.Animation.KeyFrame;
using static GaugeOMatic.CustomNodes.Animation.Tween.EaseType;
using static GaugeOMatic.Utility.Color;
using static GaugeOMatic.Widgets.GaugeBarWidgetConfig;
using static GaugeOMatic.Widgets.LabelTextProps;
using static GaugeOMatic.Widgets.MilestoneType;
using static GaugeOMatic.Widgets.NumTextProps;
using static GaugeOMatic.Widgets.OathBar;
using static GaugeOMatic.Widgets.WidgetTags;
using static GaugeOMatic.Widgets.WidgetUI;
using static GaugeOMatic.Widgets.WidgetUI.WidgetUiTab;
using static System.Math;

#pragma warning disable CS8618

namespace GaugeOMatic.Widgets;

[WidgetName("Oath Bar")]
[WidgetDescription("A recreation of Paladin's Oath Gauge Bar.")]
[WidgetAuthor("ItsBexy")]
[WidgetTags(GaugeBar | Replica | MultiComponent)]
[MultiCompData("OA", "Oath Gauge Replica", 2)]
public sealed unsafe class OathBar : GaugeBarWidget
{
    public OathBar(Tracker tracker) : base(tracker) { }

    public override CustomPartsList[] PartsLists { get; } = {
        new ("ui/uld/JobHudPLD.tex",
             new(246, 0, 168, 18),    // 0 blue bar
             new(246, 18, 168, 18),   // 1 grey bar
             new(414, 0, 34, 60),     // 2 tickmark
             new(0, 0, 246, 120),     // 3 full frame
             new(0, 0, 123, 120),     // 4 half frame L
             new(123, 0, 123, 120),   // 5 half frame R
             new(0, 0, 64, 120),      // 6 endcap L
             new(182, 0, 64, 120),    // 7 endcap R
             new(0, 301, 238, 120),   // 8 full glow
             new(0, 301, 123, 120),   // 9 half glow L
             new(123, 301, 118, 120), // 10 half glow R
             new(0, 301, 64, 120),    // 11 glow endcap L
             new(182, 301, 58, 120),  // 12 glow endcap R
             new(238, 300, 50, 50),   // 13 halo
             new(246, 36, 102, 102),  // 14 shine
             new(180, 274, 190, 26)   // 15 streak
            )
    };

    #region Nodes

    public CustomNode Contents;
    public CustomNode Bar;
    public CustomNode Backdrop;
    public CustomNode Frame;
    public CustomNode FrameL;
    public CustomNode FrameR;
    public LabelTextNode LabelTextNode;
    public CustomNode Glow;
    public CustomNode GlowL;
    public CustomNode GlowR;
    public CustomNode TickMark;
    public CustomNode Effects;
    public CustomNode Halo;
    public CustomNode Shine;
    public CustomNode Streak;

    public override Bounds GetBounds() => new(FrameL, FrameR);

    public override CustomNode BuildContainer()
    {

        Frame = BuildFrame();
        Bar = BuildBar();
        Glow = BuildGlow();

        Halo = ImageNodeFromPart(0, 13).SetPos(74, 26).SetOrigin(25, 25).SetAlpha(0).SetImageFlag(32);
        Shine = ImageNodeFromPart(0, 14).SetPos(48, 0).SetOrigin(51, 51).SetAlpha(0).SetImageFlag(32);
        Streak = ImageNodeFromPart(0, 15).SetPos(0, 36).SetOrigin(98, 13).SetAlpha(0).SetImageFlag(32);

        Effects = new CustomNode(CreateResNode(), Halo, Shine, Streak).SetPos(-98, 11);

        NumTextNode = new();
        LabelTextNode = new(Config.LabelTextProps.Text, Tracker.DisplayAttr.Name);
        LabelTextNode.SetWidth(162);

        Contents = new CustomNode(CreateResNode(), Frame, Bar, Glow, Effects).SetOrigin(0, 60);

        return new CustomNode(CreateResNode(), NumTextNode, Contents, LabelTextNode).SetOrigin(0, 60);
    }

    private CustomNode BuildFrame()
    {
        FrameL = NineGridFromPart(0, 4, 0, 59, 0, 48).SetX(-123);
        FrameR = NineGridFromPart(0, 5, 0, 48, 0, 59);

        return new(CreateResNode(), FrameL, FrameR);
    }

    private CustomNode BuildGlow()
    {
        GlowL = NineGridFromPart(0, 9, 0, 59, 0, 48).SetX(-123).SetNineGridBlend(2);
        GlowR = NineGridFromPart(0, 10, 0, 42, 0, 59).SetNineGridBlend(2);

        return new CustomNode(CreateResNode(), GlowL, GlowR).SetX(4).SetOrigin(0, 52).SetAlpha(0);
    }

    private CustomNode BuildBar()
    {
        Backdrop = NineGridFromPart(0, 1, 0, 16, 0, 16);
        Drain = NineGridFromPart(0, 0, 0, 16, 0, 16);
        Gain = NineGridFromPart(0, 0, 0, 16, 0, 16);
        Main = NineGridFromPart(0, 0, 0, 16, 0, 16);

        TickMark = new CustomNode(CreateResNode(),
            ImageNodeFromPart(0, 2).SetScale(1, 0.8f).SetOrigin(17, 30),
            ImageNodeFromPart(0, 2).SetScale(1, 0.8f)
            ).SetY(-16);

        Animator += new Tween(TickMark[0],
                              new(0) { ScaleX = 0.2f, Alpha = 0 },
                              new(130) { ScaleX = 1, Alpha = 127 },
                              new(260) { ScaleX = 1.2f, Alpha = 127 },
                              new(460) { ScaleX = 0.2f, Alpha = 0 })
                              { Repeat = true, Ease = SinInOut };

        return new CustomNode(CreateResNode(), Backdrop, Drain, Gain, Main, TickMark).SetY(51);
    }

    #endregion

    #region Animations

    public KeyFrame[] BarTimeline => new KeyFrame[] { new(0) { Width = 0 }, new(1) { Width = Config.Width }};

    public void Twinkle() =>
        Animator += new Tween[] {
            new(Halo,
                new(0) { Scale = 1, Alpha = 0 },
                new(230) { Scale = 3, Alpha = 125 },
                new(470) { Scale = 0, Alpha = 0 })
                { Ease = SinInOut },
            new(Shine,
                new(0) { Scale = 1, Alpha = 0, Rotation = 0 },
                new(230) { Scale = 2, Alpha = 255, Rotation = 6.283185f },
                new(470) { Scale = 1, Alpha = 0, Rotation = 0 })
                { Ease = SinInOut },
            new(Streak,
                new(0) { ScaleX = Config.Width / 166f, Alpha = 0 },
                new(120) { ScaleX = Config.Width / 166f, Alpha = 255 },
                new(290) { ScaleX = 0, Alpha = 0 })
                { Ease = SinInOut }
        };

    public override void HideBar(bool instant = false)
    {
        var kf = instant ? new[] { 0, 0, 0 } : new[] { 0, 150, 350 };

        Animator -= "Expand";
        Animator += new Tween[]
        {
            new(FrameR,
                new(kf[0]) { Height = 120, Y = 0 },
                new(kf[1]) { Height = 96, Y = 12 },
                new(kf[2]) { Height = 96, Y = 12 })
                { Ease = SinInOut, Label = "Collapse" },
            new(FrameL,
                new(kf[0]) { Height = 120, Y = 0 },
                new(kf[1]) { Height = 96, Y = 12 },
                new(kf[2]) { Height = 96, Y = 12 })
                { Ease = SinInOut, Label = "Collapse" },
            new(Bar,
                Visible[kf[0]],
                Hidden[kf[1] / 2])
                { Ease = SinInOut, Label = "Collapse" },
            new(Frame,
                new(kf[0]) { AddRGB = 0, Alpha = 255, ScaleX = 1 },
                new(kf[1]) { AddRGB = 120, Alpha = 255, ScaleX = 1 },
                new(kf[2]) { AddRGB = 250, Alpha = 0, ScaleX = 1.2f }),
            new(LabelTextNode,
                Visible[kf[0]],
                Hidden[kf[1]])
                {Label = "Collapse" },
            new(TickMark, Visible[kf[0]], Hidden[kf[1]])
};

        if (kf[2] > 0) Twinkle();
    }

    public override void RevealBar(bool instant = false)
    {
        var kf = instant ? new[] { 0, 0, 0 } : new[] { 0, 100, 350 };

        Animator -= "Collapse";
        Animator += new Tween[]
        {
            new(FrameR,
                new(kf[0]) { Height = 94, Y = 13 },
                new(kf[1]) { Height = 120, Y = 0 },
                new(kf[2]) { Height = 120, Y = 0 })
                { Ease = SinInOut, Label = "Expand" },
            new(FrameL,
                new(kf[0]) { Height = 94, Y = 13 },
                new(kf[1]) { Height = 120, Y = 0 },
                new(kf[2]) { Height = 120, Y = 0 })
                { Ease = SinInOut, Label = "Expand" },
            new(Bar,
                Hidden[kf[0]],
                Hidden[kf[1]],
                Visible[kf[2]])
                { Ease = SinInOut, Label = "Expand" },
            new(Frame,
                new(kf[0]) { AddRGB = 200, Alpha = 0, ScaleX = 0.8f },
                new(kf[1]) { AddRGB = 80, Alpha = 255, ScaleX = 1 },
                new(kf[2]) { AddRGB = 0, Alpha = 255, ScaleX = 1 })
                { Ease = SinInOut, Label = "Expand" },
            new(LabelTextNode,
                Hidden[kf[0]],
                Hidden[kf[1]],
                Visible[kf[2]])
                {Label = "Expand" },
            new(TickMark, Hidden[kf[0]], Visible[kf[1]])
};

        if (kf[2] > 0) Twinkle();
    }

    #endregion

    #region UpdateFuncs

    public override void OnIncrease(float prog, float prevProg) { if (Config.TwinkleInc) Twinkle(); }
    public override void OnDecrease(float prog, float prevProg) { if (Config.TwinkleDec) Twinkle(); }

    protected override void StartMilestoneAnim()
    {
        Animator -= "BarPulse";

        var halfFrame = (float)Ceiling(Config.Width / 2) + 40;
        var endX = (halfFrame + 15f) / halfFrame;
        var midX = Interpolate(1f, endX, 0.5f);

        var colorAdjust = new AddRGB(86, 47, -21);

        Animator += new Tween[] {
            new(Glow,
                new(0) { ScaleX = 1, ScaleY = 1, Alpha = 0 },
                new(460) { ScaleX = midX, ScaleY = 1.1f, Alpha = 203 },
                new(960) { ScaleX = endX, ScaleY = 1.2f, Alpha = 0 },
                new(1600) { ScaleX = endX, ScaleY = 1.2f, Alpha = 0 })
                { Repeat = true, Label = "BarPulse" },

            new(Main,
                new(0) { AddRGB = Config.PulseColor2 + colorAdjust },
                new(800) { AddRGB = Config.PulseColor + colorAdjust },
                new(1600) { AddRGB = Config.PulseColor2 + colorAdjust })
                { Ease = SinInOut, Repeat = true, Label = "BarPulse" }
        };
    }

    protected override void StopMilestoneAnim()
    {
        var halfFrame = (float)Ceiling(Config.Width / 2) + 40;
        var endX = (halfFrame + 15f) / halfFrame;

        Animator -= "BarPulse";
        Animator += new Tween(Glow,
                              new(0, Glow),
                              new(200) { ScaleX = endX, ScaleY = 1.2f, Alpha = 0 })
                              { Label = "BarPulse" };

        var colorAdjust = new AddRGB(86, 47, -21);
        Main.SetAddRGB(Config.MainColor + colorAdjust);
    }

    public override void PlaceTickMark(float prog)
    {
        TickMark.SetX(Main.Width - 19);
    }

    public override void PostUpdate(float prog)
    {
        if (Tracker.CurrentData.HasLabelOverride) LabelTextNode.SetLabelText(Tracker.CurrentData.LabelOverride ?? " ");
    }

    #endregion

    #region Configs

    public sealed class OathBarConfig : GaugeBarWidgetConfig
    {
        [DefaultValue(166)] public float Width = 166;
        public float Angle;
        [DefaultValue(true)] public bool ShowFiligree = true;

        public AddRGB BGColor = new(0, 0, 0, 229);
        public ColorRGB FrameColor = new(100, 100, 100);
        public AddRGB MainColor = new(-86, -47, 21);
        public AddRGB GainColor = new(114, 153, 221);
        public AddRGB DrainColor = new(-186, -147, -79);
        public ColorRGB TickmarkColor = new(255);
        public ColorRGB EffectsColor = new(255);

        public AddRGB PulseColor = new(-86, -47, 21);
        public AddRGB PulseColor2 = new(-86, -47, 21);
        public AddRGB PulseColor3 = new(255);

        public bool TwinkleInc = true;
        public bool TwinkleDec;
        public bool Mirror;

        public LabelTextProps LabelTextProps = new(string.Empty, false, new(0, 34), new(255), new(0), Jupiter, 22, Center);
        protected override NumTextProps NumTextDefault => new(enabled: true,
                                                              position: new(64, 0),
                                                              color: new(255),
                                                              edgeColor: new(0),
                                                              showBg: true,
                                                              bgColor: new(0, 0, 0, 0xcc),
                                                              font: MiedingerMed,
                                                              fontSize: 18,
                                                              align: Center,
                                                              invert: false);

        public OathBarConfig(WidgetConfig widgetConfig) : base(widgetConfig.OathBarCfg)
        {
            var config = widgetConfig.OathBarCfg;

            if (config == null) return;

            Width = config.Width;
            Angle = config.Angle;
            ShowFiligree = config.ShowFiligree;

            BGColor = config.BGColor;
            FrameColor = config.FrameColor;
            MainColor = config.MainColor;
            GainColor = config.GainColor;
            DrainColor = config.DrainColor;
            TickmarkColor = config.TickmarkColor;

            PulseColor = config.PulseColor;
            PulseColor2 = config.PulseColor2;
            PulseColor3 = config.PulseColor3;

            Mirror = config.Mirror;

            LabelTextProps = config.LabelTextProps;
        }

        public OathBarConfig() => MilestoneType = Above;
    }

    private OathBarConfig config;

    public override OathBarConfig Config => config;

    public override void InitConfigs()
    {
        config = new(Tracker.WidgetConfig);
        if (Tracker.WidgetConfig.OathBarCfg == null)
        {
            Config.MilestoneType = Above;
            if (ShouldInvertByDefault) Config.Invert = true;
        }
    }

    public override void ResetConfigs() => config = new();

    public override void ApplyConfigs()
    {
        WidgetContainer.SetPos(Config.Position + new Vector2(119, 36))
                  .SetScale(Config.Scale);

        Contents.SetRotation(Config.Angle, true);

        var halfWidth = (float)Ceiling(Config.Width / 2);
        var halfFrame = halfWidth + 40;
        var trueWidth = halfWidth * 2;

        FrameL.SetWidth(halfFrame).SetX(-halfFrame);
        FrameR.SetWidth(halfFrame);
        GlowL.SetWidth(halfFrame).SetX(-halfFrame);
        GlowR.SetWidth(halfFrame);

        Bar.SetX(-halfWidth)
           .SetOrigin(halfWidth, 0)
           .SetWidth(Config.Width)
           .SetScaleX(Abs(Bar.ScaleX) * (Config.Mirror ? -1 : 1));

        Backdrop.SetWidth(trueWidth);

        if (Config.ShowFiligree)
        {
            FrameL.SetPartId(4).SetNineGridOffset(50, 59, 50, 48);
            FrameR.SetPartId(5).SetNineGridOffset(50, 48, 48, 59);
            GlowL.SetPartId(9).SetNineGridOffset(50, 59, 50, 48);
            GlowR.SetPartId(10).SetNineGridOffset(50, 42, 50, 59);
        }
        else
        {
            FrameL.SetPartId(6).SetNineGridOffset(50, 0, 50, 48);
            FrameR.SetPartId(7).SetNineGridOffset(50, 48, 50, 0);
            GlowL.SetPartId(11).SetNineGridOffset(50, 0, 50, 48);
            GlowR.SetPartId(12).SetNineGridOffset(50, 42, 50, 0);
        }

        var prog = CalcProg();

        var colorOffset = new AddRGB(86, 47, -21);
        Main.SetAddRGB(Config.MainColor + colorOffset)
            .DefineTimeline(BarTimeline)
            .SetProgress(CalcProg());

        HandleMilestone(prog, true);

        Backdrop.SetAddRGB(Config.BGColor);
        Gain.SetAddRGB(Config.GainColor + colorOffset).SetWidth(0)
            .DefineTimeline(BarTimeline);
        Drain.SetAddRGB(Config.DrainColor + colorOffset).SetWidth(0)
             .DefineTimeline(BarTimeline);
        TickMark.SetMultiply(Config.TickmarkColor).SetAlpha((byte)(prog > 0 ? Config.TickmarkColor.A : 0));
        Effects.SetMultiply(Config.EffectsColor).SetAlpha(Config.EffectsColor.A);
        Frame.SetMultiply(Config.FrameColor);
        Glow.SetAddRGB(Config.PulseColor3);

        NumTextNode.ApplyProps(Config.NumTextProps, new(0, 83.5f));

        LabelTextNode.ApplyProps(Config.LabelTextProps, new(-80, 60))
                     .SetWidth(Config.Width - 4);

    }

    public override void DrawUI()
    {
        base.DrawUI();
        switch (UiTab)
        {
            case Layout:
                FloatControls("Width", ref Config.Width, Config.ShowFiligree ? 100 : 32, 2000, 1);
                AngleControls("Angle", ref Config.Angle);
                RadioIcons("Fill Direction", ref Config.Mirror, new() { false, true }, ArrowIcons);
                ToggleControls("Filigree", ref Config.ShowFiligree);
                if (Config.ShowFiligree) Config.Width = Max(Config.Width, 100);
                break;
            case Colors:
                ColorPickerRGBA("Backdrop", ref Config.BGColor);
                ColorPickerRGB("Frame Tint", ref Config.FrameColor);
                ColorPickerRGBA("Main Bar", ref Config.MainColor);
                ColorPickerRGBA("Gain", ref Config.GainColor);
                ColorPickerRGBA("Drain", ref Config.DrainColor);
                ColorPickerRGBA("Tickmark", ref Config.TickmarkColor);
                ColorPickerRGBA("Effects", ref Config.EffectsColor);
                break;
            case Behavior:
                SplitChargeControls(ref Config.SplitCharges, Tracker.RefType, Tracker.CurrentData.MaxCount);
                ToggleControls("Invert Fill", ref Config.Invert);
                HideControls("Collapse Empty", "Collapse Full");
                var twinkleEffect = new List<bool> { Config.TwinkleInc, Config.TwinkleDec };
                if (ToggleControls("Sparkle Effect", ref twinkleEffect, new() { "On Increase", "On Decrease" }))
                {
                    Config.TwinkleInc = twinkleEffect[0];
                    Config.TwinkleDec = twinkleEffect[1];
                }
                MilestoneControls("Pulse", ref Config.MilestoneType, ref Config.Milestone);
                if (Config.MilestoneType > 0)
                {
                    ColorPickerRGB("Pulse Colors", ref Config.PulseColor);
                    ColorPickerRGB(" ##Pulse2", ref Config.PulseColor2);
                    ColorPickerRGB(" ##Pulse3", ref Config.PulseColor3);
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
        Abs(Config.Angle) > 135 ? new() { ArrowLeft, ArrowRight } :
        Config.Angle > 45 ? new() { ArrowDown, ArrowUp } :
        Config.Angle < -45 ? new() { ArrowUp, ArrowDown } :
                                       new() { ArrowRight, ArrowLeft };

    #endregion
}

public partial class WidgetConfig
{
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] public OathBarConfig? OathBarCfg { get; set; }
}
