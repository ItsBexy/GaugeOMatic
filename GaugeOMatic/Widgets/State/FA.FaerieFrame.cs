using CustomNodes;
using GaugeOMatic.CustomNodes.Animation;
using GaugeOMatic.Trackers;
using GaugeOMatic.Windows;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using static CustomNodes.CustomNodeManager;
using static GaugeOMatic.CustomNodes.Animation.Tween.Eases;
using static GaugeOMatic.Utility.Color;
using static GaugeOMatic.Widgets.Common.CommonParts;
using static GaugeOMatic.Widgets.FaerieFrame;
using static GaugeOMatic.Widgets.FaerieFrame.FaerieFrameConfig;
using static GaugeOMatic.Widgets.FaerieFrame.FaerieFrameConfig.FrameState;
using static GaugeOMatic.Widgets.WidgetTags;
using static GaugeOMatic.Widgets.WidgetUI;
using static GaugeOMatic.Windows.UpdateFlags;

#pragma warning disable CS8618

namespace GaugeOMatic.Widgets;

public sealed unsafe class FaerieFrame : StateWidget
{
    public FaerieFrame(Tracker tracker) : base(tracker) { }

    public override WidgetInfo WidgetInfo => GetWidgetInfo;

    public static WidgetInfo GetWidgetInfo => new()
    {
        DisplayName = "Faerie Frame",
        Author = "ItsBexy",
        Description = "A glowing border over one of the parameter bars",
        WidgetTags = MultiComponent | State | Replica,
        MultiCompData = new("FA", "Faerie Gauge Replica", 1)
    };

    public override CustomPartsList[] PartsLists { get; } = { SCH1 };

    #region Nodes

    public CustomNode Wings;
    public CustomNode Frame;
    public CustomNode SeraphContainer;
    public CustomNode FeatherWing;
    public CustomNode Seraph;
    public CustomNode FaerieContainer;
    public CustomNode Wings2;
    public CustomNode Faerie;
    public CustomNode FancyWingBG;
    public CustomNode FancyWingBG2;
    public CustomNode FancyWingBG3;
    public CustomNode Darts;
    public CustomNode Sparkles;

    public override CustomNode BuildRoot()
    {
        Wings = ImageNodeFromPart(0, 4).SetPos(5, 1)
                                       .SetOrigin(20, 70)
                                       .SetImageWrap(1)
                                       .SetAddRGB(20, 10, 50)
                                       .SetMultiply(80, 80, 100);

        Frame = ImageNodeFromPart(0, 0).SetPos(0, 19)
                                       .SetImageWrap(1)
                                       .SetAddRGB(20, 10, 50)
                                       .SetMultiply(80, 80, 100);

        FeatherWing = ImageNodeFromPart(0, 10).SetPos(16, 80).SetRotation(-90, true).SetAlpha(0);

        Animator += new Tween(FeatherWing,
                              new(0){ AddRGB = 0 },
                              new(375){ AddRGB = new(20,20,50) },
                              new(1000){ AddRGB = 0 })
                              { Repeat = true, Ease = SinInOut };

        Seraph = ImageNodeFromPart(0, 11).SetPos(3, 94).SetRotation(-90, true).SetAlpha(0);
        SeraphContainer = new(CreateResNode(), FeatherWing, Seraph);

        Wings2 = ImageNodeFromPart(0, 4).SetOrigin(20, 70)
                                        .SetImageWrap(1);

        Faerie = ImageNodeFromPart(0, 5).SetPos(-2, 95).SetRotation(-90, true).SetAlpha(0);

        FancyWingBG = ImageNodeFromPart(0, 9).SetPos(23,54)
                                             .SetOrigin(2,14)
                                             .SetImageWrap(2)
                                             .SetRotation(-90, true);

        FancyWingBG2 = ImageNodeFromPart(0, 8).SetPos(10,4)
                                              .SetOrigin(12, 66)
                                              .SetAlpha(0);

        FancyWingBG3 = ImageNodeFromPart(0, 3).SetPos(10, 4)
                                              .SetOrigin(12, 66).SetAlpha(0);

        Darts = ImageNodeFromPart(0, 6).SetPos(27, 18)
                                       .SetScale(0.5f)
                                       .SetOrigin(0,52)
                                       .SetImageWrap(1)
                                       .SetAddRGB(0, 200, 20)
                                       .SetAlpha(0);

        FaerieContainer = new CustomNode(CreateResNode(),Wings2,Faerie, FancyWingBG, FancyWingBG2,FancyWingBG3,Darts).SetPos(5,1).SetAlpha(0).SetOrigin(110,48);

        Sparkles = ImageNodeFromPart(0, 7).SetPos(51,26).SetScale(1.5f,1.7f).SetOrigin(14,8).SetImageWrap(1).SetImageFlag(32).SetAlpha(0);

        return new(CreateResNode(),Wings,Frame,SeraphContainer, FaerieContainer,Sparkles);
    }

