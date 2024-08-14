using CustomNodes;
using GaugeOMatic.CustomNodes.Animation;
using GaugeOMatic.Trackers;
using Newtonsoft.Json;
using System.Numerics;
using static CustomNodes.CustomNode.CustomNodeFlags;
using static CustomNodes.CustomNodeManager;
using static FFXIVClientStructs.FFXIV.Component.GUI.AlignmentType;
using static FFXIVClientStructs.FFXIV.Component.GUI.FontType;
using static GaugeOMatic.CustomNodes.Animation.KeyFrame;
using static GaugeOMatic.CustomNodes.Animation.Tween.EaseType;
using static GaugeOMatic.Utility.Color;
using static GaugeOMatic.Widgets.GaugeBarWidgetConfig;
using static GaugeOMatic.Widgets.InkSlash;
using static GaugeOMatic.Widgets.NumTextProps;
using static GaugeOMatic.Widgets.WidgetTags;
using static GaugeOMatic.Widgets.WidgetUI;
using static GaugeOMatic.Widgets.WidgetUI.UpdateFlags;
using static GaugeOMatic.Widgets.WidgetUI.WidgetUiTab;
using static System.Math;

#pragma warning disable CS8618

namespace GaugeOMatic.Widgets;

public sealed unsafe class InkSlash : GaugeBarWidget
{
    public InkSlash(Tracker tracker) : base(tracker) { }

    public override WidgetInfo WidgetInfo => GetWidgetInfo;

    public static WidgetInfo GetWidgetInfo { get; } = new()
    {
        DisplayName = "InkSlash Gauge",
        Author = "ItsBexy",
        Description = "A gauge bar shaped like a streak of ink.",
        WidgetTags = GaugeBar
    };

    public override CustomPartsList[] PartsLists { get; } = {
        new ("ui/uld/Mobhunt5.tex",
             new(650, 0, 16, 192),
             new(616, 192, 50, 140)), // smudge
        new ("ui/uld/JobHudRDM0.tex",
             new(119, 322, 85, 56),
             new(0, 265, 42, 44),
             new(79, 207, 34, 34),
             new(125, 212, 24, 22),
             new(123, 234, 28, 20),
             new(148, 222, 14, 15),
             new(207, 321, 39, 59))
    };

    #region Nodes

    public CustomNode Bar;
    public CustomNode Backdrop;
    public CustomNode MainContainer;
    public CustomNode GainContainer;
    public CustomNode DrainContainer;
    public CustomNode Tick;

    public CustomNode SplatterBox;
    public CustomNode Splatter1;
    public CustomNode Splatter2;
    public CustomNode Splatter3;
    public CustomNode Splatter4;
    public CustomNode Splatter5;

    public override CustomNode BuildContainer()
    {
        Bar = BuildBar().SetOrigin(-45, 25);
        NumTextNode = new();

        return new(CreateResNode(), Bar, NumTextNode);
    }

    private CustomNode BuildBar()
    {
        Tick = ImageNodeFromPart(1, 5).SetOrigin(17, 17)
                                      .SetPos(0, 1)
                                      .SetImageFlag(32)
                                      .RemoveFlags(SetVisByAlpha)
                                      .Hide();

        Animator += new Tween(Tick,
                              new(0) { ScaleX = 0.5f, ScaleY = 1.55f },
                              new(600) { ScaleX = 0.5f, ScaleY = 1.6f },
                              new(1000) { ScaleX = 0.5f, ScaleY = 1.55f })
                              { Repeat = true, Ease = SinInOut };

        Backdrop = ImageNodeFromPart(0, 1).SetRotation((float)(PI / 2f))
                                          .SetPos(29, 11)
                                          .SetScale(0.8f, 1.2f);

        DrainContainer = BuildFillNode();
        GainContainer = BuildFillNode();
        MainContainer = BuildFillNode();
        Main = MainContainer[0];
        Gain = GainContainer[0];
        Drain = DrainContainer[0];

        Splatter1 = ImageNodeFromPart(1, 5).SetAlpha(0);
        Splatter2 = ImageNodeFromPart(1, 4).SetAlpha(0).SetOrigin(14, 10);
        Splatter3 = ImageNodeFromPart(1, 3).SetAlpha(0).SetOrigin(12, 11);
        Splatter4 = ImageNodeFromPart(1, 5).SetAlpha(0);
        Splatter5 = ImageNodeFromPart(1, 6).SetAlpha(0).SetAddRGB(-255).SetOrigin(0, 59);

        SplatterBox = new CustomNode(CreateResNode(), Splatter1, Splatter2, Splatter3, Splatter4, Splatter5).SetPos(0, 30);

        return new(CreateResNode(), Backdrop, SplatterBox, DrainContainer, GainContainer, MainContainer, Tick);
    }

