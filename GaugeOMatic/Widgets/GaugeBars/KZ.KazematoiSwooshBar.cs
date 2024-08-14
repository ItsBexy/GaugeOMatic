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
using static GaugeOMatic.Utility.Color;
using static GaugeOMatic.Widgets.Common.CommonParts;
using static GaugeOMatic.Widgets.GaugeBarWidgetConfig;
using static GaugeOMatic.Widgets.KazematoiSwooshBar;
using static GaugeOMatic.Widgets.LabelTextProps;
using static GaugeOMatic.Widgets.NumTextProps;
using static GaugeOMatic.Widgets.WidgetInfo;
using static GaugeOMatic.Widgets.WidgetTags;
using static GaugeOMatic.Widgets.WidgetUI;
using static GaugeOMatic.Widgets.WidgetUI.UpdateFlags;
using static GaugeOMatic.Widgets.WidgetUI.WidgetUiTab;

#pragma warning disable CS8618

namespace GaugeOMatic.Widgets;

public sealed unsafe class KazematoiSwooshBar : GaugeBarWidget
{
    public KazematoiSwooshBar(Tracker tracker) : base(tracker) { }

    public override WidgetInfo WidgetInfo => GetWidgetInfo;

    public static WidgetInfo GetWidgetInfo { get; } = new()
    {
        DisplayName = "Kazematoi Bar",
        Author = "ItsBexy",
        Description = "A bar based on the backdrop of NIN's Kazematoi",
        WidgetTags = GaugeBar | MultiComponent | HasAddonRestrictions,
        RestrictedAddons = ClipConflictAddons,
        MultiCompData = new("KZ", "Kazematoi Replica", 2)
    };

    public override CustomPartsList[] PartsLists { get; } = { NIN1,
        new("ui/uld/JobHudNIN1Mask.tex",new Vector4(0,0,128,96)),
        new ("ui/uld/JobHudNIN0.tex", new Vector4(256, 152, 20, 88))
    };

    #region Nodes

    public CustomNode ColorBackdrop;
    public CustomNode GreyBackdrop;
    public CustomNode Mask;
    public CustomNode FillBox;
    public CustomNode Fill1;
    public CustomNode Fill2;
    public CustomNode Fill3;
    public CustomNode Fill4;
    public CustomNode Contents;
    public CustomNode TickWrapper;
    public CustomNode TickMark;
    public CustomNode Flash;
    public LabelTextNode LabelTextNode;

    public override CustomNode BuildContainer()
    {
        Mask = ClippingMaskFromPart(1, 0).SetPos(5, 3);

        NumTextNode = new();
        LabelTextNode = new LabelTextNode(Config.LabelTextProps.Text, Tracker.DisplayAttr.Name);
        Gain = new CustomNode();
        Drain = new CustomNode();

        Fill1 = ImageNodeFromPart(0, 5)
         .SetImageWrap(2)
         .SetSize(128, 128)
         .SetPos(0, 128)
         .SetImageFlag(32)
         .DefineTimeline(Fill1Timeline);

        Fill2 = ImageNodeFromPart(0, 5)
         .SetImageWrap(2)
         .SetSize(128, 128)
         .SetPos(128, 128)
         .SetImageFlag(32)
         .SetOrigin(0, 128)
         .DefineTimeline(Fill2Timeline);

        Fill3 = ImageNodeFromPart(0, 5)
         .SetImageWrap(2)
         .SetSize(128, 128)
         .SetPos(128, 0)
         .SetImageFlag(32)
         .SetOrigin(128, 128)
         .DefineTimeline(Fill3Timeline);

        Fill4 = ImageNodeFromPart(0, 5)
         .SetImageWrap(2)
         .SetSize(128, 128)
         .SetPos(0, 0)
         .SetImageFlag(32)
         .SetOrigin(128, 0)
         .DefineTimeline(Fill4Timeline);

        TickMark = ImageNodeFromPart(2, 0)
         .SetScale(0.5f)
         .SetRotation(-90)
         .SetOrigin(20, 44)
         .SetY(83)
         .SetImageFlag(32)
         .DefineTimeline(TickTimeline);

        Main = new CustomNode(CreateResNode(), Fill1, Fill2, Fill3, Fill4).SetPos(-95, -42)
           .SetOrigin(128, 128)
           .SetAddRGB(64, 0, 128)
           .DefineTimeline(BoxTimeline);

        TickWrapper = new CustomNode(CreateResNode(), TickMark).SetPos(-95, -42)
           .SetOrigin(128, 128)
           .DefineTimeline(BoxTimeline);

        Flash = ImageNodeFromPart(0, 1).SetImageFlag(32).SetAlpha(0).SetOrigin(69, 51);

        Animator += new Tween[] {
            new(TickWrapper, new(0) { Alpha = 255 }, new(100) { Alpha = 200 }, new(200) { Alpha = 255 }) { Repeat = true },
            new(TickMark, new(0) { ScaleX = 0.5f }, new(160) { ScaleX = 0.7f }, new(280) { ScaleX = 0.5f }) { Repeat = true }
        };

        FillBox = new CustomNode(CreateResNode(), Main, Mask, TickWrapper);

        ColorBackdrop = ImageNodeFromPart(0, 1).DefineTimeline(BgTimeline);
        GreyBackdrop = ImageNodeFromPart(0, 0);

        Contents = new CustomNode(CreateResNode(), GreyBackdrop, ColorBackdrop, FillBox, Flash).SetOrigin(69, 51);


        return new CustomNode(CreateResNode(), Contents, NumTextNode, LabelTextNode).SetOrigin(69, 51);
    }


