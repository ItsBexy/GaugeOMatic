using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using FFXIVClientStructs.FFXIV.Component.GUI;
using GaugeOMatic.Config;
using GaugeOMatic.Trackers;
using GaugeOMatic.Widgets;
using GaugeOMatic.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using static Dalamud.Game.Addon.Lifecycle.AddonEvent;
using static GaugeOMatic.GameData.ActionData;
using static GaugeOMatic.GameData.JobData;
using static GaugeOMatic.GameData.StatusData;
using static GaugeOMatic.GaugeOMatic.Service;
using static GaugeOMatic.Windows.ItemRefMenu;

namespace GaugeOMatic.JobModules;

public abstract class JobModule : IDisposable
{
    public abstract Job Job { get; }
    public abstract Job Class { get; }
    public abstract Role Role { get; }
    public string Abbr => Job.ToString();

    public virtual List<AddonOption> AddonOptions => new() { new("_ParameterWidget", "Parameter Bar") };
    public string WatchedAddon => AddonOptions[0].Name;

    public Configuration Configuration;
    public TrackerManager TrackerManager;
    public TweakConfigs TweakConfigs => Configuration.TweakConfigs;
    public List<Tracker> TrackerList;
    public IEnumerable<Widget?> WidgetList => TrackerList.Select(static t => t.Widget);
    public TrackerConfig[] TrackerConfigList;

    public IEnumerable<Tracker> DrawOrder => TrackerList.ToArray().OrderBy(static t => t.TrackerConfig.Index);
    public IEnumerable<Tracker> BuildOrder => DrawOrder.Where(static t => t.TrackerConfig.Enabled).Reverse();
    public TrackerConfig[] SaveOrder => TrackerConfigList.OrderBy(static t => t.Index).ToArray();

    protected unsafe JobModule(TrackerManager trackerManager, TrackerConfig[] trackerConfigList)
    {
        TrackerManager = trackerManager;
        Configuration = trackerManager.Configuration;
        TrackerConfigList = trackerConfigList;

        TrackerList = BuildTrackerList();

        RegisterListeners();
        if ((AtkUnitBase*)GameGui.GetAddonByName(WatchedAddon) != null) BuildWidgets();
    }

    public void Dispose()
    {
        UnregisterListeners();
        DisposeTrackers();
    }

    public static int SortTrackerConfigs(TrackerConfig a, TrackerConfig b) => a.Index - b.Index;

    private List<Tracker> BuildTrackerList()
    {
        var trackerList = new List<Tracker>();
        if (TrackerConfigList.Length <= 0) return trackerList;

        Array.Sort(TrackerConfigList, SortTrackerConfigs);
        
        foreach (var trackerConfig in TrackerConfigList)
        {
            if (!JobCheck(trackerConfig))
            {
                trackerConfig.TrackerType = nameof(EmptyTracker);
                trackerConfig.ItemId = 0;
                trackerConfig.AddonName = "_ParameterWidget";
            }
            trackerConfig.Index = trackerList.Count;
            var newtracker = BuildTracker(trackerConfig);
            if (newtracker != null) trackerList.Add(newtracker);
        }
        
        return trackerList;
    }

    private bool JobCheck(TrackerConfig trackerConfig)
    {
        var itemId = trackerConfig.ItemId;
        return trackerConfig.TrackerType switch
        {
            nameof(StatusTracker) or nameof(ActionTracker) when itemId == 0 => false,
            nameof(StatusTracker) => ((StatusRef)itemId).CheckJob(this),
            nameof(ActionTracker) => ((ActionRef)itemId).CheckJob(this),
            _ => true
        };
    }

    public void RebuildTrackerList()
    {
        DisposeTrackers();
        TrackerList = BuildTrackerList();
        BuildWidgets();
    }

    public void AddTrackerConfig(TrackerConfig newConfig)
    {
        newConfig.Index = TrackerList.Count;

        var newList = new TrackerConfig[TrackerConfigList.Length + 1];
        newList[^1] = newConfig;

        TrackerConfigList.CopyTo(newList, 0);
        TrackerConfigList = newList;

        RebuildTrackerList();
        Save();
    }

