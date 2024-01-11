using FFXIVClientStructs.FFXIV.Component.GUI;
using GaugeOMatic.Trackers;
using GaugeOMatic.Utility;
using GaugeOMatic.Windows;
using ImGuiNET;
using Newtonsoft.Json;
using System;
using System.Numerics;
using static CustomNodes.CustomNodeManager;
using static CustomNodes.CustomNodeManager.Tween;
using static FFXIVClientStructs.FFXIV.Component.GUI.AlignmentType;
using static FFXIVClientStructs.FFXIV.Component.GUI.FontType;
using static GaugeOMatic.Utility.Color;
using static GaugeOMatic.Widgets.EspritBar;
using static GaugeOMatic.Widgets.GaugeBarWidget.DrainGainType;
using static GaugeOMatic.Widgets.MilestoneType;
using static GaugeOMatic.Widgets.NumTextProps;
using static GaugeOMatic.Widgets.WidgetTags;
using static GaugeOMatic.Widgets.WidgetUI;

namespace GaugeOMatic.Widgets;

public sealed unsafe class EspritBar : GaugeBarWidget
{
    public override WidgetInfo WidgetInfo => GetWidgetInfo;

    public static WidgetInfo GetWidgetInfo => new() 
    {
        DisplayName = "Esprit Bar",
        Author = "ItsBexy",
        Description = "A curved bar based on DNC's Esprit Gauge",
        WidgetTags = GaugeBar
    };

    public override CustomPartsList[] PartsLists { get; } = {
        new ("ui/uld/JobHudDNC1.tex",
            new(0,0,168,76),    // 0  bar
            new(1,77,166,74),   // 1  backdrop
            new(168,0,48,108),  // 2  feather
            new(216,0,84,100),  // 3  half fan
            new(216,100,84,80), // 4  half frame
            new(168,108,48,32), // 5  corner clip thingy
            new(168,140,48,36), // 6  number bg
            new(2,160,76,60),   // 7  feather glow
            new(80,156,54,40),  // 8  spotlights
            new(79,198,54,40),  // 9  streaks
            new(132,153,20,20), // 10 star
            new(216,180,84,80)  // 11 half frame cover
            )};

    #region Nodes

    public CustomNode Backdrop;
    public CustomNode FanPlate;
    public CustomNode BarContents;
    public CustomNode FrameCover;
    public CustomNode Frame;
    public CustomNode TextBg;
    public CustomNode TextNineGrid;
    public CustomNode FanClip;
    public CustomNode FillNodes;

    public override CustomNode NumTextNode { get; set; }
    public CustomNode MainContainer;
    public CustomNode DrainContainer;
    public CustomNode GainContainer;
    public override CustomNode Drain { get; set; }
    public override CustomNode Gain { get; set; }
    public override CustomNode Main { get; set; }

    public override CustomNode BuildRoot()
    {
        FanPlate = ImageNodeFromPart(0, 3).SetImageWrap(3).SetPos(16, 22).SetSize(168, 100);
        BarContents = BuildBarContents();
        FanClip = ImageNodeFromPart(0, 5).SetPos(76, 90).SetSize(48, 32).SetImageWrap(1);

        // feathers would go after fanplate if we kept them
        return new CustomNode(CreateResNode(), FanPlate, BarContents, FanClip).SetSize(200,128).SetOrigin(100,113);
    }

    private CustomNode BuildBarContents()
    {
        Backdrop = ImageNodeFromPart(0, 1).SetPos(1, 3).SetSize(166, 74).SetImageWrap(2).SetOrigin(84, 0);
        FillNodes = BuildFillNodes();
        FrameCover = ImageNodeFromPart(0, 11).SetSize(168, 80).SetImageWrap(3).Hide(); // dunno whether/when to bother displaying this
        Frame = ImageNodeFromPart(0, 4).SetSize(168, 80).SetImageWrap(3);

        TextBg = NineGridFromPart(0, 6).SetSize(64, 40).SetOrigin(32, 20).SetAlpha(0x7F).SetScale(0.5f,1);
        NumTextNode = CreateNumTextNode().SetPos(15,5).SetSize(33,30);
        TextNineGrid = new CustomNode(CreateResNode(), TextBg, NumTextNode).SetPos(52, 20).SetSize(64,40);

        return new CustomNode(CreateResNode(), Backdrop, FillNodes, FrameCover, Frame, TextNineGrid).SetPos(16,22).SetSize(168,80);
    }

