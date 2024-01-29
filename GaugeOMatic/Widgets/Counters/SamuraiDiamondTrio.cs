using CustomNodes;
using GaugeOMatic.CustomNodes.Animation;
using GaugeOMatic.Trackers;
using GaugeOMatic.Windows;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Numerics;
using static CustomNodes.CustomNode.CustomNodeFlags;
using static CustomNodes.CustomNodeManager;
using static GaugeOMatic.CustomNodes.Animation.Tween.Eases;
using static GaugeOMatic.Utility.Color;
using static GaugeOMatic.Widgets.CounterWidgetConfig.CounterPulse;
using static GaugeOMatic.Widgets.SamuraiDiamondTrio;
using static GaugeOMatic.Widgets.WidgetTags;
using static GaugeOMatic.Widgets.WidgetUI;
using static GaugeOMatic.Windows.UpdateFlags;
#pragma warning disable CS8618

namespace GaugeOMatic.Widgets;

public sealed unsafe class SamuraiDiamondTrio : CounterWidget
{
    public SamuraiDiamondTrio(Tracker tracker) : base(tracker) { }

    public override WidgetInfo WidgetInfo => GetWidgetInfo;

    public static WidgetInfo GetWidgetInfo => new() 
    {
        DisplayName = "Meditation Gems",
        Author = "ItsBexy",
        Description = "A counter imitating the Meditation Stack display on Samurai's Kenki Gauge. Appropriate for anything that stacks up to exactly 3.",
        WidgetTags = Counter | HasFixedCount | Replica,
        FixedCount = 3
    };

    public override CustomPartsList[] PartsLists { get; } = {
        new("ui/uld/JobHudSAM0.tex",
            new(0, 176, 102, 62),
            new(66, 78, 24, 20),
            new(66, 98, 46, 46),
            new(116, 150, 190, 26))}; // gem pulsar

    #region Nodes

    public List<CustomNode> Gems = new();
    public List<CustomNode> Glows = new();
    public List<CustomNode> Glows2 = new();
    public List<CustomNode> Pulsars = new();

    public override CustomNode BuildRoot()
    {
        Max = 3;
        return new CustomNode(ImageNodeFromPart(0, 0),
                              BuildStack(16, 23),
                              BuildStack(39, 17),
                              BuildStack(62, 23)).SetOrigin(51, 31);
    }

    private CustomNode BuildStack(float x, float y)
    {
        var gem = ImageNodeFromPart(0,1).SetOrigin(12,10);
        var glow = ImageNodeFromPart(0, 2).SetOrigin(23, 23).SetPos(-11, -13).SetAlpha(0).SetImageFlag(32)
                                          .DefineTimeline(new(0) { Alpha = 0, Scale = 1 }, new(150) { Alpha = 255, Scale = 1 }, new(300) { Alpha = 0, Scale = 1 });
        var glow2 = ImageNodeFromPart(0, 2).SetOrigin(23, 23)
                                           .SetPos(-11, -13)
                                           .RemoveFlags(SetVisByAlpha)
                                           .SetFlags(0)
                                           .SetAlpha(0)
                                           .SetImageFlag(32);

        var pulsar = ImageNodeFromPart(0, 3).SetOrigin(95,13)
                                            .SetPos(-82, -3)
                                            .SetAlpha(0)
                                            .SetScale(0.5f,0.4f)
                                            .SetImageFlag(32)
                                            .DefineTimeline(new(0) { Alpha = 0, ScaleX = 0, ScaleY = 0.4f },
                                                            new(150) { Alpha = 255, ScaleX = 0.25f, ScaleY = 0.35f },
                                                            new(300) { Alpha = 0, ScaleX = 0.5f, ScaleY = 0.3f });
        
        Gems.Add(gem);
        Glows.Add(glow);
        Glows2.Add(glow2);
        Pulsars.Add(pulsar);


        return new CustomNode(CreateResNode(), gem, glow,glow2, pulsar).SetPos(x,y);
    }

    #endregion

    #region Animations

    public override void ShowStack(int i)
    {
        if (Gems[i].Node->Color.A != 0) return;
        Glows2[i].Show();
        
        Animator += new Tween[] {
            new(Gems[i], 
                new(0) { Scale = 2, Alpha = 0 }, 
                new(150) { Scale = 1, Alpha = 255 }),
            Glows[i], 
            Pulsars[i]
        };
    }

    public override void HideStack(int i)
    {
        if (Gems[i].Node->Color.A == 0) return;
        Glows2[i].Hide();
        
        Animator += new Tween[] { 
            new(Gems[i], 
                new(0) { Scale = 1, Alpha = 255 }, 
                new(250) { Scale = 2, Alpha = 0 }),
            Glows[i], 
            Pulsars[i]
        };
    }

