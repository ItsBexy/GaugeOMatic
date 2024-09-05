using CustomNodes;
using GaugeOMatic.Trackers;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Numerics;
using static CustomNodes.CustomNode;
using static CustomNodes.CustomNodeManager;
using static GaugeOMatic.Utility.MiscMath;
using static GaugeOMatic.Widgets.Common.CommonParts;
using static GaugeOMatic.Widgets.DragonEye;
using static GaugeOMatic.Widgets.DragonEye.DragonEyeConfig.EyeState;
using static GaugeOMatic.Widgets.WidgetTags;
using static GaugeOMatic.Widgets.WidgetUI;
using static GaugeOMatic.Widgets.WidgetUI.WidgetUiTab;

#pragma warning disable CS8618

namespace GaugeOMatic.Widgets;

[WidgetName("Dragon Eye")]
[WidgetDescription("An indicator recreating the eye on Dragoon's Life of the Dragon gauge.")]
[WidgetAuthor("ItsBexy")]
[WidgetTags(State | Replica | MultiComponent)]
[WidgetUiTabs(Layout | Behavior)]
[MultiCompData("DR", "Replica Dragon Gauge", 3)]
public sealed unsafe class DragonEye(Tracker tracker) : StateWidget(tracker)
{
    public override CustomPartsList[] PartsLists { get; } = [DRG0];

    #region Nodes

    public CustomNode EyeFrame;
    public CustomNode EyeBase;
    public CustomNode Eyeball;
    public CustomNode BlackSpot;
    public CustomNode WhiteGlow;

    public override Bounds GetBounds() => EyeFrame;

    public override CustomNode BuildContainer()
    {
        EyeFrame = ImageNodeFromPart(0, 10).SetImageWrap(1);

        EyeBase = ImageNodeFromPart(0, 15).SetPos(13, 9)
                                          .SetOrigin(34, 16)
                                          .SetImageWrap(1);

        Eyeball = ImageNodeFromPart(0, 3).SetPos(30, 6)
                                         .SetOrigin(18, 18)
                                         .SetAddRGB(-7)
                                         .SetImageWrap(1)
                                         .SetAlpha(0);

        BlackSpot = ImageNodeFromPart(0, 8).SetPos(29, 5)
                                           .SetScale(1.0727273f)
                                           .SetOrigin(18, 18)
                                           .SetAddRGB(100, 0, 100)
                                           .SetImageWrap(1)
                                           .SetImageFlag(32)
                                           .SetAlpha(0);

        WhiteGlow = ImageNodeFromPart(0, 9).SetPos(41, 0)
                                           .SetSize(40, 26)
                                           .SetScale(2.2f, 1.5f)
                                           .SetRotation(-0.17453294f)
                                           .SetOrigin(34, 5)
                                           .SetImageFlag(32)
                                           .SetImageWrap(1)
                                           .SetAddRGB(200, -200, 0)
                                           .SetAlpha(0);

        return new CustomNode(CreateResNode(), EyeFrame, EyeBase, Eyeball, BlackSpot, WhiteGlow).SetPos(80, 28).SetOrigin(10, 40);
    }

    #endregion

    #region Animations