    private CustomNode BuildFillNodes()
    {
        CustomNode FillNode() => ImageNodeFromPart(0, 0).SetOrigin(84, 82).SetRotation(-151,true).SetDrawFlags(0xC);

        static CustomNode FillContainer(CustomNode node) =>
            new CustomNode(CreateResNode(), node)
                .SetSize(168, 70)
                .SetNodeFlags(NodeFlags.Clip)
                .SetDrawFlags(0x200);

        Drain = FillNode();
        Gain = FillNode();
        Main = FillNode();

        MainContainer = FillContainer(Main);
        DrainContainer = FillContainer(Drain);
        GainContainer = FillContainer(Gain);

        return new CustomNode(CreateResNode(),
                              DrainContainer, 
                              GainContainer,
                              MainContainer).SetPos(0,1)
                                            .SetSize(168,70).SetOrigin(0,-1);
    }

    #endregion

    #region Animations

    public void SetupBarPulse(int time)
    {
        ClearLabelTweens(ref Tweens,"BarPulse");
        Tweens.Add(new(MainContainer,
                       new(0) {AddRGB = Config.PulseColor2 - Config.MainColor },
                       new(time/2) {AddRGB = Config.PulseColor - Config.MainColor},
                       new(time) { AddRGB = Config.PulseColor2 - Config.MainColor }) 
                       { Ease = Eases.SinInOut, Repeat = true, Label = "BarPulse" });
        Pulsing = true;
    }

    private void StopBarPulse()
    {
        ClearLabelTweens(ref Tweens, "BarPulse");
        MainContainer.SetAddRGB(0);
        Pulsing = false;
    }

    private void ShowBar()
    {
        var flipFactor = Config.Mirror ? -1f : 1f;
        Tweens.Add(new(WidgetRoot,
                       new(0) { Alpha=0,ScaleX = 1.2f * Config.Scale, ScaleY = Config.Scale * 1.2f * flipFactor },
                       new(150) { Alpha=255,ScaleX = Config.Scale, ScaleY = Config.Scale * flipFactor }) 
                       { Ease = Eases.SinInOut });
    }

    private void HideBar()
    {
        var flipFactor = Config.Mirror ? -1f : 1f;
        Tweens.Add(new(WidgetRoot,
                       new(0) { Alpha = 255, ScaleX = Config.Scale, ScaleY = Config.Scale * flipFactor },
                       new(150) { Alpha = 0, ScaleX = 0.8f * Config.Scale, ScaleY = Config.Scale * 0.8f * flipFactor })
                       { Ease = Eases.SinInOut });
    }

    #endregion

    #region UpdateFuncs

    public override string? SharedEventGroup => null;

    public override DrainGainType DGType => Rotation;
    public override float CalcBarProperty(float prog) => (2.5639436751938f * prog) - 2.6542183675969f;

    public override void OnIncreaseFromMin(float prog, float prevProg) { if (Config.HideEmpty) ShowBar(); }
    public override void OnDecreaseToMin(float prog, float prevProg) { if (Config.HideEmpty) HideBar(); }

    public override void OnFirstRun(float prog)
    {
        Main.SetRotation(CalcBarProperty(prog));
        Gain.SetRotation(-151,true);
        Drain.SetRotation(-151, true);

        if (Config.HideEmpty && prog == 0) WidgetRoot.SetAlpha(0);
    }

    public bool Pulsing;

    public override void PostUpdate(float prog, float prevProg)
    {
        var checkPulse = Config.MilestoneCheck(prog, Milestone);
        if (!Pulsing && checkPulse) SetupBarPulse(1600);
        else if (Pulsing && !checkPulse) StopBarPulse();

        var nodeText = NumTextNode.Node->GetAsAtkTextNode()->NodeText.ToString();
        var chars = nodeText.Length;
        TextBg.SetScaleX(nodeText == " " ? 0 : (0.25f * chars) + 0.266666667f);
    }

    #endregion

    #region Configs

