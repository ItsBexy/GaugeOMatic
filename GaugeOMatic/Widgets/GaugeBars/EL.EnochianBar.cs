using CustomNodes;
using GaugeOMatic.CustomNodes.Animation;
using GaugeOMatic.Trackers;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel;
using System.Numerics;
using Dalamud.Interface;
using Dalamud.Interface.Components;
using GaugeOMatic.Widgets.Common;
using static CustomNodes.CustomNode;
using static CustomNodes.CustomNodeManager;
using static Dalamud.Interface.FontAwesomeIcon;
using static FFXIVClientStructs.FFXIV.Component.GUI.AlignmentType;
using static FFXIVClientStructs.FFXIV.Component.GUI.FontType;
using static GaugeOMatic.Utility.Color;
using static GaugeOMatic.Widgets.Common.CommonParts;
using static GaugeOMatic.Widgets.Common.NumTextProps;
using static GaugeOMatic.Widgets.WidgetTags;
using static GaugeOMatic.Widgets.Common.WidgetUI;
using static GaugeOMatic.Widgets.Common.WidgetUI.UpdateFlags;
using static GaugeOMatic.Widgets.Common.WidgetUI.WidgetUiTab;
using static System.Math;
using static GaugeOMatic.Widgets.AddonRestrictionsAttribute.RestrictionType;

#pragma warning disable CS8618

namespace GaugeOMatic.Widgets;

[WidgetName("Enochian Bar")]
[WidgetDescription("A curved bar based on BLM's Enochian timer.")]
[WidgetAuthor("ItsBexy")]
[WidgetTags(GaugeBar | MultiComponent | Replica | HasAddonRestrictions | HasClippingMask)]
[AddonRestrictions(ClipConflict)]
[MultiCompData("EL", "Elemental Gauge Replica", 1)]
public sealed unsafe class EnochianBar(Tracker tracker) : GaugeBarWidget(tracker)
{
    public override CustomPartsList[] PartsLists { get; } = [BLM0, CircleMask];

    #region Nodes

    public CustomNode Contents;
    public CustomNode Contents2;
    public CustomNode MainContainer;
    public CustomNode DrainContainer;
    public CustomNode GainContainer;
    public CustomNode MainMask;
    public CustomNode DrainMask;
    public CustomNode GainMask;
    public CustomNode Backplate;
    public CustomNode Lattice;
    public CustomNode Plate;
    public CustomNode Groove;
    public CustomNode Bar;
    public CustomNode ClockHandContainer;
    public CustomNode ClockHand;

    public override Bounds GetBounds() => Lattice;

    public override CustomNode BuildContainer()
    {
        Lattice = ImageNodeFromPart(0, 8).SetAddRGB(-20).SetMultiply(50).SetImageWrap(1);
        Plate = ImageNodeFromPart(0, 9).SetPos(9, 8).SetAddRGB(-20).SetMultiply(50).SetOrigin(0, 1);
        Groove = ImageNodeFromPart(0, 11).SetPos(6, 5).SetAddRGB(-20).SetMultiply(50).SetOrigin(0, 1);

        Backplate = new(CreateResNode(), Lattice, Plate, Groove);

        Drain = FillNode();
        Gain = FillNode();
        Main = FillNode();

        DrainMask = MaskNode();
        GainMask = MaskNode();
        MainMask = MaskNode();

        DrainContainer = new CustomNode(CreateResNode(), Drain, DrainMask).SetSize(85, 85).SetPos(0, 2);
        GainContainer = new CustomNode(CreateResNode(), Gain, GainMask).SetSize(85, 85).SetPos(0, 2);
        MainContainer = new CustomNode(CreateResNode(), Main, MainMask).SetSize(85, 85).SetPos(0, 2);

        Bar = new CustomNode(CreateResNode(), DrainContainer, GainContainer, MainContainer).SetPos(11, 10).SetSize(86, 86).SetAddRGB(-20).SetMultiply(50);

        ClockHand = ImageNodeFromPart(0, 12).SetOrigin(15, 10).DefineTimeline(new(0) { Rotation = -1.5707963267949f }, new(1) { Rotation = 0 });
        ClockHand.Node->SetPriority((ushort)(Config.HandOnTop ? 1 : 0));

        ClockHandContainer = new CustomNode(CreateResNode(), ClockHand).SetPos(-6, -2).SetSize(30, 128);

        NumTextNode = new();
        NumTextNode.Node->SetPriority((ushort)(Config.HandOnTop ? 2 : 0));

        Contents2 = new CustomNode(CreateResNode(), Backplate, Bar, ClockHandContainer).SetSize(128, 124).SetOrigin(8, 8);
        Contents = new CustomNode(CreateResNode(), Contents2).SetOrigin(7, 8);

        return new(CreateResNode(), Contents, NumTextNode);

        CustomNode MaskNode() => ClippingMaskFromPart(1, 0).SetSize(176, 176).SetScale(0.95f, 0.9f).SetOrigin(-16, -16).SetPos(-10, -8);

        CustomNode FillNode() => ImageNodeFromPart(0, 10).SetRotation(-1.55768f).SetOrigin(-2, -4).DefineTimeline(BarTimeline).SetPos(0, -1);
    }

