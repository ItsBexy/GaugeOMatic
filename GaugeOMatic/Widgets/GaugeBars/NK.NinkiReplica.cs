using CustomNodes;
using GaugeOMatic.CustomNodes.Animation;
using GaugeOMatic.Trackers;
using Newtonsoft.Json;
using System.ComponentModel;
using System.Numerics;
using static CustomNodes.CustomNodeManager;
using static FFXIVClientStructs.FFXIV.Component.GUI.AlignmentType;
using static FFXIVClientStructs.FFXIV.Component.GUI.FontType;
using static GaugeOMatic.CustomNodes.Animation.KeyFrame;
using static GaugeOMatic.CustomNodes.Animation.Tween.EaseType;
using static GaugeOMatic.Utility.Color;
using static GaugeOMatic.Widgets.NinkiReplica;
using static GaugeOMatic.Widgets.NumTextProps;
using static GaugeOMatic.Widgets.WidgetTags;
using static GaugeOMatic.Widgets.WidgetUI;
using static GaugeOMatic.Widgets.WidgetUI.UpdateFlags;
using static GaugeOMatic.Widgets.WidgetUI.WidgetUiTab;
using static System.Math;

#pragma warning disable CS8618

namespace GaugeOMatic.Widgets;

public sealed unsafe class NinkiReplica : GaugeBarWidget
{
    public NinkiReplica(Tracker tracker) : base(tracker) { }

    public override WidgetInfo WidgetInfo => GetWidgetInfo;

    public static WidgetInfo GetWidgetInfo { get; } = new()
    {
        DisplayName = "Ninki Scroll",
        Author = "ItsBexy",
        Description = "A recreation of Ninja's Ninki Gauge. Lights up in a different color when it reaches the halfway point, making it great for tracking actions with 2 charges.",
        WidgetTags = GaugeBar | MultiComponent | Replica,
        MultiCompData = new("NK", "Ninki Gauge Replica", 1)
    };

    public override CustomPartsList[] PartsLists { get; } = {
        new ("ui/uld/JobHudNIN0.tex",
             new(0, 0, 256, 100),
             new(34, 101, 174, 46),
             new(34, 149, 174, 41),
             new(0, 196, 196, 56),
             new(260, 80, 57, 36),
             new(260, 116, 60, 36),
             new(256, 0, 64, 80),
             new(0, 252, 208, 20),
             new(0, 272, 208, 16),
             new(256, 152, 20, 88),
             new(29, 101, 178, 46),
             new(280, 152, 40, 44))
    };

    #region Nodes

    public CustomNode Scroll;
    public CustomNode CalligraphyFlash1;
    public CustomNode Cloud1;
    public CustomNode Cloud2;

    public CustomNode GaugeBarV;
    public CustomNode GaugeBarH;
    public CustomNode ScrollImage;
    public CustomNode VSigil;
    public CustomNode Shine;
    public CustomNode TopBorder;
    public CustomNode BottomBorder;
    public CustomNode CalligraphyFlash2;

    public CustomNode DrainV;
    public CustomNode GainV;
    public CustomNode MainV;

    public CustomNode DrainH;
    public CustomNode GainH;
    public CustomNode MainH;

    public override CustomNode BuildContainer()
    {
        Scroll = BuildScroll();
        CalligraphyFlash1 = ImageNodeFromPart(0, 3).SetPos(23, 29).SetOrigin(98, 28).SetRGBA(new(1, 1, 1, 0));
        Cloud1 = ImageNodeFromPart(0, 11).SetOrigin(20, 22).SetPos(70, 32).SetRGBA(new(1, 1, 1, 0)).SetAddRGB(-70, -70, -90);
        Cloud2 = ImageNodeFromPart(0, 11).SetOrigin(20, 22).SetPos(170, 32).SetRGBA(new(1, 1, 1, 0)).SetAddRGB(-40, -40, -60);
        NumTextNode = new();

        return new CustomNode(CreateResNode(), Scroll, CalligraphyFlash1, Cloud1, Cloud2, NumTextNode).SetSize(256, 100).SetOrigin(128, 50);
    }

