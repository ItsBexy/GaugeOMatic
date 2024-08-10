using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using GaugeOMatic.Trackers;
using GaugeOMatic.Widgets;
using ImGuiNET;

namespace GaugeOMatic.Windows.Dropdowns;

public class WidgetDropdown : BranchingDropdown
{
    public Tracker Tracker;
    public int Hash => Tracker.GetHashCode();
    public Dictionary<string, WidgetInfo> AvailableWidgets = new();

    public override List<(string label, WidgetTags tag)> SubMenus { get; }

    public WidgetDropdown(Tracker tracker)
    {
        Tracker = tracker;
        var currentData = Tracker.GetCurrentData();
        AvailableWidgets.Clear();

        foreach (var (widgetType, widgetInfo) in WidgetInfo.WidgetList)
        {
            var tags = widgetInfo.WidgetTags;

            if (tags.HasFlag(WidgetTags.Exclude)) continue;
            if (tags.HasFlag(WidgetTags.HasFixedCount) && currentData.MaxCount != widgetInfo.FixedCount) continue;
            if (tags.HasFlag(WidgetTags.HasAddonRestrictions) && !Tracker.JobModule.AddonOptions.Any(a => widgetInfo.AllowedAddons?.Contains(a.Name) == true || widgetInfo.RestrictedAddons?.Contains(a.Name) == false)) continue;

            AvailableWidgets.Add(widgetType, widgetInfo);
        }

        SubMenus = new()
        {
            ("Counters", WidgetTags.Counter),
            ("Bars & Timers", WidgetTags.GaugeBar),
            ("State Indicators", WidgetTags.State),
            ("Multi-Component", WidgetTags.MultiComponent)
        };
    }

    public override void DrawSubMenu(int i, ref Tracker.UpdateFlags update)
    {
        var (label, tag) = SubMenus[i];

        if (tag == WidgetTags.MultiComponent) MultiCompSubMenu(label, tag, ref update);
        else
        {
            if (ImGui.BeginMenu($"{label}##{Hash}{label}Menu"))
            {
                var widgets = AvailableWidgets.Where(w => w.Value.WidgetTags.HasFlag(tag))
                                              .OrderBy(static w => w.Value.DisplayName);

                foreach (var w in widgets.Where(static w => ImGui.MenuItem(w.Value.DisplayName)))
                {
                    Tracker.WidgetType = w.Key;
                    update |= Tracker.UpdateFlags.Reset | Tracker.UpdateFlags.Save;
                }

                ImGui.EndMenu();
            }
        }
    }

    public void MultiCompSubMenu(string label, WidgetTags tag, ref Tracker.UpdateFlags update)
    {
        if (ImGui.BeginMenu($"{label}##{Hash}{label}Menu"))
        {
            var mcWidgets = AvailableWidgets.Where(w => w.Value.WidgetTags.HasFlag(tag)).ToArray();
            foreach (var (key, name) in WidgetInfo.MultiCompDict)
            {
                ImGui.PushStyleColor(ImGuiCol.Border, new Vector4(0, 0, 0, 0));
                if (ImGui.BeginMenu($"{name}##{Hash}{label}{key}Menu"))
                {
                    foreach (var w in mcWidgets.Where(w => w.Value.MultiCompData?.Key == key)
                                               .OrderBy(static w => w.Value.MultiCompData?.Index)
                                               .Where(static w => ImGui.MenuItem(w.Value.DisplayName)))
                    {
                        Tracker.WidgetType = w.Key;
                        update |= Tracker.UpdateFlags.Reset | Tracker.UpdateFlags.Save;
                    }

                    ImGui.EndMenu();
                }

                ImGui.PopStyleColor();
            }

            ImGui.EndMenu();
        }
    }

    public override string DropdownText(string fallback) =>
        Tracker.WidgetType != null && WidgetInfo.WidgetList.TryGetValue(Tracker.WidgetType, out var wType)
            ? wType.DisplayName
            : fallback;
}