    #endregion

    #region Animations

    public static KeyFrame[] BarTimeline => [new(0) { Rotation = -1.5707963267949f }, new(1) { Rotation = 0 }];

    public override void RevealBar(bool instant = false)
    {
        base.RevealBar(instant);
        var scaleX = Config.Direction == 1 ? -1 : 1;
        Animator += new Tween(Contents2,
                              new(0) { ScaleX = scaleX * 1.2f, ScaleY = 1.2f, Alpha = 0 },
                              new(150) { ScaleX = scaleX, ScaleY = 1, Alpha = 255 }){ Label = "ShowHide" };
    }

    public override void HideBar(bool instant = false)
    {
        base.HideBar(instant);
        Contents2.SetAlpha(0);
        var scaleX = Config.Direction == 1 ? -1 : 1;
        Animator += new Tween(Contents2,
                              new(0) { ScaleX = scaleX, ScaleY = 1, Alpha = 255 },
                              new(150) { ScaleX = scaleX * 0.8f, ScaleY = 0.8f, Alpha = 0 }){ Label = "ShowHide" };
    }

    private void ActivateNode(CustomNode node) =>
        Animator += new Tween(node,
                              new(0) { AddRGB = -20, MultRGB = new(53) },
                              new(150) { AddRGB = 0, MultRGB = new(100) },
                              new(250) { AddRGB = 70, MultRGB = new(100) },
                              new(360) { AddRGB = 0, MultRGB = new(100) })
        { Label = "Activate" };

    #endregion

    #region UpdateFuncs

    public override float AdjustProg(float prog)
    {
        if (!Config.Smooth) prog = (float)Floor(prog * Tracker.CurrentData.MaxGauge) / Tracker.CurrentData.MaxGauge;
        return prog;
    }
    public override void PreUpdate(float prog, float prevProg) => Config.AnimationLength = Config.Smooth || (prog > prevProg) != Config.Invert ? 250 : 0;

    public override void PlaceTickMark(float prog)
    {
        ClockHand.SetProgress(Max(Main.Progress, Drain.Progress));
        ClockHand.SetAlpha(!Config.HideHand || prog > 0);
        if (prog > 0 && !Activated) { UnDimBar(); }
    }

    public override void OnDecreaseToMin()
    {
        Animator -= "Activate";
        if (Config.DimEmpty) DimBar();
        if (Config.HideEmpty) HideBar();
    }

    public override void OnIncreaseFromMin()
    {
        if (Config.DimEmpty) UnDimBar();
        if (Config.HideEmpty) RevealBar();
    }

    public bool Activated;
    private void UnDimBar()
    {
        ActivateNode(Lattice);
        ActivateNode(Plate);
        ActivateNode(Groove);
        ActivateNode(Bar);
        Activated = true;
    }

    private void DimBar()
    {
        Lattice.SetAddRGB(-20).SetMultiply(50);
        Plate.SetAddRGB(-20).SetMultiply(50);
        Groove.SetAddRGB(-20).SetMultiply(50);
        Bar.SetAddRGB(-20).SetMultiply(50);
        Activated = false;
    }

    public override void OnFirstRun(float prog)
    {
        base.OnFirstRun(prog);
        if (prog > 0) UnDimBar();
    }

    #endregion

    #region Configs

    public sealed class EnochianBarConfig : GaugeBarWidgetConfig
    {
        public int Direction;
        public float Angle;

        public ColorRGB PlateColor = new(100, 100, 100);
        public AddRGB MainColor = new(25, -52, 90);
        public AddRGB GainColor = "0xBE9180FF";
        public AddRGB DrainColor = "0x661D5EFF";

        public bool Smooth;
        [DefaultValue(true)] public bool HideHand = true;
        [DefaultValue(true)] public bool DimEmpty = true;
        public bool HandOnTop;

        public LabelTextProps LabelText = new(string.Empty, false, new(0, 32), new(255), new(0), Jupiter, 16, Left);
        protected override NumTextProps NumTextDefault => new(enabled: false,
                                                              position: new(0, 0),
                                                              color: new(255),
                                                              edgeColor: new(0),
                                                              showBg: true,
                                                              bgColor: new(0),
                                                              font: MiedingerMed,
                                                              fontSize: 20,
                                                              align: Center,
                                                              invert: false);