    private CustomNode BuildScroll()
    {
        var vTimeline = VTimeline;
        MainV = NineGridFromPart(0, 4).SetAddRGB(Config.BarColorHigh2).SetWidth(0).DefineTimeline(vTimeline);
        GainV = NineGridFromPart(0, 4).SetAddRGB(Config.GainColorV).SetWidth(0).DefineTimeline(vTimeline);
        DrainV = NineGridFromPart(0, 4).SetAddRGB(Config.DrainColorV).SetWidth(0).DefineTimeline(vTimeline);

        GaugeBarV = new CustomNode(CreateResNode(), ImageNodeFromPart(0, 5), DrainV, GainV, MainV).SetPos(210, 75).SetSize(57, 36).SetRotation(-1.5707964f);

        var hTimeline = HTimeline;
        MainH = NineGridFromPart(0, 1).SetAddRGB(Config.BarColorLow).SetWidth(0).DefineTimeline(hTimeline);
        GainH = NineGridFromPart(0, 10).SetAddRGB(Config.GainColorH).SetWidth(0).DefineTimeline(hTimeline);
        DrainH = NineGridFromPart(0, 10).SetAddRGB(Config.BarColorHigh).SetWidth(0).DefineTimeline(hTimeline);

        GaugeBarH = new CustomNode(CreateResNode(), ImageNodeFromPart(0, 2).SetRGBA((ColorRGB)0xFFFFFFB2), DrainH, GainH, MainH).SetPos(32, 33).SetSize(174, 46).SetAddRGB(0);

        ScrollImage = ImageNodeFromPart(0, 0);
        VSigil = ImageNodeFromPart(0, 6).SetPos(199, 4).SetOrigin(32, 40).SetScale(2, 1.8f).SetAlpha(0);
        Shine = ImageNodeFromPart(0, 9).SetPos(193, 10).SetOrigin(20, 44).SetAddRGB(Config.FlashH2).SetAlpha(0);
        TopBorder = ImageNodeFromPart(0, 7).SetPos(11, 19).SetOrigin(0, 0).SetAddRGB(Config.BorderGlow);
        BottomBorder = ImageNodeFromPart(0, 8).SetPos(11, 77).SetOrigin(0, 16).SetAddRGB(Config.BorderGlow);
        CalligraphyFlash2 = ImageNodeFromPart(0, 3).SetPos(24, 29).SetOrigin(98, 28).SetScale(1.5f).SetAddRGB(Config.FlashH).SetAlpha(0);

        Animator += BorderTween(TopBorder);
        Animator += BorderTween(BottomBorder);

        return new CustomNode(CreateResNode(), GaugeBarV, GaugeBarH, ScrollImage, VSigil, Shine, TopBorder, BottomBorder, CalligraphyFlash2).SetSize(256, 100);
    }


    #endregion

    #region Animations

    public override void HideBar(bool instant = false)
    {
        Animator -= "Fade";
        Animator += new Tween(WidgetContainer, new(0, WidgetContainer), Hidden[instant ? 0 : 250]) { Label = "Fade", Ease = SinInOut };
    }

    public override void RevealBar(bool instant = false)
    {
        Animator -= "Fade";
        Animator += new Tween(WidgetContainer, new(0, WidgetContainer), Visible[instant ? 0 : 250]) { Label = "Fade", Ease = SinInOut };
    }

    private KeyFrame[] HTimeline => new KeyFrame[] { new(0) { Width = 0 }, new(Max(0.001f, Config.Midpoint)) { Width = 175 }, new(1) { Width = 175 }};
    private KeyFrame[] VTimeline => new KeyFrame[] { new(0) { Width = 0 }, new(Min(0.999f, Config.Midpoint)) { Width = 0 }, new(1) { Width = 57 }};

    public static Tween BorderTween(CustomNode target) => new(target, new(0) { ScaleY = 1.2f, Alpha = 127 }, new(166) { ScaleY = 1f, Alpha = 51 }, new(666) { ScaleY = 1.2f, Alpha = 127 }) { Repeat = true };

    private void GaugeFullAnim() =>
        Animator += new Tween[]
        {
            new(GaugeBarV,
                new(0) { AddRGB = 29, MultRGB = 100 },
                new(50) { AddRGB = 0, MultRGB = 85 },
                new(633) { AddRGB = 29, MultRGB = 100 }) { Repeat = true },
            new(VSigil,
                new(0) { Scale = 1, Rotation = 0, Alpha = 134 },
                new(450) { ScaleX = 2, ScaleY = 1.8f, Rotation = 0.147453292f, Alpha = 0 })
        };

    private void GainAnim() =>
        Animator += new Tween[]
        {
            new(CalligraphyFlash1,
                new(0) { Scale = 1, Alpha = 255 },
                new(150) { Scale = 1.03f, Alpha = 153 },
                new(500) { Scale = 1, Alpha = 0 }),
            new(CalligraphyFlash1,
                new(0) { AddRGB = new(81, 24, 129) },
                new(75) { AddRGB = new(96, 44, 105) },
                new(200) { AddRGB = new(49, -20, 176) },
                new(500) { AddRGB = 0 })
        };

