using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using CustomNodes;
using GaugeOMatic.CustomNodes.Animation;
using GaugeOMatic.Trackers;
using Newtonsoft.Json;
using static CustomNodes.CustomNodeManager;
using static GaugeOMatic.Trackers.Tracker;
using static GaugeOMatic.Trackers.Tracker.UpdateFlags;
using static GaugeOMatic.Utility.Color;
using static GaugeOMatic.Widgets.BalancePlate;
using static GaugeOMatic.Widgets.WidgetTags;
using static GaugeOMatic.Widgets.WidgetUI;
using static GaugeOMatic.Widgets.WidgetUI.WidgetUiTab;

#pragma warning disable CS8618

namespace GaugeOMatic.Widgets;

public sealed unsafe class BalancePlate : StateWidget
{
    public static WidgetInfo GetWidgetInfo { get; } = new()
    {
        DisplayName = "Balance Gauge Backplate",
        Author = "ItsBexy",
        Description =
            "A recreation of the Balance Gauge's backplate. Designed to combine with a set of mana bar widgets.",
        WidgetTags = MultiComponent | Replica | State,
        MultiCompData = new("BL", "Balance Gauge Replica", 1),
        UiTabOptions = Layout | Colors
    };

    public override CustomPartsList[] PartsLists { get; } =
    {
        new("ui/uld/JobHudRDM0.tex",
            new(0, 0, 116, 208),
            new(186, 3, 26, 124),
            new(212, 3, 26, 124),
            new(116, 0, 34, 144),
            new(0, 208, 40, 56),
            new(40, 208, 40, 56),
            new(116, 144, 40, 60),
            new(184, 132, 84, 188),
            new(125, 212, 24, 22),
            new(123, 234, 28, 20),
            new(148, 222, 14, 15),
            new(242, 3, 26, 124),
            new(116, 332, 72, 32),
            new(0, 264, 40, 48),
            new(81, 239, 30, 40),
            new(81, 279, 30, 40),
            new(0, 319, 117, 61),
            new(114, 258, 60, 60),
            new(118, 321, 89, 59),
            new(207, 321, 39, 59),
            new(150, 0, 34, 144))
    };

    public override WidgetInfo WidgetInfo => GetWidgetInfo;

    public BalancePlate(Tracker tracker) : base(tracker) => SharedEvents.Add("SpendShake", _ => BalanceBar.SpendShake(WidgetContainer, Config!.GetBGColor(Tracker.CurrentData.State) * 0.25f, Config.Position.X, Config.Position.Y, ref Animator));

    #region Nodes

    public CustomNode BackdropContainer;
    public CustomNode Backdrop;
    public CustomNode Plate;
    public CustomNode Crystal;
    public CustomNode CrystalGlow;
    public CustomNode Star;

    public override CustomNode BuildContainer()
    {
        Backdrop = ImageNodeFromPart(0, 7).SetPos(16, 23);

        BackdropContainer = new CustomNode(CreateResNode(), Backdrop).SetSize(100, 211);

        Plate = ImageNodeFromPart(0, 0).SetImageWrap(2);

        Crystal = ImageNodeFromPart(0, 4).SetPos(39, 5)
                                         .SetAddRGB(-30, -30, -30)
                                         .SetImageWrap(1);

        CrystalGlow = ImageNodeFromPart(0, 5).SetPos(39, 5)
                                             .SetOrigin(20, 28)
                                             .SetAlpha(0);

        Star = ImageNodeFromPart(0, 6).SetPos(38, -2)
                                      .SetImageFlag(32)
                                      .SetImageWrap(2)
                                      .SetOrigin(20, 33)
                                      .SetAlpha(0)
                                      .SetScale(5)
                                      .SetAddRGB(-100);

        return new(CreateResNode(), BackdropContainer, Plate, Crystal, CrystalGlow, Star);
    }

    #endregion

    #region Animations

    private void StarFlash(int state)
    {
        var starColor = Config.GetFXColor(state);
        Animator += new Tween(Star,
                              new(0) { Scale = 1, Alpha = (byte)(starColor.A * 0.75f), AddRGB = starColor },
                              new(120) { Scale = 3, Alpha = starColor.A, AddRGB = starColor },
                              new(250) { Scale = 5, Alpha = 0, AddRGB = new(-100) });
    }

    private void UpdateBorderGlow(int state)
    {
        var fx = Config.GetFXColor(state);
        var offset = new AddRGB(-128, 128, 128);
        Animator += new Tween(CrystalGlow,
                              new(0) { Alpha = (byte)(fx.A * 0.5f), Scale = 2f, AddRGB = offset + fx },
                              new(10) { Alpha = fx.A, Scale = 1f, AddRGB = offset + fx },
                              new(20) { Alpha = (byte)(fx.A * 0.8f), Scale = 1f, AddRGB = offset + fx });
    }

