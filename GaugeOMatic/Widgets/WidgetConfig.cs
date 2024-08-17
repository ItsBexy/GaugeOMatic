using GaugeOMatic.Trackers;
using GaugeOMatic.Utility;
using Newtonsoft.Json;
using System.ComponentModel;
using System.Linq;
using System.Numerics;

// ReSharper disable VirtualMemberCallInConstructor

namespace GaugeOMatic.Widgets;

public partial class WidgetConfig // each widget contributes a part to this in its own file
{
    public string? WidgetType { get; set; }
    public static implicit operator string(WidgetConfig w) => JsonConvert.SerializeObject(w, Json.JsonSettings);
    public static implicit operator WidgetConfig?(string s) => JsonConvert.DeserializeObject<WidgetConfig>(s) ?? null;

    public WidgetConfig CleanUp(string? widgetType) // nullify any configs in here that don't belong to the currently-set widget type
    {
        foreach (var p in
                 from p in typeof(WidgetConfig).GetProperties()
                 where p.GetValue(this) != null
                 let decType = p.PropertyType.DeclaringType?.Name
                 where decType != null && decType != widgetType
                 select p)
            p.SetValue(this, null);

        return this;
    }
}

public class WidgetTypeConfig // base class for the individual types of configs found inside WidgetConfig
{
    public Vector2 Position;
    [DefaultValue(1f)] public float Scale = 1;
    [JsonIgnore] public virtual Vector2 DefaultPosition { get; } = new(0);

    public WidgetTypeConfig(WidgetTypeConfig? config)
    {
        if (config == null)
        {
            Position = DefaultPosition;
            return;
        }

        Position = config.Position;
        Scale = config.Scale;
    }

    public WidgetTypeConfig() => Position = DefaultPosition;

    public void WriteToTracker(Tracker tracker) =>
        typeof(WidgetConfig).GetProperties()
                            .FirstOrDefault(prop => prop.PropertyType.Name == GetType().Name)?
                            .SetValue(tracker.TrackerConfig.WidgetConfig, this);
}