    private CustomNode BuildFillNode() => new CustomNode(CreateResNode(), ImageNodeFromPart(0, 0).SetSize(16, 0).SetImageWrap(1).SetPos(0, 22).SetScale(1, 1).SetRotation(-(float)(PI / 2f))).SetPos(-152, 30).SetSize(192, 16);

    #endregion

    #region Animations

    public static KeyFrame[] BarTimeline => new KeyFrame[] { new(0) { Height = 3 }, new(1) { Height = 188 }};

    #endregion

    #region UpdateFuncs

    public override void OnDecreaseToMin() =>
        Animator += new Tween[]
        {
            new(Tick, Visible[0], Hidden[200]),
            new(Backdrop,
                new(0) { AddRGB = Config.BackdropColor, Alpha = Config.BackdropColor.A },
                new(200) { AddRGB = Config.BackdropInactive, Alpha = Config.BackdropInactive.A })
        };

    public override void OnIncreaseFromMin() =>
        Animator += new Tween[]
        {
            new(Tick, Hidden[0], Visible[200]),
            new(Backdrop, new(0) { AddRGB = Config.BackdropInactive, Alpha = Config.BackdropInactive.A }, new(200) { AddRGB = Config.BackdropColor, Alpha = Config.BackdropColor.A })
        };

    public override void PlaceTickMark(float prog) { Tick.SetPos(MainContainer.Node->X - 12 + Main.Node->Height, MainContainer.Node->Y + 10); }

    public override void OnFirstRun(float prog)
    {
        base.OnFirstRun(prog);
        Tick.SetAlpha(prog > 0);
        Backdrop.SetAddRGB(prog <= 0 ? Config.BackdropInactive : Config.BackdropColor, true);
    }

