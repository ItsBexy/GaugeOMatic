using GaugeOMatic.Trackers;
using GaugeOMatic.Windows;
using Newtonsoft.Json;
using System;
using System.Numerics;
using static CustomNodes.CustomNodeManager;
using static FFXIVClientStructs.FFXIV.Component.GUI.AlignmentType;
using static FFXIVClientStructs.FFXIV.Component.GUI.FontType;
using static GaugeOMatic.Utility.Color;
using static GaugeOMatic.Widgets.FaerieLess;
using static GaugeOMatic.Widgets.NumTextProps;
using static GaugeOMatic.Widgets.WidgetTags;
using static GaugeOMatic.Widgets.WidgetUI;
using static GaugeOMatic.Windows.UpdateFlags;

namespace GaugeOMatic.Widgets;

public sealed unsafe class FaerieLess : GaugeBarWidget
{
    public override WidgetInfo WidgetInfo => GetWidgetInfo;

    public static WidgetInfo GetWidgetInfo => new() 
    {
        DisplayName = "Faerie-Less Gauge",
        Author = "ItsBexy",
        Description = "A slightly curved gauge bar shaped like the Faerie Gauge, but without the decorations.",
        WidgetTags = GaugeBar | Replica
    };

    public override CustomPartsList[] PartsLists { get; } = {
        new ("ui/uld/JobHudSCH1.tex",
             new(3, 80, 174, 44),
             new(3, 124, 174, 44))
    };

    #region Nodes

    public CustomNode Backdrop;
    public override CustomNode Drain { get; set; }
    public override CustomNode Gain { get; set; }
    public override CustomNode Main { get; set; }
    public override CustomNode NumTextNode { get; set; }

    public override CustomNode BuildRoot()
    {
        Backdrop = ImageNodeFromPart(0,1).SetImageWrap(1).SetRGBA(Config.Background);
        Drain = ImageNodeFromPart(0,0).SetWidth(0).SetImageWrap(1).SetRGBA(Config.DrainColor);
        Gain = ImageNodeFromPart(0,0).SetWidth(0).SetImageWrap(1).SetRGBA(Config.GainColor);
        Main = ImageNodeFromPart(0,0).SetWidth(0).SetImageWrap(1).SetRGBA(Config.MainColor);
        NumTextNode = CreateNumTextNode();

        return new(CreateResNode(), Backdrop, Drain, Gain, Main, NumTextNode);
    }

    #endregion

    #region Animations

    #endregion

    #region UpdateFuncs

    public override string? SharedEventGroup => null;

    public override DrainGainType DGType => DrainGainType.Width;
    public override float CalcBarProperty(float prog) => (ushort)Math.Round(Math.Clamp(prog, 0f, 1f) * 174f);

    #endregion

    #region Configs

    public sealed class FaerieLessConfig : GaugeBarWidgetConfig
    {
        public Vector2 Position = new(50, 28);
        public Vector2 Scale = new(1, 1);
        public float Angle;
        public ColorRGB MainColor = new(255, 255, 255);
        public ColorRGB Background = new(255, 255, 255);
        public ColorRGB GainColor = new(51, 213, 207);
        public ColorRGB DrainColor = new(113, 5, 70);
        public bool Mirror;

        protected override NumTextProps NumTextDefault => new(true, new(133, 40), 0xffffffff, 0x288246ff, MiedingerMed, 18, Right, false);

        public FaerieLessConfig(WidgetConfig widgetConfig)
        {
            NumTextProps = NumTextDefault;
            var config = widgetConfig.FaerieLessCfg;

            if (config == null) return;

            Position = config.Position;
            Scale = config.Scale;
            Angle = config.Angle;
            MainColor = config.MainColor;
            Background = config.Background;
            GainColor = config.GainColor;
            DrainColor = config.DrainColor;
            NumTextProps = config.NumTextProps;
            Mirror = config.Mirror;
            Invert = config.Invert;
        }

        public FaerieLessConfig()
        {
            NumTextProps = NumTextDefault;
        }
    }

    public override GaugeBarWidgetConfig GetConfig => Config;

    public FaerieLessConfig Config = null!;

    public override void InitConfigs()
    {
        Config = new(Tracker.WidgetConfig);
        if (Tracker.WidgetConfig.FaerieLessCfg == null && Tracker.RefType == RefType.Action) { Config.Invert = true; }
    }

    public override void ResetConfigs() => Config = new();

    public override void ApplyConfigs()
    {
        WidgetRoot.SetPos(Config.Position);
        WidgetRoot.SetScale(Config.Scale.X, Config.Scale.Y * (Config.Mirror ? -1f : 1f));
        WidgetRoot.Node->Rotation = (float)(Config.Angle * (Math.PI / 180f));
        WidgetRoot.Node->DrawFlags |= 0xD;

        Backdrop.SetRGBA(Config.Background);
        Main.SetRGBA(Config.MainColor);
        Gain.SetRGBA(Config.GainColor);
        Drain.SetRGBA(Config.DrainColor);

        Config.NumTextProps.ApplyTo(NumTextNode);
    }

    public override void DrawUI(ref WidgetConfig widgetConfig, ref UpdateFlags update)
    {
        Heading("Layout");
        PositionControls("Position", ref Config.Position, ref update);
        ScaleControls("Scale", ref Config.Scale, ref update);
        FloatControls("Angle", ref Config.Angle, -180, 180, 1f, ref update);
        ToggleControls("Mirror", ref Config.Mirror, ref update);

        Heading("Colors");
        ColorPickerRGBA("Backdrop", ref Config.Background, ref update);
        ColorPickerRGBA("Main Bar", ref Config.MainColor, ref update);
        ColorPickerRGBA("Gain", ref Config.GainColor, ref update);
        ColorPickerRGBA("Drain", ref Config.DrainColor, ref update);

        Heading("Behavior");
        ToggleControls("Invert Fill", ref Config.Invert, ref update);

        NumTextControls($"{Tracker.TermGauge} Text", ref Config.NumTextProps, ref update);

        if (update.HasFlag(Save)) ApplyConfigs();
        widgetConfig.FaerieLessCfg = Config;
    }

    #endregion

    public FaerieLess(Tracker tracker) : base(tracker) { }
}

public partial class WidgetConfig
{
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] public FaerieLessConfig? FaerieLessCfg { get; set; }
}
