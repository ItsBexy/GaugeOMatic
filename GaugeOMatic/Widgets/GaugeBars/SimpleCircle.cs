using CustomNodes;
using GaugeOMatic.CustomNodes.Animation;
using GaugeOMatic.Trackers;
using Newtonsoft.Json;
using System.ComponentModel;
using System.Numerics;
using static CustomNodes.CustomNodeManager;
using static Dalamud.Interface.FontAwesomeIcon;
using static GaugeOMatic.CustomNodes.Animation.KeyFrame;
using static GaugeOMatic.Trackers.Tracker;
using static GaugeOMatic.Utility.Color;
using static GaugeOMatic.Widgets.Common.CommonParts;
using static GaugeOMatic.Widgets.GaugeBarWidgetConfig;
using static GaugeOMatic.Widgets.NumTextProps;
using static GaugeOMatic.Widgets.SimpleCircle;
using static GaugeOMatic.Widgets.SimpleCircle.SimpleCircleConfig.CircleStyles;
using static GaugeOMatic.Widgets.WidgetInfo;
using static GaugeOMatic.Widgets.WidgetTags;
using static GaugeOMatic.Widgets.WidgetUI;
using static System.Math;

#pragma warning disable CS8618

namespace GaugeOMatic.Widgets;

public sealed unsafe class SimpleCircle : GaugeBarWidget
{
    public SimpleCircle(Tracker tracker) : base(tracker) { }

    public override WidgetInfo WidgetInfo => GetWidgetInfo;

    public static WidgetInfo GetWidgetInfo => new()
    {
        DisplayName = "Simple Circle",
        Author = "ItsBexy",
        Description = "It's a circle!",
        WidgetTags = GaugeBar | HasAddonRestrictions,
        RestrictedAddons = ClipConflictAddons
    };

    public override CustomPartsList[] PartsLists { get; } = {
        new("ui/uld/gatheringcollectable.tex", new Vector4(99, 10, 81, 160) ),
        new("ui/uld/cursorlocation.tex", new Vector4(0, 0, 128, 128)),
        BarMask
    };

    #region Nodes

    public CustomNode Circle;
    public CustomNode LeftContainer;
    public CustomNode RightContainer;
    public CustomNode LeftHalf;
    public CustomNode RightHalf;
    public CustomNode LeftMask;
    public CustomNode RightMask;
    public CustomNode Halo;

    public override CustomNode BuildContainer()
    {
        LeftHalf = ImageNodeFromPart(0, 0).SetOrigin(80, 80);
        RightHalf = ImageNodeFromPart(0, 0).SetOrigin(1, 80).SetImageFlag(1);

        LeftMask = ClippingMaskFromPart(2, 1).SetScale(-1, 10).SetPos(73, 0).SetOrigin(8, 0);
        RightMask = ClippingMaskFromPart(2, 1).SetScale(1, 10).SetPos(-7, 0).SetOrigin(8, 0);

        LeftContainer = new CustomNode(CreateResNode(), LeftHalf, LeftMask).SetX(-40).SetSize(81, 160);
        RightContainer = new CustomNode(CreateResNode(), RightHalf, RightMask).SetX(40).SetSize(81, 160);

        Circle = new CustomNode(CreateResNode(), LeftContainer, RightContainer).SetOrigin(40,40);

        Halo = ImageNodeFromPart(1, 0).SetOrigin(64, 64).SetPos(-24, 16).SetImageFlag(32).SetAlpha(0);
        NumTextNode = new();

        return new CustomNode(CreateResNode(), Circle, Halo, NumTextNode).SetOrigin(40, 80);
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

        prog = Clamp(prog, 0, 0.999f);
        if (Config.Direction == Erode)
        {
            LeftHalf.SetRotation(-(1 - prog) * 180f, true).SetAlpha(prog > 0.01f);
            RightHalf.SetRotation((1 - prog) * 180f, true).SetAlpha(prog > 0.01f);
        }
        else
        {
            Circle.SetScaleX(Config.Direction == CW ? 1 : -1);
            var rProg = Clamp((prog - 0.5f) / 0.5f, 0, 1f);
            var lProg = Clamp(prog / 0.5f, 0, 1f);
            LeftHalf.SetRotation((1 - lProg) * 180f, true).SetAlpha(prog > 0.01f);
            RightHalf.SetRotation((1 - rProg) * 180f, true).SetAlpha(prog >= 0.5f);
        }

        Animator.RunTweens();
    }

    public override void OnIncrease(float prog, float prevProg) => HaloPulse();

    public override void OnDecreaseToMin() => HaloPulse();
    public override void OnIncreaseFromMin() => Animator += new Tween(Circle, Hidden[0], new(150) { Alpha = Config.Color.A });
    public override void OnDecreaseFromMax() => Animator += new Tween(Circle, Hidden[0], new(150) { Alpha = Config.Color.A });
    public override void OnIncreaseToMax()
    {
        Animator += new Tween(Circle, new(0) { Alpha = Circle.Alpha }, Hidden[150]);
        HaloPulse();
    }


    #endregion

    #region Configs

    public sealed class SimpleCircleConfig : GaugeBarWidgetConfig
    {
        public enum CircleStyles { CW, CCW, Erode }

        public Vector2 Position;
        [DefaultValue(1f)] public float Scale = 1;
        public AddRGB Color = new(200);
        [DefaultValue(true)] public bool Dodge = true;
        [DefaultValue(CCW)] public CircleStyles Direction = CCW;
        protected override NumTextProps NumTextDefault => new() { Position = new(0, 0), FontSize = 50 };

        public SimpleCircleConfig(WidgetConfig widgetConfig) : base(widgetConfig.SimpleCircleCfg)
        {
            var config = widgetConfig.SimpleCircleCfg;

            if (config == null) return;

            Position = config.Position;
            Scale = config.Scale;
            Color = config.Color;
            Dodge = config.Dodge;
            Direction = config.Direction;
        }

        public SimpleCircleConfig() { }
    }
    public override GaugeBarWidgetConfig GetConfig => Config;

    public SimpleCircleConfig Config;

    public override void InitConfigs()
    {
        Config = new(Tracker.WidgetConfig);
        if (Tracker.WidgetConfig.SimpleCircleCfg == null && ShouldInvertByDefault) { Config.Invert = true; }
    }

    public override void ResetConfigs() => Config = new();

    public override void ApplyConfigs()
    {
        WidgetContainer.SetPos(Config.Position).SetScale(Config.Scale);
        Circle.SetMultiply(40).SetAddRGB(Config.Color, true);
        Halo.SetAddRGB(Config.Color+new AddRGB(30));
        LeftHalf.SetImageFlag((byte)(Config.Dodge ? 0x20 : 0));
        RightHalf.SetImageFlag((byte)(Config.Dodge ? 0x21 : 1));

        NumTextNode.ApplyProps(Config.NumTextProps, new(38, 80));
    }

    public override void DrawUI(ref WidgetConfig widgetConfig, ref UpdateFlags update)
    {
        Heading("Layout");
        PositionControls("Position", ref Config.Position, ref update);
        ScaleControls("Scale", ref Config.Scale, ref update);

        Heading("Color");
        ColorPickerRGBA("Color", ref Config.Color, ref update);
        RadioControls("Blend Mode", ref Config.Dodge, new() { false, true }, new() { "Normal", "Dodge" }, ref update);

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
