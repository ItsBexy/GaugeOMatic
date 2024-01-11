using FFXIVClientStructs.FFXIV.Component.GUI;
using GaugeOMatic.Trackers;
using GaugeOMatic.Windows;
using Newtonsoft.Json;
using System;
using System.Numerics;
using static CustomNodes.CustomNodeManager;
using static CustomNodes.CustomNodeManager.Tween;
using static FFXIVClientStructs.FFXIV.Component.GUI.AlignmentType;
using static FFXIVClientStructs.FFXIV.Component.GUI.FontType;
using static GaugeOMatic.Utility.Color;
using static GaugeOMatic.Widgets.GaugeBarWidget.DrainGainType;
using static GaugeOMatic.Widgets.LabelTextProps;
using static GaugeOMatic.Widgets.MahjongRibbon;
using static GaugeOMatic.Widgets.NumTextProps;
using static GaugeOMatic.Widgets.WidgetTags;
using static GaugeOMatic.Widgets.WidgetUI;

namespace GaugeOMatic.Widgets;

public sealed unsafe class MahjongRibbon : GaugeBarWidget
{
    public override WidgetInfo WidgetInfo => GetWidgetInfo;

    public static WidgetInfo GetWidgetInfo => new()
    {
        DisplayName = "Mahjong Ribbon",
        Author = "ItsBexy",
        Description = "A gauge bar based on parts of the Mahjong UI.",
        WidgetTags = GaugeBar
    };

    public override CustomPartsList[] PartsLists { get; } = {
        new ("ui/uld/emjintroparts08.tex", new Vector4(0,0,720,64) ),
        new ("ui/uld/emjintroparts03.tex", new Vector4(0,0,64,64)),
        new ("ui/uld/JobHudNIN0.tex", new Vector4(256, 152, 20, 88))
    };
    #region Nodes

    public CustomNode Bar;
    public CustomNode Frame;
    public CustomNode LabelText;

    public CustomNode Backdrop;

    public CustomNode Tick;

    public override CustomNode BuildRoot()
    {
        Bar = BuildBar();
        Frame = NineGridFromPart(1, 0, 28, 28, 28, 28).SetSize(0,32);
        Tick = ImageNodeFromPart(2, 0).SetAlpha(0).SetOrigin(20,44).SetPos(0,-27.5f).SetScale(0.8f,0.55f).SetImageFlag(32);
        LabelText = CreateLabelTextNode(Config.LabelText.Text, Tracker.DisplayName);
        NumTextNode = CreateNumTextNode();

        Tweens.Add(new(Tick,
                       new(0){ScaleY=0.54f,ScaleX=0.81f},
                       new(300){ScaleY=0.56f, ScaleX = 0.79f },
                       new(600){ ScaleY = 0.54f, ScaleX = 0.81f })
                       {Repeat = true,Ease = Eases.SinInOut});

        return new CustomNode(CreateResNode(), Bar, Frame, Tick, LabelText, NumTextNode).SetOrigin(0,32);
    }

    private CustomNode BuildBar()
    {
        Backdrop = BuildRibbon();
        Drain = BuildRibbon();
        Gain = BuildRibbon();
        Main = BuildRibbon();

        Main[0].SetSize(4320, 50);

        Tweens.Add(new(Main[0],new(0){X=0},new(10000){X=-1440}){Repeat = true});

        return new CustomNode(CreateResNode(), Backdrop, Drain, Gain, Main).SetPos(0, 0).SetSize(Config.Width, 32);
    }

    private CustomNode BuildRibbon() => new CustomNode(CreateResNode(), ImageNodeFromPart(0, 0).SetPos(0,-18).SetSize(2880, 50).SetImageWrap(3)).SetSize(0,32).SetNodeFlags(NodeFlags.Clip);

    #endregion

    #region Animations

