using GaugeOMatic.Trackers;
using GaugeOMatic.Windows;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Numerics;
using static CustomNodes.CustomNodeManager;
using static CustomNodes.CustomNodeManager.Tween;
using static GaugeOMatic.Utility.Color;
using static GaugeOMatic.Widgets.Common.CommonParts;
using static GaugeOMatic.Widgets.CounterWidgetConfig.CounterPulse;
using static GaugeOMatic.Widgets.PolyglotGem;
using static GaugeOMatic.Widgets.WidgetTags;
using static GaugeOMatic.Widgets.WidgetUI;
using static GaugeOMatic.Windows.UpdateFlags;
#pragma warning disable CS8618

namespace GaugeOMatic.Widgets;

public sealed unsafe class PolyglotGem : CounterWidget
{
    public PolyglotGem(Tracker tracker) : base(tracker) { }

    public override WidgetInfo WidgetInfo => GetWidgetInfo;

    public static WidgetInfo GetWidgetInfo => new() 
    { 
        DisplayName = "Polyglot Gems",
        Author = "ItsBexy",
        Description = "A diamond-shaped counter based on BLM's polyglot counter.",
        WidgetTags = Counter | Replica | MultiComponent,
        MultiCompData = new("EL", "Elemental Gauge Replica", 3)
    };

    public override CustomPartsList[] PartsLists { get; } = { BLM0Parts };

    #region Nodes

    public List<CustomNode> Stacks = new();
    public List<CustomNode> Frames = new();
    public List<CustomNode> GemContainers = new();
    public List<CustomNode> Gems = new();
    public List<CustomNode> Glows1 = new();
    public List<CustomNode> Glows2 = new();

    public override CustomNode BuildRoot()
    {
        var count = Config.AsTimer ? Config.TimerSize : Tracker.GetCurrentData().MaxCount;

        BuildStacks(count);

        return new(CreateResNode(), Stacks.ToArray());
    }

    private void BuildStacks(int count)
    {
        Stacks = new();
        Frames = new();
        GemContainers = new();
        Gems = new();
        Glows1 = new();
        Glows2 = new();

        CustomNode BuildGlowNode() => ImageNodeFromPart(0, 14).SetPos(12, 20).SetScale(2.2f).SetOrigin(15, 23).SetImageFlag(32).SetAlpha(0);

        for (var i = 0; i < count; i++)
        {
            Frames.Add(ImageNodeFromPart(0,19).SetOrigin(27.5f, 41.5f));
            Gems.Add(ImageNodeFromPart(0,13).SetPos(12,18).SetOrigin(15,23).SetAlpha(0));
            Glows1.Add(BuildGlowNode());
            Glows2.Add(BuildGlowNode());

            GemContainers.Add(new(CreateResNode(), Gems[i], Glows1[i]));

            Stacks.Add(new CustomNode(CreateResNode(), 
                                      Frames[i], 
                                      GemContainers[i],
                                      Glows2[i]).SetSize(54,83).SetOrigin(27.5f,41.5f));
        }
    }

    #endregion

    #region Animations

    public override void ShowStack(int i)
    {
        var (flipX, flipY) = FlipFactor(i);

        var colorOffset = Config.GemColor + new AddRGB(-27, 78, -50);
        Tweens.Add(new(Gems[i],
                       new(0) { ScaleX = 2.5f * flipX, ScaleY = flipY * 2.5f, Alpha = 0, AddRGB = colorOffset + new AddRGB(0) },
                       new(150) { ScaleX = flipX, ScaleY = flipY, Alpha = 255, AddRGB = colorOffset + new AddRGB(0) },
                       new(260) { ScaleX = flipX, ScaleY = flipY, Alpha = 255, AddRGB = colorOffset + new AddRGB(145) },
                       new(360) { ScaleX = flipX, ScaleY = flipY, Alpha = 255, AddRGB = colorOffset + new AddRGB(0) }));

        Tweens.Add(new(Glows1[i],
                       new(0) { Scale = 1.8f, Alpha = 0 },
                       new(150) { Scale = 1.8f, Alpha = 200 },
                       new(260) { Scale = 2.5f, Alpha = 0 }));

        Glows2[i].Show();
    }

