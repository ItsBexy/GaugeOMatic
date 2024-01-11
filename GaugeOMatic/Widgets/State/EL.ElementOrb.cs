using GaugeOMatic.Trackers;
using GaugeOMatic.Windows;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using static CustomNodes.CustomNodeManager;
using static CustomNodes.CustomNodeManager.Tween;
using static GaugeOMatic.Utility.Color;
using static GaugeOMatic.Widgets.ElementOrb;
using static GaugeOMatic.Widgets.ElementOrb.ElementOrbConfig.OrbBase;
using static GaugeOMatic.Widgets.WidgetTags;
using static GaugeOMatic.Widgets.WidgetUI;
using static GaugeOMatic.Windows.UpdateFlags;

namespace GaugeOMatic.Widgets;

public sealed unsafe class ElementOrb : StateWidget
{
    public override WidgetInfo WidgetInfo => GetWidgetInfo;

    public static WidgetInfo GetWidgetInfo => new()
    {
        DisplayName = "Elemental Orb",
        Author = "ItsBexy",
        Description = "A widget recreating the orb on BLM's elemental gauge.",
        WidgetTags = State | MultiComponent | Replica,
        KeyText = "EL2"
    };

    public override CustomPartsList[] PartsLists { get; } = {
        new ("ui/uld/JobHudBLM0.tex",
             new(0,0,162,144),   // 0  crescent
             new(290,0,52,52),   // 1  blue orb
             new(290,52,52,52),  // 2  red orb
             new(342,0,20,48),   // 3  blue crystal
             new(342,48,20,48),  // 4  red crystal
             new(182,236,24,68), // 5  icicle
             new(342,96,20,48),  // 6  blue glow
             new(342,144,20,48), // 7  red glow
             new(162,0,128,124), // 8  lattice
             new(0,146,90,90),   // 9  curved plate
             new(95,153,80,78),  // 10  bar fill
             new(180,146,90,90), // 11 bar backdrop
             new(362,0,30,128),  // 12 clock hand
             new(324,192,30,46), // 13 diamond
             new(354,192,30,46), // 14 diamond glow
             new(0,236,90,90),   // 15 shine
             new(90,236,68,68),  // 16 eclipse
             new(290,104,46,46), // 17 halo
             new(362,128,28,28), // 18 glowball
             new(270,150,54,83), // 19 diamond frame
             new(158,236,24,68), // 20 icicle glow
             new(206,236,64,36), // 21 text bg
             new(206,272,32,32), // 22 simple icon thingy
             new(270,233,32,42), // 23 diamond cover
             new(302,238,52,52), // 24 grey orb
             new(307,290,85,36), // 25 double pointy
             new(0,324,86,40),   // 26 null paradox gem
             new(86,324,86,40),  // 27 active paradox gem
             new(172,324,86,40), // 28 paradox glow
             new(90,306,29,18),  // 29 blue sparkles
             new(119,306,29,36)  // 30 red sparkles
        )};

    #region Nodes

    public CustomNode OrbContainer;
    public CustomNode InactiveOrb;
    public CustomNode Crescent;
    public CustomNode Halo;
    public CustomNode Orb;
    public CustomNode Shine;

    public override CustomNode BuildRoot()
    {


        // fire:   2, orb: AddRGB(20,0,-77), halo: AddRGB(255,-180,-200)
        var haloAdd = new AddRGB(-200, -180, 255);

        Halo = ImageNodeFromPart(0, 17).SetPos(4, 4).SetScale(2).SetOrigin(23, 23).SetAlpha(215).SetAddRGB(haloAdd);
        Orb = ImageNodeFromPart(0, 1).SetSize(46, 46);
        Shine = ImageNodeFromPart(0, 15).SetPos(-1.3835533f, -32.767105f)
                                        .SetScale(0.8f)
                                        .SetOrigin(45, 45)
                                        .SetDrawFlags(0xE)
                                        .SetImageFlag(32)
                                        .SetAlpha(0x69);

        OrbContainer = new CustomNode(CreateResNode(),
                                      Halo,
                                      Orb,
                                      Shine).SetPos(68, 48).SetSize(52, 52).SetAlpha(0);


        Crescent = new CustomNode(CreateResNode(), 
                                ImageNodeFromPart(0,0).SetImageWrap(2), 
                                ImageNodeFromPart(0,25).SetPos(-2,77).SetImageWrap(2)
                                ).SetSize(162,144).SetAddRGB(-20).SetMultiply(50).SetOrigin(94,76);

        InactiveOrb = ImageNodeFromPart(0, 24).SetPos(68, 48);

        return new CustomNode(CreateResNode(),OrbContainer,Crescent,InactiveOrb).SetSize(162,160);
    }

    #endregion

    #region Animations