    public void CollapseBar(int kf1, int kf2)
    {
        var halfWidth = Config.Width / 2;

        Tweens.Add(new(Frame,
                       new(0) { X = -halfWidth, Width = Config.Width, Height = 32, AddRGB = 0, Alpha = 255, Y = 0 },
                       new(kf1) { X = 0, Width = 0, Height = 32, AddRGB = 50, Alpha = 255, Y = 0 },
                       new(kf2) { X = 0, Width = 0, Height = 32, AddRGB = 255, Alpha = 0, Y = 16 })
                      { Ease = Eases.SinInOut });

        Tweens.Add(new(Bar,
                       new(0) { X = -halfWidth, Alpha = 255, Y = 0, Width = Config.Width, Height = 32 },
                       new(kf1) { X = 0 , Alpha = 255, Y = 0, Width = 0, Height = 32 },
                       new(kf2) { X = 0 , Alpha = 128, Y = 16, Width = 0, Height = 0 })
                      { Ease = Eases.SinInOut });

        Tweens.Add(new(Backdrop,
                       new(0) { Width = Config.Width, Alpha = Config.Background.A },
                       new(kf1) { Width = 0, Alpha = 0 },
                       new(kf2) { Width = 0, Alpha = 0 })
        { Ease = Eases.SinInOut });

        Tweens.Add(new(Drain,
                       new(0) { Alpha = Config.DrainColor.A },
                       new(kf1) { Alpha = 0 }));

        Tweens.Add(new(Gain,
                       new(0) { Alpha = Config.GainColor.A },
                       new(kf1) { Alpha = 0 }));

        Tweens.Add(new(Tick,
                       new(0) { Alpha = Config.TickColor.A },
                       new(kf1) { Alpha = 0 }));

        Tweens.Add(new(LabelText, new(0) { Alpha = 255 }, new(kf1) { Alpha = 0 }));
        Tweens.Add(new(NumTextNode, new(0) { Alpha = 255 }, new(kf2) { Alpha = 0 }));
    }

    public void ExpandBar(int kf1, int kf2)
    {
        var halfWidth = Config.Width / 2;

        Tweens.Add(new(Frame,
                       new(0) { Alpha = 0, Y = 16, X = -16, Width = 32, Height=0, AddRGB = new(200) },
                       new(kf1) { Alpha = 255, Y = 0, X = -16, Width = 32, Height=32, AddRGB = new(255) },
                       new(kf2) { Alpha = 255, Y = 0, X = -halfWidth, Height = 32, Width = Config.Width, AddRGB = 0 })
        { Ease = Eases.SinInOut });

        Tweens.Add(new(Bar,
                       new(0) { Alpha = 0, Y = 16, X = -16 , Width = 32, Height=0 },
                       new(kf1) { Alpha = 255, Y = 0, X = -16 , Width = 32, Height=32 },
                       new(kf2) { Alpha = 255, Y = 0, X = -halfWidth, Width = Config.Width, Height = 32 })
        { Ease = Eases.SinInOut });

        Tweens.Add(new(Backdrop,
                       new(0) { X=0,Width = 32, Height=0,Alpha = 255 },
                       new(kf1) { X = 0, Width = 32, Height=32,Alpha = Config.Background.A },
                       new(kf2) { X = 0, Width = Config.Width, Alpha = Config.Background.A })
                       { Ease = Eases.SinInOut });

        Tweens.Add(new(Main,
                       new(0) { Height = 0 },
                       new(kf1) { Height = 32 }));

        Tweens.Add(new(Drain,
                       new(0) { Alpha = 0, Height = 0 },
                       new(kf1) { Alpha = 0, Height = 32 },
                       new(kf2) { Alpha = Config.DrainColor.A, Height = 32 }));

        Tweens.Add(new(Gain,
                       new(0) { Alpha = 0, Height = 0 },
                       new(kf1) { Alpha = 0, Height = 32 },
                       new(kf2) { Alpha = Config.GainColor.A, Height = 32 }));

        Tweens.Add(new(Tick,
                       new(0) { Alpha = 0 },
                       new(kf1) { Alpha = 0 },
                       new(kf2) { Alpha = Config.TickColor.A }));

        Tweens.Add(new(LabelText, new(0) { Alpha = 0 }, new(kf1) { Alpha = 0 }, new(kf2) { Alpha = 255 }));
        Tweens.Add(new(NumTextNode, new(0) { Alpha = 0 }, new(kf1) { Alpha = 0 }, new(kf2) { Alpha = 255 }));
    }

