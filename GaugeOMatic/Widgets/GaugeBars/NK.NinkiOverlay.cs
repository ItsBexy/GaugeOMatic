using GaugeOMatic.Trackers;
using GaugeOMatic.Windows;
using Newtonsoft.Json;
using System;
using System.Numerics;
using static CustomNodes.CustomNodeManager;
using static FFXIVClientStructs.FFXIV.Component.GUI.AlignmentType;
using static FFXIVClientStructs.FFXIV.Component.GUI.FontType;
using static GaugeOMatic.Utility.Color;
using static GaugeOMatic.Utility.MiscMath;
using static GaugeOMatic.Widgets.GaugeBarWidget.DrainGainType;
using static GaugeOMatic.Widgets.NinkiOverlay;
using static GaugeOMatic.Widgets.NumTextProps;
using static GaugeOMatic.Widgets.WidgetTags;
using static GaugeOMatic.Widgets.WidgetUI;
using static GaugeOMatic.Windows.UpdateFlags;
#pragma warning disable CS8618

namespace GaugeOMatic.Widgets;

public sealed unsafe class NinkiOverlay : GaugeBarWidget
{
    public NinkiOverlay(Tracker tracker) : base(tracker) { }

    public override WidgetInfo WidgetInfo => GetWidgetInfo;

    public static WidgetInfo GetWidgetInfo => new()
    {
        DisplayName = "Ninki Overlay",
        Author = "ItsBexy",
        Description = "A glowing gauge bar aura fitted over the shape of the Ninki Gauge (or a replica of it).",
        WidgetTags = GaugeBar | MultiComponent,
        MultiCompData = new("NK", "Ninki Gauge Replica", 2)
    };

    public override CustomPartsList[] PartsLists { get; } = {
        new ("ui/uld/JobHudNIN0.tex",
             new(0, 0, 256, 100),
             new(256, 152, 20, 88),
             new(0, 196, 196, 56))
    };

    #region Nodes

    public CustomNode Scroll;
    public CustomNode Tick;
    public CustomNode Shine;
    public CustomNode Calligraphy;

    public override CustomNode BuildRoot()
    {
        Scroll = ImageNodeFromPart(0, 0).SetAlpha(0).SetImageWrap(1).SetImageFlag(32);
        Tick = ImageNodeFromPart(0, 1).SetAlpha(0).SetOrigin(22, 43.5f).SetImageFlag(32);
        Shine = ImageNodeFromPart(0, 1).SetAlpha(0).SetOrigin(22, 43.5f).SetImageFlag(32);
        Calligraphy = ImageNodeFromPart(0, 2).SetPos(23, 29).SetOrigin(98, 28).SetImageFlag(32).SetRGBA(new(1, 1, 1, 0));
        NumTextNode = new();

       //Flippy(Tick, 10f);

        return new(CreateResNode(), Scroll, Tick, Shine, Calligraphy, NumTextNode);
    }

    #endregion

    #region Animations

    private void AppearAnim()
    {
        Tweens.Add(new(Calligraphy,
                       new(0) { Scale = 1, Alpha = 70 },
                       new(100) { Scale = 1, Alpha = 160 },
                       new(300) { Scale = 1.5f, Alpha = 0 }));

        Tweens.Add(new(Shine, 
                       new(0) { X = 11, Y = 14, Alpha = 76 },
                       new(200) { X = 190, Y = 10, Alpha = 76 },
                       new(400) { X = 300, Y = 10, Alpha = 0 }));

        Tweens.Add(new(Shine, 
                       new(0) { ScaleX = 1.15f, ScaleY = 1.5f, Rotation = 0 },
                       new(100) { ScaleX = 3f, ScaleY = 1.2f, Rotation = -0.065f },
                       new(400) { ScaleX = 0.6f, ScaleY = 2f, Rotation = 0 }));
    }

    #endregion

    #region UpdateFuncs

    public static float CalcTickY(float prog) => prog >= 0.845 ? PolyCalc(prog, 674.909344670314, -1438.55803705433, 771.474668979089) : PolyCalc(prog, 11.6767206839834, 52.8756304874818, -160.237711149177, 114.469149751575);

    public static float CalcTickScale(float prog) => prog < 0.845 ? 1 : PolyCalc(prog, -40.6519205182959, 91.1244645648548, -49.4678579360407);


