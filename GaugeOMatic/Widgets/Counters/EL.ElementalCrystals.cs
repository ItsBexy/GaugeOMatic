using GaugeOMatic.Trackers;
using GaugeOMatic.Windows;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Numerics;
using static CustomNodes.CustomNodeManager;
using static CustomNodes.CustomNodeManager.Tween;
using static GaugeOMatic.Utility.Color;
using static GaugeOMatic.Widgets.ElementalCrystals;
using static GaugeOMatic.Widgets.ElementalCrystals.ElementalCrystal.BaseColors;
using static GaugeOMatic.Widgets.WidgetTags;
using static GaugeOMatic.Widgets.WidgetUI;
using static GaugeOMatic.Windows.UpdateFlags;

namespace GaugeOMatic.Widgets;

public sealed unsafe class ElementalCrystals : CounterWidget
{
    public override WidgetInfo WidgetInfo => GetWidgetInfo;

    public static WidgetInfo GetWidgetInfo => new() 
    { 
        DisplayName = "Elemental Crystals",
        Author = "ItsBexy",
        Description = "A counter based on BLM's element stack display.",
        WidgetTags = Counter | Replica | MultiComponent,
        KeyText = "EL4"
    };

    public override CustomPartsList[] PartsLists { get; } = {
        new ("ui/uld/JobHudBLM0.tex",
             new(0,0,162,144),   // 0  moon
             new(290,0,52,52),   // 1  blue orb
             new(290,52,52,52),  // 2  red orb
             new(342,0,20,48),   // 3  blue crystal
             new(342,48,20,48),  // 4  red crystal
             new(182,236,24,68), // 5  icicle
             new(342,96,20,48),  // 6  blue glow
             new(342,144,20,48), // 7  red glow
             new(162,0,128,124), // 8  lattice
             new(0,146,90,90),   // 9  curved plate
             new(95,153,80,78),  // 10 bar fill
             new(180,146,90,90), // 11 bar backdrop
             new(362,0,30,128),  // 12 clock hand
             new(324,192,30,46), // 13 diamond
             new(354,192,30,46), // 14 diamond glow
             new(0,236,90,90),   // 15 flash
             new(90,236,68,68),  // 16 eclipse
             new(290,104,46,46), // 17 halo
             new(362,128,28,28), // 18 glowball
             new(270,150,54,83), // 19 diamond frame
             new(158,236,24,68), // 20 icicle glow
             new(206,236,64,36), // 21 text bg
             new(206,272,32,32), // 22 simple icon thingy
             new(270,233,32,42), // 23 diamond cover
             new(302,238,52,52), // 24 grey orb
             new(307,290,85,36), // 25 double pointy
             new(0,324,86,40),   // 26 null paradox gem
             new(86,324,86,40),  // 27 active paradox gem
             new(172,324,86,40), // 28 paradox glow
             new(90,306,29,18),  // 29 blue sparkles
             new(119,306,29,36)  // 30 red sparkles
        )};

    #region Nodes

    public List<CustomNode> Stacks = new();
    public List<CustomNode> StackContents = new();
    public List<CustomNode> Crystals = new();
    public List<CustomNode> Glows1 = new();
    public List<CustomNode> Glows2 = new();

