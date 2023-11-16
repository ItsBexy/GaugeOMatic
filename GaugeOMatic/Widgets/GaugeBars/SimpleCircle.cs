using FFXIVClientStructs.FFXIV.Component.GUI;
using GaugeOMatic.Trackers;
using GaugeOMatic.Windows;
using Newtonsoft.Json;
using System;
using System.Numerics;
using static CustomNodes.CustomNodeManager;
using static Dalamud.Interface.FontAwesomeIcon;
using static GaugeOMatic.Utility.Color;
using static GaugeOMatic.Widgets.NumTextProps;
using static GaugeOMatic.Widgets.SimpleCircle;
using static GaugeOMatic.Widgets.SimpleCircle.SimpleCircleConfig.CircleStyles;
using static GaugeOMatic.Widgets.WidgetTags;
using static GaugeOMatic.Widgets.WidgetUI;

namespace GaugeOMatic.Widgets;

// OTHER TEXTURES TO TRY:
// ui/uld/cursorlocation.tex (0,0,128,128) -- has gradient fill, will give a pie-ish effect
// ui/uld/deepdungeoninformation.tex (99,2,67,67) -- very glowy, not that big though
// ui/uld/eurekaelementalhud.tex (0,181,75,75) -- hi-res, glowy, slightly thin but still p good
// ui/uld/gatheringcollectable.tex Quarter:(0,212,81,81) Half:(99,10,81,160) - jackpot
// ui/uld/qte_button.tex new(1,72,80,79)

public sealed unsafe class SimpleCircle : GaugeBarWidget
{
    public override WidgetInfo WidgetInfo => GetWidgetInfo;

    public static WidgetInfo GetWidgetInfo => new()
    {
        DisplayName = "Simple Circle",
        Author = "ItsBexy",
        Description = "It's a circle!",
        WidgetTags = GaugeBar
    };

    public override CustomPartsList[] PartsLists { get; } = {
        new("ui/uld/gatheringcollectable.tex",new Vector4(99,10,80,160) ),
        new("ui/uld/cursorlocation.tex",new Vector4(0,0,128,128))
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
        LeftHalf = ImageNodeFromPart(0, 0).SetOrigin(80, 80).SetDrawFlags(0xD).SetDrawFlags(0x800000).SetImageFlag(32);
        LeftContainer = new CustomNode(CreateResNode(), LeftHalf).SetDrawFlags(0x800000).SetX(-40).SetSize(81, 160).SetNodeFlags(NodeFlags.Clip).SetDrawFlags(0xD);

        RightHalf = ImageNodeFromPart(0, 0).SetOrigin(0,80).SetDrawFlags(0xD).SetImageFlag(33);
        RightContainer = new CustomNode(CreateResNode(), RightHalf).SetX(40).SetSize(80, 160).SetNodeFlags(NodeFlags.Clip).SetDrawFlags(0xD);
       
        Circle = new CustomNode(CreateResNode(), LeftContainer, RightContainer).SetDrawFlags(0xD);
        NumTextNode = CreateNumTextNode();

        Halo = ImageNodeFromPart(1, 0).SetOrigin(64, 64).SetPos(-24,16).SetImageFlag(32).SetAlpha(0);


        return new CustomNode(CreateResNode(), Circle, Halo, NumTextNode).SetDrawFlags(0xD).SetOrigin(40,80);
    }

    #endregion

    #region Animations

    #endregion

    #region UpdateFuncs

    public override string? SharedEventGroup => null;

    public override DrainGainType DGType => DrainGainType.Rotation;

