using CustomNodes;
using GaugeOMatic.CustomNodes.Animation;
using GaugeOMatic.Trackers;
using GaugeOMatic.Windows;
using Newtonsoft.Json;
using System.Linq;
using System.Numerics;
using static CustomNodes.CustomNodeManager;
using static FFXIVClientStructs.FFXIV.Component.GUI.AlignmentType;
using static FFXIVClientStructs.FFXIV.Component.GUI.FontType;
using static GaugeOMatic.CustomNodes.Animation.KeyFrame;
using static GaugeOMatic.Utility.Color;
using static GaugeOMatic.Widgets.HutonReplica;
using static GaugeOMatic.Widgets.NumTextProps;
using static GaugeOMatic.Widgets.WidgetTags;
using static GaugeOMatic.Widgets.WidgetUI;
using static GaugeOMatic.Windows.UpdateFlags;
using static System.Math;

#pragma warning disable CS8618

namespace GaugeOMatic.Widgets;

public sealed unsafe class HutonReplica : GaugeBarWidget
{
    public HutonReplica(Tracker tracker) : base(tracker) { }

    public override WidgetInfo WidgetInfo => GetWidgetInfo;

    public static WidgetInfo GetWidgetInfo => new()
    {
        DisplayName = "Huton Pinwheel",
        Author = "ItsBexy",
        Description = "A recreation of Ninja's Huton Gauge.",
        WidgetTags = GaugeBar | Replica | Exclude
    };

    public override CustomPartsList[] PartsLists { get; } = {
        new ("ui/uld/JobHudNIN1.tex",
             new(0, 0, 60, 76),
             new(60, 0, 112, 112),
             new(0, 76, 60, 60),
             new(60, 112, 72, 80),
             new(132, 112, 40, 40),
             new(0, 192, 144, 144))
    };

    #region Nodes

    public CustomNode EmptyPinwheel;
    public CustomNode ActiveClock;
    public CustomNode Puff;
    public CustomNode Blades;
    public CustomNode Shuriken;
    public CustomNode Whirl;
    public CustomNode ClockHand;

    public override CustomNode BuildContainer()
    {
        EmptyPinwheel = ImageNodeFromPart(0, 5).SetPos(-2, -2).SetScale(0.9f).SetOrigin(71, 74).SetAlpha(255);
        ActiveClock = BuildClock();
        NumTextNode = new();
        NumTextNode.Hide();
        Puff = ImageNodeFromPart(0, 4).SetPos(-2, -2).SetScale(0.6666667f).SetSize(144, 144).SetOrigin(72, 72).SetAlpha(0);

        return new CustomNode(CreateResNode(), EmptyPinwheel, ActiveClock, NumTextNode, Puff).SetSize(150, 150).SetOrigin(75, 75);
    }

    public CustomNode BuildClock()
    {
        Blades = BuildBlades();
        Shuriken = ImageNodeFromPart(0, 2).SetPos(50, 50).SetSize(60, 60).SetOrigin(30, 30).Hide();
        Whirl = ImageNodeFromPart(0, 1).SetPos(23, 25).SetScale(0.5f).SetSize(112, 112).SetOrigin(56, 56)
                                                       .SetRotation((float)PI).SetAlpha(0).SetImageFlag(32);

        return new CustomNode(CreateResNode(), Blades, Shuriken, Whirl).SetPos(-10, -10).SetScale(0.9f).SetSize(160, 160).SetOrigin(80, 80);
    }

    public CustomNode BuildBlades()
    {
        ClockHand = ImageNodeFromPart(0, 3).SetPos(44, 12)
                                           .SetSize(72, 80)
                                           .SetOrigin(36, 73)
                                           .Hide();

        return new CustomNode(CreateResNode(), BuildBlade(0), BuildBlade(1), BuildBlade(2), BuildBlade(3), BuildBlade(4), BuildBlade(5), ClockHand).SetPos(0, -6).SetSize(160, 160).SetOrigin(79, 87);
    }

    public CustomNode BuildBlade(int i) => ImageNodeFromPart(0, 0).SetPos(64, 15).SetSize(60, 76).SetOrigin(16, 72).SetRotation((1.0471975511966f * i) - 0.0698131700797732f).SetAlpha(0);

    #endregion

    #region Animations

