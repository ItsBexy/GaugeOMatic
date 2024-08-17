using Dalamud.Interface;
using Dalamud.Interface.Components;
using Dalamud.Interface.Textures.TextureWraps;
using ImGuiNET;
using System;
using System.Numerics;
using static Dalamud.Interface.Utility.ImGuiHelpers;
using static GaugeOMatic.Utility.Color;
using static ImGuiNET.ImGuiCol;
// ReSharper disable UnusedMethodReturnValue.Global
// ReSharper disable UnusedMember.Global

namespace GaugeOMatic.Utility;

public static class ImGuiHelpy
{
    public struct PushableStyleColor
    {
        public ImGuiCol ImGuiCol;
        public Vector4 Color;

        public PushableStyleColor(ImGuiCol col, Vector4 color)
        {
            ImGuiCol = col;
            Color = color;
        }
    }

    public static void MulticolorText(params (Vector4 col, string str)[] stringTuples)
    {
        var y = ImGui.GetCursorPosY();
        for (var i = 0; i < stringTuples.Length; i++)
        {
            if (i > 0) { SameLineSquished(); }

            ImGui.SetCursorPosY(y);

            var stringTuple = stringTuples[i];
            ImGui.TextColored(stringTuple.col, stringTuple.str);
        }
    }

    public static void PushStyleColorMulti(params PushableStyleColor[] styles)
    {
        foreach (var style in styles) ImGui.PushStyleColor(style.ImGuiCol, style.Color);
    }

    public static void IconButtonDisabled(FontAwesomeIcon icon) => IconButtonDisabled("", icon);

    public static void IconButtonDisabled(string label, FontAwesomeIcon icon)
    {
        var grey15 = new Vector4(1, 1, 1, 0.15f);
        PushStyleColorMulti(new(Button, grey15),
                            new(ButtonActive, grey15),
                            new(ButtonHovered, grey15),
                            new(Text, new(1, 1, 1, 0.35f)));

        ImGuiComponents.IconButton(label, icon);
        ImGui.PopStyleColor(4);
    }

    public static void TableHeadersRowNoHover(Vector4 textColor)
    {
        var black = new Vector4(0);
        PushStyleColorMulti(new(HeaderHovered, black), new(HeaderActive, black), new(TableHeaderBg, black), new(Text, textColor));
        ImGui.TableHeadersRow();
        ImGui.PopStyleColor(4);
    }

