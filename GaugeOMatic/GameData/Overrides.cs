using System;
using System.Collections.Generic;
using static GaugeOMatic.GameData.ActionFlags;
using static GaugeOMatic.GameData.JobData.Role;

namespace GaugeOMatic.GameData;

internal static class Overrides
{
    // Conditional actions that I can't automatically detect yet,
    // but I happen to know they can get ants, so we'll hardcode that information for now
    public static readonly List<uint> AntActions = new()
    {
        #region Tank

        // PLD
        15,
        21,
        27,
        3538,
        3539,
        3541,
        3542,
        7382,
        7384,
        16457,
        16458,
        16459,
        16460,
        25746,
        25748,
        25749,
        25750,
        36918,
        36919,

        // WAR
        37,
        42,
        45,
        49,
        51,
        3549,
        3550,
        16462,
        16463,
        16465,
        25753,

        // GNB
        16139,
        16145,
        16146,
        16147,
        16149,
        16150,
        16153,
        16155,
        16156,
        16157,
        16158,
        16162,
        16163,
        25759,
        25760,
        36936,
        36937,
        36938,
        36939,

        // DRK
        3623,
        3632,
        7391,
        7392,
        16468,
        25755,

        #endregion

        #region Melee

        // MNK
        53,
        54,
        56,
        61,
        62,
        66,
        69,
        70,
        74,
        3545,
        3547,
        16473,
        16474,
        25761,
        25763,
        25764,
        25767,
        25768,
        25769,
        25772,
        36944,

        // DRG
        78,
        84,
        87,
        88,
        3554,
        3556,
        7397,
        7399,
        7400,
        16477,
        16479,
        25770,
        25771,
        25772,
        25773,
        36952,

        // NIN
        2242,
        2255,
        2258,
        3563,
        7401,
        7402,
        16488,
        16489,
        16493,
        25774,
        25777,
        25778,
        36958,
        36959,
        36960,
        36961,

        // SAM
        7478,
        7479,
        7480,
        7481,
        7482,
        7484,
        7485,
        7486,
        7490,
        7491,
        7492,
        7493,
        7496,
        16481,
        16487,
        25781,
        25782,
        36964,
        36967,
        36968,

        // RPR
        24374,
        24375,
        24377,
        24382,
        24383,
        24384,
        24385,
        24386,
        24388,
        24389,
        24390,
        24391,
        24392,
        24393,
        24394,
        24395,
        24396,
        24397,
        24398,
        24399,
        24400,
        24403,

        // VPR
        34606,
        34607,
        34608,
        34609,
        34610,
        34611,
        34612,
        34613,
        34614,
        34615,
        34616,
        34617,
        34618,
        34619,
        34621,
        34622,
        34624,
        34625,
        34626,
        34627,
        34628,
        34629,
        34630,
        34631,
        34633,
        34634,
        34635,
        34636,
        34637,
        34638,
        34639,
        34640,
        34642,
        34643,
        34644,
        34645,
        34651,

        #endregion

        #region Ranged

        // BRD
        98,
        7404,
        7409,
        16496,
        25784,
        25785,
        36974,
        36976,
        36977,
        16494,

        // MCH
        2868,
        2873,
        7410,
        7412,
        7413,
        16497,
        17209,
        36978,

        // DNC
        15990,
        15991,
        15992,
        15995,
        15996,
        16005,
        16007,
        16008,
        16009,
        16192,
        16196,
        25789,
        25790,
        25791,
        25792,
        36983,
        36984,
        36985,

        #endregion

        #region Caster

        // BLM
        144,
        152,
        153,
        7420,
        7422,
        7447,
        16507,
        25797,
        36986,
        36987,
        36989,

        // SMN
        3582,
        7426,
        25823,
        25824,
        25825,
        25830,
        25832,
        25833,
        25834,
        25835,
        25836,
        25837,
        25838,
        25839,
        25840,
        25885,

        // RDM
        7510,
        7511,
        7512,
        7516,
        7525,
        7526,
        7528,
        7529,
        16530,
        25858,
        37002,
        37003,
        37005,
        37006,
        37007,

        // PCT
        34652,
        34653,
        34654,
        34655,
        34657,
        34658,
        34659,
        34660,
        34661,
        34662,
        34663,
        34670,
        34671,
        34672,
        34673,
        34674,
        34675,
        34676,
        34677,
        34678,
        34679,
        34680,
        34681,
        34683,
        34686,
        34688,

        #endregion

        #region Healer

        // WHM
        135,
        16535,
        28509,

        // SGE
        24291,
        24304,
        24314,
        24316,
        30734,
        37032

        #endregion
    };

    // Actions that are hidden from the plugin UI for any of the following reasons:
    // - if they are available no matter what (ie, the first step in a combo)
    // - if I haven't figured out an automated way to check their availability (ie, resource spenders that don't highlight)
    // - if something is screwy with them
    public static List<uint> HiddenActions = new()
    {
        2261,2263,            // NIN
        16003, 16191, 16004, 16193, 16194, 16195, // DNC
        37018 // AST
    };

    public static Dictionary<uint, Action<ActionRef>> ActionOverrideFuncs = new()
    {
        {3, static a => { a.Icon = 104; a.Role = All; a.SetFlag(Unassignable,false); } } // sprint
    };

    public static Dictionary<uint, string> ActionAliases = new()
    {
        { 2259, "Mudra" },
        { 2261, "Mudra" },
        { 2263, "Mudra" },
        { 37017, "Astral / Umbral Draw" }, // todo: deal with this in some way
        { 37018, "Astral / Umbral Draw" }
    };

    public static Dictionary<uint, string> StatusAliases = new()
    {
        { 638, "Vulnerability Up (Mug)" }
    };

    public static Dictionary<uint, int> StatusMaxes = new()
    {
        { 742,3 }
    };
}
