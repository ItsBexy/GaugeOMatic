using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using CustomNodes;
using GaugeOMatic.CustomNodes.Animation;
using GaugeOMatic.Trackers;
using Newtonsoft.Json;
using static CustomNodes.CustomNode.CustomNodeFlags;
using static CustomNodes.CustomNodeManager;
using static GaugeOMatic.CustomNodes.Animation.KeyFrame;
using static GaugeOMatic.CustomNodes.Animation.Tween.EaseType;
using static GaugeOMatic.Trackers.Tracker;
using static GaugeOMatic.Trackers.Tracker.UpdateFlags;
using static GaugeOMatic.Utility.Color;
using static GaugeOMatic.Widgets.EukrasiaReplica;
using static GaugeOMatic.Widgets.WidgetTags;
using static GaugeOMatic.Widgets.WidgetUI;

#pragma warning disable CS8618

namespace GaugeOMatic.Widgets;

public sealed unsafe class EukrasiaReplica : StateWidget
{
    public EukrasiaReplica(Tracker tracker) : base(tracker) { }

    public override WidgetInfo WidgetInfo => GetWidgetInfo;

    public static WidgetInfo GetWidgetInfo => new()
    {
        DisplayName = "Eukrasia Sigil",
        Author = "ItsBexy",
        Description = "A widget recreating Sage's Eukrasia indicator.",
        WidgetTags = State
    };

    public override CustomPartsList[] PartsLists { get; } =
    {
        new("ui/uld/JobHudGFF0.tex",
            new(0, 0, 36, 56),
            new(36, 0, 36, 56),
            new(0, 176, 40, 40),
            new(0, 56, 24, 24),
            new(40, 176, 40, 40),
            new(0, 80, 40, 96),
            new(40, 80, 40, 96))
    };

    #region Nodes

    public CustomNode RightHalfInactive;
    public CustomNode LeftHalfInactive;
    public CustomNode FullActive;
    public CustomNode NoulithContainer;
    public CustomNode[] Nouliths = new CustomNode[4];
    public CustomNode[] ElectricNouliths = new CustomNode[4];
    public CustomNode Halo;

    public override CustomNode BuildContainer()
    {
        RightHalfInactive = ImageNodeFromPart(0, 2).SetPos(40, 0).SetSize(40, 80).SetImageWrap(3);
        LeftHalfInactive = ImageNodeFromPart(0, 2).SetSize(40, 80).SetImageWrap(3).SetImageFlag(1);
        FullActive = ImageNodeFromPart(0, 4).SetSize(80, 80).SetImageWrap(3).SetAlpha(0);

        Nouliths[0] = ImageNodeFromPart(0, 1).SetPos(32, -30)
                                             .SetRotation(225)
                                             .SetSize(36, 52)
                                             .SetOrigin(18, 60)
                                             .SetImageWrap(2)
                                             .RemoveFlags(SetVisByAlpha);

        Nouliths[1] = ImageNodeFromPart(0, 1).SetPos(12, -30)
                                             .SetRotation(135)
                                             .SetSize(36, 52)
                                             .SetOrigin(18, 60)
                                             .SetImageWrap(2)
                                             .RemoveFlags(SetVisByAlpha);

        Nouliths[2] = ImageNodeFromPart(0, 1).SetPos(12, -10)
                                             .SetRotation(45)
                                             .SetSize(36, 52)
                                             .SetOrigin(18, 60)
                                             .SetImageWrap(2)
                                             .RemoveFlags(SetVisByAlpha);

        Nouliths[3] = ImageNodeFromPart(0, 1).SetPos(32, -10)
                                             .SetRotation(315)
                                             .SetSize(36, 52)
                                             .SetOrigin(18, 60)
                                             .SetImageWrap(2)
                                             .RemoveFlags(SetVisByAlpha);

        ElectricNouliths[0] = BuildElectricNoulith(0).RemoveFlags(SetVisByAlpha).SetAlpha(0);
        ElectricNouliths[1] = BuildElectricNoulith(1).RemoveFlags(SetVisByAlpha).SetAlpha(0);
        ElectricNouliths[2] = BuildElectricNoulith(2).RemoveFlags(SetVisByAlpha).SetAlpha(0);
        ElectricNouliths[3] = BuildElectricNoulith(3).RemoveFlags(SetVisByAlpha).SetAlpha(0);

        NoulithContainer = new CustomNode(CreateResNode(), Nouliths[0], Nouliths[1], Nouliths[2], Nouliths[3],
                                          ElectricNouliths[0], ElectricNouliths[1], ElectricNouliths[2],
                                          ElectricNouliths[3]).SetOrigin(40, 40);

        Halo = ImageNodeFromPart(0, 3).SetPos(28, 28).SetScale(4).SetSize(24, 24).SetOrigin(12, 12).SetImageWrap(2)
                                      .SetImageFlag(32).SetAlpha(0);

        return new CustomNode(CreateResNode(), RightHalfInactive, LeftHalfInactive, FullActive, NoulithContainer, Halo)
               .SetOrigin(40, 40).SetSize(80, 80);
    }

