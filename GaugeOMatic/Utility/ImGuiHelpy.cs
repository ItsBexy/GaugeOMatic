using Dalamud.Interface;
using Dalamud.Interface.Components;
using Dalamud.Interface.Textures.TextureWraps;
using Dalamud.Interface.Utility.Raii;
using ImGuiNET;
using System;
using System.Numerics;
using static Dalamud.Interface.Utility.ImGuiHelpers;
using static GaugeOMatic.Utility.Color;
using static ImGuiNET.ImGuiCol;

namespace GaugeOMatic.Utility;

public static class ImGuiHelpy
{
    public static void MulticolorText(params (Vector4 col, string str)[] stringTuples)
    {
        var y = ImGui.GetCursorPosY();
        for (var i = 0; i < stringTuples.Length; i++)
        {
            if (i > 0) { ImGui.SameLine(0,3); }

            ImGui.SetCursorPosY(y);

            var stringTuple = stringTuples[i];
            ImGui.TextColored(stringTuple.col, stringTuple.str);
        }
    }

    public static void IconButtonDisabled(FontAwesomeIcon icon) => IconButtonDisabled("", icon);

    public static void IconButtonDisabled(string label, FontAwesomeIcon icon)
    {
        var grey15 = new Vector4(1, 1, 1, 0.15f);
        var grey35 = new Vector4(1, 1, 1, 0.35f);
        using (ImRaii.PushColor(Text, grey35))
        {
            Utility.DalamudComponents.ImGuiComponents.IconButton(label, icon, grey15, grey15, grey15);
        }
    }

    public static void TableHeadersRowNoHover(Vector4 textColor)
    {
        var black = new Vector4(0);
        using (ImRaii.PushColor(HeaderHovered, black)
                                           .Push(HeaderActive, black)
                                           .Push(TableHeaderBg, black)
                                           .Push(Text, textColor))
        {
            ImGui.TableHeadersRow();
        }
    }

    public static bool IconButton(string label, FontAwesomeIcon icon, float minWidth = 0, Vector4? defaultColor = null, Vector4? activeColor = null, Vector4? hoveredColor = null)
    {
        return Utility.DalamudComponents.ImGuiComponents.IconButton(label,icon,defaultColor,activeColor,hoveredColor,new(minWidth,0));
    }

    public static unsafe Vector4 GetStyleColorVec4(ImGuiCol idx) => *ImGui.GetStyleColorVec4(idx);

    public static void TableSeparator(int n)
    {
        ImGui.TableNextRow();

        for (var i = 0; i < n; i++)
        {
            ImGui.TableNextColumn();
            ImGui.Spacing();
            ImGui.Separator();
            ImGui.Spacing();
            ImGui.Spacing();
        }
    }

    public static void WriteIcon(FontAwesomeIcon icon, string? iconHoverText = null, ColorRGB? iconColor = null)
    {
        float adjust;
        using (var col = new ImRaii.Color())
        {
            using (ImRaii.PushFont(UiBuilder.IconFont))
            {
                var str = icon.ToIconString();
                adjust = (14 - ImGui.CalcTextSize(str).X) / 2;

                if (iconColor.HasValue) col.Push(Text, (Vector4)iconColor);

                ImGui.SetCursorPosX(ImGui.GetCursorPosX() + adjust);
                ImGui.Text(str);
            }
        }

        if (iconHoverText != null && ImGui.IsItemHovered()) Tooltip(iconHoverText);
        ImGui.SameLine(0,adjust);
    }

    public static void Tooltip(string tooltipText)
    {
        using (ImRaii.Tooltip())
        {
            using (ImRaii.TextWrapPos(ImGui.GetFontSize() * 35f))
            {
                ImGui.TextUnformatted(tooltipText);
            }
        }
    }

