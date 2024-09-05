using Dalamud.Game.ClientState.Conditions;
using GaugeOMatic.Config;
using GaugeOMatic.JobModules;
using System;
using System.Collections.Generic;
using static GaugeOMatic.GameData.JobData;
using static GaugeOMatic.Trackers.Presets.PluginPresets;

namespace GaugeOMatic.Trackers;

public class TrackerManager : IDisposable
{
    public Configuration Configuration;
    public TrackerConfigs TrackerConfigs => Configuration.TrackerConfigs;

    public List<JobModule> JobModules;

    public TrackerManager(Configuration configuration)
    {
        Configuration = configuration;

        JobModules =
        [
            new PLDModule(this, TrackerConfigs.PLD),
            new WARModule(this, TrackerConfigs.WAR),
            new DRKModule(this, TrackerConfigs.DRK),
            new GNBModule(this, TrackerConfigs.GNB),

            new WHMModule(this, TrackerConfigs.WHM),
            new SCHModule(this, TrackerConfigs.SCH),
            new ASTModule(this, TrackerConfigs.AST),
            new SGEModule(this, TrackerConfigs.SGE),

            new MNKModule(this, TrackerConfigs.MNK),
            new DRGModule(this, TrackerConfigs.DRG),
            new NINModule(this, TrackerConfigs.NIN),
            new SAMModule(this, TrackerConfigs.SAM),
            new RPRModule(this, TrackerConfigs.RPR),
            new VPRModule(this, TrackerConfigs.VPR),

            new BRDModule(this, TrackerConfigs.BRD),
            new MCHModule(this, TrackerConfigs.MCH),
            new DNCModule(this, TrackerConfigs.DNC),

            new BLMModule(this, TrackerConfigs.BLM),
            new SMNModule(this, TrackerConfigs.SMN),
            new RDMModule(this, TrackerConfigs.RDM),
            new PCTModule(this, TrackerConfigs.PCT),

            new BLUModule(this, TrackerConfigs.BLU)
        ];

        Condition.ConditionChange += ApplyDisplayRules;
    }

    private void ApplyDisplayRules(ConditionFlag flag, bool value)
    {
        JobModules.Find(static jm => jm.Job == Current || jm.Class == Current)?.ApplyDisplayRules();
    }

    public void Dispose()
    {
        Condition.ConditionChange -= ApplyDisplayRules;
        foreach (var module in JobModules) module.Dispose();
    }
}

public class TrackerConfigs
{
    public TrackerConfig[] PLD { get; set; } = PLDDefault.Clone().Disable();
    public TrackerConfig[] WAR { get; set; } = WARDefault.Clone().Disable();
    public TrackerConfig[] DRK { get; set; } = DRKDefault.Clone().Disable();
    public TrackerConfig[] GNB { get; set; } = GNBDefault.Clone().Disable();

    public TrackerConfig[] WHM { get; set; } = WHMDefault.Clone().Disable();
    public TrackerConfig[] SCH { get; set; } = SCHDefault.Clone().Disable();
    public TrackerConfig[] AST { get; set; } = ASTDefault.Clone().Disable();
    public TrackerConfig[] SGE { get; set; } = SGEDefault.Clone().Disable();

    public TrackerConfig[] MNK { get; set; } = MNKDefault.Clone().Disable();
    public TrackerConfig[] DRG { get; set; } = DRGDefault.Clone().Disable();
    public TrackerConfig[] NIN { get; set; } = NINDefault.Clone().Disable();
    public TrackerConfig[] SAM { get; set; } = SAMDefault.Clone().Disable();
    public TrackerConfig[] RPR { get; set; } = RPRDefault.Clone().Disable();
    public TrackerConfig[] VPR { get; set; } = VPRDefault.Clone().Disable();

    public TrackerConfig[] BRD { get; set; } = BRDDefault.Clone().Disable();
    public TrackerConfig[] MCH { get; set; } = MCHDefault.Clone().Disable();
    public TrackerConfig[] DNC { get; set; } = DNCDefault.Clone().Disable();

    public TrackerConfig[] BLM { get; set; } = BLMDefault.Clone().Disable();
    public TrackerConfig[] SMN { get; set; } = SMNDefault.Clone().Disable();
    public TrackerConfig[] RDM { get; set; } = RDMDefault.Clone().Disable();
    public TrackerConfig[] PCT { get; set; } = PCTDefault.Clone().Disable();

    public TrackerConfig[] BLU { get; set; } = [];
}
