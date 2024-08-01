using System;
using Newtonsoft.Json;
using static Dalamud.Utility.Util;
using static GaugeOMatic.Utility.Json;
using static Newtonsoft.Json.JsonConvert;
using static System.Convert;

namespace GaugeOMatic.Trackers.Presets;

public struct Preset
{
    public string Name;
    public TrackerConfig[] Trackers;

    [JsonIgnore] public bool BuiltIn = false;

    public Preset(string? importStr, bool zipped = false)
    {
        if (zipped) importStr = Unzip(importStr);
        if (importStr == null)
        {
            Name = "New Preset";
            Trackers = Array.Empty<TrackerConfig>();
        }
        else
        {
            try
            {
                var import = DeserializeObject<Preset>(importStr, JsonSettings);
                Name = import.Name.Length == 0 ? "New Preset" : import.Name;
                Trackers = import.Trackers;

            }
            catch (Exception ex)
            {
                Log.Error($"Error deserializing Preset!\n{ex}");
                Name = "New Preset";
                Trackers = Array.Empty<TrackerConfig>();
            }
        }
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

    public readonly Preset Clone() => new(ExportStr(), true);

    public readonly string ExportStr() => Zip(SerializeObject(CleanUp(), JsonSettings));

    private readonly Preset CleanUp()
    {
        for (var i = 0; i < Trackers.Length; i++) Trackers[i] = Trackers[i].CleanUp();
        return this;
    }

    public static implicit operator string(Preset p) => p.ExportStr();

    public static implicit operator Preset(string s) => new(s, true);
    public static implicit operator TrackerConfig[](Preset p) => p.Trackers;

    public static string Zip(string s) => ToBase64String(CompressString(s));
    public static string? Unzip(string? s)
    {
        try
        {
            return s == null ? null : DecompressString(FromBase64String(s));
        }
        catch (Exception ex)
        {
            Log.Error($"Error decompressing preset string:\n{s}\n{ex.StackTrace}");
            return null;
        }
    }

    public readonly Preset Disable()
    {
        foreach (var t in Trackers) t.Enabled = false;
        return this;
    }
}
