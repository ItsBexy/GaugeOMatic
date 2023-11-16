using GaugeOMatic.Trackers;
using GaugeOMatic.Utility;
using GaugeOMatic.Windows;
using Newtonsoft.Json;
using static CustomNodes.CustomNodeManager;
using static FFXIVClientStructs.FFXIV.Component.GUI.AlignmentType;
using static FFXIVClientStructs.FFXIV.Component.GUI.FontType;
using static GaugeOMatic.Widgets.NumTextProps;
using static GaugeOMatic.Widgets.SimpleTimer;
using static GaugeOMatic.Widgets.WidgetTags;
using static GaugeOMatic.Widgets.WidgetUI;
using static GaugeOMatic.Windows.UpdateFlags;

namespace GaugeOMatic.Widgets;

public sealed unsafe class SimpleTimer : Widget
{
    public override WidgetInfo WidgetInfo => GetWidgetInfo;

    public static WidgetInfo GetWidgetInfo => new()
    {
        DisplayName = "Simple Timer",
        Author = "ItsBexy",
        Description = "It's just timer text. That's it. Nothing else. Hope you like numbers, because you are about to see one on your screen.",
        WidgetTags = GaugeBar
    };

    #region Nodes

    public override CustomNode BuildRoot() => new(CreateNumTextNode());

    #endregion

    #region Animations

    #endregion

    #region UpdateFuncs

    public override string? SharedEventGroup => null;

    public override void Update()
    {
        WidgetRoot.UpdateNumText(Config.NumTextProps, Tracker.CurrentData.GaugeValue, Tracker.CurrentData.MaxGauge);
        RunTweens();
    }

    #endregion

    #region Configs

    public class SimpleTimerConfig
    {
        public NumTextProps NumTextProps = new(true, 
                                               new(0), 
                                               new(255, 255, 255), 
                                               new(0, 0, 0), 
                                               MiedingerMed,
                                               20,
                                               Center,
                                               false);

        public SimpleTimerConfig(WidgetConfig widgetConfig)
        {
            if (widgetConfig.SimpleTimerCfg?.NumTextProps != null) NumTextProps = widgetConfig.SimpleTimerCfg.NumTextProps;
        }

        public SimpleTimerConfig() { }
    }

    public SimpleTimerConfig Config = null!;

    public override void InitConfigs() => Config = new(Tracker.WidgetConfig);

    public override void ResetConfigs() => Config = new();

    public override void ApplyConfigs() => Config.NumTextProps.ApplyTo(WidgetRoot);

    public override void DrawUI(ref WidgetConfig widgetConfig, ref UpdateFlags update)
    {
        var numTextProps = Config.NumTextProps;

        var label = $"{Tracker.TermGauge} Text";
        ImGuiHelpers.TableSeparator(2);

        PositionControls($"Position##{label}Pos",ref numTextProps.Position, ref update);
        ColorPickerRGBA($"Color##{label}color", ref numTextProps.Color, ref update);
        ColorPickerRGBA($"Edge Color##{label}edgeColor", ref numTextProps.EdgeColor, ref update);

        ComboControls($"Font##{label}font", ref numTextProps.Font, FontList, FontNames, ref update);

        RadioIcons($"Alignment##{label}align", ref numTextProps.Align, AlignList, AlignIcons, ref update);
        IntControls($"Font Size##{label}fontSize", ref numTextProps.FontSize, 1, 100, 1, ref update);

        RadioControls("Precision ", ref numTextProps.Precision, new() { 0, 1, 2 }, new() { "0", "1", "2" }, ref update,true);
        ToggleControls("Invert Value ", ref numTextProps.Invert, ref update);
        ToggleControls("Show Zero ", ref numTextProps.ShowZero, ref update);
        
        if (update.HasFlag(Save)) Config.NumTextProps = numTextProps;

        if (update.HasFlag(Save)) ApplyConfigs();
        widgetConfig.SimpleTimerCfg = Config;
    }

    #endregion

    public SimpleTimer(Tracker tracker) : base(tracker) { }
}

public partial class WidgetConfig
{
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] public SimpleTimerConfig? SimpleTimerCfg { get; set; }
}
