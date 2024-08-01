using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;
using GaugeOMatic.Config;
using GaugeOMatic.GameData;
using GaugeOMatic.Trackers;
using GaugeOMatic.Widgets;
using GaugeOMatic.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using static Dalamud.Game.Addon.Lifecycle.AddonEvent;
using static GaugeOMatic.GameData.JobData;
using static GaugeOMatic.Trackers.Tracker;
using static GaugeOMatic.Windows.ItemRefMenu;

namespace GaugeOMatic.JobModules;

public abstract class JobModule : IDisposable
{
    public static unsafe NumberArrayData* JobUiData => UIModule.Instance()->GetRaptureAtkModule()->GetNumberArrayData(86);

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
            var newtracker = Create(this, trackerConfig);
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
        AddonLifecycle.RegisterListener(PreDraw, AddonOptions.Select(static a => a.Name).ToArray(), DrawHandler);

        AddonLifecycle.RegisterListener(PostSetup, WatchedAddon0, SetupHandler);
        AddonLifecycle.RegisterListener(PreFinalize, WatchedAddon0, FinalizeHandler);
        AddonLifecycle.RegisterListener(PreUpdate, WatchedAddon0, (_, args) => UpdateHandler(args, ApplyTweaks0));
        AddonLifecycle.RegisterListener(PreRequestedUpdate, WatchedAddon0, (_, args) => UpdateHandler(args, ApplyTweaks0));

        if (WatchedAddon1 != null)
        {
            AddonLifecycle.RegisterListener(PostSetup, WatchedAddon1, SetupHandler);
            AddonLifecycle.RegisterListener(PreUpdate, WatchedAddon1, (_, args) => UpdateHandler(args, ApplyTweaks1));
            AddonLifecycle.RegisterListener(PreRequestedUpdate, WatchedAddon1, (_, args) => UpdateHandler(args, ApplyTweaks1));
        }
    }

    public void ApplyDisplayRules()
    {
        foreach (var tracker in DrawOrder) tracker.Widget?.ApplyDisplayRules();
    }

    public void UnregisterListeners()
    {
        var addonNames = AddonOptions.Select(static a => a.Name).ToArray();

        AddonLifecycle.UnregisterListener(PostSetup, addonNames);
        AddonLifecycle.UnregisterListener(PreFinalize, addonNames);
        AddonLifecycle.UnregisterListener(PreDraw, addonNames);
        AddonLifecycle.UnregisterListener(PreUpdate, addonNames);
        AddonLifecycle.UnregisterListener(PreRequestedUpdate, addonNames);

    }

    public void SetupHandler(AddonEvent type, AddonArgs args)
    {
        Configuration.JobTab = Current;
        foreach (var module in TrackerManager.JobModules) module.DisposeTrackers();
        BuildWidgets();
    }

    public void FinalizeHandler(AddonEvent type, AddonArgs args)
    {
        DisposeTrackers();
    }

    public void DrawHandler(AddonEvent type, AddonArgs args) => UpdateTrackers(args.AddonName);

    public static void UpdateHandler(AddonArgs args, Action<IntPtr> applyFunc)
    {
        try { applyFunc(args.Addon); }
        catch (Exception) { Log.Error($"Couldn't apply tweaks! ({args.AddonName})");}
    }

    public void BuildWidgets()
    {
        foreach (var tracker in BuildOrder) tracker.BuildWidget(Configuration, AddonOptions);
        ApplyDisplayRules();
    }

    public void DisposeTrackers()
    {
        foreach (var tracker in TrackerList) tracker.Dispose();
    }

    public void DisposeWidgets()
    {
        foreach (var tracker in TrackerList) tracker.DisposeWidget();
    }

    public void UpdateTrackers(string addonName)
    {
        foreach (var tracker in TrackerList.Where(t => t.TrackerConfig.AddonName == addonName && t.TrackerConfig.Enabled)) tracker.UpdateTracker();
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
        ApplyDisplayRules();
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
    public virtual void ApplyTweaks0(IntPtr gaugeAddon) { }
    public virtual void ApplyTweaks1(IntPtr gaugeAddon) { }
    public abstract List<MenuOption> JobGaugeMenu { get; }
}
