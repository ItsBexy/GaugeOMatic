using GaugeOMatic.Trackers;
using GaugeOMatic.Windows;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Numerics;
using static CustomNodes.CustomNodeManager;
using static GaugeOMatic.Utility.Color;
using static GaugeOMatic.Widgets.AddersCounter;
using static GaugeOMatic.Widgets.WidgetTags;
using static GaugeOMatic.Widgets.WidgetUI;
using static GaugeOMatic.Windows.UpdateFlags;

namespace GaugeOMatic.Widgets;

public sealed unsafe class AddersCounter : CounterWidget
{
    public override WidgetInfo WidgetInfo => GetWidgetInfo;

    public static WidgetInfo GetWidgetInfo => new() 
    { 
        DisplayName = "Addersgall Gems",
        Author = "ItsBexy",
        Description = "A set of gems recreating those on the Addersgall Gauge.",
        WidgetTags = Counter | Replica | MultiComponent,
        KeyText = "AG2"
    };

    public override CustomPartsList[] PartsLists { get; } = {
        new("ui/uld/JobHudGFF1.tex", 
            new(88, 56, 36, 44), // 0 frame
            new(124,56,20,24), // 1 gem (active)
            new(144,56,20,24), // 2 gem (inactive)
            new(164,56,32,32), // 3 halo
            new(176,88,24,12),  // 4 streak
            new(144,80,20,20), // 5 dot
            new(124,80,20,20) // 6 star
            ),
        new("ui/uld/JobHudGFF1.tex",
            new(0,148,50,50),
            new(50,148,50,50),
            new(100,148,50,50),
            new(150,148,50,50)
            )
    };

    #region Nodes
    
    public List<CustomNode> Stacks = new();
    public List<CustomNode> Effects = new();
    public List<CustomNode> Frames = new();
    public List<CustomNode> Gems = new();

    public override CustomNode BuildRoot()
    {
        var count = Config.AsTimer ? Config.TimerSize : Tracker.GetCurrentData().MaxCount;

        BuildStacks(count);

        return new CustomNode(CreateResNode(), Stacks.ToArray());
    }

    private void BuildStacks(int count)
    {
        Stacks = new();

        for (var i = 0; i < count; i++) Stacks.Add(CreateStack(i).SetX(30 * i).SetSize(36, 44).SetOrigin(18,22));
    }

    private CustomNode CreateStack(int i)
    {
        var inactiveGem = ImageNodeFromPart(0,2).SetPos(8,9)
                                                .SetImageWrap(2)
                                                .SetMultiply(80)
                                                .SetAlpha(255);

        var gem2 = ImageNodeFromPart(0, 1).SetPos(8, 9)
                                          .SetImageWrap(2)
                                          .SetImageFlag(32)
                                          .SetAlpha(0);

        Gems.Add(new CustomNode(CreateResNode(), ImageNodeFromPart(0, 1).SetPos(8, 9)
                                                                        .SetImageWrap(2), 
                                                 ImageNodeFromPart(1, 3).SetPos(-7,-4)
                                                                        .SetScale(0.5f,0.6f)
                                                                        .SetOrigin(25,25)
                                                                        .SetImageWrap(2)
                                                                        .SetImageFlag(32)).SetAlpha(0));

        SetupGemPulse(Gems[i], i);
        Frames.Add(ImageNodeFromPart(0, 0).SetImageWrap(2));
        Effects.Add(BuildEffects());

        return new(CreateResNode(), inactiveGem, Gems[i], gem2, Frames[i], Effects[i]);
    }

