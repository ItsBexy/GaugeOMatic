using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using static GaugeOMatic.Widgets.MultiCompDataAttribute;
using static GaugeOMatic.Widgets.WidgetAttribute;
using static GaugeOMatic.Widgets.WidgetTags;
using static GaugeOMatic.Widgets.WidgetUI;
using static GaugeOMatic.Widgets.WidgetUI.WidgetUiTab;
// ReSharper disable ParameterTypeCanBeEnumerable.Local
// ReSharper disable SuggestBaseTypeForParameter

namespace GaugeOMatic.Widgets;
[AttributeUsage(AttributeTargets.Class)]
public class WidgetNameAttribute(string name) : Attribute
{
    public string Name { get; init; } = name;
    public static implicit operator string(WidgetNameAttribute a) => a.Name;
}

[AttributeUsage(AttributeTargets.Class)]
public class WidgetDescriptionAttribute(string desc) : Attribute
{
    public string Description { get; init; } = desc;
    public static implicit operator string(WidgetDescriptionAttribute a) => a.Description;
}

[AttributeUsage(AttributeTargets.Class)]
public class WidgetAuthorAttribute(string author) : Attribute
{
    public string Author { get; init; } = author;
    public static implicit operator string(WidgetAuthorAttribute a) => a.Author;
}

[AttributeUsage(AttributeTargets.Class)]
public class WidgetTagsAttribute(WidgetTags tags) : Attribute
{
    public WidgetTags Tags { get; init; } = tags;
    public static implicit operator WidgetTags(WidgetTagsAttribute a) => a.Tags;
}

[AttributeUsage(AttributeTargets.Class)]
public class WidgetUiTabsAttribute(WidgetUiTab tabs) : Attribute
{
    public WidgetUiTab Tabs { get; init; } = tabs;
    public static implicit operator WidgetUiTab(WidgetUiTabsAttribute a) => a.Tabs;
}

[AttributeUsage(AttributeTargets.Class)]
public class MultiCompDataAttribute(string key, string groupName, int index) : Attribute
{
    public MultiComponentData MultiCompData { get; init; } = new(key, groupName, index);

    public struct MultiComponentData
    {
        public string Key;
        public int Index;

        public MultiComponentData(string key, string groupName, int index)
        {
            Key = key;
            Index = index;
            MultiCompDict.TryAdd(key, groupName);
        }
    }
}

[AttributeUsage(AttributeTargets.Class)]
public class AddonRestrictionsAttribute : Attribute
{
    public List<string> WhiteList = [];
    public List<string> BlackList = [];

    public AddonRestrictionsAttribute(bool allowed, params string[] addons)
    {
        if (allowed) WhiteList = addons.ToList();
        else BlackList = addons.ToList();
    }
}

[AttributeUsage(AttributeTargets.Class)]
public class WidgetAttribute : Attribute
{
    internal static Dictionary<string, WidgetAttribute> WidgetList = new();

    public string DisplayName { get; init; } = null!;
    public string? Author { get;  init; }
    public string? Description { get; init; }
    public WidgetTags WidgetTags { get; init; }
    public MultiComponentData? MultiCompData;
    public WidgetUiTab UiTabOptions { get; set; }
    public List<string> WhiteList = []; // only applicable if tagged with HasAddonRestrictions
    public List<string> BlackList = []; // only applicable if tagged with HasAddonRestrictions

    public bool AddonPermitted(string aName) =>
        !WidgetTags.HasFlag(HasAddonRestrictions) ||
        !((BlackList.Count > 0 && BlackList.Contains(aName)) ||
          (WhiteList.Count > 0 && !WhiteList.Contains(aName)));

    internal static Dictionary<string, string> MultiCompDict = new();

    public WidgetAttribute() { }

    public WidgetAttribute(string displayName, string? author, string? description, WidgetTags widgetTags, MultiComponentData? multiCompData, WidgetUiTab uiTabOptions, List<string> whiteList, List<string> blackList)
    {
        DisplayName = displayName;
        Author = author;
        Description = description;
        WidgetTags = widgetTags;
        MultiCompData = multiCompData;
        UiTabOptions = uiTabOptions;
        WhiteList = whiteList;
        BlackList = blackList;
    }

    public static void BuildWidgetList()
    {
        Log.Verbose("Generating list of Available Widgets");

        var types = Assembly.GetExecutingAssembly()
                            .GetTypes()
                            .Where(static t => t.IsSubclassOf(typeof(Widget)) && !t.IsAbstract)
                            .OrderBy(static t => t.Name);

        foreach (var type in types)
        {
            var (whiteList, blackList) = ParseRestrictions(type);

            var widgetAttr = new WidgetAttribute(displayName:   GetAttr<WidgetNameAttribute>(type) ?? "???",
                                                 author:        GetAttr<WidgetAuthorAttribute>(type) ?? "",
                                                 description:   GetAttr<WidgetDescriptionAttribute>(type) ?? "",
                                                 widgetTags:    GetAttr<WidgetTagsAttribute>(type) ?? WidgetTags.None,
                                                 multiCompData: GetAttr<MultiCompDataAttribute>(type)?.MultiCompData,
                                                 uiTabOptions:  GetAttr<WidgetUiTabsAttribute>(type) ?? All,
                                                 whiteList:     whiteList,
                                                 blackList:     blackList);

            WidgetList.Add(type.Name, widgetAttr);
            Log.Verbose($"Added Widget Option: {widgetAttr.DisplayName}");
        }
    }

    private static T? GetAttr<T>(Type type) => (T?)type.GetCustomAttributes(typeof(T), true).FirstOrDefault();

    private static List<T?> GetAttrList<T>(Type type) => type.GetCustomAttributes(typeof(T), true).Select(static a=>(T?)a).ToList();

    private static (List<string> whiteList, List<string> blackList) ParseRestrictions(Type type)
    {
        var addonrestrictions = GetAttrList<AddonRestrictionsAttribute>(type);

        var whiteList = new List<string>();
        var blackList = new List<string>();

        foreach (var r in addonrestrictions.Where(static r => r != null))
        {
            whiteList.AddRange(r!.WhiteList);
            blackList.AddRange(r.BlackList);
        }

        return (whiteList, blackList);
    }
}

[Flags]
public enum WidgetTags
{
    None = 0,

    // format tags
    Counter              = 0x1,
    GaugeBar             = 0x2,
    State                = 0x4,
    // ReSharper disable once UnusedMember.Global
    HasJobRestrictions   = 0x10,
    HasAddonRestrictions = 0x20, // is only designed to appear on certain HUD elements

    Replica              = 0x40,  // Designed to recreate (in full or in part) the appearance of an existing job gauge
    MultiComponent       = 0x80,

    // ReSharper disable once UnusedMember.Global
    Special              = 0x100,

    HasClippingMask      = 0x200,

    Exclude              = 0x1000 // never show this Widget as an option. for WIP widgets
}
