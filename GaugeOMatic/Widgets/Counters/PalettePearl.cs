using CustomNodes;
using GaugeOMatic.CustomNodes.Animation;
using GaugeOMatic.Trackers;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel;
using System.Numerics;
using static CustomNodes.CustomNodeManager;
using static GaugeOMatic.CustomNodes.Animation.Tween.EaseType;
using static GaugeOMatic.Trackers.Tracker;
using static GaugeOMatic.Trackers.Tracker.UpdateFlags;
using static GaugeOMatic.Utility.Color;
using static GaugeOMatic.Utility.MiscMath;
using static GaugeOMatic.Widgets.Common.CommonParts;
using static GaugeOMatic.Widgets.CounterWidgetConfig.CounterPulse;
using static GaugeOMatic.Widgets.PalettePearl;
using static GaugeOMatic.Widgets.WidgetTags;
using static GaugeOMatic.Widgets.WidgetUI;
using static System.Math;
using static GaugeOMatic.Widgets.WidgetUI.WidgetUiTab;

#pragma warning disable CS8618

namespace GaugeOMatic.Widgets;

public sealed unsafe class PalettePearl : CounterWidget
{
    public PalettePearl(Tracker tracker) : base(tracker) { }

    public override WidgetInfo WidgetInfo => GetWidgetInfo;

    public static WidgetInfo GetWidgetInfo { get; } = new()
    {
        DisplayName = "Palette Pearls",
        Author = "ItsBexy",
        Description = "A counter based on Pictomancer's White/Black Paint stacks",
        WidgetTags = Counter | Replica,
        UiTabOptions = Layout | Colors | Behavior
    };

    public override CustomPartsList[] PartsLists { get; } = { PCT1 };

    #region Nodes

    public List<CustomNode> Stacks;
    public List<CustomNode> Frames;
    public List<CustomNode> PearlContainers;
    public List<CustomNode> Pearls;
    public List<CustomNode> PulseContainers;
    public List<CustomNode> Sparkles;
    public List<CustomNode> Halos;
    public List<CustomNode> PulseHalos;
    public List<CustomNode> Glows;
    public List<CustomNode> PulseGlows;
    public List<CustomNode> Flashes;

    public override CustomNode BuildContainer()
    {
        Max = GetMax();
        BuildStacks(Max);

        return new(CreateResNode(), Stacks.ToArray());
    }

    private void BuildStacks(int count)
    {
        Stacks = new();
        Frames = new();
        Pearls = new();
        PearlContainers = new();
        PulseContainers = new();
        Halos = new();
        PulseHalos = new();
        Sparkles = new();
        Glows = new();
        PulseGlows = new();
        Flashes = new();

        for (var i = 0; i < count; i++)
        {
            Frames.Add(ImageNodeFromPart(0, 1));
            Pearls.Add(ImageNodeFromPart(0, 2).SetOrigin(18, 18).SetAlpha(0));
            PulseHalos.Add(ImageNodeFromPart(0, 14).SetScale(1.2f).SetOrigin(18, 18).SetAlpha(0).SetImageFlag(0x20));
            PulseGlows.Add(ImageNodeFromPart(0, 4).SetScale(1.5f).SetOrigin(18, 18).SetAlpha(0));
            PulseContainers.Add(new CustomNode(CreateResNode(), PulseHalos[i], PulseGlows[i]));
            Glows.Add(ImageNodeFromPart(0, 4).SetOrigin(18, 18).SetAlpha(0));
            Sparkles.Add(ImageNodeFromPart(0, 15).SetPos(5, 5).SetSize(26, 26).SetOrigin(13, 13).SetImageFlag(0x20).SetRotation(PI / 2f).SetAlpha(0));
            Halos.Add(ImageNodeFromPart(0, 14).SetScale(1.2f).SetOrigin(18, 18).SetImageFlag(0x20).SetAlpha(0));
            Flashes.Add(ImageNodeFromPart(0, 13).SetOrigin(18, 18).SetImageFlag(0x20).SetAlpha(0));
            PearlContainers.Add(new CustomNode(CreateResNode(), Pearls[i], PulseContainers[i], Glows[i], Sparkles[i], Halos[i], Flashes[i]).SetSize(36, 36));
            Stacks.Add(new CustomNode(CreateResNode(), Frames[i], PearlContainers[i]).SetOrigin(18, 18));
        }
    }