    public override void HideStack(int i)
    {
        var (flipX, flipY) = FlipFactor(i);

        Tweens.Add(new(Gems[i],
                       new(0){ScaleX=flipX, ScaleY = flipY, Alpha =255},
                       new(70) {ScaleX=1.6f* flipX, ScaleY = 1.6f * flipY, Alpha =50},
                       new(170){ScaleX=2 * flipX, ScaleY = 2f * flipY, Alpha =0}));

        Tweens.Add(new(Glows1[i],
                       new(0) { Scale = 0f, Alpha = 0 },
                       new(150) { Scale = 1.8f, Alpha = 200 },
                       new(260) { Scale = 2.2f, Alpha = 0 }));

        Glows2[i].Hide();
    }

    private (float flipX, float flipY) FlipFactor(int i) => (Gems[i].ScaleX < 0 ? -1f : 1f, Gems[i].ScaleY < 0 ? -1f : 1f);

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

    private void PulseAll()
    {
        ClearLabelTweens(ref Tweens, "Pulse");
        for (var i = 0; i < Stacks.Count; i++)
        {
            Tweens.Add(new(GemContainers[i],
                           new(0) { AddRGB = 0 },
                           new(450) { AddRGB = 148 },
                           new(900) { AddRGB = 1 },
                           new(1300) { AddRGB = 0 })
            { Repeat = true, Ease = Eases.SinInOut, Label = "Pulse" });
            
            Tweens.Add(new(Glows2[i],
                           new(0) { Scale = 0, Alpha = 0 },
                           new(400) { Scale = 1.8f, Alpha = 152 },
                           new(830) { Scale = 2.2f, Alpha = 0 },
                           new(1300) { Scale = 2.2f, Alpha = 0 }) 
                           { Repeat = true, Ease = Eases.SinInOut, Label = "Pulse" });
        }
    }

    private void StopPulseAll()
    {
        ClearLabelTweens(ref Tweens, "Pulse");
        for (var i = 0; i < Stacks.Count; i++)
        {
            Tweens.Add(new(GemContainers[i],
                           new(0, GemContainers[i]),
                           new(200) { AddRGB = 0 }) 
                           { Label = "Pulse" });

            Tweens.Add(new(Glows2[i],
                           new(0, Glows2[i]),
                           new(200) { Scale=2.2f,Alpha=0 })
                           { Label = "Pulse" });
        }
    }

    #endregion

    #region UpdateFuncs

    public override void OnFirstRun(int count, int max)
    {
        for (var i = 0; i < count; i++) Gems[i].SetAlpha(255);
        FirstRun = false;
    }

    public override void OnDecreaseToMin() { if (Config.HideEmpty) AllVanish(); }
    public override void OnIncreaseFromMin() { if (Config.HideEmpty || WidgetRoot.Alpha < 255) AllAppear(); }

    public bool Pulsing;
    public bool CheckPulse(int i) => i > 0 && (Config.Pulse == Always || (Config.Pulse == AtMax && i == Stacks.Count));

    public override void PostUpdate(int i)
    {
        var checkPulse = CheckPulse(i);
        if (!Pulsing && checkPulse) PulseAll();
        else if (Pulsing && !checkPulse) StopPulseAll();
        Pulsing = checkPulse;
    }

    #endregion

    #region Configs

    public class PolyglotGemConfig : CounterWidgetConfig
    {
        public Vector2 Position = new(0);
        public float Scale = 1;
        public AddRGB GemColor = new (27, -78, 50);
        public AddRGB GlowColor = new (76, -128, 127);
        public float Spacing = 26;
        public float Angle;
        public float Curve;
        public ColorRGB FrameColor = new(100);
        public bool HideEmpty;
        public CounterPulse Pulse = Always;