    public sealed class EspritBarConfig : GaugeBarWidgetConfig
    {
        public Vector2 Position = new(0, 0);
        public float Scale = 1;
        public bool ShowPlate = true;
        public bool Mirror;

        public AddRGB Backdrop = new(0, 0, 0);
        public ColorRGB FrameColor = new(100, 100, 100);
        public AddRGB MainColor = new(120, 0, -190);
        public AddRGB GainColor = new(-80, 30, 160);
        public AddRGB DrainColor = new(-50, -140, -70);
        public AddRGB PulseColor = new AddRGB(160, 100, 80)+ new AddRGB(120, 0, -190);
        public AddRGB PulseColor2 = new AddRGB(-120) + new AddRGB(120, 0, -190);
        public AddRGB TextBG = new(0,0,0,0x7f);

        public LabelTextProps LabelText = new(string.Empty, false, new(0, 32), new(255), 0x8E6A0CFF, Jupiter, 16, Left);
        protected override NumTextProps NumTextDefault => new(true, new(0,0), new(255), new(157,131,91), MiedingerMed,18,Center,false,0,true);

        public EspritBarConfig(WidgetConfig widgetConfig)
        {
            MilestoneType = Above;
            NumTextProps = NumTextDefault;
            var config = widgetConfig.EspritBarCfg;

            if (config != null)
            {
                Position = config.Position;
                Scale = config.Scale;
                ShowPlate = config.ShowPlate;
                Mirror = config.Mirror;

                Backdrop = config.Backdrop;
                FrameColor = config.FrameColor;
                MainColor = config.MainColor;
                GainColor = config.GainColor;
                DrainColor = config.DrainColor;
                PulseColor= config.PulseColor;
                PulseColor2 = config.PulseColor2;
                TextBG = config.TextBG;

                MilestoneType = config.MilestoneType;
                Milestone = config.Milestone;
                Invert = config.Invert;
                HideEmpty =config.HideEmpty;
                AnimationLength = config.AnimationLength;

                NumTextProps = config.NumTextProps;
                LabelText = config.LabelText;
            }
        }

        public EspritBarConfig()
        {
            MilestoneType = Above;
            NumTextProps = NumTextDefault;
        }
    }

    public override GaugeBarWidgetConfig GetConfig => Config;

    public EspritBarConfig Config = null!;

    public override void InitConfigs()
    {
        Config = new(Tracker.WidgetConfig);
        if (Tracker.WidgetConfig.EspritBarCfg == null && Tracker.RefType == RefType.Action) { Config.Invert = true; }
    }

    public override void ResetConfigs() => Config = new();

    public override void ApplyConfigs()
    {
        WidgetRoot.SetPos(Config.Position)
                  .SetScale(Config.Scale,Config.Scale*(Config.Mirror ? -1 : 1));

        FanPlate.SetVis(Config.ShowPlate)
                .SetMultiply(Config.FrameColor);

        FanClip.SetVis(Config.ShowPlate)
               .SetMultiply(Config.FrameColor);

        Frame.SetMultiply(Config.FrameColor);

        Backdrop.SetAddRGB(Config.Backdrop,true);

        TextBg.SetAddRGB(Config.TextBG,true);

        var flipOffset = Config.Mirror ? 70 : 0;
        Main.SetAddRGB(Config.MainColor).SetY(-flipOffset);
        Drain.SetAddRGB(Config.DrainColor).SetY(-flipOffset);
        Gain.SetAddRGB(Config.GainColor).SetY(-flipOffset);

        MainContainer.SetY(flipOffset);
        DrainContainer.SetY(flipOffset);
        GainContainer.SetY(flipOffset);

        var prog = CalcProg();
        switch (Config.MilestoneType)
        {
            case Above when prog >= Milestone:
            case Below when prog <= Milestone:
                SetupBarPulse(1600);
                break;
            default:
                StopBarPulse();
                break;
        }

        var numTextProps = Config.NumTextProps;
        if (Config.Mirror) numTextProps.Position *= new Vector2(1, -1);

        numTextProps.ApplyTo(NumTextNode,new Vector2(15, Config.Mirror?35:5) - numTextProps.Position);
        TextNineGrid.SetPos(numTextProps.Position + new Vector2(52, 20)).SetVis(numTextProps.Enabled);
    }

