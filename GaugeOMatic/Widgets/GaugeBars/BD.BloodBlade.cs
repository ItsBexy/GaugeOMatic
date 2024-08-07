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
using static GaugeOMatic.Widgets.BloodBlade;
using static GaugeOMatic.Widgets.Common.CommonParts;
using static GaugeOMatic.Widgets.GaugeBarWidgetConfig;
using static GaugeOMatic.Widgets.MilestoneType;
using static GaugeOMatic.Widgets.NumTextProps;
using static GaugeOMatic.Widgets.WidgetTags;
using static GaugeOMatic.Widgets.WidgetUI;
using static GaugeOMatic.Widgets.WidgetUI.WidgetUiTab;

#pragma warning disable CS8618

namespace GaugeOMatic.Widgets;

public sealed unsafe class BloodBlade : GaugeBarWidget
{
    public BloodBlade(Tracker tracker) : base(tracker) { }

    public override WidgetInfo WidgetInfo => GetWidgetInfo;

    public static WidgetInfo GetWidgetInfo { get; } = new()
    {
        DisplayName = "Blood Blade",
        Author = "ItsBexy",
        Description = "A bar in the style of DRK's Blood Gauge",
        WidgetTags = GaugeBar | MultiComponent,
        MultiCompData = new("BD", "Blood Gauge Replica", 2)
    };

    public override CustomPartsList[] PartsLists { get; } = { DRK0 };

    #region Nodes

    public CustomNode Sword;
    public CustomNode Backdrop;
    public CustomNode Frame;
    public CustomNode Ring;
    public CustomNode Effects;
    public CustomNode Glow;
    public CustomNode Sigil;

    public override CustomNode BuildContainer()
    {
        Backdrop = ImageNodeFromPart(0, 4).SetPos(0, 36).SetImageWrap(2).SetSize(173, 16);

        Drain = NineGridFromPart(0,3,0,3,0,3).SetPos(0,36)
                                             .SetSize(0,16)
                                             .DefineTimeline(BarTimeline);

        Gain = NineGridFromPart(0, 3, 0, 3, 0, 3).SetPos(0, 36)
                                                 .SetSize(0, 16)
                                                 .DefineTimeline(BarTimeline);

        Main = NineGridFromPart(0, 2, 0, 3, 0, 3).SetPos(0, 36)
                                                 .SetSize(0, 16)
                                                 .DefineTimeline(BarTimeline);

        Frame = ImageNodeFromPart(0, 1).SetPos(-20, 0)
                                       .SetImageWrap(1);

        Ring = ImageNodeFromPart(0, 0).SetPos(-89, 0)
                                      .SetOrigin(48,44)
                                      .SetImageWrap(1);

        Sigil = ImageNodeFromPart(0, 7).SetPos(61, -4)
                                       .SetOrigin(0, 30)
                                       .SetImageWrap(1)
                                       .SetImageFlag(32)
                                       .SetAlpha(0);

        Glow = ImageNodeFromPart(0, 6).SetPos(37, -9)
                                      .SetImageWrap(1);

        Effects = new CustomNode(CreateResNode(), Sigil, Glow).SetPos(-73, 19)
                                                              .SetOrigin(29, 34);

        NumTextNode = new();
        Sword = new CustomNode(CreateResNode(),
                               Backdrop,
                               Drain,
                               Gain,
                               Main,
                               Frame,
                               Ring,
                               Effects).SetPos(87,-5)
                                       .SetOrigin(-44,43);

        return new(CreateResNode(), Sword, NumTextNode);
    }

    #endregion

    #region Animations

    public static KeyFrame[] BarTimeline => new KeyFrame[] { new(0) { Width = 0 }, new(1) { Width = 173 }};

    public override void HideBar(bool instant = false)
    {
        Animator -= "Fade";
        Animator += new Tween(WidgetContainer, new(0, WidgetContainer), Hidden[instant ? 0 : 250]) { Label ="Fade", Ease = SinInOut };
    }

    public override void RevealBar(bool instant = false)
    {
        Animator -= "Fade";
        Animator += new Tween(WidgetContainer, new(0, WidgetContainer), Visible[instant?0:250]) { Label = "Fade", Ease = SinInOut };
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
        Glow.SetAlpha(255);

        var sigilColor = new AddRGB(-50,75,-141) + ((Config.PulseColor3 + Config.PulseColor4)/2);

        Animator += new Tween[] {
            new(Main,
                new (0) { AddRGB = Config.PulseColor + ColorOffset },
                new(800) { AddRGB = Config.PulseColor2 + ColorOffset },
                new (1600) { AddRGB = Config.PulseColor + ColorOffset })
                { Ease = SinInOut, Repeat = true, Label = "BarPulse" },

            new(Glow,
                new(0) { AddRGB = Config.PulseColor3 },
                new(500) { AddRGB = Config.PulseColor4 },
                new(760) { AddRGB = Config.PulseColor3 })
                { Ease = SinInOut, Repeat = true, Label = "BarPulse" },

            new(Sigil,
                new(0) { Scale = 1, Alpha = 0, AddRGB = sigilColor + new AddRGB(47, 145, 242) },
                new(200) { Scale = 1, Alpha = 255, AddRGB = sigilColor + new AddRGB(0, 50, 80) },
                new(325) { ScaleX = 1.1f, Alpha = 0, ScaleY = 1.7f, AddRGB = sigilColor + new AddRGB(-20) })
                { Ease = SinInOut, Label = "BarPulse" }
        };
    }

