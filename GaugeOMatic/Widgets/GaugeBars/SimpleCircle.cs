using CustomNodes;
using FFXIVClientStructs.FFXIV.Component.GUI;
using GaugeOMatic.CustomNodes.Animation;
using GaugeOMatic.Trackers;
using GaugeOMatic.Windows;
using Newtonsoft.Json;
using System.Numerics;
using static CustomNodes.CustomNodeManager;
using static Dalamud.Interface.FontAwesomeIcon;
using static GaugeOMatic.CustomNodes.Animation.KeyFrame;
using static GaugeOMatic.Utility.Color;
using static GaugeOMatic.Widgets.GaugeBarWidgetConfig;
using static GaugeOMatic.Widgets.NumTextProps;
using static GaugeOMatic.Widgets.SimpleCircle;
using static GaugeOMatic.Widgets.SimpleCircle.SimpleCircleConfig.CircleStyles;
using static GaugeOMatic.Widgets.WidgetTags;
using static GaugeOMatic.Widgets.WidgetUI;
using static System.Math;

#pragma warning disable CS8618

namespace GaugeOMatic.Widgets;

// OTHER TEXTURES TO TRY:
// ui/uld/cursorlocation.tex (0, 0, 128, 128) -- has gradient fill, will give a pie-ish effect
// ui/uld/deepdungeoninformation.tex (99, 2, 67, 67) -- very glowy, not that big though
// ui/uld/eurekaelementalhud.tex (0, 181, 75, 75) -- hi-res, glowy, slightly thin but still p good
// ui/uld/gatheringcollectable.tex Quarter:(0, 212, 81, 81) Half:(99, 10, 81, 160) - jackpot
// ui/uld/qte_button.tex new(1, 72, 80, 79)

public sealed unsafe class SimpleCircle : GaugeBarWidget
{
    public SimpleCircle(Tracker tracker) : base(tracker) { }

    public override WidgetInfo WidgetInfo => GetWidgetInfo;

    public static WidgetInfo GetWidgetInfo => new()
    {
        DisplayName = "Simple Circle",
        Author = "ItsBexy",
        Description = "It's a circle!",
        WidgetTags = GaugeBar
    };

    public override CustomPartsList[] PartsLists { get; } = {
        new("ui/uld/gatheringcollectable.tex", new Vector4(99, 10, 81, 160) ),
        new("ui/uld/cursorlocation.tex", new Vector4(0, 0, 128, 128))
    };

    #region Nodes

    public CustomNode Circle;
    public CustomNode LeftContainer;
    public CustomNode RightContainer;
    public CustomNode LeftHalf;
    public CustomNode RightHalf;
    public CustomNode Halo;

    public override CustomNode BuildRoot()
    {
        LeftHalf = ImageNodeFromPart(0, 0).SetOrigin(80, 80)
                                          .SetDrawFlags(0xD)
                                          .SetDrawFlags(0x800000)
                                          .SetImageFlag(32);

        LeftContainer = new CustomNode(CreateResNode(), LeftHalf).SetDrawFlags(0x800000)
                                                                 .SetX(-40)
                                                                 .SetSize(81, 160)
                                                                 .SetNodeFlags(NodeFlags.Clip)
                                                                 .SetDrawFlags(0xD);

        RightHalf = ImageNodeFromPart(0, 0).SetX(0).SetOrigin(1, 80).SetDrawFlags(0xD).SetImageFlag(33);
        RightContainer = new CustomNode(CreateResNode(), RightHalf).SetX(39)
                                                                   .SetSize(81, 160)
                                                                   .SetNodeFlags(NodeFlags.Clip)
                                                                   .SetDrawFlags(0xD);
       
        Circle = new CustomNode(CreateResNode(), LeftContainer, RightContainer).SetDrawFlags(0xD);
        Halo = ImageNodeFromPart(1, 0).SetOrigin(64, 64).SetPos(-24, 16).SetImageFlag(32).SetAlpha(0);
        NumTextNode = new();

        return new CustomNode(CreateResNode(), Circle, Halo, NumTextNode).SetDrawFlags(0xD).SetOrigin(40, 80);
    }

    #endregion

    #region Animations

    private void HaloPulse() =>
        Animator += new Tween(Halo,
                                  new(0) { Scale = 0.1f, Alpha = 0 },
                                  new(100) { Scale = 1.2f, Alpha = Config.Color.A * 0.75f },
                                  new(400) { Scale = 1.6f, Alpha = 0 });

    #endregion

    #region UpdateFuncs