    public override void OnIncrease(float prog, float prevProg)
    {
        SplatterBox.SetPos(Interpolate(-147f, 38f, prog)!.Value, 30);

        var flash = Config.MainColor / 2;
        var avgScale = (Config.Scale.Y + Config.Scale.X) / 2f;
        var pos = Config.NumTextProps.Position + new Vector2((avgScale * -46.712f) - 30.915f, (avgScale * 0.7827f) + 25.3f);

        Animator += new Tween[]
        {
            new(Bar,
                new(0) { X = 0, Y = 0, AddRGB = new(0) },
                new(30) { X = 1.9f, Y = 0.95f, AddRGB = flash * 0.1f },
                new(100) { X = -0.8f, Y = -0.9f, AddRGB = flash * 0.45f },
                new(160) { X = 1.9f, Y = 0.9f, AddRGB = flash * 1 },
                new(180) { X = 1.75f, Y = 0.85f, AddRGB = flash * 0.9f },
                new(240) { X = 0, Y = 0, AddRGB = flash * 0.5f },
                new(500) { X = 0, Y = 0, AddRGB = new(0, 0, 0) }),

            new(NumTextNode,
                new(0) { X = pos.X, Y = pos.Y },
                new(30) { X = pos.X - 1.9f, Y = pos.Y + 0.85f },
                new(100) { X = pos.X + 0.8f, Y = pos.Y + 0.9f },
                new(160) { X = pos.X - 1.9f, Y = pos.Y - 0.9f },
                new(180) { X = pos.X - 1.75f, Y = pos.Y + 0.95f },
                new(240) { X = pos.X, Y = pos.Y },
                new(500) { X = pos.X, Y = pos.Y }),

            new(SplatterBox,
                new(0) { Rotation = 0, ScaleX = 1, ScaleY = 1 },
                new(600) { Rotation = 0.01f, ScaleX = 1.05f, ScaleY = 1.06f }),

            new(Splatter1,
                new(0) { X = -90, Y = 20, Scale = 1, Alpha = 0, Rotation = 3.45F },
                new(160) { X = 20, Y = -40, ScaleX = 2.5f, ScaleY = 1.6f, Alpha = 255, Rotation = 3.05F },
                new(300) { X = 20, Y = -40, ScaleX = 2.5f, ScaleY = 1.6f, Alpha = 255, Rotation = 3.05F },
                new(600) { X = 20, Y = -40, ScaleX = 2.5f, ScaleY = 1.6f, Alpha = 0, Rotation = 3.1F }),

            new(Splatter2,
                new(0) { X = -90, Y = 20, Scale = 1, Alpha = 0, Rotation = 3.45F },
                new(150) { X = 50, Y = -5, ScaleX = 2.5f, ScaleY = 1.6f, Alpha = 255, Rotation = 3.05F },
                new(250) { X = 50, Y = -5, ScaleX = 2.5f, ScaleY = 1.6f, Alpha = 255, Rotation = 3.05F },
                new(600) { X = 50, Y = -5, ScaleX = 2.5f, ScaleY = 1.6f, Alpha = 0, Rotation = 3.1F }),

            new(Splatter3,
                new(0) { X = -100, Scale = 1, Alpha = 0, Rotation = 2.85F },
                new(100) { X = -20, ScaleX = 3.2f, ScaleY = 2.9f, Alpha = 255, Rotation = 3.45F },
                new(200) { X = -20, ScaleX = 3.2f, ScaleY = 2.9f, Alpha = 255, Rotation = 3.45F },
                new(600) { X = -20, ScaleX = 3.2f, ScaleY = 2.9f, Alpha = 0, Rotation = 3.55F }),

            new(Splatter4,
                new(0) { X = -70, Y = 10, Scale = 1, Alpha = 0, Rotation = 3.45F },
                new(180) { X = -40, Y = 30, ScaleX = 2.5f, ScaleY = 1.6f, Alpha = 255, Rotation = 3.05F },
                new(320) { X = -40, Y = 30, ScaleX = 2.5f, ScaleY = 1.6f, Alpha = 255, Rotation = 3.05F },
                new(600) { X = -40, Y = 30, ScaleX = 2.5f, ScaleY = 1.6f, Alpha = 0, Rotation = 3.1F }),

            new(Splatter5,
                new(0) { X = -220, Y = -65, ScaleX = 1.3f, ScaleY = 1.8f, Alpha = 0, Rotation = 1.2f },
                new(100) { X = -200, Y = -80, ScaleX = 2f, ScaleY = 4f, Alpha = 255, Rotation = 1.4f },
                new(200) { X = -200, Y = -80, ScaleX = 2f, ScaleY = 4f, Alpha = 255, Rotation = 1.4f },
                new(700) { X = -200, Y = -80, ScaleX = 2f, ScaleY = 4f, Alpha = 0, Rotation = 1.4f })
        };
    }

    #endregion

    #region Configs

    public sealed class InkSlashConfig : GaugeBarWidgetConfig
    {
        public Vector2 Position;
        public Vector2 Scale = new(1, 1);
        public AddRGB BackdropColor = new(-255);
        public AddRGB BackdropInactive = new(-255,-255,-255, 128);
        public AddRGB MainColor = new(55, -255, -255);
        public AddRGB GainColor = "0xFF785CFF";
        public AddRGB DrainColor = "0x6A003CFF";
        public float Rotation;
        public AddRGB TickColor = new(255, -100, -162);
        protected override NumTextProps NumTextDefault => new(enabled:   true,
                                                              position:  new(0, 0),
                                                              color:     0xFFFFFFFF,
                                                              edgeColor: 0x9B0000FF,
                                                              showBg:    false,
                                                              bgColor:   new(0),
                                                              font:      MiedingerMed,
                                                              fontSize:  20,
                                                              align:     Center,
                                                              invert:    false);

