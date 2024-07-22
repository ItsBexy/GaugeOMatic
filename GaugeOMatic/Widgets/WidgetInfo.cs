using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using static GaugeOMatic.Widgets.WidgetTags;

namespace GaugeOMatic.Widgets;

public class WidgetInfo
{
    public string DisplayName { get; init; } = null!;
    public string? Author { get;  init; }
    public string? Description { get; init; }
    public WidgetTags WidgetTags { get; init; }
    public MultiComponentData? MultiCompData;

    public struct MultiComponentData
    {
        public string Key;
        public int Index;

        public MultiComponentData(string key, string groupName, int index)
        {
            Key = key;
            Index = index;
            if (!MultiCompDict.ContainsKey(key)) MultiCompDict.Add(key, groupName);
        }
    }

    internal static Dictionary<string, string> MultiCompDict = new();

    public int? FixedCount;                // only applicable if tagged with HasFixedMaximum
    public List<string>? AllowedAddons;    // only applicable if tagged with HasAddonRestrictions
    public List<string>? RestrictedAddons; // only applicable if tagged with HasAddonRestrictions

    public static List<string> ClipConflictAddons => new() { "JobHudRPM1", "JobHudGFF1", "JobHudSMN1", "JobHudBRD0" };

    public bool AddonPermitted(string aName) => !WidgetTags.HasFlag(HasAddonRestrictions) || (RestrictedAddons?.Contains(aName) != true && AllowedAddons?.Contains(aName) != false);

    internal static Dictionary<string, WidgetInfo> WidgetList = new();

    public static void BuildWidgetList()
    {
        Log.Verbose("Generating list of Available Widgets");

        var types = Assembly.GetExecutingAssembly()
                            .GetTypes()
                            .Where(static t => t.IsSubclassOf(typeof(Widget)) && !t.IsAbstract)
                            .OrderBy(static t => t.Name);

        foreach (var type in types)
        {
            var widgetInfo = (WidgetInfo?)type.GetProperty("GetWidgetInfo")?.GetValue(null);
            if (widgetInfo == null) continue;

            WidgetList.Add(type.Name, widgetInfo);
            Log.Verbose($"Added Widget Option: {widgetInfo.DisplayName}");
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
    HasFixedCount        = 0x8,  // is only designed to display up to a specific maximum number value
    // ReSharper disable once UnusedMember.Global
    HasJobRestrictions   = 0x10,
    HasAddonRestrictions = 0x20, // is only designed to appear on certain HUD elements

    Replica              = 0x40,  // Designed to recreate (in full or in part) the appearance of an existing job gauge
    MultiComponent       = 0x80,

    // ReSharper disable once UnusedMember.Global
    Special              = 0x100,

    Exclude              = 0x1000 // never show this Widget as an option. for WIP widgets
}
