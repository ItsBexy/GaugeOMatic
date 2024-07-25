using CustomNodes;
using GaugeOMatic.CustomNodes.Animation;
using GaugeOMatic.Trackers;
using GaugeOMatic.Windows;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Numerics;
using static CustomNodes.CustomNodeManager;
using static GaugeOMatic.CustomNodes.Animation.KeyFrame;
using static GaugeOMatic.Utility.Color;
using static GaugeOMatic.Widgets.Common.CommonParts;
using static GaugeOMatic.Widgets.CounterWidgetConfig.CounterPulse;
using static GaugeOMatic.Widgets.KazematoiKunai;
using static GaugeOMatic.Widgets.WidgetTags;
using static GaugeOMatic.Widgets.WidgetUI;
using static GaugeOMatic.Windows.UpdateFlags;
using static System.Math;

#pragma warning disable CS8618

namespace GaugeOMatic.Widgets;

public sealed unsafe class KazematoiKunai : CounterWidget
{
    public KazematoiKunai(Tracker tracker) : base(tracker) { }

    public override WidgetInfo WidgetInfo => GetWidgetInfo;

    public static WidgetInfo GetWidgetInfo => new()
    {
        DisplayName = "Kazematoi Kunai",
        Author = "ItsBexy",
        Description = "A set of kunai recreating those on the Kazematoi Gauge.",
        WidgetTags = Counter | Replica | MultiComponent,
        MultiCompData = new("KZ", "Kazematoi Replica", 3)
    };

    public override CustomPartsList[] PartsLists { get; } = { NIN1 };

    #region Nodes

    public List<CustomNode> Stacks;
    public List<CustomNode> Slots;
    public List<CustomNode> Knives;
    public List<CustomNode> Knives1;
    public List<CustomNode> Knives2;

    public override CustomNode BuildContainer()
    {
        Max = GetMax();
        BuildStacks(Max);

        return new CustomNode(CreateResNode(), Stacks.ToArray()).SetAlpha(0);
    }

    private void BuildStacks(int count)
    {
        Slots = new List<CustomNode>();
        Stacks = new List<CustomNode>();
        Knives = new List<CustomNode>();
        Knives1 = new List<CustomNode>();
        Knives2 = new List<CustomNode>();

        for (var i = 0; i < count; i++)
        {
            var (x, y) = StaggerOffsets(i);

            Slots.Add(ImageNodeFromPart(0, 4).SetImageWrap(2).SetPos(x, y));
            Knives1.Add(ImageNodeFromPart(0, 2).SetImageWrap(2));
            Knives2.Add(new CustomNode(CreateResNode(), ImageNodeFromPart(0, 3).SetImageWrap(2)));

            SetupKnifePulse1(i);
            SetupKnifePulse2(i);

            Knives.Add(new CustomNode(CreateResNode(), Knives1[i], Knives2[i]).SetPos(x, y));

            Stacks.Add(new CustomNode(CreateResNode(), Slots[i], Knives[i]).SetOrigin(9, 69));
        }
    }

    private (int x, int y) StaggerOffsets(int i)
    {
        var even = i % 2 == 0;
        var x = even && Config.Stagger ? 5 : 0;
        var y = !even && Config.Stagger ? 16 : 0;
        return (x, y);
    }

    #endregion

    #region Animations

    private void SetupKnifePulse1(int i)
    {
        Animator -= $"KnifePulse1-{i}";
        Animator += new Tween[]
        {
            new(Knives1[i],
                new(0) { AddRGB = new(0) },
                new(480) { AddRGB = new(50) },
                new(960) { AddRGB = new(0) })
                { Repeat = true, Label = $"KnifePulse1-{i}" }
        };
    }

    private void StopKnifePulse1(int i) => Animator -= $"KnifePulse1-{i}";

    private void SetupKnifePulse2(int i)
    {
        Animator += new Tween[]
        {
            new(Knives2[i][0],
                new(0) { Alpha=0 },
                new(650) { Alpha=128 },
                new(1300) { Alpha=0 })
                { Repeat = true }
        };
    }

    public void PulseAll(int current)
    {
        for (var i = 0; i < current; i++)
        {
            StopKnifePulse1(i);
            Animator += new Tween(Knives2[i], new(0) { Alpha = Knives2[i].Alpha }, Visible[150]);
        }
    }

    public void StopPulseAll()
    {
        for (var i = 0; i < Max; i++)
        {
            Animator -= $"KnifePulse2{i}";
            SetupKnifePulse1(i);
            Animator += new Tween(Knives2[i], new(0) { Alpha = Knives2[i].Alpha }, Hidden[150]);
        }
    }

    public override void ShowStack(int i)
    {
        var (x, y) = StaggerOffsets(i);
        Animator += new Tween(Knives[i],
                              new(0) { X = x + 15, Y = y - 32, Alpha = 0 },
                              new(170) { X = x, Y = y, Alpha = 255 });
    }

    public override void HideStack(int i)
    {
        var (x, y) = StaggerOffsets(i);
        Animator += new Tween(Knives[i],
                              new(0) { X = x, Y = y, Alpha = 255 },
                              new(90) { X = x - 15, Y = y + 32, Alpha = 0 });
    }

    private void AllVanish()
    {
        Animator += new Tween(WidgetContainer,
                              new(0) { Alpha = 255, AddRGB = 0 },
                              new(200) { Alpha = 0, AddRGB = 100 });
    }

