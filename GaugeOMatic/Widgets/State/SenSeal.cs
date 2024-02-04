using CustomNodes;
using GaugeOMatic.CustomNodes.Animation;
using GaugeOMatic.Trackers;
using GaugeOMatic.Windows;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using static CustomNodes.CustomNode.CustomNodeFlags;
using static CustomNodes.CustomNodeManager;
using static GaugeOMatic.CustomNodes.Animation.Tween.Eases;
using static GaugeOMatic.Utility.Color;
using static GaugeOMatic.Widgets.Common.CommonParts;
using static GaugeOMatic.Widgets.SenSeal;
using static GaugeOMatic.Widgets.WidgetTags;
using static GaugeOMatic.Widgets.WidgetUI;
using static GaugeOMatic.Windows.UpdateFlags;

#pragma warning disable CS8618

namespace GaugeOMatic.Widgets;

public sealed unsafe class SenSeal : StateWidget
{
    public SenSeal(Tracker tracker) : base(tracker) { }

    public override WidgetInfo WidgetInfo => GetWidgetInfo;

    public static WidgetInfo GetWidgetInfo => new()
    {
        DisplayName = "Sen Seal",
        Author = "ItsBexy",
        Description = "A widget recreating SAM's Sen Seals",
        WidgetTags = State
    };

    public override CustomPartsList[] PartsLists { get; } = { SAM1Parts };

    #region Nodes

    public CustomNode InactiveSeal;
    public CustomNode ActiveSeal;
    public CustomNode WhiteHalo;
    public CustomNode SealGlow;
    public CustomNode SparkleGlow;
    public CustomNode Sparkle;
    public CustomNode Kanji;
    public CustomNode KanjiGlow;

    public CustomNode Glow2Container;
    public CustomNode SealPulse;
    public CustomNode KanjiPulse;

    public override CustomNode BuildRoot()
    {
        InactiveSeal = ImageNodeFromPart(0, 3);

        ActiveSeal = ImageNodeFromPart(0, 0).SetOrigin(40,40)
                                            .SetImageWrap(1)
                                            .SetAlpha(0);

        WhiteHalo = ImageNodeFromPart(0, 19).SetPos(12, 12)
                                            .SetOrigin(27,27)
                                            .SetAlpha(0);

        SealGlow = ImageNodeFromPart(0, 15).SetOrigin(40, 40)
                                           .SetImageWrap(1)
                                           .SetAlpha(0);

        SparkleGlow = ImageNodeFromPart(0, 10).SetPos(12, 0)
                                              .SetOrigin(29, 40)
                                              .SetAlpha(0)
                                              .SetImageFlag(32);

        Sparkle = ImageNodeFromPart(0, 9).SetPos(12, 0)
                                         .SetOrigin(29, 40)
                                         .SetAlpha(0)
                                         .SetImageFlag(32);

        Kanji = ImageNodeFromPart(0, 6).SetOrigin(40, 40)
                                       .SetImageWrap(1)
                                       .SetAlpha(0)
                                       .RemoveFlags(SetVisByAlpha);

        KanjiGlow = ImageNodeFromPart(0, 20).SetOrigin(40, 40)
                                            .SetImageWrap(1)
                                            .SetImageFlag(32)
                                            .SetAlpha(0)
                                            .RemoveFlags(SetVisByAlpha);

        SealPulse = ImageNodeFromPart(0, 15).SetOrigin(40, 40)
                                            .SetImageWrap(1)
                                            .SetImageFlag(32)
                                            .SetAlpha(0);

        KanjiPulse = ImageNodeFromPart(0, 20).SetOrigin(40, 40)
                                             .SetImageWrap(1)
                                             .SetImageFlag(32)
                                             .SetAlpha(0)
                                             .RemoveFlags(SetVisByAlpha);

        Glow2Container = new(CreateResNode(),SealPulse,KanjiPulse);

        return new CustomNode(CreateResNode(),
                              InactiveSeal,
                              ActiveSeal,
                              WhiteHalo,
                              SealGlow,
                              SparkleGlow,
                              Sparkle,
                              Kanji,
                              KanjiGlow,
                              Glow2Container).SetOrigin(28, 28);
    }

