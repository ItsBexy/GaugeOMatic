using CustomNodes;
using GaugeOMatic.Trackers;
using GaugeOMatic.Utility;
using GaugeOMatic.Windows;
using Newtonsoft.Json;
using static FFXIVClientStructs.FFXIV.Component.GUI.AlignmentType;
using static FFXIVClientStructs.FFXIV.Component.GUI.FontType;
using static GaugeOMatic.Widgets.NumTextProps;
using static GaugeOMatic.Widgets.SimpleTimer;
using static GaugeOMatic.Widgets.WidgetTags;
using static GaugeOMatic.Widgets.WidgetUI;
using static GaugeOMatic.Windows.UpdateFlags;
#pragma warning disable CS8618

namespace GaugeOMatic.Widgets;

public sealed class SimpleTimer : GaugeBarWidget
{
    public SimpleTimer(Tracker tracker) : base(tracker) { }

    public override WidgetInfo WidgetInfo => GetWidgetInfo;

    public static WidgetInfo GetWidgetInfo => new()
    {
        DisplayName = "Simple Timer",
        Author = "ItsBexy",
        Description = "It's just timer text. That's it. Nothing else. Hope you like numbers, because you are about to see one on your screen.",
        WidgetTags = GaugeBar
    };

    #region Nodes

    public override CustomNode BuildRoot()
    {
        NumTextNode = new();
        return NumTextNode;
    }

    #endregion

    #region Animations

    #endregion

    #region UpdateFuncs

    public override GaugeBarWidgetConfig GetConfig => Config;

    public override void Update()
    {
        var current = Tracker.CurrentData.GaugeValue;
        var max = Tracker.CurrentData.MaxGauge;
        var prog = current / max;
        var prevProg = prog;

        if (GetConfig.SplitCharges && Tracker.RefType == RefType.Action) AdjustForCharges(ref current, ref max, ref prog, ref prevProg);

        NumTextNode.UpdateValue(current, max);
        Animator.RunTweens();
    }

    #endregion

    #region Configs

    public sealed class SimpleTimerConfig : GaugeBarWidgetConfig
    {
        protected override NumTextProps NumTextDefault => new(enabled: true,
                                                              position: new(0),
                                                              color: new(255, 255, 255),
                                                              edgeColor: new(0, 0, 0), showBg: false, bgColor: new(0),
                                                              font: MiedingerMed,
                                                              fontSize: 20,
                                                              align: Center,
                                                              invert: false);

        public SimpleTimerConfig(WidgetConfig widgetConfig) : base(widgetConfig.SimpleTimerCfg) { }
        public SimpleTimerConfig() { }
    }

    public SimpleTimerConfig Config;

    public override void InitConfigs() => Config = new(Tracker.WidgetConfig);

    public override void ResetConfigs() => Config = new();

    public override void ApplyConfigs() => NumTextNode.ApplyProps(Config.NumTextProps);

    public override void DrawUI(ref WidgetConfig widgetConfig, ref UpdateFlags update)
    {
        var numTextProps = Config.NumTextProps;

        var label = $"{Tracker.TermGauge} Text";
        ImGuiHelpy.TableSeparator(2);

        PositionControls($"Position##{label}Pos", ref numTextProps.Position, ref update);
        ColorPickerRGBA($"Color##{label}color", ref numTextProps.Color, ref update);
        ColorPickerRGBA($"Edge Color##{label}edgeColor", ref numTextProps.EdgeColor, ref update);
        ToggleControls("Backdrop", ref numTextProps.ShowBg, ref update);
        if (numTextProps.ShowBg) ColorPickerRGBA($"Backdrop Color##{label}bgColor", ref numTextProps.BgColor, ref update);

        ComboControls($"Font##{label}font", ref numTextProps.Font, FontList, FontNames, ref update);

        RadioIcons($"Alignment##{label}align", ref numTextProps.Align, AlignList, AlignIcons, ref update);
        IntControls($"Font Size##{label}fontSize", ref numTextProps.FontSize, 1, 100, 1, ref update);

        RadioControls("Precision ", ref numTextProps.Precision, new() { 0, 1, 2 }, new() { "0", "1", "2" }, ref update, true);
        ToggleControls("Invert Value ", ref numTextProps.Invert, ref update);
        ToggleControls("Show Zero ", ref numTextProps.ShowZero, ref update);

        GaugeBarWidgetConfig.SplitChargeControls(ref Config.SplitCharges, Tracker.RefType, Tracker.CurrentData.MaxCount, ref update);

        if (update.HasFlag(Save)) Config.NumTextProps = numTextProps;

        if (update.HasFlag(Save)) ApplyConfigs();
        widgetConfig.SimpleTimerCfg = Config;
    }

    #endregion
}

public partial class WidgetConfig
{
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] public SimpleTimerConfig? SimpleTimerCfg { get; set; }
}
