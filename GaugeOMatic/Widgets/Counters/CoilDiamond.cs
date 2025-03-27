using CustomNodes;
using GaugeOMatic.CustomNodes.Animation;
using GaugeOMatic.Trackers;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel;
using static CustomNodes.CustomNode.CustomNodeFlags;
using static CustomNodes.CustomNodeManager;
using static GaugeOMatic.CustomNodes.Animation.Tween.EaseType;
using static GaugeOMatic.Utility.Color;
using static GaugeOMatic.Utility.MiscMath;
using static GaugeOMatic.Widgets.CoilDiamond;
using static GaugeOMatic.Widgets.Common.CommonParts;
using static GaugeOMatic.Widgets.CounterWidgetConfig.CounterPulse;
using static GaugeOMatic.Widgets.WidgetTags;
using static GaugeOMatic.Widgets.Common.WidgetUI;
using static GaugeOMatic.Widgets.Common.WidgetUI.WidgetUiTab;
using static System.Math;

#pragma warning disable CS8618

namespace GaugeOMatic.Widgets;

[WidgetName("Coil Diamonds")]
[WidgetDescription("A diamond-shaped counter based on VPR's Rattling Coil stacks.")]
[WidgetAuthor("ItsBexy")]
[WidgetTags(Counter | Replica)]
[WidgetUiTabs(Layout | Colors | Behavior | Icon | Sound)]
public sealed unsafe class CoilDiamond(Tracker tracker) : FreeGemCounter(tracker)
{
    public override CustomPartsList[] PartsLists { get; } = [VPR0];

    #region Nodes

    public List<CustomNode> Stacks2;
    public List<CustomNode> Frames;
    public List<CustomNode> Gems;
    public List<CustomNode> Flashes;
    public List<CustomNode> Pulsars;
    public List<CustomNode> Glows; // within main stack

    public List<CustomNode> Glows2; // above main stack

    public override CustomNode BuildContainer()
    {
        Max = GetMax();
        BuildStacks(Max);

        return new(CreateResNode(), Stacks.ToArray());
    }

    private void BuildStacks(int count)
    {
        Stacks2 = [];
        Stacks = [];
        Frames = [];
        Gems = [];
        Flashes = [];
        Pulsars = [];

        Glows = [];
        Glows2 = [];

        for (var i = 0; i < count; i++)
        {
            Frames.Add(ImageNodeFromPart(0, 5));
            Gems.Add(ImageNodeFromPart(0, 6).SetPos(0, -1).SetOrigin(15, 23).SetAlpha(0));
            Flashes.Add(ImageNodeFromPart(0, 8).SetPos(-8, 0).SetSize(46, 46).SetOrigin(23, 23).SetAddRGB(Config.GemColor).SetAlpha(0).SetImageFlag(0x20));
            Pulsars.Add(ImageNodeFromPart(0, 10).SetPos(-14, 16).SetSize(58, 12).SetOrigin(29, 6).SetAddRGB(Config.GemColor).SetAlpha(0).SetImageFlag(0x20));
            Glows.Add(ImageNodeFromPart(0, 7).SetPos(0, -1).SetSize(30, 46).SetOrigin(15, 23).SetAddRGB(Config.GemColor).SetAlpha(0).SetImageFlag(0x20));


            Stacks2.Add(new CustomNode(CreateResNode(), Frames[i], Gems[i], Flashes[i], Pulsars[i], Glows[i]).SetSize(30, 46));

            Glows2.Add(ImageNodeFromPart(0, 7).SetPos(0, -1)
                                             .SetScale(1.3721743f)
                                             .SetOrigin(15, 23)
                                             .SetImageFlag(0x20)
                                             .SetAlpha(0)
                                             .RemoveFlags(SetVisByAlpha)
                                             .SetAddRGB(255, -200, -200));

            Stacks.Add(new CustomNode(CreateResNode(), Stacks2[i], Glows2[i]).SetOrigin(15, 22).SetSize(30, 44));
        }
    }

    #endregion

    #region Animations

    public override void ShowStack(int i)
    {
        var gemColor = Config.GemColor + GemColorOffset;

        var midAppear = new AddRGB(50);

        Animator -= Gems[i];
        Animator +=
        [
            new(Gems[i],
                 new(0)   { Alpha = 0, AddRGB = gemColor },
                 new(165) { Alpha = 255, AddRGB = gemColor + midAppear },
                 new(330) { Alpha = 255, AddRGB = gemColor }),
             new(Flashes[i],
                 new(0)   { Alpha = 255, Rotation = 0 },
                 new(280) { Alpha = 0, Rotation = (float)(PI*2)}),
             new(Pulsars[i],
                 new(0)   { Alpha = 255, ScaleX = 0.5f, ScaleY = 1 },
                 new(280) { Alpha = 0, Scale = 1 }),
             new(Glows[i],
                 new(0)   { Alpha = 255, Scale = 1.5f },
                 new(180) { Alpha = 255, Scale = 1 },
                 new(330) { Alpha = 0, Scale = 1})
        ];

        Glows2[i].Show();
    }

    public override void HideStack(int i)
    {
        Animator -= Gems[i];
        Animator +=
        [
            new(Gems[i],
                new(0) { Alpha = 255 },
                new(165) { Alpha = 0 }),
            new(Glows[i],
                new(0)   { Alpha = 0, Scale = 1 },
                new(165) { Alpha = 255, Scale = 1 },
                new(330) { Alpha = 0, Scale = 2})
        ];

        Glows2[i].Hide();
    }

