using GaugeOMatic.Trackers;
using GaugeOMatic.Windows;
using Newtonsoft.Json;
using System;
using System.Numerics;
using static CustomNodes.CustomNodeManager;
using static FFXIVClientStructs.FFXIV.Component.GUI.AlignmentType;
using static FFXIVClientStructs.FFXIV.Component.GUI.FontType;
using static GaugeOMatic.Utility.Color;
using static GaugeOMatic.Widgets.InkSlash;
using static GaugeOMatic.Widgets.NumTextProps;
using static GaugeOMatic.Widgets.WidgetTags;
using static GaugeOMatic.Widgets.WidgetUI;
using static GaugeOMatic.Windows.UpdateFlags;

namespace GaugeOMatic.Widgets;

public sealed unsafe class InkSlash : GaugeBarWidget
{
    public override WidgetInfo WidgetInfo => GetWidgetInfo;

    public static WidgetInfo GetWidgetInfo => new()
    {
        DisplayName = "InkSlash Gauge",
        Author = "ItsBexy",
        Description = "A gauge bar shaped like a streak of ink.",
        WidgetTags = GaugeBar
    };

    public override CustomPartsList[] PartsLists { get; } = { 
        new ("ui/uld/Mobhunt5.tex", 
             new(650, 0, 16, 192), 
             new(616, 192,50, 140)), // smudge
        new ("ui/uld/JobHudRDM0.tex", 
             new(119, 322, 85,56), 
             new(0,265,42,44), 
             new(79,207,34,34), 
             new(125,212,24,22), 
             new(123,234,28,20), 
             new(148,222,14,15), 
             new(207,321,39,59))
    };

    #region Nodes

    public CustomNode Bar;
    public CustomNode Backdrop;
    public CustomNode MainContainer;
    public CustomNode GainContainer;
    public CustomNode DrainContainer;
    public CustomNode Tick;

    public CustomNode SplatterBox;
    public CustomNode Splatter1;
    public CustomNode Splatter2;
    public CustomNode Splatter3;
    public CustomNode Splatter4;
    public CustomNode Splatter5;
    public override CustomNode BuildRoot()
    {
        Bar = BuildBar().SetOrigin(-45, 25);
        NumTextNode = CreateNumTextNode().SetSize(30,30).SetOrigin(-10,10);
        return new(CreateResNode(), Bar, NumTextNode);
    }

    private CustomNode BuildBar()
    {
        Tick = ImageNodeFromPart(1, 5).SetOrigin(17, 17).SetPos(0, 1).SetImageFlag(32).SetVis(false);

        Tweens.Add(new(Tick,
                       new(0){ScaleX=0.5f, ScaleY=1.55f},
                       new(600){ScaleX=0.5f,ScaleY=1.6f},
                       new(1000){ScaleX = 0.5f, ScaleY=1.55f})
                       {Repeat=true,Ease = Tween.Eases.SinInOut});

        Backdrop = ImageNodeFromPart(0, 1).SetRotation((float)(Math.PI / 2f)).SetPos(29,11).SetScale(0.8f,1.2f);
        DrainContainer = BuildFillNode();
        GainContainer = BuildFillNode();
        MainContainer = BuildFillNode();
        Main = MainContainer[0];
        Gain = GainContainer[0];
        Drain = DrainContainer[0];

        Splatter1 = ImageNodeFromPart(1, 5).SetAlpha(0).SetRGBA(0,0,0,0);
        Splatter2 = ImageNodeFromPart(1, 4).SetAlpha(0).SetRGBA(0, 0, 0, 0).SetOrigin(14,10);
        Splatter3 = ImageNodeFromPart(1, 3).SetAlpha(0).SetRGBA(0, 0, 0, 0).SetOrigin(12,11);
        Splatter4 = ImageNodeFromPart(1, 5).SetAlpha(0).SetRGBA(0, 0, 0, 0);
        Splatter5 = ImageNodeFromPart(1, 6).SetAlpha(0).SetAddRGB(-255).SetOrigin(0,59);

        SplatterBox = new CustomNode(CreateResNode(), Splatter1, Splatter2, Splatter3, Splatter4, Splatter5).SetPos(0,30);

        return new(CreateResNode(), Backdrop, SplatterBox, DrainContainer, GainContainer, MainContainer, Tick);
    }



