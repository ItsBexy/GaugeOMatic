using GaugeOMatic.Trackers;
using GaugeOMatic.Utility;
using GaugeOMatic.Widgets;
using ImGuiNET;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using static Dalamud.Interface.Utility.ImGuiHelpers;
using static GaugeOMatic.GameData.ActionData;
using static GaugeOMatic.GameData.ActionData.ActionRef.ReadyTypes;
using static GaugeOMatic.GameData.ParamRef.ParamTypes;
using static GaugeOMatic.GameData.StatusData;
using static GaugeOMatic.Widgets.WidgetInfo;
using static GaugeOMatic.Widgets.WidgetTags;

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
            foreach (var option in addonOptions)
            {
                if (whiteList is { Count: > 0 } && !whiteList.Contains(option.Name)) continue;
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

        if (IsOpen) ImGui.PushStyleColor(ImGuiCol.Button, ImGuiHelpy.GetStyleColorUsableVec4(ImGuiCol.ButtonHovered));

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

        public MenuOption(string name, string trackerType, uint itemId = 0)
        {
            Name = name;
            TrackerType = trackerType;
            ItemId = itemId;
        }

        public static implicit operator MenuOption(ActionRef a) => new(a.Name, nameof(ActionTracker), a.ID);
        public static implicit operator MenuOption(StatusRef s) => new(s.Name, nameof(StatusTracker), s.ID);
    }

    public List<MenuOption> StatusOptions { get; init; }
    public List<MenuOption> ActionOptions { get; init; }
    public List<MenuOption> ParamOptions { get; init; }

    public sealed override List<(string label, List<MenuOption> options)> SubMenus { get; }

    public ItemRefMenu(Tracker tracker)
    {
        Tracker = tracker;
        StatusOptions = new(Statuses.Where(s => s.Value.CheckJob(Tracker.JobModule))
                                    .Select(static s => (MenuOption)s.Value)
                                    .OrderBy(static s => s.Name));

        ActionOptions = new(Actions.Where(a => a.Value.CheckJob(Tracker.JobModule))
                                   .Select(static a => (MenuOption)a.Value)
                                   .OrderBy(static a => a.Name));

        ParamOptions = new()
        {
            new("HP", nameof(ParameterTracker), (uint)HP),
            new("MP", nameof(ParameterTracker), (uint)MP),
            new("Castbar", nameof(ParameterTracker), (uint)Castbar)
        };

        SubMenus = new()
        {
            ("Status Effects", StatusOptions),
            ("Actions", ActionOptions),
            ("Other", ParamOptions)
        };

        if (Tracker.JobModule.JobGaugeMenu.Count > 0) SubMenus.Insert(2,("Job Gauge", Tracker.JobModule.JobGaugeMenu));
    }

    public override void DrawSubMenu(int i, ref UpdateFlags update)
    {
        var (label, options) = SubMenus[i];
        if (!ImGui.BeginMenu($"{label}##{Hash}{label}Menu")) return;

        foreach (var o in options)
        {
            if (ImGui.MenuItem($"{o.Name}##{Hash}Status{o.ItemId}"))
            {
                Tracker.TrackerConfig.ItemId = o.ItemId;
                Tracker.TrackerConfig.TrackerType = o.TrackerType;
                update |= UpdateFlags.Reset | UpdateFlags.Save | UpdateFlags.Rebuild;
            }

            if (ImGui.IsItemHovered())
            {
                if (o.TrackerType == nameof(ActionTracker))
                {
                    var action = (ActionRef)o.ItemId;
                    var oneCharge = action.MaxCharges == 1;


                    var counterDesc = $"Shows charges ({action.MaxCharges})";
                    var gaugeDesc = $"Shows cooldown time ({action.CooldownLength}s)";
                    var stateDesc = $"Shows {(action.ReadyType.HasFlag(Ants) ? "if highlighted" : "if ready")}";

                    var toolText = oneCharge ? $"Gauge: {gaugeDesc}\nCounter/State: {stateDesc}" :
                                               $"Counter: {counterDesc}\nGauge: {gaugeDesc}\nState: {stateDesc}" ;

                    ImGui.SetTooltip(toolText);
                } else if (o.TrackerType == nameof(StatusTracker))
                {
                    var status = (StatusRef)o.ItemId;
                    ImGui.SetTooltip($"Counter: Shows stacks ({status.MaxStacks})\n" +
                                     $"Gauge: Shows time remaining ({status.MaxTime}s)\n" +
                                     "State: Shows if active");
                }
            }
        }

        ImGui.EndMenu();
    }

    public override string DropdownText(string fallback) => Tracker.DisplayName;
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
            if (tags.HasFlag(HasAddonRestrictions) && Tracker.JobModule.AddonOptions.All(a => widgetInfo.AllowedAddons?.Contains(a.Name) != true)) continue;

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
                    update |= UpdateFlags.Reset | UpdateFlags.Save;
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
                        update |= UpdateFlags.Reset | UpdateFlags.Save;
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
