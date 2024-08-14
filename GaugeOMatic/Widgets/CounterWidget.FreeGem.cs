using CustomNodes;
using GaugeOMatic.Trackers;
using GaugeOMatic.Utility;
using ImGuiNET;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using static CustomNodes.CustomNode;
using static Dalamud.Interface.FontAwesomeIcon;
using static GaugeOMatic.Utility.MiscMath;
using static GaugeOMatic.Widgets.FreeGemCounterConfig.ArrangementStyle;
using static GaugeOMatic.Widgets.FreeGemCounterConfig.ColorStyles;
using static GaugeOMatic.Widgets.WidgetUI;
using static GaugeOMatic.Widgets.WidgetUI.WidgetUiTab;
using static System.Math;

// ReSharper disable VirtualMemberCallInConstructor

namespace GaugeOMatic.Widgets;

[SuppressMessage("ReSharper", "VirtualMemberNeverOverridden.Global")]
[SuppressMessage("ReSharper", "SwitchStatementMissingSomeEnumCasesNoDefault")]
public abstract class FreeGemCounter : CounterWidget
{
    protected FreeGemCounter(Tracker tracker) : base(tracker) { }

    public abstract override FreeGemCounterConfig GetConfig { get; }

    public List<CustomNode> Stacks = new();

    public virtual string StackTerm => "Gem";
    public virtual int ListMask => 0b111;
    public virtual int ArcMask => 0b11111;

    public void PlaceFreeGems()
    {
        if (GetConfig.Arrangement == Arc) PlaceGemsInArc();
        else PlaceGemsFromList();
    }

    public virtual void PlaceGemsFromList()
    {
        GetConfig.PrepareLists(Stacks.Count);
        WidgetContainer.SetRotation(0);

        for (var i = 0; i < Stacks.Count; i++)
        {
            var gemLayout = GetConfig.LayoutList![i];
            var gemPos = new Vector2(gemLayout.X, gemLayout.Y);
            var gemAngle = gemLayout.Z;
            var scale = GemFlipFactor(gemAngle, 0) * gemLayout.W;

            Stacks[i].SetPos(gemPos)
                     .SetScale(scale)
                     .SetRotation(gemAngle, true);
        }
    }

    public virtual void PlaceGemsInArc()
    {
        var widgetAngle = GetConfig.Angle + (GetConfig.Curve / 2f);
        WidgetContainer.SetRotation(widgetAngle, true);

        var posAngle = 0f;
        double x = 0;
        double y = 0;
        for (var i = 0; i < Stacks.Count; i++)
        {
            var gemAngle = AdjustedGemAngle(i, widgetAngle);
            var gemPos = AdjustedGemPos(i, x, y, Radians(gemAngle));
            var gemScale = GemFlipFactor(gemAngle, widgetAngle) *
                           Math.Max(0f, float.Lerp(1, GetConfig.ScaleShift, i / 10f));
            var gemSpacing = GetConfig.Spacing * GetConfig.SpacingModifier *
                             float.Lerp(1, GetConfig.ScaleShift, (i + 0.5f) / 10f);

            Stacks[i].SetPos(gemPos)
                     .SetScale(gemScale)
                     .SetRotation(gemAngle, true);

            var angleRad = Radians(posAngle);
            x += Cos(angleRad) * gemSpacing;
            y += Sin(angleRad) * gemSpacing;
            posAngle += GetConfig.Curve;
        }
    }

    public void FreeGemControls()
    {
        RadioControls("Arrangement", ref GetConfig.Arrangement, new() { Arc, Individual }, new() { "Arc", "Individual" });
        if (GetConfig.Arrangement == Arc)
        {
            ArcControls();
        }
        else
        {
            GetConfig.PrepareLists(Stacks.Count);
            ListHeaderNav(ref GetConfig.ListIndex);
            ListControls();
        }
    }

    public void ArcControls()
    {
        if ((ArcMask & 0b10000) > 0) FloatControls("Spacing", ref GetConfig.Spacing, -1000, 1000, 0.5f);
        if ((ArcMask & 0b01000) > 0) AngleControls($"Angle ({StackTerm})", ref GetConfig.GemAngle);
        if ((ArcMask & 0b00100) > 0) AngleControls("Angle (Group)", ref GetConfig.Angle);
        if ((ArcMask & 0b00010) > 0) AngleControls("Curve", ref GetConfig.Curve, true);
        if ((ArcMask & 0b00001) > 0) ScaleControls("Scale Shift", ref GetConfig.ScaleShift);
    }