    public void ShutToHalf()
    {
        Animator +=
        [
            new(EyeFrame,
                new(0) { X = 0, Y = 0, AddRGB = 0 },//
                new(75) { X = -4, Y = 3, AddRGB = 0 },
                new(135) { X = -4, Y = 0, AddRGB = 0 },
                new(200) { X = 0, Y = 0, AddRGB = 0 }),//

            new(EyeBase,
                new(0) { X = 13, Y = 9, ScaleY = 1, Rotation = 0, AddRGB = 0, PartId = 15, Alpha = 255 }, //
                new(65) { X = 13, Y = 9, ScaleY = 1, Rotation = 0, AddRGB = new(0, 0, 23), PartId = 15, Alpha = 255 },
                new(125) { X = 13, Y = 9, ScaleY = 1.3f, Rotation = Radians(3), AddRGB = new(0, 0, 55), PartId = 15, Alpha = 255 },
                new(165) { X = 9, Y = 11, ScaleY = 1.2f, Rotation = Radians(3), AddRGB = new(0, 0, 80), PartId = 15, Alpha = 255 },
                new(166) { X = 9, Y = 11, ScaleY = 1.2f, Rotation = Radians(3), AddRGB = new(0, 0, 80), PartId = 16, Alpha = 255 },
                new(225) { X = 18, Y = 9, ScaleY = 1.2f, Rotation = Radians(3), AddRGB = new(0, 0, 80), PartId = 16, Alpha = 255 },
                new(260) { X = 13, Y = 9, ScaleY = 1.3f, Rotation = Radians(3), AddRGB = new(0, 0, 80), PartId = 16, Alpha = 255 },
                new(450) { X = 13, Y = 9, ScaleY = 1, Rotation = 0, AddRGB = 0, PartId = 16, Alpha = 255 }),//

            new(WhiteGlow,
                new(0) { X = 105, Y = -40, ScaleX = 1.5f, ScaleY = 0.5f, Rotation = Radians(-30), Alpha = 255, AddRGB = new(200, -100, 0) },
                new(100) { X = 32, Y = 6, ScaleX = 1.65f, ScaleY = 0.5f, Rotation = Radians(-20), Alpha = 255, AddRGB = new(200, -100, 0) },
                new(225) { X = 32, Y = 10, ScaleX = 1.8f, ScaleY = 0.8f, Rotation = Radians(-9), Alpha = 255, AddRGB = new(200, -100, 0) },
                new(325) { X = 42, Y = 0, ScaleX = 2, ScaleY = 2, Rotation = 0, Alpha = 255, AddRGB = new(200, -100, 0) },
                new(650) { X = 32, Y = 8, ScaleX = 1.4f, ScaleY = 1, Rotation = Radians(-6), Alpha = 60, AddRGB = new(200, -100, 0) })//
        ];

        Eyeball.SetAlpha(0);
        BlackSpot.SetAlpha(0);
    }

    public void HalfToOpen()
    {
        Animator +=
        [
            new(EyeFrame,
                new(0) { X = 0, Y = 0, AddRGB = 0 },
                new(225) { X = 0, Y = 0, AddRGB = new(100, 0, 0) },
                new(450) { X = 0, Y = 0, AddRGB = 0 }),//

            new(EyeBase,
                new(0) { X = 13, Y = 9, ScaleY = 1, Rotation = Radians(-2), AddRGB = 0, PartId = 18, Alpha = 255 },
                new(90) { X = 13, Y = 9, ScaleY = 1.1f, Rotation = Radians(1), AddRGB = new(50, 0, 0), PartId = 18, Alpha = 255 },
                new(125) { X = 13, Y = 9, ScaleY = 0.95f, Rotation = Radians(-2), AddRGB = new(65, 0, 0), PartId = 18, Alpha = 255 },
                new(225) { X = 13, Y = 9, ScaleY = 1.4f, Rotation = Radians(6), AddRGB = new(80, 0, 0), PartId = 18, Alpha = 255 },
                new(360) { X = 13, Y = 9, ScaleY = 1.8f, Rotation = Radians(20), AddRGB = 0, PartId = 18, Alpha = 0 }),//

            new(Eyeball,
                new(0) { X = 29, Y = 6, Scale = 2.4f, Alpha = 200, AddRGB = new(100, 0, 50) },
                new(100) { X = 29, Y = 6, Scale = 1.4f, Alpha = 255, AddRGB = 0 },
                new(125) { X = 31, Y = 5, Scale = 1.2f, Alpha = 255, AddRGB = new(70, 0, 34) },
                new(190) { X = 26, Y = 6, Scale = 1, Alpha = 255, AddRGB = new(60, 0, 30) },
                new(250) { X = 29, Y = 6, Scale = 1, Alpha = 255, AddRGB = new(0, -10, -5) }),//

            new(BlackSpot,
                new(0) { Y = 4, Scale = 3, Alpha = 255, AddRGB = new(100, 0, 100) },
                new(125) { Y = 4, Scale = 1.5f, Alpha = 255, AddRGB = new(100, 100, 200) },
                new(300) { Y = -20, Scale = 2, Alpha = 0, AddRGB = new(200, 0, 0) }),//

            new(WhiteGlow,
                new(0) { X = 105, Y = -40, ScaleX = 1.5f, ScaleY = 0.5f, Rotation = Radians(-30), Alpha = 255, AddRGB = new(255, -71, -71) },
                new(85) { X = 36, Y = 5, ScaleX = 1.75f, ScaleY = 0.85f, Rotation = Radians(-12), Alpha = 255, AddRGB = new(255, -200, -200) },
                new(300) { X = 38, Y = 2, ScaleX = 2, ScaleY = 2, Rotation = 0, Alpha = 0, AddRGB = new(255, -200, -200) })//

        ];
    }

