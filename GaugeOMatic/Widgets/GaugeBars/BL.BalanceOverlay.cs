using CustomNodes;
using GaugeOMatic.CustomNodes.Animation;
using GaugeOMatic.Trackers;
using Newtonsoft.Json;
using System.Numerics;
using static CustomNodes.CustomNode;
using static CustomNodes.CustomNodeManager;
using static GaugeOMatic.Utility.Color;
using static GaugeOMatic.Utility.MiscMath;
using static GaugeOMatic.Widgets.BalanceOverlay;
using static GaugeOMatic.Widgets.Common.CommonParts;
using static GaugeOMatic.Widgets.GaugeBarWidgetConfig;
using static GaugeOMatic.Widgets.NumTextProps;
using static GaugeOMatic.Widgets.WidgetTags;
using static GaugeOMatic.Widgets.WidgetUI;
using static GaugeOMatic.Widgets.WidgetUI.WidgetUiTab;
using static System.Math;

#pragma warning disable CS8618

namespace GaugeOMatic.Widgets;

[WidgetName("Balance Gauge Overlay")]
[WidgetDescription("A glowing gauge bar fitted over the Balance Gauge (or a replica of it).")]
[WidgetAuthor("ItsBexy")]
[WidgetTags(GaugeBar | MultiComponent | HasAddonRestrictions | HasClippingMask)]
[AddonRestrictions(false, "JobHudRPM1", "JobHudGFF1", "JobHudSMN1", "JobHudBRD0")]
[MultiCompData("BL", "Balance Gauge Replica", 3)]
public sealed unsafe class BalanceOverlay : GaugeBarWidget
{
    public BalanceOverlay(Tracker tracker) : base(tracker) { }

    public override CustomPartsList[] PartsLists { get; } = {
        new("ui/uld/JobHudRDM0.tex",
            new(0, 0, 116, 208),
            new(186, 3, 26, 124),
            new(212, 3, 26, 124),
            new(116, 0, 34, 144),
            new(0, 208, 40, 56),
            new(40, 208, 40, 56),
            new(116, 144, 40, 60),
            new(184, 132, 84, 188),
            new(125, 212, 24, 22),
            new(123, 234, 28, 20),
            new(148, 222, 14, 15),
            new(242, 3, 26, 124),
            new(116, 332, 72, 32),
            new(0, 264, 40, 48),
            new(81, 239, 30, 40),
            new(81, 279, 30, 40),
            new(0, 319, 117, 61),
            new(114, 258, 60, 60),
            new(118, 321, 89, 59),
            new(207, 321, 39, 59),
            new(150, 0, 34, 144)),
        new ("ui/uld/JobHudNIN0.tex", new Vector4[] {
            new(256, 152, 20, 88) // flashing edge
        }),
        CircleMask
    };

    #region Nodes

    public CustomNode Plate;
    public CustomNode CrystalGlow;
    public CustomNode Tick;
    public CustomNode PlateMask;

    public override Bounds GetBounds() => Plate;

    public override CustomNode BuildContainer()
    {
        Plate = ImageNodeFromPart(0, 0).SetImageWrap(2).SetImageFlag(32);
        PlateMask = ClippingMaskFromPart(2, 1).SetSize(116,416).SetPos(0,-208).DefineTimeline(MaskTimeline);

        Main = new CustomNode(CreateResNode(), Plate, PlateMask).SetSize(116, 0)
                                                                .DefineTimeline(ContainerTimeline)
                                                                .SetAlpha(0);

        CrystalGlow = ImageNodeFromPart(0, 17).SetPos(28, 3).SetOrigin(30, 30).SetAlpha(0).SetImageFlag(32);

        Tick = ImageNodeFromPart(1, 0).SetRotation(1.5707963267949f)
                                      .SetImageFlag(33)
                                      .SetOrigin(20, 44)
                                      .SetPos(37.5f,-30)
                                      .DefineTimeline(TickTimeline)
                                      .SetAlpha(0);
        NumTextNode = new();

        return new(CreateResNode(), Main, CrystalGlow, Tick, NumTextNode);
    }

