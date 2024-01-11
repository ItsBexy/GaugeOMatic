using GaugeOMatic.Trackers;
using GaugeOMatic.Windows;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Numerics;
using static CustomNodes.CustomNodeManager;
using static CustomNodes.CustomNodeManager.Tween;
using static GaugeOMatic.Utility.Color;
using static GaugeOMatic.Widgets.CounterWidgetConfig.CounterPulse;
using static GaugeOMatic.Widgets.ManaDiamond;
using static GaugeOMatic.Widgets.WidgetTags;
using static GaugeOMatic.Widgets.WidgetUI;
using static GaugeOMatic.Windows.UpdateFlags;

namespace GaugeOMatic.Widgets;

public sealed unsafe class ManaDiamond : CounterWidget
{
    public override WidgetInfo WidgetInfo => GetWidgetInfo;

    public static WidgetInfo GetWidgetInfo => new()
    {
        DisplayName = "Mana Diamonds",
        Author = "ItsBexy",
        Description = "A recreation of Red Mage's Mana Stack counter",
        WidgetTags = Counter | MultiComponent | Replica,
        KeyText = "BL4"
    };

    public override CustomPartsList[] PartsLists { get; } = {
        new("ui/uld/JobHudRDM0.tex",
            new(81, 239, 30, 40),
            new(81, 279, 30, 40),
            new(114, 258, 60, 60),
            new(0, 323, 45, 57),
            new(45, 323, 26, 57),
            new(71, 323, 45, 57),
            new(0, 323, 32, 57),
            new(84, 323, 32, 57))
    };

    #region Nodes

    public CustomNode SocketPlate;
    public CustomNode StackContainer;

    public List<CustomNode> Stacks = new();
    public List<CustomNode> Pulses = new();
    public List<CustomNode> Halos = new();
    public List<CustomNode> Gems = new();
    public List<CustomNode> Glows = new();
    public List<CustomNode> GemContainers = new();

