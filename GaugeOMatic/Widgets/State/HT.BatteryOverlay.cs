using GaugeOMatic.Trackers;
using GaugeOMatic.Windows;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using static CustomNodes.CustomNodeManager;
using static CustomNodes.CustomNodeManager.Tween;
using static GaugeOMatic.Utility.Color;
using static GaugeOMatic.Widgets.BatteryOverlay;
using static GaugeOMatic.Widgets.Common.CommonParts;
using static GaugeOMatic.Widgets.WidgetTags;
using static GaugeOMatic.Widgets.WidgetUI;
using static GaugeOMatic.Windows.UpdateFlags;
#pragma warning disable CS8618

namespace GaugeOMatic.Widgets;

public sealed unsafe class BatteryOverlay : StateWidget
{
    public BatteryOverlay(Tracker tracker) : base(tracker) { }

    public override WidgetInfo WidgetInfo => GetWidgetInfo;

    public static WidgetInfo GetWidgetInfo => new()
    {
        DisplayName = "Battery Gauge Overlay",
        Author = "ItsBexy",
        Description = "An electrical overlay over the Battery Gauge",
        WidgetTags = State | Replica | MultiComponent,
        MultiCompData = new("HT", "Heat Gauge Replica", 4)
    };

    public override CustomPartsList[] PartsLists { get; } = { MCH0Parts };

    #region Nodes

    public CustomNode Contents;
    public CustomNode ClockOverlay;
    public CustomNode Ring;
    public CustomNode Glow;

    public CustomNode ElecWrapper1;
    public CustomNode Elec1;
    public CustomNode Elec2;
    public CustomNode Elec3;

    public CustomNode ElecWrapper2;
    public CustomNode Elec4;
    public CustomNode Elec5;
    public CustomNode Elec6;

    public override CustomNode BuildRoot()
    {
        Ring = ImageNodeFromPart(0, 6).SetPos(3, -2).SetOrigin(32, 32).SetAlpha(0);
        Glow = ImageNodeFromPart(0, 8).SetPos(11, 6).SetScale(1.8f).SetOrigin(24, 24).SetImageFlag(32).SetAlpha(0);
        Elec1 = ImageNodeFromPart(0, 20).SetPos(-18, -26).SetScale(0.4f).SetRotation(-16, true).SetOrigin(31, 62).SetAddRGB(-100, -100, 50).SetImageFlag(32).SetAlpha(0);
        Elec2 = ImageNodeFromPart(0, 19).SetPos(26, -26).SetScale(0.4f).SetRotation(20, true).SetOrigin(31, 62).SetAddRGB(-100, -71, 78).SetImageFlag(32).SetAlpha(0);
        Elec3 = ImageNodeFromPart(0, 20).SetPos(26, -26).SetScale(0.4f).SetRotation(20, true).SetOrigin(31, 62).SetAddRGB(-100, -75, 75).SetImageFlag(32).SetAlpha(0);

        ElecWrapper1 = new(CreateResNode(), Elec1, Elec2, Elec3);
        ClockOverlay = new CustomNode(CreateResNode(),Ring,Glow, ElecWrapper1).SetPos(-1,6).SetSize(96,70);

        Elec4 = ImageNodeFromPart(0, 20).SetPos(140, -10).SetScale(0.6f,1).SetRotation(90, true).SetOrigin(31, 62).SetAddRGB(-100, -90, 60).SetImageFlag(32).SetAlpha(0);
        Elec5 = ImageNodeFromPart(0, 20).SetPos(80, -40).SetScale(-0.6f, 1).SetRotation(90, true).SetOrigin(31, 62).SetAddRGB(-100, -80, 70).SetImageFlag(32).SetAlpha(0);
        Elec6 = ImageNodeFromPart(0, 19).SetPos(80, -40).SetScale(0.6f, 1).SetRotation(90, true).SetOrigin(31, 62).SetAddRGB(-100, -64, 85).SetImageFlag(32).SetAlpha(0);

        ElecWrapper2 = new(CreateResNode(), Elec4, Elec5, Elec6);

        Contents = new CustomNode(CreateResNode(),ClockOverlay, ElecWrapper2).SetOrigin(33.5f, 36.5f);

        return new CustomNode(CreateResNode(), Contents);
    }

    #endregion

    #region Animations

    #endregion

    #region UpdateFuncs

    public override string SharedEventGroup => "HeatGauge";

    public override void OnFirstRun(int current)
    {


    }

