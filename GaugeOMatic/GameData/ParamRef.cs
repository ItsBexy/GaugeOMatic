using GaugeOMatic.Trackers;
using System.Collections.Generic;
using static GaugeOMatic.GameData.JobData.Job;
using static GaugeOMatic.GameData.ParamRef.ParamTypes;
using static GaugeOMatic.Windows.ItemRefMenu;

namespace GaugeOMatic.GameData;

public class ParamRef : ItemRef
{
    public ParamTypes ParamType;

    public ParamRef(ParamTypes type)
    {
        Job = None;
        Role = JobData.Role.Combat;
        ParamType = type;
        ID = (uint)type;
        Name = Attrs[type].Name;
    }

    public static implicit operator ParamRef(uint i) => new((ParamTypes)i);
    public static implicit operator ParamRef(ParamTypes p) => new(p);

    public enum ParamTypes
    {
        HP = 1,
        MP = 2,
        Castbar = 3,
        GCD = 4,
        Combo = 5,
        LimitBreak1 = 6,
        LimitBreak2 = 7,
        LimitBreak3 = 8
    }

    public static Dictionary<ParamTypes, TrackerDisplayAttribute> Attrs = new()
    {
        { HP,  new("HP", None,61233, barDesc: "Shows HP") },
        { MP, new("MP", None,61233, barDesc: "Shows MP")},
        { Castbar, new("Castbar", None,61233, barDesc: "Shows Cast Progress") },
        { GCD, new("GCD", None,61233,barDesc: "Shows global cooldown", counterDesc: "Shows if cooldown is complete", stateDesc: "Shows if cooldown is complete") },
        { Combo, new("Combo", None,61233,barDesc: "Shows combo time remaining", counterDesc: "Shows if combo timer is active", stateDesc: "Shows if combo timer is active") },
        { LimitBreak1, new("Limit Break 1", None,61233, barDesc: "Shows first limit break segment", counterDesc: "Shows if Limit Break 1 is available", stateDesc: "Shows if Limit Break 1 is available") },
        { LimitBreak2, new("Limit Break 2", None,61233,barDesc: "Shows first limit break segment", counterDesc: "Shows if Limit Break 2 is available", stateDesc: "Shows if Limit Break 2 is available") },
        { LimitBreak3, new("Limit Break 3", None,61233,barDesc: "Shows first limit break segment", counterDesc: "Shows if Limit Break 3 is available", stateDesc: "Shows if Limit Break 3 is available") }
   };

    public static List<MenuOption> MenuOptions = new()
    {
        new ParamRef(HP).CreateMenuOption(),
        new ParamRef(MP).CreateMenuOption(),
        new ParamRef(Castbar).CreateMenuOption(),
        new ParamRef(Combo).CreateMenuOption(),
        new ParamRef(GCD).CreateMenuOption()
    };

    public MenuOption CreateMenuOption() => new(Name, nameof(ParameterTracker), ID) { DisplayAttr = Attrs[ParamType] };

    public override void DrawTooltip() => Attrs[ParamType].DrawTooltip();
}
