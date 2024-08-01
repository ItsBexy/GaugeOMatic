using GaugeOMatic.JobModules;
using static GaugeOMatic.GameData.JobData;
using static GaugeOMatic.GameData.JobData.Job;

namespace GaugeOMatic.GameData;

public abstract partial class ItemRef
{
    public Job Job;
    public Role Role;
    public uint ID;
    public string Name = string.Empty;
    public bool HideFromDropdown;
    public ushort? Icon { get; set; }

    public bool CheckJob(Job job, Job jobClass, Role role) => job == Job || (jobClass != None && jobClass == Job) || Role.HasFlag(role);

    public bool CheckJob(JobModule module) => CheckJob(module.Job, module.Class, module.Role);
}
