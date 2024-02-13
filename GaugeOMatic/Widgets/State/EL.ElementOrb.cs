using CustomNodes;
using GaugeOMatic.CustomNodes.Animation;
using GaugeOMatic.Trackers;
using GaugeOMatic.Windows;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using static CustomNodes.CustomNodeManager;
using static GaugeOMatic.CustomNodes.Animation.KeyFrame;
using static GaugeOMatic.Utility.Color;
using static GaugeOMatic.Widgets.Common.CommonParts;
using static GaugeOMatic.Widgets.ElementOrb;
using static GaugeOMatic.Widgets.ElementOrb.ElementOrbConfig.OrbBase;
using static GaugeOMatic.Widgets.WidgetTags;
using static GaugeOMatic.Widgets.WidgetUI;
using static GaugeOMatic.Windows.UpdateFlags;

#pragma warning disable CS8618

namespace GaugeOMatic.Widgets;

public sealed unsafe class ElementOrb : StateWidget
{
    public static WidgetInfo GetWidgetInfo => new()
    {
        DisplayName = "Elemental Orb",
        Author = "ItsBexy",
        Description = "A widget recreating the orb on BLM's elemental gauge.",
        WidgetTags = State | MultiComponent | Replica,
        MultiCompData = new("EL", "Elemental Gauge Replica", 2)
    };

    public override CustomPartsList[] PartsLists { get; } = { BLM0 };

    public override WidgetInfo WidgetInfo => GetWidgetInfo;
    public ElementOrb(Tracker tracker) : base(tracker) { }

    #region Nodes

    public CustomNode OrbContainer;
    public CustomNode InactiveOrb;
    public CustomNode Crescent;
    public CustomNode Halo;
    public CustomNode Orb;
    public CustomNode Shine;

    public override CustomNode BuildRoot()
    {
        Halo = ImageNodeFromPart(0, 17).SetPos(4, 4).
                                        SetScale(2)
                                        .SetOrigin(23, 23)
                                        .SetAlpha(215)
                                        .SetAddRGB(-200, -180, 255);

        Orb = ImageNodeFromPart(0, 1).SetSize(46, 46);

        Shine = ImageNodeFromPart(0, 15).SetPos(-1.3835533f, -32.767105f)
                                        .SetScale(0.8f)
                                        .SetOrigin(45, 45)
                                        .SetDrawFlags(0xE)
                                        .SetImageFlag(32)
                                        .SetAlpha(0x69);

        OrbContainer = new CustomNode(CreateResNode(), Halo, Orb, Shine).SetPos(68, 48)
                                                                        .SetSize(52, 52)
                                                                        .SetAlpha(0);

        var crescent1 = ImageNodeFromPart(0, 0).SetImageWrap(2);
        var crescent2 = ImageNodeFromPart(0, 25).SetPos(-2, 77)
                                                .SetImageWrap(2);

        Crescent = new CustomNode(CreateResNode(), crescent1, crescent2).SetSize(162, 144)
                                                                        .SetAddRGB(-20)
                                                                        .SetMultiply(50)
                                                                        .SetOrigin(94, 76);

        InactiveOrb = ImageNodeFromPart(0, 24).SetPos(68, 48);

        return new CustomNode(CreateResNode(), OrbContainer, Crescent, InactiveOrb).SetSize(162, 160);
    }

    #endregion

    #region Animations

    private void SetupPulse(AddRGB orbAdd)
    {
        Animator -= "Pulse";
        Animator += new Tween[]
        {
            new(Halo,
                new(0) { Alpha = 128 },
                new(630) { Alpha = 128 },
                Visible[965],
                new(1300) { Alpha = 128 })
                { Repeat = true, Label = "Pulse" },
            new(Orb,
                new(0) { AddRGB = 0 },
                new(630) { AddRGB = 0 },
                new(965) { AddRGB = orbAdd },
                new(1300) { AddRGB = 0 })
                { Repeat = true, Label = "Pulse" },
            new(Shine,
                Hidden[0],
                Hidden[629],
                new(630) { X = -8, Y = -40, Scale = 0.4f, Alpha = 100 },
                new(965) { X = -2, Y = -34, Scale = 0.8f, Alpha = 152 },
                new(1300) { X = 0, Y = -30, Scale = 0.8f, Alpha = 0 })
                { Repeat = true, Label = "Pulse" }
        };
    }

    #endregion

    #region UpdateFuncs

    public override void OnFirstRun(int current) { }

    public override void Activate(int current)
    {
        SetupPulse(Config.GetOrbPulse(current));
        Halo.SetAddRGB(Config.GetHaloColor(current));
        var partId = (ushort)Config.GetBaseColor(current);
        Orb.SetPartId(partId == 0 ? 24 : partId);
        OrbContainer.SetAddRGB(Config.GetOrbModifier(current));

        Animator += new Tween[]
        {
            new(OrbContainer, Hidden[0], Visible[630]),
            new(InactiveOrb, Visible[0], Hidden[160]),
            new(Crescent, new(0) { AddRGB = -20, MultRGB = 50 }, new(160) { AddRGB = 0, MultRGB = 100 })
        };
    }