    private void ListHeaderNav(ref int index)
    {
        ImGuiHelpy.TableSeparator(2);

        ImGui.TableNextRow();
        ImGui.TableNextColumn();
        ImGui.TableNextColumn();

        if (index <= 0) ImGuiHelpy.IconButtonDisabled(ChevronLeft);
        else if (ImGuiHelpy.IconButton("##decrement", ChevronLeft)) index--;

        ImGuiHelpy.SameLineSquished();
        ImGui.Text($"{StackTerm} #{index + 1}");
        ImGuiHelpy.SameLineSquished();

        if (index >= Stacks.Count - 1) ImGuiHelpy.IconButtonDisabled(ChevronRight);
        else if (ImGuiHelpy.IconButton("##increment", ChevronRight)) index++;

        index = Clamp(index, 0, Stacks.Count - 1);
    }

    private void ListControls()
    {
        var i = GetConfig.ListIndex;
        var layout = GetConfig.LayoutList![i];
        var pos = new Vector2(layout.X, layout.Y);

        if ((ListMask & 0b100) > 0 && PositionControls($"Position##pos{i}", ref pos)) layout = layout with { X = pos.X, Y = pos.Y };
        if ((ListMask & 0b010) > 0) AngleControls($"Angle##angle{i}", ref layout.Z);
        if ((ListMask & 0b001) > 0) ScaleControls($"Scale##scale{i}", ref layout.W);

        GetConfig.LayoutList[i] = layout;
    }

    protected virtual Vector2 AdjustedGemPos(int i, double x, double y, float gemAngle) => new((float)x, (float)y);
    protected virtual float AdjustedGemAngle(int i, float widgetAngle) => (GetConfig.Curve * (i - 0.5f)) + GetConfig.GemAngle;
    protected virtual Vector2 GemFlipFactor(float gemAngle, float widgetAngle) => new(1, 1);

    public override void DrawUI(ref WidgetConfig widgetConfig)
    {
        base.DrawUI(ref widgetConfig);

        switch (UiTab)
        {
            case Layout:
                FreeGemControls();
                break;
        }
    }

    public float MaxScale => Math.Max(GetConfig.Scale, GetConfig.ScaleShift * (Stacks.Count / 10) * GetConfig.Scale);

    public override void DrawBounds(uint col = 0xffffffff)
    {
          foreach (var edge in GetConcaveHull(Stacks, Math.Max(GetConfig.Spacing/2f,80), MaxScale))
              ImGui.GetBackgroundDrawList().AddLine(edge.A, edge.B, col);


          /*  foreach (var edge in GetMaxBox(Stacks))
                ImGui.GetBackgroundDrawList().AddLine(edge.A, edge.B, col);*/
    }
}

[SuppressMessage("ReSharper", "UnusedMember.Global")]
public abstract class FreeGemCounterConfig : CounterWidgetConfig
{
    public enum ArrangementStyle { Arc, Individual }
    public enum ColorStyles { Same, Unique }

    public ArrangementStyle Arrangement = Arc;
    public ColorStyles ColorStyle = Same;
    public float Spacing = 20;
    public float GemAngle;
    public float Angle;
    public float Curve;
    [DefaultValue(1)] public float ScaleShift = 1;

    [JsonIgnore] public int ListIndex;
    public List<Vector4>? LayoutList;

    protected FreeGemCounterConfig(FreeGemCounterConfig? config) : base(config)
    {
        if (config == null) { return; }

        Arrangement = config.Arrangement;
        ColorStyle = config.ColorStyle;
        Spacing = config.Spacing;
        Angle = config.Angle;
        Curve = config.Curve;
        ScaleShift = config.ScaleShift;
        GemAngle = config.GemAngle;
        LayoutList = Arrangement == Individual ? config.LayoutList : null;
    }

    protected FreeGemCounterConfig()
    {
        Spacing = 20;
        Angle = 0;
        Curve = 0;
        LayoutList = null;
    }

    public virtual float SpacingModifier => 1;

    public void PrepareLists(int i)
    {
        if (LayoutList?.Count >= i) return;

        LayoutList ??= new();
        while (LayoutList.Count < i) { LayoutList.Add(new(Spacing * SpacingModifier * LayoutList.Count, 0, 0, 1)); }
    }
}
