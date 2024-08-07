using CustomNodes;
using GaugeOMatic.CustomNodes.Animation;
using GaugeOMatic.Trackers;
using Newtonsoft.Json;
using System.ComponentModel;
using System.Numerics;
using static CustomNodes.CustomNodeManager;
using static FFXIVClientStructs.FFXIV.Component.GUI.AlignmentType;
using static FFXIVClientStructs.FFXIV.Component.GUI.FontType;
using static GaugeOMatic.CustomNodes.Animation.KeyFrame;
using static GaugeOMatic.CustomNodes.Animation.Tween.EaseType;
using static GaugeOMatic.Trackers.Tracker;
using static GaugeOMatic.Trackers.Tracker.UpdateFlags;
using static GaugeOMatic.Utility.Color;
using static GaugeOMatic.Widgets.Common.CommonParts;
using static GaugeOMatic.Widgets.GaugeBarWidgetConfig;
using static GaugeOMatic.Widgets.KenkiKatana;
using static GaugeOMatic.Widgets.NumTextProps;
using static GaugeOMatic.Widgets.WidgetTags;
using static GaugeOMatic.Widgets.WidgetUI;
using static GaugeOMatic.Widgets.WidgetUI.WidgetUiTab;

#pragma warning disable CS8618

namespace GaugeOMatic.Widgets;

public sealed unsafe class KenkiKatana : GaugeBarWidget
{
    public KenkiKatana(Tracker tracker) : base(tracker) { }

    public override WidgetInfo WidgetInfo => GetWidgetInfo;

    public static WidgetInfo GetWidgetInfo { get; } = new()
    {
        DisplayName = "Kenki Katana",
        Author = "ItsBexy",
        Description = "A bar in the style of Samurai's Kenki Gauge.",
        WidgetTags = GaugeBar | Replica
    };

    public override CustomPartsList[] PartsLists { get; } = { SAM0 };

    #region Nodes

    public CustomNode Sword;
    public CustomNode Hilt;
    public CustomNode BladeWrapper;
    public CustomNode Blade;
    public CustomNode TickWrapper;
    public CustomNode Tick1;
    public CustomNode Tick2;
    public CustomNode Tassel;
    public CustomNode Glow;
    public CustomNode Effects;

    public override CustomNode BuildContainer()
    {
        Hilt = ImageNodeFromPart(0, 0).SetY(20);

        Blade = ImageNodeFromPart(0, 2).SetPos(93, 24);

        Drain = ImageNodeFromPart(0, 2).SetPos(93, 24).SetWidth(0).SetAddRGB(-100, -100, -40).SetImageWrap(1).DefineTimeline(BarTimeline);
        Gain = ImageNodeFromPart(0, 2).SetPos(93, 24).SetWidth(0).SetAddRGB(255, 100, -10).SetImageWrap(1).DefineTimeline(BarTimeline);
        Main = ImageNodeFromPart(0, 1).SetPos(93, 24).SetWidth(0).SetImageWrap(1).DefineTimeline(BarTimeline);

        Tick1 = ImageNodeFromPart(0, 5).SetPos(-14, 12).SetScale(1.1f).SetOrigin(16,30).SetImageFlag(32);
        Animator += new Tween(Tick1,
                              new(0) { ScaleX = 0.2f, Alpha = 0 },
                              new(130) { ScaleX = 1, Alpha = 128 },
                              new(270) { ScaleX = 1.2f, Alpha = 128 },
                              new(471) { ScaleX = 0.2f, Alpha = 0 })
                              { Repeat = true };

        Tick2 = ImageNodeFromPart(0, 5).SetPos(-14, 10).SetScale(1, 0.9f).SetOrigin(16, 30);

        TickWrapper = new CustomNode(CreateResNode(),Tick1,Tick2).SetPos(91,9).DefineTimeline(TickTimeline);

        BladeWrapper = new CustomNode(CreateResNode(),Blade,Drain,Gain,Main,TickWrapper).SetPos(15, -6);

        Tassel = ImageNodeFromPart(0, 6).SetPos(-7, 28).SetOrigin(15,7);

        Effects = new(
            CreateResNode(),
            NineGridFromPart(0,4,0,50,0,50).SetPos(106,40).SetSize(190,26).SetOrigin(95,13).SetScale(2,0).SetAlpha(0),
            ImageNodeFromPart(0,8).SetPos(172,22).SetScale(3).SetSize(60,60).SetOrigin(30,30).SetAlpha(0).SetImageWrap(1),
            NineGridFromPart(0,4,0,50,0,50).SetPos(111,76).SetRotation(0.34906587f).SetSize(190,26).SetOrigin(190,13).SetScale(1.4f,0),
            NineGridFromPart(0,4,0,50,0,50).SetPos(100,76).SetRotation(-0.34906587f).SetSize(190,26).SetOrigin(0,13).SetScale(1.4f,0)
            );

        Glow = ImageNodeFromPart(0, 3).SetPos(108, 18).SetOrigin(139,25).SetAlpha(0);

        Sword = new CustomNode(CreateResNode(),Hilt,BladeWrapper,Tassel,Effects,Glow).SetScale(0.85f).SetOrigin(103, 41);

        NumTextNode = new();
        return new CustomNode(CreateResNode(), Sword, NumTextNode).SetOrigin(0, 10);
    }


