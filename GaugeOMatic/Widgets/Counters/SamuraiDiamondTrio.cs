using CustomNodes;
using GaugeOMatic.CustomNodes.Animation;
using GaugeOMatic.Trackers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Numerics;
using static CustomNodes.CustomNode.CustomNodeFlags;
using static CustomNodes.CustomNodeManager;
using static CustomNodes.CustomNodeManager.CustomPartsList;
using static GaugeOMatic.CustomNodes.Animation.Tween.EaseType;
using static GaugeOMatic.GaugeOMatic;
using static GaugeOMatic.Utility.Color;
using static GaugeOMatic.Widgets.Common.CommonParts;
using static GaugeOMatic.Widgets.CounterWidgetConfig.CounterPulse;
using static GaugeOMatic.Widgets.SamuraiDiamondTrio;
using static GaugeOMatic.Widgets.WidgetTags;
using static GaugeOMatic.Widgets.WidgetUI;
using static GaugeOMatic.Widgets.WidgetUI.UpdateFlags;
using static GaugeOMatic.Widgets.WidgetUI.WidgetUiTab;
using static System.IO.Path;

#pragma warning disable CS8618

namespace GaugeOMatic.Widgets;

public sealed unsafe class SamuraiDiamondTrio : CounterWidget
{
    public SamuraiDiamondTrio(Tracker tracker) : base(tracker) { }

    public override WidgetInfo WidgetInfo => GetWidgetInfo;

    public static WidgetInfo GetWidgetInfo { get; } = new()
    {
        DisplayName = "Meditation Gems",
        Author = "ItsBexy",
        Description = "A counter imitating the Meditation Stack display on Samurai's Kenki Gauge. Appropriate for anything that stacks up to exactly 3.",
        WidgetTags = Counter | Replica,
        UiTabOptions = Layout | Colors | Behavior
    };

    public override CustomPartsList[] PartsLists { get; } = { SAM0, new(AssetFromFile(Combine(PluginDirPath, @"TextureAssets\MedDiamondSingleFrame.tex")),new Vector4(0,0,64,64)) };

    #region Nodes

    public List<CustomNode> Stacks;
    public List<CustomNode> Gems;
    public List<CustomNode> Glows;
    public List<CustomNode> Glows2;
    public List<CustomNode> Pulsars;
    public List<CustomNode> Frames;
    public CustomNode Plate;

    public override CustomNode BuildContainer()
    {
        Max = GetMax();

        Stacks = BuildStacks();

        Plate = ImageNodeFromPart(0, 10).SetScale(Max>=3?1:0).SetOrigin(51,0);

        return new CustomNode(CreateResNode(),Plate,new CustomNode(CreateResNode(), Stacks.ToArray())).SetOrigin(51, 31).SetSize(102,62);
    }

    private List<CustomNode> BuildStacks()
    {
        var stacks = new List<CustomNode>();

        Gems = new();
        Glows = new();
        Glows2 = new();
        Pulsars = new();
        Frames = new();

        for (var i = 0; i < Max; i++)
        {
            Frames.Add(ImageNodeFromPart(1, 0).SetSize(32,32)
                                              .SetImageWrap(2)
                                              .SetPos(-4,-6)
                                              .SetOrigin(16,0));

            Gems.Add(ImageNodeFromPart(0, 11).SetOrigin(12, 10));

            Glows.Add(ImageNodeFromPart(0, 12).SetOrigin(23, 23)
                                              .SetPos(-11, -13)
                                              .SetAlpha(0)
                                              .SetImageFlag(32)
                                              .DefineTimeline(new(0) { Alpha = 0, Scale = 1 },
                                                              new(150) { Alpha = 255, Scale = 1 },
                                                              new(300) { Alpha = 0, Scale = 1 }));

            Glows2.Add(ImageNodeFromPart(0, 12).SetOrigin(23, 23)
                                               .SetPos(-11, -13)
                                               .RemoveFlags(SetVisByAlpha)
                                               .SetFlags(0)
                                               .SetAlpha(0)
                                               .SetImageFlag(32));

            Pulsars.Add(ImageNodeFromPart(0, 4).SetOrigin(95, 13)
                                               .SetPos(-82, -3)
                                               .SetAlpha(0)
                                               .SetScale(0.5f, 0.4f)
                                               .SetImageFlag(32)
                                               .DefineTimeline(new(0) { Alpha = 0, ScaleX = 0, ScaleY = 0.4f },
                                                               new(150) { Alpha = 255, ScaleX = 0.25f, ScaleY = 0.35f },
                                                               new(300) { Alpha = 0, ScaleX = 0.5f, ScaleY = 0.3f }));

            stacks.Add(new CustomNode(CreateResNode(), Frames[i], Gems[i], Glows[i], Glows2[i], Pulsars[i]).SetPos(16 + (23 * i), i % 2 == 0 ? 23 : 17));
        }

        return stacks;
    }

