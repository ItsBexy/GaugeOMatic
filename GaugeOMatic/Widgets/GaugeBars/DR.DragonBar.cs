using CustomNodes;
using GaugeOMatic.CustomNodes.Animation;
using GaugeOMatic.Trackers;
using Newtonsoft.Json;
using System.ComponentModel;
using static CustomNodes.CustomNode;
using static CustomNodes.CustomNode.CustomNodeFlags;
using static CustomNodes.CustomNodeManager;
using static FFXIVClientStructs.FFXIV.Component.GUI.AlignmentType;
using static FFXIVClientStructs.FFXIV.Component.GUI.FontType;
using static GaugeOMatic.CustomNodes.Animation.KeyFrame;
using static GaugeOMatic.CustomNodes.Animation.Tween.EaseType;
using static GaugeOMatic.Utility.Color;
using static GaugeOMatic.Widgets.Common.CommonParts;
using static GaugeOMatic.Widgets.DragonSpear;
using static GaugeOMatic.Widgets.GaugeBarWidgetConfig;
using static GaugeOMatic.Widgets.NumTextProps;
using static GaugeOMatic.Widgets.WidgetTags;
using static GaugeOMatic.Widgets.WidgetUI;
using static GaugeOMatic.Widgets.WidgetUI.WidgetUiTab;

#pragma warning disable CS8618

namespace GaugeOMatic.Widgets;

[WidgetName("Dragon Spear")]
[WidgetDescription("A replica of DRG's Life of the Dragon bar.")]
[WidgetAuthor("ItsBexy")]
[WidgetTags(GaugeBar | Replica | MultiComponent)]
[MultiCompData("DR", "Replica Dragon Gauge", 1)]
public sealed unsafe class DragonSpear : GaugeBarWidget
{
    public DragonSpear(Tracker tracker) : base(tracker) { }

    public override CustomPartsList[] PartsLists { get; } = { DRG0 };

    #region Nodes

    public CustomNode FrameTop;
    public CustomNode FrameBottom;
    public CustomNode Effects;
    public CustomNode Bar;
    public CustomNode Backdrop;
    public CustomNode DragonLineArt;
    public CustomNode DragonBg;
    public CustomNode DragonBgPulse;
    public CustomNode DragonBg1Wrap;

    public override Bounds GetBounds() => DragonBg;

    public override CustomNode BuildContainer()
    {
        NumTextNode = new();

        DragonBgPulse = ImageNodeFromPart(0, 1).SetImageWrap(1).SetAddRGB(100, -20, -20).SetAlpha(0);
        DragonBg1Wrap = new CustomNode(CreateResNode(),DragonBgPulse).SetPos(0, 12);

        DragonBg = ImageNodeFromPart(0, 1).SetPos(0, 12).SetImageWrap(1).RemoveFlags(SetVisByAlpha);
        DragonLineArt = ImageNodeFromPart(0, 0).SetPos(0, 8).SetImageWrap(1).RemoveFlags(SetVisByAlpha);

        Backdrop = ImageNodeFromPart(0, 20).SetPos(0, 4).SetSize(152, 18).SetImageWrap(1);
        Drain = NineGridFromPart(0, 21,0,2,0,0).SetPos(0, 4).SetAddRGB(-70, -80, 150).DefineTimeline(BarTimeline);
        Gain = NineGridFromPart(0, 21, 0, 2, 0, 0).SetPos(0, 4).SetAddRGB(-70, -80, 150).DefineTimeline(BarTimeline);
        Main = NineGridFromPart(0, 19, 0, 2, 0, 0).SetPos(0, 4).SetAddRGB(-100, 0, 200).DefineTimeline(BarTimeline);

        Bar = new CustomNode(CreateResNode(),Backdrop,Drain,Gain,Main).SetPos(85,70).SetAlpha(0);

        FrameTop = ImageNodeFromPart(0, 13).SetPos(44, 64).SetOrigin(0, 20).SetImageWrap(1);
        FrameBottom = ImageNodeFromPart(0,12).SetPos(30,64).SetOrigin(0,20).SetImageWrap(1);

        Effects = BuildEffects();

        return new CustomNode(CreateResNode(),
                              DragonBg1Wrap,
                              DragonBg,
                              DragonLineArt,
                              Bar,
                              FrameTop,
                              FrameBottom,
                              Effects,
                              NumTextNode).SetOrigin(186,70);
    }

