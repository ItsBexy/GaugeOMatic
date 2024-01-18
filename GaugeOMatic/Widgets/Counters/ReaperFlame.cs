using FFXIVClientStructs.FFXIV.Component.GUI;
using GaugeOMatic.Trackers;
using GaugeOMatic.Windows;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Numerics;
using static CustomNodes.CustomNodeManager;
using static CustomNodes.CustomNodeManager.Tween;
using static GaugeOMatic.Utility.Color;
using static GaugeOMatic.Widgets.ReaperFlame;
using static GaugeOMatic.Widgets.WidgetTags;
using static GaugeOMatic.Widgets.WidgetUI;
using static GaugeOMatic.Windows.UpdateFlags;
#pragma warning disable CS8618

namespace GaugeOMatic.Widgets;

public sealed unsafe class ReaperFlame : CounterWidget
{
    public ReaperFlame(Tracker tracker) : base(tracker) { }

    public override WidgetInfo WidgetInfo => GetWidgetInfo;

    public static WidgetInfo GetWidgetInfo => new()
    {
        DisplayName = "Shroud Flames",
        Author = "ItsBexy",
        Description = "A counter imitating the shroud stack display on Reaper's Death Gauge.",
        WidgetTags = Counter | Replica
    };

    public override CustomPartsList[] PartsLists { get; } = {
        new("ui/uld/JobHudRRP1.tex",
            new(0, 130, 36, 44),
            new(36, 130, 36, 44),
            new(72, 130, 36, 44),
            new(108, 130, 36, 44),
            new(144, 130, 36, 44),
            new(180, 130, 36, 44),
            new(154, 88, 32, 32),
            new(186, 88, 32, 32),
            new(154, 272, 32, 32),
            new(154, 238, 60, 34))}; // horizontal pulsar thing

    #region Nodes

    public List<CustomNode> Stacks = new();
    public List<CustomNode> Flames = new();
    public List<CustomNode> FlameTwins = new();
    public List<CustomNode> Orbs = new();
    public List<CustomNode> Halos = new();
    public List<CustomNode> Pulsars = new();

    public override CustomNode BuildRoot()
    {
        var count = Config.AsTimer ? Config.TimerSize : Tracker.GetCurrentData().MaxCount;
        Stacks = BuildStacks(count);
        return new(CreateResNode(), Stacks.ToArray());
    }

    private List<CustomNode> BuildStacks(int count)
    {
        var stacks = new List<CustomNode>();
        for (var i = 0; i < count; i++) stacks.Add(BuildFlameStack(i));
        return stacks;
    }

    public CustomNode BuildFlameStack(int i)
    {
        var flame = ImageNodeFromPart(0, 0).SetPos(-2, -18).SetOrigin(18, 44).SetAlpha(0);
        var flameTwin = ImageNodeFromPart(0, 0).SetPos(-2, -18).SetOrigin(18, 44).SetAlpha(0).SetImageFlag(32);
        var orb = ImageNodeFromPart(0, 6).SetOrigin(16, 16).SetAlpha(0);
        var halo = ImageNodeFromPart(0, 8).SetPos(-1, 0).SetOrigin(16, 16).SetScale(1.428571f).SetAlpha(0).SetImageFlag(32);
        var pulsar = ImageNodeFromPart(0,9).SetPos(-13,0).SetOrigin(30,17).SetScale(0.5f,0.55f).SetAlpha(0).SetRotation(-1.2f).SetImageFlag(32);

        Flames.Add(flame);
        FlameTwins.Add(flameTwin);
        Orbs.Add(orb);
        Halos.Add(halo);
        Pulsars.Add(pulsar);

        var tMod = 1 % 10 * (i % 2 == 0 ? -4 : 4);
        Tweens.Add(new(flame, new(0) { PartId = 0 },new(475 + tMod){PartId = 5}) { Repeat = true });
        Tweens.Add(new(flameTwin, new(0) { PartId = 0 },new(475 + tMod){PartId = 5}) { Repeat = true });

        return new CustomNode(CreateResNode(), flame, orb, halo, pulsar, flameTwin).SetSize(36, 44);
    }

    #endregion

    #region Animations

    public override void ShowStack(int i)
    {
        ClearLabelTweens(ref Tweens, $"HideAnim{i}");

        Tweens.Add(new(Flames[i], new(0) { Alpha = 0, X = -2, Y = -18 }, new(200) { Alpha = 255, X = -2, Y = -18 }));
        Tweens.Add(new(Orbs[i], new(0) { Alpha = 0 }, new(20) { Alpha = 170 }));

        FlameTwins[i].SetAlpha(0);
    }

