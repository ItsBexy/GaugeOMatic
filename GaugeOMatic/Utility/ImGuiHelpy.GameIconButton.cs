using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using ImGuiNET;
// ReSharper disable UnusedMember.Global

namespace GaugeOMatic.Utility;

public static partial class ImGuiHelpy
{
    /// <summary>
    /// A radio-like input that uses icon buttons.
    /// </summary>
    /// <typeparam name="T">The type of the value being set.</typeparam>
    /// <param name="label">Text that will be used to generate individual labels for the buttons.</param>
    /// <param name="val">The value to set.</param>
    /// <param name="options">A list of all icon/option pairs.</param>
    /// <param name="columns">Arranges the buttons in a grid with the given number of columns. 0 = ignored (all buttons drawn in one row).</param>
    /// <param name="iconSize">The size of each icon.</param>
    /// <param name="buttonSize">Sets the size of all buttons. If left null, will conform to the size of the icon.</param>
    /// <param name="defaultColor">The default color of the button range.</param>
    /// <param name="activeColor">The color of the actively-selected button.</param>
    /// <param name="hoveredColor">The color of the buttons when hovered.</param>
    /// <returns>True if any button is clicked.</returns>
    internal static unsafe bool GameIconButtonSelect<T>(
        string label, ref T val, IEnumerable<KeyValuePair<uint, T>> options, Vector2 iconSize,
        Vector2? buttonSize = null, uint columns = 0, Vector4? defaultColor = null, Vector4? activeColor = null,
        Vector4? hoveredColor = null)
    {
        defaultColor ??= *ImGui.GetStyleColorVec4(ImGuiCol.Button);
        activeColor ??= *ImGui.GetStyleColorVec4(ImGuiCol.ButtonActive);
        hoveredColor ??= *ImGui.GetStyleColorVec4(ImGuiCol.ButtonHovered);
        buttonSize ??= iconSize + ImGui.GetStyle().FramePadding;

        var result = false;

        var innerSpacing = ImGui.GetStyle().ItemInnerSpacing;

        var y = ImGui.GetCursorPosY();

        var optArr = options.ToArray();
        for (var i = 0; i < optArr.Length; i++)
        {
            if (i > 0)
            {
                if (columns == 0 || i % columns != 0)
                {
                    ImGui.SameLine(0, innerSpacing.X);
                }
                else
                {
                    y += (buttonSize.Value.Y * ImGuiHelpers.GlobalScale) + innerSpacing.Y;

                    ImGui.SetCursorPosY(y);
                }
            }

            optArr[i].Deconstruct(out var icon, out var option);

            var selected = val is not null && val.Equals(option);

            if (GameIconButton(icon, (string)$"{label}{option}{i}", iconSize, buttonSize.Value,
                               selected ? activeColor : defaultColor, activeColor, hoveredColor))
            {
                val = option;
                result = true;
            }
        }

        return result;
    }

    /// <summary>
    /// A radio-like input that uses icon buttons.
    /// </summary>
    /// <typeparam name="T">The type of the value being set.</typeparam>
    /// <param name="label">Text that will be used to generate individual labels for the buttons.</param>
    /// <param name="val">The value to set.</param>
    /// <param name="optionIcons">The game icons that will be displayed on each button.</param>
    /// <param name="optionValues">The options that each button will apply.</param>
    /// <param name="columns">Arranges the buttons in a grid with the given number of columns. 0 = ignored (all buttons drawn in one row).</param>
    /// <param name="iconSize">The size of each icon.</param>
    /// <param name="buttonSize">Sets the size of all buttons. If left null, will conform to the size of the icon.</param>
    /// <param name="defaultColor">The default color of the button range.</param>
    /// <param name="activeColor">The color of the actively-selected button.</param>
    /// <param name="hoveredColor">The color of the buttons when hovered.</param>
    /// <returns>True if any button is clicked.</returns>
    internal static bool GameIconButtonSelect<T>(
        string label, ref T val, IEnumerable<uint> optionIcons, IEnumerable<T> optionValues, Vector2 iconSize,
        Vector2? buttonSize = null, uint columns = 0, Vector4? defaultColor = null, Vector4? activeColor = null,
        Vector4? hoveredColor = null)
    {
        var options = optionIcons.Zip(optionValues, static (icon, value) => new KeyValuePair<uint, T>(icon, value));
        return GameIconButtonSelect(label, ref val, options, iconSize, buttonSize, columns, defaultColor, activeColor,
                                    hoveredColor);
    }

    /// <summary>
    /// Component to use a Game Icon as a button.
    /// </summary>
    /// <param name="gameIcon">The id of the in-game icon.</param>
    /// <param name="buttonId">The id/label to use for the ImGui button.</param>
    /// <param name="iconSize">The size of the icon, scaled by the Dalamud global scale.</param>
    /// <param name="buttonSize">The size of the ImGui button, scaled by the Dalamud global scale.</param>
    /// <param name="defaultColor">The default color of the button.</param>
    /// <param name="activeColor">The color of the button when active.</param>
    /// <param name="hoveredColor">The color of the button when hovered.</param>
    /// <returns></returns>
    public static bool GameIconButton(
        uint gameIcon,
        string buttonId,
        Vector2 iconSize,
        Vector2 buttonSize,
        Vector4? defaultColor = null,
        Vector4? activeColor = null,
        Vector4? hoveredColor = null)
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

        iconSize *= ImGuiHelpers.GlobalScale;
        buttonSize *= ImGuiHelpers.GlobalScale;

        bool button;
        var cursor = ImGui.GetCursorScreenPos();
        using (ImRaii.PushId(buttonId))
        {
            button = ImGui.Button(string.Empty, buttonSize);
        }

        TextureProvider.GetFromGameIcon(new(gameIcon)).TryGetWrap(out var tex, out _);

        if (tex != null)
        {
            var iconPos = cursor + ((buttonSize - iconSize) / 2);
            ImGui.GetWindowDrawList().AddImage(tex.ImGuiHandle, iconPos, iconPos + iconSize);
        }

        return button;
    }

    /// <summary>
    /// Component to use a Game Icon as a button.
    /// </summary>
    /// <param name="gameIcon">The id of the in-game icon.</param>
    /// <param name="buttonId">The id/label to use for the ImGui button.</param>
    /// <param name="iconSize">The size of the icon, scaled by the Dalamud global scale.</param>
    /// <param name="defaultColor">The default color of the button.</param>
    /// <param name="activeColor">The color of the button when active.</param>
    /// <param name="hoveredColor">The color of the button when hovered.</param>
    /// <returns></returns>
    public static bool GameIconButton(
        uint gameIcon, string buttonId, Vector2 iconSize, Vector4? defaultColor = null, Vector4? activeColor = null,
        Vector4? hoveredColor = null) =>
        GameIconButton(gameIcon, buttonId, iconSize,
                       iconSize + ImGui.GetStyle().FramePadding, defaultColor,
                       activeColor, hoveredColor);
}
