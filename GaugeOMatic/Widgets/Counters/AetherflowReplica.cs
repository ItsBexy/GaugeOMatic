using CustomNodes;
using GaugeOMatic.CustomNodes.Animation;
using GaugeOMatic.Trackers;
using Newtonsoft.Json;
using System.ComponentModel;
using static CustomNodes.CustomNode;
using static CustomNodes.CustomNodeManager;
using static GaugeOMatic.CustomNodes.Animation.Tween.EaseType;
using static GaugeOMatic.Utility.Color;
using static GaugeOMatic.Widgets.AetherflowReplica;
using static GaugeOMatic.Widgets.WidgetTags;
using static GaugeOMatic.Widgets.Common.WidgetUI;
using static GaugeOMatic.Widgets.Common.WidgetUI.WidgetUiTab;
using static System.Math;

#pragma warning disable CS8618

namespace GaugeOMatic.Widgets;

[WidgetName("Aetherflow Gems")]
[WidgetDescription("A recreation of Arcanist/Summoner/Scholar's Aetherflow Gauge.")]
[WidgetAuthor("ItsBexy")]
[WidgetTags(Counter | Replica)]
[WidgetUiTabs(Layout | Colors | Behavior | Icon | Sound)]
public sealed unsafe class AetherflowReplica(Tracker tracker) : CounterWidget(tracker)
{
    public override CustomPartsList[] PartsLists { get; } =
    [
        new("ui/uld/JobHudSCH0.tex",
            new(0, 76, 44, 44),
            new(44, 76, 40, 52),
            new(84, 76, 44, 32),
            new(0, 0, 57, 76),
            new(57, 0, 38, 76),
            new(95, 0, 57, 76),
            new(0, 0, 37, 76),
            new(113, 0, 39, 76)),
        new("ui/uld/JobHudSMN0.tex",
            new(0, 76, 44, 44),
            new(44, 76, 40, 52),
            new(0, 120, 44, 32))
    ];

    #region Nodes

    public CustomNode SocketPlate;
    public CustomNode Gems;

    public override Bounds GetBounds() => WidgetContainer;

    public override CustomNode BuildContainer()
    {
        Max = GetMax();

        SocketPlate = BuildSocketPlate(Max, out var width);
        Gems = BuildGems(Max);

        return new CustomNode(CreateResNode(), SocketPlate, Gems).SetOrigin((width / 2f) - 1, 37).SetSize(width, 74);
    }

    private CustomNode BuildSocketPlate(int count, out int width)
    {
        if (count == 1)
        {
            width = 76;
            return new(CreateResNode(),
                       ImageNodeFromPart(0, 6).SetPos(0, 0),
                       ImageNodeFromPart(0, 7).SetPos(37, 0));
        }

        var socketNodes = new CustomNode[count];
        var x = 0;
        for (var i = 0; i < count; i++)
        {
            var part = (ushort)(i == 0 ? 3 : i == count - 1 ? 5 : 4);
            socketNodes[i] = ImageNodeFromPart(0, part).SetPos(x, 0);
            x += i == 0 || i == count - 1 ? 57 : 38;
        }

        width = x;
        return new CustomNode(CreateResNode(), socketNodes).SetOrigin((width / 2f) - 1, 37);
    }

    private CustomNode BuildGems(int count)
    {
        var gemList = new CustomNode[count];
        for (var i = 0; i < count; i++)
        {
            var gem = ImageNodeFromPart(0, 0);
            var pulse = ImageNodeFromPart(0, 1).SetPos(2, -6)
                                               .SetScale(1.2f, 0.9f)
                                               .SetOrigin(20, 40)
                                               .SetImageFlag(32);


            var pulsar = ImageNodeFromPart(0, 2).SetPos(0, 6).SetOrigin(22, 16).SetImageFlag(32);
            var spendGlow = ImageNodeFromPart(0, 1).SetPos(2, -8).SetScale(1.2f, 0.9f).SetOrigin(20, 40).SetImageFlag(32).SetAlpha(0);

            gemList[i] = new CustomNode(CreateResNode(), gem, pulse, pulsar, spendGlow).SetPos(15 + (38 * i), 15).SetOrigin(22, 22).SetAlpha(0);

            Animator += new Tween(pulse,
                                  new(0) { ScaleX = 1.2f, Alpha = 0 },
                                  new(360) { ScaleX = 1, Alpha = 201 },
                                  new(770) { ScaleX = 0.9f, Alpha = 0 })
                                  { Repeat = true, Ease = SinInOut };
        }

        return new(CreateResNode(), gemList);
    }

    #endregion

    #region Animations

    public override void ShowStack(int i)
    {
        var flipFactor = Abs(Config.Angle) >= 90 ? -1 : 1;
        var green = Config.BaseColor == 0;

        AddRGB addStep1 = green ? new(-20, 20, -20) : new(-20, -10, 20);
        AddRGB addStep2 = green ? new(-190, 200, -200) : new(-200, -100, 200);
        AddRGB addStep3 = green ? new(100, 200, -200) : new(-200, -100, 200);

        if (Gems.Children.Length <= i) return;

        Animator -= Gems[i];
        Animator +=
        [
            new(Gems[i],
                new(0) { ScaleX = flipFactor, ScaleY = 1, Alpha = 0, AddRGB = -19 },
                new(120) { ScaleX = flipFactor, ScaleY = 1, Alpha = 255, AddRGB = new(0) }),

            new(Gems[i][2],
                new(0) { ScaleX = 3.2f, ScaleY = 0.6f, Alpha = 255, AddRGB = addStep1 },
                new(90) { ScaleX = 4.9f, ScaleY = 0.6f, Alpha = 255, AddRGB = addStep2 },
                new(300) { ScaleX = 3, ScaleY = 0.2f, Alpha = 0, AddRGB = addStep3 })
        ];
    }