    #endregion

    #region UpdateFuncs

    public override string? SharedEventGroup => null;

    public override void OnDecreaseToMin(float prog, float prevProg)
    {
        if(Config.Collapse) CollapseBar(200, 250);

        Tweens.Add(new(Tick, new(0) { Alpha = 255 }, new(200) { Alpha = 0 }));
        StopBackdropTween();
    }

    public override void OnIncreaseFromMin(float prog, float prevProg)
    {
        if (Config.Collapse) ExpandBar(100, 250);

        Tweens.Add(new(Tick, new(0) { Alpha = 0 }, new(200) { Alpha = 255 }));
        StartBackdropTween();
    }

    public override void PlaceTickMark(float prog) =>
        Tick.SetPos(Main.Node->Width - (Backdrop.Node->Width / 2f) - 20, -27.5f)
            .SetAlpha(prog switch
            {
                < 0.025f => (byte)(prog * 10200F),
                > 0.975f => (byte)((1 - prog) * 10200F),
                _ => 255
            });

    public override void PostUpdate(float prog, float prevProg)
    {
        if (Drain.Node->Width > 0) Drain[0].SetPos(Backdrop[0].Node->X, 0);
        if (Gain.Node->Width > 0) Gain[0].SetPos(Main[0].Node->X, 0);

        Main.SetWidth(Math.Clamp((float)Main.Node->Width, 0, Backdrop.Node->Width));
        Drain.SetWidth(Math.Clamp((float)Drain.Node->Width, 0, Backdrop.Node->Width));
        Gain.SetWidth(Math.Clamp((float)Gain.Node->Width, 0, Backdrop.Node->Width));
    }

    public override void OnFirstRun(float prog)
    {
        var curWid = CalcBarProperty(prog);
        Main.SetWidth(curWid);
        Gain.SetWidth(curWid);
        Drain.SetWidth(curWid);
        if (prog <= 0 && Config.Collapse) CollapseBar(0, 0);
        Tick.SetAlpha(prog > 0 ? 255 : 0);
        if (prog > 0) { StartBackdropTween(); }
    }

    public override DrainGainType DGType => Width;
    public override float CalcBarProperty(float prog) => Math.Clamp(prog, 0f, 1f) * Config.Width;

    private void StopBackdropTween()
    {
        ClearNodeTweens(ref Tweens, Backdrop[0]);
        Tweens.Add(new(Backdrop[0], new(0) { X = Backdrop[0].Node->X }, new(250) { X = 0 }) { Ease = Eases.SinInOut });
    }

    private void StartBackdropTween()
    {
        ClearNodeTweens(ref Tweens, Backdrop[0]);
        Tweens.Add(new(Backdrop[0], new(0) { X = -1440 }, new(20000) { X = 0 }) { Repeat = true });
    }

    #endregion

    #region Configs

    public sealed class MahjongRibbonConfig : GaugeBarWidgetConfig
    {
        public Vector2 Position = new(0, -27);
        public float Scale = 1;
        public float Width = 144;
        public AddRGB Background = "0x777777FF";
        public AddRGB MainColor = "0x987B7BFF";
        public AddRGB GainColor = "0xDE56B2A0";
        public AddRGB DrainColor = "0x8069B2A0";
        public AddRGB TickColor = "0xD36E27FF"; 
        public bool Collapse;
        public LabelTextProps LabelText = new(string.Empty, false, new(0, 0), new(255), 0x8E6A0CFF, Jupiter, 20, Left);
        protected override NumTextProps NumTextDefault => new(true, new(0, 12.5f), new(255), new(0), MiedingerMed, 18, Left, false);

        public MahjongRibbonConfig(WidgetConfig widgetConfig)
        {
            NumTextProps = NumTextDefault;
            var config = widgetConfig.MahjongRibbonCfg;

            if (config == null) return;

            Position = config.Position;
            Scale = config.Scale;
            Width = config.Width;
            Invert = config.Invert;
            Background = config.Background;
            MainColor = config.MainColor;
            GainColor = config.GainColor;
            DrainColor = config.DrainColor;
            AnimationLength = config.AnimationLength;
            NumTextProps = config.NumTextProps;
            LabelText = config.LabelText;
            Collapse = config.Collapse;
        }

