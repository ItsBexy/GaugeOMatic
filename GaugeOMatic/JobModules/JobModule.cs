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
using static GaugeOMatic.Windows.ItemRefMenu;

namespace GaugeOMatic.JobModules;

public abstract class JobModule : IDisposable
{
    public abstract Job Job { get; }
    public abstract Job Class { get; }
    public abstract Role Role { get; }
    public string Abbr => Job.ToString();

    public virtual List<AddonOption> AddonOptions => new() { new("_ParameterWidget", "Parameter Bar") };
    public string WatchedAddon0;
    public string? WatchedAddon1;

    public Configuration Configuration;
    public TrackerManager TrackerManager;
    public TweakConfigs TweakConfigs => Configuration.TweakConfigs;
    public List<Tracker> TrackerList;

    public IEnumerable<Widget?> WidgetList => TrackerList.Select(static t => t.Widget);
    public IEnumerable<Tracker> DrawOrder => TrackerList.ToArray().OrderBy(static t => t.TrackerConfig.Index);
    public IEnumerable<Tracker> BuildOrder => DrawOrder.Where(static t => t.TrackerConfig.Enabled).Reverse();

    public TrackerConfig[] TrackerConfigList;
    public TrackerConfig[] SaveOrder => TrackerConfigList.OrderBy(static t => t.Index).ToArray();

    protected unsafe JobModule(TrackerManager trackerManager, TrackerConfig[] trackerConfigList, string watchedAddon0, string? watchedAddon1 = null)
    {
        TrackerManager = trackerManager;
        Configuration = trackerManager.Configuration;
        TrackerConfigList = trackerConfigList;
        WatchedAddon0 = watchedAddon0;
        WatchedAddon1 = watchedAddon1;

        TrackerList = BuildTrackerList();

        RegisterListeners();
        if ((AtkUnitBase*)GameGui.GetAddonByName(WatchedAddon0) != null) BuildWidgets();
    }

    public void Dispose()
    {
        UnregisterListeners();
        DisposeTrackers();
    }

    private List<Tracker> BuildTrackerList()
    {
        var trackerList = new List<Tracker>();
        if (TrackerConfigList.Length <= 0) return trackerList;

        foreach (var trackerConfig in SaveOrder)
        {
            if (!JobCheck(trackerConfig))
            {
                trackerConfig.TrackerType = nameof(EmptyTracker);
                trackerConfig.ItemId = 0;
                trackerConfig.AddonName = "_ParameterWidget";
            }
            trackerConfig.Index = trackerList.Count;
            var newtracker = Tracker.Create(this, trackerConfig);
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
        AddonLifecycle.RegisterListener(PreDraw, WatchedAddon0, DrawHandler);
        AddonLifecycle.RegisterListener(PreFinalize, WatchedAddon0, FinalizeHandler);

        AddonLifecycle.RegisterListener(PreUpdate, WatchedAddon0, UpdateHandler0);
        AddonLifecycle.RegisterListener(PreRequestedUpdate, WatchedAddon0, UpdateHandler0);

        if (WatchedAddon1 != null)
        {
            AddonLifecycle.RegisterListener(PreUpdate, WatchedAddon1, UpdateHandler1);
            AddonLifecycle.RegisterListener(PreRequestedUpdate, WatchedAddon1, UpdateHandler1);
        }
    }

    public void UnregisterListeners()
    {
        AddonLifecycle.UnregisterListener(PostSetup, AddonOptions.Select(static a => a.Name), SetupHandler);
        AddonLifecycle.UnregisterListener(PreDraw, WatchedAddon0, DrawHandler);
        AddonLifecycle.UnregisterListener(PreFinalize, WatchedAddon0, FinalizeHandler);

        AddonLifecycle.UnregisterListener(PreUpdate, WatchedAddon0, UpdateHandler0);
        AddonLifecycle.UnregisterListener(PreRequestedUpdate, WatchedAddon0, UpdateHandler0);

        if (WatchedAddon1 != null)
        {
            AddonLifecycle.UnregisterListener(PreUpdate, WatchedAddon1, UpdateHandler1);
            AddonLifecycle.UnregisterListener(PreRequestedUpdate, WatchedAddon1, UpdateHandler1);
        }
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

    public void UpdateHandler0(AddonEvent type, AddonArgs args)
    {
        try { ApplyTweaks0(); }
        catch (Exception ex) { Log.Error($"Couldn't apply tweaks! ({WatchedAddon0}) \n{ex}");}
    }

    public void UpdateHandler1(AddonEvent type, AddonArgs args)
    {
        try { ApplyTweaks1(); }
        catch (Exception ex) { Log.Error($"Couldn't apply tweaks! ({WatchedAddon1})\n{ex}"); }
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
    public virtual void ApplyTweaks0() { }
    public virtual void ApplyTweaks1() { }
    public abstract List<MenuOption> JobGaugeMenu { get; }
}
