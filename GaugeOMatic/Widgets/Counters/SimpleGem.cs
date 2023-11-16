using GaugeOMatic.Trackers;
using GaugeOMatic.Windows;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Numerics;
using static CustomNodes.CustomNodeManager;
using static GaugeOMatic.Utility.Color;
using static GaugeOMatic.Widgets.SimpleGem;
using static GaugeOMatic.Widgets.WidgetTags;
using static GaugeOMatic.Widgets.WidgetUI;
using static GaugeOMatic.Windows.UpdateFlags;

namespace GaugeOMatic.Widgets;

public sealed unsafe class SimpleGem : CounterWidget
{
    public override WidgetInfo WidgetInfo => GetWidgetInfo;

    public static WidgetInfo GetWidgetInfo => new() 
    { 
        DisplayName = "Simple Gems",
        Author = "ItsBexy",
        Description = "A diamond-shaped counter based on Simple Mode job gauges.",
        WidgetTags = Counter | Replica
    };

    public override CustomPartsList[] PartsLists { get; } = {
        new("ui/uld/JobHudSimple_StackA.tex", 
            new(0, 0, 32, 32),
            new(32, 0, 32, 32), 
            new(0, 32, 32, 32),
            new(32,32,32,32)
            ),      
        new("ui/uld/JobHudSimple_StackB.tex",
            new(0, 0, 32, 32),
            new(32, 0, 32, 32),
            new(0, 32, 32, 32)
        )
    };

    #region Nodes
    
    public List<CustomNode> Stacks = new();
    public List<CustomNode> Frames = new();
    public List<CustomNode> Gems = new();

    public override CustomNode BuildRoot()
    {
        var count = Config.AsTimer ? Config.TimerSize : Tracker.GetCurrentData().MaxCount;

        BuildStacks(count);

        return new CustomNode(CreateResNode(), Stacks.ToArray()).SetOrigin(16,16);
    }

    private void BuildStacks(int count)
    {
        Stacks = new List<CustomNode>();
        Frames = new List<CustomNode>();
        Gems = new List<CustomNode>();

        for (var i = 0; i < count; i++)
        {
            Frames.Add(ImageNodeFromPart(0, 0)
                           .SetOrigin(16, 16));

            Gems.Add(ImageNodeFromPart(0, 1)
                     .SetMultiply(80)
                     .SetOrigin(16,16).SetAlpha(0));

            Stacks.Add(new CustomNode(CreateResNode(), Frames[i], Gems[i]).SetPos(i*20,0).SetOrigin(16,16));
        }
    }

    #endregion

    #region Animations

    public override void ShowStack(int i)
    {
        Tweens.Add(new(Gems[i],
                       new(0){Scale=2.4f,Alpha=0,AddRGB = Config.GemColor + new AddRGB(80)},
                       new(125) { Scale=1,Alpha=255,AddRGB=Config.GemColor }));
    }

    public override void HideStack(int i)
    {
        Tweens.Add(new(Gems[i],
                       new(0) { Alpha = 255, AddRGB = Config.GemColor,Scale=1 },
                       new(90) { Alpha = 0, AddRGB = Config.GemColor + new AddRGB(80),Scale=0.8f }));
    }

    private void AllVanish() =>
        Tweens.Add(new(WidgetRoot,
                       new(0){Alpha=255,AddRGB=0},
                       new(200){Alpha=0,AddRGB=100}));

    private void AllAppear() =>
        Tweens.Add(new(WidgetRoot,
                       new(0) { Alpha = 0, AddRGB = 100 },
                       new(200) { Alpha = 255, AddRGB = 0 }));

    #endregion

    #region UpdateFuncs

    public override string? SharedEventGroup => null;

    public override void OnFirstRun(int count, int max)
    {
        for (var i = 0; i < count; i++) Gems[i].SetAlpha(255);
        FirstRun = false;
    }

    public override void OnDecreaseToMin() { if (Config.HideEmpty) AllVanish(); }

