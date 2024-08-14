using GaugeOMatic.GameData;
using GaugeOMatic.Utility;
using ImGuiNET;
using System;
using System.Numerics;
using static Dalamud.Interface.Utility.ImGuiHelpers;
using static GaugeOMatic.GameData.JobData;

namespace GaugeOMatic.Trackers;

[AttributeUsage(AttributeTargets.Class)]
public class TrackerDisplayAttribute : Attribute
{
    public string Name = "[Empty Tracker]";
    public Job Job = Job.None;
    public Role Role = Role.None;
    public uint GameIcon = 60071;
    public string? BarDesc;
    public string? CounterDesc;
    public string? StateDesc;
    public string? Footer;

    public TrackerDisplayAttribute() { }

    public TrackerDisplayAttribute(string name, Job job, uint gameIcon, string? barDesc = null, string? counterDesc = null, string? stateDesc = null, string? footer = null)
    {
        GameIcon = gameIcon;
        Job = job;
        Name = name;

        BarDesc = barDesc;
        CounterDesc = counterDesc;
        StateDesc = stateDesc;
        Footer = footer;
    }

    public TrackerDisplayAttribute(string name, Job job, string? barDesc = null, string? counterDesc = null, string? stateDesc = null, string? footer = null) :
        this(name, job, GetJobIcon(job), barDesc, counterDesc, stateDesc, footer) { }

    public TrackerDisplayAttribute(ItemRef i)
    {
        Name = i.Name;
        Job = i.Job;
        Role = i.Role;
        GameIcon = i.Icon ?? GameIcon;
    }

    public TrackerDisplayAttribute(StatusRef i) : this((ItemRef)i) { }

    public TrackerDisplayAttribute(ActionRef aRef)
    {
        Name = aRef.GetAdjustedName();
        Job = aRef.Job;
        Role = aRef.Role;
        GameIcon = aRef.GetAdjustedIcon() ?? GameIcon;
    }

    public static implicit operator TrackerDisplayAttribute(ItemRef i) => new(i);
    public static implicit operator TrackerDisplayAttribute(ActionRef a) => new(a);

    public void DrawTooltip() => DrawTooltip(GameIcon, Name, BarDesc, CounterDesc, StateDesc, Footer);

    public void DrawTooltip(string trackerType, uint itemId)
    {
        switch (trackerType)
        {
            case nameof(ActionTracker):
                ((ActionRef)itemId).DrawTooltip();
                break;
            case nameof(StatusTracker):
                ((StatusRef)itemId).DrawTooltip();
                break;
            default:
                DrawTooltip();
                break;
        }
    }

    public static void DrawTooltip(uint? icon, string heading, string? w1=null, string? w2 = null, string? w3 = null, string? footer = null)
    {
        ImGui.PushStyleColor(ImGuiCol.PopupBg, new Vector4(0.03f, 0.03f, 0.03f, 1));
        ImGui.BeginTooltip();

        var startPos = ImGui.GetCursorPos();

        DrawTooltipIcon(icon, startPos);
        ImGui.BeginGroup();
        ImGui.Text(heading);
        ImGui.EndGroup();

        ImGui.SetCursorPosY(startPos.Y + (50 * GlobalScale));

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
        var texture = ImGuiHelpy.GetGameIconTexture(iconId);

        if (texture != null) ImGui.Image(texture.ImGuiHandle, new Vector2(40 * GlobalScale));

        ImGui.SetCursorPos(new(startPos.X + (50 * GlobalScale), startPos.Y + (2 * GlobalScale)));
    }
}
