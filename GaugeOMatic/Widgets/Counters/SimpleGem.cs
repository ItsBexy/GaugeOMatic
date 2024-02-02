using CustomNodes;
using GaugeOMatic.CustomNodes.Animation;
using GaugeOMatic.Trackers;
using GaugeOMatic.Windows;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Numerics;
using static CustomNodes.CustomNodeManager;
using static Dalamud.Interface.FontAwesomeIcon;
using static GaugeOMatic.Utility.Color;
using static GaugeOMatic.Utility.MiscMath;
using static GaugeOMatic.Widgets.SimpleGem;
using static GaugeOMatic.Widgets.SimpleGem.SimpleGemConfig;
using static GaugeOMatic.Widgets.SimpleGem.SimpleGemConfig.GemShapes;
using static GaugeOMatic.Widgets.WidgetTags;
using static GaugeOMatic.Widgets.WidgetUI;
using static System.Math;
#pragma warning disable CS8618

namespace GaugeOMatic.Widgets;

public sealed unsafe class SimpleGem : CounterWidget
{
    public SimpleGem(Tracker tracker) : base(tracker) { }

    public override WidgetInfo WidgetInfo => GetWidgetInfo;

    public static WidgetInfo GetWidgetInfo => new()
    {
        DisplayName = "Simple Gems",
        Author = "ItsBexy",
        Description = "A counter based on Simple Mode job gauges.",
        WidgetTags = Counter | Replica
    };

    public override CustomPartsList[] PartsLists { get; } = {
        new("ui/uld/JobHudSimple_StackA.tex", new(0, 0, 32, 32),  new(32, 0, 32, 32)), // diamond
        new("ui/uld/JobHudSimple_StackA.tex", new(0, 0, 32, 32),  new(32, 32, 32, 32)),// hollow diamond
        new("ui/uld/JobHudSimple_StackB.tex", new(0, 0, 32, 32),  new(32, 0, 32, 32)), // chevron
        new("ui/uld/jobhudsmn1.tex",          new(434, 597, 32, 32), new(434, 629, 32, 32)), // teardrop
        new("ui/uld/jobhudsmn1.tex",          new(498, 598, 32, 32), new(498, 630, 32, 32)), // rectangle
        new("ui/uld/jobhudbrd0.tex",          new(244, 110, 32, 32), new(278, 110, 32, 32)), // music note
        new("ui/uld/jobhudsam1.tex",          new(240, 318, 32, 32), new(240, 350, 32, 32)), // setsu
        new("ui/uld/jobhudsam1.tex",          new(272, 318, 32, 32), new(272, 350, 32, 32)), // getsu
        new("ui/uld/jobhudsam1.tex",          new(304, 318, 32, 32), new(304, 350, 32, 32))  // ka
    };

    #region Nodes

    public List<CustomNode> Stacks = new();
    public List<CustomNode> Frames = new();
    public List<CustomNode> Gems = new();

    public override CustomNode BuildRoot()
    {
        Max = Config.AsTimer ? Config.TimerSize : Tracker.GetCurrentData().MaxCount;

        BuildStacks(Max);

        return new CustomNode(CreateResNode(), Stacks.ToArray()).SetOrigin(16, 16);
    }

    private void BuildStacks(int count)
    {
        Stacks = new();
        Frames = new();
        Gems = new();

        for (var i = 0; i < count; i++)
        {
            Frames.Add(ImageNodeFromPart(0, 0)
                           .SetOrigin(16, 16));

            Gems.Add(ImageNodeFromPart(0, 1)
                     .SetMultiply(80)
                     .SetOrigin(16, 16).SetAlpha(0));

            Stacks.Add(new CustomNode(CreateResNode(), Frames[i], Gems[i]).SetPos(i*20, 0).SetOrigin(16, 16));
        }
    }

    #endregion

    #region Animations

    public override void ShowStack(int i)
    {
        var colorOffset = GetColorOffset();
        Animator += new Tween(Gems[i],
                              new(0) { Scale = 2.4f, Alpha = 0, AddRGB = Config.GemColor + colorOffset + new AddRGB(80) },
                              new(125) { Scale = 1, Alpha = 255, AddRGB = Config.GemColor + colorOffset });
    }

