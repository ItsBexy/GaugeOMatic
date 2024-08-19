using CustomNodes;
using GaugeOMatic.CustomNodes.Animation;
using GaugeOMatic.Trackers;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using static CustomNodes.CustomNode;
using static CustomNodes.CustomNode.CustomNodeFlags;
using static CustomNodes.CustomNodeManager;
using static GaugeOMatic.CustomNodes.Animation.KeyFrame;
using static GaugeOMatic.Utility.Color;
using static GaugeOMatic.Widgets.FinishIcon;
using static GaugeOMatic.Widgets.WidgetTags;
using static GaugeOMatic.Widgets.WidgetUI;
using static GaugeOMatic.Widgets.WidgetUI.WidgetUiTab;
using static System.Math;

#pragma warning disable CS8618

namespace GaugeOMatic.Widgets;

[WidgetName("Finish Icon")]
[WidgetDescription("A widget recreating DNC's Standard Finish timer.")]
[WidgetAuthor("ItsBexy")]
[WidgetTags(State)]
[WidgetUiTabs(Layout | Colors)]
public sealed unsafe class FinishIcon : StateWidget
{
    public FinishIcon(Tracker tracker) : base(tracker) { }

    public override CustomPartsList[] PartsLists { get; } =
    {
        new("ui/uld/JobHudDNC0.tex",
            new(168, 136, 56, 56),
            new(224, 136, 56, 56))
    };

    #region Nodes

    public CustomNode Symbol;

    public override Bounds GetBounds() => Symbol;

    public override CustomNode BuildContainer()
    {
        Symbol = ImageNodeFromPart(0, 0).RemoveFlags(SetVisByAlpha)
                                        .SetAlpha(0)
                                        .SetOrigin(28, 28);

        BeginRotation();

        return new CustomNode(CreateResNode(), Symbol).SetOrigin(28, 28);
    }

    private void BeginRotation()
    {
        StopRotation();
        var rpm = Abs(Config.Speed);
        if (rpm > 0.005f)
        {
            var startAngle = Symbol.Node->Rotation % 6.283185f;
            var endAngle = startAngle + (Config.Speed >= 0 ? 6.283185f : -6.283185f);
            Animator += new Tween(Symbol,
                                  new(0) { Rotation = startAngle },
                                  new((int)(60000f / rpm)) { Rotation = endAngle })
                                  { Repeat = true, Label = "RotationTween" };
        }
    }

    private void StopRotation() => Animator -= "RotationTween";

    #endregion

    #region Animations

    #endregion

    #region UpdateFuncs

    public override void PostUpdate() { if (!Symbol.Visible) StopRotation(); }

    public override void OnFirstRun(int current)
    {
        Symbol.SetAddRGB(Config.Colors.ElementAtOrDefault(Tracker.CurrentData.State));
        Symbol.SetAlpha(current > 0).SetVis(current > 0);
    }

    public override void Activate(int current)
    {
        var color = Config.Colors.ElementAtOrDefault(current);

        Symbol.Show();
        Animator -= "SymbolAlpha";
        Animator += new Tween(Symbol,
                              new(0) { Alpha = 0, AddRGB = color },
                              new(200) { Alpha = 255, AddRGB = color })
                              { Label = "SymbolAlpha" };

        BeginRotation();
    }

    public override void Deactivate(int previous)
    {
        Animator -= "SymbolAlpha";
        Animator += new Tween(Symbol, Visible[0], Hidden[200]) { Complete = () => Symbol.Hide(), Label = "SymbolAlpha" };
    }

    public override void StateChange(int current, int previous)
    {
        Animator += new Tween(Symbol,
                              new(0) { AddRGB = Config.Colors.ElementAtOrDefault(previous) },
                              new(200) { AddRGB = Config.Colors.ElementAtOrDefault(current) });
    }

    #endregion

    #region Configs

    public class FinishIconConfig : WidgetTypeConfig
    {
        public List<AddRGB> Colors = new();
        [DefaultValue(11.5f)] public float Speed = 11.5f;
        public bool Tech;

        public FinishIconConfig(WidgetConfig widgetConfig) : base(widgetConfig.FinishIconCfg)
        {
            var config = widgetConfig.FinishIconCfg;

            if (config == null) return;

            Colors = config.Colors;
            Speed = config.Speed;
            Tech = config.Tech;
        }

        public FinishIconConfig() { }

        public void FillColorList(int maxState)
        {
            while (Colors.Count <= maxState) Colors.Add(new(0));
        }
    }

    private FinishIconConfig config;

    public override FinishIconConfig Config => config;

    public override void InitConfigs()
    {
        config = new(Tracker.WidgetConfig);
        Config.FillColorList(Tracker.CurrentData.MaxState);
    }

    public override void ResetConfigs()
    {
        config = new();
        Config.FillColorList(Tracker.CurrentData.MaxState);
    }

    public override void ApplyConfigs()
    {
        WidgetContainer.SetPos(Config.Position)
                  .SetScale(Config.Scale);

        Symbol.SetAddRGB(Config.Colors.ElementAtOrDefault(Tracker.CurrentData.State))
              .SetPartId(Config.Tech?1:0);

        if (Symbol.Visible) { BeginRotation(); }
    }

    public override void DrawUI()
    {
        base.DrawUI();
        switch (UiTab)
        {
            case Layout:
                FloatControls("Speed", ref Config.Speed, -200, 200, 1f);
                RadioControls("Icon", ref Config.Tech, new() { false, true }, new() { "Standard", "Technical" });
                break;
            case Colors:
                var maxState = Tracker.CurrentData.MaxState;

                for (var i = 1; i <= maxState; i++)
                {
                    var color = Config.Colors[i];
                    var label = $"{Tracker.StateNames[i]}";
                    if (ColorPickerRGB(label, ref color)) Config.Colors[i] = color;
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
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] public FinishIconConfig? FinishIconCfg { get; set; }
}
