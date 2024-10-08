using GaugeOMatic.Trackers;
using GaugeOMatic.Windows.Dropdowns;
using System.Collections.Generic;
using static GaugeOMatic.GameData.JobData;
using static GaugeOMatic.GameData.JobData.Job;
using static GaugeOMatic.GameData.JobData.Role;
using static GaugeOMatic.Windows.Dropdowns.TrackerDropdown;

namespace GaugeOMatic.JobModules;

public class BLUModule(TrackerManager trackerManager, TrackerConfig[] trackerConfigList)
    : JobModule(trackerManager, trackerConfigList, "JobHudBLU0", "JobHudBLU1")
{
    public override Job Job => BLU;
    public override Job Class => Job.None;
    public override Role Role => Caster | Limited;

    public override List<AddonOption> AddonOptions =>
    [
        new("JobHudBLU0", "Elemental Gauge"),
        new("_ParameterWidget", "Parameter Bar")
    ];

    public override void Save()
    {
        Configuration.TrackerConfigs.BLU = SaveOrder;
        Configuration.Save();
    }

    public override List<MenuOption> JobGaugeMenu { get; } = [];

    public override void TweakUI()
    {

    }

}
