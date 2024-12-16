using CustomNodes;
using Dalamud.Interface.Textures.TextureWraps;
using GaugeOMatic.CustomNodes.Animation;
using GaugeOMatic.Trackers;
using ImGuiNET;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using static CustomNodes.CustomNodeManager;
using static CustomNodes.CustomNodeManager.CustomPartsList;
using static GaugeOMatic.GaugeOMatic;
using static GaugeOMatic.Utility.Color;
using static GaugeOMatic.Widgets.SimpleGem;
using static GaugeOMatic.Widgets.SimpleGem.SimpleGemConfig;
using static GaugeOMatic.Widgets.SimpleGem.SimpleGemConfig.FrameBases;
using static GaugeOMatic.Widgets.SimpleGem.SimpleGemConfig.GemShapes;
using static GaugeOMatic.Widgets.WidgetTags;
using static GaugeOMatic.Widgets.Common.WidgetUI;
using static GaugeOMatic.Widgets.Common.WidgetUI.UpdateFlags;
using static GaugeOMatic.Widgets.Common.WidgetUI.WidgetUiTab;
using static ImGuiNET.ImGuiMouseCursor;
using static System.Math;

#pragma warning disable CS8618

namespace GaugeOMatic.Widgets;

[WidgetName("Simple Gems")]
[WidgetDescription("A counter based on Simple Mode job gauges.")]
[WidgetAuthor("ItsBexy")]
[WidgetTags(Counter | Replica)]
[WidgetUiTabs(Layout | Colors | Behavior | Icon | Sound)]
public sealed unsafe class SimpleGem(Tracker tracker) : FreeGemCounter(tracker)
{
    public static readonly Vector4[] Coords =
    [
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
        new(0, 512, 64, 64), new(64, 512, 64, 64),    // 32,33 Hex
        new(128, 512, 64, 64), new(192, 512, 64, 64)  // 34,35 Hollow Hex
    ];

    public override CustomPartsList[] PartsLists { get; } =
    [
        new(AssetFromFile(Path.Combine(PluginDirPath,@"TextureAssets\SimpleGems.tex")), Coords),
        new(AssetFromFile(Path.Combine(PluginDirPath,@"TextureAssets\SimpleGemsSilver.tex")), Coords),
        new(AssetFromFile(Path.Combine(PluginDirPath,@"TextureAssets\SimpleGemsBlack.tex")), Coords)
    ];

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

    public List<CustomNode> Frames = [];
    public List<CustomNode> Gems = [];

    public override CustomNode BuildContainer()
    {
        Max = Config.AsTimer ? Config.TimerSize : Tracker.GetCurrentData().MaxCount;

        BuildStacks(Max);

        return new CustomNode(CreateResNode(), Stacks.ToArray()).SetOrigin(32, 32);
    }

    private void BuildStacks(int count)
    {
        Stacks = [];
        Frames = [];
        Gems = [];

        for (var i = 0; i < count; i++)
        {
            Frames.Add(ImageNodeFromPart(0, 0)
                           .SetOrigin(32, 32));

            Gems.Add(ImageNodeFromPart(0, 1)
                     .SetMultiply(80)
                     .SetOrigin(32, 32)
                     .SetAlpha(0));

            Stacks.Add(new CustomNode(CreateResNode(), Frames[i], Gems[i]).SetPos(i * 40, 0).SetOrigin(32, 32).SetSize(64, 64));
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

    public class SimpleGemConfig : FreeGemCounterConfig
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

        public AddRGB GemColor = new(120, 30, -40);
        public GemShapes GemShape;
        public FrameBases FrameBase;
        public ColorRGB FrameColor = new(100);
        public bool HideEmpty;

        [JsonIgnore] public override float SpacingModifier => 2;

        public SimpleGemConfig(WidgetConfig widgetConfig) : base(widgetConfig.SimpleGemCfg)
        {
            var config = widgetConfig.SimpleGemCfg;

            if (config == null) return;

            GemColor = config.GemColor;

            GemShape = config.GemShape;
            FrameColor = config.FrameColor;
            HideEmpty = config.HideEmpty;

            FrameBase = config.FrameBase;
        }

        public SimpleGemConfig() { }
    }

    private SimpleGemConfig config;

    public override SimpleGemConfig Config => config;

    public override void InitConfigs() => config = new(Tracker.WidgetConfig);

    public override void ResetConfigs() => config = new();

    public override void ApplyConfigs()
    {
        base.ApplyConfigs();
        WidgetContainer.SetPos(Config.Position - new Vector2(16))
                       .SetScale(Config.Scale / 2f);

        PlaceFreeGems();

        for (var i = 0; i < Stacks.Count; i++)
        {
            Gems[i].SetPartId(GetGemPart(Config.GemShape))
                   .SetAddRGB(Config.GemColor)
                   .SetMultiply(80);

            Frames[i].SetPartsList(PartsLists[(uint)Config.FrameBase])
                     .SetPartId(GetFramePart(Config.GemShape))
                     .SetMultiply(Config.FrameColor);
        }
    }

    protected override float AdjustedGemAngle(int i, float widgetAngle)
    {
        var gemAngle = base.AdjustedGemAngle(i, widgetAngle);

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
            gemAngle = -widgetAngle;
        }

        return gemAngle;
    }

