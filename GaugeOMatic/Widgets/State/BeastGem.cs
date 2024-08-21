using CustomNodes;
using GaugeOMatic.Trackers;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using static CustomNodes.CustomNodeManager;
using static GaugeOMatic.CustomNodes.Animation.Tween.EaseType;
using static GaugeOMatic.Utility.Color;
using static GaugeOMatic.Widgets.BeastGem;
using static GaugeOMatic.Widgets.Common.CommonParts;
using static GaugeOMatic.Widgets.WidgetTags;
using static GaugeOMatic.Widgets.WidgetUI;
using static GaugeOMatic.Widgets.WidgetUI.WidgetUiTab;

#pragma warning disable CS8618

namespace GaugeOMatic.Widgets;

[WidgetName("Beast Gem")]
[WidgetDescription("A widget recreating the low-level tank stance gem for MRD / WAR.")]
[WidgetAuthor("ItsBexy")]
[WidgetTags(State | Replica)]
[WidgetUiTabs(Layout | Colors)]
public sealed unsafe class BeastGem(Tracker tracker) : StateWidget(tracker)
{
    public override CustomPartsList[] PartsLists { get; } = [WAR0];

    #region Nodes

    public CustomNode Base;
    public CustomNode Gem;
    public CustomNode Glow;

    public override CustomNode BuildContainer()
    {
        Base = ImageNodeFromPart(0, 8).SetOrigin(54, 54);
        Gem = ImageNodeFromPart(0, 9).SetPos(21, 20).SetOrigin(32, 32).SetAlpha(0);
        Glow = ImageNodeFromPart(0, 10).SetPos(27, 24).SetImageFlag(32).SetScale(1.5f, 1.4f).SetOrigin(27, 27).SetAlpha(0);

        return new(CreateResNode(), Base, Gem, Glow);
    }

    #endregion

    #region UpdateFuncs

    public override void OnFirstRun(int current)
    {
        Gem.SetAlpha(current > 0);
    }

    public override void Activate(int current)
    {
        var color = Config.GetColor(current) + new AddRGB(21, 103, 103);
        Animator +=
        [
            new(Glow,
                new(0) { Alpha = 0, AddRGB = color, MultRGB = new ColorRGB(100) },
                new(100) { Alpha = 201, AddRGB = color, MultRGB = new ColorRGB(100) },
                new(200) { Alpha = 0, AddRGB = color, MultRGB = new ColorRGB(100) })
                { Ease = SinInOut },
            new(Gem,
                new(0) { Alpha = 0 },
                new(150) { Alpha = 255 })
                { Ease = SinInOut }
        ];
    }

    public override void Deactivate(int previous) =>
        Animator +=
        [
            new(Glow,
                new(0) { Alpha = 0, MultRGB = new ColorRGB(100) },
                new(100) { Alpha = 241, MultRGB = new ColorRGB(50) },
                new(200) { Alpha = 0, MultRGB = new ColorRGB(50) })
                { Ease = SinInOut },
            new(Gem,
                new(0) { Alpha = 255 },
                new(150) { Alpha = 0 })
                { Ease = SinInOut }
        ];

    public override void StateChange(int current, int previous)
    {
        var color = Config.GetColor(current) + new AddRGB(21, 103, 103);
        Animator +=
        [
            new(Glow,
                new(0) { Alpha = 0, AddRGB = color, MultRGB = new ColorRGB(100) },
                new(100) { Alpha = 201, AddRGB = color, MultRGB = new ColorRGB(100) },
                new(200) { Alpha = 0, AddRGB = color, MultRGB = new ColorRGB(100) })
                { Ease = SinInOut },
            new(Gem,
                new(0, Gem),
                new(200) { AddRGB = color })
                { Ease = SinInOut }
        ];
    }

    #endregion

    #region Configs

    public class BeastGemConfig : WidgetTypeConfig
    {
        public List<AddRGB> Colors = [];
        public ColorRGB BaseColor = new(100, 100, 100);

        public BeastGemConfig(WidgetConfig widgetConfig) : base(widgetConfig.BeastGemCfg)
        {
            var config = widgetConfig.BeastGemCfg;

            if (config == null)
                return;

            Colors = config.Colors;
            BaseColor = config.BaseColor;
        }

        public BeastGemConfig() { }

        public AddRGB GetColor(int state) => Colors.ElementAtOrDefault(state);

        public void FillColorLists(int max)
        {
            while (Colors.Count <= max)
                Colors.Add(new(-21, -103, -103));
        }
    }

    private BeastGemConfig config;

    public override BeastGemConfig Config => config;

    public override void InitConfigs()
    {
        config = new(Tracker.WidgetConfig);
        Config.FillColorLists(Tracker.CurrentData.MaxState);
    }

    public override void ResetConfigs()
    {
        config = new();
        Config.FillColorLists(Tracker.CurrentData.MaxState);
    }

    public override void ApplyConfigs()
    {
        WidgetContainer.SetPos(Config.Position);
        WidgetContainer.SetScale(Config.Scale);

        var state = Tracker.CurrentData.State;
        Gem.SetAddRGB(Config.GetColor(state) + new AddRGB(21, 103, 103));
        Glow.SetAddRGB(Config.GetColor(state) + new AddRGB(21, 103, 103));
        Base.SetMultiply(Config.BaseColor);
    }

    public override void DrawUI()
    {
        base.DrawUI();
        Config.FillColorLists(Tracker.CurrentData.MaxState);
        switch (UiTab)
        {
            case Colors:
                ColorPickerRGB("Base Tint", ref Config.BaseColor);

                for (var i = 1; i <= Tracker.CurrentData.MaxState; i++)
                {
                    var color = Config.Colors[i];
                    if (ColorPickerRGB($"{Tracker.StateNames[i]}", ref color)) Config.Colors[i] = color;
                }
                break;
            default:
                break;
        }
    }

    #endregion
}

public partial class WidgetConfig
{
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public BeastGemConfig? BeastGemCfg { get; set; }
}