    #endregion

    #region Animations

    #endregion

    #region UpdateFuncs

    public override void Update()
    {
        var current = Tracker.CurrentData.State;
        var previous = Tracker.PreviousData.State;

        if (FirstRun) { OnFirstRun(current); FirstRun = false; }
        else
        {
            var curFrameState = Config.FrameStates[current];
            var prevFrameState = Config.FrameStates[previous];

            if (curFrameState != prevFrameState)
            {
                switch (prevFrameState)
                {
                    case Blank when curFrameState == FrameState.Faerie:
                        BlankToFaerie();
                        break;
                    case Blank when curFrameState == FrameState.Seraph:
                        BlankToSeraph();
                        break;
                    case FrameState.Faerie when curFrameState == FrameState.Seraph:
                        FaerieToSeraph();
                        break;
                    case FrameState.Faerie when curFrameState == Blank:
                        FaerieToBlank();
                        break;
                    case FrameState.Seraph when curFrameState == Blank:
                        SeraphToBlank();
                        break;
                    case FrameState.Seraph when curFrameState == FrameState.Faerie:
                        SeraphToFaerie();
                        break;
                }
            }

            if (curFrameState == FrameState.Seraph) SeraphContainer.SetAddRGB(GetSeraphColor());
        }

        Frame.SetAddRGB(GetAdjustedColor());

        Animator.RunTweens();
    }

    private void FaerieToBlank()
    {
        Animator += new Tween[]
        {
            new(Wings,
                new(0) { Alpha = 0, AddRGB = 0, MultRGB = new(100, 100, 100) },
                new(125) { Alpha = 255, AddRGB = GetAdjustedColor(), MultRGB = new(80, 80, 100) }),
            new(FaerieContainer, new(0) { Alpha = 255 }, new(170) { Alpha = 0 })

        };
        Seraph.SetAlpha(0);
        FeatherWing.SetAlpha(0);
        Frame.SetMultiply(80, 80, 100);
    }

    private void SeraphToBlank()
    {
        Animator += new Tween[]
        {
            new(FeatherWing, new(0) { Scale = 1, Alpha = 255 }, new(225) { ScaleX = 1, ScaleY = 1.2f, Alpha = 0 }),
            new(Seraph, new(0) { Alpha = 255 }, new(225) { Alpha = 0 })
        };
        FaerieContainer.SetAlpha(0);
        Wings.SetAddRGB(GetAdjustedColor()).SetAlpha(255);
        Frame.SetMultiply(80, 80, 100);
    }


    private void BlankToFaerie()
    {
        Animator += new Tween[]
        {
            new(Faerie, new(0) { Alpha = 255 }, new(125) { Alpha = 0 }),
            new(FancyWingBG3, new(0) { Alpha = 255 }, new(125) { Alpha = 0 })
        };
        Wings.SetAlpha(0);
        Frame.SetMultiply(100);
        FaerieContainer.SetAddRGB(GetAdjustedColor())
                       .SetAlpha(255)
                       .SetScale(1);
    }

    private void SeraphToFaerie()
    {
        Animator += new Tween[]
        {
            new(FeatherWing, new(0) { Scale = 1, Alpha = 255 }, new(225) { ScaleX = 1, ScaleY = 1.2f, Alpha = 0 }),
            new(Seraph, new(0) { Alpha = 255 }, new(225) { Alpha = 0 }),
            new(FaerieContainer, new(0) { Alpha = 0, Scale = 1, AddRGB = 0 },
                new(100) { Alpha = 255, Scale = 1, AddRGB = GetAdjustedColor() })
        };
        Seraph.SetAlpha(0);
        FeatherWing.SetAlpha(0);
        Frame.SetMultiply(100);
        Wings.SetAlpha(0);
    }

    private void FaerieToSeraph()
    {
        Animator += new Tween[]
        {
            new(Seraph, new(0) { Alpha = 0 }, new(170) { Alpha = 255 }),
            new(FeatherWing, new(0) { Alpha = 0, Scale = 1 }, new(170) { Alpha = 255, Scale = 1 }),
            new(FaerieContainer, new(0) { Alpha = 255, Scale = 1, AddRGB = 0 }, new(265) { Alpha = 0, Scale = 1.2f, AddRGB = new(0, 20, 20) })
        };
        Wings.SetAlpha(0);
        Frame.SetMultiply(100);
        SeraphContainer.SetAddRGB(GetSeraphColor());
    }

