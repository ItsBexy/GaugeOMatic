using GaugeOMatic.Trackers;
using GaugeOMatic.Windows;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Numerics;
using static CustomNodes.CustomNodeManager;
using static CustomNodes.CustomNodeManager.Tween;
using static GaugeOMatic.Utility.Color;
using static GaugeOMatic.Widgets.ChakraBar;
using static GaugeOMatic.Widgets.CounterWidgetConfig.CounterPulse;
using static GaugeOMatic.Widgets.WidgetTags;
using static GaugeOMatic.Widgets.WidgetUI;
using static GaugeOMatic.Windows.UpdateFlags;

namespace GaugeOMatic.Widgets;

public sealed unsafe class ChakraBar : CounterWidget
{
    public override WidgetInfo WidgetInfo => GetWidgetInfo;

    public static WidgetInfo GetWidgetInfo => new() 
    { 
        DisplayName = "Chakra Bar",
        Author = "ItsBexy",
        Description = "A stack counter made out of a combination of Monk gauge elements.",
        WidgetTags = Counter
    };

    public override CustomPartsList[] PartsLists { get; } = {
        new("ui/uld/JobHudMNK0.tex",
            new(0, 3, 62, 58),
            new(62, 3, 36, 58),
            new(98, 3, 62, 58),
            new(0, 3, 44, 58),
            new(116, 3, 44, 58)),
        new("ui/uld/JobHudMNK1.tex",
            new(213, 1, 38, 38),
            new(173, 1, 38, 38),
            new(172, 40, 40, 40),
            new(173, 81, 46, 46),
            new(176, 130, 64, 64),
            new(89, 199, 68, 5))
    };

    #region Nodes

    public CustomNode SocketPlate;
    public CustomNode StackContainer;
    public List<CustomNode> Stacks = new();

    public override CustomNode BuildRoot()
    {
        var count = Config.AsTimer ? Config.TimerSize : Tracker.GetCurrentData().MaxCount;

        SocketPlate = BuildSocketPlate(count, out var size).SetOrigin(size / 2, 29);
        StackContainer = BuildStacks(count);

        return new CustomNode(CreateResNode(), SocketPlate, StackContainer).SetOrigin(size/2,29);
    }

    private CustomNode BuildSocketPlate(int count, out int size)
    {
        if (count == 1)
        {
            size = 88;
            return new(CreateResNode(), 
                       ImageNodeFromPart(0, 3).SetPos(0, 0), 
                       ImageNodeFromPart(0, 4).SetPos(44, 0));
        }

        var socketNodes = new CustomNode[count];
        var x = 0;
        for (var i = 0; i < count; i++)
        {
            var part = (ushort)(i == 0 ? 0 : i == count - 1 ? 2 : 1);
            socketNodes[i] = ImageNodeFromPart(0, part).SetPos(x, 0);
            x += i == 0 || i == count - 1 ? 62 : 36;
        }

        size = x;
        return new(CreateResNode(), socketNodes);
    }

    public List<CustomNode> Pearls = new();
    public List<CustomNode> Lines = new();
    public List<CustomNode> Rings = new();
    public List<CustomNode> Glows = new();
    public List<CustomNode> ActionLines = new();
    public List<CustomNode> ActionLines2 = new();

    private CustomNode BuildStacks(int count)
    {
        Stacks = new List<CustomNode>();
        for (var i = 0; i < count; i++)
        {
            Pearls.Add(ImageNodeFromPart(1, 0).SetOrigin(19.2f,19.6f)
                                              .SetAlpha(0));

            Lines.Add(ImageNodeFromPart(1, 5).SetOrigin(34,3)
                                             .SetScale(4,0)
                                             .SetPos(-15,16)
                                             .SetImageFlag(32)
                                             .SetAddRGB(200, 100,-100)
                                             .SetMultiply(50));

            Rings.Add(ImageNodeFromPart(1, 3).SetOrigin(23,23)
                                             .SetScale(1.2f)
                                             .SetPos(-4,-4)
                                             .SetImageFlag(32)
                                             .SetAddRGB(200, -100,-250)
                                             .SetMultiply(50)
                                             .SetAlpha(0));

            Glows.Add(ImageNodeFromPart(1,1).SetOrigin(19,19)
                                            .SetScale(1.2f)
                                            .SetPos(0,0)
                                            .SetMultiply(100)
                                            .SetAlpha(0));

            ActionLines.Add(ImageNodeFromPart(1,4).SetOrigin(32,32)
                                                  .SetImageFlag(32)
                                                  .SetPos(-13,-14)
                                                  .SetAddRGB(150,0,-150)
                                                  .SetMultiply(70)
                                                  .SetAlpha(0));

            ActionLines2.Add(ImageNodeFromPart(1, 4).SetOrigin(32, 32)
                                                    .SetImageFlag(32)
                                                    .SetPos(-13, -14)
                                                    .SetAddRGB(100, 0, -200)
                                                    .SetMultiply(80)
                                                    .SetRotation(1.0471976f)
                                                    .SetAlpha(0));

            Stacks.Add(new CustomNode(CreateResNode(), Pearls[i], Lines[i], Rings[i], Glows[i], ActionLines[i], ActionLines2[i]).SetScale(0.8f).SetPos(29 + (i * 36), 13f));
        }

        return new(CreateResNode(),Stacks.ToArray());
    }