    #endregion

    #region Animations

    #endregion

    #region UpdateFuncs

    public override void OnFirstRun(int current)
    {
        if (current > 0)
        {
            ActiveSeal.SetAlpha(255);
            Kanji.SetAlpha(216);
            WidgetRoot.SetAddRGB(Config.Colors.ElementAtOrDefault(current));
            StartPulse();
        }
    }

    public override void Activate(int current)
    {
        Animator += new Tween[]
        {
            new(WidgetRoot,
                new(0, WidgetRoot),
                new(300) { AddRGB = Config.Colors.ElementAtOrDefault(current) }),

            new(ActiveSeal,
                new(0) { Scale = 1.5f, Alpha = 0 },
                new(150) { Scale = 1, Alpha = 255 }),

            new(SealGlow,
                new(0) { Scale = 1, Alpha = 0 },
                new(300) { Scale = 1.2f, Alpha = 201 },
                new(660) { Scale = 1.4f, Alpha = 0 }),

            new(SparkleGlow,
                new(0) { Scale = 1, Alpha = 0 },
                new(300) { Scale = 1, Alpha = 255 },
                new(460) { Scale = 1.2f, Alpha = 50 },
                new(660) { Scale = 1.2f, Alpha = 0 }),

            new(Sparkle,
                new(0) { Scale = 1, Alpha = 0 },
                new(300) { Scale = 1, Alpha = 255 },
                new(460) { Scale = 1.2f, Alpha = 50 },
                new(660) { Scale = 1.2f, Alpha = 0 }),

            new(Kanji,
                new(0) { Scale = 2, Alpha = 0 },
                new(150) { Scale = 1, Alpha = 255 },
                new(300) { Scale = 1.2f, Alpha = 128 },
                new(460) { Scale = 1.3f, Alpha = 128 },
                new(660) { Scale = 1, Alpha = 216 }),

            new(KanjiGlow,
                new(0) { Scale = 1, Alpha = 0 },
                new(125) { Scale = 1, Alpha = 100 },
                new(300) { Scale = 1.2f, Alpha = 0 })
        };

        StartPulse();
    }

    public override void Deactivate(int previous)
    {
        Animator += new Tween[]
        {
            new(WidgetRoot,
                new(0, WidgetRoot),
                new(300) { AddRGB = Config.Colors.ElementAtOrDefault(0) }),

            new(ActiveSeal,
                new(0) { Scale = 1, Alpha = 255, AddRGB = 0 },
                new(225) { Scale = 1.3f, Alpha = 128, AddRGB = 100 },
                new(460) { Scale = 1.3f, Alpha = 0, AddRGB = 0 }),

            new(WhiteHalo,
                new(0) { Scale = 0, Alpha = 0 },
                new(225) { Scale = 2, Alpha = 255 },
                new(460) { Scale = 2, Alpha = 0 }),

            new(Kanji,
                new(0) { Scale = 1, Alpha = 216 },
                new(225) { Scale = 1.3f, Alpha = 128 },
                new(450) { Scale = 1.3f, Alpha = 0 })
        };

        StopPulse();
    }

    private void StartPulse()
    {
        Animator -= "Pulse";
        Animator += new Tween[]
        {
            new(SealPulse,
                new(0) { Scale = 1, Alpha = 0 },
                new(500) { Scale = 1.2f, Alpha = 152 },
                new(950) { Scale = 1.4f, Alpha = 0 },
                new(1325) { Scale = 1.4f, Alpha = 0 })
                { Repeat = true, Label = "Pulse", Ease = SinInOut },
            new(KanjiPulse,
                new(0) { Scale = 1, Alpha = 0 },
                new(500) { Scale = 1, Alpha = 50 },
                new(950) { Scale = 1.2f, Alpha = 0 },
                new(1325) { Scale = 1.2f, Alpha = 0 })
                { Repeat = true, Label = "Pulse", Ease = SinInOut }
        };
    }

