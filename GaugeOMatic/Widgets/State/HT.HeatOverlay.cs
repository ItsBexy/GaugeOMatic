using CustomNodes;
using GaugeOMatic.CustomNodes.Animation;
using GaugeOMatic.Trackers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using static CustomNodes.CustomNode;
using static CustomNodes.CustomNodeManager;
using static GaugeOMatic.Utility.Color;
using static GaugeOMatic.Widgets.Common.CommonParts;
using static GaugeOMatic.Widgets.HeatOverlay;
using static GaugeOMatic.Widgets.WidgetTags;
using static GaugeOMatic.Widgets.WidgetUI;
using static GaugeOMatic.Widgets.WidgetUI.WidgetUiTab;

#pragma warning disable CS8618

namespace GaugeOMatic.Widgets;

[WidgetName("Heat Gauge Overlay")]
[WidgetDescription("A glowing overlay over the Heat Gauge.")]
[WidgetAuthor("ItsBexy")]
[WidgetTags(State | Replica | MultiComponent)]
[WidgetUiTabs(Layout | Colors)]
[MultiCompData("HT", "Heat Gauge Replica", 2)]
public sealed unsafe class HeatOverlay(Tracker tracker) : StateWidget(tracker)
{
    public override CustomPartsList[] PartsLists { get; } = [MCH0];

    #region Nodes

    public CustomNode GlowWrapper;
    public CustomNode Glow;
    public CustomNode SmokeContainer;
    public CustomNode Smoke1;
    public CustomNode Smoke2;
    public CustomNode Smoke3;

    public override Bounds GetBounds() => Glow;

    public override CustomNode BuildContainer()
    {
        Smoke1 = ImageNodeFromPart(0, 16).SetPos(-11, -9).SetOrigin(32, 85).SetAlpha(0);
        Smoke2 = ImageNodeFromPart(0, 16).SetPos(71, -26).SetOrigin(32, 85).SetAlpha(0);
        Smoke3 = ImageNodeFromPart(0, 16).SetPos(171, -1).SetOrigin(32, 85).SetAlpha(0);
        SmokeContainer = new CustomNode(CreateResNode(), Smoke1, Smoke2, Smoke3).SetOrigin(35, 63);
        Glow = NineGridFromPart(0, 7, 0, 60, 0, 120).SetPos(1, 1).SetNineGridBlend(2).SetAlpha(0);
        GlowWrapper = new CustomNode(CreateResNode(), Glow).SetOrigin(35, 43);

        return new(CreateResNode(), GlowWrapper, SmokeContainer);
    }

    #endregion

    #region UpdateFuncs

    public override string SharedEventGroup => "HeatGauge";

    public override void OnFirstRun(int current) { }

    public Random R = new();
    public void RandomPosition(CustomNode node, float minX, float maxX)
    {
        minX = (minX * (Config.Width + 50f)) - 11f;
        maxX = (maxX * (Config.Width + 50f)) - 11f;
        node.Node->SetPositionFloat((R.NextSingle() * (maxX - minX)) + minX, R.NextSingle() * -20f);
    }

    public override void Activate(int current)
    {
        Animator -= "Pulse";
        InvokeSharedEvent("HeatGauge", "StartHeatGlow");

        Animator +=
        [
            new(Glow,
                new(0) { Alpha = 0, ScaleY = 1 },
                new(450) { Alpha = 101, ScaleY = 1.04f },
                new(950) { Alpha = 0, ScaleY = 1.08f })
                { Repeat = true, Label = "Pulse" },

            new(Smoke1,
                new(0) { Scale = 0.9f, Alpha = 0 },
                new(160) { Scale = 1, Alpha = 125 },
                new(260) { ScaleX = 1, ScaleY = 1.05f, Alpha = 100 },
                new(360) { ScaleX = 1, ScaleY = 1.1f, Alpha = 50 },
                new(460) { ScaleX = 1, ScaleY = 1.15f, Alpha = 25 },
                new(560) { ScaleX = 1, ScaleY = 1.2f, Alpha = 10 },
                new(660) { ScaleX = 1, ScaleY = 1.4f, Alpha = 0 },
                new(960) { ScaleX = 1, ScaleY = 1.4f, Alpha = 0 })
                { Repeat = true, Label = "Pulse", PerCycle = () => RandomPosition(Smoke1, 0, 0.2f) },

            new(Smoke2,
                new(0) { Scale = 0.9f, Alpha = 0 },
                new(100) { Scale = 0.9f, Alpha = 0 },
                new(260) { Scale = 1, Alpha = 125 },
                new(360) { ScaleX = 1, ScaleY = 1.05f, Alpha = 100 },
                new(460) { ScaleX = 1, ScaleY = 1.1f, Alpha = 50 },
                new(560) { ScaleX = 1, ScaleY = 1.15f, Alpha = 25 },
                new(660) { ScaleX = 1, ScaleY = 1.2f, Alpha = 10 },
                new(760) { ScaleX = 1, ScaleY = 1.4f, Alpha = 0 },
                new(960) { ScaleX = 1, ScaleY = 1.4f, Alpha = 0 })
                { Repeat = true, Label = "Pulse", PerCycle = () => RandomPosition(Smoke2, 0.4f, 0.6f) },

            new(Smoke3,
                new(0) { Scale = 0.9f, Alpha = 0 },
                new(200) { Scale = 0.9f, Alpha = 0 },
                new(360) { Scale = 1, Alpha = 125 },
                new(460) { ScaleX = 1, ScaleY = 1.05f, Alpha = 100 },
                new(560) { ScaleX = 1, ScaleY = 1.1f, Alpha = 50 },
                new(660) { ScaleX = 1, ScaleY = 1.15f, Alpha = 25 },
                new(760) { ScaleX = 1, ScaleY = 1.2f, Alpha = 10 },
                new(860) { ScaleX = 1, ScaleY = 1.4f, Alpha = 0 },
                new(960) { ScaleX = 1, ScaleY = 1.4f, Alpha = 0 })
                { Repeat = true, Label = "Pulse", PerCycle = () => RandomPosition(Smoke3, 0.8f, 1f) }
        ];

    }