    #endregion

    #region Animations

    public override void ShowStack(int i)
    {
        Tweens.Add(new(Pearls[i],
                       new(0){Alpha = 0,Scale = 0.6f},
                       new(80){Alpha=255,Scale = 1}));

        Tweens.Add(new(Lines[i], 
                             new(0) { Alpha = 0, ScaleX = 0f, ScaleY = 1, AddRGB = new(200,100,-100), MultRGB = new(50)}, 
                             new(80) { Alpha = 255, ScaleX = 2, ScaleY = 1, AddRGB = new(100,0,-100), MultRGB = new(100)},
                             new(300) { Alpha = 0, ScaleX = 4, ScaleY = 0, AddRGB = new(200,100,-100), MultRGB = new(50) }));
      
        Tweens.Add(new(Rings[i],
                             new(0) { Alpha = 0, Scale = 0.3f, AddRGB = new(183,83,-250), MultRGB = new(50) }, 
                             new(80) { Alpha = 255, Scale = 1, AddRGB = new(100,0,-250), MultRGB = new(100) },
                             new(300) { Alpha = 0, Scale = 1.2f , AddRGB = new(200,-100,-250), MultRGB = new(50) }));

        Tweens.Add(new(Glows[i],
                             new(0)   { Alpha = 0, X = 0, Y = 0 },
                             new(60)  { Alpha = 75, X = 2, Y = 0 },
                             new(120) { Alpha = 51, X = -3, Y = 3 },
                             new(180) { Alpha = 100, X = -1, Y = 4 },
                             new(240) { Alpha = 24, X = 0, Y = 0 },
                             new(300) { Alpha = 0, X = 2, Y = 2 }));

        Tweens.Add(new(ActionLines[i],
                             new(0) { Scale = 2,Alpha=142,AddRGB=new AddRGB(80,0,-160)},
                             new(144) { Scale = 1, Alpha = 0, AddRGB = new(80, 0, -160) },
                             new(146) { Scale = 2, Alpha =73, AddRGB = new(150, 0, -150)},
                             new(300) { Scale = 1, Alpha = 0, AddRGB = new(150, 0, -150) }));

        Tweens.Add(new(ActionLines2[i],
                             new(0) {Scale = 1.6f,Alpha=128 },
                             new(200) { Scale = 1, Alpha = 0 }));
    }

    public override void HideStack(int i)
    {
        Tweens.Add(new(Pearls[i], 
                       new(0) { Alpha = 255, Scale = 1,AddRGB=new AddRGB(0) }, 
                       new(100) { Alpha = 255, Scale = 1.1f, AddRGB = new(100) },
                       new(250) { Alpha = 0, Scale = 0.2f, AddRGB = new(0) }));

        Tweens.Add(new(Rings[i],
                             new(0) { Alpha = 0, Scale = 0.3f, AddRGB = new(183, 83, -250), MultRGB = new(50) },
                             new(80) { Alpha = 255, Scale = 1, AddRGB = new(100, 0, -250), MultRGB = new(100) },
                             new(300) { Alpha = 0, Scale = 1.2f, AddRGB = new(200, -100, -250), MultRGB = new(50) }));

    }

    private void PlateAppear() =>
        Tweens.Add(new(WidgetRoot,
                       new(0) { Scale = Config.Scale * 1.65f, Alpha = 0 },
                       new(200) { Scale = Config.Scale, Alpha = 255 })
                       { Ease = Eases.SinInOut });

    private void PlateVanish() =>
        Tweens.Add(new(WidgetRoot,
                       new(0) { Scale = Config.Scale, Alpha = 255 },
                       new(150) { Scale = Config.Scale * 0.65f, Alpha = 0 })
                       { Ease = Eases.SinInOut });

    #endregion

    #region UpdateFuncs

    public override string? SharedEventGroup => null;