    #endregion

    #region Animations

    public override void ShowStack(int i)
    {
        //var gemColor = Config.GemColor + GemColorOffset;

        Animator += Config.AnimType switch
        {
            1 => new Tween[]
            {
                new(Pearls[i],
                    new(0) { Alpha = 0, Scale = 2.5f, AddRGB = Config.PearlColor },
                    new(130) { Alpha = 255, Scale = 1, AddRGB = Config.PearlColor + 50 },
                    new(300) { Alpha = 255, Scale = 1, AddRGB = Config.PearlColor }),
                new(Sparkles[i],
                    new(0) { Alpha = 0, Scale = 3, Rotation = 0 },
                    new(300) { Alpha = 255, Scale = 2, Rotation = Radians(90) },
                    new(470) { Alpha = 0, Scale = 1, Rotation = Radians(90) }),
                new(Halos[i],
                    new(0) { Alpha = 255, Scale = 0 },
                    new(300) { Alpha = 255, Scale = 0.5f },
                    new(470) { Alpha = 0, Scale = 1.2f }),
                new(Flashes[i],
                    new(0) { Alpha = 0, Scale = 0.7f },
                    new(180) { Alpha = 255, Scale = 1.2f },
                    new(440) { Alpha = 0, Scale = 1.5f })
            },
            _ => new Tween[]
            {
                new(Pearls[i], new(0) { Alpha = 0, Scale = 1 }, new(300) { Alpha = 255, Scale = 1 }),
                new(Glows[i],
                    new(0) { Alpha = 0, Scale = 2 },
                    new(300) { Alpha = 255, Scale = 1 },
                    new(730) { Alpha = 0, Scale = 1 }),
                new(Flashes[i], new(0) { Alpha = 0, Scale = 0, Rotation = 0 },
                    new(300) { Alpha = 153, Scale = 1.5f, Rotation = Radians(180) },
                    new(700) { Alpha = 0, Scale = 1, Rotation = Radians(360) })
            }
        };

        PulseContainers[i].Show();
    }

