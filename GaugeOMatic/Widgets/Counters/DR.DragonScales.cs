using CustomNodes;
using GaugeOMatic.CustomNodes.Animation;
using GaugeOMatic.Trackers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using static CustomNodes.CustomNode;
using static CustomNodes.CustomNode.CustomNodeFlags;
using static CustomNodes.CustomNodeManager;
using static GaugeOMatic.CustomNodes.Animation.Tween.EaseType;
using static GaugeOMatic.Utility.Color;
using static GaugeOMatic.Widgets.Common.CommonParts;
using static GaugeOMatic.Widgets.CounterWidgetConfig.CounterPulse;
using static GaugeOMatic.Widgets.DragonScales;
using static GaugeOMatic.Widgets.WidgetTags;
using static GaugeOMatic.Widgets.WidgetUI;
using static GaugeOMatic.Widgets.WidgetUI.WidgetUiTab;

#pragma warning disable CS8618

namespace GaugeOMatic.Widgets;

[WidgetName("Dragon Scales")]
[WidgetDescription("A stack counter recreating DRG's First Minds' Focus counter.")]
[WidgetAuthor("ItsBexy")]
[WidgetTags(Counter | Replica | MultiComponent)]
[WidgetUiTabs(Layout | Colors | Behavior)]
[MultiCompData("DR", "Replica Dragon Gauge", 1)]
public sealed unsafe class DragonScales(Tracker tracker) : CounterWidget(tracker)
{
    public override CustomPartsList[] PartsLists { get; } = [DRG0];

    #region Nodes

    public CustomNode Frame;
    public CustomNode StackContainer;
    public List<CustomNode> Stacks = [];
    public List<CustomNode> Scales = [];
    public List<CustomNode> Glows = [];
    public List<CustomNode> Glows2 = [];
    public List<CustomNode> Glows3 = [];
    public List<CustomNode> Pierces = [];

    public override Bounds GetBounds() => WidgetContainer;

    public override CustomNode BuildContainer()
    {
        Max = GetMax();
        Frame = BuildFrame(Max, out var width);
        StackContainer = BuildStacks(Max);

        return new CustomNode(CreateResNode(), Frame, StackContainer).SetOrigin(width / 2f, 21).SetSize(width, 42);
    }


    public CustomNode BuildFrame(int count, out int width)
    {
        var frameNodes = new List<CustomNode> { ImageNodeFromPart(0, 23) };

        width = 21;
        for (var i = 0; i < count - 1; i++)
        {
            frameNodes.Add(ImageNodeFromPart(0, 24).SetX(width));
            width += 24;
        }
        frameNodes.Add(ImageNodeFromPart(0, 25).SetX(width));
        width += 25;

        return new(CreateResNode(), frameNodes.ToArray());
    }

    public CustomNode BuildStacks(int count)
    {
        Stacks = [];
        Scales = [];
        Glows = [];
        Glows2 = [];
        Glows3 = [];
        Pierces = [];

        var x = 8;
        for (var i = 0; i < count; i++)
        {
            Scales.Add(ImageNodeFromPart(0, (ushort)(i == 0 ? 26 : 27)).SetPos(x, 6).SetAlpha(0));

            var partId = (ushort)(i == 0 ? 28 : 29);
            Glows.Add(CreateGlowNode(x, partId));
            Glows2.Add(CreateGlowNode(x, partId).SetVis(false));
            Glows3.Add(CreateGlowNode(x, partId).SetVis(false));
            Pierces.Add(ImageNodeFromPart(0, 14).SetPos(x - 12, 0)
                                               .SetScale(0.6f, 0.8f)
                                               .SetOrigin(28, 34)
                                               .SetAddRGB(20, -100, 100)
                                               .SetImageWrap(2)
                                               .SetImageFlag(32)
                                               .SetAlpha(0));

            Animator +=
            [
                new(Glows2[i],
                    new(0) { Alpha = 0 },
                    new(675) { Alpha = 34 },
                    new(1380) { Alpha = 0 })
                    { Label = $"Pulse{i}", Ease = SinInOut, Repeat = true },
                new(Glows3[i],
                    new(0) { Alpha = 29 },
                    new(250) { Alpha = 100 },
                    new(800) { Alpha = 0 })
                    { Label = $"Pulse{i}", Ease = SinInOut, Repeat=true }
            ];

            x += i == 0 ? 28 : 24;

            Stacks.Add(new(CreateResNode(), Scales[i], Glows[i], Glows2[i], Glows3[i], Pierces[i]));
        }

        return new(CreateResNode(), Stacks.ToArray());
    }

