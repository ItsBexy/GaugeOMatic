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
using static GaugeOMatic.Widgets.TargetReticle;
using static GaugeOMatic.Widgets.WidgetTags;
using static GaugeOMatic.Widgets.WidgetUI;
using static GaugeOMatic.Windows.UpdateFlags;

namespace GaugeOMatic.Widgets;

public sealed unsafe class TargetReticle : StateWidget
{
    public override WidgetInfo WidgetInfo => GetWidgetInfo;

    public static WidgetInfo GetWidgetInfo => new()
    {
        DisplayName = "Target Reticle",
        Author = "ItsBexy",
        Description = "A revolving reticle that appears while the tracker's condition is met.",
        WidgetTags = State
    };

    public override CustomPartsList[] PartsLists { get; } =
    {
        new("ui/uld/mycrelicwindowsymbol3.tex", new Vector4(0, 0, 450, 450))
    };
    #region Nodes

    public CustomNode Halo;
    public CustomNode InnerHalo;

    public override CustomNode BuildRoot()
    {
        Halo = ImageNodeFromPart(0, 0).SetAlpha(0)
                                      .SetImageFlag(32)
                                      .SetOrigin(225, 225)
                                      .SetPos(-225, -225);

        InnerHalo = ImageNodeFromPart(0,0).SetAlpha(0)
                                          .SetImageFlag(32)
                                          .SetOrigin(225, 225)
                                          .SetPos(-225, -225)
                                          .SetScale(0.6f);

        BeginRotation();

        return new(CreateResNode(), Halo,InnerHalo);
    }

    private void BeginRotation()
    {
        StopRotation();
        var rpm = Math.Abs(Config.Speed);

        if (rpm > 0.005f)
        {
            var rotationTime = 60000f / rpm;

            var startAngle = (Halo.Node->Rotation + 6.283185f) % 6.283185f;
            var endAngle = startAngle + (Config.Speed >= 0 ? 6.283185f : -6.283185f);
            Tweens.Add(new(Halo, 
                           new(0) { Rotation = startAngle }, 
                           new((int)rotationTime) { Rotation = endAngle }) 
                           { Repeat = true, Label="RotationTween" });


            var startAngle2 = (InnerHalo.Node->Rotation + 6.283185f) % 6.283185f;
            var endAngle2 = startAngle2 + (Config.Speed >= 0 ? -6.283185f : 6.283185f);
            Tweens.Add(new(InnerHalo,
                           new(0) { Rotation = startAngle2 },
                           new((int)(rotationTime*1.2f)) { Rotation = endAngle2 })
                           { Repeat = true, Label = "RotationTween" });
        }
    }

    private void StopRotation() => ClearLabelTweens(ref Tweens, "RotationTween");

    #endregion

    #region Animations

    #endregion

    #region UpdateFuncs

    public override string? SharedEventGroup => null;

    public override void OnUpdate() { if (!Halo.Visible) StopRotation(); }

    public override void OnFirstRun(int current) => Halo.SetAlpha(current > 0 ? 255:0);

    public override void Activate(int current)
    {
        var color = Config.ColorList.ElementAtOrDefault(current);

        Halo.Show();
        InnerHalo.Show();

        ClearLabelTweens(ref Tweens, "HaloAlpha");

        Tweens.Add(new(Halo, 
                       new(0) { Alpha = 0, Scale = 0.2f, MultRGB = color }, 
                       new(400) { Alpha = 255, Scale = 1, MultRGB = color }){ Label = "HaloAlpha" });
        Tweens.Add(new(InnerHalo,
                       new(0) { Alpha = 0, Scale = 2.1f, MultRGB = color },
                       new(200) { Alpha = 255, Scale = 0.6f, MultRGB = color })
                       { Label = "HaloAlpha" });

        BeginRotation();
    }

    public override void Deactivate(int previous)
    {
        ClearLabelTweens(ref Tweens, "HaloAlpha");
        Tweens.Add(new(Halo, 
                       new(0) { Alpha = 255, ScaleX = 1, ScaleY = 1 }, 
                       new(200) { Alpha = 0, ScaleX=1.2f,ScaleY=1.2f }) 
                       { Complete = () => Halo.Hide(), Label = "HaloAlpha" });

        Tweens.Add(new(InnerHalo, 
                       new(0) { Alpha = 255, ScaleX=0.6f,ScaleY=0.6f }, 
                       new(200) { Alpha = 0, ScaleX = 0.2f, ScaleY = 0.2f }) 
                       { Complete = () => Halo.Hide(), Label = "HaloAlpha" });
    }

    public override void StateChange(int current, int previous)
    {
        Tweens.Add(new(Halo,
                       new(0) { MultRGB = Config.ColorList.ElementAtOrDefault(previous) },
                       new(200) { MultRGB = Config.ColorList.ElementAtOrDefault(current) }));
        Tweens.Add(new(InnerHalo,
                       new(0) { MultRGB = Config.ColorList.ElementAtOrDefault(previous) },
                       new(200) { MultRGB = Config.ColorList.ElementAtOrDefault(current) }));
    }

    #endregion

    #region Configs

    public class TargetReticleConfig
    {
        public Vector2 Position = new(83, 89);
        public float Scale = 1;
        public float Angle;
        public List<ColorRGB> ColorList = new();
        public float Speed = 20f;

        public TargetReticleConfig(WidgetConfig widgetConfig)
        {
            var config = widgetConfig.TargetReticleCfg;

            if (config == null) return;

            Position = config.Position;
            Scale = config.Scale;
            Angle = config.Angle;
            ColorList = config.ColorList;
            Speed = config.Speed;
        }

        public TargetReticleConfig() { }
    }

    public TargetReticleConfig Config = null!;

    public override void InitConfigs()
    {
        Config = new(Tracker.WidgetConfig);
        while (Config.ColorList.Count <= Tracker.CurrentData.MaxState) Config.ColorList.Add(new(100,70,50));
    }

    public override void ResetConfigs() => Config = new();

    public override void ApplyConfigs()
    {
        WidgetRoot.SetPos(Config.Position)
                  .SetScale(Config.Scale*0.5f)
                  .SetRotation(Config.Angle * 0.0174532925199433f);

        Halo.SetMultiply(Config.ColorList.ElementAtOrDefault(Tracker.CurrentData.State));
        InnerHalo.SetMultiply(Config.ColorList.ElementAtOrDefault(Tracker.CurrentData.State));

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

        for (var i = 1; i <= Tracker.CurrentData.MaxState; i++)
        {
            var color = Config.ColorList[i];
            var label = $"{Tracker.StateNames[i]}";
            if (ColorPickerRGB(label, ref color, ref update)) Config.ColorList[i] = color;
        }

        if (update.HasFlag(Save)) ApplyConfigs();
        widgetConfig.TargetReticleCfg = Config;
    }

    #endregion

    public TargetReticle(Tracker tracker) : base(tracker) { }
}

public partial class WidgetConfig
{
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] public TargetReticleConfig? TargetReticleCfg { get; set; }
}
