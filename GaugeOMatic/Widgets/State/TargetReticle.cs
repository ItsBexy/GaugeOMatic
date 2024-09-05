using CustomNodes;
using GaugeOMatic.Trackers;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using static CustomNodes.CustomNode;
using static CustomNodes.CustomNode.CustomNodeFlags;
using static CustomNodes.CustomNodeManager;
using static GaugeOMatic.Utility.Color;
using static GaugeOMatic.Widgets.TargetReticle;
using static GaugeOMatic.Widgets.WidgetTags;
using static GaugeOMatic.Widgets.WidgetUI;
using static GaugeOMatic.Widgets.WidgetUI.WidgetUiTab;
using static System.Math;

#pragma warning disable CS8618

namespace GaugeOMatic.Widgets;

[WidgetName("Target Reticle")]
[WidgetDescription("A revolving reticle that appears while the tracker's condition is met.")]
[WidgetAuthor("ItsBexy")]
[WidgetTags(State | HasAddonRestrictions | HasClippingMask)]
[WidgetUiTabs(Layout | Colors)]
[AddonRestrictions(false, "JobHudRPM1", "JobHudGFF1", "JobHudSMN1", "JobHudBRD0")]
public sealed unsafe class TargetReticle(Tracker tracker) : StateWidget(tracker)
{
    public override CustomPartsList[] PartsLists { get; } =
    [
        new("ui/uld/mycrelicwindowsymbol3.tex",
            new(0, 0, 450, 450),
            new(216,106,20,20))
    ];

    #region Nodes

    public CustomNode Halo;
    public CustomNode HaloFill;
    public CustomNode HaloMask;
    public CustomNode InnerHalo;
    public CustomNode InnerHaloFill;
    public CustomNode InnerHaloMask;

    public override Bounds GetBounds() => new Bounds(WidgetContainer) - (new Vector2(112.5f) * Config.Scale);

    public override CustomNode BuildContainer()
    {
        HaloFill = ImageNodeFromPart(0, 1).SetScale(23).SetImageFlag(32);
        HaloMask = ClippingMaskFromPart(0, 0);

        InnerHaloFill = ImageNodeFromPart(0, 1).SetScale(23).SetImageFlag(32);
        InnerHaloMask = ClippingMaskFromPart(0, 0);

        Halo = new CustomNode(CreateResNode(), HaloFill, HaloMask).RemoveFlags(SetVisByAlpha)
                                                                 .SetAlpha(0)
                                                                 .SetOrigin(225, 225)
                                                                 .SetPos(-225, -225);

        InnerHalo = new CustomNode(CreateResNode(), InnerHaloFill, InnerHaloMask).RemoveFlags(SetVisByAlpha)
                                                                  .SetAlpha(0)
                                                                  .SetOrigin(225, 225)
                                                                  .SetPos(-225, -225)
                                                                  .SetScale(0.6f);

        BeginRotation();

        return new CustomNode(CreateResNode(), Halo, InnerHalo).SetSize(450, 450);
    }

    #endregion

    #region Animations

    private void BeginRotation()
    {
        StopRotation();
        var rpm = Abs(Config.Speed);

        if (rpm > 0.005f)
        {
            var rotationTime = 60000f / rpm;

            var startAngle = (Halo.Node->Rotation + 6.283185f) % 6.283185f;
            var endAngle = startAngle + (Config.Speed >= 0 ? 6.283185f : -6.283185f);

            var startAngle2 = (InnerHalo.Node->Rotation + 6.283185f) % 6.283185f;
            var endAngle2 = startAngle2 + (Config.Speed >= 0 ? -6.283185f : 6.283185f);

            Animator +=
            [
                new(Halo,
                    new(0) { Rotation = startAngle },
                    new((int)rotationTime) { Rotation = endAngle })
                    { Repeat = true, Label ="RotationTween" },
                new(InnerHalo,
                    new(0) { Rotation = startAngle2 },
                    new((int)(rotationTime*1.2f)) { Rotation = endAngle2 })
                    { Repeat = true, Label = "RotationTween" }
            ];
        }
    }

    private void StopRotation() => Animator -= "RotationTween";

    #endregion