        public InkSlashConfig(WidgetConfig widgetConfig) : base(widgetConfig.InkSlashCfg)
        {
            var config = widgetConfig.InkSlashCfg;

            if (config == null) return;

            Position = config.Position;
            Scale = config.Scale;
            Rotation = config.Rotation;
            MainColor = config.MainColor;
            BackdropColor = config.BackdropColor;
            GainColor = config.GainColor;
            DrainColor = config.DrainColor;
            BackdropInactive = config.BackdropInactive;
        }

        public InkSlashConfig() { }
    }

    public override GaugeBarWidgetConfig GetConfig => Config;

    public InkSlashConfig Config;

    public override void InitConfigs()
    {
        Config = new(Tracker.WidgetConfig);
        if (Tracker.WidgetConfig.InkSlashCfg == null && ShouldInvertByDefault) { Config.Invert = true; }
    }

    public override void ResetConfigs() => Config = new();

    public AddRGB ColorOffset = new(-46, 110, 110, 0);
    public AddRGB SplatterOffset = new(-102, 128, 112);
    public Vector2 PosAdjust = new(60,-50);
    public override void ApplyConfigs()
    {
        WidgetContainer.SetPos(Config.Position+PosAdjust);
        Bar.SetScale(Config.Scale);
        Tick.SetAddRGB(Config.TickColor);

        var splatterColor = Config.MainColor + SplatterOffset;
        Splatter1.SetAddRGB(splatterColor);
        Splatter2.SetAddRGB(splatterColor);
        Splatter3.SetAddRGB(splatterColor);
        Splatter4.SetAddRGB(splatterColor);

        Backdrop.SetAddRGB(Tracker.CurrentData.GaugeValue <= 0 ? Config.BackdropInactive : Config.BackdropColor, true);
        MainContainer.SetAddRGB(Config.MainColor + ColorOffset, true);
        GainContainer.SetAddRGB(Config.GainColor + ColorOffset, true);
        DrainContainer.SetAddRGB(Config.DrainColor + ColorOffset, true);

        Main.DefineTimeline(BarTimeline);
        Gain.DefineTimeline(BarTimeline).SetHeight(0);
        Drain.DefineTimeline(BarTimeline).SetHeight(0);

        var avgScale = (Config.Scale.Y + Config.Scale.X) / 2f;
        var pos = Config.NumTextProps.Position + new Vector2((avgScale * -46.712f) - 30.915f, (avgScale * 0.7827f) + 25.3f);

        NumTextNode.SetScale(avgScale);
        NumTextNode.ApplyProps(Config.NumTextProps);
        NumTextNode.SetPos(pos);
    }

    public override void DrawUI(ref WidgetConfig widgetConfig)
    {
        switch (UiTab)
        {
            case Layout:
                PositionControls("Position", ref Config.Position);
                ScaleControls("Scale", ref Config.Scale);
                break;
            case Colors:
                ColorPickerRGBA("Active Backdrop", ref Config.BackdropColor);
                ColorPickerRGBA("Inactive Backdrop", ref Config.BackdropInactive);
                ColorPickerRGBA("Main Bar", ref Config.MainColor);
                ColorPickerRGBA("Gain", ref Config.GainColor);
                ColorPickerRGBA("Drain", ref Config.DrainColor);
                ColorPickerRGBA("Tick Color", ref Config.TickColor);
                break;
            case Behavior:
                SplitChargeControls(ref Config.SplitCharges, Tracker.RefType, Tracker.CurrentData.MaxCount);
                ToggleControls("Invert Fill", ref Config.Invert);
                break;
            case Text:
                NumTextControls($"{Tracker.TermGauge} Text", ref Config.NumTextProps);
                break;
            default:
                break;
        }

        if (UpdateFlag.HasFlag(Save)) ApplyConfigs();
        widgetConfig.InkSlashCfg = Config;
    }

    #endregion
}

public partial class WidgetConfig
{
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] public InkSlashConfig? InkSlashCfg { get; set; }
}