    public override void HideStack(int i)
    {
        Animator += new Tween[] {
            new(Pearls[i],
                new(0)   { Alpha = 255, Scale=1 },
                new(300) { Alpha = 0,Scale=2 }),
            new(Glows[i],
                new(0){Alpha=255,Scale=1,AddRGB=Config.Effects2},
                new(135){Alpha=127,Scale=1.5f,AddRGB=Config.Effects2 +100},
                new(350){Alpha=0,Scale=2,AddRGB=Config.Effects2}
            )
        };

        PulseContainers[i].Hide();
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
            Animator += new Tween[]
            {
                new(PulseHalos[i],
                    new(0) { Scale = 0.0f, Alpha = 255 },
                    new(240) { Scale = 0.5f, Alpha = 255 },
                    new(790) { Scale = 1.2f, Alpha = 0 },
                    new(1300) { Scale = 1.2f, Alpha = 0 })
                    { Repeat = true, Ease = Linear, Label = "Pulse" },
                new(PulseGlows[i],
                    new(0) { Scale = 0.5f, Alpha = 0 },
                    new(240) { Scale = 1.2f, Alpha = 128 },
                    new(1300) { Scale = 1.5f, Alpha = 0 })
                    { Repeat = true, Ease = Linear, Label = "Pulse" }
            };
        }
    }

    private void StopPulseAll()
    {
       Animator -= "Pulse";
       for (var i = 0; i < Stacks.Count; i++)
       {
           Animator += new Tween[]
           {
               new(PulseHalos[i],
                   new(0, PulseHalos[i]),
                   new(500) { Scale = 1.2f, Alpha = 0 })
                   { Label = "Pulse" },
               new(PulseGlows[i],
                   new(0, PulseGlows[i]),
                   new(500) { Scale = 1.5f, Alpha = 0 })
                   { Label = "Pulse" }
           };
       }
    }

    #endregion

    #region UpdateFuncs

    public override void OnFirstRun(int count, int max)
    {
        for (var i = 0; i < max; i++)
        {
            Pearls[i].SetAlpha(i < count);
            PulseContainers[i].SetVis(i < count);
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

    public class PalettePearlConfig : CounterWidgetConfig
    {
        public Vector2 Position;
        [DefaultValue(1f)] public float Scale = 1;
        [DefaultValue(23f)] public float Spacing = 22;
        public uint BasePearl;
        public uint AnimType;
        public float GemAngle;
        public float Angle;
        public float Curve;
        public ColorRGB FrameColor = new(100);
        public bool HideEmpty;

        public AddRGB PearlColorW = new(0);
        public AddRGB Effects1W = new(0,0,150);
        public AddRGB Effects2W = new(-128, 77, 127);
        public AddRGB PearlColorB = new(103, -22, 122);
        public AddRGB Effects1B = new(150, -150, -50);
        public AddRGB Effects2B = new(97, -128, 122);

        [JsonIgnore] public AddRGB PearlColor => BasePearl == 0 ? PearlColorW : PearlColorB + new AddRGB(-103, 22, -122);
        [JsonIgnore] public AddRGB Effects1 => BasePearl == 0 ? Effects1W : Effects1B;
        [JsonIgnore] public AddRGB Effects2 => BasePearl == 0 ? Effects2W + new AddRGB(128,-77,-127) : Effects2B + new AddRGB(-97,128,-122);

        [DefaultValue(Always)] public CounterPulse Pulse = Always;

        public PalettePearlConfig(WidgetConfig widgetConfig)
        {
            var config = widgetConfig.PalettePearlCfg;

            if (config == null) return;

            Position = config.Position;
            Scale = config.Scale;
            PearlColorW = config.PearlColorW;
            Effects1W = config.Effects1W;
            Effects2W = config.Effects2W;
            PearlColorB = config.PearlColorB;
            Effects1B = config.Effects1B;
            Effects2B = config.Effects2B;
            BasePearl = config.BasePearl;
            AnimType = config.AnimType;

            Spacing = config.Spacing;
            GemAngle = config.GemAngle;
            Angle = config.Angle;
            Curve = config.Curve;
            FrameColor = config.FrameColor;
            HideEmpty = config.HideEmpty;

            Pulse = config.Pulse;
            AsTimer = config.AsTimer;
            TimerSize = config.TimerSize;
            InvertTimer = config.InvertTimer;
        }

        public PalettePearlConfig() { }
    }

    public override CounterWidgetConfig GetConfig => Config;

    public PalettePearlConfig Config;

    public override void InitConfigs() => Config = new(Tracker.WidgetConfig);

    public override void ResetConfigs() => Config = new();

    public override void ApplyConfigs()
    {
        var widgetAngle = Config.Angle + (Config.Curve / 2f);
        WidgetContainer.SetPos(Config.Position)
                  .SetScale(Config.Scale)
                  .SetRotation(widgetAngle, true);

        var posAngle = 0f;
        double x = 0;
        double y = 0;
        for (var i = 0; i < Stacks.Count; i++)
        {
            Frames[i].SetMultiply(Config.FrameColor);

            var gemAngle = Config.GemAngle + (Config.Curve * (i - 0.5f));
            while (gemAngle + widgetAngle > 360) gemAngle -= 360;
            while (gemAngle + widgetAngle <= -360) gemAngle += 360;

            var combinedAngle = gemAngle + widgetAngle;

            Pearls[i].SetPartId(Config.BasePearl > 0 ? 3 : 2).SetRotation(-combinedAngle,true).SetAddRGB(Config.PearlColor);


            Glows[i].SetPartId(Config.BasePearl > 0 ? 5 : 4).SetAddRGB(Config.Effects2);
            PulseGlows[i].SetPartId(Config.BasePearl > 0 ? 5 : 4).SetAddRGB(Config.Effects2);

            Flashes[i].SetAddRGB(Config.Effects1);
            Sparkles[i].SetAddRGB(Config.Effects1);
            Halos[i].SetAddRGB(Config.Effects1);
            PulseHalos[i].SetAddRGB(Config.Effects1);

            Stacks[i].SetPos((float)x, (float)y)
                     .SetRotation(gemAngle, true);

            x += Cos(posAngle * (PI / 180)) * Config.Spacing;
            y += Sin(posAngle * (PI / 180)) * Config.Spacing;
            posAngle += Config.Curve;
        }
    }

    public override void DrawUI(ref WidgetConfig widgetConfig, ref UpdateFlags update)
    {
        switch (UiTab)
        {
            case Layout:
                PositionControls("Position", ref Config.Position, ref update);
                ScaleControls("Scale", ref Config.Scale, ref update);
                FloatControls("Spacing", ref Config.Spacing, -1000, 1000, 0.5f, ref update);
                AngleControls("Angle (Pearl)", ref Config.GemAngle, ref update);
                AngleControls("Angle (Group)", ref Config.Angle, ref update);
                AngleControls("Curve", ref Config.Curve, ref update, true);
                break;
            case Colors:
                RadioControls("Base Color", ref Config.BasePearl, new() { 0, 1 }, new() { "White Paint", "Black Paint" }, ref update);

                if (Config.BasePearl == 0)
                {
                    ColorPickerRGB("Pearl Tint", ref Config.PearlColorW, ref update);
                    ColorPickerRGB("Effects", ref Config.Effects1W, ref update);
                    ColorPickerRGB(" ##Effects2", ref Config.Effects2W, ref update);
                }
                else
                {
                    ColorPickerRGB("Pearl Tint", ref Config.PearlColorB, ref update);
                    ColorPickerRGB("Effects", ref Config.Effects1B, ref update);
                    ColorPickerRGB(" ##Effects2", ref Config.Effects2B, ref update);
                }

                ColorPickerRGB("Frame Tint", ref Config.FrameColor, ref update);
                break;
            case Behavior:
                if (RadioControls("Appear Animation", ref Config.AnimType, new() { 0, 1 }, new() { "Type 1", "Type 2" }, ref update))
                    for (var i = 0; i < Tracker.CurrentData.Count; i++)
                        ShowStack(i);

                if (ToggleControls("Hide Empty", ref Config.HideEmpty, ref update))
                {
                    if (Config.HideEmpty && ((!Config.AsTimer && Tracker.CurrentData.Count == 0) || (Config.AsTimer && Tracker.CurrentData.GaugeValue == 0))) AllVanish();
                    if (!Config.HideEmpty && WidgetContainer.Alpha < 255) AllAppear();
                }

                RadioControls("Pulse", ref Config.Pulse, new() { Never, AtMax, Always }, new() { "Never", "At Maximum", "Always" }, ref update);

                CounterAsTimerControls(ref Config.AsTimer, ref Config.InvertTimer, ref Config.TimerSize, Tracker.TermGauge, ref update);
                break;
            default:
                break;
        }

        if (update.HasFlag(Save)) ApplyConfigs();
        widgetConfig.PalettePearlCfg = Config;
    }

    #endregion
}

public partial class WidgetConfig
{
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] public PalettePearlConfig? PalettePearlCfg { get; set; }
}