        public EnochianBarConfig(WidgetConfig widgetConfig) : base(widgetConfig.EnochianBarCfg)
        {
            var config = widgetConfig.EnochianBarCfg;

            if (config == null) return;

            Direction = config.Direction;
            Angle = config.Angle;

            PlateColor = config.PlateColor;
            MainColor = config.MainColor;
            GainColor = config.GainColor;
            DrainColor = config.DrainColor;

            Smooth = config.Smooth;
            HideHand = config.HideHand;
            DimEmpty = config.DimEmpty;
            HandOnTop = config.HandOnTop;

            LabelText = config.LabelText;
        }

        public EnochianBarConfig() { }
    }

    private EnochianBarConfig config;

    public override EnochianBarConfig Config => config;

    public override void InitConfigs()
    {
        config = new(Tracker.WidgetConfig);
        if (Tracker.WidgetConfig.EnochianBarCfg == null && ShouldInvertByDefault) { Config.Invert = true; }
    }

    public override void ResetConfigs() => config = new();

    public override void ApplyConfigs()
    {
        base.ApplyConfigs();
        var colorOffset = new AddRGB(-25, 52, -90);

        Main.SetAddRGB(Config.MainColor + colorOffset, true).SetProgress(CalcProg());
        Gain.SetAddRGB(Config.GainColor + colorOffset, true).SetProgress(0);
        Drain.SetAddRGB(Config.DrainColor + colorOffset, true).SetProgress(0);

        WidgetContainer.SetPos(Config.Position + new Vector2(86, 68));

        Contents.SetRotation(Config.Angle, true);

        Backplate.SetMultiply(Config.PlateColor);

        DirectionAdjust(Config.Direction);

        NumTextNode.ApplyProps(NumTextProps, new Vector2(8, 5));
        NumTextNode.Show().SetAlpha(NumTextProps.Enabled);
    }

    private void DirectionAdjust(int direction)
    {
        var flipFactor = direction == 1 ? -1 : 1;
        var angle = direction == 1 ? -90 : 0;

        ClockHand.SetScaleX(flipFactor);

        Lattice.SetScaleX(flipFactor)
               .SetRotation(angle, true);

        Contents2.SetScaleX(flipFactor)
                .SetRotation(angle, true);
    }

    public override void DrawUI()
    {
        base.DrawUI();
        switch (UiTab)
        {
            case Layout:
                AngleControls("Angle", ref Config.Angle);
                RadioIcons("Direction", ref Config.Direction, [0, 1], [RedoAlt, UndoAlt]);

                if (ToggleControls("Force Clock Hand To Top", ref Config.HandOnTop)) UpdateFlag |= Reset;
                ImGuiComponents.HelpMarker("Attempts to force the clock hand to the top layer, above other widgets.\n\nNOTE: Can be finicky, may not always work.",FontAwesomeIcon.QuestionCircle);

                break;
            case Colors:
                ColorPickerRGB("Plate Tint", ref Config.PlateColor);
                ColorPickerRGBA("Main Bar", ref Config.MainColor);
                ColorPickerRGBA("Gain", ref Config.GainColor);
                ColorPickerRGBA("Drain", ref Config.DrainColor);
                break;
            case Behavior:
                SplitChargeControls(ref Config.SplitCharges, Tracker.RefType, Tracker.CurrentData.MaxCount);
                ToggleControls("Turn Smoothly", ref Config.Smooth);
                ToggleControls("Invert Fill", ref Config.Invert);

                var emptyBehavior = new List<bool> { Config.DimEmpty, Config.HideEmpty, Config.HideHand };
                if (ToggleControls("When Empty", ref emptyBehavior, ["Dim Bar", "Hide Bar", "Hide Clock Hand"]))
                {
                    if (Config.HideEmpty != emptyBehavior[1]) HideCheck(emptyBehavior[1]);
                    if (Config.DimEmpty != emptyBehavior[0]) DimCheck(emptyBehavior[0]);
                    Config.DimEmpty = emptyBehavior[0];
                    Config.HideEmpty = emptyBehavior[1];
                    Config.HideHand = emptyBehavior[2];
                    Config.HideFull = false;
                }
                break;
            case Text:
                NumTextControls($"{Tracker.TermGauge} Text", ref Config.NumTextProps);
                break;
            default:
                break;
        }
    }

    private void DimCheck(bool dimEmpty)
    {
        if (Tracker.CurrentData.GaugeValue == 0 || (Config.Invert && Abs(Tracker.CurrentData.GaugeValue - Tracker.CurrentData.MaxGauge) < 0.01f))
        {
            if (dimEmpty) DimBar();
            else UnDimBar();
        }
    }

    private void HideCheck(bool hideEmpty)
    {
        if (Tracker.CurrentData.GaugeValue == 0 || (Config.Invert && Abs(Tracker.CurrentData.GaugeValue - Tracker.CurrentData.MaxGauge) < 0.01f))
        {
            if (hideEmpty) HideBar();
            else RevealBar();
        }
    }

    #endregion
}

public partial class WidgetConfig
{
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] public EnochianBar.EnochianBarConfig? EnochianBarCfg { get; set; }
}
