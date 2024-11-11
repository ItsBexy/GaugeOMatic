using Dalamud.Interface;
using Dalamud.Interface.Utility.Raii;

using ImGuiNET;

namespace GaugeOMatic.Utility.DalamudComponents;

/// <summary>
/// Class containing various methods providing ImGui components.
/// </summary>
public static partial class ImGuiComponents
{
    /// <summary>
    /// HelpMarker component to add a help icon with text on hover.
    /// </summary>
    /// <param name="helpText">The text to display on hover.</param>
    public static void HelpMarker(string helpText) => HelpMarker(helpText, FontAwesomeIcon.InfoCircle);

    /// <summary>
    /// HelpMarker component to add a custom icon with text on hover.
    /// </summary>
    /// <param name="helpText">The text to display on hover.</param>
    /// <param name="icon">The icon to use.</param>
    public static void HelpMarker(string helpText, FontAwesomeIcon icon)
    {
        ImGui.SameLine();

        using (ImRaii.PushFont(UiBuilder.IconFont))
        {
            ImGui.TextDisabled(icon.ToIconString());
        }

        if (ImGui.IsItemHovered())
        {
            using (ImRaii.Tooltip())
            {
                using (ImRaii.TextWrapPos(ImGui.GetFontSize() * 35.0f))
                {
                    ImGui.TextUnformatted(helpText);
                }
            }
        }
    }
}
