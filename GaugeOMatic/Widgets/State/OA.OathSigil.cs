using CustomNodes;
using GaugeOMatic.CustomNodes.Animation;
using GaugeOMatic.Trackers;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using static CustomNodes.CustomNode;
using static CustomNodes.CustomNodeManager;
using static GaugeOMatic.CustomNodes.Animation.Tween.EaseType;
using static GaugeOMatic.Utility.Color;
using static GaugeOMatic.Widgets.OathSigil;
using static GaugeOMatic.Widgets.WidgetTags;
using static GaugeOMatic.Widgets.WidgetUI;
using static GaugeOMatic.Widgets.WidgetUI.UpdateFlags;
using static GaugeOMatic.Widgets.WidgetUI.WidgetUiTab;

#pragma warning disable CS8618

namespace GaugeOMatic.Widgets;

public sealed unsafe class OathSigil : StateWidget
{
    public OathSigil(Tracker tracker) : base(tracker) { }

    public override WidgetInfo WidgetInfo => GetWidgetInfo;

    public static WidgetInfo GetWidgetInfo { get; } = new()
    {
        DisplayName = "Oath Sigil",
        Author = "ItsBexy",
        Description = "A glowing winged shield sigil recreating Paladin's tank stance indicator.",
        WidgetTags = State | Replica | MultiComponent,
        MultiCompData = new("OA", "Oath Gauge Replica", 1),
        UiTabOptions = Layout | Colors
    };

    public override CustomPartsList[] PartsLists { get; } =
    {
        new("ui/uld/JobHudPLD.tex",
            new (0, 120, 180, 180),  // 0 sigil
            new(316, 306, 78, 52))   // 1 wing
    };

    #region Nodes

    public CustomNode SigilWrapper;
    public CustomNode Sigil;
    public CustomNode WingR;
    public CustomNode WingL;

    public override CustomNode BuildContainer()
    {
        Sigil = ImageNodeFromPart(0, 0).SetOrigin(90, 90).SetAlpha(0).SetScale(0).SetY(50).SetImageFlag(32);
        SigilWrapper = new CustomNode(CreateResNode(), Sigil).SetSize(180,180);

        WingR = ImageNodeFromPart(0, 1).SetPos(107, 42).SetOrigin(8, 52).SetScale(0).SetAlpha(0).SetImageWrap(1).SetImageFlag(1);
        WingL = ImageNodeFromPart(0, 1).SetPos(-6, 42).SetOrigin(70, 52).SetScale(0).SetAlpha(0);

        Animator += new Tween(Sigil,
                              new(0) { AddRGB = new(-127,-112,-36) },
                              new(500) { AddRGB = new(-107, -92, -16) },
                              new(960) { AddRGB = new(-127, -112, -36) })
                              { Repeat = true, Ease = SinInOut };

        return new(CreateResNode(), SigilWrapper, WingR, WingL);
    }

    #endregion

    #region Animations

    #endregion

    #region UpdateFuncs

    public override void OnFirstRun(int current)
    {
        WingL.SetVis(Config.IncludeWings);
        WingR.SetVis(Config.IncludeWings);
        if (current > 0)
        {
            SigilWrapper.SetAddRGB(Config.SigilColors.ElementAtOrDefault(current));
            Sigil.SetScale(1).SetAlpha(255).SetY(0);
            WingL.SetScale(1).SetAlpha(255);
            WingR.SetScale(1).SetAlpha(255);
        }
    }

    public override void Activate(int current)
    {
        var sigilColor = Config.SigilColors.ElementAtOrDefault(current);
        var wingColor = Config.WingColors.ElementAtOrDefault(current);
        SigilWrapper.SetAddRGB(sigilColor);
        WingL.SetMultiply(wingColor);
        WingR.SetMultiply(wingColor);

        Animator += new Tween[]
        {
            new(Sigil,
                new(0) { Y = 50, Scale = 0, Alpha = 0 },
                new(160) { Y = 0, Scale = 1, Alpha = 255 }),
            new(WingL,
                new(0) { Scale = 0, Alpha = 0 },
                new(80) { ScaleX = 1, ScaleY = 0.2f, Alpha = 100 },
                new(160) { Scale = 1, Alpha = 255 }),
            new(WingR,
                new(0) { Scale = 0, Alpha = 0 },
                new(80) { ScaleX = 1, ScaleY = 0.2f, Alpha = 100 },
                new(160) { Scale = 1, Alpha = 255 })
        };

    }

