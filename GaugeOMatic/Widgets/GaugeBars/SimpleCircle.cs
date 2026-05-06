using CustomNodes;
using GaugeOMatic.CustomNodes.Animation;
using GaugeOMatic.Trackers;
using Newtonsoft.Json;
using System.ComponentModel;
using System.Numerics;
using GaugeOMatic.Widgets.Common;
using static CustomNodes.CustomNode;
using static CustomNodes.CustomNodeManager;
using static Dalamud.Interface.FontAwesomeIcon;
using static GaugeOMatic.CustomNodes.Animation.KeyFrame;
using static GaugeOMatic.Utility.Color;
using static GaugeOMatic.Widgets.Common.CommonParts;
using static GaugeOMatic.Widgets.Common.NumTextProps;
using static GaugeOMatic.Widgets.SimpleCircle;
using static GaugeOMatic.Widgets.SimpleCircle.SimpleCircleConfig.CircleStyles;
using static GaugeOMatic.Widgets.WidgetTags;
using static GaugeOMatic.Widgets.Common.WidgetUI;
using static GaugeOMatic.Widgets.Common.WidgetUI.WidgetUiTab;
using static System.Math;
using static GaugeOMatic.Widgets.AddonRestrictionsAttribute.RestrictionType;

#pragma warning disable CS8618

namespace GaugeOMatic.Widgets;

[WidgetName("Simple Circle")]
[WidgetDescription("It's a circle!")]
[WidgetAuthor("ItsBexy")]
[WidgetTags(GaugeBar | HasAddonRestrictions | HasClippingMask)]
[AddonRestrictions(ClipConflict)]
public sealed unsafe class SimpleCircle(Tracker tracker) : GaugeBarWidget(tracker)
{
    public override CustomPartsList[] PartsLists { get; } =
    [
        new("ui/uld/gatheringcollectable.tex", new Vector4(99, 10, 81, 160) ),
        new("ui/uld/cursorlocation.tex", new Vector4(0, 0, 128, 128)),
        BarMask
    ];

    #region Nodes

    public CustomNode Circle;
    public CustomNode LeftContainer;
    public CustomNode RightContainer;
    public CustomNode LeftHalf;
    public CustomNode RightHalf;
    public CustomNode LeftMask;
    public CustomNode RightMask;
    public CustomNode Halo;

    public override Bounds GetBounds() => new(LeftContainer, RightContainer);

    public override CustomNode BuildContainer()
    {
        LeftHalf = ImageNodeFromPart(0, 0).SetOrigin(80, 80);
        RightHalf = ImageNodeFromPart(0, 0).SetOrigin(1, 80).SetImageFlag(1);

        LeftMask = ClippingMaskFromPart(2, 1).SetScale(-1, 10).SetPos(73, 0).SetOrigin(8, 0);
        RightMask = ClippingMaskFromPart(2, 1).SetScale(1, 10).SetPos(-7, 0).SetOrigin(8, 0);

        LeftContainer = new CustomNode(CreateResNode(), LeftHalf, LeftMask).SetX(-40).SetSize(81, 160);
        RightContainer = new CustomNode(CreateResNode(), RightHalf, RightMask).SetX(40).SetSize(81, 160);

        Circle = new CustomNode(CreateResNode(), LeftContainer, RightContainer).SetOrigin(40, 40);

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

        if (Config.SplitCharges && Tracker.RefType == RefType.Action) AdjustForCharges(ref current, ref max, ref prog, ref prevProg);
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
        HandleMilestone(prog);
    }

    public override void OnIncrease(float prog, float prevProg) => HaloPulse();

    public override void OnDecreaseToMin() => HideBar();
    public override void OnIncreaseToMax() => HideBar();
    public override void OnIncreaseFromMin() => RevealBar();
    public override void OnDecreaseFromMax() => RevealBar();

    public override void HideBar(bool instant = false)
    {
        base.HideBar(instant);
        Animator += new Tween(Circle, new(0) { Alpha = Circle.Alpha }, Hidden[150]) {Label = "ShowHide"};
        HaloPulse();
    }

    public override void RevealBar(bool instant = false)
    {
        base.RevealBar(instant);
        Animator += new Tween(Circle, Hidden[0], new(150) { Alpha = Config.Color.A }){Label = "ShowHide"};
    }

    #endregion

    #region Configs

    public sealed class SimpleCircleConfig : GaugeBarWidgetConfig
    {
        public enum CircleStyles { CW, CCW, Erode }

        public AddRGB Color = new(200);
        [DefaultValue(true)] public bool Dodge = true;
        [DefaultValue(CCW)] public CircleStyles Direction = CCW;
        protected override NumTextProps NumTextDefault => new() { Position = new(0, 0), FontSize = 50 };

        public SimpleCircleConfig(WidgetConfig widgetConfig) : base(widgetConfig.SimpleCircleCfg)
        {
            var config = widgetConfig.SimpleCircleCfg;

            if (config == null) return;

            Color = config.Color;
            Dodge = config.Dodge;
            Direction = config.Direction;
        }

        public SimpleCircleConfig() { }
    }

    private SimpleCircleConfig config;

    public override SimpleCircleConfig Config => config;

    public override void InitConfigs()
    {
        config = new(Tracker.WidgetConfig);
        if (Tracker.WidgetConfig.SimpleCircleCfg == null && ShouldInvertByDefault) { Config.Invert = true; }
    }

    public override void ResetConfigs() => config = new();

    public override void ApplyConfigs()
    {
        base.ApplyConfigs();
        Circle.SetMultiply(40).SetAddRGB(Config.Color, true);
        Halo.SetAddRGB(Config.Color + new AddRGB(30));
        LeftHalf.SetImageFlag((byte)(Config.Dodge ? 0x20 : 0));
        RightHalf.SetImageFlag((byte)(Config.Dodge ? 0x21 : 1));
        
        HandleMilestone(CalcProg(), true);

        NumTextNode.ApplyProps(Config.NumTextProps, new(38, 80));
    }

    public override void DrawUI()
    {
        base.DrawUI();
        switch (UiTab)
        {
            case Colors:
                ColorPickerRGBA("Color", ref Config.Color);
                RadioControls("Blend Mode", ref Config.Dodge, [false, true], ["Normal", "Dodge"]);
                break;
            case Behavior:
                SplitChargeControls(ref Config.SplitCharges, Tracker.RefType, Tracker.CurrentData.MaxCount);
                ToggleControls("Invert Fill", ref Config.Invert);
                RadioIcons("Direction", ref Config.Direction, [CW, CCW, Erode], [Redo, Undo, CircleNotch]);
                break;
            case Text:
                NumTextControls($"{Tracker.TermGauge} Text", ref Config.NumTextProps);
                break;
            default:
                break;
        }
    }

    #endregion
}

public partial class WidgetConfig
{
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] public SimpleCircleConfig? SimpleCircleCfg { get; set; }
}