    public override void OnIncreaseFromMin() { if (Config.HideEmpty || WidgetRoot.Alpha < 255) { AllAppear(); } }

    #endregion

    #region Configs

    public class SimpleGemConfig : CounterWidgetConfig
    {
        public Vector2 Position = new(0);
        public float Scale = 1;
        public AddRGB GemColor = new(120,30,-40);
        public int GemType;
        public float Spacing = 20;
        public float Angle;
        public float Curve;
        public ColorRGB FrameColor = new(100);
        public bool HideEmpty;

        public SimpleGemConfig(WidgetConfig widgetConfig)
        {
            var config = widgetConfig.SimpleGemCfg;

            if (config == null) return;

            Position = config.Position;
            Scale = config.Scale;
            GemColor = config.GemColor;
            GemType = config.GemType;
            Spacing = config.Spacing;
            Angle = config.Angle;
            Curve = config.Curve;
            FrameColor = config.FrameColor;
            HideEmpty = config.HideEmpty;

            AsTimer = config.AsTimer;
            TimerSize = config.TimerSize;
            InvertTimer = config.InvertTimer;
        }

        public SimpleGemConfig() { }
    }

    public override CounterWidgetConfig GetConfig => Config;

    public SimpleGemConfig Config = null!;

    public override void InitConfigs() => Config = new(Tracker.WidgetConfig);

    public override void ResetConfigs() => Config = new();

    public override void ApplyConfigs()
    {
        WidgetRoot.SetPos(Config.Position)
                  .SetScale(Config.Scale)
                  .SetRotation(Config.Angle+(Config.Curve/2f),true);

        var posAngle = 0f;
        double x = 0;
        double y = 0;
        for (var i = 0; i < Stacks.Count; i++)
        {
            Gems[i].Node->GetAsAtkImageNode()->PartsList = PartsLists[Config.GemType].AtkPartsList;
            Frames[i].Node->GetAsAtkImageNode()->PartsList = PartsLists[Config.GemType].AtkPartsList;

            var gemAngle = Config.Curve * (i - 0.5f);

            if (Config.GemType == 0) gemAngle = AdjustDiamondAngle(gemAngle);

            Stacks[i].SetPos((float)x, (float)y)
                     .SetRotation(gemAngle, true);
            Gems[i].SetAddRGB(Config.GemColor);
            Frames[i].SetMultiply(Config.FrameColor);
            x += Math.Cos(posAngle * (Math.PI / 180)) * Config.Spacing;
            y += Math.Sin(posAngle * (Math.PI / 180)) * Config.Spacing;
            posAngle += Config.Curve;
        }
    }

    private float AdjustDiamondAngle(float gemAngle)
    {
        var offset = Config.Angle + (Config.Curve / 2f);
        while (gemAngle + offset >= 45) gemAngle -= 90;
        while (gemAngle + offset < -45) gemAngle += 90;
        return gemAngle;
    }

    public override void DrawUI(ref WidgetConfig widgetConfig, ref UpdateFlags update)
    {
        Heading("Layout");
        RadioControls("Style", ref Config.GemType, new() { 0,1 }, new() { "Diamond", "Chevron" }, ref update);
        PositionControls("Position", ref Config.Position, ref update);
        ScaleControls("Scale", ref Config.Scale, ref update);
        FloatControls("Spacing", ref Config.Spacing, 0, 1000, 0.5f, ref update);
        FloatControls("Angle", ref Config.Angle, -180, 180, 1f, ref update);
        FloatControls("Curve", ref Config.Curve, -180, 180, 1f, ref update);

        Heading("Colors");
        ColorPickerRGB("Gem Color", ref Config.GemColor, ref update);
        ColorPickerRGB("Frame Tint", ref Config.FrameColor, ref update);

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
        widgetConfig.SimpleGemCfg = Config;
    }

    #endregion

    public SimpleGem(Tracker tracker) : base(tracker) { }
}

public partial class WidgetConfig
{
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] public SimpleGemConfig? SimpleGemCfg { get; set; }
}