    #endregion

    #region Animations

    public static KeyFrame[] BoxTimeline =>
        new KeyFrame[] {
            new(0) { X = -88, Y = -50F, Rotation = -0.226892803f },
            new(25) { X = -65.625f, Y = -69.125F, Rotation = 0.701622359f },
            new(50) { X = -51, Y = -82F, Rotation = 1.944296787f },
            new(75) { X = -44.125f, Y = -88.625F, Rotation = 3.50113048f },
            new(100) { X = -45, Y = -89F, Rotation = 5.372123438f }
        };


    public static KeyFrame[] Fill1Timeline =>
        new KeyFrame[]
        {
            new(0) { ScaleX = 1, ScaleY = 0.05F },
            new(10) { ScaleX = 0.94F, ScaleY = 0.15F },
            new(20) { ScaleX = 0.94F, ScaleY = 1 },
            new(25) { ScaleX = 1, ScaleY = 1 },
            new(100) { ScaleX = 1, ScaleY = 1 }
        };

    public static KeyFrame[] Fill2Timeline =>
        new KeyFrame[]
        {
            new(0) { Alpha = 0, ScaleY = 0, ScaleX = 0 },
            new(29) { Alpha = 0, ScaleY = 0, ScaleX = 0 },
            new(30) { Alpha = 255, ScaleY = 0.7f, ScaleX = 0.05F },
            new(40) { Alpha = 255, ScaleY = 0.7F, ScaleX = 0.23F },
            new(50) { Alpha = 255, ScaleY = 0.85F, ScaleX = 1 },
            new(57) { Alpha = 255, ScaleY = 1, ScaleX = 1 },
            new(100) { Alpha = 255, ScaleY = 1, ScaleX = 1 }
        };

    public static KeyFrame[] Fill3Timeline =>
        new KeyFrame[]
        {
            new(0) { Alpha = 0, ScaleX = 0, ScaleY = 0 },
            new(56) { Alpha = 0, ScaleX = 0, ScaleY = 0 },
            new(57) { Alpha = 255, ScaleX = 0.01f, ScaleY = 0.01f },
            new(58) { Alpha = 255, ScaleX = 0.65F, ScaleY = 0.03F },
            new(60) { Alpha = 255, ScaleX = 0.65f, ScaleY = 0.05F },
            new(65) { Alpha = 255, ScaleX = 0.65f, ScaleY = 0.2F },
            new(70) { Alpha = 255, ScaleX = 0.71F, ScaleY = 0.4F },
            new(75) { Alpha = 255, ScaleX = 0.84F, ScaleY = 1 },
            new(80) { Alpha = 255, ScaleX = 1, ScaleY = 1 },
            new(90) { Alpha = 255, ScaleX = 1, ScaleY = 1 },
            new(100) { Alpha = 255, ScaleX = 1, ScaleY = 1 }
        };