    #region UpdateFuncs

    public override void PostUpdate() { if (!Halo.Visible) StopRotation(); }

    public override void OnFirstRun(int current) => Halo.SetAlpha(current > 0);

    public override void Activate(int current)
    {
        var color = Config.ColorList.ElementAtOrDefault(current);

        Halo.Show();
        InnerHalo.Show();

        Animator -= "HaloAlpha";
        Animator +=
        [
            new(Halo,
                new(0) { Alpha = 0, Scale = 0.2f, MultRGB = color },
                new(400) { Alpha = 255, Scale = 1, MultRGB = color }) { Label = "HaloAlpha" },
            new(InnerHalo,
                new(0) { Alpha = 0, Scale = 2.1f, MultRGB = color },
                new(200) { Alpha = 255, Scale = 0.6f, MultRGB = color })
                { Label = "HaloAlpha" }

        ];

        BeginRotation();
    }

    public override void Deactivate(int previous)
    {
        Animator -= "HaloAlpha";
        Animator +=
        [
            new(Halo,
                new(0) { Alpha = 255, ScaleX = 1, ScaleY = 1 },
                new(200) { Alpha = 0, ScaleX = 1.2f, ScaleY = 1.2f })
                { Complete = () => Halo.Hide(), Label = "HaloAlpha" },
            new(InnerHalo,
                new(0) { Alpha = 255, ScaleX = 0.6f, ScaleY = 0.6f },
                new(200) { Alpha = 0, ScaleX = 0.2f, ScaleY = 0.2f })
                { Complete = () => Halo.Hide(), Label = "HaloAlpha" }
        ];
    }

    public override void StateChange(int current, int previous) =>
        Animator +=
        [
            new(Halo,
                new(0) { MultRGB = Config.ColorList.ElementAtOrDefault(previous) },
                new(200) { MultRGB = Config.ColorList.ElementAtOrDefault(current) }),
            new(InnerHalo,
                new(0) { MultRGB = Config.ColorList.ElementAtOrDefault(previous) },
                new(200) { MultRGB = Config.ColorList.ElementAtOrDefault(current) })
        ];

    #endregion

    #region Configs

    public class TargetReticleConfig : WidgetTypeConfig
    {
        public float Angle;
        public List<ColorRGB> ColorList = [];
        [DefaultValue(20f)] public float Speed = 20f;

        public TargetReticleConfig(WidgetConfig widgetConfig) : base(widgetConfig.TargetReticleCfg)
        {
            var config = widgetConfig.TargetReticleCfg;

            if (config == null) return;

            Angle = config.Angle;
            ColorList = config.ColorList;
            Speed = config.Speed;
        }

        public TargetReticleConfig() { }

        public void FillColorLists(int max)
        {
            while (ColorList.Count <= max) ColorList.Add(new(100, 70, 50));
        }
    }

    private TargetReticleConfig config;

    public override TargetReticleConfig Config => config;

    public override void InitConfigs()
    {
        config = new(Tracker.WidgetConfig);
        Config.FillColorLists(Tracker.CurrentData.MaxState);
    }

    public override void ResetConfigs()
    {
        config = new();
        Config.FillColorLists(Tracker.CurrentData.MaxState);
    }

    public override void ApplyConfigs()
    {
        WidgetContainer.SetPos(Config.Position)
                  .SetScale(Config.Scale * 0.5f)
                  .SetRotation(Config.Angle * 0.0174532925199433f);

        Halo.SetMultiply(Config.ColorList.ElementAtOrDefault(Tracker.CurrentData.State));
        InnerHalo.SetMultiply(Config.ColorList.ElementAtOrDefault(Tracker.CurrentData.State));

        if (Halo.Visible) { BeginRotation(); }
    }

    public override void DrawUI()
    {
        base.DrawUI();
        switch (UiTab)
        {
            case Layout:
                FloatControls("Speed", ref Config.Speed, -200, 200, 1f);
                break;
            case Colors:
                for (var i = 1; i <= Tracker.CurrentData.MaxState; i++)
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

    #endregion
}

public partial class WidgetConfig
{
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] public TargetReticleConfig? TargetReticleCfg { get; set; }
}