    private CustomNode BuildEffects()
    {
        var smear = ImageNodeFromPart(0, 6).SetPos(68, 10).SetScale(1, 5).SetRotation(1.5707964f).SetSize(40, 84)
                                           .SetOrigin(20, 72).SetAddRGB(0, -100, -100).SetImageFlag(32).SetImageWrap(1)
                                           .SetAlpha(0);

        var scaleFx1 = ImageNodeFromPart(0, 5).SetPos(60, 62).SetScale(2, 1.8f).SetSize(100, 48).SetOrigin(100, 24)
                                              .SetAddRGB(-20, -200, -200).SetImageWrap(1).SetImageFlag(32).SetAlpha(0);

        var twoSmears = ImageNodeFromPart(0, 7).SetPos(180, -20).SetScale(0.65f, 0.35f).SetSize(44, 84)
                                               .SetOrigin(22, 80).SetAddRGB(50, -250, -250).SetImageFlag(1)
                                               .SetImageWrap(1).SetAlpha(0);

        var twoSmears2 = ImageNodeFromPart(0, 7).SetPos(80, -48).SetScale(0.2f, 0.2f).SetSize(44, 84)
                                                .SetOrigin(22, 80).SetAddRGB(50, -250, -250).SetImageFlag(1)
                                                .SetImageWrap(1).SetAlpha(0);

        var twoSmears3 = ImageNodeFromPart(0, 7).SetPos(190, -23).SetScale(0.73333f, 0.8f).SetSize(44, 84)
                                                .SetOrigin(22, 80).SetAddRGB(50, -200, -200)
                                                .SetImageWrap(1).SetAlpha(0);

        var twoSmears4 = ImageNodeFromPart(0, 7).SetPos(140, -30).SetScale(0.43333f, 0.4f).SetSize(44, 84)
                                                .SetOrigin(22, 80).SetAddRGB(50, -250, -250)
                                                .SetImageWrap(1).SetAlpha(0);

        return new(CreateResNode(), smear, scaleFx1, twoSmears, twoSmears2, twoSmears3, twoSmears4);
    }

    #endregion

    #region Animations

    public static KeyFrame[] BarTimeline => new KeyFrame[] { new(0) { Width = 0 }, new(1) { Width = 152 }};

    public override void HideBar(bool instant = false)
    {


        Animator -= "Fade";
        Animator += new Tween[]
        {
            new(FrameTop,
                new(0){ X=23, Y=44, ScaleX=1, AddRGB=-4, PartId=2 },
                new(instant?0:40){X=51,Y=45,ScaleX=1,AddRGB=-14,PartId=2},
                new(instant?0:165){X=30,Y=52,ScaleX=1.2f,AddRGB=-42,PartId=2},
                new(instant?0:199.9f){X=40,Y=54,ScaleX=1,AddRGB=-50,PartId=2},
                new(instant?0:201.1f){X=20,Y=64,ScaleX=1,AddRGB=0,PartId=13},
                new(instant?0:450){X=44,Y=64,ScaleX=1,AddRGB=0,PartId=13})
                {Label = "Fade"},

            new(FrameBottom,
                new(0){X=23,Y=84,ScaleX=1,AddRGB=0,PartId=11},
                new(instant?0:40){X=51,Y=75,ScaleX=1,AddRGB=0,PartId=11},
                new(instant?0:165){X=30,Y=75,ScaleX=1.2f,AddRGB=0,PartId=11},
                new(instant?0:215){X=40,Y=70,ScaleX=1,AddRGB=0,PartId=11},
                new(instant?0:234.9f){X=40,Y=64,ScaleX=1,AddRGB=17,PartId=11},
                new(instant?0:235){X=40,Y=64,ScaleX=1,AddRGB=17,PartId=12},
                new(instant?0:310){X=30,Y=64,ScaleX=1,AddRGB=new(0,20,20),PartId=12},
                new(instant?0:450){X=30,Y=64,ScaleX=1,AddRGB=new(10,20,20),PartId=12},
                new(instant?0:525){X=30,Y=64,ScaleX=1,AddRGB=0,PartId=12})
                {Label = "Fade"},

            new(Bar,
                Visible[0],
                Hidden[instant?0:40])
                {Label = "Fade"}
        };
    }