    private void ChangeCrystalColor(int state, int prevState)
    {
        var crystal = Config.GetCrystalColor(state);
        var prevCrystal = Config.GetCrystalColor(prevState);
        Animator += new Tween(Crystal, new(0) { AddRGB = prevCrystal },
                              new(20) { AddRGB = crystal + new AddRGB(30) },
                              new(39) { AddRGB = crystal + new AddRGB(30) },
                              new(40) { AddRGB = crystal });
    }

    private void ChangeBGColor(int state, int prevState)
    {
        var bg = Config.GetBGColor(state);
        var prevBg = Config.GetBGColor(prevState);
        Animator += new Tween(Backdrop,
                              new(0) { Alpha = prevBg.A, AddRGB = prevBg },
                              new(200) { Alpha = bg.A, AddRGB = bg });
    }

    #endregion

    #region UpdateFuncs

    public override string SharedEventGroup => "BalanceGauge";

    public override void OnFirstRun(int current) => ApplyConfigs();
    public override void Activate(int current) => StateChange(Tracker.CurrentData.State, Tracker.PreviousData.State);
    public override void Deactivate(int previous) => StateChange(Tracker.CurrentData.State, Tracker.PreviousData.State);

    public override void StateChange(int current, int previous)
    {
        ChangeBGColor(current, previous);
        ChangeCrystalColor(current, previous);
        UpdateBorderGlow(current);
        StarFlash(current);
    }

    #endregion

    #region Configs

    public class BalancePlateConfig
    {
        [DefaultValue(1)] public float Scale = 1;
        public List<AddRGB> BGColors = new();
        public List<AddRGB> CrystalColors = new();
        public List<AddRGB> FXColors = new();
        public Vector2 Position;

        public BalancePlateConfig(WidgetConfig widgetConfig)
        {
            var config = widgetConfig.BalancePlateCfg;

            if (config == null) return;

            Position = config.Position;
            Scale = config.Scale;
            BGColors = config.BGColors;
            CrystalColors = config.CrystalColors;
            FXColors = config.FXColors;
        }

        public BalancePlateConfig() { }

        public AddRGB GetBGColor(int state) => BGColors.ElementAtOrDefault(state);
        public AddRGB GetCrystalColor(int state) => CrystalColors.ElementAtOrDefault(state);
        public AddRGB GetFXColor(int state) => FXColors.ElementAtOrDefault(state);

        public void FillColorLists(int maxState)
        {
            while (BGColors.Count <= maxState) BGColors.Add(BGColors.Count == 0 ? new(0) : new(160, 0, 0));
            while (CrystalColors.Count <= maxState) CrystalColors.Add(CrystalColors.Count == 0 ? new(0, 0, 0) : new(57, -12, -12));
            while (FXColors.Count <= maxState) FXColors.Add(FXColors.Count == 0 ? new AddRGB(0, 0, 0, 0) : "0xCF2222FF");
        }
    }

    public BalancePlateConfig Config;

    public override void InitConfigs() => Config = new(Tracker.WidgetConfig);

    public override void ResetConfigs()
    {
        Config = new();
        Config.FillColorLists(Tracker.CurrentData.MaxState);
    }

    public override void ApplyConfigs()
    {
        WidgetContainer.SetPos(Config.Position).SetScale(Config.Scale);

        var state = Tracker.CurrentData.State;

        Backdrop.SetAddRGB(Config.GetBGColor(state), true);
        Crystal.SetAddRGB(Config.GetCrystalColor(state));
        CrystalGlow.SetAddRGB(Config.GetFXColor(state));
    }

    public override void DrawUI(ref WidgetConfig widgetConfig, ref UpdateFlags update)
    {
        Config.FillColorLists(Tracker.CurrentData.MaxState);
        switch (UiTab)
        {
            case Layout:
                PositionControls("Position", ref Config.Position, ref update);
                ScaleControls("Scale", ref Config.Scale, ref update);
                break;
            case Colors:
                for (var i = 0; i <= Tracker.CurrentData.MaxState; i++)
                {
                    Heading(Tracker.StateNames[i]);

                    var bgColor = Config.BGColors[i];
                    var crystalColor = Config.CrystalColors[i];
                    var fxColor = Config.FXColors[i];
                    if (ColorPickerRGBA($"Backdrop##{i}", ref bgColor, ref update)) Config.BGColors[i] = bgColor;
                    if (ColorPickerRGB($"Crystal##{i}", ref crystalColor, ref update)) Config.CrystalColors[i] = crystalColor;
                    if (ColorPickerRGBA($"Effects##{i}", ref fxColor, ref update)) Config.FXColors[i] = fxColor;
                }
                break;
            default:
                break;
        }

        if (update.HasFlag(Save)) ApplyConfigs();
        widgetConfig.BalancePlateCfg = Config;
    }

    #endregion
}

public partial class WidgetConfig
{
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public BalancePlateConfig? BalancePlateCfg { get; set; }
}