    public override void HideStack(int i)
    {
        if (Config.SpendAnim == 1)
        {
            Tweens.Add(new(Flames[i], new(0) { X = -2, Y = -18, Alpha = 255 }, new(500) { X = -5, Y = -38, Alpha = 0 }){Label= $"HideAnim{i}" });
            Tweens.Add(new(FlameTwins[i], new(0) { X = -2, Y = -18, Alpha = 255 }, new(500) { X = 1, Y = -2, Alpha = 0 }) { Label = $"HideAnim{i}" });
        }
        else
        {
            Tweens.Add(new(Flames[i], new(0) { Alpha = 255 }, new(500) { Alpha = 0 }) { Label = $"HideAnim{i}" });
        }

        Tweens.Add(new(Orbs[i], new(0) { Alpha = 170 }, new(20) { Alpha = 0 }) { Label = $"HideAnim{i}" });
        Tweens.Add(HaloFadeTween(Halos[i]));
        Tweens.Add(PulsarWinkTween(Pulsars[i]));
    }

    public static Tween HaloFadeTween(CustomNode node) =>
        new(node, new(0) { Alpha = 255, ScaleX = 1, ScaleY = 1 },
            new(250) { Alpha = 0, ScaleX = 1.5f, ScaleY = 1.5f });
    public static Tween PulsarWinkTween(AtkResNode* node) =>
        new(node, new(0) { Alpha = 0, ScaleX = 0.5f, ScaleY = 0.55f },
            new(150) { Alpha = 255, ScaleX = 1.5f, ScaleY = 0f });

    #endregion

    #region UpdateFuncs

    public override void OnFirstRun(int count, int max)
    {
        for (var i = 0; i < count; i++)
        {
            Flames[i].SetAlpha(255);
            Orbs[i].SetAlpha(170);
        }

        for (var i = count; i < max; i++)
        {
            Flames[i].SetAlpha(0);
            Orbs[i].SetAlpha(0);
        }
    }

    #endregion

    #region Configs

    public class ReaperFlameConfig : CounterWidgetConfig
    {
        public Vector2 Position = new(0);
        public float Scale = 1;
        public float Spacing = 20;
        public float Angle;
        public float Curve;
        public AddRGB BaseColor = new(-150, 20, 150);
        public AddRGB OrbColor = new(0, 0, 0);
        public AddRGB FlashColor = new(-120, -50, 0);
        public int SpendAnim;

        public ReaperFlameConfig(WidgetConfig widgetConfig)
        {
            var config = widgetConfig.ReaperFlameCfg;

            if (config == null) return;

            Position = config.Position;
            Scale = config.Scale;
            Spacing = config.Spacing;
            Angle = config.Angle;
            Curve = config.Curve;
            BaseColor = config.BaseColor;
            OrbColor = config.OrbColor;
            FlashColor = config.FlashColor;
            SpendAnim = config.SpendAnim;

            AsTimer = config.AsTimer;
            TimerSize = config.TimerSize;
            InvertTimer = config.InvertTimer;
        }

        public ReaperFlameConfig() { }
    }

    public override CounterWidgetConfig GetConfig => Config;

    public ReaperFlameConfig Config;

    public override void InitConfigs() => Config = new(Tracker.WidgetConfig);

    public override void ResetConfigs() => Config = new();

    public override void ApplyConfigs()
    {
        WidgetRoot.SetPos(Config.Position);
        WidgetRoot.SetScale(Config.Scale);

        var angle = Config.Angle;
        double x = 0;
        double y = 0;

        for (var i = 0; i < Stacks.Count; i++)
        {
            Stacks[i].SetPos((float)x, (float)y).SetAddRGB(Config.BaseColor);
            x += Math.Cos(angle * (Math.PI / 180)) * Config.Spacing;
            y += Math.Sin(angle * (Math.PI / 180)) * Config.Spacing;
            angle += Config.Curve;

            Orbs[i].SetAddRGB(Config.OrbColor);
            Halos[i].SetAddRGB(Config.FlashColor);
            Pulsars[i].SetAddRGB(Config.FlashColor);
            FlameTwins[i].SetAddRGB(Config.FlashColor);
            Pulsars[i].Node->Rotation = Config.SpendAnim == 1 ? -1.2f : 0;
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
        ColorPickerRGB("Base Color", ref Config.BaseColor, ref update);
        ColorPickerRGB("Orb Tint", ref Config.OrbColor, ref update);
        ColorPickerRGB("Flash Color", ref Config.FlashColor, ref update);

        Heading("Behavior");
        RadioControls("Animation", ref Config.SpendAnim, new() { 0, 1 }, new() { "Default", "Divide" }, ref update);

        if (ToggleControls($"Use as {Tracker.TermGauge}", ref Config.AsTimer, ref update)) update |= Reset;
        if (Config.AsTimer)
        {
            if (ToggleControls($"Invert {Tracker.TermGauge}", ref Config.InvertTimer, ref update)) update |= Reset;
            if (IntControls($"{Tracker.TermGauge} Size", ref Config.TimerSize, 1, 20, 1, ref update)) update |= Reset;
        }

        if (update.HasFlag(Save)) ApplyConfigs();
        widgetConfig.ReaperFlameCfg = Config;
    }

    #endregion
}

public partial class WidgetConfig
{
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] public ReaperFlameConfig? ReaperFlameCfg { get; set; }
}
