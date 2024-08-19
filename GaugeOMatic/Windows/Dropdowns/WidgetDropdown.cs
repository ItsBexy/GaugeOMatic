using GaugeOMatic.Trackers;
using GaugeOMatic.Widgets;
using ImGuiNET;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using static GaugeOMatic.Widgets.WidgetAttribute;
using static GaugeOMatic.Widgets.WidgetTags;
using static GaugeOMatic.Widgets.WidgetUI;
using static GaugeOMatic.Widgets.WidgetUI.UpdateFlags;

namespace GaugeOMatic.Windows.Dropdowns;

public class WidgetDropdown : BranchingDropdown
{
    public Tracker Tracker;
    public int Hash => Tracker.GetHashCode();
    public Dictionary<string, WidgetAttribute> AvailableWidgets = new();

    public override List<(string label, WidgetTags tag)> SubMenus { get; }

    public WidgetDropdown(Tracker tracker)
    {
        Tracker = tracker;
        AvailableWidgets.Clear();

        foreach (var (widgetType, widgetInfo) in WidgetList)
        {
            var tags = widgetInfo.WidgetTags;

            if (tags.HasFlag(Exclude)) continue;
            if (tags.HasFlag(HasAddonRestrictions) && !Tracker.JobModule.AddonOptions.Any(a => widgetInfo.WhiteList.Contains(a.Name) || widgetInfo.BlackList.Contains(a.Name) == false)) continue;

            AvailableWidgets.Add(widgetType, widgetInfo);
        }

        SubMenus = new()
        {
            ("Counters", Counter),
            ("Bars & Timers", GaugeBar),
            ("State Indicators", State),
            ("Multi-Component", MultiComponent)
        };
    }

    public override void DrawSubMenu(int i)
    {
        var (label, tag) = SubMenus[i];

        if (tag == MultiComponent) MultiCompSubMenu(label);
        else
        {
            if (ImGui.BeginMenu($"{label}##{Hash}{label}Menu"))
            {
                var widgets = AvailableWidgets.Where(w => w.Value.WidgetTags.HasFlag(tag))
                                              .OrderBy(static w => w.Value.DisplayName);

                foreach (var w in widgets.Where(static w => ImGui.MenuItem(w.Value.DisplayName)))
                {
                    Tracker.WidgetType = w.Key;
                    UpdateFlag |= Reset | Save;
                }

                ImGui.EndMenu();
            }
        }
    }

    public void MultiCompSubMenu(string label)
    {
        if (!ImGui.BeginMenu($"{label}##{Hash}{label}Menu")) return;

        foreach (var (key, name) in MultiCompDict)
        {
            ImGui.PushStyleColor(ImGuiCol.Border, new Vector4(0, 0, 0, 0));
            if (ImGui.BeginMenu($"{name}##{Hash}{label}{key}Menu"))
            {
                foreach (var w in AvailableWidgets.Where(w => w.Value.WidgetTags.HasFlag(MultiComponent) && w.Value.MultiCompData?.Key == key)
                                                  .OrderBy(static w => w.Value.MultiCompData?.Index)
                                                  .Where(static w => ImGui.MenuItem(w.Value.DisplayName)))
                {
                    Tracker.WidgetType = w.Key;
                    UpdateFlag |= Reset | Save;
                }

                ImGui.EndMenu();
            }

            ImGui.PopStyleColor();
        }

        ImGui.EndMenu();
    }

    public override string DropdownText(string fallback) => WidgetList.TryGetValue(Tracker.WidgetType ?? "", out var attr) ? attr.DisplayName : fallback;
}
