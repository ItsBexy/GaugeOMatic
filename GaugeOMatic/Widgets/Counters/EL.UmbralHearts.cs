using GaugeOMatic.Trackers;
using GaugeOMatic.Windows;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Numerics;
using FFXIVClientStructs.FFXIV.Component.GUI;
using static CustomNodes.CustomNodeManager;
using static CustomNodes.CustomNodeManager.Tween;
using static GaugeOMatic.Utility.Color;
using static GaugeOMatic.Widgets.Common.CommonParts;
using static GaugeOMatic.Widgets.UmbralHearts;
using static GaugeOMatic.Widgets.WidgetTags;
using static GaugeOMatic.Widgets.WidgetUI;
using static GaugeOMatic.Windows.UpdateFlags;
#pragma warning disable CS8618

namespace GaugeOMatic.Widgets;

public sealed unsafe class UmbralHearts : CounterWidget
{
    public UmbralHearts(Tracker tracker) : base(tracker) { }

    public override WidgetInfo WidgetInfo => GetWidgetInfo;

    public static WidgetInfo GetWidgetInfo => new() 
    { 
        DisplayName = "Umbral Hearts",
        Author = "ItsBexy",
        Description = "A counter based on BLM's Umbral Heart counter",
        WidgetTags = Counter | Replica | MultiComponent,
        MultiCompData = new("EL", "Elemental Gauge Replica", 5)
    };

    public override CustomPartsList[] PartsLists { get; } = { BLM0Parts };

    #region Nodes

    public List<CustomNode> Stacks = new();
    public List<CustomNode> Hearts = new();
    public List<CustomNode> GlowWrappers = new();
    public List<CustomNode> Glows = new();

    public override CustomNode BuildRoot()
    {
        var count = Config.AsTimer ? Config.TimerSize : Tracker.GetCurrentData().MaxCount;

        BuildStacks(count);

        return new CustomNode(CreateResNode(), Stacks.ToArray()).SetOrigin(12,34);
    }

    private void BuildStacks(int count)
    {
        Stacks = new();
        Hearts = new();

        for (var i = 0; i < count; i++)
        {
            Hearts.Add(ImageNodeFromPart(0,5).SetPos(0,-20).SetOrigin(12,34).SetAlpha(0));

            Glows.Add(ImageNodeFromPart(0,20).SetScale(1.3f,1.2f).SetOrigin(12,34).SetImageWrap(1));

            Tweens.Add(new(Glows[i],
                           new(0) { ScaleX = 1, ScaleY = 1, Alpha = 4 },
                           new(300) { ScaleX = 1.2f, ScaleY = 1.1f, Alpha = 152 },
                           new(630) { ScaleX = 1.3f, ScaleY = 1.2f, Alpha = 0 },
                           new(960) { ScaleX = 1.3f, ScaleY = 1.2f, Alpha = 0 }) 
                           { Repeat=true, Ease=Eases.SinInOut });

            Glows[i].UnsetNodeFlags(NodeFlags.UseDepthBasedPriority);
            GlowWrappers.Add(new CustomNode(CreateResNode(), Glows[i]).SetAlpha(0));

            Stacks.Add(new CustomNode(CreateResNode(), Hearts[i], GlowWrappers[i]).SetOrigin(12,34));
        }
    }

    #endregion

    #region Animations

    public override void ShowStack(int i)
    {
        Tweens.Add(new(Hearts[i],
                       new(0) {Y=-20,Alpha=0}, 
                       new(200) {Y=0,Alpha=200},
                       new(300){Y=0,Alpha=255}
                       ));

        Tweens.Add(new(GlowWrappers[i],new(0){Alpha=0},new(300){Alpha=255}));
    }

    public override void HideStack(int i)
    {
        Tweens.Add(new(Hearts[i],
                       new(0) { Y = 0, Alpha = 255 },
                       new(200) { Y = -20, Alpha = 255 },
                       new(300) { Y = -20, Alpha = 0 }
                   ));
        Tweens.Add(new(GlowWrappers[i], new(0) { Alpha = 255 }, new(300) { Alpha =0 }));
    }

    #endregion

    #region UpdateFuncs

    public override void OnFirstRun(int count, int max)
    {
        for (var i = 0; i < count; i++)
        {
            Hearts[i].SetAlpha(255).SetY(0);
            GlowWrappers[i].SetAlpha(255);
        }
        FirstRun = false;
    }

    #endregion

    #region Configs

    public class UmbralHeartConfig : CounterWidgetConfig
    {
        public Vector2 Position = new(0);
        public float Scale = 1;
        public AddRGB StackColor = new(0);
        public AddRGB GlowColor = new(0);
        public float Spacing = -16.5f;
        public float Angle = -126;
        public float Curve = -20;

        public UmbralHeartConfig(WidgetConfig widgetConfig)
        {
            var config = widgetConfig.UmbralHeartCfg;

            if (config == null) return;

            Position = config.Position;
            Scale = config.Scale;
            StackColor = config.StackColor;
            
            GlowColor = config.GlowColor;
            Spacing = config.Spacing;
            Angle = config.Angle;
            Curve = config.Curve;

            AsTimer = config.AsTimer;
            TimerSize = config.TimerSize;
            InvertTimer = config.InvertTimer;
        }

        public UmbralHeartConfig() { }
    }

    public override CounterWidgetConfig GetConfig => Config;

    public UmbralHeartConfig Config;

    public override void InitConfigs() => Config = new(Tracker.WidgetConfig);

    public override void ResetConfigs() => Config = new();

    public override void ApplyConfigs()
    {

        var widgetAngle = Config.Angle+(Config.Curve/2f);
        WidgetRoot.SetPos(Config.Position+new Vector2(19,22))
                  .SetScale(Config.Scale)
                  .SetRotation(widgetAngle,true);

        var posAngle = 0f;
        double x = 0;
        double y = 0;
        for (var i = 0; i < Stacks.Count; i++)
        {
            Hearts[i].SetAddRGB(Config.StackColor);
            GlowWrappers[i].SetAddRGB(Config.GlowColor);
            var gemAngle = Config.Curve * (i - 0.5f);

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
        ColorPickerRGB("Color Modifier", ref Config.StackColor, ref update);
        ColorPickerRGB("Glow Color", ref Config.GlowColor, ref update);

        Heading("Behavior");

        if (ToggleControls($"Use as {Tracker.TermGauge}", ref Config.AsTimer, ref update)) update |= Reset;
        if (Config.AsTimer)
        {
            if (ToggleControls($"Invert {Tracker.TermGauge}", ref Config.InvertTimer, ref update)) update |= Reset;
            if (IntControls($"{Tracker.TermGauge} Size", ref Config.TimerSize, 1, 30, 1, ref update)) update |= Reset;
        }

        if (update.HasFlag(Save)) ApplyConfigs();
        widgetConfig.UmbralHeartCfg = Config;
    }

    #endregion
}

public partial class WidgetConfig
{
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] public UmbralHeartConfig? UmbralHeartCfg { get; set; }
}