    public static KeyFrame[] Fill4Timeline =>
        new KeyFrame[]
        {
            new(0) { Alpha = 0, ScaleY = 0, ScaleX = 0 },
            new(79) { Alpha = 255, ScaleY = 0, ScaleX = 0 },
            new(80) { Alpha = 255, ScaleY = 0.55F, ScaleX = 0.02F },
            new(85) { Alpha = 255, ScaleY = 0.55F, ScaleX = 0.1F },
            new(90) { Alpha = 255, ScaleY = 0.67F, ScaleX = 0.31F },
            new(95) { Alpha = 255, ScaleY = 0.82F, ScaleX = 0.44F },
            new(100) { Alpha = 255, ScaleY = 0.94F, ScaleX = 1f }
        };

    public static KeyFrame[] BgTimeline =>
        new KeyFrame[] {
            new(0) { Alpha = 0 },
            new(100) { Alpha = 255 }
        };

    public static KeyFrame[] TickTimeline =>
        new KeyFrame[] {
            new(0) { X = 95, ScaleY = 0.05f, Alpha = 0 },
            new(5) { X = 85, ScaleY = 0.165f, Alpha = 255 },
            new(10) { X = 76, ScaleY = 0.25f, Alpha = 255 },
            new(15) { X = 71, ScaleY = 0.27f, Alpha = 255 },
            new(20) { X = 70, ScaleY = 0.29f, Alpha = 255 },
            new(25) { X = 70, ScaleY = 0.325f, Alpha = 255 },
            new(30) { X = 72, ScaleY = 0.325f, Alpha = 255 },
            new(35) { X = 75, ScaleY = 0.325f, Alpha = 255 },
            new(40) { X = 78, ScaleY = 0.325f, Alpha = 255 },
            new(45) { X = 79, ScaleY = 0.34f, Alpha = 255 },
            new(50) { X = 81, ScaleY = 0.39f, Alpha = 255 },
            new(55) { X = 80, ScaleY = 0.44f, Alpha = 255 },
            new(60) { X = 79, ScaleY = 0.5f, Alpha = 255 },
            new(65) { X = 79, ScaleY = 0.55f, Alpha = 255 },
            new(70) { X = 80, ScaleY = 0.575f, Alpha = 255 },
            new(75) { X = 80, ScaleY = 0.59f, Alpha = 255 },
            new(80) { X = 80, ScaleY = 0.56f, Alpha = 255 },
            new(85) { X = 79, ScaleY = 0.54f, Alpha = 255 },
            new(90) { X = 76, ScaleY = 0.57f, Alpha = 255 },
            new(95) { X = 72, ScaleY = 0.62f, Alpha = 255 },
            new(100) { X = 64, ScaleY = 0.67f, Alpha = 0 }
        };

    public override void HideBar(bool instant = false)
    {
        Animator -= "Fade";
        Animator += new Tween(WidgetContainer, new(0, WidgetContainer), Hidden[instant ? 0 : 250]) { Label = "Fade", Ease = SinInOut };
    }

    public override void RevealBar(bool instant = false)
    {
        Animator -= "Fade";
        Animator += new Tween(WidgetContainer, new(0, WidgetContainer), Visible[instant ? 0 : 250]) { Label = "Fade", Ease = SinInOut };
    }

    #endregion

    #region UpdateFuncs

    public override void OnIncrease(float prog, float prevProg)
    {
        Animator += new Tween(Flash,
         new(0) { Alpha = 0, Scale = 1, Rotation = 0 },
         new(150) { Alpha = 200, Scale = 1f, Rotation = 0 },
         new(400) { Alpha = 0, ScaleX = 1.6f, ScaleY = 1.8f, Rotation = 0f });
    }

    public override void PreUpdate(float prog, float prevProg)
    {

    }

    public override void PostUpdate(float prog)
    {
        Fill1.SetProgress(Main.Prog);
        Fill2.SetProgress(Main.Prog);
        Fill3.SetProgress(Main.Prog);
        Fill4.SetProgress(Main.Prog);
        ColorBackdrop.SetProgress(Main.Prog);
        TickWrapper.SetProgress(Main.Prog);
        TickMark.SetProgress(Main.Prog);
    }

    #endregion

    #region Configs