    public override void HideStack(int i)
    {
        var colorOffset = GetColorOffset();
        Animator += new Tween(Gems[i],
                              new(0) { Alpha = 255, AddRGB = Config.GemColor + colorOffset, Scale = 1 },
                              new(90) { Alpha = 0, AddRGB = Config.GemColor + colorOffset + new AddRGB(80), Scale = 0.8f });
    }

    private void AllVanish() =>
        Animator += new Tween(WidgetRoot,
                              new(0) { Alpha = 255, AddRGB = 0 },
                              new(200) { Alpha = 0, AddRGB = 100 });

    private void AllAppear() =>
        Animator += new Tween(WidgetRoot,
                              new(0) { Alpha = 0, AddRGB = 100 },
                              new(200) { Alpha = 255, AddRGB = 0 });

    #endregion

    #region UpdateFuncs

    public override void OnFirstRun(int count, int max)
    {
        for (var i = 0; i < max; i++) Gems[i].SetAlpha(i < count);
        if (count == 0 && Config.HideEmpty) WidgetRoot.SetAlpha(0);
    }

    public override void OnDecreaseToMin() { if (Config.HideEmpty) AllVanish(); }

    public override void OnIncreaseFromMin() { if (Config.HideEmpty || WidgetRoot.Alpha < 255) { AllAppear(); }}

    #endregion

    #region Configs

    public class SimpleGemConfig : CounterWidgetConfig
    {
        public enum GemShapes
        {
            DiamondFull,
            DiamondHollow,
            Chevron,

            Teardrop,
            Rectangle,
            MusicNote,

            Setsu,
            Getsu,
            Ka
        }

        public Vector2 Position;
        public float Scale = 1;
        public AddRGB GemColor = new(120, 30,-40);
        public GemShapes GemShape;
        public float Spacing = 20;
        public float Angle;
        public float GemAngle;
        public float Curve;
        public ColorRGB FrameColor = new(100);
        public bool HideEmpty;
        public bool ChevDir;

        public SimpleGemConfig(WidgetConfig widgetConfig)
        {
            var config = widgetConfig.SimpleGemCfg;

            if (config == null) return;

            Position = config.Position;
            Scale = config.Scale;
            GemColor = config.GemColor;

            GemShape = config.GemShape;
            Spacing = config.Spacing;
            Angle = config.Angle;
            GemAngle = config.GemAngle;
            Curve = config.Curve;
            FrameColor = config.FrameColor;
            HideEmpty = config.HideEmpty;
            ChevDir = config.ChevDir;

            AsTimer = config.AsTimer;
            TimerSize = config.TimerSize;
            InvertTimer = config.InvertTimer;
        }

        public SimpleGemConfig() { }
    }

    public override CounterWidgetConfig GetConfig => Config;

    public SimpleGemConfig Config;

    public override void InitConfigs() => Config = new(Tracker.WidgetConfig);

    public override void ResetConfigs() => Config = new();

    public override void ApplyConfigs()
    {
        var widgetAngle = Config.Angle+(Config.Curve/2f);
        WidgetRoot.SetPos(Config.Position)
                  .SetScale(Config.Scale)
                  .SetRotation(widgetAngle, true);

        var posAngle = 0f;
        double x = 0;
        double y = 0;

        var colorOffset = GetColorOffset();

        for (var i = 0; i < Stacks.Count; i++)
        {
            var (gemAngle, origin, scale) = CalcGemProps(i, widgetAngle);

            Gems[i].SetPartsList(PartsLists[(int)Config.GemShape])
                   .SetAddRGB(Config.GemColor + colorOffset)
                   .SetMultiply(80);

            Frames[i].SetPartsList(PartsLists[(int)Config.GemShape])
                     .SetMultiply(Config.FrameColor);


            Stacks[i].SetPos((float)x, (float)y)
                     .SetScale(scale)
                     .SetOrigin(origin)
                     .SetRotation(gemAngle, true);

            var angleRad = Radians(posAngle);
            x += Cos(angleRad) * Config.Spacing;
            y += Sin(angleRad) * Config.Spacing;
            posAngle += Config.Curve;
        }
    }