    private CustomNode BuildEffects()
    {
        CustomNode CreateHalo() => ImageNodeFromPart(0, 3).SetPos(0, 3)
                                                          .SetSize(36, 36)
                                                          .SetScale(0.5f)
                                                          .SetRotation(1.5707964f)
                                                          .SetOrigin(18, 18)
                                                          .SetImageFlag(32)
                                                          .SetImageWrap(2)
                                                          .SetAlpha(0);

        var streak1 = ImageNodeFromPart(0, 4).SetPos(20, -20)
                                            .SetScale(0.2f, 0.1f)
                                            .SetRotation(1.5707964f)
                                            .SetOrigin(0, 6)
                                            .SetAlpha(0)
                                            .SetImageFlag(32)
                                            .SetImageWrap(2);

        var streak2 = ImageNodeFromPart(0, 4).SetPos(14, -20)
                                             .SetAlpha(0)
                                             .SetScale(1.4f, 0.5f)
                                             .SetRotation(1.5707964f)
                                             .SetOrigin(0, 6)
                                             .SetImageFlag(32)
                                             .SetImageWrap(2);

        var dot = ImageNodeFromPart(0, 5).SetPos(4, -4)
                                         .SetOrigin(10, 0)
                                         .SetDrawFlags(0xA)
                                         .SetImageFlag(32)
                                         .SetImageWrap(2)
                                         .SetAlpha(0);

        var star1 = ImageNodeFromPart(0, 6).SetPos(16, -12)
                                          .SetScale(0.5f)
                                          .SetOrigin(10, 10)
                                          .SetAlpha(0)
                                          .SetImageFlag(32)
                                          .SetImageWrap(2);

        var star2 = ImageNodeFromPart(0, 6).SetPos(-2, 0)
                                           .SetScale(0.8f)
                                           .SetOrigin(10, 10)
                                           .SetAlpha(0)
                                           .SetImageFlag(32)
                                           .SetImageWrap(2);

        var streak3 = ImageNodeFromPart(0, 4).SetPos(6, 15)
                                             .SetScale(1.5240965f, 0.2128514f)
                                             .SetOrigin(12, 6)
                                             .SetAlpha(0)
                                             .SetImageFlag(32)
                                             .SetImageWrap(2);

        return new CustomNode(CreateResNode(), CreateHalo(), CreateHalo(), CreateHalo(), streak1, streak2, dot, star1, star2, streak3).SetOrigin(18, 21);
    }

    private void SetupGemPulse(CustomNode gemContainer, int i)
    {
        Tween.ClearLabelTweens(ref Tweens, $"GemPulse{i}");
        var offset = Config.GemColor + new AddRGB(10, -49, -82);
        Tweens.Add(new(gemContainer[0],
                       new(0) { AddRGB = new AddRGB(0, 4, 5) + offset },
                       new(1150) { AddRGB = new AddRGB(9, 45, 54) + offset },
                       new(2300) { AddRGB = new AddRGB(0, 4, 5) + offset })
                       { Repeat = true, Ease = Tween.Eases.SinInOut,Label= $"GemPulse{i}" });

        Tweens.Add(new(gemContainer[1],
                       new(0) { Alpha = 102, PartId = 0 },
                       new(133) { Alpha = 102, PartId = 1 },
                       new(267) { Alpha = 102, PartId = 2 },
                       new(400) { Alpha = 102, PartId = 3 },
                       new(1325) { Alpha = 0, PartId = 3 },
                       new(2300) { Alpha = 0, PartId = 3 })
                       { Repeat = true,Label= $"GemPulse{i}" });
    }

    #endregion

    #region Animations

    public override void ShowStack(int i)
    {
        Stacks[i][0].SetAlpha(0);
        Stacks[i][1].SetAlpha(255);

        Tweens.Add(new(Stacks[i][2],
                       new(0) { Alpha = 0,AddRGB=0 },
                       new(160) { Alpha = 255,AddRGB=Config.GemColor },
                       new(630) { Alpha = 0, AddRGB = 0 }));

        Tweens.Add(new(Effects[i][8],
                       new(0) { Alpha = 255,ScaleX=1.5f,ScaleY=1 },
                       new(160) { Alpha = 255,ScaleX=3,ScaleY=1},
                       new(630) { Alpha = 0,ScaleX=1.5f,ScaleY=0.2f }));
    }

