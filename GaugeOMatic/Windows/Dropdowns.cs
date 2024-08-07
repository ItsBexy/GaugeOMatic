using GaugeOMatic.GameData;
using GaugeOMatic.Trackers;
using GaugeOMatic.Utility;
using GaugeOMatic.Widgets;
using ImGuiNET;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using static Dalamud.Interface.Utility.ImGuiHelpers;
using static GaugeOMatic.GameData.ActionRef;
using static GaugeOMatic.GameData.StatusRef;
using static GaugeOMatic.Trackers.Tracker;
using static GaugeOMatic.Trackers.Tracker.UpdateFlags;
using static GaugeOMatic.Widgets.WidgetInfo;
using static GaugeOMatic.Widgets.WidgetTags;
using static System.StringComparison;

namespace GaugeOMatic.Windows;

public abstract class Dropdown<T>
{
    public abstract List<T> Values { get; }
    public abstract List<string> DisplayNames { get; }
    public string[] ComboList => DisplayNames.ToArray();
    public T CurrentSelection => Values[Index];

    public int Index;

    public bool Draw(string label, float size)
    {
        var index = Index;

        ImGui.SetNextItemWidth(size * GlobalScale);
        if (ImGui.Combo($"##{label}", ref index, ComboList, ComboList.Length) && index != Index)
        {
            Index = index;
            return true;
        }

        return false;
    }
}

public struct AddonOption
{
    public string Name;
    public string DisplayName;

    public AddonOption(string name, string displayName)
    {
        Name = name;
        DisplayName = displayName;
    }
}

public class AddonDropdown : Dropdown<string>
{
    public AddonDropdown(Tracker tracker)
    {
        Tracker = tracker;
        Prepare(Tracker.JobModule.AddonOptions);
    }
    public sealed override List<string> Values { get; } = new();
    public sealed override List<string> DisplayNames { get; } = new();
    public Tracker Tracker;

    public void Prepare(List<AddonOption> addonOptions)
    {
        Values.Clear();
        DisplayNames.Clear();

        if (Tracker.WidgetType != null && WidgetList.TryGetValue(Tracker.WidgetType, out var wType))
        {
            var whiteList = wType.AllowedAddons;
            var blackList = wType.RestrictedAddons;
            foreach (var option in addonOptions)
            {
                if (whiteList is { Count: > 0 } && !whiteList.Contains(option.Name)) continue;
                if (blackList is { Count: > 0 } && blackList.Contains(option.Name)) continue;
                Values.Add(option.Name);
                DisplayNames.Add(option.DisplayName);
            }
        }

        Index = Values.IndexOf(Tracker.TrackerConfig.AddonName);
    }
}

public abstract class BranchingDropdown
{
    public bool IsOpen { get; set; }
    public abstract string DropdownText(string fallback);

    /// <summary>An indexed collection, whose values can inform the behaviour of the <see cref ="DrawSubMenu">DrawSubMenu()</see> method.</summary>
    public abstract ICollection SubMenus { get; }
    public abstract void DrawSubMenu(int i, ref UpdateFlags update);

    public void Draw(string label, float width, ref UpdateFlags update)
    {
        var i = 0;

        if (IsOpen) ImGui.PushStyleColor(ImGuiCol.Button, ImGuiHelpy.GetStyleColorVec4(ImGuiCol.ButtonHovered));

        var windowPos = ImGui.GetWindowPos();
        var cursorPos = ImGui.GetCursorPos();
        ImGui.SetNextItemWidth(width * GlobalScale);
        ImGui.Combo($"##{label}{GetHashCode()}FakeCombo", ref i, DropdownText(label));
        if (IsOpen) ImGui.PopStyleColor(1);

        var popupPos = new Vector2(windowPos.X + cursorPos.X, windowPos.Y + cursorPos.Y + 22f);
        CreateMenuPopup($"##{label}{GetHashCode()}MenuPopup", width * GlobalScale, popupPos, ref update);

        if (ImGui.IsItemClicked()) ImGui.OpenPopup($"##{label}{GetHashCode()}MenuPopup");
    }

