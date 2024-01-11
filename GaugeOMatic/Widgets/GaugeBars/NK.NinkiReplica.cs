using GaugeOMatic.Trackers;
using GaugeOMatic.Windows;
using Newtonsoft.Json;
using System;
using System.Numerics;
using static CustomNodes.CustomNodeManager;
using static CustomNodes.CustomNodeManager.Tween;
using static FFXIVClientStructs.FFXIV.Component.GUI.AlignmentType;
using static FFXIVClientStructs.FFXIV.Component.GUI.FontType;
using static GaugeOMatic.Utility.Color;
using static GaugeOMatic.Widgets.GaugeBarWidget;
using static GaugeOMatic.Widgets.GaugeBarWidget.DrainGainType;
using static GaugeOMatic.Widgets.NinkiReplica;
using static GaugeOMatic.Widgets.NumTextProps;
using static GaugeOMatic.Widgets.WidgetTags;
using static GaugeOMatic.Widgets.WidgetUI;
using static GaugeOMatic.Windows.UpdateFlags;

namespace GaugeOMatic.Widgets;

public sealed unsafe class NinkiReplica : Widget
{
    public override WidgetInfo WidgetInfo => GetWidgetInfo;

    public static WidgetInfo GetWidgetInfo => new()
    {
        DisplayName = "Ninki Scroll",
        Author = "ItsBexy",
        Description = "A recreation of Ninja's Ninki Gauge. Lights up in a different color when it reaches the halfway point, making it great for tracking actions with 2 charges.",
        WidgetTags = GaugeBar | MultiComponent | Replica,
        KeyText = "NK1"
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
    public CustomNode NumTextNode { get; set; }

    public CustomNode GaugeBarV;
    public CustomNode GaugeBarH;
    public CustomNode ScrollImage;
    public CustomNode VSigil;
    public CustomNode Shine;
    public CustomNode TopBorder;
    public CustomNode BottomBorder;
    public CustomNode CalligraphyFlash2;

    public override CustomNode BuildRoot()
    {
        Scroll = BuildScroll();
        CalligraphyFlash1 = ImageNodeFromPart(0, 3).SetPos(23, 29).SetOrigin(98, 28).SetRGBA(new(1, 1, 1, 0));
        Cloud1 = ImageNodeFromPart(0, 11).SetOrigin(20, 22).SetPos(70, 32).SetRGBA(new(1, 1, 1, 0)).SetAddRGB(-70, -70, -90);
        Cloud2 = ImageNodeFromPart(0, 11).SetOrigin(20, 22).SetPos(170, 32).SetRGBA(new(1, 1, 1, 0)).SetAddRGB(-40, -40, -60);
        NumTextNode = BuildNumText();

        return new CustomNode(CreateResNode(), Scroll, CalligraphyFlash1, Cloud1, Cloud2, NumTextNode).SetSize(256, 100).SetOrigin(128, 50);
    }

    private CustomNode BuildScroll()
    {
        GaugeBarV = BuildVGauge();
        GaugeBarH = BuildHGauge();
        ScrollImage = ImageNodeFromPart(0, 0);
        VSigil = ImageNodeFromPart(0, 6).SetPos(199, 4).SetOrigin(32, 40).SetScale(2, 1.8f).SetAlpha(0);
        Shine = ImageNodeFromPart(0, 9).SetPos(193, 10).SetOrigin(20, 44).SetAddRGB(Config.FlashH2).SetAlpha(0);
        TopBorder = ImageNodeFromPart(0, 7).SetPos(11, 19).SetOrigin(0, 0).SetAddRGB(Config.BorderGlow);
        BottomBorder = ImageNodeFromPart(0, 8).SetPos(11, 77).SetOrigin(0, 16).SetAddRGB(Config.BorderGlow);
        CalligraphyFlash2 = ImageNodeFromPart(0, 3).SetPos(24, 29).SetOrigin(98, 28).SetScale(1.5f).SetAddRGB(Config.FlashH).SetAlpha(0);

        Tweens.Add(BorderTween(TopBorder));
        Tweens.Add(BorderTween(BottomBorder));

        return new CustomNode(CreateResNode(), GaugeBarV, GaugeBarH, ScrollImage, VSigil, Shine, TopBorder, BottomBorder, CalligraphyFlash2).SetSize(256, 100);
    }

    private CustomNode BuildVGauge() =>
        new CustomNode(CreateResNode(), ImageNodeFromPart(0, 5), NineGridFromPart(0, 4).SetAddRGB(Config.DrainColorV).SetWidth(0), NineGridFromPart(0, 4).SetAddRGB(Config.GainColorV).SetWidth(0), NineGridFromPart(0, 4).SetAddRGB(Config.BarColorHigh2).SetWidth(0)).SetPos(210, 75).SetSize(57, 36).SetRotation(-1.5707964f);

    private CustomNode BuildHGauge()
    {
        return new CustomNode(CreateResNode(), ImageNodeFromPart(0, 2).SetRGBA((ColorRGB)0xFFFFFFB2), NineGridFromPart(0, 10).SetAddRGB(Config.BarColorHigh).SetWidth(0), NineGridFromPart(0, 10).SetAddRGB(Config.GainColorH).SetWidth(0), NineGridFromPart(0, 1).SetAddRGB(Config.BarColorLow).SetWidth(0)).SetPos(32, 33).SetSize(174, 46).SetAddRGB(0);
    }

    private CustomNode BuildNumText() => new CustomNode(CreateTextNode("0", 18, 20)).SetTextColor(Config.NumTextProps.Color, Config.NumTextProps.EdgeColor).SetPos(227, 83);

    #endregion

    #region Animations

    public static Tween BorderTween(CustomNode target) => new(target, new(0) { ScaleY = 1.2f, Alpha = 127 }, new(166) { ScaleY = 1f, Alpha = 51 }, new(666) { ScaleY = 1.2f, Alpha = 127 }) { Repeat = true };

    private void GaugeFullAnim()
    {
        Tweens.Add(new(GaugeBarV, new(0) { AddRGB = 29, MultRGB = 100 }, new(50) { AddRGB = 0, MultRGB = 85 }, new(633) { AddRGB = 29, MultRGB = 100 }) { Repeat = true });

        Tweens.Add(VSigil.CreateTween(new(0) { Scale = 1, Rotation = 0, Alpha = 134 },
                                      new(450) { ScaleX = 2, ScaleY = 1.8f, Rotation = 0.147453292f, Alpha = 0 }));
    }

    private void GainAnim()
    {
        Tweens.Add(CalligraphyFlash1.CreateTween(new(0) { Scale = 1, Alpha = 255 },
                                                 new(150) { Scale = 1.03f, Alpha = 153 },
                                                 new(500) { Scale = 1, Alpha = 0 }));

        Tweens.Add(CalligraphyFlash1.CreateTween(new(0) { AddRGB = new(81, 24, 129) },
                                                 new(75) { AddRGB = new(96, 44, 105) },
                                                 new(200) { AddRGB = new(49, -20, 176) },
                                                 new(500) { AddRGB = 0 }));
    }

    private void MidpointAnim()
    {
        Tweens.Add(CalligraphyFlash2.CreateTween(new(0) { Scale = 1, Alpha = 110 },
                                                 new(100) { Scale = 1, Alpha = 255 },
                                                 new(300) { Scale = 1.5f, Alpha = 0 }));

        Tweens.Add(Shine.CreateTween(new(0) { X = 11, Y = 14, Alpha = 76 },
                                     new(200) { X = 190, Y = 10, Alpha = 76 },
                                     new(300) { X = 193, Y = 10, Alpha = 0 }));

        Tweens.Add(Shine.CreateTween(new(0) { ScaleX = 1.15f, ScaleY = 0.9f, Rotation = 0 },
                                     new(100) { ScaleX = 3f, ScaleY = 1f, Rotation = -0.065f },
                                     new(300) { ScaleX = 0.6f, ScaleY = 2f, Rotation = 0 }));
    }

    private void SpendAnim()
    {
        Tweens.Add(Cloud1.CreateTween(new(0) { ScaleX = 2.65f, ScaleY = 2.15f, Alpha = 204 },
                                      new(70) { ScaleX = 2.95f, ScaleY = 2.4f, Alpha = 204 },
                                      new(650) { ScaleX = 5.5f, ScaleY = 4.5f, Alpha = 0 }));

        Tweens.Add(Cloud2.CreateTween(new(0) { ScaleX = 3.1f, ScaleY = 2.1f, Alpha = 206 },
                                      new(100) { ScaleX = 4f, ScaleY = 3f, Alpha = 222 },
                                      new(650) { ScaleX = 4.5f, ScaleY = 3.5f, Alpha = 0 }));

        Tweens.Add(Scroll.CreateTween(new(0) { X = 0, Y = 0 },
                                      new(50) { X = 0, Y = 1.95f },
                                      new(150) { X = -0.85f, Y = -0.95f },
                                      new(200) { X = 0.95f, Y = 0 },
                                      new(250) { X = 0, Y = 0 }));
    }

    #endregion

    #region UpdateFuncs

    public override string? SharedEventGroup => null;

    public bool FirstRun = true;

    public override void Update()
    {
        var current = Tracker.CurrentData.GaugeValue;
        var previous = Tracker.PreviousData.GaugeValue;
        var max = Tracker.CurrentData.MaxGauge;

        NumTextNode.UpdateNumText(Config.NumTextProps,current,max);

        if (Config.Invert)
        {
            current = max - current;
            previous = max - previous;
        }

        const float midpoint = 0.5f;
        var prog = current / max;
        var prevProg = previous / max;

        var sizeH = (ushort)(Math.Clamp(prog, 0, midpoint) / midpoint * 175F);
        var prevSizeH = (ushort)(Math.Clamp(prevProg, 0, midpoint) / midpoint * 175F);

        var sizeV = (ushort)(Math.Clamp((prog - midpoint) / (1 - midpoint), 0, 1) * 57f);
        var prevSizeV = (ushort)(Math.Clamp((prevProg - midpoint) / (1 - midpoint), 0, 1) * 57f);

        var spend = prevProg - prog >= 0.1f;

        if (FirstRun)
        {
            GaugeBarH[3].SetWidth(sizeH);
            GaugeBarV[3].SetWidth(sizeV);
            FirstRun = false;
        }
        else
        {
            AnimateDrainGain(Width, ref Tweens, GaugeBarH[3], GaugeBarH[2], GaugeBarH[1], sizeH, prevSizeH, 0, spend ? 250 : 300);
            AnimateDrainGain(Width, ref Tweens, GaugeBarV[3], GaugeBarV[2], GaugeBarV[1], sizeV, prevSizeV, 0, 300);
        }

        var pastMid = prog >= midpoint;
        TopBorder.SetVis(pastMid);
        BottomBorder.SetVis(pastMid);
        GaugeBarH.SetAddRGB(pastMid ? Config.BarColorHigh - Config.BarColorLow : new(0, 0, 0));

        if (pastMid && prevProg < midpoint) MidpointAnim();
        if (spend || (prog < midpoint && prevProg >= midpoint)) OnSpend();
        if (prog - prevProg >= 0.049f) OnGain();
        if (prog >= 1f && prevProg < 1f) OnReachMax();
        else if (prog < 1f) OnDropFromMax();

        RunTweens();
    }

    public void OnGain() => GainAnim();
    public void OnSpend() => SpendAnim();
    public void OnReachMax() => GaugeFullAnim();
    public void OnDropFromMax()
    {
        ClearNodeTweens(ref Tweens, GaugeBarV);
        GaugeBarV.SetAddRGB(0).SetMultiply(new ColorRGB(100, 100, 100));
    }

    #endregion

    #region Configs

    public sealed class NinkiReplicaConfig : GaugeBarWidgetConfig
    {
        public Vector2 Position = new(0, 0);
        public float Scale = 1;
        public AddRGB BarColorLow = new(0, -70, 100);
        public AddRGB BarColorHigh = new(200, -90, -150);
        public AddRGB BarColorHigh2 = new(150, 20, -100);
        public AddRGB GainColorH = new(-40, -200, 20);
        public AddRGB GainColorV = new(30, -170, 60);
        public AddRGB DrainColorV = new(80, -50, -150);
        public AddRGB FlashH = new(200, 100, 100);
        public AddRGB FlashH2 = new(100, -100, -50);
        public AddRGB BorderGlow = new(100, -50, -120);
        protected override NumTextProps NumTextDefault => new(true, new(212, 83), 0xCCCCCCFFu, 0x5534C2FFu, MiedingerMed, 18, Center, false, 0, true);

        public NinkiReplicaConfig(WidgetConfig widgetConfig)
        {
            NumTextProps = NumTextDefault;
            var config = widgetConfig.NinkiReplicaCfg;

            if (config == null) return;

            NumTextProps = config.NumTextProps;
            Invert = config.Invert;
            Position = config.Position;
            Scale = config.Scale;
            BarColorLow = config.BarColorLow;
            BarColorHigh = config.BarColorHigh;
            BarColorHigh2 = config.BarColorHigh2;
            GainColorH = config.GainColorH;
            GainColorV = config.GainColorV;
            DrainColorV = config.DrainColorV;
            FlashH = config.FlashH;
            FlashH2 = config.FlashH2;
            BorderGlow = config.BorderGlow;
            AnimationLength = config.AnimationLength;
        }

        public NinkiReplicaConfig() { }
    }

    public NinkiReplicaConfig Config = null!;

    public override void InitConfigs()
    {
        Config = new(Tracker.WidgetConfig);
        if (Tracker.WidgetConfig.NinkiReplicaCfg == null && Tracker.RefType == RefType.Action) { Config.Invert = true; }
    }

    public override void ResetConfigs() => Config = new();

    public override void ApplyConfigs()
    {
        WidgetRoot.SetPos(Config.Position).SetScale(Config.Scale);

        GaugeBarH[1].SetAddRGB(Config.BarColorHigh);
        GaugeBarH[2].SetAddRGB(Config.GainColorH);
        GaugeBarH[3].SetAddRGB(Config.BarColorLow);

        GaugeBarV[1].SetAddRGB(Config.DrainColorV);
        GaugeBarV[2].SetAddRGB(Config.GainColorV);
        GaugeBarV[3].SetAddRGB(Config.BarColorHigh2);

        CalligraphyFlash2.SetAddRGB(Config.FlashH);
        Shine.SetAddRGB(Config.FlashH2);
        TopBorder.SetAddRGB(Config.BorderGlow);
        BottomBorder.SetAddRGB(Config.BorderGlow);

        Config.NumTextProps.ApplyTo(WidgetRoot[4]);
    }

    public override void DrawUI(ref WidgetConfig widgetConfig, ref UpdateFlags update)
    {
        Heading("Layout");
        PositionControls("Position", ref Config.Position, ref update);
        ScaleControls("Scale", ref Config.Scale, ref update);

        Heading("Colors (Horizontal)");

        ColorPickerRGB("Main Bar (Low)", ref Config.BarColorLow, ref update);
        ColorPickerRGB("Main Bar (High)", ref Config.BarColorHigh, ref update);
        ColorPickerRGB("Gain", ref Config.GainColorH, ref update);
        ColorPickerRGB("Border Glow", ref Config.BorderGlow, ref update);
        ColorPickerRGB("Flash Effects##flash1", ref Config.FlashH, ref update);
        ColorPickerRGB("##flash2", ref Config.FlashH2, ref update);

        Heading("Colors (Vertical)");
        ColorPickerRGB("Bar Color", ref Config.BarColorHigh2, ref update);
        ColorPickerRGB("Gain Color", ref Config.GainColorV, ref update);
        ColorPickerRGB("Drain Color", ref Config.DrainColorV, ref update);

        Heading("Behavior");
        ToggleControls("Invert Fill", ref Config.Invert, ref update);

        NumTextControls($"{Tracker.TermGauge} Text", ref Config.NumTextProps, ref update);

        if (update.HasFlag(Save)) ApplyConfigs();
        widgetConfig.NinkiReplicaCfg = Config;
    }

    #endregion

    public NinkiReplica(Tracker tracker) : base(tracker) { }
}

public partial class WidgetConfig
{
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] public NinkiReplicaConfig? NinkiReplicaCfg { get; set; }
}
