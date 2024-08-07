using CustomNodes;
using Dalamud.Interface.Textures.TextureWraps;
using GaugeOMatic.CustomNodes.Animation;
using GaugeOMatic.Trackers;
using ImGuiNET;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Numerics;
using static CustomNodes.CustomNodeManager;
using static CustomNodes.CustomNodeManager.CustomPartsList;
using static GaugeOMatic.GaugeOMatic;
using static GaugeOMatic.Utility.Color;
using static GaugeOMatic.Utility.MiscMath;
using static GaugeOMatic.Widgets.SimpleGem;
using static GaugeOMatic.Widgets.SimpleGem.SimpleGemConfig;
using static GaugeOMatic.Widgets.SimpleGem.SimpleGemConfig.FrameBases;
using static GaugeOMatic.Widgets.SimpleGem.SimpleGemConfig.GemShapes;
using static GaugeOMatic.Widgets.WidgetTags;
using static GaugeOMatic.Widgets.WidgetUI;
using static System.Math;
using static GaugeOMatic.Trackers.Tracker;
using static GaugeOMatic.Trackers.Tracker.UpdateFlags;
using static GaugeOMatic.Widgets.WidgetUI.WidgetUiTab;
using static ImGuiNET.ImGuiMouseCursor;

#pragma warning disable CS8618

namespace GaugeOMatic.Widgets;

public sealed unsafe class SimpleGem : CounterWidget
{
    public SimpleGem(Tracker tracker) : base(tracker) { }

    public override WidgetInfo WidgetInfo => GetWidgetInfo;

    public static WidgetInfo GetWidgetInfo { get; } = new()
    {
        DisplayName = "Simple Gems",
        Author = "ItsBexy",
        Description = "A counter based on Simple Mode job gauges.",
        WidgetTags = Counter | Replica,
        UiTabOptions = Layout | Colors | Behavior
    };

    public static Vector4[] Coords = {
        new(0, 0, 64, 64), new(64, 0, 64, 64),        // 0,1   Diamond
        new(0, 64, 64, 64), new(64, 64, 64, 64),      // 2,3   Hollow Diamond
        new(0, 128, 64, 64), new(64, 128, 64, 64),    // 4,5   Chevron
        new(0, 192, 64, 64), new(64, 192, 64, 64),    // 6,7   Circle
        new(0, 256, 64, 64), new(64, 256, 64, 64),    // 8,9   Hollow Circle
        new(0, 320, 64, 64), new(64, 320, 64, 64),    // 10,11 Teardrop
        new(0, 384, 64, 64), new(64, 384, 64, 64),    // 12,13 Rectangle
        new(0, 448, 64, 64), new(64, 448, 64, 64),    // 14,15 Music Note
        new(128, 0, 64, 64), new(192, 0, 64, 64),     // 16,17 Setsu
        new(128, 64, 64, 64), new(192, 64, 64, 64),   // 18,19 Getsu
        new(128, 128, 64, 64), new(192, 128, 64, 64), // 20,21 Ka
        new(128, 192, 64, 64), new(192, 192, 64, 64), // 22,23 Triangle
        new(128, 256, 64, 64), new(192, 256, 64, 64), // 24,25 Hollow Triangle
        new(128, 320, 64, 64), new(192, 320, 64, 64), // 26,27 Cross
        new(128, 384, 64, 64), new(192, 384, 64, 64), // 28,29 Star
        new(128, 448, 64, 64), new(192, 448, 64, 64), // 30,31 Horse
        new(0, 512, 64, 64), new(64, 512, 64, 64),   // 32,33 Hex
        new(128, 512, 64, 64), new(192, 512, 64, 64)  // 34,35 Hollow Hex
    };

    public override CustomPartsList[] PartsLists { get; } = {
        new(AssetFromFile(Path.Combine(PluginDirPath,@"TextureAssets\SimpleGems.tex")), Coords),
        new(AssetFromFile(Path.Combine(PluginDirPath,@"TextureAssets\SimpleGemsSilver.tex")), Coords),
        new(AssetFromFile(Path.Combine(PluginDirPath,@"TextureAssets\SimpleGemsBlack.tex")), Coords)


       /* new("ui/uld/JobHudSimple_StackA.tex", new(0, 0, 32, 32),  new(32, 0, 32, 32)), // diamond
        new("ui/uld/JobHudSimple_StackA.tex", new(0, 0, 32, 32),  new(32, 32, 32, 32)),// hollow diamond
        new("ui/uld/JobHudSimple_StackB.tex", new(0, 0, 32, 32),  new(32, 0, 32, 32)), // chevron
        new("ui/uld/jobhudsmn1.tex",          new(434, 597, 32, 32), new(434, 629, 32, 32)), // teardrop
        new("ui/uld/jobhudsmn1.tex",          new(498, 598, 32, 32), new(498, 630, 32, 32)), // rectangle
        new("ui/uld/jobhudbrd0.tex",          new(244, 110, 32, 32), new(278, 110, 32, 32)), // music note
        new("ui/uld/jobhudsam1.tex",          new(240, 318, 32, 32), new(240, 350, 32, 32)), // setsu
        new("ui/uld/jobhudsam1.tex",          new(272, 318, 32, 32), new(272, 350, 32, 32)), // getsu
        new("ui/uld/jobhudsam1.tex",          new(304, 318, 32, 32), new(304, 350, 32, 32))  // ka*/
    };