    private void AllVanish() =>
        Animator += new Tween(WidgetContainer,
                              new(0) { Alpha = 255, AddRGB = 0 },
                              new(200) { Alpha = 0, AddRGB = 50 });

    private void AllAppear() =>
        Animator += new Tween(WidgetContainer,
                              new(0) { Alpha = 0, AddRGB = 50 },
                              new(200) { Alpha = 255, AddRGB = 0 });

    private void PulseAll()
    {
        Animator -= "Pulse";
        for (var i = 0; i < Stacks.Count; i++)
        {
            Animator +=
            [
                new(Glows2[i],
                    new(0) { Scale = 1, Alpha = 0 },
                    new(250) { Scale = 1.25f, Alpha = 255 },
                    new(1300) { Scale = 1.5f, Alpha = 0 })
                    { Repeat = true, Ease = Linear, Label = "Pulse" }
            ];
        }
    }

    private void StopPulseAll()
    {
        Animator -= "Pulse";
        for (var i = 0; i < Stacks.Count; i++)
        {
            Animator +=
            [
                new(Glows2[i],
                     new(0, Glows2[i]),
                     new(200) { Scale = 1.5f, Alpha = 0 })
                     { Label = "Pulse" }
            ];
        }
    }

    #endregion

    #region UpdateFuncs

    public override void OnFirstRun(int count, int max)
    {
        for (var i = 0; i < max; i++)
        {
            Gems[i].SetAlpha(i < count);
            Glows2[i].SetVis(i < count);
        }
        if (Config.HideEmpty && count == 0) WidgetContainer.Hide();
    }

    public override void OnDecreaseToMin() { if (Config.HideEmpty) AllVanish(); }
    public override void OnIncreaseFromMin() { if (Config.HideEmpty || WidgetContainer.Alpha < 255) AllAppear(); }

    public bool Pulsing;
    public bool CheckPulse(int i) => i > 0 && (Config.Pulse == Always || (Config.Pulse == AtMax && i == Stacks.Count));

    public override void PostUpdate(int i)
    {
        var checkPulse = CheckPulse(i);
        if (!Pulsing && checkPulse) PulseAll();
        else if (Pulsing && !checkPulse) StopPulseAll();
        Pulsing = checkPulse;
    }

    #endregion

    #region Configs

    public class CoilDiamondConfig : FreeGemCounterConfig
    {
        public AddRGB GemColor = new(61, -92, -95);
        public AddRGB GlowColor = new(255, -200, -200);
        public ColorRGB FrameColor = new(100);
        public bool HideEmpty;
        [DefaultValue(AtMax)] public CounterPulse Pulse = AtMax;

        public CoilDiamondConfig(WidgetConfig widgetConfig) : base(widgetConfig.CoilDiamondCfg)
        {
            var config = widgetConfig.CoilDiamondCfg;

            if (config == null) return;

            GemColor = config.GemColor;
            GlowColor = config.GlowColor;
            FrameColor = config.FrameColor;
            HideEmpty = config.HideEmpty;

            Pulse = config.Pulse;
        }

        [JsonIgnore]
        public override float DefaultSpacing => 23;

        public CoilDiamondConfig() { }
    }

    private CoilDiamondConfig config;

    public override CoilDiamondConfig Config => config;

    public AddRGB GemColorOffset = new(-61, 92, 95);

    public override void InitConfigs() => config = new(Tracker.WidgetConfig);

    public override void ResetConfigs() => config = new();

    public override void ApplyConfigs()
    {
        base.ApplyConfigs();

        PlaceFreeGems();

        for (var i = 0; i < Stacks.Count; i++)
        {
            var combinedAngle = Degrees(Stacks[i].Rotation + WidgetContainer.Rotation);

            float scaleX = combinedAngle is <= -53 or >= 128 ? -1 : 1;
            float scaleY = combinedAngle is <= -128 or >= 53 ? -1 : 1;

            Gems[i].SetScale(scaleX, scaleY).SetAddRGB(GemColorOffset + Config.GemColor);
            Frames[i].SetMultiply(Config.FrameColor);
            Glows[i].SetAddRGB(Config.GlowColor);
            Glows2[i].SetAddRGB(Config.GlowColor);
            Flashes[i].SetAddRGB(Config.GlowColor);
            Pulsars[i].SetAddRGB(Config.GlowColor);
        }
    }

    public override void DrawUI()
    {
        base.DrawUI();
        switch (UiTab)
        {
            case Colors:
                ColorPickerRGB("Gem Color", ref Config.GemColor);
                ColorPickerRGB("Glow Color", ref Config.GlowColor);
                ColorPickerRGB("Frame Tint", ref Config.FrameColor);
                break;
            case Behavior:
                if (ToggleControls("Hide Empty", ref Config.HideEmpty))
                {
                    if (Config.HideEmpty && ((!Config.AsTimer && Tracker.CurrentData.Count == 0) || (Config.AsTimer && Tracker.CurrentData.GaugeValue == 0))) AllVanish();
                    if (!Config.HideEmpty && WidgetContainer.Alpha < 255) AllAppear();
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
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] public CoilDiamondConfig? CoilDiamondCfg { get; set; }
}