        public PolyglotGemConfig(WidgetConfig widgetConfig)
        {
            var config = widgetConfig.PolyglotGemCfg;

            if (config == null) return;

            Position = config.Position;
            Scale = config.Scale;
            GemColor = config.GemColor;
            GlowColor = config.GlowColor;
            
            Spacing = config.Spacing;
            Angle = config.Angle;
            Curve = config.Curve;
            FrameColor = config.FrameColor;
            HideEmpty = config.HideEmpty;

            Pulse = config.Pulse;
            AsTimer = config.AsTimer;
            TimerSize = config.TimerSize;
            InvertTimer = config.InvertTimer;
        }

        public PolyglotGemConfig() { }
    }

    public override CounterWidgetConfig GetConfig => Config;

    public PolyglotGemConfig Config;

    public override void InitConfigs() => Config = new(Tracker.WidgetConfig);

    public override void ResetConfigs() => Config = new();

    public override void ApplyConfigs()
    {
        var widgetAngle = Config.Angle+(Config.Curve/2f);
        WidgetRoot.SetPos(Config.Position)
                  .SetScale(Config.Scale)
                  .SetRotation(widgetAngle,true);

        var gemColorOffset = new AddRGB(-27, 78, -50);
        var glowColorOffset = new AddRGB(-76, 128, -127);
        var posAngle = 0f;
        double x = 0;
        double y = 0;
        for (var i = 0; i < Stacks.Count; i++)
        {
            Frames[i].SetMultiply(Config.FrameColor);

            var gemAngle = Config.Curve * (i - 0.5f);
            while (gemAngle + widgetAngle > 180) gemAngle -= 360;
            while (gemAngle + widgetAngle < -180) gemAngle += 360;


            var combinedAngle = gemAngle + widgetAngle;

            Frames[i].SetScaleY(Math.Abs(combinedAngle) > 90?-1:1);

            float scaleX = combinedAngle is <= -53 or >= 128 ? -1 : 1;
            float scaleY = combinedAngle is <= -128 or >= 53 ? -1 : 1;

            Gems[i].SetScale(scaleX,scaleY).SetAddRGB(gemColorOffset + Config.GemColor);
            Glows1[i].SetAddRGB(glowColorOffset + Config.GlowColor);
            Glows2[i].SetAddRGB(glowColorOffset + Config.GlowColor);

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
        FloatControls("Spacing", ref Config.Spacing, -1000, 1000, 0.5f, ref update);
        FloatControls("Angle", ref Config.Angle, -180, 180, 1f, ref update);
        FloatControls("Curve", ref Config.Curve, -180, 180, 1f, ref update);

        Heading("Colors");
        ColorPickerRGB("Gem Color", ref Config.GemColor, ref update);
        ColorPickerRGB("Glow Color", ref Config.GlowColor, ref update);
        ColorPickerRGB("Frame Tint", ref Config.FrameColor, ref update);

        Heading("Behavior");
        if (ToggleControls("Hide Empty", ref Config.HideEmpty, ref update))
        {
            if (Config.HideEmpty && Tracker.CurrentData.Count == 0) AllVanish();
            if (!Config.HideEmpty && WidgetRoot.Alpha < 255) AllAppear();
        }

        RadioControls("Pulse", ref Config.Pulse, new() { Never, AtMax, Always }, new() { "Never", "At Maximum", "Always" }, ref update);

        if (ToggleControls($"Use as {Tracker.TermGauge}", ref Config.AsTimer, ref update)) update |= Reset;
        if (Config.AsTimer)
        {
            if (ToggleControls($"Invert {Tracker.TermGauge}", ref Config.InvertTimer, ref update)) update |= Reset;
            if (IntControls($"{Tracker.TermGauge} Size", ref Config.TimerSize, 1, 30, 1, ref update)) update |= Reset;
        }

        if (update.HasFlag(Save)) ApplyConfigs();
        widgetConfig.PolyglotGemCfg = Config;
    }

    #endregion
}

public partial class WidgetConfig
{
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] public PolyglotGemConfig? PolyglotGemCfg { get; set; }
}