    private CustomNode BuildElectricNoulith(int i)
    {
        var elecNoulith = new CustomNode(CreateResNode(),
                                         ImageNodeFromPart(0, 0).SetSize(36, 56).SetOrigin(24, 90).SetImageWrap(2)
                                                                .SetAddRGB(0, 0, 100),
                                         ImageNodeFromPart(0, 6).SetSize(40, 96).SetPos(10, -4).SetScale(0.6f)
                                                                .SetImageFlag(32).SetImageWrap(2).SetAddRGB(0, 0, 200)
                                                                .SetMultiply(50),
                                         ImageNodeFromPart(0, 6).SetSize(40, 96).SetScale(0.55f).SetImageFlag(35)
                                                                .SetImageWrap(2).SetAddRGB(-191, -200, 200)
                                                                .SetMultiply(54, 54, 54),
                                         ImageNodeFromPart(0, 5).SetSize(40, 96).SetPos(4, 2).SetScale(0.6f)
                                                                .SetImageWrap(2).SetImageFlag(32)
                                                                .SetAddRGB(-100, -200, 200).SetMultiply(50))
                          .SetPos(22, -20).SetSize(36, 56).SetOrigin(18, 60).SetRotation((405 - (90 * i)) % 360);

        SetUpLightning(elecNoulith);

        return elecNoulith;
    }

    #endregion

    #region Animations

    private void SetupSigilPulse(int state)
    {
        if (state == 0) state = 1;

        var color = Config.GetColor(state) + ColorOffset;
        var fxColor = Config.GetFXColor(state);
        Animator += new Tween(FullActive,
                              new(0) { AddRGB = color },
                              new(560) { AddRGB = color + new AddRGB(30, 30, 100) + fxColor },
                              new(1000) { AddRGB = color })
            { Ease = SinInOut, Repeat = true, Label = "SigilPulse" };
    }

    private void SetUpLightning(CustomNode elecNoulith)
    {
        Animator += new Tween[]
        {
            new(elecNoulith[1],
                new(0) { Alpha = 255, AddRGB = new(-200, -200, 100), MultRGB = new(0x63) },
                new(200) { Alpha = 0, AddRGB = new(-100, -200, 200), MultRGB = new(0x32) },
                new(500) { Alpha = 0, AddRGB = new(-100, -200, 200), MultRGB = new(0x32) })
                { Repeat = true },
            new(elecNoulith[2],
                new(0) { Alpha = 49, AddRGB = new(-181, -200, 200), MultRGB = new(0x3b) },
                new(100) { Alpha = 49, AddRGB = new(-181, -200, 200), MultRGB = new(0x3b) },
                new(300) { Alpha = 0, AddRGB = new(-200, -200, 200), MultRGB = new(0x32) },
                new(496) { Alpha = 0, AddRGB = new(-200, -200, 200), MultRGB = new(0x32) })
                { Repeat = true },
            new(elecNoulith[3],
                new(0) { Alpha = 255, AddRGB = new(-200, -200, 100), MultRGB = new(0x63) },
                new(200) { Alpha = 0, AddRGB = new(-100, -200, 200), MultRGB = new(0x32) },
                new(505) { Alpha = 0, AddRGB = new(-100, -200, 200), MultRGB = new(0x32) })
                { Repeat = true }
        };
    }

    private void NoulithSpin()
    {
        Animator += new Tween(NoulithContainer,
                              new(0) { Rotation = 0 },
                              new(300) { Rotation = 0 },
                              new(560) { Rotation = 3.141593f });

        for (var i = 0; i < 4; i++) NoulithInOut(i);
    }

