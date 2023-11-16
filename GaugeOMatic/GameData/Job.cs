using System;
using static GaugeOMatic.GaugeOMatic.Service;

// ReSharper disable UnusedMember.Local
// ReSharper disable UnusedType.Global
// ReSharper disable UnusedMember.Global

namespace GaugeOMatic.GameData;

public class JobData
{
    public enum Job
    {
        None,
        MNK, DRG, NIN, SAM, RPR,
        BRD, MCH, DNC,
        BLM, SMN, RDM,
        PLD, WAR, DRK, GNB,
        WHM, SCH, AST, SGE,

        PGL, LNC, ROG, ARC, THM, ACN, GLA, MRD, CNJ
    }

    [Flags]
    public enum Role
    {
        None   =     0b0,
        Tank   =     0b1,
        Healer =    0b10,
        Melee  =   0b100,
        Ranged =  0b1000,
        Caster = 0b10000,
        All    = 0b11111
    }

    internal static int Current => LastKnown = (int)(ClientState.LocalPlayer?.ClassJob.Id ?? 0);
    internal static int LastKnown;
    public static string JobAbbr => ClientState.LocalPlayer?.ClassJob.GameData?.Abbreviation ?? "???";
    internal static bool JobChanged => LastKnown != Current;
}