    protected override void StopMilestoneAnim()
    {
        Animator -= "BarPulse";
        Main.SetAddRGB(Config.MainColor + ColorOffset);
        Glow.SetAlpha(0);
    }

    #endregion

    #region Configs

    public sealed class BloodBladeConfig : GaugeBarWidgetConfig
    {
        public Vector2 Position;
        [DefaultValue(1f)] public float Scale = 1;
        public float Angle;
        [DefaultValue(true)] public bool Ring = true;

        public AddRGB BGColor = new(0, 0, 0, 229);
        public ColorRGB FrameColor = new(100, 100, 100);
        public AddRGB MainColor = new(84, -122, -75);
        public AddRGB GainColor = new(200,-20,-20);
        public AddRGB DrainColor = new(-50,-50,150);

        public AddRGB PulseColor = new (84, -122, -75);
        public AddRGB PulseColor2 = new (84, -122, -75);
        public AddRGB PulseColor3 = new(50,-100,81);
        public AddRGB PulseColor4 = new(50,-50,200);

        public LabelTextProps LabelTextProps = new(string.Empty, false, new(0, 0), new(255), new(0), MiedingerMed, 18, Center);
        protected override NumTextProps NumTextDefault => new(enabled:   true,
                                                              position:  new(0, 0),
                                                              color:     new(255),
                                                              edgeColor: "0x440b00ff",
                                                              showBg:    false,
                                                              bgColor:   new(0),
                                                              font:      Jupiter,
                                                              fontSize:  23,
                                                              align:     Center,
                                                              showZero:  true,
                                                              invert:    false);

        public BloodBladeConfig(WidgetConfig widgetConfig) : base(widgetConfig.BloodBladeCfg)
        {
            var config = widgetConfig.BloodBladeCfg;

            if (config == null) return;

            Position = config.Position;
            Scale = config.Scale;
            Angle = config.Angle;
            Ring = config.Ring;

            BGColor = config.BGColor;
            FrameColor = config.FrameColor;
            MainColor = config.MainColor;
            GainColor = config.GainColor;
            DrainColor = config.DrainColor;

            PulseColor = config.PulseColor;
            PulseColor2 = config.PulseColor2;
            PulseColor3 = config.PulseColor3;
            PulseColor4 = config.PulseColor4;

            LabelTextProps = config.LabelTextProps;
        }

        public BloodBladeConfig() => MilestoneType = Above;
    }

    public override GaugeBarWidgetConfig GetConfig => Config;

    public BloodBladeConfig Config;

    public override void InitConfigs()
    {
        Config = new(Tracker.WidgetConfig);
        if (Tracker.WidgetConfig.BloodBladeCfg == null)
        {
            Config.MilestoneType = Above;
            if (ShouldInvertByDefault) Config.Invert = true;
        }
    }

    public override void ResetConfigs() => Config = new();

    private static AddRGB ColorOffset = new(-84, 122, 75);
    public override void ApplyConfigs()
    {
        WidgetContainer.SetPos(Config.Position)
                  .SetScale(Config.Scale);

        Sword.SetRotation(Config.Angle, true);

        HandleMilestone(CalcProg(), true);

        Drain.SetAddRGB(Config.DrainColor);
        Gain.SetAddRGB(Config.GainColor);
        Main.SetAddRGB(Config.MainColor + ColorOffset);

        Frame.SetMultiply(Config.FrameColor);
        Ring.SetMultiply(Config.FrameColor).SetAlpha(Config.Ring);

        NumTextNode.ApplyProps(Config.NumTextProps,new(42,35));
    }

    public override void DrawUI(ref WidgetConfig widgetConfig, ref UpdateFlags update)
    {
        switch (UiTab)
        {
            case Layout:
                PositionControls("Position", ref Config.Position, ref update);
                ScaleControls("Scale", ref Config.Scale, ref update);
                AngleControls("Angle", ref Config.Angle, ref update);
                ToggleControls("Show Ring", ref Config.Ring, ref update);
                break;
            case Colors:
                ColorPickerRGBA("Backdrop", ref Config.BGColor, ref update);
                ColorPickerRGB("Frame Tint", ref Config.FrameColor, ref update);
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
                    ColorPickerRGB(" ##Pulse4", ref Config.PulseColor4, ref update);
                }
                break;
            case Text:
                NumTextControls($"{Tracker.TermGauge} Text", ref Config.NumTextProps, ref update);
                //   LabelTextControls("Label Text", ref Config.LabelTextProps, Tracker.DisplayName, ref update); //todo: why is this commented out?
                break;
            default:
                break;
        }

        if (update.HasFlag(Save)) ApplyConfigs();
        widgetConfig.BloodBladeCfg = Config;
    }

    #endregion
}

public partial class WidgetConfig
{
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] public BloodBladeConfig? BloodBladeCfg { get; set; }
}
