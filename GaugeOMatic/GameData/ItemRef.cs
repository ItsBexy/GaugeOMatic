using GaugeOMatic.JobModules;
using static GaugeOMatic.GameData.JobData;
using static GaugeOMatic.GameData.JobData.Job;

namespace GaugeOMatic.GameData;

public abstract class ItemRef
{
    public Job Job;
    public Role Role;
    public uint ID;
    public string Name = string.Empty;

    public bool CheckJob(Job job, Job jobClass, Role role) => job == Job || (jobClass!= None && jobClass == Job) || Role.HasFlag(role);
    public bool CheckJob(JobModule module) => CheckJob(module.Job, module.Class, module.Role);
}

public class ParamRef : ItemRef
{
    public ParamTypes ParamType;

    public ParamRef(ParamTypes type)
    {
        Job = None;
        Role = Role.Combat;
        ParamType = type;
        ID = (uint)type;
        Name = type.ToString();
    }

    public static implicit operator ParamRef(uint i) => new((ParamTypes)i);
    public static implicit operator ParamRef(ParamTypes p) => new(p);

    public enum ParamTypes { HP = 1, MP = 2, Castbar = 3 }
}
