using Dalamud.Interface;
using FFXIVClientStructs.FFXIV.Component.GUI;
using ImGuiNET;
using Newtonsoft.Json;
using static Dalamud.Interface.FontAwesomeIcon;
using static GaugeOMatic.GaugeOMatic;
using static GaugeOMatic.Utility.Color;

namespace GaugeOMatic.JobModules;

internal static class Tweaks
{
    public static unsafe void VisibilityTweak(bool hide, bool simple, AtkResNode* standardNode, AtkResNode* simpleNode)
    {
        if (standardNode != null) standardNode->SetAlpha((byte)(hide || simple ? 0 : 255));
        if (simpleNode != null) simpleNode->SetAlpha((byte)(hide || !simple ? 0 : 255));
    }

    // ReSharper disable once UnusedType.Global
    public static class TweakUI
    {
        // ReSharper disable once UnusedMember.Global
        public static void Warning(bool highlight, string helpText)
        {
            ImGui.SameLine();
            ImGui.PushFont(UiBuilder.IconFont);
            ImGui.TextColored(highlight ? new(0.9f, 0.71f, 0, 1) : new(1, 1, 1, 0.3f), ExclamationTriangle.ToIconString());
            ImGui.PopFont();
            if (ImGui.IsItemHovered())
            {
                ImGui.BeginTooltip();
                ImGui.PushTextWrapPos(ImGui.GetFontSize() * 35f);
                ImGui.TextUnformatted(helpText);
                ImGui.PopTextWrapPos();
                ImGui.EndTooltip();
            }
        }
    }

}

public partial class TweakConfigs
{
    [JsonIgnore] public bool Preview = false;
    [JsonIgnore] public bool ShowPreviews => Preview && ConfigWindow.IsOpen;
    [JsonIgnore] public AddRGB? TestColor;
}