    public override void Update()
    {
        var current = Tracker.CurrentData.GaugeValue;
        var previous = Tracker.PreviousData.GaugeValue;
        var max = Tracker.CurrentData.MaxGauge;

        var prog = Math.Clamp(current / max, 0f, 1f);
        var prevProg = Math.Clamp(previous / max, 0f, 1f);

        if (prog > 0 && prevProg == 0) AppearAnim();

        NumTextNode.UpdateValue(current, max);

        var curWid = (ushort)Math.Round((prog * 229f) + 18f);
        var prevWid = (ushort)Math.Round((prevProg * 229f) + 18f);

        AnimateDrainGain(Width,ref Tweens, Scroll, curWid, prevWid, 0, Config.AnimationLength);

        var tweenWidth = Scroll.Node->Width;
        var tweenProg = (tweenWidth - 18f) / 229f;
        Tick.SetPos(tweenWidth - 19f, CalcTickY(tweenProg));

        if (tweenProg > 0) Tick.SetScaleY(CalcTickScale(tweenProg) * (Tick.Node->ScaleY < 0 ? -1 : 1));

        switch (tweenProg)
        {
            case < 0.05f:
                Scroll.SetAlpha((byte)(tweenProg / 0.05f * Config.ScrollColor.A));
                Tick.SetAlpha((byte)(tweenProg / 0.05f * Config.TickColor.A));
                break;
            default:
                Scroll.SetAlpha(Config.ScrollColor.A);
                Tick.SetAlpha(Config.TickColor.A);
                break;
        }

        RunTweens();
    }

    public override DrainGainType DGType => Width;
    public override float CalcBarProperty(float prog) => (ushort)Math.Round((prog * 229f) + 18f);

    #endregion

    #region Configs

    public sealed class NinkiOverlayConfig : GaugeBarWidgetConfig
    {
        public Vector2 Position = new(0, 0);
        public float Scale = 1;
        public AddRGB ScrollColor = new(80, 30, -70, 90);
        public ColorRGB TickColor = new(255, 164, 93);
        protected override NumTextProps NumTextDefault => new(enabled:   true, 
                                                              position:  new(167, 58),
                                                              color:     new(255, 241, 197),
                                                              edgeColor: new(110, 25, 0),
                                                              showBg:    false,
                                                              bgColor:   new(0), 
                                                              font:      MiedingerMed, 
                                                              fontSize:  20, 
                                                              align:     Center,
                                                              invert:    false);

        public NinkiOverlayConfig(WidgetConfig widgetConfig)
        {
            NumTextProps = NumTextDefault;
            var config = widgetConfig.NinkiOverlayCfg;

            if (config == null) return;

            Position = config.Position;
            Scale = config.Scale;
            ScrollColor = config.ScrollColor;
            TickColor = config.TickColor;
            NumTextProps = config.NumTextProps;
            AnimationLength = config.AnimationLength;
            Invert = config.Invert;
            SplitCharges = config.SplitCharges;
        }

        public NinkiOverlayConfig()
        {
            NumTextProps = NumTextDefault;
        }
    }

    public NinkiOverlayConfig Config;
    public override GaugeBarWidgetConfig GetConfig => Config;

    public override void InitConfigs()
    {
        Config = new(Tracker.WidgetConfig);
        if (Tracker.WidgetConfig.NinkiOverlayCfg == null && Tracker.RefType == RefType.Action) { Config.Invert = true; }
    }

    public override void ResetConfigs() => Config = new();

    public override void ApplyConfigs()
    {
        WidgetRoot.SetPos(Config.Position);
        WidgetRoot.SetScale(Config.Scale);
        Scroll.SetAddRGB(Config.ScrollColor, true);
        Tick.SetRGB(Config.TickColor);
        Shine.SetRGB(Config.TickColor);
        Calligraphy.SetAddRGB((Vector4)Config.TickColor);

        NumTextNode.ApplyProps(Config.NumTextProps);
    }

    public override void DrawUI(ref WidgetConfig widgetConfig, ref UpdateFlags update)
    {
        Heading("Layout");
        PositionControls("Position", ref Config.Position, ref update);
        ScaleControls("Scale", ref Config.Scale, ref update);

        Heading("Colors");
        ColorPickerRGBA("Scroll Color", ref Config.ScrollColor, ref update);
        ColorPickerRGBA("Tick Color", ref Config.TickColor, ref update);

        Heading("Behavior");
        
        ToggleControls("Invert Fill", ref Config.Invert, ref update);
        //  IntControls("Animation Time", ref Config.AnimationLength, 0, 2000, 50, ref update);


        NumTextControls($"{Tracker.TermGauge} Text", ref Config.NumTextProps, ref update);

        if (update.HasFlag(Save)) ApplyConfigs();
        widgetConfig.NinkiOverlayCfg = Config;
    }

    #endregion
}

public partial class WidgetConfig
{
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] public NinkiOverlayConfig? NinkiOverlayCfg { get; set; }
}
