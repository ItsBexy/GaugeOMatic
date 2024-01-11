using GaugeOMatic.Trackers;
using GaugeOMatic.Windows;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using static CustomNodes.CustomNodeManager;
using static CustomNodes.CustomNodeManager.Tween;
using static GaugeOMatic.Utility.Color;
using static GaugeOMatic.Widgets.ShimmerHalo;
using static GaugeOMatic.Widgets.WidgetTags;
using static GaugeOMatic.Widgets.WidgetUI;
using static GaugeOMatic.Windows.UpdateFlags;

namespace GaugeOMatic.Widgets;

public sealed unsafe class ShimmerHalo : StateWidget
{
    public override WidgetInfo WidgetInfo => GetWidgetInfo;

    public static WidgetInfo GetWidgetInfo => new()
    {
        DisplayName = "Shimmering Halo",
        Author = "ItsBexy",
        Description = "A revolving circular aura that appears while the tracker's condition is met.",
        WidgetTags = State
    };

    public override CustomPartsList[] PartsLists { get; } =
    {
        new("ui/uld/gachaeffect03.tex", 
            new Vector4(0, 0, 256, 256) 
            )
    };
    #region Nodes

    public CustomNode Halo;

    public override CustomNode BuildRoot()
    {
        Halo = ImageNodeFromPart(0,0)
               .SetAlpha(0)
               .SetImageFlag(32)
               .SetOrigin(128, 128)
               .SetPos(-128, -128);

        BeginRotation();

        return new(CreateResNode(), Halo);
    }

    private void BeginRotation()
    {
        StopRotation();
        var rpm = Math.Abs(Config.Speed);

        if (rpm > 0.005f)
        {
            var rotationTime = 60000f / rpm;
            var startAngle = Halo.Node->Rotation % 6.283185f;
            var endAngle = startAngle + (Config.Speed >= 0 ? 6.283185f : -6.283185f);
            Tweens.Add(new(Halo, 
                           new(0) { Rotation = startAngle }, 
                           new((int)rotationTime) { Rotation = endAngle }) 
                           { Repeat = true, Label="RotationTween" });
        }
    }

    private void StopRotation() => ClearLabelTweens(ref Tweens, "RotationTween");

    #endregion

    #region Animations

    #endregion

    #region UpdateFuncs

    public override string? SharedEventGroup => null;

    public override void OnUpdate() { if (!Halo.Visible) StopRotation(); }

    public override void OnFirstRun(int current)
    {
        Halo.SetMultiply(Config.ColorList.ElementAtOrDefault(Tracker.CurrentData.State));
        Halo.SetAlpha(current > 0 ? 255 : 0).SetVis(current > 0);
    }

    public override void Activate(int current)
    {
        var color = Config.ColorList.ElementAtOrDefault(current);

        Halo.Show();
        ClearLabelTweens(ref Tweens, "ShimmerAlpha");
        Tweens.Add(new(Halo, 
                       new(0) { Alpha = 0, MultRGB = color }, 
                       new(200) { Alpha = 255, MultRGB = color }){ Label = "ShimmerAlpha" });

        BeginRotation();
    }

    public override void Deactivate(int previous)
    {
        ClearLabelTweens(ref Tweens,"ShimmerAlpha");
        Tweens.Add(new(Halo, 
                       new(0) { Alpha = 255 }, 
                       new(200) { Alpha = 0 }) 
                       { Complete = () => Halo.Hide(),
                         Label = "ShimmerAlpha" });
    }

    public override void StateChange(int current, int previous) =>
        Tweens.Add(new(Halo,
                       new(0) { MultRGB = Config.ColorList.ElementAtOrDefault(previous) },
                       new(200) { MultRGB = Config.ColorList.ElementAtOrDefault(current) }));

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
            while (ColorList.Count <= maxState) ColorList.Add(new(255,255,255));
        }
    }

    public ShimmerHaloConfig Config = null!;

    public override void InitConfigs()
    {
        Config = new(Tracker.WidgetConfig);
        Config.FillColorList(Tracker.CurrentData.MaxState);
    }

    public override void ResetConfigs() => Config = new();

    public override void ApplyConfigs()
    {
        WidgetRoot.SetPos(Config.Position)
                  .SetScale(Config.Scale)
                  .SetRotation(Config.Angle * 0.0174532925199433f);

        Halo.SetMultiply(Config.ColorList.ElementAtOrDefault(Tracker.CurrentData.State));

        if (Halo.Visible) { BeginRotation(); }
    }

    public override void DrawUI(ref WidgetConfig widgetConfig, ref UpdateFlags update)
    {
        Heading("Layout");

        PositionControls("Position", ref Config.Position, ref update);
        ScaleControls("Scale", ref Config.Scale, ref update);
        FloatControls("Angle", ref Config.Angle, -180, 180, 1f, ref update);
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

    public ShimmerHalo(Tracker tracker) : base(tracker) { }
}

public partial class WidgetConfig
{
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] public ShimmerHaloConfig? ShimmerHaloCfg { get; set; }
}