    public void OpenToShut()
    {
        Animator +=
        [
            new(EyeFrame,
                new(0) { X = 0, Y = 0, AddRGB = 0 },
                new(230) { X = 0, Y = 0, AddRGB = new(80, 0, 0) },
                new(560) { X = 0, Y = 0, AddRGB = 0 }),

            new(EyeBase,
                new(0) { X = 13, Y = 9, ScaleY = 1.2f, Rotation = Radians(12), Alpha = 128, AddRGB = new(100, 50, 50), PartId = 17 },
                new(75) { X = 13, Y = 9, ScaleY = 1, Rotation = 0, Alpha = 153, AddRGB = new(100, 21, 21), PartId = 17 },
                new(76) { X = 13, Y = 9, ScaleY = 1, Rotation = 0, Alpha = 153, AddRGB = new(100, 21, 21), PartId = 16 },
                new(99) { X = 13, Y = 9, ScaleY = 1, Rotation = 0, Alpha = 153, AddRGB = new(80, 10, 10), PartId = 16 },
                new(100) { X = 13, Y = 9, ScaleY = 1, Rotation = 0, Alpha = 153, AddRGB = new(80, 10, 10), PartId = 15 },
                new(275) { X = 13, Y = 9, ScaleY = 1, Rotation = 0, Alpha = 255, AddRGB = 0, PartId = 15 },
                new(650) { X = 13, Y = 9, ScaleY = 1, Rotation = 0, Alpha = 255, AddRGB = 0, PartId = 15 }),

            new(Eyeball,
                new(0) { X = 29, Y = 6, Scale = 1, Alpha = 255, AddRGB = new(55, 44, 44) },
                new(100) { X = 29, Y = 6, Scale = 1.5f, Alpha = 255, AddRGB = new(100, -1, -1) },
                new(400) { X = 29, Y = 6, Scale = 2, Alpha = 0, AddRGB = new(-1, -1, -1) }),

            new(BlackSpot,
                new(0) { Y = 5, Scale = 1, Alpha = 0, AddRGB = new(100, 0, 100) },
                new(100) { Y = 5, Scale = 2f, Alpha = 175, AddRGB = new(200, 0, 0) },
                new(300) { Y = 5, Scale = 4, Alpha = 0, AddRGB = new(200, 0, 0) })
        ];

        WhiteGlow.SetAlpha(0);

    }

    private void HalfToShut()
    {
        Animator +=
        [
            new(EyeFrame,
                new(0) { X = 0, Y = 0, AddRGB = 0 },
                new(230) { X = 0, Y = 0, AddRGB = new(100, 0, 0) },
                new(560) { X = 0, Y = 0, AddRGB = 0 }),

            new(EyeBase,
                new(0) { X = 13, Y = 9, ScaleY = 1, Rotation = 0, Alpha = 255, AddRGB = 0, PartId = 16  },
                new(75) { X = 13, Y = 9, ScaleY = 1, Rotation = 0, Alpha = 153, AddRGB = new(0, 0, 80), PartId = 16 },
                new(99) { X = 13, Y = 9, ScaleY = 1, Rotation = 0, Alpha = 153, AddRGB = new(0, 0, 55), PartId = 16 },
                new(100) { X = 13, Y = 9, ScaleY = 1, Rotation = 0, Alpha = 153, AddRGB = new(0, 0, 55), PartId = 15 },
                new(275) { X = 13, Y = 9, ScaleY = 1, Rotation = 0, Alpha = 255, AddRGB = 0, PartId = 15 }),

            new(BlackSpot,
                new(0) { Y = 5, Scale = 1, Alpha = 0, AddRGB = new(100, 0, 100) },
                new(100) { Y = 5, Scale = 2f, Alpha = 175, AddRGB = new(100, 0, 100) },
                new(300) { Y = 5, Scale = 4, Alpha = 0, AddRGB = new(100, 0, 200) }) //
        ];

        Eyeball.SetAlpha(0);
        WhiteGlow.SetAlpha(0);
    }