    private void StopPulse()
    {
        Animator -= "Pulse";
        Animator += new Tween[]
        {
            new(SealPulse,
                new(0, SealPulse),
                new(500) { Scale = 1.4f, Alpha = 0 })
                { Label = "Pulse" },
            new(KanjiPulse,
                new(0, KanjiPulse),
                new(500) { Scale = 1.2f, Alpha = 0 })
                { Label = "Pulse" }
        };
    }

    public override void StateChange(int current, int previous) => Animator += new Tween(WidgetRoot, new(0,WidgetRoot), new(300){AddRGB = Config.Colors.ElementAtOrDefault(current)});

    #endregion

    #region Configs

    public class SenSealConfig
    {
        public Vector2 Position = new(0);
        public float Scale = 1;
        public List<AddRGB> Colors = new();
        public int Seal;
        public bool Kanji = true;

        public SenSealConfig(WidgetConfig widgetConfig)
        {
            var config = widgetConfig.SenSealCfg;

            if (config == null) return;

            Position = config.Position;
            Scale = config.Scale;
            Colors = config.Colors;
            Seal = config.Seal;
            Kanji = config.Kanji;
        }

        public SenSealConfig() { }

        public void FillColorList(int maxState)
        {
            while (Colors.Count <= maxState) Colors.Add(new(0));
        }
    }

    public SenSealConfig Config;

    public override void InitConfigs()
    {
        Config = new(Tracker.WidgetConfig);
        Config.FillColorList(Tracker.CurrentData.MaxState);
    }

    public override void ResetConfigs()
    {
        Config = new();
        Config.FillColorList(Tracker.CurrentData.MaxState);
    }

    public override void ApplyConfigs()
    {
        WidgetRoot.SetPos(Config.Position)
                  .SetScale(Config.Scale)
                  .SetAddRGB(Config.Colors.ElementAtOrDefault(Tracker.CurrentData.State));

        InactiveSeal.SetPartId(3 + Config.Seal)
                    .SetAlpha(Config.Colors.ElementAtOrDefault(0).A);

        ActiveSeal.SetPartId(Config.Seal);
        SealGlow.SetPartId(15 + Config.Seal);
        SealPulse.SetPartId(15 + Config.Seal);

        Kanji.SetPartId(6 + Config.Seal)
             .SetVis(Config.Kanji);

        KanjiGlow.SetPartId(20 + Config.Seal)
                 .SetVis(Config.Kanji);

        KanjiPulse.SetPartId(20 + Config.Seal)
                  .SetVis(Config.Kanji);

        Sparkle.SetPartId(Config.Seal switch
        {
            0 => 9,
            1 => 13,
            _ => 11
        });

        SparkleGlow.SetPartId(Config.Seal switch
        {
            0 => 10,
            1 => 14,
            _ => 12
        });
    }

    public override void DrawUI(ref WidgetConfig widgetConfig, ref UpdateFlags update)
    {
        Heading("Layout");

        PositionControls("Position", ref Config.Position, ref update);
        ScaleControls("Scale", ref Config.Scale, ref update);
        RadioControls("Seal", ref Config.Seal, new() { 0,1,2 }, new() { "Setsu", "Getsu", "Ka" }, ref update);
        ToggleControls("Show Kanji", ref Config.Kanji, ref update);

        Heading("Color Modifier");

        var inactiveColor = Config.Colors[0];
        if (ColorPickerRGBA("Inactive", ref inactiveColor, ref update)) Config.Colors[0] = inactiveColor;


        var maxState = Tracker.CurrentData.MaxState;
        for (var i = 1; i <= maxState; i++)
        {
            var color = Config.Colors[i];
            var label = $"{Tracker.StateNames[i]}";
            if (ColorPickerRGB(label, ref color, ref update)) Config.Colors[i] = color;
        }

        if (update.HasFlag(Save)) ApplyConfigs();
        widgetConfig.SenSealCfg = Config;
    }

    #endregion

}

public partial class WidgetConfig
{
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] public SenSealConfig? SenSealCfg { get; set; }
}
