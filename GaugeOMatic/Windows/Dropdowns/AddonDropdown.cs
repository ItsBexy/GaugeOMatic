using GaugeOMatic.Trackers;
using System.Collections.Generic;

namespace GaugeOMatic.Windows.Dropdowns;

public struct AddonOption(string name, string displayName)
{
    public string Name = name;
    public string DisplayName = displayName;
}

public class AddonDropdown : Dropdown<string>
{
    public AddonDropdown(Tracker tracker)
    {
        Tracker = tracker;
        Prepare(Tracker.JobModule.AddonOptions);
    }

    public sealed override List<string> Values { get; } = [];
    public sealed override List<string> DisplayNames { get; } = [];
    public Tracker Tracker;

    public void Prepare(List<AddonOption> addonOptions)
    {
        Values.Clear();
        DisplayNames.Clear();

        var widgetInfo = Tracker.Widget?.GetAttributes;

        var whiteList = widgetInfo?.WhiteList;
        var blackList = widgetInfo?.BlackList;
        foreach (var option in addonOptions)
        {
            if (whiteList is { Count: > 0 } && !whiteList.Contains(option.Name)) continue;
            if (blackList is { Count: > 0 } && blackList.Contains(option.Name)) continue;
            Values.Add(option.Name);
            DisplayNames.Add(option.DisplayName);
        }

        Index = Values.IndexOf(Tracker.TrackerConfig.AddonName);
    }
}
