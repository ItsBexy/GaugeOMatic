using System;
using static GaugeOMatic.GameData.JobData.Job;
using static GaugeOMatic.GameData.JobData.Role;

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

        BLU = 36, BST = 43,

        CRP = 8, BSM = 9, ARM = 10, GSM = 11, LTW = 12, WVR = 13, ALC = 14, CUL = 15,
        MIN = 16, BTN = 17, FSH = 18
    }

    public static Job GetJobByCategory(uint cat) =>
        cat switch
        {
            20 => GLA,
            21 => MNK,
            22 => WAR,
            23 => DRG,
            24 => BRD,
            25 => WHM,
            26 => BLM,
            28 => SMN,
            29 => SCH,
            38 => GLA,
            41 => PGL,
            44 => MRD,
            47 => LNC,
            50 => ARC,
            55 => THM,
            69 => ACN,
            92 => NIN,
            93 => ROG,
            96 => MCH,
            98 => DRK,
            99 => AST,
            111 => SAM,
            112 => RDM,
            149 => GNB,
            150 => DNC,
            180 => RPR,
            181 => SGE,
            196 => VPR,
            197 => PCT,
            _ => Job.None
        };

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

    public static Role GetRoleByCategory(uint cat) =>
        cat switch
        {
            113 => Tank,
            114 => Melee,
            115 => Ranged,
            116 => Caster,
            117 => Healer,
            118 => Melee | Ranged,
            120 => Healer | Caster,
            161 => Melee | Tank | Ranged,
            _ => Role.None
        };

    internal static Job Current => LastKnown = (Job)(ClientState.LocalPlayer?.ClassJob.Id ?? 0);
    internal static Job LastKnown;
    internal static bool JobChanged => LastKnown != Current;

    public static uint GetJobIcon(Job job) =>
        job switch
        {
            MNK => 62402,
            DRG => 62404,
            SAM => 62414,
            RPR => 62419,
            VPR => 62421,
            BRD => 62405,
            MCH => 62411,
            DNC => 62418,
            BLM => 62407,
            SMN => 62408,
            RDM => 62415,
            PCT => 62422,
            PLD => 62401,
            WAR => 62403,
            DRK => 62412,
            GNB => 62417,
            WHM => 62406,
            SCH => 62409,
            AST => 62413,
            SGE => 62420,
            PGL => 62302,
            LNC => 62304,
            ROG => 62309,
            NIN => 62410,
            ARC => 62305,
            THM => 62307,
            ACN => 62308,
            GLA => 62301,
            MRD => 62303,
            CNJ => 62306,
            BLU => 62416,
            CRP => 62310,
            BSM => 62311,
            ARM => 62312,
            GSM => 62313,
            LTW => 62314,
            WVR => 62315,
            ALC => 62316,
            CUL => 62317,
            MIN => 62318,
            BTN => 62319,
            FSH => 62320,
            _ => 60071
        };
}