    public override void OnDecreaseToMin() { if (Config.HideEmpty) PlateVanish(); }
    public override void OnIncreaseFromMin() { if (Config.HideEmpty || WidgetRoot.Alpha < 255) PlateAppear(); }

    public override void OnFirstRun(int count, int max)
    {
        for (var i = 0; i < count; i++) Pearls[i].SetAlpha(255);
        for (var i = count; i < max; i++) Pearls[i].SetAlpha(0);
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

    private void PulseAll()
    {
        foreach (var s in Stacks)
        {
            ClearNodeTweens(ref Tweens, s);
            Tweens.Add(new(s, new(0) { AddRGB = new(0) }, new(460) { AddRGB = new(100, 90, 80) }, new(1000) { AddRGB = new(0) }) { Repeat = true, Ease = Eases.SinInOut });
        }
    }

    private void StopPulseAll()
    {
        foreach (var s in Stacks)
        {
            ClearNodeTweens(ref Tweens, s);
            s.SetAddRGB(0);
        }
    }

    #endregion

    #region Configs

    public class ChakraBarConfig : CounterWidgetConfig
    {
        public Vector2 Position = new(0);
        public float Scale = 1f;
        public float Angle;
        public AddRGB GemColor = new(108, -25, -100);
        public ColorRGB FrameColor = new(100);
        public bool HideEmpty;
        public CounterPulse Pulse = AtMax;

        public ChakraBarConfig(WidgetConfig widgetConfig)
        {
            var config = widgetConfig.ChakraBarCfg;

            if (config == null) return;

            Position = config.Position;
            Scale = config.Scale;
            Angle = config.Angle;
            GemColor = config.GemColor;
            FrameColor = config.FrameColor;
            HideEmpty = config.HideEmpty;

            Pulse = config.Pulse;
            AsTimer = config.AsTimer;
            TimerSize = config.TimerSize;
            InvertTimer = config.InvertTimer;
        }

        public ChakraBarConfig() { }
    }

    public override CounterWidgetConfig GetConfig => Config;

    public ChakraBarConfig Config = null!;

    public override void InitConfigs() => Config = new(Tracker.WidgetConfig);

    public override void ResetConfigs() => Config = new();

    public AddRGB ColorOffset = new(-66, -1, 85);
    public override void ApplyConfigs()
    {
        var angle = Config.Angle * 0.0174532925199433f;
        var flipFactor = Math.Abs(Config.Angle) >= 90 ? -1 : 1;

        WidgetRoot.SetPos(Config.Position)
                  .SetScale(Config.Scale)
                  .SetRotation(angle);
        SocketPlate.SetMultiply(Config.FrameColor).SetScale(flipFactor);
        StackContainer.SetAddRGB(Config.GemColor + ColorOffset);

        for (var i = 0; i < Tracker.CurrentData.MaxCount; i++) Pearls[i].SetRotation(-angle);
    }

    public override void DrawUI(ref WidgetConfig widgetConfig, ref UpdateFlags update)
    {
        Heading("Layout");
        PositionControls("Position", ref Config.Position, ref update);
        ScaleControls("Scale", ref Config.Scale, ref update);
        FloatControls("Angle", ref Config.Angle, -180, 180, 1f, ref update);

        Heading("Colors");
        ColorPickerRGB("Pearl Color", ref Config.GemColor, ref update);
        ColorPickerRGB("Frame Tint", ref Config.FrameColor, ref update);

        Heading("Behavior");
        if (ToggleControls("Hide Empty", ref Config.HideEmpty, ref update))
        {
            if (Config.HideEmpty && Tracker.CurrentData.Count == 0) PlateVanish();
            if (!Config.HideEmpty && WidgetRoot.Alpha < 255) PlateAppear();
        }

        RadioControls("Pulse", ref Config.Pulse, new() { Never, AtMax, Always }, new() { "Never", "At Maximum", "Always" }, ref update);

        if (ToggleControls($"Use as {Tracker.TermGauge}", ref Config.AsTimer, ref update)) update |= Reset;
        if (Config.AsTimer)
        {
            if (ToggleControls($"Invert {Tracker.TermGauge}", ref Config.InvertTimer, ref update)) update |= Reset;
            if (IntControls($"{Tracker.TermGauge} Size", ref Config.TimerSize, 1, 20, 1, ref update)) update |= Reset;
        }

        if (update.HasFlag(Save)) ApplyConfigs();
        widgetConfig.ChakraBarCfg = Config;
    }

    #endregion

    public ChakraBar(Tracker tracker) : base(tracker) { }
}

public partial class WidgetConfig
{
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] public ChakraBarConfig? ChakraBarCfg { get; set; }
}
