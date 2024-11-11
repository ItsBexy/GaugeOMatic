using System;
using System.Numerics;
using Dalamud.Interface;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;

using ImGuiNET;

namespace GaugeOMatic.Utility.DalamudComponents;

/// <summary>
/// Class containing various methods providing ImGui components.
/// </summary>
public static partial class ImGuiComponents
{
    /// <summary>
    /// IconButton component to use an icon as a button.
    /// </summary>
    /// <param name="icon">The icon for the button.</param>
   /// <returns>Indicator if button is clicked.</returns>
    public static bool IconButton(FontAwesomeIcon icon) => IconButton(icon, null);

    /// <summary>
    /// IconButton component to use an icon as a button.
    /// </summary>
    /// <param name="icon">The icon for the button.</param>
    /// <param name="size">Sets the size of the button. If either dimension is set to 0, that dimension will conform to the size of the icon.</param>
    /// <returns>Indicator if button is clicked.</returns>
    public static bool IconButton(FontAwesomeIcon icon, Vector2? size)
        => IconButton(icon, null, null, null, size);

    /// <summary>
    /// IconButton component to use an icon as a button.
    /// </summary>
    /// <param name="id">The ID of the button.</param>
    /// <param name="icon">The icon for the button.</param>
    /// <param name="size">Sets the size of the button. If either dimension is set to 0, that dimension will conform to the size of the icon.</param>
    /// <returns>Indicator if button is clicked.</returns>
    public static bool IconButton(int id, FontAwesomeIcon icon, Vector2? size = null)
        => IconButton(id, icon, null, null, null, size);

    /// <summary>
    /// IconButton component to use an icon as a button.
    /// </summary>
    /// <param name="id">The ID of the button.</param>
    /// <param name="icon">The icon for the button.</param>
    /// <returns>Indicator if button is clicked.</returns>
    public static bool IconButton(string id, FontAwesomeIcon icon) => IconButton(id, icon, null);

    /// <summary>
    /// IconButton component to use an icon as a button.
    /// </summary>
    /// <param name="id">The ID of the button.</param>
    /// <param name="icon">The icon for the button.</param>
    /// <param name="size">Sets the size of the button. If either dimension is set to 0, that dimension will conform to the size of the icon.</param>
    /// <returns>Indicator if button is clicked.</returns>
    public static bool IconButton(string id, FontAwesomeIcon icon, Vector2? size)
        => IconButton(id, icon, null, null, null, size);

    /// <summary>
    /// IconButton component to use an icon as a button.
    /// </summary>
    /// <param name="iconText">Text already containing the icon string.</param>
    /// <param name="size">Sets the size of the button. If either dimension is set to 0, that dimension will conform to the size of the icon.</param>
    /// <returns>Indicator if button is clicked.</returns>
    public static bool IconButton(string iconText, Vector2? size = null)
        => IconButton(iconText, null, null, null, size);

    /// <summary>
    /// IconButton component to use an icon as a button.
    /// </summary>
    /// <param name="icon">The icon for the button.</param>
    /// <param name="defaultColor">The default color of the button.</param>
    /// <param name="activeColor">The color of the button when active.</param>
    /// <param name="hoveredColor">The color of the button when hovered.</param>
    /// <param name="size">Sets the size of the button. If either dimension is set to 0, that dimension will conform to the size of the icon.</param>
    /// <returns>Indicator if button is clicked.</returns>
    public static bool IconButton(FontAwesomeIcon icon, Vector4? defaultColor, Vector4? activeColor = null, Vector4? hoveredColor = null, Vector2? size = null)
        => IconButton($"{icon.ToIconString()}", defaultColor, activeColor, hoveredColor, size);

    /// <summary>
    /// IconButton component to use an icon as a button with color options.
    /// </summary>
    /// <param name="id">The ID of the button.</param>
    /// <param name="icon">The icon for the button.</param>
    /// <param name="defaultColor">The default color of the button.</param>
    /// <param name="activeColor">The color of the button when active.</param>
    /// <param name="hoveredColor">The color of the button when hovered.</param>
    /// <param name="size">Sets the size of the button. If either dimension is set to 0, that dimension will conform to the size of the icon.</param>
    /// <returns>Indicator if button is clicked.</returns>
    public static bool IconButton(int id, FontAwesomeIcon icon, Vector4? defaultColor = null, Vector4? activeColor = null, Vector4? hoveredColor = null, Vector2? size = null)
        => IconButton($"{icon.ToIconString()}##{id}", defaultColor, activeColor, hoveredColor, size);

    /// <summary>
    /// IconButton component to use an icon as a button with color options.
    /// </summary>
    /// <param name="id">The ID of the button.</param>
    /// <param name="icon">The icon for the button.</param>
    /// <param name="defaultColor">The default color of the button.</param>
    /// <param name="activeColor">The color of the button when active.</param>
    /// <param name="hoveredColor">The color of the button when hovered.</param>
    /// <param name="size">Sets the size of the button. If either dimension is set to 0, that dimension will conform to the size of the icon.</param>
    /// <returns>Indicator if button is clicked.</returns>
    public static bool IconButton(string id, FontAwesomeIcon icon, Vector4? defaultColor, Vector4? activeColor = null, Vector4? hoveredColor = null, Vector2? size = null)
        => IconButton($"{icon.ToIconString()}##{id}", defaultColor, activeColor, hoveredColor, size);