    private void BlankToSeraph()
    {
        Animator += new Tween[]
        {
            new(Seraph, new(0) { Alpha = 0 }, new(170) { Alpha = 255 }),
            new(FeatherWing, new(0) { Alpha = 0, Scale = 1 }, new(170) { Alpha = 255, Scale = 1 })
        };
        FaerieContainer.SetAlpha(0);
        Wings.SetAlpha(0);
        Frame.SetMultiply(100);
    }

    public override void OnFirstRun(int current)
    {
        var curFrameState = Config.FrameStates[current];

        Wings.SetAddRGB(GetAdjustedColor()).SetAlpha(curFrameState == Blank);
        Frame.SetMultiply(curFrameState == Blank? new ColorRGB(80,80,100) : 100);

        FaerieContainer.SetAddRGB(GetAdjustedColor())
                       .SetAlpha(curFrameState == FrameState.Faerie).SetScale(1);

        Seraph.SetAlpha(curFrameState == FrameState.Seraph);
        FeatherWing.SetAlpha(curFrameState == FrameState.Seraph).SetScale(1);
        SeraphContainer.SetAddRGB(GetSeraphColor());
    }

    public override void Activate(int current) { }
    public override void Deactivate(int previous) { }
    public override void StateChange(int current, int previous) { }

    #endregion

    #region Configs

    public class FaerieFrameConfig
    {
        public enum FrameState { Blank, Faerie, Seraph }

        public Vector2 Position;
        public float Scale = 1;
        public List<FrameState> FrameStates = new();
        public List<AddRGB> FrameColors = new();
        public List<AddRGB> SeraphColors = new();

        public FaerieFrameConfig(WidgetConfig widgetConfig)
        {
            var config = widgetConfig.FaerieFrameCfg;

            if (config == null) return;

            Position = config.Position;
            Scale = config.Scale;
            FrameStates = config.FrameStates;
            FrameColors = config.FrameColors;
            SeraphColors = config.SeraphColors;
        }

        public FaerieFrameConfig() { }

        public void FillLists(int maxState)
        {
            while (FrameStates.Count <= maxState) FrameStates.Add(FrameStates.Count switch { < 1 => Blank, < 2 => FrameState.Faerie, _ => FrameState.Seraph });
            while (FrameColors.Count <= maxState) FrameColors.Add(FrameColors.Count < 1 ? new(20-69, 10-64, 50-103) : new(-69, -64, -103));
            while (SeraphColors.Count <= maxState) SeraphColors.Add(new(-128,-47,94));
        }
    }

    public FaerieFrameConfig Config;

    public override void InitConfigs()
    {
        Config = new(Tracker.WidgetConfig);
        Config.FillLists(Tracker.CurrentData.MaxState);
    }

    public override void ResetConfigs()
    {
        Config = new();
        Config.FillLists(Tracker.CurrentData.MaxState);
    }

    public AddRGB GetAdjustedColor() => Config.FrameColors.ElementAtOrDefault(Tracker.CurrentData.State) + new AddRGB(69, 64, 103);
    public AddRGB GetSeraphColor() => Config.SeraphColors.ElementAtOrDefault(Tracker.CurrentData.State) + new AddRGB(128, 47, -94);

    public override void ApplyConfigs()
    {
        WidgetRoot.SetPos(Config.Position)
                  .SetScale(Config.Scale);

        Frame.SetAddRGB(GetAdjustedColor());

        OnFirstRun(Tracker.CurrentData.State);
    }


    public override void DrawUI(ref WidgetConfig widgetConfig, ref UpdateFlags update)
    {
        Heading("Layout");
        PositionControls("Position", ref Config.Position, ref update);
        ScaleControls("Scale", ref Config.Scale, ref update);

        var maxState = Tracker.CurrentData.MaxState;

        for (var i = 0; i <= maxState; i++)
        {
            Heading($"{Tracker.StateNames[i]}");

            var frameState = Config.FrameStates[i];
            var frameColor = Config.FrameColors[i];
            var seraphColor = Config.SeraphColors[i];
            if (RadioControls($"Appearance##appearance{i}", ref frameState, new(){Blank,FrameState.Faerie, FrameState.Seraph }, new() {"Blank","Faerie","Seraph"}, ref update)) Config.FrameStates[i] = frameState;
            if (ColorPickerRGB($"Frame Tint##framecolor{i}", ref frameColor, ref update)) Config.FrameColors[i] = frameColor;

            if (frameState == FrameState.Seraph)
            {
                if (ColorPickerRGB($"Seraph Color##seraphcolor{i}", ref seraphColor, ref update)) Config.SeraphColors[i] = seraphColor;
            }
        }

        if (update.HasFlag(Save)) ApplyConfigs();
        widgetConfig.FaerieFrameCfg = Config;
    }

    #endregion
}

public partial class WidgetConfig
{
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] public FaerieFrameConfig? FaerieFrameCfg { get; set; }
}
