using CustomNodes;
using GaugeOMatic.CustomNodes.Animation;
using GaugeOMatic.Trackers;
using GaugeOMatic.Windows;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Numerics;
using static CustomNodes.CustomNodeManager;
using static GaugeOMatic.CustomNodes.Animation.KeyFrame;
using static GaugeOMatic.CustomNodes.Animation.Tween.EaseType;
using static GaugeOMatic.Utility.Color;
using static GaugeOMatic.Widgets.Common.CommonParts;
using static GaugeOMatic.Widgets.ElementalCrystals;
using static GaugeOMatic.Widgets.ElementalCrystals.ElementalCrystal.BaseColors;
using static GaugeOMatic.Widgets.WidgetTags;
using static GaugeOMatic.Widgets.WidgetUI;
using static GaugeOMatic.Windows.UpdateFlags;
using static System.Math;

#pragma warning disable CS8618

namespace GaugeOMatic.Widgets;

public sealed unsafe class ElementalCrystals : CounterWidget
{
    public ElementalCrystals(Tracker tracker) : base(tracker) { }

    public override WidgetInfo WidgetInfo => GetWidgetInfo;

    public static WidgetInfo GetWidgetInfo => new()
    {
        DisplayName = "Elemental Crystals",
        Author = "ItsBexy",
        Description = "A counter based on BLM's element stack display.",
        WidgetTags = Counter | Replica | MultiComponent,
        MultiCompData = new("EL", "Elemental Gauge Replica", 4)
    };

    public override CustomPartsList[] PartsLists { get; } = { BLM0 };

    #region Nodes

    public List<CustomNode> Stacks = new();
    public List<CustomNode> StackContents = new();
    public List<CustomNode> Crystals = new();
    public List<CustomNode> Glows1 = new();
    public List<CustomNode> Glows2 = new();

    public override CustomNode BuildRoot()
    {
        Max = GetMax();
        BuildStacks(Max);

        return new CustomNode(CreateResNode(), Stacks.ToArray()).SetOrigin(16, 16);
    }

    private void BuildStacks(int count)
    {
        Stacks = new();
        Crystals = new();
        Glows1 = new();
        Glows2 = new();
        StackContents = new();

        for (var i = 0; i < count; i++)
        {
            Crystals.Add(ImageNodeFromPart(0, 4).SetOrigin(10, 24));
            Glows1.Add(ImageNodeFromPart(0, 7).SetOrigin(10, 24).SetAlpha(0));
            Glows2.Add(ImageNodeFromPart(0, 7).SetOrigin(10, 24).SetAlpha(0).SetScale(1.3f, 1.2f).SetAlpha(0).Hide());
            StackContents.Add(new CustomNode(CreateResNode(),
                                             Crystals[i],
                                             Glows1[i],
                                             new (CreateResNode(), Glows2[i])).SetAlpha(0));

            Animator += new Tween(Glows2[i],
                                  new(0) { ScaleX = 1, ScaleY = 1, Alpha = 0 },
                                  new(450) { ScaleX = 1.2f, ScaleY = 1.1f, Alpha = 101 },
                                  new(950) { ScaleX = 1.3f, ScaleY = 1.2f, Alpha = 0 },
                                  new(1600) { ScaleX = 1.3f, ScaleY = 1.2f, Alpha = 0 })
                                  { Repeat = true, Ease = SinInOut, Label = "Pulse" };
            Stacks.Add(new CustomNode(CreateResNode(), StackContents[i]).SetOrigin(10, 24));
        }
    }

    #endregion

    #region Animations

    public override void ShowStack(int i)
    {
        Animator += new Tween(StackContents[i],
                              new(0) { Y = -20, Alpha = 0 },
                              new(225) { Y = 0, Alpha = 200 },
                              new(300) { Y = 0, Alpha = 255 });

        Glows2[i].Show();
    }

    public override void HideStack(int i)
    {
        Glows2[i].Hide();

        Animator += new Tween[]
        {
            new(StackContents[i],
                Visible[0],
                Hidden[325]),
            new(Glows1[i],
                new(0) { Alpha = 0, ScaleX = 1.3f, ScaleY = 1.2f },
                new(50) { Alpha = 73, ScaleX = 1.2f, ScaleY = 1.1f },
                new(200) { Alpha = 0, ScaleX = 1, ScaleY = 1 })
        };
    }

    #endregion

    #region UpdateFuncs

    public override void OnFirstRun(int count, int max)
    {
        for (var i = 0; i < max; i++)
        {
            StackContents[i].SetAlpha(i < count);
            Glows2[i].SetVis(i < count);
        }
    }

    #endregion

    #region Configs

    public class ElementalCrystal : CounterWidgetConfig
    {
        public enum BaseColors { Ice = 3, Fire = 4 }

        public Vector2 Position = new(19, 22);
        public float Scale = 1;
        public BaseColors BaseColor = Ice;
        public AddRGB CrystalColor = new(0);
        public AddRGB GlowColor = new(0);
        public float Spacing = 20;
        public float Angle = -62;
        public float Curve = 18;

        public ElementalCrystal(WidgetConfig widgetConfig)
        {
            var config = widgetConfig.ElementalCrystalCfg;

            if (config == null) return;

            Position = config.Position;
            Scale = config.Scale;
            CrystalColor = config.CrystalColor;

            BaseColor = config.BaseColor;
            GlowColor = config.GlowColor;
            Spacing = config.Spacing;
            Angle = config.Angle;
            Curve = config.Curve;

            AsTimer = config.AsTimer;
            TimerSize = config.TimerSize;
            InvertTimer = config.InvertTimer;
        }

        public ElementalCrystal() { }
    }

    public override CounterWidgetConfig GetConfig => Config;

    public ElementalCrystal Config;

    public override void InitConfigs() => Config = new(Tracker.WidgetConfig);

    public override void ResetConfigs() => Config = new();

    public override void ApplyConfigs()
    {

        var widgetAngle = Config.Angle+(Config.Curve/2f);
        WidgetRoot.SetPos(Config.Position+new Vector2(19, 22))
                  .SetScale(Config.Scale)
                  .SetRotation(widgetAngle, true);

        var posAngle = 0f;
        double x = 0;
        double y = 0;
        for (var i = 0; i < Stacks.Count; i++)
        {
            Crystals[i].SetPartId((ushort)Config.BaseColor);
            Glows1[i].SetPartId((ushort)(Config.BaseColor + 3));
            Glows2[i].SetPartId((ushort)(Config.BaseColor + 3));

            var gemAngle = Config.Curve * (i - 0.5f);

            Stacks[i].SetPos((float)x, (float)y)
                     .SetRotation(gemAngle, true);

            Crystals[i].SetAddRGB(Config.CrystalColor);
            Glows1[i].SetAddRGB(Config.GlowColor);
            Glows2[i].SetAddRGB(Config.GlowColor);
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
        RadioControls("Base Color", ref Config.BaseColor, new() { Ice, Fire }, new() { "Ice", "Fire" }, ref update, true);
        ColorPickerRGB("Color Modifier", ref Config.CrystalColor, ref update);
        ColorPickerRGB("Glow Color", ref Config.GlowColor, ref update);

        Heading("Behavior");

        CounterAsTimerControls(ref Config.AsTimer, ref Config.InvertTimer, ref Config.TimerSize, Tracker.TermGauge, ref update);

        if (update.HasFlag(Save)) ApplyConfigs();
        widgetConfig.ElementalCrystalCfg = Config;
    }

    #endregion
}

public partial class WidgetConfig
{
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] public ElementalCrystal? ElementalCrystalCfg { get; set; }
}
