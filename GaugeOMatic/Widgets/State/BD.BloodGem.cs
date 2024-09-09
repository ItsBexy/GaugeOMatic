using CustomNodes;
using GaugeOMatic.CustomNodes.Animation;
using GaugeOMatic.Trackers;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using static CustomNodes.CustomNodeManager;
using static GaugeOMatic.CustomNodes.Animation.Tween.EaseType;
using static GaugeOMatic.Utility.Color;
using static GaugeOMatic.Widgets.BloodGem;
using static GaugeOMatic.Widgets.Common.CommonParts;
using static GaugeOMatic.Widgets.WidgetTags;
using static GaugeOMatic.Widgets.WidgetUI;
using static GaugeOMatic.Widgets.WidgetUI.WidgetUiTab;

#pragma warning disable CS8618

namespace GaugeOMatic.Widgets;

[WidgetName("Blood Gem")]
[WidgetDescription("A widget recreating the tank stance gem for DRK.")]
[WidgetAuthor("ItsBexy")]
[WidgetTags(State | Replica | MultiComponent)]
[WidgetUiTabs(Layout | Colors)]
[MultiCompData("BD", "Blood Gauge Replica", 1)]
public sealed unsafe class BloodGem(Tracker tracker) : StateWidget(tracker)
{
    public override CustomPartsList[] PartsLists { get; } = [DRK0];

    #region Nodes

    public CustomNode Ring;
    public CustomNode GemWrapper;
    public CustomNode BlueHalo;
    public CustomNode Gem;
    public CustomNode PinkHalo;

    public override CustomNode BuildContainer()
    {
        Ring = ImageNodeFromPart(0, 0).SetPos(-16, -19)
                                      .SetOrigin(48, 44)
                                      .SetImageWrap(1);

        BlueHalo = ImageNodeFromPart(0, 8).SetOrigin(50, 50)
                                          .SetAddRGB(-146, 9, 245)
                                          .SetAlpha(0);

        Gem = ImageNodeFromPart(0, 11).SetPos(23, 25)
                                      .SetOrigin(28, 28)
                                      .SetImageWrap(1);

        GemWrapper = new CustomNode(CreateResNode(), BlueHalo, Gem).SetPos(-22, -26)
                                                                   .SetOrigin(50, 50);

        PinkHalo = ImageNodeFromPart(0, 9).SetAddRGB(-255)
                                          .SetAlpha(0)
                                          .SetPos(-12, -16)
                                          .SetOrigin(40, 40)
                                          .SetImageWrap(1)
                                          .SetImageFlag(32);

        return new(CreateResNode(), Ring, GemWrapper, PinkHalo);
    }

    #endregion

    #region Animations

    private void ActivateAnim(AddRGB flashColor, AddRGB gemColor) =>
        Animator +=
        [
            new(Gem,
                new(0) { AddRGB = new(0, -8, -8), MultRGB = new(100, 100, 100), PartId = 11 },
                new(99.9f) { AddRGB = flashColor, MultRGB = new(100, 80, 80), PartId = 11 },
                new(100.1f) { AddRGB = gemColor + flashColor, MultRGB = new(100, 80, 80), PartId = 5 },
                new(175) { AddRGB = gemColor, MultRGB = new(100, 100, 100), PartId = 5 })
                { Ease = SinInOut },

            new(BlueHalo,
                new(0) { Scale = 0.5f },
                new(150) { Scale = 1 })
        ];

    private void BeginHaloSpin()
    {
        Animator -= "HaloSpin";
        Animator += new Tween(BlueHalo,
                              new(0) { Rotation = 0 },
                              new(975) { Rotation = 0.785398163397448f },
                              new(1950) { Rotation = 1.5707963267949f })
        { Repeat = true, Label = "HaloSpin" };
    }

    private void BeginHaloPulse(int current)
    {
        var haloColor = Config.GetHaloColor(current) + HaloOffset;
        var haloColor2 = Config.GetHaloColor2(current) + HaloOffset;

        Animator -= "HaloPulse";
        Animator +=
        [
            new(BlueHalo,
                new(0) { Alpha = 229, AddRGB = haloColor },
                new(975) { Alpha = 255, AddRGB = haloColor2 },
                new(1950) { Alpha = 229, AddRGB = haloColor })
                { Repeat = true, Label = "HaloPulse" }
        ];
    }