    public override void Update()
    {

        var prog = CalcProg();
        var prevProg = CalcProg(true);

        var current = Tracker.CurrentData.GaugeValue;
        var max = Tracker.CurrentData.MaxGauge;

        if (GetConfig.SplitCharges && Tracker.RefType == RefType.Action) AdjustForCharges(ref current, ref max, ref prog, ref prevProg);
        NumTextNode.UpdateValue(current, max);

        if (prog > prevProg)
        {
            if (prog - prevProg >= GainTolerance) OnIncrease(prog, prevProg);
            if (prog > 0 && prevProg <= 0) OnIncreaseFromMin();
        }

        if (prevProg > prog)
        {
            if (prevProg - prog >= DrainTolerance) OnDecrease(prog, prevProg);
            if (prevProg > 0 && prog <= 0) OnDecreaseToMin();
        }

        prog = Clamp(prog, 0, 0.996f);

        if (Config.Direction == Erode)
        {
            LeftHalf.SetRotation(-(1 - prog) * 180 * 0.998f, true).SetAlpha(prog > 0.01f);
            RightHalf.SetRotation((1 - prog) * 180 * 0.998f, true).SetAlpha(prog > 0.01f);
        }
        else if (Config.Direction == CW)
        {
            var lProg = Clamp((prog - 0.5f) / 0.5f, 0, 1f);
            var rProg = Clamp(prog / 0.5f, 0, 1f);
            LeftHalf.SetRotation(-(1 - lProg) * 180 * 0.998f, true).SetAlpha(prog >= 0.5f);
            RightHalf.SetRotation(-(1 - rProg) * 180 * 0.998f, true).SetAlpha(prog > 0.01f);
        }
        else
        {
            var rProg = Clamp((prog - 0.5f) / 0.5f, 0, 1f);
            var lProg = Clamp(prog / 0.5f, 0, 1f);
            LeftHalf.SetRotation((1 - lProg) * 180 * 0.998f, true).SetAlpha(prog > 0.01f);
            RightHalf.SetRotation((1 - rProg) * 180 * 0.998f, true).SetAlpha(prog >= 0.5f);
        }

        Animator.RunTweens();
    }

    public override void OnIncrease(float prog, float prevProg) => HaloPulse();

    public override void OnDecreaseToMin() => HaloPulse();

    public override void OnIncreaseFromMin() => Animator += new Tween(Circle, Hidden[0], new(150) { Alpha = Config.Color.A });

    #endregion

    #region Configs

    public sealed class SimpleCircleConfig : GaugeBarWidgetConfig
    {
        public enum CircleStyles { CW, CCW, Erode }

        public Vector2 Position;
        public float Scale = 1;
        public AddRGB Color = new(200);
        public CircleStyles Direction = CCW;
        protected override NumTextProps NumTextDefault => new() { Position = new(0, 0), FontSize = 50 };

        public SimpleCircleConfig(WidgetConfig widgetConfig) : base(widgetConfig.SimpleCircleCfg)
        {
            var config = widgetConfig.SimpleCircleCfg;

            if (config == null) return;

            Position = config.Position;
            Scale = config.Scale;
            Color = config.Color;
            Direction = config.Direction;
        }

        public SimpleCircleConfig() { }
    }
    public override GaugeBarWidgetConfig GetConfig => Config;

    public SimpleCircleConfig Config;

    public override void InitConfigs()
    {
        Config = new(Tracker.WidgetConfig);
        if (Tracker.WidgetConfig.SimpleCircleCfg == null && Tracker.RefType == RefType.Action) { Config.Invert = true; }
    }

    public override void ResetConfigs() => Config = new();

    public override void ApplyConfigs()
    {
        WidgetRoot.SetPos(Config.Position).SetScale(Config.Scale);
        Circle.SetMultiply(40).SetAddRGB(Config.Color, true);
        Halo.SetAddRGB(Config.Color+new AddRGB(30));

        NumTextNode.ApplyProps(Config.NumTextProps, new(38, 80));
    }

    public override void DrawUI(ref WidgetConfig widgetConfig, ref UpdateFlags update)
    {
        Heading("Layout");
        PositionControls("Position", ref Config.Position, ref update);
        ScaleControls("Scale", ref Config.Scale, ref update);

        Heading("Color");
        ColorPickerRGBA("Color", ref Config.Color, ref update);

        Heading("Behavior");

        SplitChargeControls(ref Config.SplitCharges, Tracker.RefType, Tracker.CurrentData.MaxCount, ref update);
        ToggleControls("Invert Fill", ref Config.Invert, ref update);
        RadioIcons("Direction", ref Config.Direction, new() { CW, CCW, Erode }, new () { Redo , Undo, CircleNotch }, ref update);
       
        NumTextControls($"{Tracker.TermGauge} Text", ref Config.NumTextProps, ref update);

        if (update.HasFlag(UpdateFlags.Save)) ApplyConfigs();
        widgetConfig.SimpleCircleCfg = Config;
    }

    #endregion
}

public partial class WidgetConfig
{
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] public SimpleCircleConfig? SimpleCircleCfg { get; set; }
}
