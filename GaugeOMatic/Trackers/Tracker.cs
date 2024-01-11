using GaugeOMatic.Config;
using GaugeOMatic.GameData;
using GaugeOMatic.JobModules;
using GaugeOMatic.Widgets;
using GaugeOMatic.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using static GaugeOMatic.GaugeOMatic;

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
    public ItemRef? ItemRef = null;

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
}
