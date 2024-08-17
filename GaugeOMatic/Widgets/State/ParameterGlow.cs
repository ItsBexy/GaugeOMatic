using CustomNodes;
using GaugeOMatic.CustomNodes.Animation;
using GaugeOMatic.Trackers;
using Newtonsoft.Json;
using System.Numerics;
using static CustomNodes.CustomNode;
using static CustomNodes.CustomNodeManager;
using static GaugeOMatic.CustomNodes.Animation.KeyFrame;
using static GaugeOMatic.Utility.Color;
using static GaugeOMatic.Widgets.ParameterGlow;
using static GaugeOMatic.Widgets.WidgetTags;
using static GaugeOMatic.Widgets.WidgetUI;
using static GaugeOMatic.Widgets.WidgetUI.UpdateFlags;
using static GaugeOMatic.Widgets.WidgetUI.WidgetUiTab;

#pragma warning disable CS8618

namespace GaugeOMatic.Widgets;

public sealed unsafe class ParameterGlow : StateWidget
{
    public ParameterGlow(Tracker tracker) : base(tracker) { }

    public override WidgetInfo WidgetInfo => GetWidgetInfo;

    public static WidgetInfo GetWidgetInfo { get; } = new()
    {
        DisplayName = "Parameter Bar Glow",
        Author = "ItsBexy",
        Description = "A glowing border over one of the parameter bars",
        WidgetTags = HasAddonRestrictions | State,
        AllowedAddons = new () { "_ParameterWidget" },
        UiTabOptions = Layout | Colors
    };

    public override CustomPartsList[] PartsLists { get; } =
    {
        new("ui/uld/Parameter_Gauge.tex", new Vector4(0, 20, 160, 20) )
    };

    #region Nodes

    public CustomNode BarGlow;
    public CustomNode BarGlow2;

    public override CustomNode BuildContainer()
    {
        BarGlow = NineGridFromPart(0, 0, 9, 9, 9, 9).SetNineGridBlend(2);
        BarGlow2 = NineGridFromPart(0, 0, 9, 9, 9, 9).SetNineGridBlend(2).SetOrigin(80, 10);

        Animator += new Tween(BarGlow2,
                              new(0) { Alpha = 0, ScaleX = 1, ScaleY = 1 },
                              new(600) { Alpha = 255, ScaleX = 1.02f, ScaleY = 1.2f },
                              new(1200) { Alpha = 0, ScaleX = 1.05f, ScaleY = 1.6f })
                              { Repeat = true };

        return new CustomNode(CreateResNode(), BarGlow, BarGlow2).SetAlpha(0);
    }

    #endregion

    #region Animations

    #endregion

    #region UpdateFuncs

    public override void PostUpdate() { PlaceOnBar(); }

    public override void OnFirstRun(int current)
    {
        if (Tracker.CurrentData.State > 0) WidgetContainer.SetAlpha(255);
    }
    public override void Activate(int current) => Animator += new Tween(WidgetContainer, Hidden[0], Visible[250]);
    public override void Deactivate(int previous) => Animator += new Tween(WidgetContainer, Visible[0], Hidden[250]);
    public override void StateChange(int current, int previous) { }

    #endregion

    #region Configs

    public class ParameterGlowConfig : WidgetTypeConfig
    {
        public uint Bar;
        public AddRGB Color = new(0);
        public bool PositionFreely; //todo: implement

        public ParameterGlowConfig(WidgetConfig widgetConfig) : base(widgetConfig.ParameterGlowCfg)
        {
            var config = widgetConfig.ParameterGlowCfg;

            if (config == null) return;

            Bar = config.Bar;
            Color = config.Color;
            PositionFreely = config.PositionFreely;
        }

        public ParameterGlowConfig() { }
    }

    public ParameterGlowConfig Config;
    public override WidgetTypeConfig GetConfig => Config;

    public override void InitConfigs() => Config = new(Tracker.WidgetConfig);

    public override void ResetConfigs() => Config = new();

    public override void ApplyConfigs()
    {
        BarGlow.SetAddRGB(Config.Color);
        BarGlow2.SetAddRGB(Config.Color);
    }

    private void PlaceOnBar()
    {
        var barNode = ((AddonIndex)Addon)[Config.Bar == 1u ? 4u : 3u];
        WidgetContainer.SetPos(barNode.X, barNode.Y);
    }

    public override void DrawUI()
    {
        switch (UiTab)
        {
            case Layout:
                RadioControls("Bar", ref Config.Bar, new() { 0, 1 }, new() { "HP", "MP" });
                break;
            case Colors:
                ColorPickerRGB("Color", ref Config.Color);
                break;
            default:
                break;
        }

        if (UpdateFlag.HasFlag(Save))
        {
            ApplyConfigs();
            Config.WriteToTracker(Tracker);
        }
    }

    public override Bounds GetBounds() => BarGlow;

    #endregion
}

public partial class WidgetConfig
{
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] public ParameterGlowConfig? ParameterGlowCfg { get; set; }
}
