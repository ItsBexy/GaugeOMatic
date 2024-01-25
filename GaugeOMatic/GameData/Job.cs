using System;
// ReSharper disable UnusedMember.Global

namespace GaugeOMatic.GameData;

public class JobData
{
    public enum Job
    {
        None,
        MNK, DRG, NIN, SAM, RPR, VPR,
        BRD, MCH, DNC,
        BLM, SMN, RDM, PCT,
        PLD, WAR, DRK, GNB,
        WHM, SCH, AST, SGE,

        PGL, LNC, ROG, ARC, THM, ACN, GLA, MRD, CNJ,

        BLU, BST
    }

    [Flags]
    public enum Role
    {
        None     = 0x00,
        Tank     = 0x01,
        Healer   = 0x02,
        Melee    = 0x04,
        Ranged   = 0x08,
        Caster   = 0x10,
        Limited  = 0x20,
        Crafter  = 0x40,
        Gatherer = 0x80,
        Combat   = 0x3F,
        All      = 0xFF
    }

    internal static int Current => LastKnown = (int)(ClientState.LocalPlayer?.ClassJob.Id ?? 0);
    internal static int LastKnown;
    public static string JobAbbr => ClientState.LocalPlayer?.ClassJob.GameData?.Abbreviation ?? "???";
    internal static bool JobChanged => LastKnown != Current;
}