    #endregion

    #region Animations

    public override void ShowStack(int i)
    {
        if (Gems[i].Node->Color.A != 0) return;
        Glows2[i].Show();

        Animator += new Tween[] {
            new(Gems[i],
                new(0) { Scale = 2, Alpha = 0 },
                new(150) { Scale = 1, Alpha = 255 }),
            Glows[i],
            Pulsars[i]
        };
    }

    public override void HideStack(int i)
    {
        for (var j = i; j < Max; j++) Glows2[j].Hide();

        Animator += new Tween[] {
            new(Gems[i],
                new(0) { Scale = 1, Alpha = 255 },
                new(250) { Scale = 2, Alpha = 0 }),
            Glows[i],
            Pulsars[i]
        };
    }

    private void PlateAppear() =>
        Animator += new Tween(WidgetContainer,
                              new(0) { Scale = Config.Scale * 1.65f, Alpha = 0 },
                              new(200) { Scale = Config.Scale, Alpha = 255 })
        { Ease = SinInOut };

    private void PlateVanish() =>
        Animator += new Tween(WidgetContainer,
                              new(0) { Scale = Config.Scale, Alpha = 255 },
                              new(150) { Scale = Config.Scale * 0.65f, Alpha = 0 })
        { Ease = SinInOut };

    #endregion

    #region UpdateFuncs

    public override void OnFirstRun(int count, int max)
    {
        for (var i = 0; i < max; i++)
        {
            Gems[i].SetAlpha(i < count);
            Glows2[i].SetVis(i < count);
        }
    }

    public override void OnDecreaseToMin() { if (Config.HideEmpty) PlateVanish(); }
    public override void OnIncreaseFromMin() { if (Config.HideEmpty || WidgetContainer.Alpha < 255) PlateAppear(); }

    private void PulseAll()
    {
        Animator -= "Pulse";
        for (var i = 0; i < Max; i++)
            Animator += new Tween(Glows2[i],
                                  new(0) { Alpha = 0, Scale = 0f },
                                  new(230) { Alpha = 200, Scale = 1 },
                                  new(1125) { Alpha = 0, Scale = 1.3f })
            { Repeat = true, Label = "Pulse" };
    }

    private void StopPulseAll()
    {
        Animator -= "Pulse";
        for (var i = 0; i < Max; i++)
            Animator += new Tween(Glows2[i],
                                  new(0) { Alpha = Glows2[i].Alpha, Scale = Glows2[i].ScaleX },
                                  new(500) { Alpha = 0, Scale = 1.3f })
            { Repeat = false, Label = "Pulse" };
    }

    public bool CheckPulse(int i) => i > 0 && (Config.Pulse == Always || (Config.Pulse == AtMax && i == Max));

    public bool Pulsing;
    public override void PostUpdate(int i)
    {
        var checkPulse = CheckPulse(i);
        if (!Pulsing && checkPulse) PulseAll();
        else if (Pulsing && !checkPulse) StopPulseAll();
        Pulsing = checkPulse;
    }

    #endregion

    #region Configs