    protected override Vector2 GemFlipFactor(float gemAngle, float widgetAngle) =>
        Config.GemShape switch
        {
            Chevron => new(1, Abs(gemAngle + widgetAngle) % 360 <= 90 ? 1 : -1),
            Rectangle => CalcRectFlip((gemAngle + widgetAngle) % 360),
            _ => new(1, 1)
        };

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

    public override int ArcMask => Config.GemShape is Circle or CircleHollow ? 0b10111 : 0b11111;

    public override void DrawUI()
    {
        switch (UiTab)
        {
            case Layout:
                GemSelectUI();
                base.DrawUI();
                break;
            case Colors:
                ColorPickerRGB("Gem Color", ref Config.GemColor);
                RadioControls("Frame Base", ref Config.FrameBase, [Brass, Silver, Black], ["Brass", "Silver", "Black"]);
                ColorPickerRGB("Frame Tint", ref Config.FrameColor);
                break;
            case Behavior:
                base.DrawUI();
                if (ToggleControls("Hide Empty", ref Config.HideEmpty))
                {
                    if (Config.HideEmpty && ((!Config.AsTimer && Tracker.CurrentData.Count == 0) || (Config.AsTimer && Tracker.CurrentData.GaugeValue == 0))) AllVanish();
                    if (!Config.HideEmpty && WidgetContainer.Alpha < 255) AllAppear();
                }
                break;
            default:
                break;
        }
    }

    private void GemSelectUI()
    {
        var spriteSheet = TextureProvider.GetFromFile(Path.Combine(PluginDirPath, @"TextureAssets\SimpleGemUI.png"))
                                         .GetWrapOrDefault();
        if (spriteSheet != null)
        {
            LabelColumn("Shape");

            GemSelect(spriteSheet, ref Config.GemShape, 0, 0, Diamond);
            ImGui.SameLine();
            GemSelect(spriteSheet, ref Config.GemShape, 0, 1, DiamondHollow);
            ImGui.SameLine();
            GemSelect(spriteSheet, ref Config.GemShape, 0, 6, Rectangle);

            GemSelect(spriteSheet, ref Config.GemShape, 0, 3, Circle);
            ImGui.SameLine();
            GemSelect(spriteSheet, ref Config.GemShape, 0, 4, CircleHollow);
            ImGui.SameLine();
            GemSelect(spriteSheet, ref Config.GemShape, 2, 5, Cross);

            GemSelect(spriteSheet, ref Config.GemShape, 2, 3, Triangle);
            ImGui.SameLine();
            GemSelect(spriteSheet, ref Config.GemShape, 2, 4, TriangleHollow);
            ImGui.SameLine();
            GemSelect(spriteSheet, ref Config.GemShape, 0, 2, Chevron);

            GemSelect(spriteSheet, ref Config.GemShape, 0, 8, Hex);
            ImGui.SameLine();
            GemSelect(spriteSheet, ref Config.GemShape, 2, 8, HexHollow);
            ImGui.SameLine();
            GemSelect(spriteSheet, ref Config.GemShape, 0, 5, Teardrop);

            GemSelect(spriteSheet, ref Config.GemShape, 2, 0, Setsu);
            ImGui.SameLine();
            GemSelect(spriteSheet, ref Config.GemShape, 2, 1, Getsu);
            ImGui.SameLine();
            GemSelect(spriteSheet, ref Config.GemShape, 2, 2, Ka);

            GemSelect(spriteSheet, ref Config.GemShape, 0, 7, MusicNote);
            ImGui.SameLine();
            GemSelect(spriteSheet, ref Config.GemShape, 2, 6, Star);
            ImGui.SameLine();
            GemSelect(spriteSheet, ref Config.GemShape, 2, 7, Horse);
        }
    }

    private static void GemSelect(IDalamudTextureWrap spriteSheet, ref GemShapes currentShape, int col, int row, GemShapes shape)
    {
        if (currentShape == shape) col++;
        ImGui.Image(spriteSheet.ImGuiHandle, new(32, 32), new(col / 4f, row / 9f), new((col + 1) / 4f, (row + 1) / 9f));
        if (ImGui.IsItemClicked())
        {
            currentShape = shape;
            UpdateFlag |= Save;
        }

        if (ImGui.IsItemHovered()) ImGui.SetMouseCursor(Hand);
    }

    #endregion
}

public partial class WidgetConfig
{
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] public SimpleGemConfig? SimpleGemCfg { get; set; }
}
