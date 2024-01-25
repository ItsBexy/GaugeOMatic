using CustomNodes;
using GaugeOMatic.CustomNodes.Animation;
using GaugeOMatic.Trackers;
using GaugeOMatic.Windows;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Numerics;
using static CustomNodes.CustomNodeManager;
using static Dalamud.Interface.FontAwesomeIcon;
using static FFXIVClientStructs.FFXIV.Component.GUI.AlignmentType;
using static FFXIVClientStructs.FFXIV.Component.GUI.FontType;
using static FFXIVClientStructs.FFXIV.Component.GUI.NodeFlags;
using static GaugeOMatic.Utility.Color;
using static GaugeOMatic.Widgets.Common.CommonParts;
using static GaugeOMatic.Widgets.EnochianBar.EnochianBarConfig.Quadrants;
using static GaugeOMatic.Widgets.GaugeBarWidgetConfig;
using static GaugeOMatic.Widgets.NumTextProps;
using static GaugeOMatic.Widgets.WidgetTags;
using static GaugeOMatic.Widgets.WidgetUI;
using static System.Math;

#pragma warning disable CS8618

namespace GaugeOMatic.Widgets;

public sealed unsafe class EnochianBar : GaugeBarWidget
{
    public EnochianBar(Tracker tracker) : base(tracker) { }

    public override WidgetInfo WidgetInfo => GetWidgetInfo;

    public static WidgetInfo GetWidgetInfo => new()
    {
        DisplayName = "Enochian Bar",
        Author = "ItsBexy",
        Description = "A curved bar based on BLM's Enochian timer.",
        WidgetTags = GaugeBar | MultiComponent | Replica,
        MultiCompData = new("EL", "Elemental Gauge Replica", 1)
    };

    public override CustomPartsList[] PartsLists { get; } = { BLM0Parts };

    #region Nodes

    public CustomNode Contents;
    public CustomNode MainContainer;
    public CustomNode DrainContainer;
    public CustomNode GainContainer;

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

        Backplate = new(CreateResNode(), Lattice, Plate, Groove);

        Drain = ImageNodeFromPart(0, 10).SetRotation(-1.55768f).SetOrigin(-2, -4).DefineTimeline(BarTimeline);
        Gain = ImageNodeFromPart(0, 10).SetRotation(-1.55768f).SetOrigin(-2, -4).DefineTimeline(BarTimeline);
        Main = ImageNodeFromPart(0, 10).SetRotation(-1.55768f).SetOrigin(-2, -4).DefineTimeline(BarTimeline);

        DrainContainer = new CustomNode(CreateResNode(), Drain).SetSize(85, 85).SetNodeFlags(Clip);
        GainContainer = new CustomNode(CreateResNode(), Gain).SetSize(85, 85).SetNodeFlags(Clip);
        MainContainer = new CustomNode(CreateResNode(), Main).SetSize(85, 85).SetNodeFlags(Clip);

        Bar = new CustomNode(CreateResNode(), DrainContainer, GainContainer, MainContainer).SetPos(11, 10).SetSize(86, 86).SetAddRGB(-20).SetMultiply(50);

        ClockHand = ImageNodeFromPart(0, 12).SetOrigin(15, 10).DefineTimeline(new(0){ Rotation = -1.5707963267949f },new(1){ Rotation = 0 });

        ClockHand.Node->Priority = 1;

        ClockHandContainer = new CustomNode(CreateResNode(), ClockHand).SetPos(-6, -2).SetSize(30, 128);

        NumTextNode = new();
        NumTextNode.Node->SetPriority(1);

        Contents = new CustomNode(CreateResNode(), Backplate, Bar, ClockHandContainer).SetSize(128, 124).SetOrigin(8, 8);

