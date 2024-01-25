using GaugeOMatic.JobModules;
using GaugeOMatic.Widgets;
using Newtonsoft.Json;
using System;
using System.Linq;
using static GaugeOMatic.GameData.ActionData;
using static GaugeOMatic.GameData.JobData;
using static GaugeOMatic.GameData.JobData.Role;
using static GaugeOMatic.GameData.StatusData;
using static Newtonsoft.Json.JsonConvert;

namespace GaugeOMatic.Trackers;

public enum RefType { None, Status, Action, JobGauge, Parameter }

public class TrackerConfig
{
    public string TrackerType { get; set; }
    public uint ItemId;
    public string AddonName { get; set; }
    public WidgetConfig WidgetConfig;

    public bool Enabled { get; set; }

    [JsonIgnore] public int Index { get; set; }
    [JsonIgnore] public bool Preview { get; set; }
    [JsonIgnore] public float PreviewValue { get; set; } = 1f;

    [JsonIgnore] public string? WidgetType
    {
        get => WidgetConfig.WidgetType;
        set => WidgetConfig.WidgetType = value;
    }

    [JsonIgnore] public string? GetDisplayName => TrackerType switch { 
        nameof(StatusTracker) => Statuses.TryGetValue(ItemId, out var statusRef) ? statusRef.Name : null,
        nameof(ActionTracker) => Actions.TryGetValue(ItemId, out var actionRef) ? actionRef.Name : null,
        _ => DefaultName
    };

    public string DefaultName { get; set; }

    public TrackerConfig(string trackerType, string? displayName, bool enabled, string addonName, WidgetConfig widgetConfig, uint itemId = 0)
    {
        TrackerType = trackerType;
        Enabled = enabled;
        AddonName = addonName;
        WidgetConfig = widgetConfig;
        ItemId = itemId;
        DefaultName = displayName ?? trackerType;
    }

    public TrackerConfig? Clone() => DeserializeObject<TrackerConfig>(SerializeObject(this));


    public TrackerDisplayAttribute DisplayAttributes()
    {
        var displayAttr = (TrackerDisplayAttribute?)Type.GetType($"{typeof(Tracker).Namespace}.{TrackerType}")?.GetCustomAttributes(typeof(TrackerDisplayAttribute), true).First() ?? new TrackerDisplayAttribute();

        if (TrackerType == nameof(StatusTracker) && Statuses.TryGetValue(ItemId, out var statusRef))
        {
            displayAttr.Job = statusRef.Job;
            displayAttr.Role = statusRef.Role;
        }

        if (TrackerType == nameof(ActionTracker) && Actions.TryGetValue(ItemId, out var actionRef))
        {
            displayAttr.Job = actionRef.Job;
            displayAttr.Role = actionRef.Role;
        }

        return displayAttr;
    }

    public bool JobRoleMatch(JobModule module)
    {
        var attr = DisplayAttributes();
        return (attr.Role != None && attr.Role.HasFlag(module.Role)) || attr.Job == module.Job || (attr.Job == module.Class && module.Class != Job.None);
    }

    public bool JobMatch(JobModule module)
    {
        var attr = DisplayAttributes();
        return attr.Job == module.Job || (module.Class != Job.None && attr.Job == module.Class);
    }
}