    public override void Update()
    {
        UpdateNumText();

        var prog = CalcProg();
        var prevProg = CalcProg(true);

        if (Config.Direction == Erode)
        {
            LeftHalf.SetRotation(-(1 - prog) * Math.PI * 0.998f).SetAlpha(255);
            RightHalf.SetRotation((1 - prog) * Math.PI * 0.998f).SetAlpha(255);
        }
        else if (Config.Direction == CW)
        {
            var lProg = Math.Clamp((prog - 0.5f) / 0.5f, 0, 1);
            var rProg = Math.Clamp(prog / 0.5f, 0, 1);
            LeftHalf.SetRotation(-(1 - lProg) * Math.PI * 0.998f).SetAlpha(prog < 0.5f ? 0 : 255);
            RightHalf.SetRotation(-(1 - rProg) * Math.PI * 0.998f).SetAlpha(255);
        }
        else
        {
            var rProg = Math.Clamp((prog - 0.5f) / 0.5f, 0, 1);
            var lProg = Math.Clamp(prog / 0.5f, 0, 1);
            LeftHalf.SetRotation((1 - lProg) * Math.PI * 0.998f).SetAlpha(255);
            RightHalf.SetRotation((1 - rProg) * Math.PI * 0.998f).SetAlpha(prog < 0.5f ? 0 : 255);
        }

        if (prog > prevProg)
        {
            if (prog - prevProg >= GainThreshold) OnIncrease(prog, prevProg);
            if (prog > 0 && prevProg <= 0) OnIncreaseFromMin(prog, prevProg);
            if (prog >= 1f && prevProg < 1f) OnIncreaseToMax(prog, prevProg);
            if (prog >= MidPoint && prevProg < MidPoint) OnIncreasePastMid(prog, prevProg);
        }

        if (prevProg > prog)
        {
            if (prevProg - prog >= DrainThreshold) OnDecrease(prog, prevProg);
            if (prevProg >= 1f && prog < 1f) OnDecreaseFromMax(prog, prevProg);
            if (prevProg > 0 && prog <= 0) OnDecreaseToMin(prog, prevProg);
            if (prevProg >= MidPoint && prog < MidPoint) OnDecreasePastMid(prog, prevProg);
        }

        RunTweens();
    }

    public override float CalcBarSize(float prog)
    {
        return 0;
    }

     public override void OnIncrease(float prog, float prevProg)
     {
        Tweens.Add(new(Halo,
                    new(0) { Scale = 0.1f, Alpha = 0 },
                    new(100) { Scale = 1.2f, Alpha = Config.Color.A*0.75f },
                    new(400) { Scale = 1.6f, Alpha = 0 }
                ));
     }

    public override void OnIncreaseFromMin(float prog, float prevProg)
    {
        Tweens.Add(new(Circle,new(0){Alpha=0},new(150){Alpha=Config.Color.A}));
    }

    #endregion

    #region Configs

    public sealed class SimpleCircleConfig : GaugeBarWidgetConfig
    {
        public enum CircleStyles { CW,CCW,Erode }

        public Vector2 Position = new(0);
        public float Scale = 1;
        public AddRGB Color = new(0);
        public CircleStyles Direction = CCW;
        protected override NumTextProps NumTextDefault => new() { Position = new(20, 80), FontSize = 50 };

        public SimpleCircleConfig(WidgetConfig widgetConfig)
        {
            NumTextProps = NumTextDefault;
            var config = widgetConfig.SimpleCircleCfg;

            if (config == null) return;

            Position = config.Position;
            Scale = config.Scale;
            Color = config.Color;
            Direction = config.Direction;
            NumTextProps = config.NumTextProps;
        }

        public SimpleCircleConfig()
        {
            NumTextProps = NumTextDefault;
        }
    }
    public override GaugeBarWidgetConfig GetConfig => Config;

    public SimpleCircleConfig Config = null!;

    public override void InitConfigs()
    {
        Config = new(Tracker.WidgetConfig);
        if (Tracker.WidgetConfig.SimpleCircleCfg == null && Tracker.RefType == RefType.Action) { Config.Invert = true; }
    }

    public override void ResetConfigs() => Config = new();

    public override void ApplyConfigs()
    {
        WidgetRoot.SetPos(Config.Position).SetScale(Config.Scale); 
        Circle.SetAddRGB(Config.Color,true);
        Halo.SetAddRGB(Config.Color+new AddRGB(30));

        Config.NumTextProps.ApplyTo(NumTextNode);
    }

    public override void DrawUI(ref WidgetConfig widgetConfig, ref UpdateFlags update)
    {
        Heading("Layout");
        PositionControls("Position", ref Config.Position, ref update);
        ScaleControls("Scale", ref Config.Scale, ref update);

        Heading("Color");
        ColorPickerRGBA("Color", ref Config.Color, ref update);

        Heading("Behavior");
        ToggleControls("Invert Fill", ref Config.Invert, ref update);
        RadioIcons("Direction",ref Config.Direction, new[] { CW, CCW, Erode },new () { Redo ,Undo, CircleNotch },ref update);
       
        NumTextControls($"{Tracker.TermGauge} Text", ref Config.NumTextProps, ref update);

        if (update.HasFlag(UpdateFlags.Save)) ApplyConfigs();
        widgetConfig.SimpleCircleCfg = Config;
    }

    #endregion

    public SimpleCircle(Tracker tracker) : base(tracker) { }
}

public partial class WidgetConfig
{
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] public SimpleCircleConfig? SimpleCircleCfg { get; set; }
}
