using GaugeOMatic.Trackers;
using GaugeOMatic.Windows;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Numerics;
using static CustomNodes.CustomNodeManager;
using static CustomNodes.CustomNodeManager.Tween;
using static Dalamud.Interface.FontAwesomeIcon;
using static FFXIVClientStructs.FFXIV.Component.GUI.AlignmentType;
using static FFXIVClientStructs.FFXIV.Component.GUI.FontType;
using static FFXIVClientStructs.FFXIV.Component.GUI.NodeFlags;
using static GaugeOMatic.Utility.Color;
using static GaugeOMatic.Widgets.EnochianBar;
using static GaugeOMatic.Widgets.EnochianBar.EnochianBarConfig;
using static GaugeOMatic.Widgets.EnochianBar.EnochianBarConfig.Quadrants;
using static GaugeOMatic.Widgets.GaugeBarWidget.DrainGainType;
using static GaugeOMatic.Widgets.NumTextProps;
using static GaugeOMatic.Widgets.WidgetTags;
using static GaugeOMatic.Widgets.WidgetUI;

namespace GaugeOMatic.Widgets;

public sealed unsafe class EnochianBar : GaugeBarWidget
{
    public override WidgetInfo WidgetInfo => GetWidgetInfo;

    public static WidgetInfo GetWidgetInfo => new()
    {
        DisplayName = "Enochian Bar",
        Author = "ItsBexy",
        Description = "A curved bar based on BLM's Enochian timer.",
        WidgetTags = GaugeBar | MultiComponent | Replica,
        KeyText = "EL1"
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
            new(95,153,80,78),  // 10  bar fill
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

    public override CustomNode NumTextNode { get; set; }

    public CustomNode Contents { get; set; }
    public CustomNode MainContainer;
    public CustomNode DrainContainer;
    public CustomNode GainContainer;
    public override CustomNode Drain { get; set; }
    public override CustomNode Gain { get; set; }
    public override CustomNode Main { get; set; }

    public CustomNode Backplate;
    public CustomNode Lattice;
    public CustomNode Plate;
    public CustomNode Groove;
    public CustomNode Bar;

    public CustomNode ClockHandContainer;
    public CustomNode ClockHand;

    public override CustomNode BuildRoot()
    {
        Lattice = ImageNodeFromPart(0, 8).SetAddRGB(-20).SetMultiply(50).SetImageWrap(1);
        Plate = ImageNodeFromPart(0, 9).SetPos(9, 8).SetAddRGB(-20).SetMultiply(50).SetOrigin(0, 1);
        Groove = ImageNodeFromPart(0, 11).SetPos(6, 5).SetAddRGB(-20).SetMultiply(50).SetOrigin(0, 1);

        Backplate = new CustomNode(CreateResNode(), Lattice, Plate, Groove);

        Drain = ImageNodeFromPart(0, 10).SetRotation(-1.55768f).SetOrigin(-2, -4);
        Gain = ImageNodeFromPart(0, 10).SetRotation(-1.55768f).SetOrigin(-2, -4);
        Main = ImageNodeFromPart(0, 10).SetRotation(-1.55768f).SetOrigin(-2, -4);

        DrainContainer = new CustomNode(CreateResNode(), Drain).SetSize(85, 85).SetNodeFlags(Clip);
        GainContainer = new CustomNode(CreateResNode(), Gain).SetSize(85, 85).SetNodeFlags(Clip);
        MainContainer = new CustomNode(CreateResNode(), Main).SetSize(85, 85).SetNodeFlags(Clip);

        Bar = new CustomNode(CreateResNode(), DrainContainer, GainContainer, MainContainer)
              .SetPos(11, 10).SetSize(86, 86).SetAddRGB(-20).SetMultiply(50);

        ClockHand = ImageNodeFromPart(0, 12).SetOrigin(15, 10);

        ClockHand.Node->Priority = 1;

        ClockHandContainer = new CustomNode(CreateResNode(), ClockHand).SetPos(-6, -2).SetSize(30, 128);

        NumTextNode = CreateNumTextNode();

        NumTextNode.Node->SetPriority(1);

        Contents = new CustomNode(CreateResNode(),
                              Backplate,
                              Bar,
                              ClockHandContainer
                              ).SetSize(128, 124).SetOrigin(8, 8);

        return new CustomNode(CreateResNode(), Contents,NumTextNode);
    }

    #endregion

    #region Animations

    private void ShowBar()
    {
        var scaleX = Config.Direction == 1 ? -1 : 1;
        Tweens.Add(new(Contents,
                       new(0) { ScaleX = scaleX * 1.2f, ScaleY = 1.2f, Alpha = 0 },
                       new(150) { ScaleX = scaleX, ScaleY = 1, Alpha = 255 }));
    }

    private void HideBar()
    {
        var scaleX = Config.Direction == 1 ? -1 :1;
        Tweens.Add(new(Contents,
                       new(0) { ScaleX = scaleX, ScaleY = 1, Alpha = 255 },
                       new(150) { ScaleX = scaleX * 0.8f, ScaleY = 0.8f, Alpha = 0 }));
    }

    #endregion

    #region UpdateFuncs

    public override string? SharedEventGroup => null;

    public override DrainGainType DGType => Rotation;
    public override float CalcBarProperty(float prog)
    {
        if (!Config.Smooth) prog = (float)Math.Floor(prog * Tracker.CurrentData.MaxGauge) / Tracker.CurrentData.MaxGauge;
        return (1.5707963267949f * prog) - 1.5707963267949f;
    }

    public override void PreUpdate(float prog, float prevProg) => Config.AnimationLength = Config.Smooth || prevProg > prog ? 250 : prog > prevProg ? 0 : Config.AnimationLength;

    public override void PlaceTickMark(float prog)
    {
        ClockHand.SetRotation(Math.Max(Main.Rotation, Drain.Rotation));
        ClockHand.SetAlpha(Config.HideHand && ClockHand.Rotation <= CalcBarProperty(0) ? 0 : 255);
        if (prog > 0 && !Activated) { UnDimBar(); }
    }

    public override void OnIncreaseFromMin(float prog, float prevProg)
    {
        if (Config.DimEmpty) UnDimBar();
        if (Config.HideEmpty) ShowBar();
    }

    public bool Activated;
    private void UnDimBar()
    {
        ActivateNode(Lattice);
        ActivateNode(Plate);
        ActivateNode(Groove);
        ActivateNode(Bar);
        Activated = true;
    }

    private void ActivateNode(CustomNode node) =>
        Tweens.Add(new(node,
                       new(0) { AddRGB = -20, MultRGB = new(53) },
                       new(150) { AddRGB = 0, MultRGB = new(100) },
                       new(250) { AddRGB = 70, MultRGB = new(100) },
                       new(360) { AddRGB = 0, MultRGB = new(100) })
        { Label = "Activate" });

    public override void OnIncreaseMilestone(float prog, float prevProg) { }

    public override void OnIncreaseToMax(float prog, float prevProg) { }

    public override void OnDecreaseFromMax(float prog, float prevProg) { }

    public override void OnDecreaseMilestone(float prog, float prevProg) { }

    public override void OnDecreaseToMin(float prog, float prevProg)
    {
        ClearLabelTweens(ref Tweens, "Activate");
        if (Config.DimEmpty) DimBar();
        if (Config.HideEmpty) HideBar();
    }

    private void DimBar()
    {
        Lattice.SetAddRGB(-20).SetMultiply(50);
        Plate.SetAddRGB(-20).SetMultiply(50);
        Groove.SetAddRGB(-20).SetMultiply(50);
        Bar.SetAddRGB(-20).SetMultiply(50);
        Activated = false;
    }

    public override void OnFirstRun(float prog)
    {
        Main.SetRotation(CalcBarProperty(prog));
        Gain.SetRotation(-91, true);
        Drain.SetRotation(-91, true);
        if (prog > 0) UnDimBar();
        if (Config.HideEmpty && prog == 0) Contents.SetAlpha(0);
    }

    #endregion

    #region Configs

    public sealed class EnochianBarConfig : GaugeBarWidgetConfig
    {
        public enum Quadrants
        {
            BR = 0,
            BL = 1,
            TL = 2,
            TR = 3
        }

        public Vector2 Position = new(0, 0);
        public float Scale = 1;
        public Quadrants Quadrant;
        public int Direction;

        public ColorRGB PlateColor = new(100, 100, 100);
        public AddRGB MainColor = new(25, -52, 90);
        public AddRGB GainColor = "0xBE9180FF";
        public AddRGB DrainColor = "0x661D5EFF";

        public bool Smooth;
        public bool HideHand = true;
        public bool DimEmpty = true;

        public LabelTextProps LabelText = new(string.Empty, false, new(0, 32), new(255), new(0), Jupiter, 16, Left);
        protected override NumTextProps NumTextDefault => new(false, new(0, 0), new(255), new(0), MiedingerMed, 20, Center, false);

        public EnochianBarConfig(WidgetConfig widgetConfig)
        {
            NumTextProps = NumTextDefault;
            var config = widgetConfig.EnochianBarCfg;

            if (config != null)
            {
                Position = config.Position;
                Scale = config.Scale;
                Quadrant = config.Quadrant;
                Direction = config.Direction;

                PlateColor = config.PlateColor;
                MainColor = config.MainColor;
                GainColor = config.GainColor;
                DrainColor = config.DrainColor;

                Smooth = config.Smooth;
                HideEmpty = config.HideEmpty;
                HideHand = config.HideHand;
                DimEmpty = config.DimEmpty;

                AnimationLength = config.Smooth ? 250 : 0;
                Invert = config.Invert;
                NumTextProps = config.NumTextProps;
                LabelText = config.LabelText;
            }
        }

        public EnochianBarConfig() => NumTextProps = NumTextDefault;
    }

    public override GaugeBarWidgetConfig GetConfig => Config;

    public EnochianBarConfig Config = null!;

    public override void InitConfigs()
    {
        Config = new(Tracker.WidgetConfig);
        if (Tracker.WidgetConfig.EnochianBarCfg == null && Tracker.RefType == RefType.Action) { Config.Invert = true; }
    }

    public override void ResetConfigs() => Config = new();

    public override void ApplyConfigs()
    {
        var colorOffset = new AddRGB(-25, 52, -90);
        WidgetRoot.SetPos(Config.Position + new Vector2(86, 68))
                  .SetScale(Config.Scale);

        Backplate.SetMultiply(Config.PlateColor);

        QuadrantAdjust(Config.Quadrant, Config.Direction);

        Main.SetAddRGB(Config.MainColor + colorOffset, true);
        Gain.SetAddRGB(Config.GainColor + colorOffset, true);
        Drain.SetAddRGB(Config.DrainColor + colorOffset, true);
    }

    private void QuadrantAdjust(Quadrants quad, int direction)
    {
        var flip = direction == 1;
        var flipFactor = flip ? -1 : 1;
        var angleOffset = flip ? -90 : 0;

        ClockHand.SetScaleX(flipFactor);
        Lattice.SetScaleX(flipFactor).SetRotation(angleOffset, true);
        Contents.SetRotation(((float)quad * 90f) + angleOffset, true).SetScaleX(flipFactor);

        Vector2 barPos = quad switch
        {
            BL when direction == 0 => new(0, -85),
            TR when direction == 1 => new(0, -85),
            TL => new(-85, -85),
            TR when direction == 0 => new(-85, -1),
            BL when direction == 1 => new(-85, -1),
            _ => new(0, -1)
        };

        Vector2 containerPos = quad switch
        {
            BL when direction == 0 => new(0, 86),
            TR when direction == 1 => new(0, 86),
            TL => new(85, 86),
            TR when direction == 0 => new(85, 2),
            BL when direction == 1 => new(85, 2),
            _ => new(0, 2)
        };

        Drain.SetPos(barPos);
        Gain.SetPos(barPos);
        Main.SetPos(barPos);

        DrainContainer.SetPos(containerPos);
        GainContainer.SetPos(containerPos);
        MainContainer.SetPos(containerPos);

        NumTextProps.ApplyTo(NumTextNode,new (-8,5));
    }

    public override void DrawUI(ref WidgetConfig widgetConfig, ref UpdateFlags update)
    {
        Heading("Layout");
        PositionControls("Position", ref Config.Position, ref update);
        ScaleControls("Scale", ref Config.Scale, ref update);
        RadioIcons("Quadrant##Quad1", ref Config.Quadrant, new() { TL, TR }, new() { None, None }, ref update);
        RadioIcons(" ##Quad2", ref Config.Quadrant, new() { BL, BR }, new() { None, None }, ref update);
        RadioIcons("Direction", ref Config.Direction, new() { 0, 1 }, new() { RedoAlt, UndoAlt }, ref update);

        Heading("Colors");
        ColorPickerRGB("Plate Tint", ref Config.PlateColor, ref update);
        ColorPickerRGBA("Main Bar", ref Config.MainColor, ref update);
        ColorPickerRGBA("Gain", ref Config.GainColor, ref update);
        ColorPickerRGBA("Drain", ref Config.DrainColor, ref update);

        Heading("Behavior");
        ToggleControls("Turn Smoothly", ref Config.Smooth, ref update);

        ToggleControls("Invert Fill", ref Config.Invert, ref update);

        var emptyBehavior = new List<bool> { Config.DimEmpty, Config.HideEmpty, Config.HideHand };
        if (ToggleControls("When Empty", ref emptyBehavior, new() { "Dim Bar", "Hide Bar", "Hide Clock Hand" }, ref update))
        {
            if (Config.HideEmpty != emptyBehavior[1]) HideCheck(emptyBehavior[1]);
            if (Config.DimEmpty != emptyBehavior[0]) DimCheck(emptyBehavior[0]);
            Config.DimEmpty = emptyBehavior[0];
            Config.HideEmpty = emptyBehavior[1];
            Config.HideHand = emptyBehavior[2];
        }

        NumTextControls($"{Tracker.TermGauge} Text", ref Config.NumTextProps, ref update);

        if (update.HasFlag(UpdateFlags.Save)) ApplyConfigs();
        widgetConfig.EnochianBarCfg = Config;
    }

    private void DimCheck(bool dimEmpty)
    {
        if (Tracker.CurrentData.GaugeValue == 0 || (Config.Invert && Math.Abs(Tracker.CurrentData.GaugeValue - Tracker.CurrentData.MaxGauge) < 0.01f))
        {
            if (dimEmpty) DimBar();
            else UnDimBar();
        }
    }

    private void HideCheck(bool hideEmpty)
    {
        if (Tracker.CurrentData.GaugeValue == 0 || (Config.Invert && Math.Abs(Tracker.CurrentData.GaugeValue - Tracker.CurrentData.MaxGauge) < 0.01f))
        {
            if (hideEmpty) HideBar();
            else ShowBar();
        }
    }

    #endregion

    public EnochianBar(Tracker tracker) : base(tracker) { }
}

public partial class WidgetConfig
{
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] public EnochianBarConfig? EnochianBarCfg { get; set; }
}
