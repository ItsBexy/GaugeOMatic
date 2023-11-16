using System;
using Newtonsoft.Json;
using System.Linq;
using Dalamud.Utility;
using static GaugeOMatic.Utility.Json;
using static Newtonsoft.Json.JsonConvert;

namespace GaugeOMatic.Trackers.Presets;

public struct Preset
{
    public string Name;
    public TrackerConfig[] Trackers;

    [JsonIgnore] public bool BuiltIn = false;

    public Preset(string importStr, bool zipped = false)
    {
        if (zipped) importStr = Unzip(importStr);
        var import = DeserializeObject<Preset>(importStr, JsonSettings);
        Name = import.Name.Length == 0 ? "New Preset" : import.Name;
        Trackers = ReIndex(import.Trackers);
    }

    public Preset(TrackerConfig[] trackers, string name = "New Preset")
    {
        Name = name;
        Trackers = trackers;
    }

    public Preset(string name, bool builtIn, TrackerConfig[] trackers)
    {
        Name = name;
        BuiltIn = builtIn;
        Trackers = trackers;
    }

    public readonly Preset Clone() => new(ExportStr(),true);

    public readonly string ExportStr() => Zip(SerializeObject(this,JsonSettings));
    public static implicit operator string(Preset p) => p.ExportStr();

    public static implicit operator Preset(string s) => new(s,true);
    public static implicit operator TrackerConfig[](Preset p) => ReIndex(p.Trackers);
    public static TrackerConfig[] operator +(Preset a, Preset b) => ReIndex(a.Trackers.Concat(b.Trackers).ToArray());
    
    private static TrackerConfig[] ReIndex(TrackerConfig[] trackerConfigs, bool disableAll = false)
    {
        for (var i = 0; i < trackerConfigs.Length; i++)
        {
            trackerConfigs[i].Index = i;
            if (disableAll) trackerConfigs[i].Enabled = false;
        }
        return trackerConfigs;
    }

    public static string Zip(string s) => Convert.ToBase64String(Util.CompressString(s));
    public static string Unzip(string s) => Util.DecompressString(Convert.FromBase64String(s));

    public Preset Disable()
    {
        foreach (var t in Trackers) t.Enabled = false;
        return this;
    }
}