    public override void Activate(int current)
    {
        var color = Config.PulseColors.ElementAtOrDefault(current);
        InvokeSharedEvent("HeatGauge", "StartBatteryGlow",new(){AddRGB = color});
        ClearLabelTweens(ref Tweens, "Pulse");
        Tweens.Add(new(Ring,
                       new(0) { Alpha = 0 },
                       new(225) { Alpha = 255 })
                       { Ease = Eases.SinInOut });

        Tweens.Add(new(Glow,
                       new(0) { Alpha = 0 },
                       new(126) { Alpha = 125 },
                       new(300) { Alpha = 0 },
                       new(1430) { Alpha = 0 })
                       { Ease = Eases.SinInOut,Repeat=true,Label="Pulse" });

        ElecTweens(Elec1, Elec2, Elec3);
        ElecTweens(Elec4, Elec5, Elec6);
    }

    private void ElecTweens(CustomNode node1, CustomNode node2, CustomNode node3)
    {
        Tweens.Add(new(node1,
                       new(0) { Alpha = 255, AddRGB = new(-100, -100, 50), MultRGB = new(100) },
                       new(90) { Alpha = 0, AddRGB = new(-100, -50, 100), MultRGB = new(50) },
                       new(359) { Alpha = 0, AddRGB = new(-100, -50, 100), MultRGB = new(50) },
                       new(360) { Alpha = 255, AddRGB = new(-100, -100, 50), MultRGB = new(100) },
                       new(560) { Alpha = 0, AddRGB = new(-100, -50, 100), MultRGB = new(50) },
                       new(769) { Alpha = 0, AddRGB = new(-100, -50, 100), MultRGB = new(50) },
                       new(770) { Alpha = 255, AddRGB = new(-100, -100, 50), MultRGB = new(100) },
                       new(930) { Alpha = 0, AddRGB = new(-100, -50, 100), MultRGB = new(50) },
                       new(1430) { Alpha = 0, AddRGB = new(-100, -50, 100), MultRGB = new(50) })
                       { Ease = Eases.SinInOut, Repeat = true, Label = "Elec" });

        Tweens.Add(new(node2,
                       new(0) { Alpha = 255, AddRGB = new(-100, -100, 50), MultRGB = new(100) },
                       new(100) { Alpha = 0, AddRGB = new(-100, -50, 100), MultRGB = new(50) },
                       new(529) { Alpha = 0, AddRGB = new(-100, -50, 100), MultRGB = new(50) },
                       new(530) { Alpha = 255, AddRGB = new(-100, -100, 50), MultRGB = new(100) },
                       new(730) { Alpha = 0, AddRGB = new(-100, -50, 100), MultRGB = new(50) },
                       new(1029) { Alpha = 0, AddRGB = new(-100, -50, 100), MultRGB = new(50) },
                       new(1030) { Alpha = 255, AddRGB = new(-100, -100, 50), MultRGB = new(100) },
                       new(1258) { Alpha = 4, AddRGB = new(-100, -50, 100), MultRGB = new(50) },
                       new(1430) { Alpha = 4, AddRGB = new(-100, -50, 100), MultRGB = new(50) })
                       { Ease = Eases.SinInOut, Repeat = true, Label = "Elec" });

        Tweens.Add(new(node3,
                       new(0) { Alpha = 255, AddRGB = new(-100, -100, 50), MultRGB = new(100) },
                       new(130) { Alpha = 0, AddRGB = new(-100, -50, 100), MultRGB = new(50) },
                       new(329) { Alpha = 0, AddRGB = new(-100, -50, 100), MultRGB = new(50) },
                       new(330) { Alpha = 255, AddRGB = new(-100, -100, 50), MultRGB = new(100) },
                       new(500) { Alpha = 0, AddRGB = new(-100, -50, 100), MultRGB = new(50) },
                       new(829) { Alpha = 0, AddRGB = new(-100, -50, 100), MultRGB = new(50) },
                       new(830) { Alpha = 255, AddRGB = new(-100, -100, 50), MultRGB = new(100) },
                       new(1000) { Alpha = 0, AddRGB = new(-100, -50, 100), MultRGB = new(50) },
                       new(1430) { Alpha = 0, AddRGB = new(-100, -50, 100), MultRGB = new(50) })
                       { Ease = Eases.SinInOut, Repeat = true, Label = "Elec" });
    }

    public override void Deactivate(int previous)
    {
        InvokeSharedEvent("HeatGauge", "StopBatteryGlow");
        ClearLabelTweens(ref Tweens,"Pulse"); 
        ClearLabelTweens(ref Tweens, "Elec");
        Tweens.Add(new(Ring,
                       new(0) { Alpha = 255 },
                       new(320) { Alpha = 0 })
                       { Ease = Eases.SinInOut });

        FadeOut(Glow);
        FadeOut(Elec1);
        FadeOut(Elec2);
        FadeOut(Elec3);
        FadeOut(Elec4);
        FadeOut(Elec5);
        FadeOut(Elec6);
    }