    #endregion

    #region Animations

    public static KeyFrame[] ContainerTimeline => new KeyFrame[]
    {
        new(0) { Alpha = 0 },
        new(20) { Alpha = 255 },
        new(192) { Alpha = 255 }
    };

    public static KeyFrame[] MaskTimeline => new KeyFrame[]
    {
        new(0) { Y=-6 },
        new(20) {Y=-26 },
        new(192) {Y=-208 }
    };

    public KeyFrame[] TickTimeline => new KeyFrame[]
    {
        new(0) { Y = 173, Alpha = 0 },
        new(20) { Y = 153, Alpha = Config.TickColor.A },
        new(182) { Y = -9, Alpha = Config.TickColor.A },
        new(192) { Y = -19, Alpha = 0 }
    };

    #endregion

    #region UpdateFuncs

    public override void OnFirstRun(float prog)
    {
        Main.SetProgress(prog);
        Tick.SetProgress(prog);
        PlateMask.SetProgress(prog);
    }

    public override void PlaceTickMark(float prog)
    {
        PlateMask.SetProgress(Main);
        var yFactor = PlateMask.Y+216;

        Tick.SetProgress(Main)
            .SetScaleY(Clamp(yFactor switch
            {
                <= 80 => PolyCalc(yFactor, -0.369408145220107d, 0.0761410396507537d, -0.00102457794640973d, 0.0000049971702224042d),
                <= 163.5f => PolyCalc(yFactor, 2.9911068686918d, -0.00223868498009971d, -0.000303975874680723d, 0.00000164245988203053d),
                _ => PolyCalc(yFactor, -88.9665931925754d, 1.53310683483466d, -0.00854319135223209d, 0.0000156241038226192d)
            }, 0, 2f));
    }

    public override void OnIncrease(float prog, float prevProg)
    {
        Animator += new Tween[]{
            new(CrystalGlow,
                              new(0) { Rotation = 0 },
                              new(400) { Rotation = (float)PI*1 }),
            new(CrystalGlow,
                new(0) { Alpha = 0, Scale = 0.8f },
                new(150) { Alpha = 255, Scale = 1.5f },
                new(400) { Alpha = 0, Scale = 3 })
        };

    }

    public override string SharedEventGroup => "BalanceGauge";

    #endregion

    #region Configs

    public sealed class BalanceOverlayConfig : GaugeBarWidgetConfig
    {
        public AddRGB Color = "0xAF4A5A6B";
        public AddRGB TickColor = "0xD9462BD3";

        public BalanceOverlayConfig(WidgetConfig widgetConfig) : base(widgetConfig.BalanceOverlayCfg)
        {
            var config = widgetConfig.BalanceOverlayCfg;

            if (config == null) return;

            Color = config.Color;
            TickColor = config.TickColor;
        }

        public BalanceOverlayConfig() { }
    }

    private BalanceOverlayConfig config;

    public override BalanceOverlayConfig Config => config;

    public override void InitConfigs() => config = new(Tracker.WidgetConfig);

    public override void ResetConfigs() => config = new();

    public override void ApplyConfigs()
    {
        WidgetContainer.SetPos(Config.Position).SetScale(Config.Scale);
        Plate.SetAddRGB(Config.Color, true);
        Tick.SetAddRGB(Config.TickColor)
            .DefineTimeline(TickTimeline);
        CrystalGlow.SetAddRGB(Config.Color + new AddRGB(-95, 106, 74));

        NumTextNode.ApplyProps(Config.NumTextProps);
    }

    public override void DrawUI()
    {
        base.DrawUI();
        switch (UiTab)
        {
            case Colors:
                ColorPickerRGBA("Color", ref Config.Color);
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
    }

    #endregion
}

public partial class WidgetConfig
{
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] public BalanceOverlayConfig? BalanceOverlayCfg { get; set; }
}
