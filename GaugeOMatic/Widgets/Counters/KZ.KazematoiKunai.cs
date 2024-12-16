using CustomNodes;
using GaugeOMatic.CustomNodes.Animation;
using GaugeOMatic.Trackers;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel;
using System.Numerics;
using static CustomNodes.CustomNodeManager;
using static GaugeOMatic.CustomNodes.Animation.KeyFrame;
using static GaugeOMatic.Utility.Color;
using static GaugeOMatic.Widgets.Common.CommonParts;
using static GaugeOMatic.Widgets.CounterWidgetConfig.CounterPulse;
using static GaugeOMatic.Widgets.FreeGemCounterConfig.ArrangementStyle;
using static GaugeOMatic.Widgets.KazematoiKunai;
using static GaugeOMatic.Widgets.WidgetTags;
using static GaugeOMatic.Widgets.Common.WidgetUI;
using static GaugeOMatic.Widgets.Common.WidgetUI.WidgetUiTab;
using static System.Math;

#pragma warning disable CS8618

namespace GaugeOMatic.Widgets;

[WidgetName("Kazematoi Kunai")]
[WidgetDescription("A set of kunai recreating those on the Kazematoi Gauge.")]
[WidgetAuthor("ItsBexy")]
[WidgetTags(Counter | Replica | MultiComponent)]
[WidgetUiTabs(Layout | Colors | Behavior | Icon | Sound)]
[MultiCompData("KZ", "Kazematoi Replica", 3)]
public sealed unsafe class KazematoiKunai(Tracker tracker) : FreeGemCounter(tracker)
{
    public override CustomPartsList[] PartsLists { get; } = [NIN1];

    #region Nodes

    public List<CustomNode> Slots;
    public List<CustomNode> Knives;
    public List<CustomNode> Knives1;
    public List<CustomNode> Knives2;

    public override CustomNode BuildContainer()
    {
        Max = GetMax();
        BuildStacks(Max);

        return new CustomNode(CreateResNode(), Stacks.ToArray()).SetAlpha(0);
    }

    private void BuildStacks(int count)
    {
        Slots = [];
        Stacks = [];
        Knives = [];
        Knives1 = [];
        Knives2 = [];

        for (var i = 0; i < count; i++)
        {
            Slots.Add(ImageNodeFromPart(0, 4).SetImageWrap(2));
            Knives1.Add(ImageNodeFromPart(0, 2).SetImageWrap(2));
            Knives2.Add(new(CreateResNode(), ImageNodeFromPart(0, 3).SetImageWrap(2)));

            SetupKnifePulse1(i);
            SetupKnifePulse2(i);

            Knives.Add(new(CreateResNode(), Knives1[i], Knives2[i]));

            Stacks.Add(new CustomNode(CreateResNode(), Slots[i], Knives[i]).SetOrigin(9, 69).SetSize(46, 78));
        }
    }

    #endregion

    #region Animations

    private void SetupKnifePulse1(int i)
    {
        Animator -= $"KnifePulse1-{i}";
        Animator +=
        [
            new(Knives1[i],
                new(0) { AddRGB = new(0) },
                new(480) { AddRGB = new(50) },
                new(960) { AddRGB = new(0) })
                { Repeat = true, Label = $"KnifePulse1-{i}" }
        ];
    }

    private void StopKnifePulse1(int i) => Animator -= $"KnifePulse1-{i}";

    private void SetupKnifePulse2(int i)
    {
        Animator +=
        [
            new(Knives2[i][0],
                new(0) { Alpha=0 },
                new(650) { Alpha=128 },
                new(1300) { Alpha=0 })
                { Repeat = true }
        ];
    }

    public void PulseAll(int current)
    {
        for (var i = 0; i < current; i++)
        {
            StopKnifePulse1(i);
            Animator += new Tween(Knives2[i], new(0) { Alpha = Knives2[i].Alpha }, Visible[150]);
        }
    }

    public void StopPulseAll()
    {
        for (var i = 0; i < Max; i++)
        {
            Animator -= $"KnifePulse2{i}";
            SetupKnifePulse1(i);
            Animator += new Tween(Knives2[i], new(0) { Alpha = Knives2[i].Alpha }, Hidden[150]);
        }
    }