    private void PlateAppear() =>
        Animator += new Tween(WidgetRoot,
                              new(0) { Scale = Config.Scale * 1.65f, Alpha = 0 },
                              new(200) { Scale = Config.Scale, Alpha = 255 }) 
                              { Ease = SinInOut };

    private void PlateVanish() =>
        Animator += new Tween(WidgetRoot,
                              new(0) { Scale = Config.Scale, Alpha = 255 },
                              new(150) { Scale = Config.Scale * 0.65f, Alpha = 0 }) 
                              { Ease = SinInOut };

    #endregion

    #region UpdateFuncs

    public override void OnFirstRun(int count, int max)
    {
        for (var i = 0; i < max; i++) Gems[i].SetAlpha(i < count);
    }

    public override void OnDecreaseToMin() { if (Config.HideEmpty) PlateVanish(); }
    public override void OnIncreaseFromMin() { if (Config.HideEmpty || WidgetRoot.Alpha < 255) PlateAppear(); }

    private void PulseAll()
    {
        Animator -= "Pulse";
        for (var i = 0; i < 3; i++)
            Animator += new Tween(Glows2[i],
                                  new(0) { Alpha = 0, Scale = 0f },
                                  new(230) { Alpha = 200, Scale = 1 },
                                  new(1125) { Alpha = 0, Scale = 1.3f })
                                  { Ease = Linear, Repeat = true, Label = "Pulse" };
    }

    private void StopPulseAll()
    {
        Animator -= "Pulse";
        for (var i = 0; i < 3; i++)
            Animator += new Tween(Glows2[i],
                                  new(0) { Alpha = Glows2[i].Alpha, Scale = Glows2[i].ScaleX },
                                  new(500) { Alpha = 0, Scale = 1.3f })
                                  { Ease = Linear, Repeat = false, Label = "Pulse" };
    }

    public bool CheckPulse(int i) => i > 0 && (Config.Pulse == Always || (Config.Pulse == AtMax && i == 3));

    public bool Pulsing;
    public override void PostUpdate(int i)
    {
        var checkPulse = CheckPulse(i);
        if (!Pulsing && checkPulse) PulseAll();
        else if (Pulsing && !checkPulse) StopPulseAll();
        Pulsing = checkPulse;
    }

    #endregion

    #region Configs

    public class SamuraiDiamondConfig : CounterWidgetConfig
    {
        public Vector2 Position;
        public float Scale = 1;
        public AddRGB GemColor = new(0, 0, 0);
        public bool HideEmpty;
        public CounterPulse Pulse = AtMax;

        public SamuraiDiamondConfig(WidgetConfig widgetConfig)
        {
            var config = widgetConfig.SamuraiDiamondCfg;

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

        public SamuraiDiamondConfig() { }
    }

    public override CounterWidgetConfig GetConfig => Config;

    public SamuraiDiamondConfig Config;

    public override void InitConfigs() => Config = new(Tracker.WidgetConfig);

    public override void ResetConfigs() => Config = new();

    public override void ApplyConfigs()
    {
        WidgetRoot.SetPos(Config.Position);
        WidgetRoot.SetScale(Config.Scale);

        for (var i = 0; i < 3; i++)
        {
            Gems[i].SetAddRGB(Config.GemColor);
            Glows[i].SetAddRGB(Config.GemColor);
            Glows2[i].SetAddRGB(Config.GemColor);
            Pulsars[i].SetAddRGB(Config.GemColor);
        }
    }

    public override void DrawUI(ref WidgetConfig widgetConfig, ref UpdateFlags update)
    {
        PositionControls("Position", ref Config.Position, ref update);
        ScaleControls("Scale", ref Config.Scale, ref update);
        ColorPickerRGB("Color Modifier", ref Config.GemColor, ref update);

        if (ToggleControls("Hide Empty", ref Config.HideEmpty, ref update))
        {
            if (Config.HideEmpty && Tracker.CurrentData.Count == 0) PlateVanish();
            if (!Config.HideEmpty && WidgetRoot.Alpha < 255) PlateAppear();
        }

        RadioControls("Pulse", ref Config.Pulse, new() { Never, AtMax, Always }, new() { "Never", "At Maximum", "Always" }, ref update);

        if (update.HasFlag(Save)) ApplyConfigs();
        widgetConfig.SamuraiDiamondCfg = Config;
    }

    #endregion
}

public partial class WidgetConfig
{
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] public SamuraiDiamondConfig? SamuraiDiamondCfg { get; set; }
}