    public sealed class KazematoiSwooshBarConfig : GaugeBarWidgetConfig
    {
        public Vector2 Position = new(0);
        [DefaultValue(1f)] public float Scale = 1;
        public float Angle;
        public bool Mirror;
        public AddRGB MainColor = new(64, 0, 128);
        public AddRGB BgFullColor = new(0, 0, 0);
        public AddRGB BgEmptyColor = new(0, 0, 0);
        public ColorRGB TickColor = new(228, 139, 170);

        public LabelTextProps LabelTextProps = new(string.Empty, false, new(0, 0), new(255), new(0), JupiterLarge, 18, Center);
        protected override NumTextProps NumTextDefault => new(enabled: true,
           position: new(0, 0),
           color: new(255),
           edgeColor: "0x9f835bff",
           showBg: true,
           bgColor: new(0),
           font: MiedingerMed,
           fontSize: 18,
           align: Center,
           showZero: false,
           invert: false);

        public KazematoiSwooshBarConfig(WidgetConfig widgetConfig) : base(widgetConfig.KazematoiSwooshBarCfg)
        {
            var config = widgetConfig.KazematoiSwooshBarCfg;

            if (config == null) return;

            Position = config.Position;
            Scale = config.Scale;
            Angle = config.Angle;
            MainColor = config.MainColor;
            BgFullColor = config.BgFullColor;
            BgEmptyColor = config.BgEmptyColor;
            TickColor = config.TickColor;
            Mirror = config.Mirror;

            LabelTextProps = config.LabelTextProps;
        }

        public KazematoiSwooshBarConfig() { }
    }

    public override GaugeBarWidgetConfig GetConfig => Config;

    public KazematoiSwooshBarConfig Config;

    public override void InitConfigs()
    {
        Config = new(Tracker.WidgetConfig);
        if (Tracker.WidgetConfig.KazematoiSwooshBarCfg == null && ShouldInvertByDefault) { Config.Invert = true; }
    }

    public override void ResetConfigs() => Config = new();

    public override void ApplyConfigs()
    {
        WidgetContainer.SetPos(Config.Position).SetScale(Config.Scale);

        Contents.SetRotation(Config.Angle, true).SetScaleX(Config.Mirror ? -1 : 1);

        Main.SetAddRGB(Config.MainColor);
        Flash.SetAddRGB(Config.BgFullColor);

        ColorBackdrop.SetAddRGB(Config.BgFullColor);
        GreyBackdrop.SetAddRGB(Config.BgEmptyColor);

        TickMark.SetRGB(Config.TickColor);

        NumTextNode.ApplyProps(Config.NumTextProps, new(65, 53));
        LabelTextNode.ApplyProps(Config.LabelTextProps, new(-43,100));
    }

    public override void DrawUI(ref WidgetConfig widgetConfig)
    {
        switch (UiTab)
        {
            case Layout:
                PositionControls("Position", ref Config.Position);
                ScaleControls("Scale", ref Config.Scale);
                AngleControls("Angle", ref Config.Angle);
                ToggleControls("Mirror", ref Config.Mirror);
                break;
            case Colors:
                ColorPickerRGB("Fill Color", ref Config.MainColor);
                ColorPickerRGB("Tick Color", ref Config.TickColor);
                ColorPickerRGB("Backdrop (Full)", ref Config.BgFullColor);
                ColorPickerRGB("Backdrop (Empty)", ref Config.BgEmptyColor);
                break;
            case Behavior:
                SplitChargeControls(ref Config.SplitCharges, Tracker.RefType, Tracker.CurrentData.MaxCount);
                ToggleControls("Invert Fill", ref Config.Invert);
                HideControls();
                break;
            case Text:
                NumTextControls($"{Tracker.TermGauge} Text", ref Config.NumTextProps, true);
                LabelTextControls("Label Text", ref Config.LabelTextProps, Tracker.DisplayAttr.Name);
                break;
            default:
                break;
        }

        if (UpdateFlag.HasFlag(Save)) ApplyConfigs();
        widgetConfig.KazematoiSwooshBarCfg = Config;
    }

    #endregion
}

public partial class WidgetConfig
{
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] public KazematoiSwooshBarConfig? KazematoiSwooshBarCfg { get; set; }
}