    public static bool IconButtonWithText(FontAwesomeIcon icon, string text, Vector4? defaultColor = null, Vector4? activeColor = null, Vector4? hoveredColor = null, Vector2? size = null)
    {
        using var col = new ImRaii.Color();

        if (defaultColor.HasValue)
        {
            col.Push(Button, defaultColor.Value);
        }

        if (activeColor.HasValue)
        {
            col.Push(ButtonActive, activeColor.Value);
        }

        if (hoveredColor.HasValue)
        {
            col.Push(ButtonHovered, hoveredColor.Value);
        }

        if (size.HasValue)
        {
            size *= GlobalScale;
        }

        bool button;

        Vector2 iconSize;
        using (ImRaii.PushFont(UiBuilder.IconFont))
        {
            iconSize = ImGui.CalcTextSize(icon.ToIconString());
        }

        var textStr = text;
        if (textStr.Contains("#"))
        {
            textStr = textStr[..textStr.IndexOf("#", StringComparison.Ordinal)];
        }

        var framePadding = ImGui.GetStyle().FramePadding;
        var iconPadding = 3 * GlobalScale;

        var cursor = ImGui.GetCursorScreenPos();

        using (ImRaii.PushId(text))
        {
            var textSize = ImGui.CalcTextSize(textStr);
            var width = size is { X: not 0 } ? size.Value.X : iconSize.X + textSize.X + (framePadding.X * 2) + iconPadding;
            var height = size is { Y: not 0 } ? size.Value.Y : ImGui.GetFrameHeight();

            button = ImGui.Button(string.Empty, new Vector2(width, height));
        }

        var iconPos = cursor + framePadding;
        var textPos = new Vector2(iconPos.X + iconSize.X + iconPadding, cursor.Y + framePadding.Y);

        var dl = ImGui.GetWindowDrawList();

        using (ImRaii.PushFont(UiBuilder.IconFont))
        {
            dl.AddText(iconPos, ImGui.GetColorU32(Text), icon.ToIconString());
        }

        dl.AddText(textPos, ImGui.GetColorU32(Text), textStr);

        return button;
    }


    public static void TextRightAligned(string text, bool nowrap = false)
    {
        var w = ImGui.CalcTextSize(text).X;
        var space = ImGui.GetColumnWidth();
        if (w > 0)
        {
            if (!nowrap && space < w)
            {
                ImGui.TextWrapped(text);
            }
            else
            {
                ImGui.SetCursorPosX(ImGui.GetCursorPosX() + (space - w));
                ImGui.Text(text);
            }
        }
    }

    public static void DrawGameIcon(uint trackerGameIcon, float height, bool active = true)
    {
        var startPos = ImGui.GetCursorPosX();

        TextureProvider.GetFromGameIcon(new(trackerGameIcon)).TryGetWrap(out var tex, out _);

        if (tex != null)
        {
            var adjustedHeight = height * GlobalScale;
            var width = tex.Width / (float)tex.Height * adjustedHeight;

            var margin = (adjustedHeight - width) / 2f;

            ImGui.SetCursorPosX(ImGui.GetCursorPosX() + margin);
            ImGui.Image(tex.ImGuiHandle, new(width, adjustedHeight), new(0), new(1), new(1, 1, 1, active ? 1 : 0.3f));
            ImGui.SameLine();
        }

        ImGui.SetCursorPosX(startPos + (30 * GlobalScale));
    }

    public static IDalamudTextureWrap? GetGameIconTexture(uint? id)
    {
        if (id == null) return null;
        try
        {
            return TextureProvider.GetFromGameIcon(new(id.Value))
                                  .TryGetWrap(out var texture, out _) ? texture : null;
        }
        catch
        {
            return null;
        }
    }

    public static Vector4 Plain => GetStyleColorVec4(Text);
    public static Vector4 Disabled => GetStyleColorVec4(TextDisabled);
    public static readonly Vector4 Yellow = new(1, 1, 0, 1);
    public static readonly Vector4 Orange = new(1, 0.5f, 0, 1);
}
