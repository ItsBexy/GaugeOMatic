using CustomNodes;
using GaugeOMatic.CustomNodes.Animation;
using GaugeOMatic.Trackers;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel;
using static CustomNodes.CustomNode;
using static CustomNodes.CustomNodeManager;
using static GaugeOMatic.CustomNodes.Animation.KeyFrame;
using static GaugeOMatic.CustomNodes.Animation.Tween.EaseType;
using static GaugeOMatic.Utility.Color;
using static GaugeOMatic.Widgets.CounterWidgetConfig.CounterPulse;
using static GaugeOMatic.Widgets.ManaDiamond;
using static GaugeOMatic.Widgets.WidgetTags;
using static GaugeOMatic.Widgets.WidgetUI;
using static GaugeOMatic.Widgets.WidgetUI.WidgetUiTab;

#pragma warning disable CS8618

namespace GaugeOMatic.Widgets;

[WidgetName("Mana Diamonds")]
[WidgetDescription("A recreation of Red Mage's Mana Stack counter.")]
[WidgetAuthor("ItsBexy")]
[WidgetTags(Counter | MultiComponent | Replica)]
[WidgetUiTabs(Layout | Colors | Behavior)]
[MultiCompData("BL", "Balance Gauge Replica", 4)]
public sealed unsafe class ManaDiamond(Tracker tracker) : CounterWidget(tracker)
{
    public override CustomPartsList[] PartsLists { get; } =
    [
        new("ui/uld/JobHudRDM0.tex",
            new(81, 239, 30, 40),
            new(81, 279, 30, 40),
            new(114, 258, 60, 60),
            new(0, 323, 45, 57),
            new(45, 323, 26, 57),
            new(71, 323, 45, 57),
            new(0, 323, 32, 57),
            new(84, 323, 32, 57))
    ];

    #region Nodes

    public CustomNode SocketPlate;
    public CustomNode StackContainer;

    public List<CustomNode> Stacks = [];
    public List<CustomNode> Pulses = [];
    public List<CustomNode> Halos = [];
    public List<CustomNode> Gems = [];
    public List<CustomNode> Glows = [];
    public List<CustomNode> GemContainers = [];

    public override Bounds GetBounds() => WidgetContainer;

    public override CustomNode BuildContainer()
    {
        Max = GetMax();
        SocketPlate = BuildSocketPlate(Max, out var width);
        StackContainer = BuildStacks(Max);

        return new CustomNode(CreateResNode(), SocketPlate, StackContainer).SetOrigin(width/2, 30.5f).SetSize(width,61);
    }

    private CustomNode BuildSocketPlate(int count, out int size)
    {
        if (count == 1)
        {
            size = 64;
            return new(CreateResNode(), ImageNodeFromPart(0, 6).SetPos(0, 0), ImageNodeFromPart(0, 7).SetPos(32, 0));
        }

        var socketNodes = new CustomNode[count];
        var x = 0;
        for (var i = 0; i < count; i++)
        {
            var part = (ushort)(i == 0 ? 3 : i == count - 1 ? 5 : 4);
            socketNodes[i] = ImageNodeFromPart(0, part).SetPos(x, 0);
            x += i == 0 || i == count - 1 ? 45 : 26;
        }
        size = x;
        return new(CreateResNode(), socketNodes);
    }

    private CustomNode BuildStacks(int count)
    {
        Stacks = [];
        Pulses = [];
        Halos = [];
        Gems = [];
        Glows = [];
        GemContainers = [];
        for (var i = 0; i < count; i++)
        {
            Pulses.Add(ImageNodeFromPart(0, 1).SetOrigin(15, 18).SetAlpha(0).Hide().SetPos(17 + (26 * i), 10));
            Halos.Add(ImageNodeFromPart(0, 2).SetPos(-15,-10).SetOrigin(30, 30).SetAlpha(0));
            Gems.Add(ImageNodeFromPart(0, 0).SetOrigin(15, 20));
            Glows.Add(ImageNodeFromPart(0, 1).SetOrigin(15, 18).SetAlpha(0));

            GemContainers.Add(new CustomNode(CreateResNode(), Halos[i], Gems[i], Glows[i]).SetPos(17 + (26 * i), 10).SetOrigin(18, 24));

            Stacks.Add(new(CreateResNode(), Pulses[i], GemContainers[i]));
        }

        return new(CreateResNode(), Stacks.ToArray());
    }

    #endregion

    #region Animations

    public override void ShowStack(int i)
    {
        Pulses[i].Show();

        Animator +=
        [
            new(Halos[i],
                new(0) { Scale = 1, Alpha = 0 },
                new(150) { Scale = 1.2f, Alpha = 200 },
                new(360) { Scale = 0, Alpha = 0 }),

            new(Gems[i],
                new(0) { Scale = 2, Alpha = 0 },
                new(166) { Scale = 1, Alpha = 255 }),

            new(Gems[i],
                new(0) { AddRGB = new(0) },
                new(150) { AddRGB = new(150) },
                new(360) { AddRGB = new(0) })
                { Ease = SinInOut },

            new(Glows[i],
                new(0) { Scale = 0, Alpha = 0 },
                new(160) { Scale = 1.8f, Alpha = 200 },
                new(250) { Scale = 2.2f, Alpha = 0 })
        ];
    }

