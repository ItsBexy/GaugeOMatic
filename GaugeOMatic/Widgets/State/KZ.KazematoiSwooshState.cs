using CustomNodes;
using GaugeOMatic.CustomNodes.Animation;
using GaugeOMatic.Trackers;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using static CustomNodes.CustomNode;
using static CustomNodes.CustomNodeManager;
using static GaugeOMatic.Utility.Color;
using static GaugeOMatic.Widgets.Common.CommonParts;
using static GaugeOMatic.Widgets.KazematoiSwooshState;
using static GaugeOMatic.Widgets.WidgetTags;
using static GaugeOMatic.Widgets.WidgetUI;
using static GaugeOMatic.Widgets.WidgetUI.WidgetUiTab;

#pragma warning disable CS8618

namespace GaugeOMatic.Widgets;

[WidgetName("Kazematoi Swoosh")]
[WidgetDescription("A state indicator based on the backdrop of NIN's Kazematoi.")]
[WidgetAuthor("ItsBexy")]
[WidgetTags(State | MultiComponent | HasClippingMask)]
[WidgetUiTabs(Layout | Colors)]
[MultiCompData("KZ", "Kazematoi Replica", 1)]
[AddonRestrictions(false, "JobHudRPM1", "JobHudGFF1", "JobHudSMN1", "JobHudBRD0")]
public sealed unsafe class KazematoiSwooshState : StateWidget
{
    public KazematoiSwooshState(Tracker tracker) : base(tracker) { }

    public override CustomPartsList[] PartsLists { get; } = { NIN1,
        new("ui/uld/JobHudNIN1Mask.tex",new Vector4(0,0,128,96)),
        new("ui/uld/CruisingEffect.tex",new Vector4(0,0,128,128))
    };

    #region Nodes

    public CustomNode Backdrop;
    public CustomNode CloudBox;
    public CustomNode ScrollCloud;
    public CustomNode SpinCloud;
    public CustomNode Mask;

    public override Bounds GetBounds() => Mask;

    public override CustomNode BuildContainer()
    {
        Mask = ClippingMaskFromPart(1, 0).SetPos(5, 3);

        ScrollCloud = ImageNodeFromPart(2, 0)
                      .SetWidth(256)
                      .SetImageWrap(1)
                      .SetAddRGB(64, 0, 128)
                      .SetAlpha(63)
                      .SetImageFlag(32).SetAlpha(0);

        SpinCloud = ImageNodeFromPart(2, 0)
                    .SetImageWrap(2)
                    .SetAddRGB(64, 0, 128)
                    .SetPos(36, 82)
                    .SetRotation(-4.71238898038469f)
                    .SetImageFlag(32);

        CloudBox = new(CreateResNode(), ScrollCloud, SpinCloud, Mask);

        Backdrop = ImageNodeFromPart(0, 0);

        return new CustomNode(CreateResNode(), Backdrop, CloudBox).SetOrigin(69F, 51F);
    }

    #endregion

    #region Animations

    #endregion

    #region UpdateFuncs

    public override void OnFirstRun(int current)
    {
        Animator += new Tween(ScrollCloud, new(0) { X = -128 }, new(2000) { X = 0 }) { Repeat = true };
        if (current == 0)
        {
            Backdrop.SetMultiply(50);
        }
    }

    public override void Activate(int current)
    {
        StateChange(current, 0);
    }

    public override void Deactivate(int previous)
    {
        StateChange(0, previous);
    }

    public override void PostUpdate()
    {
        var curCount = Tracker.CurrentData.Count;
        var prevCount = Tracker.PreviousData.Count;

        if (curCount > prevCount)
        {
            Animator += new Tween(SpinCloud,
                                  new(0) { Rotation = -4.71238898038469f },
                                  new(600) { Rotation = 1.5707963267949f });
        }
    }

    public override void StateChange(int current, int previous)
    {
        // hardcoding an extremely specific exception here
        if (current != 0 && !(current < previous && Tracker.TrackerConfig.TrackerType == "KazematoiTracker"))
            Animator += new Tween(SpinCloud,
                                  new(0) { Rotation = -4.71238898038469f },
                                  new(600) { Rotation = 1.5707963267949f });

        SpinCloud.SetAddRGB(Config.GetSweepColor(current));

        var curPart = Config.GetBackdropBase(current);
        var prevPart = Config.GetBackdropBase(previous);

        Animator += new Tween[]
        {
            new(Backdrop,
                new(0) { PartId = prevPart },
                new(199) { PartId = prevPart },
                new(201) { PartId = curPart },
                new(400) { PartId = curPart }),
            new(Backdrop,
                new(0)
                {
                    AddRGB = Config.GetBackdropTint(previous),
                    Alpha = previous != 0 || !Config.HideInactive ? 255 : 0,
                    MultRGB = previous > 0 ? new ColorRGB(100) : new (50)
                },
                new(400){
                    AddRGB = Config.GetBackdropTint(current),
                    Alpha = current != 0 || !Config.HideInactive ? 255 : 0,
                    MultRGB = current > 0 ? new ColorRGB(100) : new(50)
                }),
            new(ScrollCloud,
                new(0)
                {
                    AddRGB = Config.GetCloudColor(previous),
                    Alpha = Config.GetClouds(previous) ? 64:0
                },
                new(400)
                {
                    AddRGB = Config.GetCloudColor(current),
                    Alpha = Config.GetClouds(current) ? 64:0
                })
        };
    }

