using CustomNodes;
using GaugeOMatic.CustomNodes.Animation;
using GaugeOMatic.Trackers;
using GaugeOMatic.Windows;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Numerics;
using static CustomNodes.CustomNode.CustomNodeFlags;
using static CustomNodes.CustomNodeManager;
using static GaugeOMatic.CustomNodes.Animation.Tween.EaseType;
using static GaugeOMatic.Utility.Color;
using static GaugeOMatic.Widgets.Common.CommonParts;
using static GaugeOMatic.Widgets.CounterWidgetConfig.CounterPulse;
using static GaugeOMatic.Widgets.PolyglotGem;
using static GaugeOMatic.Widgets.WidgetTags;
using static GaugeOMatic.Widgets.WidgetUI;
using static GaugeOMatic.Windows.UpdateFlags;
using static System.Math;

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

    public override CustomPartsList[] PartsLists { get; } = { BLM0 };

    #region Nodes

    public List<CustomNode> Stacks = new();
    public List<CustomNode> Frames = new();
    public List<CustomNode> GemContainers = new();
    public List<CustomNode> Gems = new();
    public List<CustomNode> Glows1 = new();
    public List<CustomNode> Glows2 = new();

    public override CustomNode BuildRoot()
    {
        Max = GetMax();
        BuildStacks(Max);

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

        CustomNode BuildGlowNode() => ImageNodeFromPart(0, 14).SetPos(12, 20)
                                                              .SetScale(2.2f)
                                                              .SetOrigin(15, 23)
                                                              .SetImageFlag(32)
                                                              .RemoveFlags(SetVisByAlpha)
                                                              .SetAlpha(0);

        for (var i = 0; i < count; i++)
        {
            Frames.Add(ImageNodeFromPart(0, 19).SetOrigin(27.5f, 41.5f));
            Gems.Add(ImageNodeFromPart(0, 13).SetPos(12, 18).SetOrigin(15, 23).SetAlpha(0));
            Glows1.Add(BuildGlowNode());
            Glows2.Add(BuildGlowNode());

            Glows1[i].RemoveFlags(SetVisByAlpha);
            Glows2[i].RemoveFlags(SetVisByAlpha);

            GemContainers.Add(new(CreateResNode(), Gems[i], Glows1[i]));

            Stacks.Add(new CustomNode(CreateResNode(),
                                      Frames[i],
                                      GemContainers[i],
                                      Glows2[i]).SetSize(54, 83).SetOrigin(27.5f, 41.5f));
        }
    }

    #endregion

    #region Animations

    public override void ShowStack(int i)
    {
        var (flipX, flipY) = FlipFactor(i);

        var colorOffset = Config.GemColor + new AddRGB(-27, 78, -50);

        Animator += new Tween[] {
            new(Gems[i],
                new (0) { ScaleX = 2.5f * flipX, ScaleY = flipY * 2.5f, Alpha = 0, AddRGB = colorOffset + new AddRGB(0) },
                new(150) { ScaleX = flipX, ScaleY = flipY, Alpha = 255, AddRGB = colorOffset + new AddRGB(0) },
                new(260) { ScaleX = flipX, ScaleY = flipY, Alpha = 255, AddRGB = colorOffset + new AddRGB(145) },
                new(360) { ScaleX = flipX, ScaleY = flipY, Alpha = 255, AddRGB = colorOffset + new AddRGB(0) }),

            new(Glows1[i],
                new (0) { Scale = 1.8f, Alpha = 0 },
                new (150) { Scale = 1.8f, Alpha = 200 },
                new (260) { Scale = 2.5f, Alpha = 0 })
        };

        Glows2[i].Show();
    }

    public override void HideStack(int i)
    {
        var (flipX, flipY) = FlipFactor(i);

        Animator += new Tween[]
        {
            new(Gems[i],
                new(0) { ScaleX = flipX, ScaleY = flipY, Alpha = 255 },
                new(70) { ScaleX = 1.6f* flipX, ScaleY = 1.6f * flipY, Alpha = 50 },
                new(170) { ScaleX = 2 * flipX, ScaleY = 2f * flipY, Alpha = 0 }),
            new(Glows1[i],
                new(0) { Scale = 0f, Alpha = 0 },
                new(150) { Scale = 1.8f, Alpha = 200 },
                new(260) { Scale = 2.2f, Alpha = 0 })
        };

        Glows2[i].Hide();
    }

    private (float flipX, float flipY) FlipFactor(int i) => (Gems[i].ScaleX < 0 ? -1f : 1f, Gems[i].ScaleY < 0 ? -1f : 1f);

    private void AllVanish() =>
        Animator += new Tween(WidgetRoot,
                              new(0) { Alpha = 255, AddRGB = 0 },
                              new(200) { Alpha = 0, AddRGB = 100 });

    private void AllAppear() =>
        Animator += new Tween(WidgetRoot,
                              new(0) { Alpha = 0, AddRGB = 100 },
                              new(200) { Alpha = 255, AddRGB = 0 });

    private void PulseAll()
    {
        Animator -= "Pulse";
        for (var i = 0; i < Stacks.Count; i++)
        {
            Animator += new Tween[]
            {
                new(GemContainers[i],
                    new(0) { AddRGB = 0 },
                    new(450) { AddRGB = 148 },
                    new(900) { AddRGB = 1 },
                    new(1300) { AddRGB = 0 })
                    { Repeat = true, Ease = SinInOut, Label = "Pulse" },
                new(Glows2[i],
                    new(0) { Scale = 0, Alpha = 0 },
                    new(400) { Scale = 1.8f, Alpha = 152 },
                    new(830) { Scale = 2.2f, Alpha = 0 },
                    new(1300) { Scale = 2.2f, Alpha = 0 })
                    { Repeat = true, Ease = SinInOut, Label = "Pulse" }
            };
        }
    }

    private void StopPulseAll()
    {
        Animator -= "Pulse";
        for (var i = 0; i < Stacks.Count; i++)
        {
            Animator += new Tween[]
            {
                new(GemContainers[i],
                    new(0, GemContainers[i]),
                    new(200) { AddRGB = 0 })
                    { Label = "Pulse" },
                new(Glows2[i],
                    new(0, Glows2[i]),
                    new(200) { Scale = 2.2f, Alpha = 0 })
                    { Label = "Pulse" }

            };
        }
    }

    #endregion

    #region UpdateFuncs

    public override void OnFirstRun(int count, int max)
    {
        for (var i = 0; i < max; i++) Gems[i].SetAlpha(i < count);
        if (Config.HideEmpty && count == 0) WidgetRoot.Hide();
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
        public Vector2 Position;
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
                  .SetRotation(widgetAngle, true);

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

            Frames[i].SetScaleY(Abs(combinedAngle) > 90?-1:1);

            float scaleX = combinedAngle is <= -53 or >= 128 ? -1 : 1;
            float scaleY = combinedAngle is <= -128 or >= 53 ? -1 : 1;

            Gems[i].SetScale(scaleX, scaleY).SetAddRGB(gemColorOffset + Config.GemColor);
            Glows1[i].SetAddRGB(glowColorOffset + Config.GlowColor);
            Glows2[i].SetAddRGB(glowColorOffset + Config.GlowColor);

            Stacks[i].SetPos((float)x, (float)y)
                     .SetRotation(gemAngle, true);

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
        FloatControls("Spacing", ref Config.Spacing, -1000, 1000, 0.5f, ref update);
        AngleControls("Angle", ref Config.Angle, ref update);
        AngleControls("Curve", ref Config.Curve, ref update);

        Heading("Colors");
        ColorPickerRGB("Gem Color", ref Config.GemColor, ref update);
        ColorPickerRGB("Glow Color", ref Config.GlowColor, ref update);
        ColorPickerRGB("Frame Tint", ref Config.FrameColor, ref update);

        Heading("Behavior");
        if (ToggleControls("Hide Empty", ref Config.HideEmpty, ref update))
        {
            if (Config.HideEmpty && ((!Config.AsTimer && Tracker.CurrentData.Count == 0) || (Config.AsTimer && Tracker.CurrentData.GaugeValue == 0))) AllVanish();
            if (!Config.HideEmpty && WidgetRoot.Alpha < 255) AllAppear();
        }

        RadioControls("Pulse", ref Config.Pulse, new() { Never, AtMax, Always }, new() { "Never", "At Maximum", "Always" }, ref update);

        CounterAsTimerControls(ref Config.AsTimer, ref Config.InvertTimer, ref Config.TimerSize, Tracker.TermGauge, ref update);

        if (update.HasFlag(Save)) ApplyConfigs();
        widgetConfig.PolyglotGemCfg = Config;
    }

    #endregion
}

public partial class WidgetConfig
{
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] public PolyglotGemConfig? PolyglotGemCfg { get; set; }
}