        return new(CreateResNode(), NumTextNode,Contents );
    }

    #endregion

    #region Animations

    public static KeyFrame[] BarTimeline => new KeyFrame[] { new(0) { Rotation = -1.5707963267949f }, new(1) { Rotation = 0 } };

    private void ShowBar()
    {
        var scaleX = Config.Direction == 1 ? -1 : 1;
        Animator += new Tween(Contents,
                              new(0) { ScaleX = scaleX * 1.2f, ScaleY = 1.2f, Alpha = 0 },
                              new(150) { ScaleX = scaleX, ScaleY = 1, Alpha = 255 });
    }

    private void HideBar()
    {
        var scaleX = Config.Direction == 1 ? -1 : 1;
        Animator += new Tween(Contents,
                              new(0) { ScaleX = scaleX, ScaleY = 1, Alpha = 255 },
                              new(150) { ScaleX = scaleX * 0.8f, ScaleY = 0.8f, Alpha = 0 });
    }

    private void ActivateNode(CustomNode node) =>
        Animator += new Tween(node,
                              new(0) { AddRGB = -20, MultRGB = new(53) },
                              new(150) { AddRGB = 0, MultRGB = new(100) },
                              new(250) { AddRGB = 70, MultRGB = new(100) },
                              new(360) { AddRGB = 0, MultRGB = new(100) })
                              { Label = "Activate" };

    #endregion

    #region UpdateFuncs

    public override float AdjustProg(float prog)
    {
         if (!Config.Smooth) prog = (float)Floor(prog * Tracker.CurrentData.MaxGauge) / Tracker.CurrentData.MaxGauge;
         return prog;
    }
    public override void PreUpdate(float prog, float prevProg) => Config.AnimationLength = Config.Smooth || (prog > prevProg) != Config.Invert ? 250 : 0;

    public override void PlaceTickMark(float prog)
    {
        ClockHand.SetProgress(Max(Main.Progress,Drain.Progress));
        ClockHand.SetAlpha(!Config.HideHand || prog > 0);
        if (prog > 0 && !Activated) { UnDimBar(); }
    }

    public override void OnIncreaseFromMin()
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

    public override void OnDecreaseToMin()
    {
        Animator -= "Activate";
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
        base.OnFirstRun(prog);

        if (prog > 0) UnDimBar();
        if (Config.HideEmpty && prog == 0) Contents.SetAlpha(0);
    }

    #endregion

    #region Configs

    public sealed class EnochianBarConfig : GaugeBarWidgetConfig
    {
        public enum Quadrants { BR = 0, BL = 1, TL = 2, TR = 3 }

        public Vector2 Position;
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
        protected override NumTextProps NumTextDefault => new(enabled:   false,
                                                              position:  new(0, 0), 
                                                              color:     new(255),
                                                              edgeColor: new(0),
                                                              showBg:    true, 
                                                              bgColor:   new(0),
                                                              font:      MiedingerMed, 
                                                              fontSize:  20, 
                                                              align:     Center,
                                                              invert:    false);

        public EnochianBarConfig(WidgetConfig widgetConfig) : base(widgetConfig.EnochianBarCfg)
        {
            var config = widgetConfig.EnochianBarCfg;

            if (config == null) return;

            Position = config.Position;
            Scale = config.Scale;
            Quadrant = config.Quadrant;
            Direction = config.Direction;

            PlateColor = config.PlateColor;
            MainColor = config.MainColor;
            GainColor = config.GainColor;
            DrainColor = config.DrainColor;

            Smooth = config.Smooth;
            HideHand = config.HideHand;
            DimEmpty = config.DimEmpty;

            LabelText = config.LabelText;
        }

        public EnochianBarConfig() { }
    }

    public override GaugeBarWidgetConfig GetConfig => Config;

    public EnochianBarConfig Config;

    public override void InitConfigs()
    {
        Config = new(Tracker.WidgetConfig);
        if (Tracker.WidgetConfig.EnochianBarCfg == null && Tracker.RefType == RefType.Action) { Config.Invert = true; }
    }

    public override void ResetConfigs() => Config = new();

    public override void ApplyConfigs()
    {
        var colorOffset = new AddRGB(-25, 52, -90);

        Main.SetAddRGB(Config.MainColor + colorOffset, true).SetProgress(CalcProg());
        Gain.SetAddRGB(Config.GainColor + colorOffset, true).SetProgress(0);
        Drain.SetAddRGB(Config.DrainColor + colorOffset, true).SetProgress(0);

        WidgetRoot.SetPos(Config.Position + new Vector2(86, 68))
                  .SetScale(Config.Scale);

        Backplate.SetMultiply(Config.PlateColor);

        QuadrantAdjust(Config.Quadrant, Config.Direction);

    }

    private void QuadrantAdjust(EnochianBarConfig.Quadrants quad, int direction)
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

        NumTextNode.ApplyProps(NumTextProps,new Vector2(8, 5));
        NumTextNode.Show().SetAlpha(NumTextProps.Enabled);
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

        SplitChargeControls(ref Config.SplitCharges, Tracker.RefType, Tracker.CurrentData.MaxCount, ref update);
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
        if (Tracker.CurrentData.GaugeValue == 0 || (Config.Invert && Abs(Tracker.CurrentData.GaugeValue - Tracker.CurrentData.MaxGauge) < 0.01f))
        {
            if (dimEmpty) DimBar();
            else UnDimBar();
        }
    }

    private void HideCheck(bool hideEmpty)
    {
        if (Tracker.CurrentData.GaugeValue == 0 || (Config.Invert && Abs(Tracker.CurrentData.GaugeValue - Tracker.CurrentData.MaxGauge) < 0.01f))
        {
            if (hideEmpty) HideBar();
            else ShowBar();
        }
    }

    #endregion
}

public partial class WidgetConfig
{
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] public EnochianBar.EnochianBarConfig? EnochianBarCfg { get; set; }
}