    public static uint GetFramePart(GemShapes shape) =>
        shape switch
        {
            Diamond => 0,
            DiamondHollow => 2,
            Chevron => 4,
            Circle => 6,
            CircleHollow => 8,
            Teardrop => 10,
            Rectangle => 12,
            MusicNote => 14,
            Setsu => 16,
            Getsu => 18,
            Ka => 20,
            Triangle => 22,
            TriangleHollow => 24,
            Cross => 26,
            Star => 28,
            Horse => 30,
            Hex => 32,
            HexHollow => 34,
            _ => 0
        };

    public static uint GetGemPart(GemShapes shape) => GetFramePart(shape) + 1;

    #region Nodes

    public List<CustomNode> Stacks = new();
    public List<CustomNode> Frames = new();
    public List<CustomNode> Gems = new();

    public override CustomNode BuildContainer()
    {
        Max = Config.AsTimer ? Config.TimerSize : Tracker.GetCurrentData().MaxCount;

        BuildStacks(Max);

        return new CustomNode(CreateResNode(), Stacks.ToArray()).SetOrigin(32, 32);
    }

    private void BuildStacks(int count)
    {
        Stacks = new();
        Frames = new();
        Gems = new();

        for (var i = 0; i < count; i++)
        {
            Frames.Add(ImageNodeFromPart(0, 0)
                           .SetOrigin(32, 32));

            Gems.Add(ImageNodeFromPart(0, 1)
                     .SetMultiply(80)
                     .SetOrigin(32, 32)
                     .SetAlpha(0));

            Stacks.Add(new CustomNode(CreateResNode(), Frames[i], Gems[i]).SetPos(i * 40, 0).SetOrigin(32, 32));
        }
    }

    #endregion

    #region Animations

    public override void ShowStack(int i)
    {
        var colorOffset = new AddRGB(0);
        Animator += new Tween(Gems[i],
                              new(0) { Scale = 2.4f, Alpha = 0, AddRGB = Config.GemColor + colorOffset + new AddRGB(80) },
                              new(125) { Scale = 1, Alpha = 255, AddRGB = Config.GemColor + colorOffset });
    }

    public override void HideStack(int i)
    {
        var colorOffset = new AddRGB(0);
        Animator += new Tween(Gems[i],
                              new(0) { Alpha = 255, AddRGB = Config.GemColor + colorOffset, Scale = 1 },
                              new(90) { Alpha = 0, AddRGB = Config.GemColor + colorOffset + new AddRGB(80), Scale = 0.8f });
    }

    private void AllVanish() =>
        Animator += new Tween(WidgetContainer,
                              new(0) { Alpha = 255, AddRGB = 0 },
                              new(200) { Alpha = 0, AddRGB = 100 });

    private void AllAppear() =>
        Animator += new Tween(WidgetContainer,
                              new(0) { Alpha = 0, AddRGB = 100 },
                              new(200) { Alpha = 255, AddRGB = 0 });

    #endregion

    #region UpdateFuncs

    public override void OnFirstRun(int count, int max)
    {
        for (var i = 0; i < max; i++) Gems[i].SetAlpha(i < count);
        if (count == 0 && Config.HideEmpty) WidgetContainer.SetAlpha(0);
    }

    public override void OnDecreaseToMin() { if (Config.HideEmpty) AllVanish(); }

    public override void OnIncreaseFromMin() { if (Config.HideEmpty || WidgetContainer.Alpha < 255) { AllAppear(); } }

    #endregion

    #region Configs

    public class SimpleGemConfig : CounterWidgetConfig
    {
        public enum GemShapes
        {
            Diamond,
            DiamondHollow,
            Chevron,

            Teardrop,
            Rectangle,
            MusicNote,

            Setsu,
            Getsu,
            Ka,

            Circle,
            CircleHollow,
            Triangle,
            TriangleHollow,
            Cross,
            Star,
            Horse,
            Hex,
            HexHollow
        }

        public enum FrameBases
        {
            Brass = 0,
            Silver = 1,
            Black = 2
        }