    private void MidpointAnim() =>
        Animator += new Tween[]
        {
            new(CalligraphyFlash2,
                new(0) { Scale = 1, Alpha = 110 },
                new(100) { Scale = 1, Alpha = 255 },
                new(300) { Scale = 1.5f, Alpha = 0 }),
            new(Shine,
                new(0) { X = 11, Y = 14, Alpha = 76 },
                new(200) { X = 190, Y = 10, Alpha = 76 },
                new(300) { X = 193, Y = 10, Alpha = 0 }),
            new(Shine,
                new(0) { ScaleX = 1.15f, ScaleY = 0.9f, Rotation = 0 },
                new(100) { ScaleX = 3f, ScaleY = 1f, Rotation = -0.065f },
                new(300) { ScaleX = 0.6f, ScaleY = 2f, Rotation = 0 })
        };

    private void SpendAnim() =>
        Animator += new Tween[]
        {
            new(Cloud1,
                new(0) { ScaleX = 2.65f, ScaleY = 2.15f, Alpha = 204 },
                new(70) { ScaleX = 2.95f, ScaleY = 2.4f, Alpha = 204 },
                new(650) { ScaleX = 5.5f, ScaleY = 4.5f, Alpha = 0 }),
            new(Cloud2,
                new(0) { ScaleX = 3.1f, ScaleY = 2.1f, Alpha = 206 },
                new(100) { ScaleX = 4f, ScaleY = 3f, Alpha = 222 },
                new(650) { ScaleX = 4.5f, ScaleY = 3.5f, Alpha = 0 }),
            new(Scroll,
                new(0) { X = 0, Y = 0 },
                new(50) { X = 0, Y = 1.95f },
                new(150) { X = -0.85f, Y = -0.95f },
                new(200) { X = 0.95f, Y = 0 },
                new(250) { X = 0, Y = 0 })
        };

    #endregion

    #region UpdateFuncs

    public override void Update()
    {
        var current = Tracker.CurrentData.GaugeValue;
        var previous = Tracker.PreviousData.GaugeValue;
        var max = Tracker.CurrentData.MaxGauge;

        NumTextNode.UpdateValue(current, max);

        if (Config.Invert)
        {
            current = max - current;
            previous = max - previous;
        }

        var prog = current / max;
        var prevProg = previous / max;

        if (FirstRun)
        {
            MainH.SetProgress(prog);
            MainV.SetProgress(prog);
            FirstRun = false;
        }
        else
        {
            AnimateDrainGain(MainH, GainH, DrainH, prog, prevProg);
            AnimateDrainGain(MainV, GainV, DrainV, prog, prevProg);
        }

        var pastMid = prog >= Config.Midpoint;
        TopBorder.SetVis(pastMid);
        BottomBorder.SetVis(pastMid);
        GaugeBarH.SetAddRGB(pastMid ? Config.BarColorHigh - Config.BarColorLow : new(0, 0, 0));

        if (pastMid && prevProg < Config.Midpoint) MidpointAnim();

        if (prog > prevProg)
        {
            if (prog - prevProg >= GainTolerance) OnIncrease(prog, prevProg);
            if (prog > 0 && prevProg <= 0) OnIncreaseFromMin();
            if (prog >= 1 && prevProg < 1) OnIncreaseToMax();
        }
        else if (prevProg > prog)
        {
            if (prevProg - prog >= DrainTolerance || (prog < Config.Midpoint && prevProg >= Config.Midpoint)) OnDecrease(prog, prevProg);
            if (prevProg >= 1 && prog < 1) OnDecreaseFromMax();
            if (prevProg > 0 && prog <= 0) OnDecreaseToMin();
        }

        Animator.RunTweens();
    }

    public override void OnIncrease(float prog, float prevProg) => GainAnim();
    public override void OnDecrease(float prog, float prevProg) => SpendAnim();
    public override void OnIncreaseToMax()
    {
        GaugeFullAnim();
        base.OnIncreaseToMax();
    }

    public override void OnDecreaseFromMax()
    {
        Animator -= GaugeBarV;
        GaugeBarV.SetAddRGB(0).SetMultiply(new ColorRGB(100, 100, 100));
        base.OnDecreaseFromMax();
    }

    #endregion

    #region Configs

