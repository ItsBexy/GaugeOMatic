using GaugeOMatic.Config;
using GaugeOMatic.GameData;
using GaugeOMatic.JobModules;
using GaugeOMatic.Widgets;
using GaugeOMatic.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using static GaugeOMatic.GameData.ActionData;
using static GaugeOMatic.GameData.StatusData;
using static GaugeOMatic.GaugeOMatic;
using static System.Activator;

namespace GaugeOMatic.Trackers;

public abstract partial class Tracker : IDisposable
{
    public TrackerConfig TrackerConfig { get; set; } = null!;
    public abstract string DisplayName { get; }

    public Widget? Widget { get; set; }
    public bool Available;

    public bool UsePreviewValue => TrackerConfig.Preview && (GaugeOMatic.ConfigWindow.IsOpen || Window?.IsOpen == true);

    public JobModule JobModule = null!;
    public WidgetMenu WidgetMenuTable = null!;
    public WidgetMenu WidgetMenuWindow = null!;
    public AddonDropdown AddonDropdown = null!;
    public ItemRefMenu ItemRefMenu = null!;
    public ItemRef? ItemRef;

    public string AddonName
    {
        get => TrackerConfig.AddonName;
        set => TrackerConfig.AddonName = value;
    }

    public string? WidgetType
    {
        get => TrackerConfig.WidgetType;
        set => TrackerConfig.WidgetType = value;
    }

    public WidgetConfig WidgetConfig
    {
        get => TrackerConfig.WidgetConfig;
        set => TrackerConfig.WidgetConfig = value;
    }

    public TrackerWindow? Window { get; set; }

    public void Dispose()
    {
        if (WindowSystem.Windows.Contains(Window)) WindowSystem.RemoveWindow(Window!);
        Window?.Dispose();

        DisposeWidget();
    }

    public void BuildWidget(Configuration configuration, List<AddonOption> addonOptions)
    {
        if (TrackerConfig.Enabled == false) return;

        UpdateValues();
        Widget = Widget.Create(this);

        if (Widget != null)
        {
            if (Window == null) CreateWindow(Widget, configuration);
            else
            {
                if (!WindowSystem.Windows.Contains(Window)) WindowSystem.AddWindow(Window);
                Window.Widget = Widget;
            }

            Available = true;
        }
        AddonDropdown.Prepare(addonOptions);
    }

    public void CreateWindow(Widget widget, Configuration configuration)
    {
        Window = new(this, widget, configuration, $"{TrackerConfig.DisplayAttributes().TypeDesc}: {DisplayName}##{GetHashCode()}");
        WindowSystem.AddWindow(Window);
    }

    public void DisposeWidget()
    {
        Widget?.Dispose();
        Available = false;
        Widget = null;
    }

    public void UpdateValues()
    {
        PreviousData = CurrentData;
        CurrentData = GetCurrentData(UsePreviewValue? TrackerConfig.PreviewValue : null);
    }

    public void UpdateTracker()
    {
        UpdateValues();
        Widget?.Update();
    }

    public static Tracker? Create(JobModule jobModule, TrackerConfig trackerConfig)
    {
        var qualifiedTypeStr = $"{typeof(Tracker).Namespace}.{trackerConfig.TrackerType}";
        var type = Type.GetType(qualifiedTypeStr);

        var tracker = (Tracker?)CreateInstance(type ?? typeof(EmptyTracker));

        if (tracker == null) return null;

        tracker.JobModule = jobModule;

        if (trackerConfig.ItemId != 0)
        {
            tracker.ItemRef = trackerConfig.TrackerType switch
            {
                nameof(ActionTracker)    => (ActionRef)trackerConfig.ItemId,
                nameof(StatusTracker)    => (StatusRef)trackerConfig.ItemId,
                nameof(ParameterTracker) => (ParamRef)trackerConfig.ItemId,
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
