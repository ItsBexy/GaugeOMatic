using GaugeOMatic.GameData;
using GaugeOMatic.Trackers;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using static GaugeOMatic.GameData.ActionRef;
using static GaugeOMatic.GameData.ParamRef;
using static GaugeOMatic.GameData.StatusRef;
using static GaugeOMatic.Widgets.WidgetUI;
using static GaugeOMatic.Widgets.WidgetUI.UpdateFlags;
using static System.StringComparison;

namespace GaugeOMatic.Windows.Dropdowns;

public class TrackerDropdown : BranchingDropdown
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
                DisplayAttr = Attrs[(ParamTypes)itemId];
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
    public List<MenuOption> StatusOptionsUnfiltered { get; init; }
    public List<MenuOption> ActionOptions { get; init; }
    public List<MenuOption> ParamOptions = ParamRef.ParamOptions;

    public sealed override List<(string label, List<MenuOption> options)> SubMenus { get; }

    public TrackerDropdown(Tracker tracker)
    {
        Tracker = tracker;
        StatusOptions = new(StatusData.Where(s => !s.Value.HideFromDropdown && s.Value.CheckJob(Tracker.JobModule))
                                      .Select(static s => (MenuOption)s.Value)
                                      .OrderBy(static s => s.Name));

        StatusOptionsUnfiltered = Sheets.AllStatuses;

        ActionOptions = new(ActionData.Where(a => !a.Value.HideFromDropdown && a.Value.CheckJob(Tracker.JobModule))
                                      .Select(static a => (MenuOption)a.Value)
                                      .OrderBy(static a => a.Name));

        SubMenus = new()
        {
            ("Status Effects", StatusOptions),
            ("Actions", ActionOptions),
            ("Other", ParamOptions)
        };

        if (Tracker.JobModule.JobGaugeMenu.Count > 0) SubMenus.Insert(2, ("Job Gauge", Tracker.JobModule.JobGaugeMenu));
    }

    public static string StatusSearchString = "";
    public static string ActionSearchString = "";

    public override void DrawSubMenu(int i)
    {
        var (label, options) = SubMenus[i];
        if (!options.Any()) return;

        if (!ImGui.BeginMenu($"{label}##{Hash}{label}Menu")) return;

        if (label == "Actions")
        {
            ImGui.SetNextItemWidth(200f);
            ImGui.InputTextWithHint("##ActionSearch", "Search...", ref ActionSearchString, 64);
        }
        else if (label == "Status Effects")
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
                UpdateFlag |= Reset | Save | Rebuild;
            }

            if (ImGui.IsItemHovered()) o.DisplayAttr.DrawTooltip(o.TrackerType, o.ItemId);
        }

        ImGui.EndMenu();
    }

    public override string DropdownText(string fallback) => Tracker.DisplayAttr.Name;
}