    public void CreateMenuPopup(string label, float width, Vector2 popupPos, ref UpdateFlags update)
    {
        IsOpen = ImGui.BeginPopup(label);
        if (!IsOpen) return;

        ImGui.SetWindowPos(popupPos);
        ImGui.Button("", new(width - 16f, 0.01f));
        for (var i = 0; i < SubMenus.Count; i++) DrawSubMenu(i, ref update);
        ImGui.Button("", new(width - 16f, 0.01f));
        ImGui.EndPopup();
    }
}

public class ItemRefMenu : BranchingDropdown
{
    public Tracker Tracker;
    public int Hash => Tracker.GetHashCode();

    public struct MenuOption
    {
        public string Name;
        public string TrackerType;
        public uint ItemId;
        public TrackerDisplayAttribute DisplayAttr;

        public MenuOption(string name, string trackerType, uint itemId = 0)
        {
            Name = name;
            TrackerType = trackerType;
            ItemId = itemId;

            var attrList = Type.GetType($"{typeof(Tracker).Namespace}.{TrackerType}")?
                .GetCustomAttributes(typeof(TrackerDisplayAttribute), true);

            if (trackerType == nameof(ParameterTracker))
            {
                DisplayAttr = ParamRef.Attrs[(ParamRef.ParamTypes)itemId];
            }
            else
            {
                DisplayAttr = (TrackerDisplayAttribute?)attrList?.FirstOrDefault(new TrackerDisplayAttribute()) ?? new();
            }
        }

        public static implicit operator MenuOption(ActionRef a) => new(a.NameChain, nameof(ActionTracker), a.ID);
        public static implicit operator MenuOption(StatusRef s) => new(s.Name, nameof(StatusTracker), s.ID);
    }

    public List<MenuOption> StatusOptions { get; init; }
    public List<MenuOption> ActionOptions { get; init; }

    public sealed override List<(string label, List<MenuOption> options)> SubMenus { get; }

    public ItemRefMenu(Tracker tracker)
    {
        Tracker = tracker;
        StatusOptions = new(StatusData.Where(s => !s.Value.HideFromDropdown && s.Value.CheckJob(Tracker.JobModule))
                                     .Select(static s => (MenuOption)s.Value)
                                     .OrderBy(static s => s.Name));

        ActionOptions = new List<MenuOption>(ActionData.Where(a => !a.Value.HideFromDropdown && a.Value.CheckJob(Tracker.JobModule))
                                                      .Select(static a => (MenuOption)a.Value)
                                                      .OrderBy(static a => a.Name));

        SubMenus = new List<(string label, List<MenuOption> options)>
        {
            ("Status Effects", StatusOptions),
            ("Actions", ActionOptions),
            ("Other", ParamRef.MenuOptions)
        };

        if (Tracker.JobModule.JobGaugeMenu.Count > 0) SubMenus.Insert(2, ("Job Gauge", Tracker.JobModule.JobGaugeMenu));
    }

    public static string StatusSearchString = "";
    public static string ActionSearchString = "";

    public override void DrawSubMenu(int i, ref UpdateFlags update)
    {
        var (label, options) = SubMenus[i];
        if (!options.Any()) return;

        if (!ImGui.BeginMenu($"{label}##{Hash}{label}Menu")) return;

        if (label == "Actions")
        {
            ImGui.SetNextItemWidth(200f);
            ImGui.InputTextWithHint("##ActionSearch","Search...", ref ActionSearchString,64);
        } else if (label == "Status Effects")
        {
            ImGui.SetNextItemWidth(200f);
            ImGui.InputTextWithHint("##StatusSearch", "Search...", ref StatusSearchString, 64);
        }

        Func<MenuOption, bool> filter = label switch
        {
            "Actions" when ActionSearchString != "" => static o => o.Name.Contains(ActionSearchString, CurrentCultureIgnoreCase),
            "Status Effects" when StatusSearchString != "" => static o => o.Name.Contains(StatusSearchString, CurrentCultureIgnoreCase),
            _ => static _ => true
        };

        foreach (var o in options.Where(filter))
        {
            if (ImGui.MenuItem($"{o.Name}##{Hash}Status{o.ItemId}"))
            {
                Tracker.TrackerConfig.ItemId = o.ItemId;
                Tracker.TrackerConfig.TrackerType = o.TrackerType;
                update |= Reset | Save | Rebuild;
            }

            if (ImGui.IsItemHovered()) o.DisplayAttr.DrawTooltip(o.TrackerType, o.ItemId);
        }

        ImGui.EndMenu();
    }

