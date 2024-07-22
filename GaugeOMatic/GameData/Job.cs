using System;
// ReSharper disable UnusedMember.Global

namespace GaugeOMatic.GameData;

public class JobData
{
    public enum Job
    {
        None = 0,
        MNK = 20, DRG = 22, NIN = 30, SAM = 34, RPR = 39, VPR = 41,
        BRD = 23, MCH = 31, DNC = 38,
        BLM = 25, SMN = 27, RDM = 35, PCT = 42,
        PLD = 19, WAR = 21, DRK = 32, GNB = 37,
        WHM = 24, SCH = 28, AST = 33, SGE = 40,

        PGL = 2, LNC = 4, ROG = 29, ARC = 5, THM = 7, ACN = 26, GLA = 1, MRD = 3, CNJ = 6,

        BLU = 36, BST = 43
    }

    public const byte LevelCap = 100;

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

    internal static Job Current => LastKnown = (Job)(ClientState.LocalPlayer?.ClassJob.Id ?? 0);
    internal static Job LastKnown;
    internal static bool JobChanged => LastKnown != Current;
}
