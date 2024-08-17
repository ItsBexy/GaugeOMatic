using CustomNodes;
using GaugeOMatic.CustomNodes.Animation;
using GaugeOMatic.Trackers;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Numerics;
using static CustomNodes.CustomNodeManager;
using static GaugeOMatic.CustomNodes.Animation.KeyFrame;
using static GaugeOMatic.CustomNodes.Animation.Tween.EaseType;
using static GaugeOMatic.Utility.Color;
using static GaugeOMatic.Widgets.Common.CommonParts;
using static GaugeOMatic.Widgets.ElementalCrystals;
using static GaugeOMatic.Widgets.ElementalCrystals.ElementalCrystalConfig.BaseColors;
using static GaugeOMatic.Widgets.WidgetTags;
using static GaugeOMatic.Widgets.WidgetUI;
using static GaugeOMatic.Widgets.WidgetUI.UpdateFlags;
using static GaugeOMatic.Widgets.WidgetUI.WidgetUiTab;

#pragma warning disable CS8618

namespace GaugeOMatic.Widgets;

public sealed unsafe class ElementalCrystals : FreeGemCounter
{
    public ElementalCrystals(Tracker tracker) : base(tracker) { }

    public override WidgetInfo WidgetInfo => GetWidgetInfo;

    public static WidgetInfo GetWidgetInfo { get; } = new()
    {
        DisplayName = "Elemental Crystals",
        Author = "ItsBexy",
        Description = "A counter based on BLM's element stack display.",
        WidgetTags = Counter | Replica | MultiComponent,
        MultiCompData = new("EL", "Elemental Gauge Replica", 4),
        UiTabOptions = Layout | Colors | Behavior
    };

    public override CustomPartsList[] PartsLists { get; } = { BLM0 };

    #region Nodes

    public List<CustomNode> StackContents;
    public List<CustomNode> Crystals;
    public List<CustomNode> Glows1;
    public List<CustomNode> Glows2;

    public override CustomNode BuildContainer()
    {
        Max = GetMax();
        BuildStacks(Max);

        return new CustomNode(CreateResNode(), Stacks.ToArray()).SetOrigin(16, 16);
    }

    private void BuildStacks(int count)
    {
        Stacks = new();
        Crystals = new();
        Glows1 = new();
        Glows2 = new();
        StackContents = new();

        for (var i = 0; i < count; i++)
        {
            Crystals.Add(ImageNodeFromPart(0, 4).SetOrigin(10, 24));
            Glows1.Add(ImageNodeFromPart(0, 7).SetOrigin(10, 24).SetAlpha(0));
            Glows2.Add(ImageNodeFromPart(0, 7).SetOrigin(10, 24).SetAlpha(0).SetScale(1.3f, 1.2f).SetAlpha(0).Hide());
            StackContents.Add(new CustomNode(CreateResNode(),
                                             Crystals[i],
                                             Glows1[i],
                                             new (CreateResNode(), Glows2[i])).SetAlpha(0));

            Animator += new Tween(Glows2[i],
                                  new(0) { ScaleX = 1, ScaleY = 1, Alpha = 0 },
                                  new(450) { ScaleX = 1.2f, ScaleY = 1.1f, Alpha = 101 },
                                  new(950) { ScaleX = 1.3f, ScaleY = 1.2f, Alpha = 0 },
                                  new(1600) { ScaleX = 1.3f, ScaleY = 1.2f, Alpha = 0 })
                                  { Repeat = true, Ease = SinInOut, Label = "Pulse" };
            Stacks.Add(new CustomNode(CreateResNode(), StackContents[i]).SetOrigin(10, 24));
        }
    }

    #endregion

    #region Animations

    public override void ShowStack(int i)
    {
        Animator += new Tween(StackContents[i],
                              new(0) { Y = -20, Alpha = 0 },
                              new(225) { Y = 0, Alpha = 200 },
                              new(300) { Y = 0, Alpha = 255 });

        Glows2[i].Show();
    }

    public override void HideStack(int i)
    {
        Glows2[i].Hide();

        Animator += new Tween[]
        {
            new(StackContents[i],
                Visible[0],
                Hidden[325]),
            new(Glows1[i],
                new(0) { Alpha = 0, ScaleX = 1.3f, ScaleY = 1.2f },
                new(50) { Alpha = 73, ScaleX = 1.2f, ScaleY = 1.1f },
                new(200) { Alpha = 0, ScaleX = 1, ScaleY = 1 })
        };
    }

    #endregion

    #region UpdateFuncs

    public override void OnFirstRun(int count, int max)
    {
        for (var i = 0; i < max; i++)
        {
            StackContents[i].SetAlpha(i < count);
            Glows2[i].SetVis(i < count);
        }
    }

    #endregion

    #region Configs

    public class ElementalCrystalConfig : FreeGemCounterConfig
    {
        public enum BaseColors { Ice = 3, Fire = 4 }

        public BaseColors BaseColor = Ice;
        public AddRGB CrystalColor = new(0);
        public AddRGB GlowColor = new(0);

        public ElementalCrystalConfig(WidgetConfig widgetConfig) : base(widgetConfig.ElementalCrystalCfg)
        {
            var config = widgetConfig.ElementalCrystalCfg;

            if (config == null) return;

            CrystalColor = config.CrystalColor;

            BaseColor = config.BaseColor;
            GlowColor = config.GlowColor;
        }

        public ElementalCrystalConfig()
        {
            Angle = -62;
            Curve = 18;
            Position = new(19, 22);
        }
    }

    public override FreeGemCounterConfig GetConfig => Config;

    public ElementalCrystalConfig Config;

    public override void InitConfigs() => Config = new(Tracker.WidgetConfig);

    public override void ResetConfigs() => Config = new();

    public override void ApplyConfigs()
    {
        WidgetContainer.SetPos(Config.Position + new Vector2(19, 22))
                       .SetScale(Config.Scale);

        PlaceFreeGems();

        for (var i = 0; i < Stacks.Count; i++)
        {
            Crystals[i].SetPartId((ushort)Config.BaseColor)
                       .SetAddRGB(Config.CrystalColor);

            Glows1[i].SetPartId((ushort)(Config.BaseColor + 3))
                     .SetAddRGB(Config.GlowColor);

            Glows2[i].SetPartId((ushort)(Config.BaseColor + 3))
                     .SetAddRGB(Config.GlowColor);
        }
    }

    public override string StackTerm => "Crystal";
    public override void DrawUI(ref WidgetConfig widgetConfig)
    {
        base.DrawUI(ref widgetConfig);

        switch (UiTab)
        {
            case Layout:
                break;
            case Colors:
                RadioControls("Base Color", ref Config.BaseColor, new() { Ice, Fire }, new() { "Ice", "Fire" }, true);
                ColorPickerRGB("Color Modifier", ref Config.CrystalColor);
                ColorPickerRGB("Glow Color", ref Config.GlowColor);
                break;
            case Behavior:
                break;
            default:
                break;
        }

        if (UpdateFlag.HasFlag(Save)) ApplyConfigs();
        widgetConfig.ElementalCrystalCfg = Config;
    }

    #endregion
}

public partial class WidgetConfig
{
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] public ElementalCrystalConfig? ElementalCrystalCfg { get; set; }
}