    public override void Deactivate(int previous)
    {
        InvokeSharedEvent("HeatGauge", "StopHeatGlow");

        Animator -= "Pulse";
        Animator +=
        [
            new(Glow,
                new(0, Glow),
                new(450) { Alpha = 0, ScaleY = 1.08f })
                { Label = "Pulse" },

            new(Smoke1,
                new(0, Smoke1),
                new(400) { ScaleY = 1.4f, Alpha = 0 })
                { Label = "Pulse" },

            new(Smoke2,
                new(0, Smoke2),
                new(460) { ScaleY = 1.4f, Alpha = 0 })
                { Label = "Pulse" },

            new(Smoke3,
                new(0, Smoke3),
                new(400) { ScaleY = 1.4f, Alpha = 0 })
                { Label = "Pulse" }
        ];
    }

    public override void StateChange(int current, int previous)
    {
        var color = Config.Colors.ElementAtOrDefault(current);
        Animator += new Tween(Glow,
                              new(0, Glow),
                              new(300) { AddRGB = color + ColorOffset });
    }

    #endregion

    #region Configs

    public class HeatOverlayConfig : WidgetTypeConfig
    {
        [DefaultValue(148)] public float Width = 148;
        public List<AddRGB> Colors = [];
        public float Angle;

        public HeatOverlayConfig(WidgetConfig widgetConfig) : base(widgetConfig.HeatOverlayCfg)
        {
            var config = widgetConfig.HeatOverlayCfg;

            if (config == null) return;

            Width = config.Width;
            Colors = config.Colors;
            Angle = config.Angle;
        }

        public HeatOverlayConfig() { }

        public void FillColorLists(int max)
        {
            while (Colors.Count <= max) Colors.Add(new(86, -75, -79));
        }
    }

    private HeatOverlayConfig config;

    public override HeatOverlayConfig Config => config;

    public readonly AddRGB ColorOffset = new(-86, 75, 79);

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
        WidgetContainer.SetPos(Config.Position)
                  .SetScale(Config.Scale);

        GlowWrapper.SetRotation(Config.Angle, true);
        SmokeContainer.SetRotation(Config.Angle, true);

        Smoke1.SetRotation(-Config.Angle, true);
        Smoke2.SetRotation(-Config.Angle, true);
        Smoke3.SetRotation(-Config.Angle, true);

        var state = Tracker.CurrentData.State;
        var color = Config.Colors.ElementAtOrDefault(state);
        Glow.SetWidth(Config.Width + 80)
            .SetOrigin((Config.Width + 80) / 2, 53)
            .SetAddRGB(color + ColorOffset);
    }

    public override void DrawUI()
    {
        base.DrawUI();
        switch (UiTab)
        {
            case Layout:
                FloatControls("Width", ref Config.Width, 70, 1000, 1);
                AngleControls("Angle", ref Config.Angle);
                break;
            case Colors:
                for (var i = 1; i <= Tracker.CurrentData.MaxState; i++)
                {
                    var color = Config.Colors[i];
                    if (ColorPickerRGB($"{Tracker.StateNames[i]}##Color{i}", ref color)) Config.Colors[i] = color;
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
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] public HeatOverlayConfig? HeatOverlayCfg { get; set; }
}