    public override void RevealBar(bool instant = false)
    {
        Animator -= "Fade";
        Animator += new Tween[]
        {
            new(FrameTop,
                new(0){X=44,Y=64,ScaleY=1,Alpha=255,PartId=13},
                new(instant?0:50){X=12,Y=56,ScaleY=2,Alpha=0,PartId=13},
                new(instant?0:99.9f){X=-9,Y=52,ScaleY=1,Alpha=255,PartId=13},
                new(instant?0:100.1f){X=-9,Y=52,ScaleY=1,Alpha=255,PartId=2},
                new(instant?0:200){X=23,Y=44,ScaleY=1,Alpha=255,PartId=2})
                {Label = "Fade"},

            new(FrameBottom,
                new(0){X=23,Y=64,PartId=12},
                new(instant?0:50){X=16,Y=64,PartId=12},
                new(instant?0:99.9f){X=-9,Y=75,PartId=12},
                new(instant?0:100.1f){X=-9,Y=75,PartId=11},
                new(instant?0:200){X=23,Y=84,PartId=11})
                {Label = "Fade"},

            new(Bar,
                Hidden[0],
                Hidden[instant?0:100],
                Visible[instant?0:200])
                {Label = "Fade"}
        };
    }

    #endregion

    #region UpdateFuncs

    protected override void StartMilestoneAnim()
    {
        Animator -= "BarPulse";
        var colorFrame1 = new KeyFrame { AddRGB = Config.PulseColor  };
        var colorFrame2 = new KeyFrame { AddRGB = Config.PulseColor2 };

        var avg = (Config.PulseColor3.R+Config.PulseColor3.G+Config.PulseColor3.B)/3f;


        var p1 = Config.PulseColor3 - new AddRGB((short)avg);
        var p2 = Config.PulseColor3 - new AddRGB((short)(avg - 33));
        var p3 = Config.PulseColor3 - new AddRGB((short)(avg - 17));
        var p4 = Config.PulseColor3 - new AddRGB((short)(avg - 32));

        Animator += new Tween[]{
            new(Main,
                colorFrame1[0],
                colorFrame2[800],
                colorFrame1[1600])
                { Ease = SinInOut, Repeat = true, Label = "BarPulse" },

            new(DragonBg,
            new(0){AddRGB=p1},
            new(450){AddRGB=p2},
            new(900){AddRGB=p1})
            { Ease = SinInOut, Repeat = true, Label = "BarPulse" },

            new(DragonLineArt,
                new(0){AddRGB=p3},
                new(450){AddRGB=p4},
                new(900){AddRGB=p3})
                { Ease = SinInOut, Repeat = true, Label = "BarPulse" }
        };

    }

    protected override void StopMilestoneAnim()
    {
        Animator -= "BarPulse";
        Main.SetAddRGB(Config.MainColor);
        DragonBg.SetAddRGB(Config.DragonBg);
        DragonLineArt.SetAddRGB(Config.DragonLineArt);
    }

    #endregion

    #region Configs

    public sealed class DragonSpearConfig : GaugeBarWidgetConfig
    {
        [DefaultValue(true)] public bool ShowDragon = true;

        public AddRGB DragonBg = new(0, 50, 180);
        public AddRGB DragonLineArt = new(-60, -20, 80);

        public AddRGB BarBg = new(0, 0, 0, 229);
        public ColorRGB FrameColor = new(100, 100, 100);

        public AddRGB MainColor = new(60,-80,-100);
        public AddRGB GainColor = "0xDD7F30FF";
        public AddRGB DrainColor = new(20,-100,-50);

