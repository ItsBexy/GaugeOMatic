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
using static GaugeOMatic.Widgets.ShimmerHalo;
using static GaugeOMatic.Widgets.WidgetTags;
using static GaugeOMatic.Widgets.WidgetUI;
using static GaugeOMatic.Windows.UpdateFlags;
using static System.Math;

#pragma warning disable CS8618

namespace GaugeOMatic.Widgets;

public sealed unsafe class ShimmerHalo : StateWidget
{
    public ShimmerHalo(Tracker tracker) : base(tracker) { }

    public override WidgetInfo WidgetInfo => GetWidgetInfo;

    public static WidgetInfo GetWidgetInfo => new()
    {
        DisplayName = "Shimmering Halo",
        Author = "ItsBexy",
        Description = "A revolving circular aura that appears while the tracker's condition is met.",
        WidgetTags = State
    };

    public override CustomPartsList[] PartsLists { get; } = {
        new("ui/uld/gachaeffect03.tex", new Vector4(0, 0, 256, 256), new Vector4(13, 124, 8, 8))
    };

    #region Nodes

    public CustomNode Halo;
    public CustomNode Fill;

    public override CustomNode BuildRoot()
    {
        Halo = ClippingMaskFromPart(0, 0)
               .SetOrigin(128, 128)
               .SetPos(-128, -128)
               .SetDrawFlags(0xA);

        Fill = ImageNodeFromPart(0, 1)
               .SetPos(-128, -128)
               .SetSize(256, 256)
               .RemoveFlags(SetVisByAlpha)
               .SetImageFlag(32)
               .SetDrawFlags(0xA)
               .SetAlpha(255)
               .SetImageWrap(2);

        Halo.Node->Priority = 0;
        Fill.Node->Priority = 0;

        BeginRotation();

        return new CustomNode(CreateResNode(), Fill, Halo).SetDrawFlags(0xA);
    }

    #endregion

    #region Animations

    private void BeginRotation()
    {
        StopRotation();
        var rpm = Abs(Config.Speed);
        if (rpm > 0.005f)
        {
            var startAngle = Halo.Node->Rotation % 6.283185f;
            var endAngle = startAngle + (Config.Speed >= 0 ? 6.283185f : -6.283185f);
            Animator += new Tween(Halo,
                                  new(0) { Rotation = startAngle },
                                  new((int)(60000f / rpm)) { Rotation = endAngle })
            { Repeat = true, Label = "RotationTween" };
        }
    }

    private void StopRotation() => Animator -= "RotationTween";

    #endregion

    #region UpdateFuncs

    public override void PostUpdate()
    {
        Fill.SetDrawFlags(0xA);
        Halo.SetDrawFlags(0xA);
        if (!Fill.Visible) StopRotation();
    }

    public override void OnFirstRun(int current)
    {
        var color = Config.ColorList.ElementAtOrDefault(Tracker.CurrentData.State);
        Fill.SetRGB(color);
        Fill.SetAlpha(current > 0).SetVis(current > 0);
    }

    public override void Activate(int current)
    {
        var color = Config.ColorList.ElementAtOrDefault(current);

        Fill.Show();
        Animator -= "ShimmerAlpha";
        Animator += new Tween[]
        {
            new(Fill,
                new (0) { Alpha = 0, RGB = color },
                new(200) { Alpha = 255, RGB = color }
                ) { Label = "ShimmerAlpha" }
        };

        BeginRotation();
    }

    public override void Deactivate(int previous)
    {
        Animator -= "ShimmerAlpha";
        Animator += new Tween(Fill, Visible[0], Hidden[200]) { Complete = () => Fill.Hide(), Label = "ShimmerAlpha" };
    }

    public override void StateChange(int current, int previous)
    {
        Animator += new Tween(Fill,
                              new(0) { RGB = Config.ColorList.ElementAtOrDefault(previous) },
                              new(200) { RGB = Config.ColorList.ElementAtOrDefault(current) });
    }

    #endregion

    #region Configs

    public class ShimmerHaloConfig
    {
        public Vector2 Position = new(83, 89);
        public Vector2 Scale = new(1);
        public float Angle;
        public List<ColorRGB> ColorList = new();
        public float Speed = 20f;

        public ShimmerHaloConfig(WidgetConfig widgetConfig)
        {
            var config = widgetConfig.ShimmerHaloCfg;

            if (config == null) return;

            Position = config.Position;
            Scale = config.Scale;
            Angle = config.Angle;
            ColorList = config.ColorList;
            Speed = config.Speed;
        }

        public ShimmerHaloConfig() { }

        public void FillColorList(int maxState)
        {
            while (ColorList.Count <= maxState) ColorList.Add(new(255, 255, 255));
        }
    }

    public ShimmerHaloConfig Config;

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

        Fill.SetRGB(Config.ColorList.ElementAtOrDefault(Tracker.CurrentData.State));

        if (Fill.Visible) { BeginRotation(); }
    }

    public override void DrawUI(ref WidgetConfig widgetConfig, ref UpdateFlags update)
    {
        Heading("Layout");

        PositionControls("Position", ref Config.Position, ref update);
        ScaleControls("Scale", ref Config.Scale, ref update);
        AngleControls("Angle", ref Config.Angle, ref update);
        FloatControls("Speed", ref Config.Speed, -200, 200, 1f, ref update);

        Heading("Colors");

        var maxState = Tracker.CurrentData.MaxState;

        for (var i = 1; i <= maxState; i++)
        {
            var color = Config.ColorList[i];
            var label = $"{Tracker.StateNames[i]}";
            if (ColorPickerRGB(label, ref color, ref update)) Config.ColorList[i] = color;
        }

        if (update.HasFlag(Save)) ApplyConfigs();
        widgetConfig.ShimmerHaloCfg = Config;
    }

    #endregion

}

public partial class WidgetConfig
{
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] public ShimmerHaloConfig? ShimmerHaloCfg { get; set; }
}