    private void ShutToOpen()
    {
        Animator +=
        [
            new(EyeFrame,
                new(0) { X = 0, Y = 0, AddRGB = 0 }, //
                new(75) { X = -4, Y = 3, AddRGB = 0 },
                new(135) { X = -4, Y = 0, AddRGB = 0 },
                new(200) { X = 0, Y = 0, AddRGB = 0 }),

            new(Eyeball,
                new(0) { X = 29, Y = 6, Scale = 2.4f, Alpha = 200, AddRGB = new(100, 0, 50) },
                new(100) { X = 29, Y = 6, Scale = 1.4f, Alpha = 255, AddRGB = 0 },
                new(125) { X = 31, Y = 5, Scale = 1.2f, Alpha = 255, AddRGB = new(70, 0, 34) },
                new(190) { X = 26, Y = 6, Scale = 1, Alpha = 255, AddRGB = new(60, 0, 30) },
                new(250) { X = 29, Y = 6, Scale = 1, Alpha = 255, AddRGB = new(0, -10, -5) }),

            new(EyeBase,
                new(0) { X = 13, Y = 9, ScaleY = 1, Rotation = Radians(-2), AddRGB = 0, PartId = 18, Alpha = 255 },
                new(90) { X = 13, Y = 9, ScaleY = 1.1f, Rotation = Radians(1), AddRGB = new(50, 0, 0), PartId = 18, Alpha = 255 },
                new(125) { X = 13, Y = 9, ScaleY = 0.95f, Rotation = Radians(-2), AddRGB = new(65, 0, 0), PartId = 18, Alpha = 255 },
                new(225) { X = 13, Y = 9, ScaleY = 1.4f, Rotation = Radians(6), AddRGB = new(80, 0, 0), PartId = 18, Alpha = 255 },
                new(360) { X = 13, Y = 9, ScaleY = 1.8f, Rotation = Radians(20), AddRGB = 0, PartId = 18, Alpha = 0 }),//

            new(BlackSpot,
                new(0) { Y = 4, Scale = 3, Alpha = 255, AddRGB = new(100, 0, 100) },
                new(125) { Y = 4, Scale = 1.5f, Alpha = 255, AddRGB = new(100, 100, 200) },
                new(300) { Y = -20, Scale = 2, Alpha = 0, AddRGB = new(200, 0, 0) }), //

            new(WhiteGlow,
                new(0) { X = 105, Y = -40, ScaleX = 1.5f, ScaleY = 0.5f, Rotation = Radians(-30), Alpha = 255, AddRGB = new(255, -71, -71) },
                new(85) { X = 36, Y = 5, ScaleX = 1.75f, ScaleY = 0.85f, Rotation = Radians(-12), Alpha = 255, AddRGB = new(255, -200, -200) },
                new(300) { X = 38, Y = 2, ScaleX = 2, ScaleY = 2, Rotation = 0, Alpha = 0, AddRGB = new(255, -200, -200) })//
        ];
    }

    private void OpenToHalf()
    {
        Animator +=
        [
            new(EyeFrame,
                new(0) { X = 0, Y = 0, AddRGB = 0 },
                new(230) { X = 0, Y = 0, AddRGB = new(80, 0, 0) },
                new(560) { X = 0, Y = 0, AddRGB = 0 }),

            new(EyeBase,
                new(0) { X = 13, Y = 9, ScaleY = 1.8f, Rotation = Radians(20), AddRGB = 0, PartId = 18, Alpha = 0 },
                new(135) { X = 13, Y = 9, ScaleY = 1.4f, Rotation = Radians(6), AddRGB = new(80, 0, 0), PartId = 18, Alpha = 255 },
                new(235) { X = 13, Y = 9, ScaleY = 0.95f, Rotation = Radians(-2), AddRGB = new(65, 0, 0), PartId = 18, Alpha = 255 },
                new(270) { X = 13, Y = 9, ScaleY = 1.1f, Rotation = Radians(1), AddRGB = new(50, 0, 0), PartId = 18, Alpha = 255 },
                new(360) { X = 13, Y = 9, ScaleY = 1, Rotation = 0, AddRGB = 0, PartId = 16, Alpha = 255 }),

            new(Eyeball,
                new(0) { X = 29, Y = 6, Scale = 1, Alpha = 255, AddRGB = new(55, 44, 44) },
                new(100) { X = 29, Y = 6, Scale = 1.5f, Alpha = 255, AddRGB = new(100, -1, -1) },
                new(400) { X = 29, Y = 6, Scale = 2, Alpha = 0, AddRGB = new(-1, -1, -1) }),

            new(BlackSpot,
                new(0) { Y = 5, Scale = 1, Alpha = 0, AddRGB = new(100, 0, 100) },
                new(100) { Y = 5, Scale = 2f, Alpha = 175, AddRGB = new(200, 0, 0) },
                new(300) { Y = 5, Scale = 4, Alpha = 0, AddRGB = new(200, 0, 0) }) //
        ];

        WhiteGlow.SetAlpha(0);
    }