    #endregion

    #region Animations

    public static KeyFrame[] BarTimeline => new KeyFrame[]
    {
        new(0) { Width = 0 },
        new(1) { Width = 276 }
    };
    public static KeyFrame[] TickTimeline => new KeyFrame[]
    {
        new(0) { X = 91, Alpha = 0, ScaleY=1 },
        new(0.05f) { X = 104, Alpha = 255, ScaleY = 1 },
        new(0.95f) { X = 353, Alpha = 255, ScaleY=1 },
        new(1) { X = 367, Alpha = 0,ScaleY=0.6f }
    };

    public override void HideBar(bool instant = false)
    {
        Animator -= "Fade";
        Animator += new Tween(WidgetContainer,
                              new(0, WidgetContainer),
                              Hidden[instant?0:250])
                              { Label ="Fade", Ease = SinInOut };
    }

    public override void RevealBar(bool instant = false)
    {
        Animator -= "Fade";
        Animator += new Tween(WidgetContainer,
                              new(0, WidgetContainer),
                              Visible[instant ? 0 : 250])
                              { Label = "Fade", Ease = SinInOut };
    }

    #endregion

    #region UpdateFuncs

    public override void OnDecreaseToMin() { if (Config.HideEmpty) HideBar(); }
    public override void OnIncreaseFromMin() { if (Config.HideEmpty) RevealBar(); }

    public override void OnIncreaseToMax() { if (Config.HideFull) HideBar(); }
    public override void OnDecreaseFromMax() { if (Config.HideFull) RevealBar(); }

    protected override void StartMilestoneAnim()
    {
        Animator -= "BarPulse";
        Animator += new Tween[]
        {
            new(Effects[0],
                new(0) { ScaleX = 0, ScaleY = 1, Alpha = 0 },
                new(225) { ScaleX = 1.2f, ScaleY = 1, Alpha = 255 },
                new(460) { ScaleX = 2, ScaleY = 0, Alpha = 0 }),

            new(Effects[1],
                new(0) { Scale = 1, Alpha = 0 },
                new(90) { Scale = 2, Alpha = 255 },
                new(260) { Scale = 3, Alpha = 0 }),

            new(Effects[2],
                new(0) { ScaleX = 0, ScaleY = 1 },
                new(160) { ScaleX = 1.2f, ScaleY = 1 },
                new(360) { ScaleX = 1.4f, ScaleY = 0 }),

            new(Effects[3],
                new(0) { ScaleX = 0, ScaleY = 1 },
                new(160) { ScaleX = 1.2f, ScaleY = 1 },
                new(360) { ScaleX = 1.4f, ScaleY = 0 }),

            new(Blade,
                new(0) { AddRGB = 0 },
                new(400) { AddRGB = 0 },
                new(860) { AddRGB = Config.PulseColor3 },
                new(1360) { AddRGB = 0 })
                { Ease = SinInOut, Repeat = true, Label = "BarPulse" },

            new(Glow,
                new(0) { Scale = 1, Alpha = 0 },
                new(460) { Scale = 1, Alpha = 152 },
                new(970) { ScaleX = 1, ScaleY = 1.4f, Alpha = 0 },
                new(1360) { ScaleX = 1, ScaleY = 1.4f, Alpha = 0 })
                { Ease = SinInOut, Repeat = true, Label = "BarPulse" },

            new(Main,
                new(0) { AddRGB = Config.PulseColor2 },
                new(860) { AddRGB = Config.PulseColor },
                new(1360) { AddRGB = Config.PulseColor2 })
                { Ease = SinInOut, Repeat = true, Label = "BarPulse" }
        };

    }

    protected override void StopMilestoneAnim()
    {
        Animator -= "BarPulse";
        Main.SetAddRGB(Config.MainColor);
        Blade.SetAddRGB(0);
        Glow.SetAlpha(0);
    }

    public override void PlaceTickMark(float prog) => TickWrapper.SetProgress(Main);

    #endregion

    #region Configs

    public sealed class KenkiKatanaConfig : GaugeBarWidgetConfig
    {
        public Vector2 Position;
        [DefaultValue(1f)] public float Scale = 1;
        public float Angle;
        public bool Mirror;

        public ColorRGB HiltColor = new(100, 100, 100);
        public AddRGB MainColor = new(160,-24,-27);
        public AddRGB GainColor = new(255, 100, -10);
        public AddRGB DrainColor = new(-100, -100, -40);

        public AddRGB PulseColor = new(160, -24, -27);
        public AddRGB PulseColor2 = new(160, -24, -27);
        public AddRGB PulseColor3 = new(160, 0, 20);

