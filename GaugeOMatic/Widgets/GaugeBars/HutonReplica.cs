using CustomNodes;
using GaugeOMatic.CustomNodes.Animation;
using GaugeOMatic.Trackers;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using static CustomNodes.CustomNode;
using static CustomNodes.CustomNodeManager;
using static CustomNodes.CustomNodeManager.CustomPartsList;
using static FFXIVClientStructs.FFXIV.Component.GUI.AlignmentType;
using static FFXIVClientStructs.FFXIV.Component.GUI.FontType;
using static GaugeOMatic.CustomNodes.Animation.KeyFrame;
using static GaugeOMatic.GaugeOMatic;
using static GaugeOMatic.Utility.Color;
using static GaugeOMatic.Widgets.HutonReplica;
using static GaugeOMatic.Widgets.NumTextProps;
using static GaugeOMatic.Widgets.WidgetTags;
using static GaugeOMatic.Widgets.WidgetUI;
using static GaugeOMatic.Widgets.WidgetUI.UpdateFlags;
using static GaugeOMatic.Widgets.WidgetUI.WidgetUiTab;
using static System.Math;

#pragma warning disable CS8618

namespace GaugeOMatic.Widgets;

public sealed unsafe class HutonReplica : GaugeBarWidget
{
    public HutonReplica(Tracker tracker) : base(tracker) { }

    public override WidgetInfo WidgetInfo => GetWidgetInfo;

    public static WidgetInfo GetWidgetInfo { get; } = new()
    {
        DisplayName = "Huton Pinwheel",
        Author = "ItsBexy",
        Description = "A recreation of Ninja's Huton Gauge.",
        WidgetTags = GaugeBar | Replica
    };

    public override CustomPartsList[] PartsLists { get; } = {
        new (AssetFromFile(Path.Combine(PluginDirPath,@"TextureAssets\huton.tex")),
             new(0, 0, 120, 152),
             new(120, 0, 224, 224),
             new(0, 152, 120, 120),
             new(120, 224, 144, 160),
             new(264, 224, 80, 80),
             new(0, 384, 288, 288))
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
        EmptyPinwheel = ImageNodeFromPart(0, 5).SetPos(-2, -2)
                                               .SetScale(0.9f)
                                               .SetOrigin(142, 148)
                                               .SetAlpha(255);

        ActiveClock = BuildClock();
        NumTextNode = new();
        NumTextNode.Hide().SetScale(2);
        Puff = ImageNodeFromPart(0, 4).SetPos(-4, -4)
                                      .SetScale(0.6666667f)
                                      .SetSize(288, 288)
                                      .SetOrigin(144, 144)
                                      .SetAlpha(0)
                                      .SetImageWrap(2);

        return new CustomNode(CreateResNode(), EmptyPinwheel, ActiveClock, NumTextNode, Puff).SetSize(300, 300).SetOrigin(150, 150);
    }

    public CustomNode BuildClock()
    {
        Blades = BuildBlades();
        Shuriken = ImageNodeFromPart(0, 2).SetPos(100, 100)
                                          .SetSize(120, 120)
                                          .SetOrigin(60, 60)
                                          .Hide();

        Whirl = ImageNodeFromPart(0, 1).SetPos(46, 50)
                                       .SetSize(224, 224)
                                       .SetOrigin(112, 112)
                                       .SetRotation((float)PI)
                                       .SetAlpha(0)
                                       .SetImageFlag(32);

        return new CustomNode(CreateResNode(), Blades, Shuriken, Whirl).SetPos(-10, -10)
                                                                       .SetScale(0.9f)
                                                                       .SetSize(160, 160)
                                                                       .SetOrigin(80, 80);
    }

    public CustomNode BuildBlades()
    {
        ClockHand = ImageNodeFromPart(0, 3).SetPos(88, 24)
                                           .SetSize(144, 160)
                                           .SetOrigin(72, 146)
                                           .Hide();

        var blades = new List<CustomNode>();
        for (var i = 0; i < 6; i++) blades.Add(BuildBlade(i));

        blades.Add(ClockHand);

        return new CustomNode(CreateResNode(), blades.ToArray()).SetPos(0, -12)
                                                                .SetSize(320, 320)
                                                                .SetOrigin(158, 174);
    }

    public CustomNode BuildBlade(int i) =>
        ImageNodeFromPart(0, 0).SetPos(128, 30)
                               .SetSize(120, 152)
                               .SetOrigin(32, 144)
                               .SetRotation((1.0471975511966f * i) - 0.0698131700797732f)
                               .SetAlpha(0);

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
        if (Tracker.WidgetConfig.HutonReplicaCfg == null && ShouldInvertByDefault) { Config.Invert = true; }
    }

    public override void ResetConfigs() => Config = new();

    public override void ApplyConfigs()
    {
        WidgetContainer.SetPos(Config.Position-new Vector2(75))
                  .SetScale(Config.Scale/2f);

        ClockHand.SetMultiply(Config.HandColor);

        NumTextNode.ApplyProps(Config.NumTextProps, new(86, 144));
    }

    public override void DrawUI()
    {
        switch (UiTab)
        {
            case Layout:
                PositionControls("Position", ref Config.Position);
                ScaleControls("Scale", ref Config.Scale);
                break;
            case Colors:
                ColorPickerRGB("Blade", ref Config.ActiveColor);
                ColorPickerRGB("Fade", ref Config.FadeColor);
                ColorPickerRGB("Clock Hand", ref Config.HandColor);
                break;
            case Behavior:
                ToggleControls("Turn Smoothly", ref Config.Smooth);
                ToggleControls("Invert Fill", ref Config.Invert);
                break;
            case Text:
                NumTextControls($"{Tracker.TermGauge} Text", ref Config.NumTextProps);
                break;
            default:
                break;
        }

        if (UpdateFlag.HasFlag(Save))
        {
            ApplyConfigs();
            Config.WriteToTracker(Tracker);
        }
    }

    public override Bounds GetBounds() => WidgetContainer;

    #endregion

}

public partial class WidgetConfig
{
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] public HutonReplicaConfig? HutonReplicaCfg { get; set; }
}
