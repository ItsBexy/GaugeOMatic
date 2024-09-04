using Dalamud.Configuration;
using Dalamud.Plugin;
using GaugeOMatic.JobModules;
using GaugeOMatic.Trackers;
using GaugeOMatic.Trackers.Presets;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using static GaugeOMatic.GameData.JobData;
using static GaugeOMatic.Windows.ConfigWindow;
using static GaugeOMatic.Windows.ConfigWindow.GeneralTab;

namespace GaugeOMatic.Config;

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 0;

    [JsonIgnore] public GeneralTab GeneralTab { get; set; } = Jobs;
    [JsonIgnore] public Job JobTab { get; set; } = Current;

    public List<Preset> SavedPresets { get; set; } = [];
    public int PresetFiltering { get; set; } = 2;

    public TrackerConfigs TrackerConfigs { get; set; } = new();
    public TweakConfigs TweakConfigs { get; set; } = new();

    [NonSerialized]
    internal IDalamudPluginInterface? PluginInterface;

    public void Initialize(IDalamudPluginInterface pluginInterface) => PluginInterface = pluginInterface;
    public void Save() => PluginInterface!.SavePluginConfig(this);
}