    private void NoulithInOut(int i)
    {
        var initX = i % 3 == 0 ? 32 : 12;
        var initY = i < 2 ? -30 : -10;

        SetupNoulithPulse(i);
        Animator += new Tween[]
        {
            new(Nouliths[i],
                new(0) { X = initX, Y = initY, AddRGB = new(0), PartId = 1 },
                new(120) { X = 22, Y = -20, AddRGB = new(50, 80, 80), PartId = 0 },
                new(560) { X = 22, Y = -20, AddRGB = new(50, 80, 80), PartId = 0 },
                new(860) { X = 22, Y = -20, AddRGB = new(0), PartId = 0 },
                new(1000) { X = initX, Y = initY, AddRGB = new(0), PartId = 0 }),
            new(ElectricNouliths[i],
                new(0) { AddRGB = new(50, 80, 80), Alpha = 0 },
                new(299) { AddRGB = new(50, 80, 80), Alpha = 0 },
                new(300) { AddRGB = new(50, 80, 80), Alpha = 255 },
                new(560) { AddRGB = new(50, 80, 80), Alpha = 255 },
                new(860) { AddRGB = new(0), Alpha = 0 })
        };
    }

    private void SetupNoulithPulse(int i) =>
        Animator += new Tween(Nouliths[i],
                              new(0) { AddRGB = new(0) },
                              new(360) { AddRGB = new(20, 20, 60) },
                              new(1000) { AddRGB = new(0) })
                              { Repeat = true, Ease = SinInOut, Label = "NoulithPulse" };

    #endregion

    #region UpdateFuncs

    public override void OnFirstRun(int current)
    {
        if (current > 0)
        {
            RightHalfInactive.SetAlpha(0);
            LeftHalfInactive.SetAlpha(0);
            FullActive.SetAlpha(255);

            for (var i = 0; i < 4; i++) SetupNoulithPulse(i);
        }
        else
        {
            RightHalfInactive.SetAlpha(255);
            LeftHalfInactive.SetAlpha(255);
            FullActive.SetAlpha(0);

            Animator -= "NoulithPulse";
            Animator -= "SigilPulse";
        }
    }

    public override void Activate(int current)
    {
        SetupSigilPulse(current);

        var fx = Config.GetNoulithColor(current);
        NoulithSpin();

        Animator += new Tween[]
        {
            new(RightHalfInactive, Visible[0], Hidden[600]),
            new(LeftHalfInactive, Visible[0], Hidden[600]),
            new(FullActive, Hidden[0], Visible[560]),
            new(NoulithContainer, new(0) { AddRGB = new(0) }, new(120) { AddRGB = fx }),
            new(Halo,
                new(0) { Scale = 1.4f, Alpha = 255, AddRGB = fx + ColorOffset },
                new(360) { Scale = 3, Alpha = 255, AddRGB = fx + ColorOffset },
                new(660) { Scale = 4, Alpha = 0, AddRGB = fx + ColorOffset }
            )
        };
    }

    public override void Deactivate(int previous)
    {
        Animator -= "NoulithPulse";
        Animator -= "SigilPulse";

        var fx = Config.GetFXColor(previous);
        var color = Config.GetColor(previous);
        Animator += new Tween[]
        {
            new(RightHalfInactive, Hidden[0], Visible[160]),
            new(LeftHalfInactive, Hidden[0], Visible[160]),
            new(FullActive,
                new(0) { Alpha = 255, AddRGB = new AddRGB(30, 30, 100) + fx + ColorOffset },
                new(300) { Alpha = 0, AddRGB = color + ColorOffset }),
            new(NoulithContainer, new(0) { AddRGB = fx + ColorOffset }, new(300) { AddRGB = new(0) }),
            new(Halo,
                new(0) { Scale = 1.5f, Alpha = 255, AddRGB = fx + ColorOffset },
                new(100) { Scale = 3, Alpha = 255, AddRGB = fx + ColorOffset },
                new(200) { Scale = 4, Alpha = 0, AddRGB = fx + ColorOffset }
            )
        };

        Animator += Nouliths.Select(static n => new Tween(n, new(0) { AddRGB = new(20, 20, 60), PartId = 0 },
                                                          new(360) { AddRGB = new(0), PartId = 1 })).ToList();
    }