    #endregion

    #region UpdateFuncs

    public override void OnFirstRun(int current)
    {
        if (current > 0)
        {
            Gem.SetPartId(5)
               .SetAddRGB(Config.GetGemColor(current) + GemOffset);

            BeginHaloSpin();
            BeginHaloPulse(current);
        }
    }

    public override void Activate(int current)
    {
        var gemColor = Config.GetGemColor(current) + GemOffset;

        var flashColor = gemColor.Transform(-80, 0);

        ActivateAnim(flashColor, gemColor);

        BeginHaloSpin();
        BeginHaloPulse(current);
    }

    public override void Deactivate(int previous)
    {
        Animator -= Gem;
        Animator -= BlueHalo;

        Gem.SetPartId(11).SetAddRGB(0);
        BlueHalo.SetAlpha(0).SetScale(0.5f);
    }

    public override void StateChange(int current, int previous)
    {
        Animator += new Tween(Gem, new(0, Gem), new(150) { AddRGB = Config.GetGemColor(current) + GemOffset });
        BeginHaloPulse(current);
    }

    #endregion

    #region Configs

    public class BloodGemConfig : WidgetTypeConfig
    {
        public List<AddRGB> GemColors = [];
        public List<AddRGB> HaloColors = [];
        public List<AddRGB> HaloColors2 = [];
        public ColorRGB RingColor = new(100, 100, 100);
        public bool Ring;

        public BloodGemConfig(WidgetConfig widgetConfig) : base(widgetConfig.BloodGemCfg)
        {
            var config = widgetConfig.BloodGemCfg;

            if (config == null) return;

            GemColors = config.GemColors;
            HaloColors = config.HaloColors;
            HaloColors2 = config.HaloColors2;
            RingColor = config.RingColor;
        }

        public BloodGemConfig() { }

        public AddRGB GetGemColor(int state) => GemColors.ElementAtOrDefault(state);
        public AddRGB GetHaloColor(int state) => HaloColors.ElementAtOrDefault(state);
        public AddRGB GetHaloColor2(int state) => HaloColors2.ElementAtOrDefault(state);

        public void FillColorLists(int max)
        {
            while (GemColors.Count <= max) GemColors.Add(new(60, -63, -41));
            while (HaloColors.Count <= max) HaloColors.Add(new(-72, 15, 174));
            while (HaloColors2.Count <= max) HaloColors2.Add(new(-222, 24, 423));
        }
    }

    private BloodGemConfig config;

    public override BloodGemConfig Config => config;

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

    public AddRGB GemOffset = new(-60, 63, 41);
    public AddRGB HaloOffset = new(72, -15, -74);
    public override void ApplyConfigs()
    {
        WidgetContainer.SetPos(Config.Position + new Vector2(14));
        WidgetContainer.SetScale(Config.Scale);

        Ring.SetMultiply(Config.RingColor).SetAlpha(Config.Ring);

        var state = Tracker.CurrentData.State;
        if (state > 0)
        {
            Gem.SetAddRGB(Config.GetGemColor(state) + GemOffset);
            BeginHaloPulse(state);
        }
    }

    public override void DrawUI()
    {
        base.DrawUI();
        Config.FillColorLists(Tracker.CurrentData.MaxState);
        switch (UiTab)
        {
            case Layout:
                ToggleControls("Show Ring", ref Config.Ring);
                break;
            case Colors:
                if (Config.Ring) ColorPickerRGB("Ring Tint", ref Config.RingColor);

                for (var i = 1; i <= Tracker.CurrentData.MaxState; i++)
                {
                    Heading(Tracker.StateNames[i]);

                    var gemColor = Config.GemColors[i];
                    var haloColor = Config.HaloColors[i];
                    var haloColor2 = Config.HaloColors2[i];
                    if (ColorPickerRGB($"Gem Color##gemColor{i}", ref gemColor)) Config.GemColors[i] = gemColor;
                    if (ColorPickerRGB($"Halo Color##haloColor{i}", ref haloColor)) Config.HaloColors[i] = haloColor;
                    if (ColorPickerRGB($" ##haloColor2{i}", ref haloColor2)) Config.HaloColors2[i] = haloColor2;
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
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public BloodGemConfig? BloodGemCfg { get; set; }
}
