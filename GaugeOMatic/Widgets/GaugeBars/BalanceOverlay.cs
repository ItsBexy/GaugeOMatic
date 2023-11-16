using FFXIVClientStructs.FFXIV.Component.GUI;
using GaugeOMatic.Trackers;
using GaugeOMatic.Windows;
using Newtonsoft.Json;
using System;
using System.Numerics;
using static CustomNodes.CustomNodeManager;
using static GaugeOMatic.Utility.Color;
using static GaugeOMatic.Utility.MiscMath;
using static GaugeOMatic.Widgets.BalanceOverlay;
using static GaugeOMatic.Widgets.NumTextProps;
using static GaugeOMatic.Widgets.WidgetTags;
using static GaugeOMatic.Widgets.WidgetUI;
using static GaugeOMatic.Windows.UpdateFlags;

namespace GaugeOMatic.Widgets;

public sealed unsafe class BalanceOverlay : GaugeBarWidget
{
    public BalanceOverlay(Tracker tracker) : base(tracker)
    {
     //   SharedEvents.Add("SpendShake", () => BalanceBar.SpendShake(ref Tweens, WidgetRoot, Config.GetBGColor(Tracker.CurrentData.State) * 0.25f, Config.Position.X, Config.Position.Y));
    }

    public override WidgetInfo WidgetInfo => GetWidgetInfo;

    public static WidgetInfo GetWidgetInfo => new()
    {
        DisplayName = "Balance Gauge Overlay",
        Author = "ItsBexy",
        Description = "A glowing gauge bar fitted over the Balance Gauge (or a replica of it)",
        WidgetTags = GaugeBar | MultiComponent,
        KeyText = "BL3"
    };

    public override CustomPartsList[] PartsLists { get; } = {
        new("ui/uld/JobHudRDM0.tex", 
            new(0,0,116,208), 
            new(186,3,26,124),
            new(212,3,26,124),
            new(116,0,34,144),
            new(0,208,40,56),
            new(40,208,40,56),
            new(116,144,40,60),
            new(184,132,84,188),
            new(125,212,24,22),
            new(123,234,28,20),
            new(148,222,14,15),
            new(242,3,26,124),
            new(116,332,72,32),
            new(0,264,40,48),
            new(81,239,30,40),
            new(81,279,30,40),
            new(0,319,117,61),
            new(114,258,60,60),
            new(118,321,89,59),
            new(207,321,39,59),
            new(150,0,34,144)),
        new ("ui/uld/JobHudNIN0.tex", new Vector4[] { 
            new(256, 152, 20, 88) // flashing edge 
        })
    };
    #region Nodes

    public CustomNode PlateContainer;
    public CustomNode Plate;
    public CustomNode CrystalGlow;
    public CustomNode Tick;
    public override CustomNode Main => PlateContainer;

    public override CustomNode BuildRoot()
    {
        Plate = ImageNodeFromPart(0, 0).SetImageWrap(2).SetImageFlag(32);
        PlateContainer = new CustomNode(CreateResNode(), Plate).SetNodeFlags(NodeFlags.Clip).SetSize(116, 208);
        CrystalGlow = ImageNodeFromPart(0, 5).SetPos(39, 5).SetOrigin(20, 28).SetAlpha(0);
        Tick = ImageNodeFromPart(1, 0).SetRotation(1.5707963267949f).SetImageFlag(33).SetOrigin(20,44).SetPos(37.5f,-30);
        NumTextNode = new(CreateTextNode("0", 18, 20));

        return new(CreateResNode(), PlateContainer, CrystalGlow,Tick,NumTextNode);
    }

    #endregion

    #region Animations

    #endregion

    #region UpdateFuncs

    public override float CalcBarSize(float prog) => 10 + (prog * 192);

    public override void PostUpdate(float prog)
    {
        PlateContainer.SetY(208 - PlateContainer.Height);
        Plate.SetY(-(208 - PlateContainer.Height));
    }

    public override void PlaceTickMark(float prog)
    {
        var containerY = PlateContainer.Node->Y;

        var alpha = (byte)Math.Clamp((double)(containerY <= 163.5f ? 255 : PolyCalc(containerY, -6155.37357130828d, 79.3597925187652, -0.245581741361458)), 0, 255);
        var scaleY = containerY switch
        {
            <= 80 => PolyCalc(containerY, -0.369408145220107d, 0.0761410396507537d, -0.00102457794640973d, 0.0000049971702224042d),
            <= 163.5f => PolyCalc(containerY, 2.9911068686918d, -0.00223868498009971d, -0.000303975874680723d, 0.00000164245988203053d),
            _ => PolyCalc(containerY, -88.9665931925754d, 1.53310683483466d, -0.00854319135223209d, 0.0000156241038226192d)
        };
        

        Tick.SetY(containerY-25).SetAlpha(alpha).SetScaleY(Math.Clamp(scaleY, 0, 2f));
    }

    public override DrainGainType DGType => DrainGainType.Height;
    public override string SharedEventGroup => "BalanceGauge";

    #endregion

    #region Configs

    public sealed class BalanceOverlayConfig : GaugeBarWidgetConfig
    {
        public Vector2 Position = new(0);
        public float Scale = 1;
        public AddRGB Color = new(0);
        public AddRGB TickColor = new(0);

        public BalanceOverlayConfig(WidgetConfig widgetConfig)
        {
            NumTextProps = NumTextDefault;
            var config = widgetConfig.BalanceOverlayCfg;

            if (config == null) return;

            Position = config.Position;
            Scale = config.Scale;
            Color=config.Color;
            TickColor= config.TickColor;
            NumTextProps = config.NumTextProps;
            AnimationLength = config.AnimationLength;
            Invert = config.Invert;
        }

        public BalanceOverlayConfig() { }
    }

    public override GaugeBarWidgetConfig GetConfig => Config;
    public BalanceOverlayConfig Config = null!;

    public override void InitConfigs() => Config = new(Tracker.WidgetConfig);

    public override void ResetConfigs() => Config = new();

    public override void ApplyConfigs()
    {
        WidgetRoot.SetPos(Config.Position).SetScale(Config.Scale);
        Plate.SetAddRGB(Config.Color,true);
        Tick.SetAddRGB(Config.TickColor, true);

        Config.NumTextProps.ApplyTo(NumTextNode);
    }

    public override void DrawUI(ref WidgetConfig widgetConfig, ref UpdateFlags update)
    {
        Heading("Layout");
        PositionControls("Position", ref Config.Position, ref update);
        ScaleControls("Scale", ref Config.Scale, ref update);

        Heading("Colors");
        ColorPickerRGBA("Color", ref Config.Color, ref update);
        ColorPickerRGBA("Tick Color", ref Config.TickColor, ref update);

        Heading("Behavior");
        ToggleControls("Invert Fill", ref Config.Invert, ref update);

        NumTextControls($"{Tracker.TermGauge} Text", ref Config.NumTextProps, ref update);

        if (update.HasFlag(Save)) ApplyConfigs();
        widgetConfig.BalanceOverlayCfg = Config;
    }

    #endregion
}

public partial class WidgetConfig
{
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] public BalanceOverlayConfig? BalanceOverlayCfg { get; set; }
}