    public override void Deactivate(int previous) =>
        Animator += new Tween[]
        {
            new(Sigil,
                new(0) { Y = 0, Scale = 1, Alpha = 255 },
                new(160) { Y = 50, Scale = 0, Alpha = 0 }),
            new(WingL,
                new(0) { Scale = 1, Alpha = 255 },
                new(80) { ScaleX = 1, ScaleY = 0.2f, Alpha = 100 },
                new(160) { Scale = 0, Alpha = 0 }),
            new(WingR,
                new(0) { Scale = 1, Alpha = 255 },
                new(80) { ScaleX = 1, ScaleY = 0.2f, Alpha = 100 },
                new(160) { Scale = 0, Alpha = 0 })
        };

    public override void StateChange(int current, int previous)
    {
        var wingColor = Config.WingColors.ElementAtOrDefault(current);
        Animator += new Tween[]
        {
            new(SigilWrapper, new(0, SigilWrapper), new(160) { AddRGB = Config.SigilColors.ElementAtOrDefault(current) }),
            new(WingL, new(0, WingL), new(160) { MultRGB = wingColor }),
            new(WingR, new(0, WingR), new(160) { MultRGB = wingColor })
        };
    }

    #endregion

    #region Configs

    public class OathSigilConfig : WidgetTypeConfig
    {
        public List<AddRGB> SigilColors = new();
        public List<ColorRGB> WingColors = new();
        [DefaultValue(true)] public bool IncludeWings = true;
        public byte BlendMode;

        public OathSigilConfig(WidgetConfig widgetConfig) : base(widgetConfig.OathSigilCfg)
        {
            var config = widgetConfig.OathSigilCfg;

            if (config == null) return;

            IncludeWings = config.IncludeWings;
            SigilColors = config.SigilColors;
            WingColors = config.WingColors;
            BlendMode = config.BlendMode;
        }

        public OathSigilConfig() { }

        public void FillColorLists(int max)
        {
            while (SigilColors.Count <= max) SigilColors.Add(new(127, 112, 36));
            while (WingColors.Count <= max) WingColors.Add(new(100));
        }
    }

    public OathSigilConfig Config;
    public override WidgetTypeConfig GetConfig => Config;

    public override void InitConfigs()
    {
        Config = new(Tracker.WidgetConfig);
        Config.FillColorLists(Tracker.CurrentData.MaxState);
    }

    public override void ResetConfigs()
    {
        Config = new();
        Config.FillColorLists(Tracker.CurrentData.MaxState);
    }

    public override void ApplyConfigs()
    {
        WidgetContainer.SetPos(Config.Position+new Vector2(30.5F, 0))
                  .SetScale(Config.Scale);

        var state = Tracker.CurrentData.State;
        SigilWrapper.SetAddRGB(Config.SigilColors.ElementAtOrDefault(state));
        Sigil.SetImageFlag(Config.BlendMode);

        var wingColor = Config.WingColors.ElementAtOrDefault(state);
        WingL.SetMultiply(wingColor);
        WingR.SetMultiply(wingColor);

        WingL.SetVis(Config.IncludeWings);
        WingR.SetVis(Config.IncludeWings);
    }

    public override void DrawUI()
    {
        switch (UiTab)
        {
            case Layout:
                PositionControls("Position", ref Config.Position);
                ScaleControls("Scale", ref Config.Scale);
                ToggleControls("Show Wings", ref Config.IncludeWings);
                break;
            case Colors:
                RadioControls("Blend Mode", ref Config.BlendMode, new() { 0, 32 }, new() { "Normal", "Dodge" }, true);

                for (var i = 1; i <= Tracker.CurrentData.MaxState; i++)
                {
                    Heading(Tracker.StateNames[i]);

                    var sigilColor = Config.SigilColors[i];
                    var wingColor = Config.WingColors[i];
                    if (ColorPickerRGB($"Sigil Color##sigilColor{i}", ref sigilColor)) Config.SigilColors[i] = sigilColor;
                    if (Config.IncludeWings && ColorPickerRGB($"Wing Tint##wingColor{i}", ref wingColor)) Config.WingColors[i] = wingColor;
                }

                break;
            default:
                break;
        }

        if (UpdateFlag.HasFlag(Save))
        {
            ApplyConfigs();
            Config.WriteToTracker(Tracker);
        }
    }

    public override Bounds GetBounds() => SigilWrapper;

    #endregion
}

public partial class WidgetConfig
{
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] public OathSigilConfig? OathSigilCfg { get; set; }
}
