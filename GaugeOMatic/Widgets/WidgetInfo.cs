using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using static GaugeOMatic.GameData.JobData;

namespace GaugeOMatic.Widgets;

public class WidgetInfo
{
    public string DisplayName { get; init; } = null!;
    public string? Author { get;  init; }
    public string? Description { get; init; }
    public WidgetTags WidgetTags { get; init; }
    public string? KeyText { get; init; }

    // ReSharper disable once NotAccessedField.Global
    public int? FixedCount;           // only applicable if tagged with HasFixedMaximum
    public List<string>? AllowedAddons; // only applicable if tagged with HasAddonRestrictions
    // ReSharper disable once CollectionNeverUpdated.Global
    // ReSharper disable once UnassignedField.Global
    public List<Job>? AllowedJobs;

    internal static Dictionary<string, WidgetInfo> WidgetList = new();

    public static void BuildWidgetList()
    {
        GaugeOMatic.Service.Log.Info("Generating list of Available Widgets");

        var types = Assembly.GetExecutingAssembly()
                            .GetTypes()
                            .Where(static t => t.IsSubclassOf(typeof(Widget)) && !t.IsAbstract)
                            .OrderBy(static t => t.Name);

        foreach (var type in types)
        {
            var widgetInfo = (WidgetInfo?)type.GetProperty("GetWidgetInfo")?.GetValue(null);

            if (widgetInfo == null) continue;

            WidgetList.Add(type.Name,widgetInfo);

            GaugeOMatic.Service.Log.Info($"Added Widget Option: {widgetInfo.DisplayName}");
        }
    }
}

[Flags]
public enum WidgetTags
{
    // format tags
    Counter              = 0x1,
    GaugeBar             = 0x2,
    State                = 0x4,

    // restriction tags
    HasFixedCount        = 0x8,   // is only designed to display up to a specific maximum number value
    HasJobRestrictions   = 0x10,  // can only be used by certain jobs
    HasAddonRestrictions = 0x20,  // is only designed to appear on certain HUD elements

    // ReSharper disable once UnusedMember.Global
    Replica              = 0x40,  // Designed to recreate (in full or in part) the appearance of an existing job gauge
    MultiComponent       = 0x80,

    Exclude              = 0x1000 // never show this Widget as an option. for WIP widgets
}