    private CustomNode BuildFillNode() => new CustomNode(CreateResNode(), ImageNodeFromPart(0, 0).SetSize(16,0).SetImageWrap(1).SetPos(0,22).SetScale(1,1).SetRotation(-(float)(Math.PI / 2f))).SetPos(-152, 30).SetSize(192, 16);

    #endregion

    #region Animations

    #endregion

    #region UpdateFuncs

    public override string? SharedEventGroup => null;

    public override DrainGainType DGType => DrainGainType.Height;

    public override void OnDecreaseToMin(float prog, float prevProg)
    {
        Tweens.Add(new(Tick, new(0) { Alpha = 255 }, new(200) { Alpha = 0 }));
        Tweens.Add(new(Backdrop,new(0)
        {
            AddRGB = Config.BackdropColor,
            Alpha = Config.BackdropColor.A
        },new(200)
        {
            AddRGB = Config.BackdropInactive,
            Alpha = Config.BackdropInactive.A
        }));
    }

    public override void OnIncreaseFromMin(float prog, float prevProg)
    {
        Tweens.Add(new(Tick, new(0) { Alpha = 0 }, new(200) { Alpha = 255 }));

        Tweens.Add(new(Backdrop, new(0)
        {
            AddRGB = Config.BackdropInactive,
            Alpha = Config.BackdropInactive.A
        }, new(200)
        {
            AddRGB = Config.BackdropColor,
            Alpha = Config.BackdropColor.A
        }));
    }
    public override void PlaceTickMark(float prog) { Tick.SetPos(MainContainer.Node->X - 12 + Main.Node->Height, MainContainer.Node->Y + 10); }

    public override void OnFirstRun(float prog)
    {
        var curWid = CalcBarSize(prog);
        MainContainer.SetWidth(curWid);
        GainContainer.SetWidth(curWid);
        DrainContainer.SetWidth(curWid);
        Tick.SetAlpha(prog <= 0 ? 0 : 255);
        Backdrop.SetAddRGB(prog <= 0 ? Config.BackdropInactive : Config.BackdropColor,true);
    }

