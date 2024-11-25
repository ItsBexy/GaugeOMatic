using CustomNodes;
using GaugeOMatic.CustomNodes.Animation;
using GaugeOMatic.Trackers;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using static CustomNodes.CustomNode;
using static CustomNodes.CustomNode.CustomNodeFlags;
using static CustomNodes.CustomNodeManager;
using static GaugeOMatic.CustomNodes.Animation.KeyFrame;
using static GaugeOMatic.Utility.Color;
using static GaugeOMatic.Widgets.ShimmerHalo;
using static GaugeOMatic.Widgets.WidgetTags;
using static GaugeOMatic.Widgets.Common.WidgetUI;
using static GaugeOMatic.Widgets.Common.WidgetUI.WidgetUiTab;
using static System.Math;
using static GaugeOMatic.Widgets.AddonRestrictionsAttribute.RestrictionType;

#pragma warning disable CS8618

namespace GaugeOMatic.Widgets;

[WidgetName("Shimmering Halo")]
[WidgetDescription("A revolving circular aura that appears while the tracker's condition is met.")]
[WidgetAuthor("ItsBexy")]
[WidgetTags(State | HasAddonRestrictions | HasClippingMask)]
[WidgetUiTabs(Layout | Colors | Behavior)]
[AddonRestrictions(ClipConflict)]
public sealed unsafe class ShimmerHalo(Tracker tracker) : StateWidget(tracker)
{
    public override CustomPartsList[] PartsLists { get; } =
    [
        new("ui/uld/gachaeffect03.tex", new Vector4(0, 0, 256, 256), new Vector4(13, 124, 8, 8))
    ];

    #region Nodes

    public CustomNode Halo;
    public CustomNode Fill;

    public override Bounds GetBounds() => Fill;

    public override CustomNode BuildContainer()
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
        Animator +=
        [
            new(Fill,
                new (0) { Alpha = 0, RGB = color },
                new(200) { Alpha = 255, RGB = color }
                ) { Label = "ShimmerAlpha" }
        ];

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

    public class ShimmerHaloConfig : WidgetTypeConfig
    {
        public new Vector2 Scale = new(1);
        public float Angle;
        public List<ColorRGB> ColorList = [];
        [DefaultValue(20f)] public float Speed = 20f;

        public ShimmerHaloConfig(WidgetConfig widgetConfig) : base(widgetConfig.ShimmerHaloCfg)
        {
            var config = widgetConfig.ShimmerHaloCfg;

            if (config == null) return;

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

    private ShimmerHaloConfig config;

    public override ShimmerHaloConfig Config => config;

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
        base.ApplyConfigs();
        WidgetContainer.SetRotation(Config.Angle * 0.0174532925199433f);

        Fill.SetRGB(Config.ColorList.ElementAtOrDefault(Tracker.CurrentData.State));

        if (Fill.Visible) { BeginRotation(); }
    }

    public override void DrawUI()
    {
        switch (UiTab)
        {
            case Layout:
                PositionControls("Position", ref Config.Position);
                ScaleControls("Scale", ref Config.Scale);
                AngleControls("Angle", ref Config.Angle);
                FloatControls("Speed", ref Config.Speed, -200, 200, 1f);
                break;
            case Colors:
                var maxState = Tracker.CurrentData.MaxState;

                for (var i = 1; i <= maxState; i++)
                {
                    var color = Config.ColorList[i];
                    var label = $"{Tracker.StateNames[i]}";
                    if (ColorPickerRGB(label, ref color)) Config.ColorList[i] = color;
                }
                break;
            default:
                break;
        }
    }

    public override void ChangeScale(float amt)
    {
        var y = Config.Scale.Y;
        var x = Config.Scale.X;

        var a1 = (y + x) / 2f;
        var a2 = a1 + (0.05f * amt);

        Config.Scale = new(x / a1 * a2, y / a1 * a2);
    }

    #endregion
}

public partial class WidgetConfig
{
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] public ShimmerHaloConfig? ShimmerHaloCfg { get; set; }
}