    private void AllAppear()
    {
        Animator += new Tween(WidgetContainer,
                              new(0) { Alpha = 0, AddRGB = 100 },
                              new(200) { Alpha = 255, AddRGB = 0 });
    }

    #endregion

    #region UpdateFuncs

    public override void OnFirstRun(int count, int max)
    {
        for (var i = 0; i < max; i++) Knives[i].SetAlpha(i < count);
        if (!Config.HideEmpty || count > 0)
        {
            AllAppear();
        }
        else
        {
            WidgetContainer.SetAlpha(0);
        }
    }

    public override void OnDecreaseToMin() { if (Config.HideEmpty) AllVanish(); }
    public override void OnIncreaseFromMin() { if (Config.HideEmpty || WidgetContainer.Alpha < 255) { AllAppear(); } }

    public bool Pulsing;
    public bool CheckPulse(int i) => i > 0 && (Config.Pulse == Always || (Config.Pulse == AtMax && i == Stacks.Count));
    public override void PostUpdate(int i)
    {
        var checkPulse = CheckPulse(i);
        if (!Pulsing && checkPulse) PulseAll(i);
        else if (Pulsing && !checkPulse) StopPulseAll();
        Pulsing = checkPulse;
    }
    #endregion

    #region Configs

    public class KazematoiKunaiConfig : CounterWidgetConfig
    {
        public Vector2 Position;
        public float Scale = 1;
        public AddRGB KunaiColor = new(0);
        public bool Stagger = true;
        public float Spacing = 23;
        public float KunaiAngle;
        public float Angle;
        public float Curve;
        public bool HideEmpty;
        public CounterPulse Pulse = AtMax;

        public KazematoiKunaiConfig(WidgetConfig widgetConfig)
        {
            var config = widgetConfig.KazematoiKunaiCfg;

            if (config == null) return;

            Position = config.Position;
            Scale = config.Scale;
            KunaiColor = config.KunaiColor;
            Stagger = config.Stagger;
            Spacing = config.Spacing;
            KunaiAngle = config.KunaiAngle;
            Angle = config.Angle;
            Curve = config.Curve;
            HideEmpty = config.HideEmpty;

            AsTimer = config.AsTimer;
            TimerSize = config.TimerSize;
            InvertTimer = config.InvertTimer;
        }

        public KazematoiKunaiConfig() { }
    }

    public override CounterWidgetConfig GetConfig => Config;

    public KazematoiKunaiConfig Config;

    public override void InitConfigs() => Config = new(Tracker.WidgetConfig);

    public override void ResetConfigs() => Config = new();

    public override void ApplyConfigs()
    {
        var widgetAngle = Config.Angle + (Config.Curve / 2f);
        WidgetContainer.SetPos(Config.Position + new Vector2(-3, 0))
                  .SetScale(Config.Scale)
                  .SetRotation(widgetAngle, true);

        var posAngle = 0f;
        double x = 0;
        double y = 0;

        for (var i = 0; i < Stacks.Count; i++)
        {
            var (sx, sy) = StaggerOffsets(i);

            var knifeAngle = (Config.Curve * (i - 0.5f)) + Config.KunaiAngle;

            Stacks[i].SetPos((float)x, (float)y)
                     .SetRotation(knifeAngle, true);

            Knives[i].SetPos(sx, sy).SetAddRGB(Config.KunaiColor);
            Slots[i].SetPos(sx, sy);

            x += Cos(posAngle * (PI / 180)) * Config.Spacing;
            y += Sin(posAngle * (PI / 180)) * Config.Spacing;
            posAngle += Config.Curve;
        }
    }

    public override void DrawUI(ref WidgetConfig widgetConfig, ref UpdateFlags update)
    {
        Heading("Layout");

        PositionControls("Position", ref Config.Position, ref update);
        ScaleControls("Scale", ref Config.Scale, ref update);
        ToggleControls("Staggered", ref Config.Stagger, ref update);
        FloatControls("Spacing", ref Config.Spacing, -1000, 1000, 0.5f, ref update);
        AngleControls("Angle (Knife)", ref Config.KunaiAngle, ref update);
        AngleControls("Angle (All)", ref Config.Angle, ref update);
        AngleControls("Curve", ref Config.Curve, ref update);

        Heading("Colors");
        ColorPickerRGB("Kunai Tint", ref Config.KunaiColor, ref update);

        Heading("Behavior");
        if (ToggleControls("Hide Empty", ref Config.HideEmpty, ref update))
        {
            if (Config.HideEmpty && ((!Config.AsTimer && Tracker.CurrentData.Count == 0) || (Config.AsTimer && Tracker.CurrentData.GaugeValue == 0))) AllVanish();
            if (!Config.HideEmpty && WidgetContainer.Alpha < 255) AllAppear();
        }

        RadioControls("Pulse", ref Config.Pulse, new() { Never, AtMax, Always }, new() { "Never", "At Maximum", "Always" }, ref update);

        CounterAsTimerControls(ref Config.AsTimer, ref Config.InvertTimer, ref Config.TimerSize, Tracker.TermGauge, ref update);

        if (update.HasFlag(Save)) ApplyConfigs();
        widgetConfig.KazematoiKunaiCfg = Config;
    }

    #endregion
}

public partial class WidgetConfig
{
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] public KazematoiKunaiConfig? KazematoiKunaiCfg { get; set; }
}