    public void RegisterListeners()
    {
        AddonLifecycle.RegisterListener(PostSetup, AddonOptions.Select(static a => a.Name), SetupHandler);
        AddonLifecycle.RegisterListener(PreDraw, WatchedAddon, DrawHandler);
        AddonLifecycle.RegisterListener(PreFinalize, WatchedAddon, FinalizeHandler);
        AddonLifecycle.RegisterListener(PostUpdate, WatchedAddon, ReqUpdateHandler);
    }

    public void UnregisterListeners()
    {
        AddonLifecycle.UnregisterListener(PostSetup, AddonOptions.Select(static a => a.Name), SetupHandler);
        AddonLifecycle.UnregisterListener(PreDraw, WatchedAddon, DrawHandler);
        AddonLifecycle.UnregisterListener(PreFinalize, WatchedAddon, FinalizeHandler);
        AddonLifecycle.UnregisterListener(PostUpdate, WatchedAddon, ReqUpdateHandler);
    }
    
    public void SetupHandler(AddonEvent type, AddonArgs args)
    {
        foreach (var module in TrackerManager.JobModules) module.DisposeTrackers();
        BuildWidgets();
    }

    public void FinalizeHandler(AddonEvent type, AddonArgs args)
    {
        DisposeTrackers();
    }

    public void DrawHandler(AddonEvent type, AddonArgs args) => UpdateTrackers();

    public void ReqUpdateHandler(AddonEvent type, AddonArgs args)
    {
        try { ApplyTweaks(); }
        catch (Exception ex) { Log.Error($"Couldn't apply tweaks! \n{ex}"); }
    }

    public void BuildWidgets()
    {
        foreach (var tracker in BuildOrder) tracker.BuildWidget(Configuration, AddonOptions);
    }

    public void DisposeTrackers()
    {
        foreach (var tracker in TrackerList) tracker.Dispose();
    }

    public void DisposeWidgets()
    {
        foreach (var tracker in TrackerList) tracker.DisposeWidget();
    }

    public void UpdateTrackers()
    {
        foreach (var tracker in TrackerList.Where(static t => t.TrackerConfig.Enabled)) tracker.UpdateTracker();
    }

    public void ResetWidgets()
    {
        DisposeWidgets();
        BuildWidgets();
    }

    public void SoftReset()
    {
        foreach (var tracker in TrackerList) tracker.Widget?.Detach();
        foreach (var tracker in BuildOrder) tracker.Widget?.Attach();
    }

    public void RemoveTracker(Tracker tracker)
    {
        tracker.Dispose();
        TrackerList.Remove(tracker);
        TrackerConfigList = TrackerList.Select(static t => t.TrackerConfig).ToArray();
        RebuildTrackerList();
        Save();
    }

    public void AddBlankTracker() => AddTrackerConfig(new(nameof(EmptyTracker), "Empty Tracker", true, AddonOptions[0].Name, new()));

    public abstract void Save();
    public abstract void TweakUI(ref UpdateFlags update);
    public abstract void ApplyTweaks();
    public abstract List<MenuOption> JobGaugeMenu { get; }

    public Tracker? BuildTracker(TrackerConfig trackerConfig)
    {
        var qualifiedTypeStr = $"{typeof(Tracker).Namespace}.{trackerConfig.TrackerType}";
        var tracker = (Tracker?)Activator.CreateInstance(Type.GetType(qualifiedTypeStr) ?? typeof(EmptyTracker));

        if (tracker == null) return null;

        tracker.JobModule = this;

        if (trackerConfig.ItemId != 0)
        {
            tracker.ItemRef = trackerConfig.TrackerType switch
            {
                nameof(ActionTracker) => (ActionRef)trackerConfig.ItemId,
                nameof(StatusTracker) => (StatusRef)trackerConfig.ItemId,
                _ => null
            };
        }

        tracker.TrackerConfig = trackerConfig;

        tracker.AddonDropdown = new(tracker);
        tracker.WidgetMenuTable = new(tracker);
        tracker.WidgetMenuWindow = new(tracker);
        tracker.ItemRefMenu = new(tracker);

        trackerConfig.DefaultName = tracker.DisplayName;

        return tracker;
    }
}
