namespace GaugeOMatic.GameData;

public class ParamRef : ItemRef
{
    public ParamTypes ParamType;

    public ParamRef(ParamTypes type)
    {
        Job = JobData.Job.None;
        Role = JobData.Role.Combat;
        ParamType = type;
        ID = (uint)type;
        Name = type.ToString();
    }

    public static implicit operator ParamRef(uint i) => new((ParamTypes)i);
    public static implicit operator ParamRef(ParamTypes p) => new(p);

    public enum ParamTypes { HP = 1, MP = 2, Castbar = 3 }
}
