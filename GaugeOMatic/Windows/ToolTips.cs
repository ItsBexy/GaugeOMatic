using Dalamud.Interface.Utility;
using ImGuiNET;
using System.Numerics;
using static GaugeOMatic.Utility.ImGuiHelpy;

namespace GaugeOMatic.Windows;

internal static class Tooltips
{
    public static void DrawTooltip(uint? icon, string heading, string? w1=null, string? w2 = null, string? w3 = null, string? footer = null)
    {
        ImGui.PushStyleColor(ImGuiCol.PopupBg, new Vector4(0.03f, 0.03f, 0.03f, 1));
        ImGui.BeginTooltip();

        var startPos = ImGui.GetCursorPos();

        DrawTooltipIcon(icon, startPos);
        ImGui.BeginGroup();
        ImGui.Text(heading);
        ImGui.EndGroup();

        ImGui.SetCursorPosY(startPos.Y + (50 * ImGuiHelpers.GlobalScale));

        if (w1 != null || w2 != null || w3 != null) WidgetBehaviorTable(w1, w2, w3);

        if (footer != null) ImGui.TextDisabled(footer);

        ImGui.EndTooltip();
        ImGui.PopStyleColor(1);
    }

    public static void WidgetBehaviorTable(string? barDesc, string? counterDesc, string? stateDesc)
    {
        ImGui.TextDisabled("Widget Behavior");

        ImGui.BeginTable("BehaviorTable", 2);

        ImGui.TableSetupColumn("Widget");
        ImGui.TableSetupColumn("Value");

        if (barDesc != null)
        {
            ImGui.TableNextRow();
            ImGui.TableNextColumn();
            ImGui.Text("Bar / Timer:");
            ImGui.TableNextColumn();
            ImGui.Text(barDesc);
        }

        if (counterDesc != null)
        {
            ImGui.TableNextRow();
            ImGui.TableNextColumn();
            ImGui.Text("Counter:");
            ImGui.TableNextColumn();
            ImGui.Text(counterDesc);
        }

        if (stateDesc != null)
        {
            ImGui.TableNextRow();
            ImGui.TableNextColumn();
            ImGui.Text("State Indicator:");
            ImGui.TableNextColumn();
            ImGui.Text(stateDesc);
        }

        ImGui.EndTable();
    }

    private static void DrawTooltipIcon(uint? iconId, Vector2 startPos)
    {
        var texture = GetGameIconTexture(iconId);

        if (texture != null) ImGui.Image(texture.ImGuiHandle, new Vector2(40 * ImGuiHelpers.GlobalScale));

        ImGui.SetCursorPos(new(startPos.X + (50 * ImGuiHelpers.GlobalScale), startPos.Y + (2 * ImGuiHelpers.GlobalScale)));
    }
}