    public override void StateChange(int current, int previous)
    {
        var fx = Config.GetFXColor(current);
        var prevFx = Config.GetFXColor(previous);

        Animator -= "SigilPulse";
        SetupSigilPulse(current);

        Animator += new Tween[]
        {
            new(Halo,
                new(0) { Scale = 1.4f, Alpha = 255, AddRGB = prevFx + ColorOffset },
                new(360) { Scale = 3, Alpha = 255, AddRGB = fx + ColorOffset },
                new(660) { Scale = 4, Alpha = 0, AddRGB = fx + ColorOffset }
            ),
            new(NoulithContainer,
                new(0) { AddRGB = prevFx },
                new(360) { AddRGB = fx }
            )
        };

        NoulithSpin();
    }

    #endregion

    #region Configs

    public class EukrasiaReplicaConfig
    {
        public Vector2 Position;
        [DefaultValue(1)] public float Scale = 1;
        [DefaultValue(true)] public bool ShowNouliths = true;
        public List<AddRGB> Colors = new();
        public List<AddRGB> FXColors = new();
        public List<AddRGB> NoulithColors = new();

        public EukrasiaReplicaConfig(WidgetConfig widgetConfig)
        {
            var config = widgetConfig.EukrasiaReplicaCfg;

            if (config == null) return;

            Position = config.Position;
            Scale = config.Scale;
            ShowNouliths = config.ShowNouliths;
            Colors = config.Colors;
            FXColors = config.FXColors;
            NoulithColors = config.NoulithColors;
        }

        public EukrasiaReplicaConfig() { }

        public AddRGB GetFXColor(int state)
        {
            if (state == 0) state = 1;
            return FXColors.ElementAtOrDefault(state);
        }

        public AddRGB GetColor(int state)
        {
            if (state == 0) state = 1;
            return Colors.ElementAtOrDefault(state);
        }

        public AddRGB GetNoulithColor(int state)
        {
            if (state == 0) state = 1;
            return NoulithColors.ElementAtOrDefault(state);
        }


        public void FillColorLists(int max)
        {
            while (Colors.Count <= max) Colors.Add(new(-109, -47, 41));
            while (FXColors.Count <= max) FXColors.Add(new(0));
            while (NoulithColors.Count <= max) NoulithColors.Add(new(0));
        }
    }

    public EukrasiaReplicaConfig Config;

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

    public AddRGB ColorOffset = new(109, 47, -41);

    public override void ApplyConfigs()
    {
        WidgetContainer.SetPos(Config.Position);
        WidgetContainer.SetScale(Config.Scale);

        var state = Tracker.CurrentData.State;

        var noulithColor = Config.GetNoulithColor(state);
        NoulithContainer.SetAddRGB(state > 0 ? noulithColor : new(0));

        Animator -= "SigilPulse";
        SetupSigilPulse(Tracker.CurrentData.State);

        for (var i = 0; i < 4; i++)
        {
            Nouliths[i].SetVis(Config.ShowNouliths);
            ElectricNouliths[i].SetVis(Config.ShowNouliths);
        }
    }

    public override void DrawUI(ref WidgetConfig widgetConfig, ref UpdateFlags update)
    {
        Heading("Layout");
        PositionControls("Position", ref Config.Position, ref update);
        ScaleControls("Scale", ref Config.Scale, ref update);
        ToggleControls("Show Nouliths", ref Config.ShowNouliths, ref update);

        for (var i = 1; i <= Tracker.CurrentData.MaxState; i++)
        {
            Heading(Tracker.StateNames[i]);

            var color = Config.Colors[i];
            var fxColor = Config.FXColors[i];
            var noulithColor = Config.NoulithColors[i];
            if (ColorPickerRGB($"Color##{i}", ref color, ref update)) Config.Colors[i] = color;
            if (ColorPickerRGB($"Pulse##{i}", ref fxColor, ref update)) Config.FXColors[i] = fxColor;
            if (Config.ShowNouliths && ColorPickerRGB($"Nouliths##{i}", ref noulithColor, ref update))
                Config.NoulithColors[i] = noulithColor;
        }

        if (update.HasFlag(Save)) ApplyConfigs();
        widgetConfig.EukrasiaReplicaCfg = Config;
    }

    #endregion
}

public partial class WidgetConfig
{
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public EukrasiaReplicaConfig? EukrasiaReplicaCfg { get; set; }
}