    public override void OnIncrease(float prog, float prevProg)
    {
        SplatterBox.SetPos(CalcBarSize(prog)-150, 30);

        Tweens.Add(new(Splatter1,
                       new(0) { X = -90, Y = 20, Scale = 1, Alpha = 0, Rotation = 0.3f + (float)Math.PI },
                       new(160) { X = 20, Y = -40, ScaleX = 2.5f, ScaleY = 1.6f, Alpha = 255, Rotation = -0.1f + (float)Math.PI },
                       new(300) { X = 20, Y = -40, ScaleX = 2.5f, ScaleY = 1.6f, Alpha = 255, Rotation = -0.1f + (float)Math.PI },
                       new(600) { X = 20, Y = -40, ScaleX = 2.5f, ScaleY = 1.6f, Alpha = 0, Rotation = -0.05f + (float)Math.PI }
                   ));

        Tweens.Add(new(Splatter2,
                       new(0) { X = -90, Y = 20, Scale = 1, Alpha = 0, Rotation = 0.3f + (float)Math.PI },
                       new(150) { X = 50, Y = -5, ScaleX = 2.5f, ScaleY = 1.6f, Alpha = 255, Rotation = -0.1f + (float)Math.PI },
                       new(250) { X = 50, Y = -5, ScaleX = 2.5f, ScaleY = 1.6f, Alpha = 255, Rotation = -0.1f + (float)Math.PI },
                       new(600) { X = 50, Y = -5, ScaleX = 2.5f, ScaleY = 1.6f, Alpha = 0, Rotation = -0.05f + (float)Math.PI }
                   ));

        Tweens.Add(new(Splatter3,
                       new(0){X=-100,Scale = 1,Alpha=0,Rotation=-0.3f + (float)Math.PI },
                       new(100) {X=-20, ScaleX = 3.2f, ScaleY = 2.9f,Alpha=255,Rotation=0.3f + (float)Math.PI },
                       new(200) { X = -20, ScaleX = 3.2f, ScaleY = 2.9f, Alpha = 255, Rotation = 0.3f + (float)Math.PI },
                       new(600){X=-20,ScaleX=3.2f,ScaleY=2.9f,Alpha=0, Rotation = 0.4f + (float)Math.PI }
                       ));
        
        Tweens.Add(new(Splatter4,
                       new(0) { X = -70, Y = 10, Scale = 1, Alpha = 0, Rotation = 0.3f + (float)Math.PI },
                       new(180) { X = -40, Y = 30, ScaleX = 2.5f, ScaleY = 1.6f, Alpha = 255, Rotation = -0.1f + (float)Math.PI },
                       new(320) { X = -40, Y = 30, ScaleX = 2.5f, ScaleY = 1.6f, Alpha = 255, Rotation = -0.1f + (float)Math.PI },
                       new(600) { X = -40, Y = 30, ScaleX = 2.5f, ScaleY = 1.6f, Alpha = 0, Rotation = -0.05f + (float)Math.PI }
                       ));

        Tweens.Add(new(Splatter5,
                       new(0) { X = -220, Y = -65, ScaleX = 1.3f, ScaleY = 1.8f, Alpha = 0, Rotation = 1.2f },
                       new(100) { X = -200, Y = -80, ScaleX = 2f, ScaleY = 4f, Alpha = 255, Rotation = 1.4f },
                       new(200) { X = -200, Y = -80, ScaleX = 2f, ScaleY = 4f, Alpha = 255, Rotation = 1.4f },
                       new(700) { X = -200, Y = -80, ScaleX = 2f, ScaleY = 4f, Alpha = 0, Rotation = 1.4f }
                       ));

        Tweens.Add(new(SplatterBox, 
                       new(0){Rotation=0,ScaleX=1,ScaleY=1},
                       new(600){Rotation=0.01f,ScaleX=1.05f,ScaleY=1.06f}));
        
        var flash = Config.MainColor/2;
        Tweens.Add(new(Bar,
                       new(0) { X = 0, Y = 0, AddRGB = new(0) },
                       new(30) { X = 1.9f, Y = 0.95f, AddRGB = flash * 0.1f },
                       new(100) { X = -0.8f, Y = -0.9f, AddRGB = flash * 0.45f },
                       new(160) { X = 1.9f, Y = 0.9f, AddRGB = flash * 1 },
                       new(180) { X = 1.75f, Y = 0.85f, AddRGB = flash * 0.9f },
                       new(240) { X = 0, Y = 0, AddRGB = flash * 0.5f },
                       new(500) { X = 0, Y = 0, AddRGB = new(0, 0, 0) }
                       ));

        var pos = Config.NumTextProps.Position + new Vector2(-33,15);
        Tweens.Add(new(NumTextNode,
                       new(0) { X = pos.X, Y = pos.Y },
                       new(30) { X = pos.X - 1.9f, Y = pos.Y + 0.85f },
                       new(100) { X = pos.X + 0.8f, Y = pos.Y + 0.9f },
                       new(160) { X = pos.X - 1.9f, Y = pos.Y - 0.9f },
                       new(180) { X = pos.X - 1.75f, Y = pos.Y + 0.95f },
                       new(240) { X = pos.X, Y = pos.Y },
                       new(500) { X = pos.X, Y = pos.Y }
                       ));
    }

    public override void PostUpdate(float prog)
    {
    }

    public override float CalcBarSize(float prog) => (Math.Clamp(prog, 0f, 1f) * 185f) + 3f;

    #endregion

    #region Configs