    public override void HideStack(int i)
    {
        Tweens.Add(new(Stacks[i][0],
                       new(0){Alpha=255,AddRGB=100},
                       new(420){Alpha=255,AddRGB=0}));

        Tweens.Add(new(Stacks[i][1],
                       new(0) { Alpha = 255 },
                       new(420) { Alpha = 0 }));

        Tweens.Add(new(Stacks[i][2],
                       new(0) { Alpha = 0 },
                       new(130) { Alpha = 255 },
                       new(465) { Alpha = 0 }));

        Tweens.Add(new(Effects[i][0],
                       new(0) { Alpha = 255,Scale=0.5f },
                       new(160) { Alpha = 200,Scale=1.5f },
                       new(260) { Alpha = 0,Scale=2 }));

        Tweens.Add(new(Effects[i][1],
                       new(0) { Alpha = 0, Scale = 0.5f },
                       new(99) { Alpha = 0, Scale = 0.5f },
                       new(100) { Alpha = 255, Scale = 0.5f },
                       new(260) { Alpha = 200, Scale = 1.5f },
                       new(360) { Alpha = 0, Scale = 2 }));

        Tweens.Add(new(Effects[i][2],
                       new(0) { Alpha = 0, Scale = 0.5f },
                       new(199) { Alpha = 0, Scale = 0.5f },
                       new(200) { Alpha = 255, Scale = 0.5f },
                       new(360) { Alpha = 200, Scale = 1.5f },
                       new(460) { Alpha = 0, Scale = 2 }));

        Tweens.Add(new(Effects[i][3],
                       new(0) { ScaleX = 1.4f, ScaleY = 0.5f, Alpha = 255,Y=-16 },
                       new(240) { ScaleX = 0.2f, ScaleY = 0.1f, Alpha = 0,Y=-20 }));

        Tweens.Add(new(Effects[i][4],
                       new(0) {  X=6,Y=10,ScaleX=1.4f,ScaleY=0.5f,Alpha=255},
                       new(239) {X=6,Y=-10,ScaleX=0.2f,ScaleY=0.1f,Alpha=255 },
                       new(241) {X=14,Y=-10,ScaleX=1.4f,ScaleY=0.5f,Alpha=255 },
                       new(560) {X=14,Y=-20,ScaleX=0.2f,ScaleY=0.1f,Alpha=0 }));

        Tweens.Add(new(Effects[i][5],
                       new(0) { X = 4, Y = -4, Alpha = 255 },
                       new(160) { X = 4, Y = -12, Alpha = 0 },
                       new(329) { X = 4, Y = -12,Alpha = 0 },
                       new(330) { X = 14, Y = 8, Alpha = 255 },
                       new(560) { X = 14, Y = -7.6f, Alpha = 0 }));

        Tweens.Add(new(Effects[i][6],
                       new(0) { Y=0,Scale=0.8f,Alpha=255 },
                       new(400) { Y=-16,Scale=0.4f,Alpha=0}));

        Tweens.Add(new(Effects[i][7],
                       new(0) { Y = 0, Scale = 0.8f, Alpha = 255 },
                       new(560) { Y = -10, Scale = 0.3f, Alpha = 0 }));
    }

    private void AllVanish()
    {
        Tweens.Add(new(WidgetRoot,
                       new(0) { Alpha = 255, AddRGB = 0 },
                       new(200) { Alpha = 0, AddRGB = 100 }));
    }

    private void AllAppear() =>
        Tweens.Add(new(WidgetRoot,
                       new(0) { Alpha = 0, AddRGB = 100 },
                       new(200) { Alpha = 255, AddRGB = 0 }));

    #endregion

    #region UpdateFuncs

    public override string? SharedEventGroup => null;

    public override void OnFirstRun(int count, int max)
    {
        for (var i = 0; i < count; i++) Stacks[i][1].SetAlpha(255);
        FirstRun = false;
    }

    public override void OnDecreaseToMin() { if (Config.HideEmpty) AllVanish(); }

    public override void OnIncreaseFromMin() { if (Config.HideEmpty || WidgetRoot.Alpha < 255) { AllAppear(); } }

    #endregion

    #region Configs