    public override void DrawUI(ref WidgetConfig widgetConfig, ref UpdateFlags update)
    {
        Heading("Layout");

        PositionControls("Position", ref Config.Position, ref update);
        ScaleControls("Scale", ref Config.Scale, ref update);
        ToggleControls("Fan Plate", ref Config.ShowPlate, ref update);

        ToggleControls("Mirror", ref Config.Mirror, ref update);

        Heading("Colors");

        ColorPickerRGBA("Backdrop", ref Config.Backdrop, ref update);
        ColorPickerRGB("Frame Tint", ref Config.FrameColor, ref update);
        ColorPickerRGBA("Main Bar", ref Config.MainColor, ref update);
        ColorPickerRGBA("Gain", ref Config.GainColor, ref update);
        ColorPickerRGBA("Drain", ref Config.DrainColor, ref update);

        if (Config.MilestoneType > 0)
        {
            ColorPickerRGB("Pulse Colors", ref Config.PulseColor, ref update);
            ColorPickerRGB(" ##Pulse2", ref Config.PulseColor2, ref update);
        }

        Heading("Behavior");
        ToggleControls("Invert Fill", ref Config.Invert, ref update);
        if (ToggleControls("Hide Empty", ref Config.HideEmpty, ref update)) HideCheck(Config.HideEmpty);
        RadioControls("Pulse", ref Config.MilestoneType, new() { None, Above, Below }, new() { "Never", "Above Milestone", "Below Milestone" }, ref update);

        if (Config.MilestoneType > 0) PercentControls(ref Config.Milestone, ref update);

        NumTextControlsEsprit(ref update);

        if (update.HasFlag(UpdateFlags.Save)) ApplyConfigs();
        widgetConfig.EspritBarCfg = Config;
    }

    public void HideCheck(bool hideEmpty)
    {
        if (Tracker.CurrentData.GaugeValue == 0 || (Config.Invert && Math.Abs(Tracker.CurrentData.GaugeValue - Tracker.CurrentData.MaxGauge) < 0.01f))
        {
            if (hideEmpty) HideBar();
            else ShowBar();
        }
    }

    private void NumTextControlsEsprit(ref UpdateFlags update)
    {
        var label = $"{Tracker.TermGauge} Text";
        var numTextProps = Config.NumTextProps;
        ImGuiHelpers.TableSeparator(2);

        ImGui.TableNextRow();
        ImGui.TableNextColumn();
        var treeNode = ImGui.TreeNodeEx($"{label}##{label}treeRow");

        var enabled = numTextProps.Enabled;
        ImGui.TableNextColumn();
        if (ImGui.Checkbox($"##{label}Enabled", ref enabled))
        {
            numTextProps.Enabled = enabled;
            update |= UpdateFlags.Save;
        }

        if (treeNode)
        {
            PositionControls("Position", ref numTextProps.Position, ref update);
            ColorPickerRGBA($"Color##{label}color", ref numTextProps.Color, ref update);
            ColorPickerRGBA($"Edge Color##{label}edgeColor", ref numTextProps.EdgeColor, ref update);
            ColorPickerRGBA($"Backdrop##{label}backdrop", ref Config.TextBG, ref update);

            ComboControls($"Font##{label}font", ref numTextProps.Font, FontList, FontNames, ref update);
            RadioIcons($"Alignment##{label}align", ref numTextProps.Align, AlignList, AlignIcons, ref update);
            IntControls($"Font Size##{label}fontSize", ref numTextProps.FontSize, 1, 100, 1, ref update);

            RadioControls("Precision ", ref numTextProps.Precision, new() { 0, 1, 2 }, new() { "0", "1", "2" }, ref update,true);
            ToggleControls("Invert Value ", ref numTextProps.Invert, ref update);
            ToggleControls("Show Zero ", ref numTextProps.ShowZero, ref update);
            ImGui.TreePop();
        }

        if (update.HasFlag(UpdateFlags.Save)) Config.NumTextProps = numTextProps;
    }

    #endregion

    public EspritBar(Tracker tracker) : base(tracker) { }
}

public partial class WidgetConfig
{
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] public EspritBarConfig? EspritBarCfg { get; set; }
}