    public static bool IconButton(string label, FontAwesomeIcon icon, float minWidth = 15f, Vector4? defaultColor = null, Vector4? activeColor = null, Vector4? hoveredColor = null)
    {
        minWidth *= GlobalScale;
        var iconText = icon.ToIconString();
        defaultColor ??= GetStyleColorVec4(Button);
        activeColor ??= GetStyleColorVec4(ButtonActive);
        hoveredColor ??= GetStyleColorVec4(ButtonHovered);
        var count = 0;
        if (defaultColor.HasValue)
        {
            ImGui.PushStyleColor(Button, defaultColor.Value);
            ++count;
        }
        if (activeColor.HasValue)
        {
            ImGui.PushStyleColor(ButtonActive, activeColor.Value);
            ++count;
        }
        if (hoveredColor.HasValue)
        {
            ImGui.PushStyleColor(ButtonHovered, hoveredColor.Value);
            ++count;
        }
        var str = iconText;
        if (str.Contains('#'))
            str = str[..str.IndexOf("#", StringComparison.Ordinal)];
        ImGui.PushID(label);
        ImGui.PushFont(UiBuilder.IconFont);
        var vector2 = ImGui.CalcTextSize(str);

        var diff = 0f;
        if (vector2.X < minWidth)
        {
            diff = minWidth - vector2.X;
            vector2.X = minWidth;
        }

        ImGui.PopFont();
        var windowDrawList = ImGui.GetWindowDrawList();
        var cursorScreenPos = ImGui.GetCursorScreenPos();
        var x1 = vector2.X + (ImGui.GetStyle().FramePadding.X * 2f);
        var frameHeight = ImGui.GetFrameHeight();
        var flag = ImGui.Button(string.Empty, new(x1, frameHeight));
        Vector2 pos = default;
        ref var local = ref pos;
        var x2 = (double)cursorScreenPos.X;
        var style = ImGui.GetStyle();
        var x3 = (double)style.FramePadding.X;
        var x4 = x2 + x3 + (diff/2);
        var y1 = (double)cursorScreenPos.Y;
        style = ImGui.GetStyle();
        var y2 = (double)style.FramePadding.Y;
        var y3 = y1 + y2;
        local = new((float)x4, (float)y3);
        ImGui.PushFont(UiBuilder.IconFont);
        windowDrawList.AddText(pos, ImGui.GetColorU32(Text), str);
        ImGui.PopFont();
        ImGui.PopID();
        if (count > 0) ImGui.PopStyleColor(count);
        return flag;
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

    public static void SameLineSquished()
    {
        ImGui.SameLine();
        ImGui.SetCursorPosX(ImGui.GetCursorPosX() - ((ImGui.GetFontSize() / -6f) + 7f));
    }

    public static void SameLineWrappable(float max)
    {
        ImGui.SameLine();
        if (ImGui.GetCursorPosX() > max) ImGui.NewLine();
    }

    public static void WriteIcon(FontAwesomeIcon icon, string? iconHoverText = null, ColorRGB? iconColor = null)
    {
        var gottaPop = false;
        if (iconColor.HasValue)
        {
            ImGui.PushStyleColor(Text, (Vector4)iconColor);
            gottaPop = true;
        }
        ImGui.PushFont(UiBuilder.IconFont);
        var str = icon.ToIconString();
        var adjust = (14 - ImGui.CalcTextSize(str).X) / 2;

        ImGui.SetCursorPosX(ImGui.GetCursorPosX() + adjust);
        ImGui.Text(icon.ToIconString());
        ImGui.PopFont();
        if (gottaPop) ImGui.PopStyleColor();
        if (iconHoverText != null && ImGui.IsItemHovered()) Tooltip(iconHoverText);
        ImGui.SameLine();
        ImGui.SetCursorPosX(ImGui.GetCursorPosX() + adjust);

    }

    // ReSharper disable once UnusedMember.Global
    public static void WriteIconText(FontAwesomeIcon icon, string text, string? iconHoverText = null, ColorRGB? iconColor = null, ColorRGB? textColor = null)
    {
        var gottaPop = false;
        if (iconColor.HasValue)
        {
            ImGui.PushStyleColor(Text, (Vector4)iconColor);
            gottaPop = true;
        }

        ImGui.PushFont(UiBuilder.IconFont);

        var str = icon.ToIconString();
        var adjust = (14 - ImGui.CalcTextSize(str).X)/2;
        ImGui.SetCursorPosX(ImGui.GetCursorPosX() + adjust);

        ImGui.Text(str);
        ImGui.PopFont();
        if (gottaPop) { ImGui.PopStyleColor(); gottaPop = false; }

        if (textColor.HasValue)
        {
            ImGui.PushStyleColor(Text, (Vector4)textColor);
            gottaPop = true;
        }
        if (iconHoverText != null && ImGui.IsItemHovered()) Tooltip(iconHoverText);

        ImGui.SameLine();
        ImGui.SetCursorPosX(ImGui.GetCursorPosX() + adjust);
        ImGui.Text(text);

        if (gottaPop) { ImGui.PopStyleColor(); }
    }

    public static void Tooltip(string tooltipText)
    {
        ImGui.BeginTooltip();
        ImGui.PushTextWrapPos(ImGui.GetFontSize() * 35f);
        ImGui.TextUnformatted(tooltipText);
        ImGui.PopTextWrapPos();
        ImGui.EndTooltip();
    }

    public static bool IconButtonWithText(string text, FontAwesomeIcon icon, string id, float? width = null, ColorRGB? color = null)
    {
        width *= GlobalScale;
        var pushed = 0;
        if (color.HasValue)
        {
            pushed++;
            ImGui.PushStyleColor(Button, (Vector4)color);
        }
        ImGui.PushID(id);
        ImGui.PushFont(UiBuilder.IconFont);

        var iconStr = icon.ToIconString();
        var iconStrSize = ImGui.CalcTextSize(iconStr);
        var adjust = ((16 * GlobalScale) - iconStrSize.X) / 2;

        ImGui.PopFont();

        var textSize = ImGui.CalcTextSize(text);
        var windowDrawList = ImGui.GetWindowDrawList();
        var cursorScreenPos = ImGui.GetCursorScreenPos();
        var num = 3f * GlobalScale;

        var x = Math.Max(width ?? 0, (float)(iconStrSize.X + (double)textSize.X + (ImGui.GetStyle().FramePadding.X * 2.0)) + num);
        var textAdjust = (x - textSize.X - iconStrSize.X) / 2;

        var frameHeight = ImGui.GetFrameHeight();
        var button = ImGui.Button(string.Empty, new(x, frameHeight));
        var pos1 = new Vector2(cursorScreenPos.X + ImGui.GetStyle().FramePadding.X + adjust, cursorScreenPos.Y + ImGui.GetStyle().FramePadding.Y);
        ImGui.PushFont(UiBuilder.IconFont);
        windowDrawList.AddText(pos1, ImGui.GetColorU32(Text), iconStr);
        ImGui.PopFont();
        var pos2 = new Vector2(pos1.X + iconStrSize.X + num + textAdjust - 8f, cursorScreenPos.Y + ImGui.GetStyle().FramePadding.Y);
        windowDrawList.AddText(pos2, ImGui.GetColorU32(Text), text);
        ImGui.PopID();

        if (pushed > 0) ImGui.PopStyleColor(pushed);
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

    public static void TextRightAligned(string text, Vector4 color)
    {
        var w = ImGui.CalcTextSize(text).X;
        var space = ImGui.GetColumnWidth();
        ImGui.SetCursorPosX(ImGui.GetCursorPosX() + (space - w));

        ImGui.TextColored(color, text);
    }

    public static void DrawGameIcon(uint trackerGameIcon, float height, bool active=true)
    {
        var startPos = ImGui.GetCursorPosX();

        TextureProvider.GetFromGameIcon(new(trackerGameIcon)).TryGetWrap(out var tex, out _);

        if (tex != null)
        {
            var adjustedHeight = height * GlobalScale;
            var width = tex.Width / (float)tex.Height * adjustedHeight;

            var margin = (adjustedHeight - width) / 2f;

            ImGui.SetCursorPosX(ImGui.GetCursorPosX() + margin);
            ImGui.Image(tex.ImGuiHandle, new(width, adjustedHeight),new(0),new(1),new(1,1,1,active?1:0.3f));
            ImGui.SameLine();
        }

        ImGui.SetCursorPosX(startPos+(30*GlobalScale));
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