        public MahjongRibbonConfig() => NumTextProps = NumTextDefault;
    }

    public override GaugeBarWidgetConfig GetConfig => Config;

    public MahjongRibbonConfig Config = null!;

    public override void InitConfigs()
    {
        Config = new(Tracker.WidgetConfig);
        if (Tracker.WidgetConfig.MahjongRibbonCfg == null && Tracker.RefType == RefType.Action) { Config.Invert = true; }
    }

    public override void ResetConfigs() => Config = new();

    public Vector2 PosAdjust = new(0, -32);
    public override void ApplyConfigs()
    {
        WidgetRoot.SetPos(Config.Position+PosAdjust).SetWidth(Config.Width).SetScale(Config.Scale);
        Frame.SetPos(Config.Width / -2, 0).SetWidth(Config.Width);
        Bar.SetPos(Config.Width / -2, 0).SetWidth(Config.Width);
        Backdrop.SetSize(Config.Width, 64);
       // Main.SetWidth((Tracker.CurrentData.GaugeValue / Tracker.CurrentData.MaxGauge) * Config.Width);

        Main.SetAddRGB(Config.MainColor, true);
        Backdrop.SetAddRGB(Config.Background,true);
        Drain.SetAddRGB(Config.DrainColor, true);
        Gain.SetAddRGB(Config.GainColor, true);
        Tick.SetAddRGB(Config.TickColor);

        var configLabelText = Config.LabelText;
        configLabelText.Position += new Vector2(Config.Width/-2,-15);
        LabelText.SetText(configLabelText.Text.Length > 0 ? configLabelText.Text : Tracker.DisplayName);
        configLabelText.ApplyTo(LabelText);

        Config.NumTextProps.ApplyTo(NumTextNode);
        NumTextNode.SetPos(Config.NumTextProps.Position.X + (Config.Width / 2) + 10, Config.NumTextProps.Position.Y);
    }

    public override void DrawUI(ref WidgetConfig widgetConfig, ref UpdateFlags update)
    {
        Heading("Layout");
        PositionControls("Position", ref Config.Position, ref update);
        ScaleControls("Scale", ref Config.Scale, ref update);
        FloatControls("Width", ref Config.Width, 64, 1440, 1, ref update);

        Heading("Colors");
        ColorPickerRGBA("Backdrop", ref Config.Background, ref update);
        ColorPickerRGBA("Main Bar", ref Config.MainColor, ref update);
        ColorPickerRGBA("Gain", ref Config.GainColor, ref update);
        ColorPickerRGBA("Drain", ref Config.DrainColor, ref update);
        ColorPickerRGB("Tick Color", ref Config.TickColor, ref update);

        Heading("Behavior");
        ToggleControls("Invert Fill", ref Config.Invert, ref update);
        if (ToggleControls("Collapse Empty", ref Config.Collapse, ref update)) CollapseCheck();
      //  IntControls("Animation Time", ref Config.AnimationLength, 0, 2000, 50, ref update);


        NumTextControls($"{Tracker.TermGauge} Text", ref Config.NumTextProps, ref update);
        LabelTextControls2("Label Text", ref Config.LabelText, Tracker.DisplayName, ref update);

        if (update.HasFlag(UpdateFlags.Save)) ApplyConfigs();
        widgetConfig.MahjongRibbonCfg = Config;
    }

    private void CollapseCheck()
    {
        if (Tracker.CurrentData.GaugeValue == 0 || (Config.Invert && Math.Abs(Tracker.CurrentData.GaugeValue - Tracker.CurrentData.MaxGauge) < 0.01f))
        {
            if (Config.Collapse) CollapseBar(250, 350);
            else ExpandBar(100, 350);
        }
    }

    #endregion

    public MahjongRibbon(Tracker tracker) : base(tracker) { }
}

public partial class WidgetConfig
{
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] public MahjongRibbonConfig? MahjongRibbonCfg { get; set; }
}