    public override CustomNode BuildRoot()
    {
        var count = Config.AsTimer ? Config.TimerSize : Tracker.GetCurrentData().MaxCount;

        SocketPlate = BuildSocketPlate(count, out var size);
        StackContainer = BuildStacks(count);

        return new CustomNode(CreateResNode(), SocketPlate, StackContainer).SetOrigin(size/2,30.5f);
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
        Stacks = new List<CustomNode>();
        for (var i = 0; i < count; i++)
        {
            Pulses.Add(ImageNodeFromPart(0, 1).SetOrigin(15, 18).SetAlpha(0).Hide().SetPos(17 + (26 * i), 10));
            Halos.Add(ImageNodeFromPart(0, 2).SetPos(-15,-10).SetOrigin(30,30).SetAlpha(0));
            Gems.Add(ImageNodeFromPart(0, 0).SetOrigin(15,20));
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
        Tweens.Add(new(Halos[i],
                       new(0){Scale=1,Alpha=0},
                       new(150){Scale=1.2f,Alpha=200},
                       new(360) {Scale=0,Alpha=0}));

        Tweens.Add(new(Gems[i],
                       new(0){Scale = 2,Alpha=0},
                       new(166){Scale = 1,Alpha=255}));

        Tweens.Add(new(Gems[i],
                       new(0) { AddRGB = new(0) },
                       new(150) { AddRGB = new(150) },
                       new(360) { AddRGB = new(0) }) 
                       { Ease = Eases.SinInOut });

        Tweens.Add(new(Glows[i],
                       new(0) { Scale = 0, Alpha = 0 },
                       new(160) { Scale = 1.8f, Alpha = 200 },
                       new(250) { Scale = 2.2f, Alpha = 0 }));
    }

    public override void HideStack(int i)
    {
        Pulses[i].Hide();
        Tweens.Add(new(Gems[i], 
                       new(0) { Scale = 1, Alpha = 255 },
                       new(166) { Scale = 2, Alpha = 0 }));
    
        Tweens.Add(new(Glows[i],
                       new(0){Scale=1.8f,Alpha=0},
                       new(160){ Scale= 1.8f,Alpha=200 },
                       new(250){ Scale= 2.5f,Alpha=0 }));
    }

    private void PlateAppear() =>
        Tweens.Add(new(WidgetRoot,
                       new(0) { Scale = Config.Scale * 1.65f, Alpha = 0 },
                       new(200) { Scale = Config.Scale, Alpha = 255 })
                       { Ease = Eases.SinInOut });

    private void PlateVanish() =>
        Tweens.Add(new(WidgetRoot,
                       new(0) { Scale = Config.Scale, Alpha = 255 },
                       new(150) { Scale=Config.Scale*0.65f, Alpha = 0 })
                       { Ease = Eases.SinInOut });

    #endregion

    #region UpdateFuncs

    public override string? SharedEventGroup => null;

    public override void OnFirstRun(int count, int max)
    {
        for (var i = 0; i < count; i++) Gems[i].SetAlpha(255);
        for (var i = count; i < max; i++) Gems[i].SetAlpha(0);
    }

    private void PulseAll()
    {
        ClearLabelTweens(ref Tweens,"Pulse");
        for (var i = 0; i < Stacks.Count; i++)
        {
            Tweens.Add(new(Stacks[i][0],
                           new(0) { Scale = 0, Alpha = 0 },
                           new(390) { Scale = 0f, Alpha = 0 },
                           new(870) { Scale = 1.4f, Alpha = 152 },
                           new(1290) { Scale = 1.8f, Alpha = 0 })
                           { Repeat = true, Ease = Eases.SinInOut,Label="Pulse" });

            Tweens.Add(new(Stacks[i][1],
                           new(0) { AddRGB = new(0) },
                           new(870) { AddRGB = new(150) },
                           new(1290) { AddRGB = new(0) })
                           { Repeat = true, Ease = Eases.SinInOut, Label = "Pulse" });
        }
    }

    private void StopPulseAll()
    {
        ClearLabelTweens(ref Tweens, "Pulse");
        for (var i = 0; i < Stacks.Count; i++)
        {
            Tweens.Add(new(Stacks[i][1], new(0, Stacks[i][1]), new(150) { AddRGB = 0 }) { Label = "Pulse" });
            Tweens.Add(new(Stacks[i][0], new(0, Stacks[i][0]), new(150) { Alpha = 0}) { Label = "Pulse" });
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
    public override void OnIncreaseFromMin() { if (Config.HideEmpty || WidgetRoot.Alpha < 255) PlateAppear(); }

    #endregion

    #region Configs

    public class ManaDiamondConfig : CounterWidgetConfig
    {
        public Vector2 Position = new(0);
        public float Scale = 1f;
        public AddRGB GemColor = new(65, -120, -120);
        public bool HideEmpty;
        public CounterPulse Pulse = AtMax;

        public ManaDiamondConfig(WidgetConfig widgetConfig)
        {
            var config = widgetConfig.ManaDiamondCfg;

            if (config == null) return;

            Position = config.Position;
            Scale = config.Scale;
            GemColor = config.GemColor;
            HideEmpty = config.HideEmpty;

            Pulse=config.Pulse;
            AsTimer = config.AsTimer;
            TimerSize = config.TimerSize;
            InvertTimer = config.InvertTimer;
        }

        public ManaDiamondConfig() { }
    }

    public override CounterWidgetConfig GetConfig => Config;

    public ManaDiamondConfig Config = null!;

    public override void InitConfigs() => Config = new(Tracker.WidgetConfig);

    public override void ResetConfigs() => Config = new();

    public AddRGB ColorOffset = new(-65, 120, 120);
    public override void ApplyConfigs()
    {
        WidgetRoot.SetPos(Config.Position);
        WidgetRoot.SetScale(Config.Scale);
        StackContainer.SetAddRGB(Config.GemColor + ColorOffset);
    }

    public override void DrawUI(ref WidgetConfig widgetConfig, ref UpdateFlags update)
    {
        Heading("Layout");
        PositionControls("Position", ref Config.Position, ref update);
        ScaleControls("Scale", ref Config.Scale, ref update);

        Heading("Color");
        ColorPickerRGB("Gem Color", ref Config.GemColor, ref update);

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
        widgetConfig.ManaDiamondCfg = Config;
    }

    #endregion

    public ManaDiamond(Tracker tracker) : base(tracker) { }
}

public partial class WidgetConfig
{
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] public ManaDiamondConfig? ManaDiamondCfg { get; set; }
}