        public AddRGB PulseColor = "0xE39A63ff";
        public AddRGB PulseColor2 = "0xD16347FF";
        public AddRGB PulseColor3 = "0xB23F57ff";

        protected override NumTextProps NumTextDefault => new(enabled:   true,
                                                              position:  new(0, 0),
                                                              color:     new(255),
                                                              edgeColor: "0x9f835bff",
                                                              showBg:    true,
                                                              bgColor:   new(0),
                                                              font:      MiedingerMed,
                                                              fontSize:  18,
                                                              align:     Center,
                                                              showZero:  true,
                                                              invert:    false);

        public DragonSpearConfig(WidgetConfig widgetConfig) : base(widgetConfig.DragonSpearCfg)
        {
            var config = widgetConfig.DragonSpearCfg;

            if (config == null) return;

            ShowDragon = config.ShowDragon;

            DragonBg = config.DragonBg;
            DragonLineArt = config.DragonLineArt;

            BarBg = config.BarBg;
            FrameColor = config.FrameColor;
            MainColor = config.MainColor;
            GainColor = config.GainColor;
            DrainColor = config.DrainColor;

            PulseColor = config.PulseColor;
            PulseColor2 = config.PulseColor2;
            PulseColor3 = config.PulseColor3;
        }

        public DragonSpearConfig() => HideEmpty = true;
    }

    private DragonSpearConfig config;

    public override DragonSpearConfig Config => config;

    public override void InitConfigs()
    {
        config = new(Tracker.WidgetConfig);
        if (Tracker.WidgetConfig.DragonSpearCfg == null)
        {
            Config.HideEmpty = true;
            if (ShouldInvertByDefault) Config.Invert = true;
        }
    }

    public override void ResetConfigs() => config = new();

    public override void ApplyConfigs()
    {
        WidgetContainer.SetPos(Config.Position)
                  .SetScale(Config.Scale);

        Backdrop.SetAddRGB(Config.BarBg,true);
        Main.SetAddRGB(Config.MainColor);
        Drain.SetAddRGB(Config.DrainColor);
        Gain.SetAddRGB(Config.GainColor);

        FrameTop.SetMultiply(Config.FrameColor);
        FrameBottom.SetMultiply(Config.FrameColor);

        DragonBg.SetVis(Config.ShowDragon);

        DragonLineArt.SetVis(Config.ShowDragon);


        HandleMilestone(CalcProg(), true);
    }

    public override void DrawUI()
    {
        base.DrawUI();
        switch (UiTab)
        {
            case Layout:
                ToggleControls("Show Dragon", ref Config.ShowDragon);
                break;
            case Colors:
                if (Config.ShowDragon)
                {
                    ColorPickerRGBA("Backdrop", ref Config.DragonBg);
                    ColorPickerRGBA("Line Art", ref Config.DragonLineArt);
                }

                ColorPickerRGBA("Bar Backdrop", ref Config.BarBg);
                ColorPickerRGB("Frame Tint", ref Config.FrameColor);
                ColorPickerRGBA("Main Bar", ref Config.MainColor);
                ColorPickerRGBA("Gain", ref Config.GainColor);
                ColorPickerRGBA("Drain", ref Config.DrainColor);
                break;
            case Behavior:
                SplitChargeControls(ref Config.SplitCharges, Tracker.RefType, Tracker.CurrentData.MaxCount);
                ToggleControls("Invert Fill", ref Config.Invert);
                HideControls("Collapse Empty", "Collapse Full");
                MilestoneControls("Pulse", ref Config.MilestoneType, ref Config.Milestone);

                if (Config.MilestoneType > 0)
                {
                    ColorPickerRGB("Pulse Colors", ref Config.PulseColor);
                    ColorPickerRGB(" ##Pulse2", ref Config.PulseColor2);

                    if (Config.ShowDragon) ColorPickerRGB(" ##Pulse3", ref Config.PulseColor3);
                }
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
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] public DragonSpearConfig? DragonSpearCfg { get; set; }
}
