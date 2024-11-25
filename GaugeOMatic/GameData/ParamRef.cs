using GaugeOMatic.Trackers;
using System.Collections.Generic;
using static GaugeOMatic.GameData.ActionRef;
using static GaugeOMatic.GameData.JobData.Job;
using static GaugeOMatic.GameData.ParamRef.ParamTypes;
using static GaugeOMatic.Trackers.Tracker;
using static GaugeOMatic.Windows.Dropdowns.TrackerDropdown;

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

    internal static Dictionary<ParamTypes, TrackerDisplayAttribute> Attrs = new()
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

    internal static List<MenuOption> ParamOptions =
    [
        new ParamRef(HP).CreateMenuOption(),
        new ParamRef(MP).CreateMenuOption(),
        new ParamRef(Castbar).CreateMenuOption(),
        new ParamRef(Combo).CreateMenuOption(),
        new ParamRef(GCD).CreateMenuOption()
    ];

    private static float LastKnownGCD = 2.5f;

    public MenuOption CreateMenuOption() => new(Name, nameof(ParameterTracker), ID) { DisplayAttr = Attrs[ParamType] };

    public override void DrawTooltip() => Attrs[ParamType].DrawTooltip();

    public override unsafe TrackerData GetTrackerData(float? preview)
    {
        var count = 0;
        const int maxCount = 1;
        float gaugeValue = 0;
        float maxGauge = 1;
        var state = 0;
        const int maxState = 1;

        var hasLabelOverride = false;
        string? labelOverride = null;
        uint? iconOverride = null;
        if (FrameworkData.LocalPlayer != null)
        {
            switch (ParamType)
            {
                case HP:
                    maxGauge = FrameworkData.LocalPlayer.MaxHp;
                    gaugeValue = preview != null ? preview.Value * maxGauge : FrameworkData.LocalPlayer.CurrentHp;
                    break;
                case MP:
                    maxGauge = FrameworkData.LocalPlayer.MaxMp;
                    gaugeValue = preview != null ? preview.Value * maxGauge : FrameworkData.LocalPlayer.CurrentMp;
                    break;
                case Castbar:
                {
                    hasLabelOverride = true;
                    if (FrameworkData.LocalPlayer.IsCasting)
                    {
                        maxGauge = FrameworkData.LocalPlayer.TotalCastTime;
                        gaugeValue = FrameworkData.LocalPlayer.CurrentCastTime;

                        var castActionId = FrameworkData.LocalPlayer.CastActionId;
                        labelOverride = Sheets.ActionSheet?.GetRowOrDefault(castActionId)?.Name.ToString() ?? " ";

                        iconOverride = Sheets.ActionSheet?.GetRowOrDefault(castActionId)?.Icon;
                        state = 1;
                        count = 1;
                    }
                    else if (preview != null)
                    {
                        maxGauge = 1;
                        gaugeValue = preview.Value;
                    }
                    break;
                }
                case GCD:
                    var groupDetail = ActionManager->GetRecastGroupDetail(57);
                    var totalGCD = groupDetail->Total;

                    if (totalGCD == 0)
                    {
                        maxGauge = LastKnownGCD;
                        gaugeValue = 0;
                    }
                    else
                    {
                        LastKnownGCD = totalGCD;
                        maxGauge = totalGCD;
                        gaugeValue = totalGCD - groupDetail->Elapsed;
                    }
                    state = gaugeValue > 0 ? 0 : 1;
                    count = state;
                    break;
                case Combo:
                    maxGauge = 30;
                    gaugeValue = ActionManager->Combo.Timer;
                    state = gaugeValue > 0 ? 1 : 0;
                    count = state;
                    break;
                default:
                    break;
            }
        }

        return new(count, maxCount, gaugeValue, maxGauge, state, maxState, preview) { HasLabelOverride = hasLabelOverride, LabelOverride = labelOverride, IconOverride = iconOverride};
    }
}