        public Vector2 Position;
        [DefaultValue(1f)] public float Scale = 1;
        public AddRGB GemColor = new(120, 30, -40);
        public GemShapes GemShape;
        public FrameBases FrameBase;
        [DefaultValue(20f)] public float Spacing = 20;
        public float Angle;
        public float Curve;
        public float GemAngle;
        public ColorRGB FrameColor = new(100);
        public bool HideEmpty;

        public SimpleGemConfig(WidgetConfig widgetConfig)
        {
            var config = widgetConfig.SimpleGemCfg;

            if (config == null) return;

            Position = config.Position;
            Scale = config.Scale;
            GemColor = config.GemColor;

            GemShape = config.GemShape;
            Spacing = config.Spacing;
            Angle = config.Angle;
            GemAngle = config.GemAngle;
            Curve = config.Curve;
            FrameColor = config.FrameColor;
            HideEmpty = config.HideEmpty;

            FrameBase = config.FrameBase;

            AsTimer = config.AsTimer;
            TimerSize = config.TimerSize;
            InvertTimer = config.InvertTimer;
        }

        public SimpleGemConfig() { }
    }

    public override CounterWidgetConfig GetConfig => Config;

    public SimpleGemConfig Config;

    public override void InitConfigs() => Config = new(Tracker.WidgetConfig);

    public override void ResetConfigs() => Config = new();

    public override void ApplyConfigs()
    {
        var widgetAngle = Config.Angle + (Config.Curve / 2f);
        WidgetContainer.SetPos(Config.Position - new Vector2(16))
                  .SetScale(Config.Scale / 2f)
                  .SetRotation(widgetAngle, true);

        var posAngle = 0f;
        double x = 0;
        double y = 0;

        for (var i = 0; i < Stacks.Count; i++)
        {
            var (gemAngle, scale) = CalcGemProps(i, widgetAngle);

            Gems[i].SetPartId(GetGemPart(Config.GemShape))
                   .SetAddRGB(Config.GemColor)
                   .SetMultiply(80);

            Frames[i].SetPartsList(PartsLists[(uint)Config.FrameBase])
                     .SetPartId(GetFramePart(Config.GemShape))
                     .SetMultiply(Config.FrameColor);

            Stacks[i].SetPos((float)x, (float)y)
                     .SetScale(scale)
                     .SetRotation(gemAngle, true);

            var angleRad = Radians(posAngle);
            x += Cos(angleRad) * Config.Spacing * 2;
            y += Sin(angleRad) * Config.Spacing * 2;
            posAngle += Config.Curve;
        }
    }

    private (float gemAngle, Vector2 scale) CalcGemProps(int i, float widgetAngle)
    {
        var gemAngle = (Config.Curve * (i - 0.5f)) + Config.GemAngle;

        if (Config.GemShape is Diamond or DiamondHollow or Cross)
        {
            while (gemAngle + widgetAngle > 45) gemAngle -= 90;
            while (gemAngle + widgetAngle <= -45) gemAngle += 90;
        }
        else if (Config.GemShape is Triangle or TriangleHollow)
        {
            while (gemAngle + widgetAngle >= 80) gemAngle -= 120;
            while (gemAngle + widgetAngle < -40) gemAngle += 120;
        }
        else if (Config.GemShape is Star)
        {
            while (gemAngle + widgetAngle >= 36) gemAngle -= 72;
            while (gemAngle + widgetAngle < -36) gemAngle += 72;
        }
        else if (Config.GemShape is Hex or HexHollow)
        {
            while (gemAngle + widgetAngle >= 30) gemAngle -= 60;
            while (gemAngle + widgetAngle < -30) gemAngle += 60;
        }
        else if (Config.GemShape is Circle or CircleHollow)
        {
            gemAngle = 0;
        }

        var scale = Config.GemShape switch
        {
            Chevron => new(1, Abs(gemAngle + widgetAngle) % 360 <= 90 ? 1 : -1),
            Rectangle => CalcRectFlip((gemAngle + widgetAngle) % 360),
            _ => new(1, 1)
        };

        return (gemAngle, scale);
    }

    private static Vector2 CalcRectFlip(float angle)
    {
        while (angle < -180) angle += 360;
        while (angle > 180) angle -= 360;

        return angle switch
        {
            <= -90 => new(-1, -1),
            <= 0 => new(-1, 1),
            <= 90 => new(1, 1),
            _ => new(1, -1)
        };
    }

