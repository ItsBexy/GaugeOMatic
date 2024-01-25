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
using static GaugeOMatic.CustomNodes.Animation.KeyFrame;
using static GaugeOMatic.Utility.Color;
using static GaugeOMatic.Widgets.FinishIcon;
using static GaugeOMatic.Widgets.WidgetTags;
using static GaugeOMatic.Widgets.WidgetUI;
using static GaugeOMatic.Windows.UpdateFlags;
using static System.Math;

#pragma warning disable CS8618

namespace GaugeOMatic.Widgets;

public sealed unsafe class FinishIcon : StateWidget
{
    public FinishIcon(Tracker tracker) : base(tracker) { }

    public override WidgetInfo WidgetInfo => GetWidgetInfo;

    public static WidgetInfo GetWidgetInfo => new()
    {
        DisplayName = "Finish Icon",
        Author = "ItsBexy",
        Description = "A widget recreating DNC's Standard Finish timer",
        WidgetTags = State
    };

    public override CustomPartsList[] PartsLists { get; } =
    {
        new("ui/uld/JobHudDNC0.tex",
            new(168,136,56,56),
            new(224,136,56,56))
    };

    #region Nodes

    public CustomNode Symbol;

    public override CustomNode BuildRoot()
    {
        Symbol = ImageNodeFromPart(0, 0).RemoveFlags(SetVisByAlpha)
                                        .SetAlpha(0)
                                        .SetOrigin(28,28);

        BeginRotation();

        return new CustomNode(CreateResNode(), Symbol).SetOrigin(28,28);
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
        Animator -= "ShimmerAlpha";
        Animator += new Tween(Symbol,
                              new(0) { Alpha = 0, AddRGB = color },
                              new(200) { Alpha = 255, AddRGB = color }) 
                              { Label = "ShimmerAlpha" };

        BeginRotation();
    }

    public override void Deactivate(int previous)
    {
        Animator -= "ShimmerAlpha";
        Animator += new Tween(Symbol, Visible[0], Hidden[200]) { Complete = () => Symbol.Hide(), Label = "ShimmerAlpha" };
    }

    public override void StateChange(int current, int previous)
    {
        Animator += new Tween(Symbol,
                              new(0) { AddRGB = Config.Colors.ElementAtOrDefault(previous) },
                              new(200) { AddRGB = Config.Colors.ElementAtOrDefault(current) });
    }

    #endregion

    #region Configs

    public class FinishIconConfig
    {
        public Vector2 Position = new(0);
        public float Scale = 1;
        public float Angle;
        public List<AddRGB> Colors = new();
        public float Speed = 11.5f;
        public bool Tech;

        public FinishIconConfig(WidgetConfig widgetConfig)
        {
            var config = widgetConfig.FinishIconCfg;

            if (config == null) return;

            Position = config.Position;
            Scale = config.Scale;
            Angle = config.Angle;
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

    public FinishIconConfig Config;

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
                  .SetRotation(Config.Angle * 0.0174532925199433f);
        
        Symbol.SetAddRGB(Config.Colors.ElementAtOrDefault(Tracker.CurrentData.State))
              .SetPartId((ushort)(Config.Tech?1:0));

        if (Symbol.Visible) { BeginRotation(); }
    }

    public override void DrawUI(ref WidgetConfig widgetConfig, ref UpdateFlags update)
    {
        Heading("Layout");

        PositionControls("Position", ref Config.Position, ref update);
        ScaleControls("Scale", ref Config.Scale, ref update);
        FloatControls("Angle", ref Config.Angle, -180, 180, 1f, ref update);
        FloatControls("Speed", ref Config.Speed, -200, 200, 1f, ref update);
        RadioControls("Icon", ref Config.Tech, new() { false, true }, new() { "Standard", "Technical" }, ref update);

        Heading("Color Modifier");

        var maxState = Tracker.CurrentData.MaxState;

        for (var i = 1; i <= maxState; i++)
        {
            var color = Config.Colors[i];
            var label = $"{Tracker.StateNames[i]}";
            if (ColorPickerRGB(label, ref color, ref update)) Config.Colors[i] = color;
        }

        if (update.HasFlag(Save)) ApplyConfigs();
        widgetConfig.FinishIconCfg = Config;
    }

    #endregion

}

public partial class WidgetConfig
{
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] public FinishIconConfig? FinishIconCfg { get; set; }
}
