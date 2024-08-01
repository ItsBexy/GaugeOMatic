using System.Collections.Generic;

namespace GaugeOMatic.GameData;

internal static class Overrides
{
    // Conditional actions that I can't automatically detect yet,
    // but I happen to know they can get ants, so we'll hardcode that information for now
    public static List<uint> AntActions = new()
    {
        16458, 7384, 3542,   //PLD
        49, 51,              //WAR
        16162, 16155, 16163, //GNB

        62, 53, 74, 16474, 69, 25764, 25761, 25763, //MNK
        25773,                                      //DRG
        7402, 7401,                                 //NIN
        7490, 7491,                                 //SAM
        24389, 24398, 24392, 24385,                 //RPR
        34633,                                      //VPR

        16496, 98, 36974,                                //BRD
        7410,                                            //MCH todo: figure out why this isn't coming up as status-based
        15996, 16007, 16008, 15992, 15991, 15995, 16005, //DNC

        36989, 7422, 16507, //BLM
        34662,              // PCT

        16535, //WHM
        24304  //SGE
    };

    // Actions that are hidden from the plugin UI for any of the following reasons:
    // - if they are available no matter what (ie, the first step in a combo)
    // - if I haven't figured out an automated way to check their availability (ie, resource spenders that don't highlight such as SAM's Hagakure)
    // - if something is screwy with them
    public static List<uint> HiddenActions = new()
    {
        7568, 16560, 7557, 25880,             // Role
        3541, 9, 28, 16, 24, 7381,            // PLD
        48, 31, 41, 46,                       // WAR
        16467, 16466, 3629, 3617, 3621, 3624, // DRK
        16141, 16137, 16143, 16142,           // GNB

        4262, 36941, 16476, 3547,            // MNK
        86, 90, 75,                          // DRG
        2261, 2263, 2254, 2240, 2247, 36940, // NIN
        7483, 7495, 7477, 7867, 16483,       // SAM
        24378, 24373, 24387, 24376, 24379,   // RPR
        35920, 35922, 35921, 34632,          // VPR

        97, 3560, 106, 100, 113,                                // BRD
        2866, 2870,                                             // MCH
        15989, 15993, 16003, 16191, 16004, 16193, 16194, 16195, // DNC

        7419, 142, 25793, 154, 3576, 16505, 141, 147, 3577, 162, 159, 156, 149, 16506, // BLM
        25822, 3578,                                                                   // SMN
        7503, 7513, 16529, 7504, 7509, 7507, 16525, 7514, 7523, 7505, 16524,           // RDM
        34659, 34653, 34689, 34656, 34650, 34691, 34690, 34668,34664,34691,            // PCT

        16534, 16531, 131, 16532, 16533, 25859, 139, 37010, 137, 3568, 7431,            // WHM
        185, 7437, 16539, 17864, 167, 181, 25883, 189, 16511, 190, 16230, 25884, 17869, // SCH
        163, 17870, 186, 25798, 25804, 17215, 25802, 25803, 37037, 25800,               //     todo: sort out SCH/SMN overlap
        3603, 3595, 3601, 3594, 3599, 3615, 3600, 3596, 37022, 37019, 37020, 37021,     // AST
        24284, 24283, 24296, 24297, 24287, 24290, 24285, 24286                          // SGE
    };

    public static Dictionary<uint, string> ActionAliases = new()
    {
        { 2259, "Mudra" },
        { 2261, "Mudra" },
        { 2263, "Mudra" }
    };

    public static Dictionary<uint, string> StatusAliases = new()
    {
        { 638, "Vulnerability Up (Mug)" }
    };

    public static Dictionary<uint, int> StatusMaxes = new()
    {
        { 742,3}
    };
}
