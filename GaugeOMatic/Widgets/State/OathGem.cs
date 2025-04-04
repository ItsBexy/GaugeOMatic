using CustomNodes;
using GaugeOMatic.Trackers;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using static CustomNodes.CustomNodeManager;
using static GaugeOMatic.CustomNodes.Animation.Tween.EaseType;
using static GaugeOMatic.Utility.Color;
using static GaugeOMatic.Widgets.Common.CommonParts;
using static GaugeOMatic.Widgets.OathGem;
using static GaugeOMatic.Widgets.WidgetTags;
using static GaugeOMatic.Widgets.Common.WidgetUI;
using static GaugeOMatic.Widgets.Common.WidgetUI.WidgetUiTab;

#pragma warning disable CS8618

namespace GaugeOMatic.Widgets;

[WidgetName("Oath Gem")]
[WidgetDescription("A widget recreating the low-level tank stance gem for GLA / PLD")]
[WidgetAuthor("ItsBexy")]
[WidgetTags(State | Replica)]
[WidgetUiTabs(Layout | Colors | Behavior)]
public sealed unsafe class OathGem(Tracker tracker) : StateWidget(tracker)
{
    public override CustomPartsList[] PartsLists { get; } = [PLD0];

    #region Nodes

    public CustomNode Frame;
    public CustomNode Gem;
    public CustomNode Glow;

    public override CustomNode BuildContainer()
    {
        Gem = ImageNodeFromPart(0, 20).SetPos(34, 34).SetOrigin(34, 34);
        Frame = ImageNodeFromPart(0, 18).SetOrigin(68, 68);
        Glow = ImageNodeFromPart(0, 21).SetPos(35, 34)
            .SetImageFlag(32).SetScale(1.5f, 1.4f).SetOrigin(34, 34)
                                       .SetAlpha(0);

        return new(CreateResNode(), Gem, Frame, Glow);
    }

    #endregion

    #region Animations

    private void AppearAnim(AddRGB adjustedColor)
    {
        var highlight = adjustedColor + new AddRGB(50, 50, 0);
        Animator +=
        [
            new(Glow,
                new(0) { Alpha = 0, Scale = 1, AddRGB = highlight },
                new(100) { Alpha = 180, Scale = 0.95f, AddRGB = adjustedColor },
                new(200) { Alpha = 0, Scale = 1, AddRGB = highlight })
                { Ease = SinInOut },
            new(Gem,
                new(0) { PartId = 20, AddRGB = 0 },
                new(99) { PartId = 20, AddRGB = 0 },
                new(100) { PartId = 19, AddRGB = adjustedColor })
        ];
    }

    private void HideAnim(AddRGB adjustedColor)
    {
        var highlight = adjustedColor + new AddRGB(50, 50, 0);
        Animator +=
        [
            new(Glow,
                new(0) { Alpha = 0, Scale = 1, AddRGB = highlight },
                new(100) { Alpha = 180, Scale = 0.95f, AddRGB = adjustedColor },
                new(200) { Alpha = 0, Scale = 1, AddRGB = highlight })
                { Ease = SinInOut },
            new(Gem,
                new(0) { PartId = 19, AddRGB = adjustedColor },
                new(99) { PartId = 19, AddRGB = adjustedColor },
                new(100) { PartId = 20, AddRGB = 0 })
                { Ease = SinInOut }
        ];
    }

    private void StateChangeAnim(AddRGB adjustedColor)
    {
        var highlight = adjustedColor + new AddRGB(50, 50, 0);
        Animator +=
        [
            new(Glow,
                new(0) { Alpha = 0, Scale = 1, AddRGB = highlight },
                new(100) { Alpha = 180, Scale = 0.95f, AddRGB = adjustedColor },
                new(200) { Alpha = 0, Scale = 1, AddRGB = highlight })
                { Ease = SinInOut },
            new(Gem,
                new(0, Gem),
                new(200) { AddRGB = adjustedColor })
                { Ease = SinInOut }
        ];
    }

    #endregion

    #region UpdateFuncs

    public override void OnFirstRun(int current) =>
        Gem.SetPartId(current > 0 ? 19 : 20)
           .SetAddRGB(current > 0 ? Config.GetColor(current) + ColorOffset : 0);

    public override void Activate(int current) => AppearAnim(Config.GetColor(current) + ColorOffset);
    public override void Deactivate(int previous) => HideAnim(Config.GetColor(previous) + ColorOffset);
    public override void StateChange(int current, int previous) => StateChangeAnim(Config.GetColor(current) + ColorOffset);

    #endregion

    #region Configs

    public class OathGemConfig : WidgetTypeConfig
    {
        public List<AddRGB> Colors = [];
        public ColorRGB FrameColor = new(100, 100, 100);

        public OathGemConfig(WidgetConfig widgetConfig) : base(widgetConfig.OathGemCfg)
        {
            var config = widgetConfig.OathGemCfg;

            if (config == null) return;

            Colors = config.Colors;
            FrameColor = config.FrameColor;
        }

        public OathGemConfig() { }

        public AddRGB GetColor(int state) => Colors.ElementAtOrDefault(state);

        public void FillColorLists(int max)
        {
            while (Colors.Count <= max)
                Colors.Add(new(53, -11, -54));
        }
    }

    private OathGemConfig config;

    public override OathGemConfig Config => config;

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

    public AddRGB ColorOffset = new(-53, 11, 54);
    public override void ApplyConfigs()
    {
        base.ApplyConfigs();

        var state = Tracker.CurrentData.State;
        if (state > 0)
        {
            Gem.SetAddRGB(Config.GetColor(state) + ColorOffset);
            Glow.SetAddRGB(Config.GetColor(state) + ColorOffset);
        }

        Frame.SetMultiply(Config.FrameColor);
    }

    public override void DrawUI()
    {
        base.DrawUI();
        Config.FillColorLists(Tracker.CurrentData.MaxState);
        switch (UiTab)
        {
            case Colors:
                ColorPickerRGB("Frame Tint", ref Config.FrameColor);

                for (var i = 1; i <= Tracker.CurrentData.MaxState; i++)
                {
                    Heading(Tracker.StateNames[i]);

                    var color = Config.Colors[i];
                    if (ColorPickerRGB($"Gem Color##gemColor{i}", ref color))
                        Config.Colors[i] = color;
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
    public OathGemConfig? OathGemCfg { get; set; }
}