    #endregion

    #region Configs

    public class KazematoiSwooshStateConfig : WidgetTypeConfig
    {
        public float Angle;
        public List<ushort> BackdropBases = new();
        public List<AddRGB> BackdropTints = new();
        public List<bool> Clouds = new();
        public List<AddRGB> CloudColors = new();
        public List<AddRGB> SweepColors = new();
        public bool HideInactive;

        public KazematoiSwooshStateConfig(WidgetConfig widgetConfig) : base(widgetConfig.KazematoiSwooshStateCfg)
        {
            var config = widgetConfig.KazematoiSwooshStateCfg;

            if (config == null) return;

            Angle = config.Angle;
            BackdropBases = config.BackdropBases;
            BackdropTints = config.BackdropTints;
            Clouds = config.Clouds;
            CloudColors = config.CloudColors;
            SweepColors = config.SweepColors;
            HideInactive = config.HideInactive;
        }

        public KazematoiSwooshStateConfig() { }

        public void FillColorLists(int maxState)
        {
            while (BackdropBases.Count <= maxState) BackdropBases.Add(1);
            while (BackdropTints.Count <= maxState) BackdropTints.Add(new(0));
            while (CloudColors.Count <= maxState) CloudColors.Add(CloudColors.Count == 0 ? new(-255) : new(64, 0, 128));
            while (SweepColors.Count <= maxState) SweepColors.Add(new(64, 0, 128));
            while (Clouds.Count <= maxState) Clouds.Add(Clouds.Count != 0);
        }

        public ushort GetBackdropBase(int state) => BackdropBases.ElementAtOrDefault(state);
        public AddRGB GetBackdropTint(int state) => BackdropTints.ElementAtOrDefault(state);
        public AddRGB GetCloudColor(int state) => CloudColors.ElementAtOrDefault(state);
        public AddRGB GetSweepColor(int state) => SweepColors.ElementAtOrDefault(state);
        public bool GetClouds(int state) => Clouds.ElementAtOrDefault(state);
    }

    private KazematoiSwooshStateConfig config;

    public override KazematoiSwooshStateConfig Config => config;

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
                       .SetScale(Config.Scale)
                       .SetRotation(Config.Angle, true);

        var state = Tracker.CurrentData.State;

        Backdrop.SetPartId(Config.GetBackdropBase(state))
                .SetAddRGB(Config.GetBackdropTint(state))
                .SetAlpha(state != 0 || !Config.HideInactive);

        ScrollCloud.SetAddRGB(Config.GetCloudColor(state))
                   .SetAlpha(Config.GetClouds(state) ? 0x3f : 0);

        SpinCloud.SetAddRGB(Config.GetSweepColor(state));
    }

    public override void DrawUI()
    {
        base.DrawUI();
        Config.FillColorLists(Tracker.CurrentData.MaxState);
        switch (UiTab)
        {
            case Layout:
                AngleControls("Angle", ref Config.Angle);
                break;
            case Colors:
                for (var i = 0; i <= Tracker.CurrentData.MaxState; i++)
                {
                    Heading(Tracker.StateNames[i]);

                    var backdropBase = Config.BackdropBases[i];
                    var backdropTint = Config.BackdropTints[i];
                    var cloudColor = Config.CloudColors[i];
                    var sweepColor = Config.SweepColors[i];
                    var clouds = Config.Clouds[i];

                    if (i == 0)
                    {
                        ToggleControls("Hide", ref Config.HideInactive);
                    }
                    if (i != 0 || !Config.HideInactive)
                    {
                        if (RadioControls($"Base Color##baseColor{i}", ref backdropBase, new() { 1, 0 }, new() { "Violet", "Grey" }, true)) Config.BackdropBases[i] = backdropBase;
                        if (ColorPickerRGB($"Backdrop Tint##backdrop{i}", ref backdropTint)) Config.BackdropTints[i] = backdropTint;
                        if (i != 0 && ColorPickerRGB($"Flash Color##sweep{i}", ref sweepColor)) Config.SweepColors[i] = sweepColor;
                        if (i != 0 && ToggleControls($"Show Clouds##clouds{i}", ref clouds)) Config.Clouds[i] = clouds;
                        if (i != 0 && clouds && ColorPickerRGB($"Cloud Color##cloudColor{i}", ref cloudColor)) Config.CloudColors[i] = cloudColor;
                    }
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
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] public KazematoiSwooshStateConfig? KazematoiSwooshStateCfg { get; set; }
}
