using GaugeOMatic.GameData;
using GaugeOMatic.JobModules;
using GaugeOMatic.Widgets;
using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.Linq;
using static GaugeOMatic.GameData.JobData;
using static GaugeOMatic.GameData.JobData.Role;
using static GaugeOMatic.GameData.ParamRef;
using static GaugeOMatic.Widgets.WidgetAttribute;
using static Newtonsoft.Json.JsonConvert;

namespace GaugeOMatic.Trackers;

public enum RefType { None, Status, Action, JobGauge, Parameter }

public class TrackerConfig
{
    public string TrackerType { get; set; } = null!;
    public uint ItemId;
    public string AddonName { get; set; } = null!;
    public WidgetConfig WidgetConfig = null!;

    public bool Enabled { get; set; }

    public bool LimitLevelRange = false;

    [DefaultValue(1)]
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
    public byte LevelMin { get; set; } = 1;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
    [DefaultValue(LevelCap)]
    public byte LevelMax { get; set; } = LevelCap;

    public bool HideOutsideCombatDuty;

    [JsonIgnore] public int Index { get; set; }
    [JsonIgnore] public bool Preview { get; set; }
    [JsonIgnore] public float PreviewValue { get; set; } = 1f;

    [JsonIgnore] public string? WidgetType
    {
        get => WidgetConfig.WidgetType;
        set => WidgetConfig.WidgetType = value;
    }

    [JsonIgnore] public TrackerDisplayAttribute? DisplayAttr;
    public TrackerDisplayAttribute GetDisplayAttr() =>
        DisplayAttr ??= TrackerType switch
        {
            nameof(StatusTracker) => new((StatusRef)ItemId),
            nameof(ActionTracker) => new((ActionRef)ItemId),
            nameof(ParameterTracker) => Attrs[(ParamTypes)ItemId],
            _ => Type.GetType($"{typeof(Tracker).Namespace}.{TrackerType}")?
                     .GetCustomAttributes(typeof(TrackerDisplayAttribute), true)
                     .FirstOrDefault() as TrackerDisplayAttribute ?? new()
        };

    public TrackerConfig? Clone() => DeserializeObject<TrackerConfig>(SerializeObject(this));

    public bool JobRoleMatch(JobModule module)
    {
        var attr = GetDisplayAttr();
        return (attr.Role != None && attr.Role.HasFlag(module.Role)) || attr.Job == module.Job || (attr.Job == module.Class && module.Class != Job.None);
    }

    public bool JobMatch(JobModule module)
    {
        var attr = GetDisplayAttr();
        return attr.Job == module.Job || (module.Class != Job.None && attr.Job == module.Class);
    }

    public TrackerConfig CleanUp()
    {
        WidgetConfig = WidgetConfig.CleanUp(WidgetType);
        return this;
    }

    public void DrawTooltip() => GetDisplayAttr().DrawTooltip(TrackerType, ItemId);

    [JsonIgnore] public string WidgetDisplayName => WidgetList.TryGetValue(WidgetType ?? string.Empty, out var result) ? result.DisplayName : "";
}