    public class SamuraiDiamondConfig : CounterWidgetConfig
    {
        public AddRGB? GemColor = null; // deprecated
        public AddRGB GemTint = new(9, -47, -91);
        public ColorRGB PlateTint = new(100);
        public bool HidePlate;
        public bool MirrorPlate;
        public int PlatePos;
        public bool HideEmpty;
        [DefaultValue(AtMax)] public CounterPulse Pulse = AtMax;

        public SamuraiDiamondConfig(WidgetConfig widgetConfig) : base(widgetConfig.SamuraiDiamondCfg)
        {
            var config = widgetConfig.SamuraiDiamondCfg;

            if (config == null) return;

            PlateTint = config.PlateTint;
            PlatePos = config.PlatePos;
            HidePlate = config.HidePlate;
            MirrorPlate = config.MirrorPlate;
            HideEmpty = config.HideEmpty;
            GemTint = config.GemColor == null ? config.GemTint : config.GemColor.Value - GemTintOffset;

            Pulse = config.Pulse;
        }

        public SamuraiDiamondConfig() { }
    }

    public override CounterWidgetConfig GetConfig => Config;

    public SamuraiDiamondConfig Config;

    public override void InitConfigs() => Config = new(Tracker.WidgetConfig);

    public override void ResetConfigs() => Config = new();

    private static AddRGB GemTintOffset = new(-9, 47, 91);
    public override void ApplyConfigs()
    {
        if (Max >= 5)
        {
            Config.PlatePos = Math.Clamp(Config.PlatePos, 0, Max - 4 + (Max % 2));
            Config.PlatePos -= Config.PlatePos % 2;
        }
        else
        {
            Config.PlatePos = 0;
        }


        WidgetContainer.SetPos(Config.Position);
        WidgetContainer.SetScale(Config.Scale);
        Plate.SetMultiply(Config.PlateTint)
             .SetScaleX(Config.MirrorPlate?-1:1)
             .SetX(Config.PlatePos * 23f)
             .SetAlpha(!Config.HidePlate);

        for (var i = 0; i < Max; i++)
        {
            Frames[i].SetMultiply(Config.PlateTint)
                     .SetAlpha(Config.HidePlate || Max < 3 || i < Config.PlatePos || i > Config.PlatePos + 2)
                     .SetScaleX(Config.MirrorPlate?-1:1);

            Gems[i].SetAddRGB(Config.GemTint + GemTintOffset);
            Glows[i].SetAddRGB(Config.GemTint + GemTintOffset);
            Glows2[i].SetAddRGB(Config.GemTint + GemTintOffset);
            Pulsars[i].SetAddRGB(Config.GemTint + GemTintOffset);
        }
    }

    public override void DrawUI(ref WidgetConfig widgetConfig)
    {
        base.DrawUI(ref widgetConfig);
        switch (UiTab)
        {
            case Layout:
                if (Max >= 3)
                {
                    ToggleControls("Hide Plate", ref Config.HidePlate);

                    if (!Config.HidePlate)
                    {
                        ToggleControls("Mirror Plate", ref Config.MirrorPlate);
                        if (Max >= 5) IntControls("Plate Position", ref Config.PlatePos, 0, Max - 3, 2);
                    }
                }


                break;
            case Colors:
                ColorPickerRGB("Gem Color", ref Config.GemTint);
                ColorPickerRGB("Plate Tint", ref Config.PlateTint);
                break;
            case Behavior:
                if (ToggleControls("Hide Empty", ref Config.HideEmpty))
                {
                    if (Config.HideEmpty && Tracker.CurrentData.Count == 0) PlateVanish();
                    if (!Config.HideEmpty && WidgetContainer.Alpha < 255) PlateAppear();
                }
                RadioControls("Pulse", ref Config.Pulse, new() { Never, AtMax, Always }, new() { "Never", "At Maximum", "Always" });
                break;
            default:
                break;
        }

        if (UpdateFlag.HasFlag(Save)) ApplyConfigs();
        widgetConfig.SamuraiDiamondCfg = Config;
    }

    #endregion
}

public partial class WidgetConfig
{
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] public SamuraiDiamondConfig? SamuraiDiamondCfg { get; set; }
}