    public override void Deactivate(int previous) =>
        Animator += new Tween[]
        {
            new(OrbContainer, Visible[0], Hidden[160]),
            new(InactiveOrb, Hidden[0], Visible[160]),
            new(Crescent, new(0) { AddRGB = 0, MultRGB = 100 }, new(160) { AddRGB = -20, MultRGB = 50 })
        };

    public override void StateChange(int current, int previous)
    {
        OrbContainer.SetAlpha(0);
        Activate(current);
    }

    #endregion

    #region Configs

    public class ElementOrbConfig
    {
        public enum OrbBase { Grey = 24, Blue = 1, Red = 2 }

        public float CrescentAngle;
        public float Scale = 1;
        public List<AddRGB> HaloColors = new();
        public List<AddRGB> OrbModifiers = new();
        public List<AddRGB> OrbPulses = new();
        public List<OrbBase> BaseColors = new();
        public Vector2 Position;

        public ElementOrbConfig(WidgetConfig widgetConfig)
        {
            var config = widgetConfig.ElementOrbCfg;

            if (config == null) return;

            Position = config.Position;
            Scale = config.Scale;
            CrescentAngle = config.CrescentAngle;

            BaseColors = config.BaseColors;
            OrbPulses = config.OrbPulses;
            HaloColors = config.HaloColors;
            OrbModifiers = config.OrbModifiers;
        }

        public ElementOrbConfig() { }

        public OrbBase GetBaseColor(int state) => BaseColors.ElementAtOrDefault(state);
        public AddRGB GetOrbPulse(int state) => OrbPulses.ElementAtOrDefault(state);
        public AddRGB GetHaloColor(int state) => HaloColors.ElementAtOrDefault(state);
        public AddRGB GetOrbModifier(int state) => OrbModifiers.ElementAtOrDefault(state);

        public void FillColorLists(int max)
        {
            while (BaseColors.Count <= max) BaseColors.Add(BaseColors.Count switch { 0 => Grey, 1 => Red, _ => Blue });

            while (OrbPulses.Count <= max)
                OrbPulses.Add(OrbPulses.Count switch
                {
                    0 => new(0),
                    1 => new(20, 0, -77),
                    _ => new(-56, 0, 13)
                });

            while (HaloColors.Count <= max)
                HaloColors.Add(HaloColors.Count switch
                {
                    0 => new(0),
                    1 => new(255, -180, -200),
                    _ => new(-200, -180, 255)
                });

            while (OrbModifiers.Count <= max) OrbModifiers.Add(new(0));
        }
    }

    public ElementOrbConfig Config;

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
        WidgetRoot.SetPos(Config.Position);
        WidgetRoot.SetScale(Config.Scale);
        Crescent.SetRotation(Config.CrescentAngle, true);

        var state = Tracker.CurrentData.State;

        OrbContainer.SetAddRGB(Config.GetOrbModifier(state));
        Orb.SetPartId((ushort)Config.GetBaseColor(state));
        Halo.SetAddRGB(Config.GetHaloColor(state));
        SetupPulse(Config.GetOrbPulse(state));
    }

    public override void DrawUI(ref WidgetConfig widgetConfig, ref UpdateFlags update)
    {
        Heading("Layout");
        PositionControls("Position", ref Config.Position, ref update);
        ScaleControls("Scale", ref Config.Scale, ref update);
        AngleControls("Crescent Angle", ref Config.CrescentAngle, ref update);

        Config.FillColorLists(Tracker.CurrentData.MaxState);

        for (var i = 1; i <= Tracker.CurrentData.MaxState; i++)
        {
            Heading(Tracker.StateNames[i]);

            var baseColor = Config.BaseColors[i];
            var orbMod = Config.OrbModifiers[i];
            var orbPulse = Config.OrbPulses[i];
            var haloColor = Config.HaloColors[i];
            if (RadioControls($"Base Color##baseColor{i}", ref baseColor, new() {Grey, Red, Blue}, new() {"Grey", "Red", "Blue"}, ref update)) Config.BaseColors[i] = baseColor;
            if (ColorPickerRGB($"Color Modifier##orbMod{i}", ref orbMod, ref update)) Config.OrbModifiers[i] = orbMod;
            if (ColorPickerRGB($"Orb Pulse##orbPulse{i}", ref orbPulse, ref update)) Config.OrbPulses[i] = orbPulse;
            if (ColorPickerRGB($"Rim Pulse##rimPulse{i}", ref haloColor, ref update)) Config.HaloColors[i] = haloColor;
        }

        if (update.HasFlag(Save)) ApplyConfigs();
        widgetConfig.ElementOrbCfg = Config;
    }

    #endregion
}

public partial class WidgetConfig
{
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public ElementOrbConfig? ElementOrbCfg { get; set; }
}