        public LabelTextProps LabelTextProps = new(string.Empty, false, new(0, 0), new(255), new(0), MiedingerMed, 18, Center);
        protected override NumTextProps NumTextDefault => new(enabled:   true,
                                                              position:  new(0, 0),
                                                              color:     new(255),
                                                              edgeColor: "0x9f835bff",
                                                              showBg:    true,
                                                              bgColor:   new(0),
                                                              font:      MiedingerMed,
                                                              fontSize:  15,
                                                              align:     Center,
                                                              showZero:  true,
                                                              invert:    false);

        public KenkiKatanaConfig(WidgetConfig widgetConfig) : base(widgetConfig.KenkiKatanaCfg)
        {
            var config = widgetConfig.KenkiKatanaCfg;

            if (config == null) return;

            Position = config.Position;
            Scale = config.Scale;
            Angle = config.Angle;
            Mirror = config.Mirror;

            HiltColor = config.HiltColor;
            MainColor = config.MainColor;
            GainColor = config.GainColor;
            DrainColor = config.DrainColor;

            PulseColor = config.PulseColor;
            PulseColor2 = config.PulseColor2;
            PulseColor3 = config.PulseColor3;

            LabelTextProps = config.LabelTextProps;
        }

        public KenkiKatanaConfig()
        {
            Milestone = 1;
            MilestoneType = MilestoneType.Above;
        }
    }

    public override GaugeBarWidgetConfig GetConfig => Config;

    public KenkiKatanaConfig Config;

    public override void InitConfigs()
    {
        Config = new(Tracker.WidgetConfig);
        if (Tracker.WidgetConfig.KenkiKatanaCfg == null)
        {
            Config.MilestoneType = MilestoneType.Above;
            Config.Milestone = 1;
            if (ShouldInvertByDefault) Config.Invert = true;
        }
    }

    public override void ResetConfigs() => Config = new();

    public override void ApplyConfigs()
    {
        var flipFactor = Config.Mirror ? -1 : 1;

        WidgetContainer.SetPos(Config.Position + new Vector2(-15.5F, -6))
                  .SetScale(Config.Scale);

        Sword.SetRotation(Config.Angle,true)
             .SetScaleX(0.85f*flipFactor);

        Hilt.SetMultiply(Config.HiltColor);

        Tassel.SetRotation(-Config.Angle * flipFactor, true)
              .SetMultiply(Config.HiltColor);

        Main.SetAddRGB(Config.MainColor + new AddRGB(-160, 24, 27));
        Drain.SetAddRGB(Config.DrainColor);
        Gain.SetAddRGB(Config.GainColor);

        Glow.SetAddRGB(Config.PulseColor3 + new AddRGB(-160, 0, -20));

        HandleMilestone(CalcProg(), true);

        NumTextNode.ApplyProps(Config.NumTextProps,new(75,24));
    }

    public override void DrawUI(ref WidgetConfig widgetConfig, ref UpdateFlags update)
    {
        switch (UiTab)
        {
            case Layout:
                PositionControls("Position", ref Config.Position, ref update);
                ScaleControls("Scale", ref Config.Scale, ref update);
                AngleControls("Angle", ref Config.Angle, ref update);
                ToggleControls("Mirror", ref Config.Mirror, ref update);
                break;
            case Colors:
                ColorPickerRGB("Hilt Tint", ref Config.HiltColor, ref update);
                ColorPickerRGBA("Main Bar", ref Config.MainColor, ref update);
                ColorPickerRGBA("Gain", ref Config.GainColor, ref update);
                ColorPickerRGBA("Drain", ref Config.DrainColor, ref update);
                break;
            case Behavior:
                SplitChargeControls(ref Config.SplitCharges, Tracker.RefType, Tracker.CurrentData.MaxCount, ref update);

                ToggleControls("Invert Fill", ref Config.Invert, ref update);
                HideControls(ref Config.HideEmpty, ref Config.HideFull, EmptyCheck, FullCheck, ref update);

                MilestoneControls("Pulse", ref Config.MilestoneType, ref Config.Milestone, ref update);
                if (Config.MilestoneType > 0)
                {
                    ColorPickerRGB("Pulse Colors", ref Config.PulseColor, ref update);
                    ColorPickerRGB(" ##Pulse2", ref Config.PulseColor2, ref update);
                    ColorPickerRGB(" ##Pulse3", ref Config.PulseColor3, ref update);
                }
                break;
            case Text:
                NumTextControls($"{Tracker.TermGauge} Text", ref Config.NumTextProps, ref update);
                //LabelTextControls("Label Text", ref Config.LabelTextProps, Tracker.DisplayName, ref update); //todo: why is this commented out
                break;
            default:
                break;
        }


        if (update.HasFlag(Save)) ApplyConfigs();
        widgetConfig.KenkiKatanaCfg = Config;
    }

    #endregion
}

public partial class WidgetConfig
{
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] public KenkiKatanaConfig? KenkiKatanaCfg { get; set; }
}