    private void SetupPulse(AddRGB orbAdd)
    {
        ClearLabelTweens(ref Tweens,"Pulse");
        Tweens.Add(new(Halo,
                       new(0) { Alpha = 128 },
                       new(630) { Alpha = 128 },
                       new(965) { Alpha = 255 },
                       new(1300) { Alpha = 128 })
                       { Repeat = true, Label = "Pulse" });

        Tweens.Add(new(Orb,
                       new(0) { AddRGB = 0 },
                       new(630) { AddRGB = 0 },
                       new(965) { AddRGB = orbAdd },
                       new(1300) { AddRGB = 0 })
                       { Repeat = true, Label = "Pulse" });

        Tweens.Add(new(Shine,
                       new(0) { Alpha = 0 },
                       new(629) { Alpha = 0 },
                       new(630) { X = -8, Y = -40, Scale = 0.4f, Alpha = 100 },
                       new(965) { X = -2, Y = -34, Scale = 0.8f, Alpha = 152 },
                       new(1300) { X = 0, Y = -30, Scale = 0.8f, Alpha = 0 })
                       { Repeat = true, Label = "Pulse" });
    }

    #endregion

    #region UpdateFuncs

    public override string? SharedEventGroup => null;

    public override void OnUpdate() { }

    public override void OnFirstRun(int current) { }

    public override void Activate(int current)
    {
        SetupPulse(Config.GetOrbPulse(current));
        Halo.SetAddRGB(Config.GetHaloColor(current));
        var partId = (ushort)Config.GetBaseColor(current);
        Orb.SetPartId((ushort)(partId == 0? 24:partId));
        OrbContainer.SetAddRGB(Config.GetOrbModifier(current));

        Tweens.Add(new(OrbContainer, new(0) { Alpha = 0 }, new(630) { Alpha = 255 }));
        Tweens.Add(new(InactiveOrb, new(0) { Alpha = 255 }, new(160) { Alpha = 0 }));
        Tweens.Add(new(Crescent, new(0) { AddRGB = -20, MultRGB = 50 }, new(160) { AddRGB = 0, MultRGB = 100 }));
    }

    public override void Deactivate(int previous)
    {
        Tweens.Add(new(OrbContainer, new(0) { Alpha = 255 }, new(160) { Alpha = 0 }));
        Tweens.Add(new(InactiveOrb, new(0) { Alpha = 0 }, new(160) { Alpha = 255 }));
        Tweens.Add(new(Crescent, new(0) { AddRGB = 0, MultRGB = 100 }, new(160) { AddRGB = -20, MultRGB = 50 }));
    }

    public override void StateChange(int current, int previous)
    {
        OrbContainer.SetAlpha(0);
        Activate(current);
    }

    #endregion

    #region Configs

    public class ElementOrbConfig
    {
        public enum OrbBase { Grey=24, Blue=1, Red=2 }

        public Vector2 Position = new(0);
        public float Scale = 1;
        public float CrescentAngle;

        public List<OrbBase> BaseColors = new();
        public List<AddRGB> OrbPulses = new();
        public List<AddRGB> HaloColors = new();
        public List<AddRGB> OrbModifiers = new();

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
            while (OrbPulses.Count <= max) OrbPulses.Add(OrbPulses.Count switch
            {
                0 => new(0), 
                1 => new(20, 0, -77), 
                _ => new(-56, 0, 13)
            });
            
            while (HaloColors.Count <= max) HaloColors.Add(HaloColors.Count switch
            {
                0 => new(0),
                1 => new(255, -180, -200),
                _ => new(-200, -180, 255)
            });
            while (OrbModifiers.Count <=max) OrbModifiers.Add(new(0));
        }

    }

    public ElementOrbConfig Config = null!;

    public override void InitConfigs() => Config = new(Tracker.WidgetConfig);

    public override void ResetConfigs() => Config = new();

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
        FloatControls("Crescent Angle", ref Config.CrescentAngle, -180, 180, 1, ref update);

        Config.FillColorLists(Tracker.CurrentData.MaxState);

        for (var i = 1; i <= Tracker.CurrentData.MaxState; i++)
        {
            Heading(Tracker.StateNames[i]);

            var baseColor = Config.BaseColors[i];
            var orbMod = Config.OrbModifiers[i];
            var orbPulse = Config.OrbPulses[i];
            var haloColor = Config.HaloColors[i];
            if (RadioControls($"Base Color##baseColor{i}", ref baseColor, new(){ Grey, Red, Blue }, new (){ "Grey", "Red", "Blue" }, ref update)) Config.BaseColors[i] = baseColor;
            if (ColorPickerRGB($"Color Modifier##orbMod{i}", ref orbMod, ref update)) Config.OrbModifiers[i] = orbMod;
            if (ColorPickerRGB($"Orb Pulse##orbPulse{i}", ref orbPulse, ref update)) Config.OrbPulses[i] = orbPulse;
            if (ColorPickerRGB($"Rim Pulse##rimPulse{i}", ref haloColor, ref update)) Config.HaloColors[i] = haloColor;
        }

        if (update.HasFlag(Save)) ApplyConfigs();
        widgetConfig.ElementOrbCfg = Config;
    }

    #endregion

    public ElementOrb(Tracker tracker) : base(tracker) { }
}

public partial class WidgetConfig
{
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] public ElementOrbConfig? ElementOrbCfg { get; set; }
}