    public override string DropdownText(string fallback) => Tracker.DisplayAttr.Name;
}

public class WidgetMenu : BranchingDropdown
{
    public Tracker Tracker;
    public int Hash => Tracker.GetHashCode();
    public Dictionary<string, WidgetInfo> AvailableWidgets = new();

    public override List<(string label, WidgetTags tag)> SubMenus { get; }

    public WidgetMenu(Tracker tracker)
    {
        Tracker = tracker;
        var currentData = Tracker.GetCurrentData();
        AvailableWidgets.Clear();

        foreach (var (widgetType, widgetInfo) in WidgetList)
        {
            var tags = widgetInfo.WidgetTags;

            if (tags.HasFlag(Exclude)) continue;
            if (tags.HasFlag(HasFixedCount) && currentData.MaxCount != widgetInfo.FixedCount) continue;
            if (tags.HasFlag(HasAddonRestrictions) && !Tracker.JobModule.AddonOptions.Any(a => widgetInfo.AllowedAddons?.Contains(a.Name) == true || widgetInfo.RestrictedAddons?.Contains(a.Name) == false)) continue;

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

    public override void DrawSubMenu(int i, ref UpdateFlags update)
    {
        var (label, tag) = SubMenus[i];

        if (tag == MultiComponent) MultiCompSubMenu(label, tag, ref update);
        else
        {
            if (ImGui.BeginMenu($"{label}##{Hash}{label}Menu"))
            {
                var widgets = AvailableWidgets.Where(w => w.Value.WidgetTags.HasFlag(tag))
                                              .OrderBy(static w => w.Value.DisplayName);

                foreach (var w in widgets.Where(static w => ImGui.MenuItem(w.Value.DisplayName)))
                {
                    Tracker.WidgetType = w.Key;
                    update |= Reset | Save;
                }

                ImGui.EndMenu();
            }
        }
    }

    public void MultiCompSubMenu(string label, WidgetTags tag, ref UpdateFlags update)
    {
        if (ImGui.BeginMenu($"{label}##{Hash}{label}Menu"))
        {
            var mcWidgets = AvailableWidgets.Where(w => w.Value.WidgetTags.HasFlag(tag)).ToArray();
            foreach (var (key, name) in MultiCompDict)
            {
                ImGui.PushStyleColor(ImGuiCol.Border, new Vector4(0, 0, 0, 0));
                if (ImGui.BeginMenu($"{name}##{Hash}{label}{key}Menu"))
                {
                    foreach (var w in mcWidgets.Where(w => w.Value.MultiCompData?.Key == key)
                                               .OrderBy(static w => w.Value.MultiCompData?.Index)
                                               .Where(static w => ImGui.MenuItem(w.Value.DisplayName)))
                    {
                        Tracker.WidgetType = w.Key;
                        update |= Reset | Save;
                    }

                    ImGui.EndMenu();
                }

                ImGui.PopStyleColor();
            }

            ImGui.EndMenu();
        }
    }

    public override string DropdownText(string fallback) =>
        Tracker.WidgetType != null && WidgetList.TryGetValue(Tracker.WidgetType, out var wType)
            ? wType.DisplayName
            : fallback;
}