    /// <summary>
    /// IconButton component to use an icon as a button with color options.
    /// </summary>
    /// <param name="iconText">Text already containing the icon string.</param>
    /// <param name="defaultColor">The default color of the button.</param>
    /// <param name="activeColor">The color of the button when active.</param>
    /// <param name="hoveredColor">The color of the button when hovered.</param>
    /// <param name="size">Sets the size of the button. If either dimension is set to 0, that dimension will conform to the size of the icon.</param>
    /// <returns>Indicator if button is clicked.</returns>
    public static bool IconButton(string iconText, Vector4? defaultColor = null, Vector4? activeColor = null, Vector4? hoveredColor = null, Vector2? size = null)
    {
        using var col = new ImRaii.Color();

        if (defaultColor.HasValue)
        {
            col.Push(ImGuiCol.Button, defaultColor.Value);
        }

        if (activeColor.HasValue)
        {
            col.Push(ImGuiCol.ButtonActive, activeColor.Value);
        }

        if (hoveredColor.HasValue)
        {
            col.Push(ImGuiCol.ButtonHovered, hoveredColor.Value);
        }

        if (size.HasValue)
        {
            size *= ImGuiHelpers.GlobalScale;
        }

        var icon = iconText;
        if (icon.Contains("#"))
        {
            icon = icon[..icon.IndexOf("#", StringComparison.Ordinal)];
        }

        bool button;

        using (ImRaii.PushFont(UiBuilder.IconFont))
        {
            var iconSize = ImGui.CalcTextSize(icon);
            var cursor = ImGui.GetCursorScreenPos();

            // Draw a blank ImGuiButton

            var width = size is { X: not 0 } ? size.Value.X : iconSize.X + (ImGui.GetStyle().FramePadding.X * 2);
            var height = size is { Y: not 0 } ? size.Value.Y : ImGui.GetFrameHeight();

            var buttonSize = new Vector2(width, height);

            using (ImRaii.PushId(iconText))
            {
                button = ImGui.Button(string.Empty, buttonSize);
            }

            // Draw the icon over the button

            var iconPos = cursor + ((buttonSize - iconSize) / 2f);

            ImGui.GetWindowDrawList().AddText(iconPos, ImGui.GetColorU32(ImGuiCol.Text), icon);
        }

        return button;
    }

    /// <summary>
    /// IconButton component to use an icon as a button with color options.
    /// </summary>
    /// <param name="icon">Icon to show.</param>
    /// <param name="text">Text to show.</param>
    /// <param name="defaultColor">The default color of the button.</param>
    /// <param name="activeColor">The color of the button when active.</param>
    /// <param name="hoveredColor">The color of the button when hovered.</param>
    /// <param name="size">Sets the size of the button. If either dimension is set to 0, that dimension will conform to the size of the icon & text.</param>
    /// <returns>Indicator if button is clicked.</returns>
    public static bool IconButtonWithText(FontAwesomeIcon icon, string text, Vector4? defaultColor = null, Vector4? activeColor = null, Vector4? hoveredColor = null, Vector2? size = null)
    {
        using var col = new ImRaii.Color();

        if (defaultColor.HasValue)
        {
            col.Push(ImGuiCol.Button, defaultColor.Value);
        }

        if (activeColor.HasValue)
        {
            col.Push(ImGuiCol.ButtonActive, activeColor.Value);
        }

        if (hoveredColor.HasValue)
        {
            col.Push(ImGuiCol.ButtonHovered, hoveredColor.Value);
        }

        if (size.HasValue)
        {
            size *= ImGuiHelpers.GlobalScale;
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
        var iconPadding = 3 * ImGuiHelpers.GlobalScale;

        var cursor = ImGui.GetCursorScreenPos();

        using (ImRaii.PushId(text))
        {
            var textSize = ImGui.CalcTextSize(textStr);
            // Draw a blank ImGui button
            var width = size is { X: not 0 } ? size.Value.X : iconSize.X + textSize.X + (framePadding.X * 2) + iconPadding;
            var height = size is { Y: not 0 } ? size.Value.Y : ImGui.GetFrameHeight();

            button = ImGui.Button(string.Empty, new Vector2(width, height));
        }

        var iconPos = cursor + framePadding;
        var textPos = new Vector2(iconPos.X + iconSize.X + iconPadding, cursor.Y + framePadding.Y);

        var dl = ImGui.GetWindowDrawList();

        // Draw the icon on the window drawlist
        using (ImRaii.PushFont(UiBuilder.IconFont))
        {
            dl.AddText(iconPos, ImGui.GetColorU32(ImGuiCol.Text), icon.ToIconString());
        }

        // Draw the text on the window drawlist
        dl.AddText(textPos, ImGui.GetColorU32(ImGuiCol.Text), textStr);

        return button;
    }

    /// <summary>
    /// Get width of IconButtonWithText component.
    /// </summary>
    /// <param name="icon">Icon to use.</param>
    /// <param name="text">Text to use.</param>
    /// <returns>Width.</returns>
    internal static float GetIconButtonWithTextWidth(FontAwesomeIcon icon, string text)
    {
        using (ImRaii.PushFont(UiBuilder.IconFont))
        {
            var iconSize = ImGui.CalcTextSize(icon.ToIconString());

            var textSize = ImGui.CalcTextSize(text);

            var iconPadding = 3 * ImGuiHelpers.GlobalScale;

            return iconSize.X + textSize.X + (ImGui.GetStyle().FramePadding.X * 2) + iconPadding;
        }
    }
}