    public override void HideStack(int i)
    {
        if (Gems.Children.Length <= i) return;

        var flipFactor = Abs(Config.Angle) >= 90 ? -1 : 1;
        
        Animator -= Gems[i];
        Animator +=
        [
            new(Gems[i],
                new(0) { ScaleX = 1 * flipFactor, ScaleY = 1, Alpha = 255 },
                new(300) { ScaleX = 1.5f * flipFactor, ScaleY = 1.5f, Alpha = 0 }),

            new(Gems[i][3],
                new(0) { Scale = 1.1f, Alpha = 135 },
                new(50) { Scale = 1.1f, Alpha = 176 },
                new(250) { ScaleX = 2.3f, ScaleY = 2f, Alpha = 0 })
        ];
    }

    private void PlateVanish()
    {
        var flipFactor = Abs(Config.Angle) >= 90 ? -1 : 1;
        var downScale = Config.Scale * 0.65f;
        Animator += new Tween(WidgetContainer,
                              new(0) { ScaleX = Config.Scale, ScaleY = Config.Scale * flipFactor, Alpha = 255 },
                              new(150) { ScaleX = downScale, ScaleY = downScale * flipFactor, Alpha = 0 })
        { Ease = SinInOut };
    }

    private void PlateAppear()
    {
        var flipFactor = Abs(Config.Angle) >= 90 ? -1 : 1;
        var upScale = Config.Scale * 1.65f;
        Animator += new Tween(WidgetContainer,
                                 new(0) { ScaleX = upScale, ScaleY = upScale * flipFactor, Alpha = 0 },
                                 new(200) { ScaleX = Config.Scale, ScaleY = Config.Scale * flipFactor, Alpha = 255 })
        { Ease = SinInOut };
    }

    #endregion

    #region UpdateFuncs

    public override void OnFirstRun(int count, int max)
    {
        var flipFactor = Abs(Config.Angle) >= 90 ? -1 : 1;
        for (var i = 0; i < max; i++) Gems[i].SetAlpha(i < count).SetScale(flipFactor, 1);
        if (Config.HideEmpty && count == 0) WidgetContainer.Hide();
    }

    public override void OnDecreaseToMin() { if (Config.HideEmpty) PlateVanish(); }

    public override void OnIncreaseFromMin() { if (Config.HideEmpty || WidgetContainer.Alpha < 255) { PlateAppear(); } }

    #endregion

    #region Configs

    public class AetherflowReplicaConfig : CounterWidgetConfig
    {
        [DefaultValue(1)] public int BaseColor = 1;
        public AddRGB ColorModifier = new(0);
        public float Angle;
        public ColorRGB FrameColor = new(100);
        public bool HideEmpty;

        public AetherflowReplicaConfig(WidgetConfig widgetConfig) : base(widgetConfig.AetherflowReplicaCfg)
        {
            var config = widgetConfig.AetherflowReplicaCfg;

            if (config == null) return;

            BaseColor = config.BaseColor;
            ColorModifier = config.ColorModifier;
            Angle = config.Angle;
            FrameColor = config.FrameColor;
            HideEmpty = config.HideEmpty;
        }

        public AetherflowReplicaConfig() { }
    }

    private AetherflowReplicaConfig config;

    public override AetherflowReplicaConfig Config => config;

    public override void InitConfigs() => config = new(Tracker.WidgetConfig);

    public override void ResetConfigs() => config = new();

    public override void ApplyConfigs()
    {
        base.ApplyConfigs();

        var flipFactor = Abs(Config.Angle) >= 90 ? -1 : 1;
        WidgetContainer.SetScale(Config.Scale, Config.Scale * flipFactor)
                  .SetRotation(Config.Angle, true)
                  .SetAlpha(Tracker.CurrentData.Count != 0 || !Config.HideEmpty);

        SocketPlate.SetScaleX(flipFactor)
                   .SetMultiply(Config.FrameColor);

        Gems.SetAddRGB(Config.ColorModifier);
        foreach (var gem in Gems.Children)
        {
            gem.SetScaleX(flipFactor);
            for (var i = 0; i < 4; i++) gem[i].SetPartsList(PartsLists[Config.BaseColor]);
        }
    }

    public override void DrawUI()
    {
        base.DrawUI();
        switch (UiTab)
        {
            case Layout:
                AngleControls("Angle", ref Config.Angle);
                break;
            case Colors:
                RadioControls("Base Color", ref Config.BaseColor, [1, 0], ["Pink", "Green"]);
                ColorPickerRGB("Color Modifier", ref Config.ColorModifier);
                ColorPickerRGB("Frame Tint", ref Config.FrameColor);
                break;
            case Behavior:
                if (ToggleControls("Hide Empty", ref Config.HideEmpty))
                {
                    if (Config.HideEmpty && ((!Config.AsTimer && Tracker.CurrentData.Count == 0) || (Config.AsTimer && Tracker.CurrentData.GaugeValue == 0))) PlateVanish();
                    if (!Config.HideEmpty && WidgetContainer.Alpha < 255) PlateAppear();
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
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] public AetherflowReplicaConfig? AetherflowReplicaCfg { get; set; }
}