    private CustomNode CreateGlowNode(int x, ushort part) =>
        ImageNodeFromPart(0, part).SetPos(x - 10, 0)
                                  .SetImageFlag(32)
                                  .SetOrigin(16, 21)
                                  .SetAddRGB(Config.ScaleColor.Transform(-160, 200))
                                  .RemoveFlags(SetVisByAlpha)
                                  .SetAlpha(0);

    #endregion

    #region Animations

    public override void ShowStack(int i)
    {
        Animator -= $"Hide{i}";

        Scales[i].SetAlpha(255);

        var glowX = (i == 0 ? 5 : 36 + ((i - 1) * 24)) - 4;

        var glowColor2 = Config.ScaleColor.Transform(-100, 50);
        var glowColor1 = Config.ScaleColor.Transform(-100, 40);

        Animator +=
        [
            new(Glows[i],
                new(0) { X = glowX + 48, ScaleX = 1, ScaleY = 0, Alpha = 255, AddRGB = glowColor1, Y = 0 },
                new(100) { X = glowX, Scale = 1, Alpha = 128, AddRGB = glowColor2, Y = 0 },
                new(225) { X = glowX, Scale = 2, Alpha = 0, AddRGB = glowColor2, Y = 0 },
                new(275) { X = glowX, Scale = 1, AddRGB = glowColor2, Y = 0 })
                {Label = $"Appear{i}", Complete = () => Glows2[i].SetVis(true)}
        ];
    }

    public override void HideStack(int i)
    {
        Animator -= $"Appear{i}";

        var glowColor1 = Config.ScaleColor.Transform(-140, 200);
        var glowColor2 = Config.ScaleColor.Transform(-200, 255);

        Animator +=
        [
            new(Scales[i],
                new(0) { Alpha = 255 },
                new(300) { Alpha = 255 },
                new(301) { Alpha = 0 })
                {Label = $"Hide{i}"},

            new(Glows[i], new(0) { Scale = 1, Alpha = 0, AddRGB = glowColor1, Y = 0 },
                new(300) { Scale = 2, Alpha = 150, AddRGB = glowColor2, Y = 0 },
                new(450) { ScaleX = 0.4f, ScaleY = 1.4f, Alpha = 0, AddRGB = glowColor1, Y = -48 })
                {Label = $"Hide{i}"},

            new(Pierces[i],
                new(0) { Y = -10, ScaleX = 1.4f, ScaleY = 1, Alpha = 0 },
                new(180) { Y = -10, ScaleX = 1.4f, ScaleY = 1, Alpha = 0 },
                new(285) { Y = -25, ScaleX = 1.25f, ScaleY = 1, Alpha = 152 },
                new(530) { Y = -60, ScaleX = 0.6f, ScaleY = 0.8f, Alpha = 0 })
        ];

        Glows2[i].SetVis(false);
        Glows3[i].SetVis(false);
    }

    private void FrameAppear() =>
        Animator += new Tween(WidgetContainer,
                              new(0) { Alpha = 0, Scale = Config.Scale * 0.75f },
                              new(150) { Alpha = 255, Scale = Config.Scale })
        { Ease = SinInOut };

    private void FrameVanish() =>
        Animator += new Tween(WidgetContainer,
                              new(0) { Alpha = 255, Scale = Config.Scale },
                              new(50) { Alpha = 200, Scale = Config.Scale * 1.05f },
                              new(200) { Alpha = 0, Scale = Config.Scale * 0.75f })
        { Ease = SinInOut };