    public override CustomNode BuildRoot()
    {
        var count = Config.AsTimer ? Config.TimerSize : Tracker.GetCurrentData().MaxCount;

        BuildStacks(count);

        return new CustomNode(CreateResNode(), Stacks.ToArray()).SetOrigin(16,16);
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
            Crystals.Add(ImageNodeFromPart(0,4).SetOrigin(10,24));
            Glows1.Add(ImageNodeFromPart(0,7).SetOrigin(10,24).SetAlpha(0));
            Glows2.Add(ImageNodeFromPart(0, 7).SetOrigin(10, 24).SetAlpha(0).SetScale(1.3f,1.2f).SetAlpha(0).Hide());
            StackContents.Add(new CustomNode(CreateResNode(),
                                             Crystals[i],
                                             Glows1[i], 
                                             new (CreateResNode(),Glows2[i])).SetAlpha(0));

            Tweens.Add(new(Glows2[i],
                           new(0) {ScaleX=1,ScaleY=1,Alpha=0},
                           new(450) {ScaleX=1.2f,ScaleY=1.1f,Alpha=101},
                           new(950) {ScaleX=1.3f,ScaleY=1.2f,Alpha=0},
                           new(1600) { ScaleX = 1.3f, ScaleY = 1.2f, Alpha = 0 })
                           {Repeat=true,Ease=Eases.SinInOut,Label="Pulse"});
            Stacks.Add(new CustomNode(CreateResNode(), StackContents[i]).SetOrigin(10, 24));
        }
    }

    #endregion

    #region Animations

    public override void ShowStack(int i)
    {
        Tweens.Add(new(StackContents[i],
                       new(0){Y=-20,Alpha=0},
                       new(225){Y=0,Alpha=200},
                       new(300){Y=0,Alpha=255}));

        Glows2[i].Show();
    }

    public override void HideStack(int i)
    {
        Tweens.Add(new(StackContents[i],
                       new(0) { Alpha = 255 },
                       new(325) { Alpha = 0 }));

        Glows2[i].Hide();

        Tweens.Add(new(Glows1[i],
                       new(0) { Alpha = 0, ScaleX = 1.3f, ScaleY = 1.2f },
                       new(50) { Alpha = 73, ScaleX = 1.2f, ScaleY = 1.1f },
                       new(200) { Alpha = 0, ScaleX = 1, ScaleY = 1 }));
    }
    

    #endregion

    #region UpdateFuncs

    public override string? SharedEventGroup => null;

    public override void OnFirstRun(int count, int max)
    {
        for (var i = 0; i < count; i++)
        {
            StackContents[i].SetAlpha(255);
            Glows2[i].Show();
        }
        FirstRun = false;
    }

    #endregion

    #region Configs

    public class ElementalCrystal : CounterWidgetConfig
    {
        public enum BaseColors
        {
            Ice=3,Fire=4
        }

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

    public ElementalCrystal Config = null!;

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
            Crystals[i].SetPartId((ushort)Config.BaseColor);
            Glows1[i].SetPartId((ushort)(Config.BaseColor + 3));
            Glows2[i].SetPartId((ushort)(Config.BaseColor + 3));

            var gemAngle = Config.Curve * (i - 0.5f);

            Stacks[i].SetPos((float)x, (float)y)
                     .SetRotation(gemAngle, true);

            Crystals[i].SetAddRGB(Config.CrystalColor);
            Glows1[i].SetAddRGB(Config.GlowColor);
            Glows2[i].SetAddRGB(Config.GlowColor);
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
        RadioControls("Base Color", ref Config.BaseColor, new() { Ice, Fire }, new() { "Ice", "Fire" }, ref update,true);
        ColorPickerRGB("Color Modifier", ref Config.CrystalColor, ref update);
        ColorPickerRGB("Glow Color", ref Config.GlowColor, ref update);

        Heading("Behavior");

        if (ToggleControls($"Use as {Tracker.TermGauge}", ref Config.AsTimer, ref update)) update |= Reset;
        if (Config.AsTimer)
        {
            if (ToggleControls($"Invert {Tracker.TermGauge}", ref Config.InvertTimer, ref update)) update |= Reset;
            if (IntControls($"{Tracker.TermGauge} Size", ref Config.TimerSize, 1, 30, 1, ref update)) update |= Reset;
        }

        if (update.HasFlag(Save)) ApplyConfigs();
        widgetConfig.ElementalCrystalCfg = Config;
    }

    #endregion

    public ElementalCrystals(Tracker tracker) : base(tracker) { }
}

public partial class WidgetConfig
{
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] public ElementalCrystal? ElementalCrystalCfg { get; set; }
}