    public sealed class NinkiReplicaConfig : GaugeBarWidgetConfig
    {
        public Vector2 Position;
        [DefaultValue(1f)] public float Scale = 1;
        [DefaultValue(0.5f)] public float Midpoint = 0.5f;
        public AddRGB BarColorLow = new(0, -70, 100);
        public AddRGB BarColorHigh = new(200, -90, -150);
        public AddRGB BarColorHigh2 = new(150, 20, -100);
        public AddRGB GainColorH = new(-40, -200, 20);
        public AddRGB GainColorV = new(30, -170, 60);
        public AddRGB DrainColorV = new(80, -50, -150);
        public AddRGB FlashH = new(200, 100, 100);
        public AddRGB FlashH2 = new(100, -100, -50);
        public AddRGB BorderGlow = new(100, -50, -120);
        protected override NumTextProps NumTextDefault => new(enabled:   true,
                                                              position:  new(0, 0),
                                                              color:     0xCCCCCCFFu,
                                                              edgeColor: 0x5534C2FFu,
                                                              showBg:    false,
                                                              bgColor:   new(0),
                                                              font:      MiedingerMed,
                                                              fontSize:  18,
                                                              align:     Center,
                                                              invert:    false,
                                                              precision: 0,
                                                              showZero:  true);

        public NinkiReplicaConfig(WidgetConfig widgetConfig) : base(widgetConfig.NinkiReplicaCfg)
        {
            var config = widgetConfig.NinkiReplicaCfg;

            if (config == null) return;

            Position = config.Position;
            Scale = config.Scale;
            Midpoint = config.Midpoint;
            BarColorLow = config.BarColorLow;
            BarColorHigh = config.BarColorHigh;
            BarColorHigh2 = config.BarColorHigh2;
            GainColorH = config.GainColorH;
            GainColorV = config.GainColorV;
            DrainColorV = config.DrainColorV;
            FlashH = config.FlashH;
            FlashH2 = config.FlashH2;
            BorderGlow = config.BorderGlow;
        }

        public NinkiReplicaConfig() { }
    }

    public NinkiReplicaConfig Config;
    public override GaugeBarWidgetConfig GetConfig => Config;

    public override void InitConfigs()
    {
        Config = new(Tracker.WidgetConfig);
        if (Tracker.WidgetConfig.NinkiReplicaCfg == null && ShouldInvertByDefault) { Config.Invert = true; }
    }

    public override void ResetConfigs() => Config = new();

    public override void ApplyConfigs()
    {
        WidgetContainer.SetPos(Config.Position).SetScale(Config.Scale);

        var hTimeline = HTimeline;
        var vTimeline = VTimeline;

        MainH.SetAddRGB(Config.BarColorLow).DefineTimeline(hTimeline);
        GainH.SetAddRGB(Config.GainColorH).DefineTimeline(hTimeline);
        DrainH.SetAddRGB(Config.BarColorLow).DefineTimeline(hTimeline);

        MainV.SetAddRGB(Config.BarColorHigh2).DefineTimeline(vTimeline);
        DrainV.SetAddRGB(Config.DrainColorV).DefineTimeline(vTimeline);
        GainV.SetAddRGB(Config.GainColorV).DefineTimeline(vTimeline);

        CalligraphyFlash2.SetAddRGB(Config.FlashH);
        Shine.SetAddRGB(Config.FlashH2);
        TopBorder.SetAddRGB(Config.BorderGlow);
        BottomBorder.SetAddRGB(Config.BorderGlow);

        NumTextNode.ApplyProps(Config.NumTextProps, new(229, 83));
    }

    public override void DrawUI(ref WidgetConfig widgetConfig)
    {
        switch (UiTab)
        {
            case Layout:
                PositionControls("Position", ref Config.Position);
                ScaleControls("Scale", ref Config.Scale);
                break;
            case Colors:
                Heading("Horizontal Section");

                ColorPickerRGB("Main Bar (Low)", ref Config.BarColorLow);
                ColorPickerRGB("Main Bar (High)", ref Config.BarColorHigh);
                ColorPickerRGB("Gain", ref Config.GainColorH);
                ColorPickerRGB("Border Glow", ref Config.BorderGlow);
                ColorPickerRGB("Flash Effects##flash1", ref Config.FlashH);
                ColorPickerRGB("##flash2", ref Config.FlashH2);

                Heading("Vertical Section");
                ColorPickerRGB("Bar Color", ref Config.BarColorHigh2);
                ColorPickerRGB("Gain Color", ref Config.GainColorV);
                ColorPickerRGB("Drain Color", ref Config.DrainColorV);
                break;
            case Behavior:
                ToggleControls("Invert Fill", ref Config.Invert);
                HideControls();
                PercentControls("Midpoint", ref Config.Midpoint);
                break;
            case Text:
                NumTextControls($"{Tracker.TermGauge} Text", ref Config.NumTextProps);
                break;
            default:
                break;
        }

        if (UpdateFlag.HasFlag(Save)) ApplyConfigs();
        widgetConfig.NinkiReplicaCfg = Config;
    }

    #endregion
}

public partial class WidgetConfig
{
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] public NinkiReplicaConfig? NinkiReplicaCfg { get; set; }
}