    public class AddersCounterConfig : CounterWidgetConfig
    {
        public Vector2 Position = new(0);
        public float Scale = 1;
        public AddRGB GemColor = new(-10,49,82);
        public AddRGB FXColor = new(-200, -100, 255);
        public float Spacing = 30;
        public float Angle;
        public float Curve;
        public ColorRGB FrameColor = new(100);
        public bool HideEmpty;

        public AddersCounterConfig(WidgetConfig widgetConfig)
        {
            var config = widgetConfig.AddersCounterCfg;

            if (config == null) return;

            Position = config.Position;
            Scale = config.Scale;
            GemColor = config.GemColor;
            FXColor = config.FXColor;
            Spacing = config.Spacing;
            Angle = config.Angle;
            Curve = config.Curve;
            FrameColor = config.FrameColor;
            HideEmpty = config.HideEmpty;

            AsTimer = config.AsTimer;
            TimerSize = config.TimerSize;
            InvertTimer = config.InvertTimer;
        }

        public AddersCounterConfig() { }
    }

    public override CounterWidgetConfig GetConfig => Config;

    public AddersCounterConfig Config = null!;

    public override void InitConfigs() => Config = new(Tracker.WidgetConfig);

    public override void ResetConfigs() => Config = new();

    public override void ApplyConfigs()
    {
        var widgetAngle = Config.Angle+(Config.Curve/2f);
        WidgetRoot.SetPos(Config.Position+new Vector2(-48,-38))
                  .SetScale(Config.Scale)
                  .SetRotation(widgetAngle,true);

        var posAngle = 0f;
        double x = 0;
        double y = 0;
        for (var i = 0; i < Stacks.Count; i++)
        {
            Effects[i].SetAddRGB(Config.FXColor);
            
            SetupGemPulse(Gems[i], i);

            Frames[i].SetMultiply(Config.FrameColor);

            var gemAngle = Config.Curve * (i - 0.5f);

            Effects[i].SetRotation(-gemAngle -widgetAngle , true);
            
            Stacks[i].SetPos((float)x, (float)y)
                     .SetRotation(gemAngle, true);

            x += Math.Cos(posAngle * (Math.PI / 180)) * Config.Spacing;
            y += Math.Sin(posAngle * (Math.PI / 180)) * Config.Spacing;
            posAngle += Config.Curve;
        }
    }

    public override void DrawUI(ref WidgetConfig widgetConfig, ref UpdateFlags update)
    {
        Heading("Layout");

        PositionControls("Position", ref Config.Position, ref update);
        ScaleControls("Scale", ref Config.Scale, ref update);
        FloatControls("Spacing", ref Config.Spacing, 0, 1000, 0.5f, ref update);
        FloatControls("Angle", ref Config.Angle, -180, 180, 1f, ref update);
        FloatControls("Curve", ref Config.Curve, -180, 180, 1f, ref update);

        Heading("Colors");
        ColorPickerRGB("Gem Color", ref Config.GemColor, ref update);
        ColorPickerRGB("Frame Tint", ref Config.FrameColor, ref update);
        ColorPickerRGB("Effects Color", ref Config.FXColor, ref update);

        Heading("Behavior");
        if (ToggleControls("Hide Empty", ref Config.HideEmpty, ref update))
        {
            if (Config.HideEmpty && Tracker.CurrentData.Count == 0) AllVanish();
            if (!Config.HideEmpty && WidgetRoot.Alpha < 255) AllAppear();
        }

        if (ToggleControls($"Use as {Tracker.TermGauge}", ref Config.AsTimer, ref update)) update |= Reset;
        if (Config.AsTimer)
        {
            if (ToggleControls($"Invert {Tracker.TermGauge}", ref Config.InvertTimer, ref update)) update |= Reset;
            if (IntControls($"{Tracker.TermGauge} Size", ref Config.TimerSize, 1, 30, 1, ref update)) update |= Reset;
        }

        if (update.HasFlag(Save)) ApplyConfigs();
        widgetConfig.AddersCounterCfg = Config;
    }

    #endregion

    public AddersCounter(Tracker tracker) : base(tracker) { }
}

public partial class WidgetConfig
{
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] public AddersCounterConfig? AddersCounterCfg { get; set; }
}
