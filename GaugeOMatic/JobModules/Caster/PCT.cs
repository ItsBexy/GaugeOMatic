using GaugeOMatic.Trackers;
using GaugeOMatic.Windows;
using System.Collections.Generic;
using static GaugeOMatic.GameData.JobData;
using static GaugeOMatic.GameData.JobData.Job;
using static GaugeOMatic.GameData.JobData.Role;
using static GaugeOMatic.Windows.ItemRefMenu;

namespace GaugeOMatic.JobModules;

public class PCTModule : JobModule
{
    public override Job Job => PCT;
    public override Job Class => Job.None;
    public override Role Role => Caster;
    public override List<AddonOption> AddonOptions => new()
    {
        new("JobHudRPM0", "Canvases"),
        new("JobHudRPM1", "Palette Gauge"),
        new("_ParameterWidget", "Parameter Bar")
    };

    public override List<MenuOption> JobGaugeMenu { get; } = new() {
        new("Creature Motif Deadline", nameof(CreatureMotifDeadline)),
        new("Weapon Motif Deadline", nameof(WeaponMotifDeadline)),
        new("Landscape Motif Deadline", nameof(LandscapeMotifDeadline)),
    };

    public PCTModule(TrackerManager trackerManager, TrackerConfig[] trackerConfigList) : base(trackerManager, trackerConfigList) { }

    public override void Save()
    {
        Configuration.TrackerConfigs.PCT = SaveOrder;
        Configuration.Save();
    }

    public override void TweakUI(ref UpdateFlags update)
    {

        if (update.HasFlag(UpdateFlags.Save)) ApplyTweaks();
    }

    public override void ApplyTweaks()
    {

    }
}

public partial class TweakConfigs
{

}