    public override void ShowStack(int i)
    {
        Animator += new Tween(Knives[i],
                              new(0) { X = 15, Y = -32, Alpha = 0 },
                              new(170) { X = 0, Y = 0, Alpha = 255 });
    }

    public override void HideStack(int i)
    {
        Animator += new Tween(Knives[i],
                              new(0) { X = 0, Y = 0, Alpha = 255 },
                              new(90) { X = -15, Y = 32, Alpha = 0 });
    }

    private void AllVanish()
    {
        Animator += new Tween(WidgetContainer,
                              new(0) { Alpha = 255, AddRGB = 0 },
                              new(200) { Alpha = 0, AddRGB = 100 });
    }

    private void AllAppear()
    {
        Animator += new Tween(WidgetContainer,
                              new(0) { Alpha = 0, AddRGB = 100 },
                              new(200) { Alpha = 255, AddRGB = 0 });
    }

    #endregion

    #region UpdateFuncs

    public override void OnFirstRun(int count, int max)
    {
        for (var i = 0; i < max; i++) Knives[i].SetAlpha(i < count);
        if (!Config.HideEmpty || count > 0)
        {
            AllAppear();
        }
        else
        {
            WidgetContainer.SetAlpha(0);
        }
    }

    public override void OnDecreaseToMin() { if (Config.HideEmpty) AllVanish(); }
    public override void OnIncreaseFromMin() { if (Config.HideEmpty || WidgetContainer.Alpha < 255) { AllAppear(); } }

    public bool Pulsing;
    public bool CheckPulse(int i) => i > 0 && (Config.Pulse == Always || (Config.Pulse == AtMax && i == Stacks.Count));
    public override void PostUpdate(int i)
    {
        var checkPulse = CheckPulse(i);
        if (!Pulsing && checkPulse) PulseAll(i);
        else if (Pulsing && !checkPulse) StopPulseAll();
        Pulsing = checkPulse;
    }
    #endregion

    #region Configs

    public class KazematoiKunaiConfig : FreeGemCounterConfig
    {
        public AddRGB KunaiColor = new(0);
        [DefaultValue(true)] public bool Stagger = true;
        public bool HideEmpty;
        [DefaultValue(AtMax)] public CounterPulse Pulse = AtMax;

        public KazematoiKunaiConfig(WidgetConfig widgetConfig) : base(widgetConfig.KazematoiKunaiCfg)
        {
            var config = widgetConfig.KazematoiKunaiCfg;

            if (config == null) return;

            KunaiColor = config.KunaiColor;
            Stagger = config.Stagger;
            HideEmpty = config.HideEmpty;
        }

        [JsonIgnore] public override float DefaultSpacing => 23;
        public KazematoiKunaiConfig() { }
    }

    private KazematoiKunaiConfig config;

    public override KazematoiKunaiConfig Config => config;

    public override void InitConfigs() => config = new(Tracker.WidgetConfig);

    public override void ResetConfigs() => config = new();

    public override void ApplyConfigs()
    {
        base.ApplyConfigs();
        WidgetContainer.SetPos(Config.Position + new Vector2(-3, 0));

        PlaceFreeGems();

        for (var i = 0; i < Stacks.Count; i++)
        {
            Knives[i].SetAddRGB(Config.KunaiColor);
        }
    }

    protected override Vector2 AdjustedGemPos(int i, double x, double y, float gemAngle)
    {
        var even = i % 2 == 0;
        var x2 = even && Config.Stagger ? 5 : 0;
        var y2 = !even && Config.Stagger ? 16 : 0;

        var cos = Cos(gemAngle);
        var sin = Sin(gemAngle);

        return new((float)((x2 * cos) - (y2 * sin) + x),
                   (float)((y2 * cos) + (x2 * sin) + y));
    }

    public override string StackTerm => "Knife";

    public override void DrawUI()
    {
        base.DrawUI();
        switch (UiTab)
        {
            case Layout:
                if (Config.Arrangement == Arc) ToggleControls("Staggered", ref Config.Stagger);
                break;
            case Colors:
                ColorPickerRGB("Kunai Tint", ref Config.KunaiColor);
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
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] public KazematoiKunaiConfig? KazematoiKunaiCfg { get; set; }
}