    private void FadeOut(CustomNode node) =>
        Tweens.Add(new(node,
                       new(0, node),
                       new(150) { Alpha = 0 })
                       { Ease = Eases.SinInOut, Label = "Pulse" });

    public override void StateChange(int current, int previous)
    {
        var pulseColor = Config.PulseColors.ElementAtOrDefault(current);
        var ringColor = Config.RingColors.ElementAtOrDefault(current) + new AddRGB(112, -74, -119);
        var elecColor = Config.ElecColors.ElementAtOrDefault(current) + new AddRGB(100, 75, -75);

        InvokeSharedEvent("HeatGauge", "StartBatteryGlow", new() { AddRGB = pulseColor });

        Tweens.Add(new(Ring,
                       new(0, Ring),
                       new(250) { AddRGB = ringColor })
                       { Ease = Eases.SinInOut });

        Tweens.Add(new(ElecWrapper1,
                       new(0, ElecWrapper1),
                       new(250) { AddRGB = elecColor })
                       { Ease = Eases.SinInOut });
        Tweens.Add(new(ElecWrapper2,
                       new(0, ElecWrapper2),
                       new(250) { AddRGB = elecColor })
                       { Ease = Eases.SinInOut });
    }

    #endregion

    #region Configs

    public class BatteryOverlayConfig
    {
        public Vector2 Position = new(0,0);
        public float Scale = 1;
        public List<AddRGB> PulseColors = new();
        public List<AddRGB> RingColors = new();
        public List<AddRGB> ElecColors = new();
        public float Angle;

        public BatteryOverlayConfig(WidgetConfig widgetConfig)
        {
            var config = widgetConfig.BatteryOverlayCfg;

            if (config == null) return;

            Position = config.Position;
            Scale = config.Scale;
            PulseColors = config.PulseColors;
            RingColors = config.RingColors;
            ElecColors = config.ElecColors;
            Angle = config.Angle;
        }

        public BatteryOverlayConfig() { }

        public void FillColorLists(int max)
        {
            while (PulseColors.Count <= max) PulseColors.Add(new(0,70,100));
            while (RingColors.Count <= max) RingColors.Add(new(-112, 74, 119));
            while (ElecColors.Count <= max) ElecColors.Add(new(-100,-75,75));
        }
    }

    public BatteryOverlayConfig Config;

    public override void InitConfigs()
    {
        Config = new(Tracker.WidgetConfig);
        Config.FillColorLists(Tracker.CurrentData.MaxState);
    }

    public override void ResetConfigs()
    {
        Config = new();
        Config.FillColorLists(Tracker.CurrentData.MaxState);
    }

    public override void ApplyConfigs()
    {
        WidgetRoot.SetPos(Config.Position)
                  .SetScale(Config.Scale);
        Contents.SetRotation(Config.Angle,true);
        
        var state = Tracker.CurrentData.State;
        var prev = Tracker.PreviousData.State;

        if (state > 0) StateChange(state, prev);
        else state = 1;

        var ringColor = Config.RingColors.ElementAtOrDefault(state);
        var elecColor = Config.ElecColors.ElementAtOrDefault(state);
        Ring.SetAddRGB(ringColor + new AddRGB(112, -74, -119));
        Glow.SetAddRGB(ringColor + new AddRGB(112, -74, -119));
        ElecWrapper1.SetAddRGB(elecColor + new AddRGB(100, 75, -75));
        ElecWrapper2.SetAddRGB(elecColor + new AddRGB(100, 75, -75));
    }

    public override void DrawUI(ref WidgetConfig widgetConfig, ref UpdateFlags update)
    {
        Heading("Layout");

        PositionControls("Position", ref Config.Position, ref update);
        ScaleControls("Scale", ref Config.Scale, ref update);
        FloatControls("Angle", ref Config.Angle, -180, 180, 1, ref update);

        Heading("Colors");

        for (var i = 1; i <= Tracker.CurrentData.MaxState; i++)
        {
            Heading(Tracker.StateNames[i]);
            var pulseColor = Config.PulseColors[i];
            var ringColor = Config.RingColors[i];
            var elecColor = Config.ElecColors[i];
            if (ColorPickerRGB($"Pulse##Color{i}", ref pulseColor, ref update)) Config.PulseColors[i] = pulseColor;
            if (ColorPickerRGB($"Ring##Color{i}", ref ringColor, ref update)) Config.RingColors[i] = ringColor;
            if (ColorPickerRGB($"Lightning##Color{i}", ref elecColor, ref update)) Config.ElecColors[i] = elecColor;
        }

        if (update.HasFlag(Save)) ApplyConfigs();
        widgetConfig.BatteryOverlayCfg = Config;
    }

    #endregion
}

public partial class WidgetConfig
{
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] public BatteryOverlayConfig? BatteryOverlayCfg { get; set; }
}