    #endregion

    #region UpdateFuncs

    public override void OnDecreaseToMin() { if (Config.HideEmpty) FrameVanish(); }
    public override void OnIncreaseFromMin() { if (Config.HideEmpty || WidgetContainer.Alpha < 255) FrameAppear(); }

    public override void OnFirstRun(int count, int max)
    {
        for (var i = 0; i < count; i++)
        {
            Scales[i].SetAlpha(255);
            Glows2[i].SetVis(true);
        }
    }

    public bool Pulsing;
    public bool CheckPulse(int i) => i > 0 && (Config.Pulse == Always || (Config.Pulse == AtMax && i == Stacks.Count));

    public override void PostUpdate(int i)
    {
        if (Tracker.CurrentData.Count > Tracker.PreviousData.Count) Pulsing = false;
        var checkPulse = CheckPulse(i);
        if (!Pulsing && checkPulse) PulseAll();
        else if (Pulsing && !checkPulse) StopPulseAll();
        Pulsing = checkPulse;
    }

    private void PulseAll()
    {
        for (var i = 0; i < Tracker.CurrentData.MaxCount; i++)
        {
            Glows3[i].SetVis(i < Tracker.CurrentData.Count);
            Glows2[i].SetVis(false);
        }
    }

    private void StopPulseAll()
    {
        for (var i = 0; i < Tracker.CurrentData.MaxCount; i++)
        {
            Glows3[i].SetVis(false);
            Glows2[i].SetVis(i < Tracker.CurrentData.Count);
        }
    }

    #endregion

    #region Configs

    public class DragonScalesConfig : CounterWidgetConfig
    {
        public float Angle;
        public AddRGB ScaleColor = new(-40, -100, 11);
        public ColorRGB FrameColor = new(100);
        public bool HideEmpty;
        [DefaultValue(AtMax)] public CounterPulse Pulse = AtMax;

        public DragonScalesConfig(WidgetConfig widgetConfig) : base(widgetConfig.DragonScalesCfg)
        {
            var config = widgetConfig.DragonScalesCfg;

            if (config == null) return;

            Angle = config.Angle;
            ScaleColor = config.ScaleColor;
            FrameColor = config.FrameColor;
            HideEmpty = config.HideEmpty;

            Pulse = config.Pulse;
        }

        public DragonScalesConfig() { }
    }

    private DragonScalesConfig config;

    public override DragonScalesConfig Config => config;

    public override void InitConfigs() => config = new(Tracker.WidgetConfig);

    public override void ResetConfigs() => config = new();

    public AddRGB ColorOffset = new(40, 100, -11);
    public override void ApplyConfigs()
    {
        WidgetContainer.SetPos(Config.Position)
                  .SetScale(Config.Scale, Math.Abs(Config.Angle) > 90 ? -Config.Scale : Config.Scale)
                  .SetRotation(Config.Angle, true);

        StackContainer.SetAddRGB(Config.ScaleColor + ColorOffset);
    }

    public override void DrawUI()
    {
        base.DrawUI();
        switch (UiTab)
        {
            case Layout:
                AngleControls("Angle", ref Config.Angle);
                break;
            case Colors:
                ColorPickerRGB("Scale Color", ref Config.ScaleColor);
                ColorPickerRGB("Frame Tint", ref Config.FrameColor);
                break;
            case Behavior:
                if (ToggleControls("Hide Empty", ref Config.HideEmpty))
                {
                    if (Config.HideEmpty && ((!Config.AsTimer && Tracker.CurrentData.Count == 0) || (Config.AsTimer && Tracker.CurrentData.GaugeValue == 0))) FrameVanish();
                    if (!Config.HideEmpty && WidgetContainer.Alpha < 255) FrameAppear();
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
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] public DragonScalesConfig? DragonScalesCfg { get; set; }
}