    private (float gemAngle, Vector2 origin, Vector2 scale) CalcGemProps(int i, float widgetAngle)
    {
        var gemAngle = (Config.Curve * (i - 0.5f)) + Config.GemAngle;

        if (Config.GemShape is DiamondFull or DiamondHollow)
        {
            while (gemAngle + widgetAngle >= 45) gemAngle -= 90;
            while (gemAngle + widgetAngle < -45) gemAngle += 90;
        }

        Vector2 origin = Config.GemShape is Chevron or Rectangle ? new(16, 15) : new(16, 16);

        var scale = Config.GemShape switch
        {
            Chevron => new(1, Abs(gemAngle + widgetAngle) % 360 <= 90 ? 1 : -1),
            Rectangle => CalcRectFlip((gemAngle + widgetAngle) % 360),
            _ => new(1, 1)
        };

        return (gemAngle, origin, scale);
    }

    private static Vector2 CalcRectFlip(float angle)
    {
        while (angle < -180) angle += 360;
        while (angle > 180) angle -= 360;

        return angle switch
        {
            <= -90 => new(-1, -1),
            <= 0 => new(-1, 1),
            <= 90 => new(1, 1),
            _ => new(1, -1)
        };
    }

    private AddRGB GetColorOffset() =>
        Config.GemShape switch
        {
            Setsu => new(56, -8, -41),
            Getsu => new(14, 7, -82),
            Ka => new(-65, 29, 25),
            Teardrop => new(-53, 34, 15),
            Rectangle => new(78, 24, 86),
            _ => new(0)
        };

    public override void DrawUI(ref WidgetConfig widgetConfig, ref UpdateFlags update)
    {
        Heading("Layout");
        RadioIcons("Shape", ref Config.GemShape, new() { DiamondFull, DiamondHollow, Chevron }, new() { Diamond, Expand, ChevronRight }, ref update);
        RadioIcons(" ##Shape2", ref Config.GemShape, new List<GemShapes> { Teardrop, Rectangle, MusicNote }, new() { MapMarker, Mobile, Music }, ref update);
        RadioIcons(" ##Shape3", ref Config.GemShape, new List<GemShapes> { Setsu, Getsu, Ka }, new() { Snowflake, Moon, Splotch }, ref update);

        PositionControls("Position", ref Config.Position, ref update);
        ScaleControls("Scale", ref Config.Scale, ref update);
        FloatControls("Spacing", ref Config.Spacing, -1000, 1000, 0.5f, ref update);
        AngleControls("Angle (Gem)", ref Config.GemAngle, ref update);
        AngleControls("Angle (Group)", ref Config.Angle, ref update);
        AngleControls("Curve", ref Config.Curve, ref update);

        Heading("Colors");
        ColorPickerRGB("Gem Color", ref Config.GemColor, ref update);
        ColorPickerRGB("Frame Tint", ref Config.FrameColor, ref update);

        Heading("Behavior");
        if (ToggleControls("Hide Empty", ref Config.HideEmpty, ref update))
        {
            if (Config.HideEmpty && ((!Config.AsTimer && Tracker.CurrentData.Count == 0) || (Config.AsTimer && Tracker.CurrentData.GaugeValue == 0))) AllVanish();
            if (!Config.HideEmpty && WidgetRoot.Alpha < 255) AllAppear();
        }

        CounterAsTimerControls(ref Config.AsTimer, ref Config.InvertTimer, ref Config.TimerSize, Tracker.TermGauge, ref update);

        if (update.HasFlag(UpdateFlags.Save)) ApplyConfigs();
        widgetConfig.SimpleGemCfg = Config;
    }

    #endregion
}

public partial class WidgetConfig
{
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] public SimpleGemConfig? SimpleGemCfg { get; set; }
}
