using Newtonsoft.Json;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using static GaugeOMatic.Utility.Json;
using static Newtonsoft.Json.JsonConvert;

namespace GaugeOMatic.Widgets;

// container class for any/all widget configs associated with a tracker.
// has a field for each type of widget (my sloppy way of getting around polymorphic deserialization)
// each widget contributes a part to this class in its own file.
public partial class WidgetConfig
{
    public string? WidgetType { get; set; }
    public static implicit operator string(WidgetConfig w) => SerializeObject(w, JsonSettings);
    public static implicit operator WidgetConfig?(string s) => DeserializeObject<WidgetConfig>(s) ?? null;

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
    [JsonIgnore] public virtual Vector2 DefaultPosition { get; } = new(0);
    [DefaultValue(1f)] public float Scale = 1;

    public bool ShowIcon;
    public Vector2 IconPosition;
    [DefaultValue(1f)] public float IconScale = 1;
    public MilestoneType SoundType;
    [DefaultValue(0.5f)] public float SoundMilestone = 0.5f;
    [DefaultValue(78)] public uint SoundId = 78;

    public WidgetTypeConfig(WidgetTypeConfig? config)
    {
        if (config == null)
        {
            Position = DefaultPosition;
            return;
        }

        Position = config.Position;
        Scale = config.Scale;
        ShowIcon = config.ShowIcon;
        IconPosition = config.IconPosition;
        IconScale = config.IconScale == 0 ? 1: config.IconScale;

        SoundType = config.SoundType;
        SoundMilestone = config.SoundMilestone;
        SoundId = config.SoundId;
    }

    public WidgetTypeConfig() => Position = DefaultPosition;
}