    public override void HideStack(int i)
    {
        Pulses[i].Hide();

        Animator +=
        [
            new(Gems[i],
                new(0) { Scale = 1, Alpha = 255 },
                new(166) { Scale = 2, Alpha = 0 }),
            new(Glows[i],
                new(0) { Scale = 1.8f, Alpha = 0 },
                new(160) { Scale = 1.8f, Alpha = 200 },
                new(250) { Scale = 2.5f, Alpha = 0 })
        ];
    }

    private void PlateAppear() =>
        Animator += new Tween(WidgetContainer,
                                 new(0) { Scale = Config.Scale * 1.65f, Alpha = 0 },
                                 new(200) { Scale = Config.Scale, Alpha = 255 })
                                 { Ease = SinInOut };

    private void PlateVanish() =>
        Animator += new Tween(WidgetContainer,
                                 new(0) { Scale = Config.Scale, Alpha = 255 },
                                 new(150) { Scale = Config.Scale*0.65f, Alpha = 0 })
                                 { Ease = SinInOut };

    #endregion

    #region UpdateFuncs

    public override void OnFirstRun(int count, int max)
    {
        for (var i = 0; i < max; i++) Gems[i].SetAlpha(i < count);
        if (Config.HideEmpty && count == 0) WidgetContainer.Hide();
    }

    private void PulseAll()
    {
        Animator -= "Pulse";
        foreach (var stack in Stacks)
        {
            Animator +=
            [
                new(stack[0],
                    new(0) { Scale = 0, Alpha = 0 },
                    new(390) { Scale = 0f, Alpha = 0 },
                    new(870) { Scale = 1.4f, Alpha = 152 },
                    new(1290) { Scale = 1.8f, Alpha = 0 })
                { Repeat = true, Ease = SinInOut, Label ="Pulse" },
                new(stack[1],
                    new(0) { AddRGB = new(0) },
                    new(870) { AddRGB = new(150) },
                    new(1290) { AddRGB = new(0) })
                    { Repeat = true, Ease = SinInOut, Label = "Pulse" }
            ];
        }
    }

    private void StopPulseAll()
    {
        Animator -= "Pulse";
        foreach (var stack in Stacks)
        {
            Animator +=
            [
                new(stack[1], new(0, stack[1]), new(150) { AddRGB = 0 }) { Label = "Pulse" },
                new(stack[0], new(0, stack[0]), Hidden[150]) { Label = "Pulse" }
            ];
        }
    }

    public bool Pulsing;
    public bool CheckPulse(int i) => i > 0 && (Config.Pulse == Always || (Config.Pulse == AtMax && i == Stacks.Count));

    public override void PostUpdate(int i)
    {
        var checkPulse = CheckPulse(i);
        if (!Pulsing && checkPulse) PulseAll();
        else if (Pulsing && !checkPulse) StopPulseAll();
        Pulsing = checkPulse;
    }

    public override void OnDecreaseToMin() { if (Config.HideEmpty) PlateVanish(); }
    public override void OnIncreaseFromMin() { if (Config.HideEmpty || WidgetContainer.Alpha < 255) PlateAppear(); }

    #endregion

    #region Configs // todo: add rotation

    public class ManaDiamondConfig : CounterWidgetConfig
    {
        public AddRGB GemColor = new(65, -120, -120);
        public bool HideEmpty;
        [DefaultValue(AtMax)] public CounterPulse Pulse = AtMax;

        public ManaDiamondConfig(WidgetConfig widgetConfig) : base(widgetConfig.ManaDiamondCfg)
        {
            var config = widgetConfig.ManaDiamondCfg;

            if (config == null) return;

            GemColor = config.GemColor;
            HideEmpty = config.HideEmpty;

            Pulse = config.Pulse;
        }

        public ManaDiamondConfig() { }
    }

    private ManaDiamondConfig config;

    public override ManaDiamondConfig Config => config;

    public override void InitConfigs() => config = new(Tracker.WidgetConfig);

    public override void ResetConfigs() => config = new();

    public AddRGB ColorOffset = new(-65, 120, 120);

    public override void ApplyConfigs()
    {
        WidgetContainer.SetPos(Config.Position);
        WidgetContainer.SetScale(Config.Scale);
        StackContainer.SetAddRGB(Config.GemColor + ColorOffset);
    }

    public override void DrawUI()
    {
        base.DrawUI();
        switch (UiTab)
        {
            case Colors:
                ColorPickerRGB("Gem Color", ref Config.GemColor);
                break;
            case Behavior:
                if (ToggleControls("Hide Empty", ref Config.HideEmpty))
                {
                    if (Config.HideEmpty && ((!Config.AsTimer && Tracker.CurrentData.Count == 0) || (Config.AsTimer && Tracker.CurrentData.GaugeValue == 0))) PlateVanish();
                    if (!Config.HideEmpty && WidgetContainer.Alpha < 255) PlateAppear();
                }
                RadioControls("Pulse", ref Config.Pulse, [Never, AtMax, Always], ["Never", "At Maximum", "Always"]);
                break;
            default:
                break;
        }
    }

    #endregion
}

public partial class WidgetConfig
{
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] public ManaDiamondConfig? ManaDiamondCfg { get; set; }
}