    private void AppearAnim() =>
        Animator += new Tween[]
        {
            new(Puff,
                new(0) { Scale = 0.2f, Alpha = 0 },
                new(100) { Scale = 0.9f, Alpha = 255 },
                new(200) { Scale = 1.5f, Alpha = 0 }),
            new(EmptyPinwheel,
                new(0) { Scale = 0.9f, Rotation = 0, AddRGB = new(0, 0, 0), Alpha = 255 },
                new(125) { Scale = 1, Rotation = 1.5256047f, AddRGB = new(0, 48, 77), Alpha = 255 },
                new(250) { Scale = 1.4f, Rotation = 2.0694206f, Alpha = 0, AddRGB = new(0, 97, 99) })
        };

    private void DepleteAnim() =>
        Animator += new Tween(EmptyPinwheel,
                              new(0) { Scale = 1.2f, Rotation = 3.141593f, Alpha = 0, AddRGB = new(0, 0, 0) },
                              new(250) { Scale = 0.9f, Rotation = 6.283185f, Alpha = 255, AddRGB = new(0, 0, 0) });

    private void GainAnim() =>
        Animator += new Tween[]
        {
            new(Puff,
                new(0) { Scale = 0.2f, Alpha = 0 },
                new(100) { Scale = 0.9f, Alpha = 255 },
                new(200) { Scale = 2f, Alpha = 0 }),
            new(Whirl,
                new(0) { Scale = 0.7f, Rotation = 0, Alpha = 0 },
                new(287) { Scale = 1.3f, Rotation = 1f, Alpha = 255 },
                new(450) { Scale = 0.5f, Rotation = (float)PI, Alpha = 0 }),
            new(Blades,
                new(0) { Rotation = 0, AddRGB = new(0, 0, 0) },
                new(275) { Rotation = 5.654867f, AddRGB = new(0, 38, 58) },
                new(385) { Rotation = 6.283185f, AddRGB = new(0, 0, 0) })
        };

    private void ShowBlade(int i, AddRGB add, ColorRGB mult) =>
        Animator += new Tween(Blades[i % 6],
                              new(0) { ScaleX = 1.3f, ScaleY = 1.2f, Alpha = 0, AddRGB = new(-80, -100, -80), MultRGB = Config.FadeColor },
                              new(200) { Scale = 1, Alpha = 127, AddRGB = add, MultRGB = mult });

    private void HideBlade(int i) =>
        Animator += new Tween(Blades[i % 6],
                              new(0) { Scale = 1, Alpha = 127, AddRGB = new(-51, -81, -51), MultRGB = Config.FadeColor },
                              new(200) { ScaleX = 1.3f, ScaleY = 1.2f, Alpha = 0, AddRGB = new(-80, -100, -80), MultRGB = Config.FadeColor });

    #endregion

    #region UpdateFuncs

    public override void Update()
    {
        var current = Tracker.CurrentData.GaugeValue;
        var previous = Tracker.PreviousData.GaugeValue;
        var max = Tracker.CurrentData.MaxGauge;

        NumTextNode.UpdateValue(Tracker.CurrentData.GaugeValue, Tracker.CurrentData.MaxGauge);

        if (Config.Invert)
        {
            current = max - current;
            previous = max - previous;
        }

        var active = current > 0;

        if (FirstRun)
        {
            EmptyPinwheel.SetAlpha(!active);
            FirstRun = false;
        }
        else if (active && EmptyPinwheel.Node->Color.A == 255 && Animator.All(t => t.Target.Node != EmptyPinwheel.Node)) AppearAnim();
        if (current - previous >= 5f) GainAnim();
        if (current == 0 && previous > 0) DepleteAnim();

        ClockHand.SetRotation(AdjustProg(current/max));

        ClockHand.SetVis(active);
        Shuriken.SetVis(active);

        var bladeId = (int)Floor((max - current) / max * 6);
        var prevBladeId = (int)Floor((max - previous) / max * 6);

        var bladeSpan = max / 6;
        var subProg = (max - current) % bladeSpan / bladeSpan;

        if (bladeId > prevBladeId) for (var i = prevBladeId; i < bladeId; i++) HideBlade(i);
        else if (bladeId < prevBladeId) for (var i = bladeId; i < prevBladeId; i++) ShowBlade(i, i == bladeId ? CalcCurAdd(subProg) : new(0), i == bladeId ? CalcCurMult(subProg) : new(100));
        UpdateCurrentBlade(bladeId, subProg);
        for (var i = 5; i > bladeId; i--) UpdateOtherBlade(i);

        Animator.RunTweens();
    }

