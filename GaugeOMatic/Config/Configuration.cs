using System;
using System.Collections.Generic;
using Dalamud.Configuration;
using Dalamud.Plugin;
using GaugeOMatic.JobModules;
using GaugeOMatic.Trackers;
using GaugeOMatic.Trackers.Presets;
using static GaugeOMatic.GameData.JobData;
using static GaugeOMatic.Windows.ConfigWindow;
using static GaugeOMatic.Windows.ConfigWindow.GeneralTab;

namespace GaugeOMatic.Config;

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 0;

    public GeneralTab GeneralTab { get; set; } = Jobs;
    public Job JobTab { get; set; }

    public List<Preset> SavedPresets { get; set; } = new();
    public int PresetFiltering { get; set; } = 2;

    public TrackerConfigs TrackerConfigs { get; set; } = new();
    public TweakConfigs TweakConfigs { get; set; } = new();

    [NonSerialized]
    internal DalamudPluginInterface? PluginInterface;

    public void Initialize(DalamudPluginInterface pluginInterface) => PluginInterface = pluginInterface;
    public void Save() => PluginInterface!.SavePluginConfig(this);
}