    #endregion

    #region UpdateFuncs

    public override void Update()
    {
        if (FirstRun) { OnFirstRun(Tracker.CurrentData.State); FirstRun = false; }
        else
        {
            var curEyeState = Config.EyeStates[Tracker.CurrentData.State];
            var prevEyeState = Config.EyeStates[Tracker.PreviousData.State];

            if (curEyeState != prevEyeState)
            {
                Animator -= EyeFrame;
                Animator -= EyeBase;
                Animator -= Eyeball;
                Animator -= WhiteGlow;
                Animator -= BlackSpot;

                switch (prevEyeState)
                {
                    case Closed when curEyeState == HalfOpen:
                        ShutToHalf();
                        break;
                    case Closed when curEyeState == Open:
                        ShutToOpen();
                        break;
                    case HalfOpen when curEyeState == Open:
                        HalfToOpen();
                        break;
                    case HalfOpen when curEyeState == Closed:
                        HalfToShut();
                        break;
                    case Open when curEyeState == Closed:
                        OpenToShut();
                        break;
                    case Open when curEyeState == HalfOpen:
                        OpenToHalf();
                        break;
                }
            }
        }
        PostUpdate();
        Animator.RunTweens();
    }

    public override void OnFirstRun(int current)
    {

    }

    public override void Activate(int current)
    {

    }

    public override void Deactivate(int previous)
    {

    }

    public override void StateChange(int current, int previous)
    {

    }

    #endregion

    #region Configs

    public class DragonEyeConfig : WidgetTypeConfig
    {
        public enum EyeState { Closed, HalfOpen, Open }

        public override Vector2 DefaultPosition => new(83, 89);

        public bool Mirror;
        public List<EyeState> EyeStates = [];

        public DragonEyeConfig(WidgetConfig widgetConfig) : base(widgetConfig.DragonEyeCfg)
        {
            var config = widgetConfig.DragonEyeCfg;

            if (config == null) return;

            Mirror = config.Mirror;
            EyeStates = config.EyeStates;
        }

        public DragonEyeConfig() { }

        public void FillLists(int maxState)
        {
            while (EyeStates.Count <= maxState) EyeStates.Add(EyeStates.Count switch { < 1 => Closed, < 2 => HalfOpen, _ => Open });
        }
    }

    private DragonEyeConfig config;

    public override DragonEyeConfig Config => config;

    internal static List<DragonEyeConfig.EyeState> EyeStateList = [Closed, HalfOpen, Open];
    public List<string> EyeStateNames = ["Closed", "Half-Open", "Open"];

    public override void InitConfigs()
    {
        config = new(Tracker.WidgetConfig);
        Config.FillLists(Tracker.CurrentData.MaxState);
    }

    public override void ResetConfigs()
    {
        config = new();
        Config.FillLists(Tracker.CurrentData.MaxState);
    }

    public override void ApplyConfigs()
    {
        WidgetContainer.SetPos(Config.Position)
                  .SetScale(Config.Mirror ? -Config.Scale : Config.Scale, Config.Scale);
    }

    public override void DrawUI()
    {
        base.DrawUI();
        switch (UiTab)
        {
            case Layout:
                ToggleControls("Mirror", ref Config.Mirror);
                break;
            case Behavior:
                var maxState = Tracker.CurrentData.MaxState;

                for (var i = 0; i <= maxState; i++)
                {
                    var label = $"{Tracker.StateNames[i]}";
                    var eyeState = Config.EyeStates[i];
                    if (RadioControls($"{label}##appearance{i}", ref eyeState, EyeStateList, EyeStateNames)) Config.EyeStates[i] = eyeState;
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
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] public DragonEyeConfig? DragonEyeCfg { get; set; }
}