    public override float AdjustProg(float prog)
    {
        if (!Config.Smooth) prog = (float)Floor(prog * Tracker.CurrentData.MaxGauge) / Tracker.CurrentData.MaxGauge;
        return prog * -6.28318530717959f;
    }

    private void UpdateOtherBlade(int i)
    {
        if (Animator.Tweens.All(t => t.Target != Blades[i]))
            Blades[i].SetAddRGB(0)
                     .SetMultiply(Config.ActiveColor)
                     .SetAlpha(255)
                     .SetScale(1);
    }

    private void UpdateCurrentBlade(int i, float prog)
    {
        if (i == 6) return;

        Blades[i].SetAddRGB(CalcCurAdd(prog))
                 .SetMultiply(CalcCurMult(prog))
                 .SetAlpha((byte)Interpolate(255f, 127f, prog)!);
    }

    private ColorRGB CalcCurMult(float prog) => (ColorRGB)Interpolate(Config.ActiveColor, Config.FadeColor, prog)!;
    private static AddRGB CalcCurAdd(float prog) => (AddRGB)Interpolate(new AddRGB(0, 0, 0), new AddRGB(-50, -80, -50), prog)!;

    #endregion

    #region Configs

    public sealed class HutonReplicaConfig : GaugeBarWidgetConfig
    {
        public Vector2 Position;
        public float Scale = 1;
        public bool Smooth;
        public ColorRGB ActiveColor = new(100);
        public ColorRGB FadeColor = new(0x32, 0x32, 0x64);
        public ColorRGB HandColor = new(100);

        protected override NumTextProps NumTextDefault => new(enabled:   true,
                                                              position:  new(0, 0),
                                                              color:     0xffffffFFu,
                                                              edgeColor: 0x000000FFu,
                                                              showBg:    false,
                                                              bgColor:   new(0),
                                                              font:      MiedingerMed,
                                                              fontSize:  18,
                                                              align:     Center,
                                                              invert:    false);

        public HutonReplicaConfig(WidgetConfig widgetConfig) : base(widgetConfig.HutonReplicaCfg)
        {
            var config = widgetConfig.HutonReplicaCfg;

            if (config == null) return;

            Position = config.Position;
            Scale = config.Scale;
            Smooth = config.Smooth;
            ActiveColor = config.ActiveColor;
            FadeColor = config.FadeColor;
            HandColor = config.HandColor;
        }

        public HutonReplicaConfig() { }
    }

    public HutonReplicaConfig Config;
    public override GaugeBarWidgetConfig GetConfig => Config;

    public override void InitConfigs()
    {
        Config = new(Tracker.WidgetConfig);
        if (Tracker.WidgetConfig.HutonReplicaCfg == null && Tracker.RefType == RefType.Action) { Config.Invert = true; }
    }

    public override void ResetConfigs() => Config = new();

    public override void ApplyConfigs()
    {
        WidgetContainer.SetPos(Config.Position)
                  .SetScale(Config.Scale);

        ClockHand.SetMultiply(Config.HandColor);

        NumTextNode.ApplyProps(Config.NumTextProps, new(69, 72));
    }

    public override void DrawUI(ref WidgetConfig widgetConfig, ref UpdateFlags update)
    {
        Heading("Layout");
        PositionControls("Position", ref Config.Position, ref update);
        ScaleControls("Scale", ref Config.Scale, ref update);

        Heading("Colors");
        ColorPickerRGB("Blade", ref Config.ActiveColor, ref update);
        ColorPickerRGB("Fade", ref Config.FadeColor, ref update);
        ColorPickerRGB("Clock Hand", ref Config.HandColor, ref update);

        Heading("Behavior");

        ToggleControls("Turn Smoothly", ref Config.Smooth, ref update);
        ToggleControls("Invert Fill", ref Config.Invert, ref update);

        NumTextControls($"{Tracker.TermGauge} Text", ref Config.NumTextProps, ref update);

        if (update.HasFlag(Save)) ApplyConfigs();
        widgetConfig.HutonReplicaCfg = Config;
    }

    #endregion

}

public partial class WidgetConfig
{
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] public HutonReplicaConfig? HutonReplicaCfg { get; set; }
}