    public override void DrawUI(ref WidgetConfig widgetConfig, ref UpdateFlags update)
    {
        switch (UiTab)
        {
            case Layout:
                GemSelectUI(ref update);
                PositionControls("Position", ref Config.Position, ref update);
                ScaleControls("Scale", ref Config.Scale, ref update);
                FloatControls("Spacing", ref Config.Spacing, -1000, 1000, 0.5f, ref update);
                if (Config.GemShape is not (Circle or CircleHollow)) AngleControls("Angle (Gem)", ref Config.GemAngle, ref update);
                AngleControls("Angle (Group)", ref Config.Angle, ref update);
                AngleControls("Curve", ref Config.Curve, ref update, true);
                break;
            case Colors:
                ColorPickerRGB("Gem Color", ref Config.GemColor, ref update);
                RadioControls("Frame Base", ref Config.FrameBase, new() { Brass, Silver, Black }, new() { "Brass", "Silver", "Black" }, ref update);
                ColorPickerRGB("Frame Tint", ref Config.FrameColor, ref update);
                break;
            case Behavior:
                if (ToggleControls("Hide Empty", ref Config.HideEmpty, ref update))
                {
                    if (Config.HideEmpty && ((!Config.AsTimer && Tracker.CurrentData.Count == 0) || (Config.AsTimer && Tracker.CurrentData.GaugeValue == 0))) AllVanish();
                    if (!Config.HideEmpty && WidgetContainer.Alpha < 255) AllAppear();
                }
                CounterAsTimerControls(ref Config.AsTimer, ref Config.InvertTimer, ref Config.TimerSize, Tracker.TermGauge, ref update);
                break;
            default:
                break;
        }

        if (update.HasFlag(Save)) ApplyConfigs();
        widgetConfig.SimpleGemCfg = Config;
    }

    private void GemSelectUI(ref UpdateFlags update)
    {
        var spriteSheet = TextureProvider.GetFromFile(Path.Combine(PluginDirPath, @"TextureAssets\SimpleGemUI.png"))
                                         .GetWrapOrDefault();
        if (spriteSheet != null)
        {
            LabelColumn("Shape");

            GemSelect(spriteSheet, ref Config.GemShape, 0, 0, Diamond, ref update);
            ImGui.SameLine();
            GemSelect(spriteSheet, ref Config.GemShape, 0, 1, DiamondHollow, ref update);
            ImGui.SameLine();
            GemSelect(spriteSheet, ref Config.GemShape, 0, 6, Rectangle, ref update);

            GemSelect(spriteSheet, ref Config.GemShape, 0, 3, Circle, ref update);
            ImGui.SameLine();
            GemSelect(spriteSheet, ref Config.GemShape, 0, 4, CircleHollow, ref update);
            ImGui.SameLine();
            GemSelect(spriteSheet, ref Config.GemShape, 2, 5, Cross, ref update);

            GemSelect(spriteSheet, ref Config.GemShape, 2, 3, Triangle, ref update);
            ImGui.SameLine();
            GemSelect(spriteSheet, ref Config.GemShape, 2, 4, TriangleHollow, ref update);
            ImGui.SameLine();
            GemSelect(spriteSheet, ref Config.GemShape, 0, 2, Chevron, ref update);

            GemSelect(spriteSheet, ref Config.GemShape, 0, 8, Hex, ref update);
            ImGui.SameLine();
            GemSelect(spriteSheet, ref Config.GemShape, 2, 8, HexHollow, ref update);
            ImGui.SameLine();
            GemSelect(spriteSheet, ref Config.GemShape, 0, 5, Teardrop, ref update);

            GemSelect(spriteSheet, ref Config.GemShape, 2, 0, Setsu, ref update);
            ImGui.SameLine();
            GemSelect(spriteSheet, ref Config.GemShape, 2, 1, Getsu, ref update);
            ImGui.SameLine();
            GemSelect(spriteSheet, ref Config.GemShape, 2, 2, Ka, ref update);

            GemSelect(spriteSheet, ref Config.GemShape, 0, 7, MusicNote, ref update);
            ImGui.SameLine();
            GemSelect(spriteSheet, ref Config.GemShape, 2, 6, Star, ref update);
            ImGui.SameLine();
            GemSelect(spriteSheet, ref Config.GemShape, 2, 7, Horse, ref update);
        }
    }

    private static void GemSelect(IDalamudTextureWrap spriteSheet, ref GemShapes currentShape, int col, int row, GemShapes shape, ref UpdateFlags update)
    {
        if (currentShape == shape) col++;
        ImGui.Image(spriteSheet.ImGuiHandle, new(32, 32), new Vector2(col / 4f, row / 9f), new((col + 1) / 4f, (row + 1) / 9f));
        if (ImGui.IsItemClicked())
        {
            currentShape = shape;
            update |= Save;
        }

        if (ImGui.IsItemHovered()) ImGui.SetMouseCursor(Hand);
    }

    #endregion
}

public partial class WidgetConfig
{
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] public SimpleGemConfig? SimpleGemCfg { get; set; }
}