    public sealed class InkSlashConfig : GaugeBarWidgetConfig
    {
        public Vector2 Position = new(0);
        public Vector2 Scale = new(1, 1);
        public AddRGB BackdropColor = new(-255);
        public AddRGB BackdropInactive = new(-255,-255,-255,128);
        public AddRGB MainColor = new(55, -255, -255);
        public AddRGB GainColor = "0xFF785CFF";
        public AddRGB DrainColor = "0x6A003CFF";
        public float Rotation;
        public AddRGB TickColor = new(255, -100, -162);
        protected override NumTextProps NumTextDefault => new(true, new(0, 0), 0xFFFFFFFF, 0x9B0000FF, MiedingerMed, 20, Center, false);

        public InkSlashConfig(WidgetConfig widgetConfig)
        {
            NumTextProps = NumTextDefault;
            var config = widgetConfig.InkSlashCfg;

            if (config == null) return;

            Position = config.Position;
            Scale = config.Scale;
            Rotation = config.Rotation;
            MainColor = config.MainColor;
            BackdropColor = config.BackdropColor;
            GainColor = config.GainColor;
            DrainColor = config.DrainColor;
            NumTextProps = config.NumTextProps;
            Invert = config.Invert;
            AnimationLength = config.AnimationLength;
            BackdropInactive = config.BackdropInactive;
        }

        public InkSlashConfig() => NumTextProps = NumTextDefault;
    }

    public override GaugeBarWidgetConfig GetConfig => Config;

    public InkSlashConfig Config = null!;

    public override void InitConfigs()
    {
        Config = new(Tracker.WidgetConfig);
        if (Tracker.WidgetConfig.InkSlashCfg == null && Tracker.RefType == RefType.Action) { Config.Invert = true; }
    }

    public override void ResetConfigs() => Config = new();

    public AddRGB ColorOffset = new(-46, 110, 110, 0);
    public Vector2 PosAdjust = new(60,-50);
    public override void ApplyConfigs()
    {
        WidgetRoot.SetPos(Config.Position+PosAdjust);
        Bar.SetScale(Config.Scale);
        NumTextNode.SetScale((Config.Scale.Y+Config.Scale.X) / 2f);
        Tick.SetAddRGB(Config.TickColor);

        Splatter1.SetAddRGB(Config.MainColor*2);
        Splatter2.SetAddRGB(Config.MainColor * 2);
        Splatter3.SetAddRGB(Config.MainColor * 2);
        Splatter4.SetAddRGB(Config.MainColor * 2);

        Backdrop.SetAddRGB(Tracker.CurrentData.GaugeValue <= 0 ? Config.BackdropInactive : Config.BackdropColor, true);
        MainContainer.SetAddRGB(Config.MainColor + ColorOffset, true);
        GainContainer.SetAddRGB(Config.GainColor + ColorOffset, true);
        DrainContainer.SetAddRGB(Config.DrainColor + ColorOffset, true);
        var numTextConfig = Config.NumTextProps;
        numTextConfig.Position += new Vector2(-33, 15);
        numTextConfig.ApplyTo(NumTextNode);
    }

    public override void DrawUI(ref WidgetConfig widgetConfig, ref UpdateFlags update)
    {
        Heading("Layout");

        PositionControls("Position", ref Config.Position, ref update);
        ScaleControls("Scale", ref Config.Scale, ref update);

        Heading("Colors");
        ColorPickerRGBA("Active Backdrop", ref Config.BackdropColor, ref update);
        ColorPickerRGBA("Inactive Backdrop", ref Config.BackdropInactive, ref update);
        ColorPickerRGBA("Main Bar", ref Config.MainColor, ref update);
        ColorPickerRGBA("Gain", ref Config.GainColor, ref update);
        ColorPickerRGBA("Drain", ref Config.DrainColor, ref update);
        ColorPickerRGBA("Tick Color", ref Config.TickColor, ref update);

        Heading("Behavior");
        ToggleControls("Invert Fill", ref Config.Invert, ref update);

        NumTextControls($"{Tracker.TermGauge} Text", ref Config.NumTextProps, ref update);

        if (update.HasFlag(Save)) ApplyConfigs();
        widgetConfig.InkSlashCfg = Config;
    }

    #endregion

    public InkSlash(Tracker tracker) : base(tracker) { }
}

public partial class WidgetConfig
{
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] public InkSlashConfig? InkSlashCfg { get; set; }
}
