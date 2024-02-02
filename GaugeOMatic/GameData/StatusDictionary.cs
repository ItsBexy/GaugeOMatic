using System.Collections.Generic;
using static GaugeOMatic.GameData.JobData;
using static GaugeOMatic.GameData.JobData.Job;
using static GaugeOMatic.GameData.JobData.Role;
using static GaugeOMatic.GameData.StatusData.StatusHolder;

namespace GaugeOMatic.GameData;

public abstract partial class StatusData
{
    public static readonly Dictionary<uint, StatusRef> Statuses = new()
    {
         #region Role/General

        { 2,    new(2,    Job.None, "Stun",                    Target, 1, 1,  Ranged                          ) },
        { 13,   new(13,   Job.None, "Bind",                    Target, 1, 7,  Ranged                          ) },
        { 14,   new(14,   Job.None, "Heavy",                   Target, 1, 10, Ranged                          ) },
        { 49,   new(49,   Job.None, "Medicated",               Self,   1, 30, All                             ) },
        { 50,   new(50,   Job.None, "Sprint",                  Self,   1, 10, All                             ) },
        { 84,   new(84,   Job.None, "Bloodbath",               Self,   1, 20, Melee                           ) },
        { 1191, new(1191, Job.None, "Rampart",                 Self,   1, 20, Tank                            ) },
        { 1193, new(1193, Job.None, "Reprisal",                Target, 1, 10, Tank                            ) },
        { 1195, new(1195, Job.None, "Feint",                   Target, 1, 10, Melee ) },
        { 1203, new(1203, Job.None, "Addle",                   Target, 1, 10, Caster                          ) },
        { 1204, new(1204, Job.None, "Lucid Dreaming",          Self,   1, 21, Caster | Healer            ) },
        { 1209, new(1209, Job.None, "Arm's Length",            Self,   1, 6,  Melee | Ranged | Tank ) },
        { 1250, new(1250, Job.None, "True North",              Self,   1, 10, Melee                           ) },
        { 160,  new(160,  Job.None, "Surecast",                Self,   1, 6,  Caster | Healer            ) },
        { 167,  new(167,  Job.None, "Swiftcast",               Self,   1, 10, Caster | Healer            ) },

         #endregion

         #region Melee

        { 102,  new(102,  MNK, "Mantra",                   Self,   1, 15 ) },
        { 107,  new(107,  MNK, "Opo-opo Form",             Self,   1, 30 ) },
        { 108,  new(108,  MNK, "Raptor Form",              Self,   1, 30 ) },
        { 109,  new(109,  MNK, "Coeurl Form",              Self,   1, 30 ) },
        { 110,  new(110,  MNK, "Perfect Balance",          Self,   3, 20 ) },
        { 246,  new(246,  MNK, "Demolish",                 Target, 1, 18 ) },
        { 1179, new(1179, MNK, "Riddle of Earth",          Self,   1, 10 ) },
        { 1181, new(1181, MNK, "Riddle of Fire",           Self,   1, 20 ) },
        { 1182, new(1182, MNK, "Meditative Brotherhood",   Self,   1, 15 ) },
        { 1185, new(1185, MNK, "Brotherhood",              Self,   1, 15 ) },
        { 1861, new(1861, MNK, "Leaden Fist",              Self,   1, 30 ) },
        { 1862, new(1862, MNK, "Anatman",                  Self,   1, 30 ) },
        { 2513, new(2513, MNK, "Formless Fist",            Self,   1, 30 ) },
        { 2514, new(2514, MNK, "Six-sided Star",           Self,   1, 5  ) },
        { 2687, new(2687, MNK, "Riddle of Wind",           Self,   1, 15 ) },
        { 3001, new(3001, MNK, "Disciplined Fist",         Self,   1, 15 ) },

        { 116,  new(116,  DRG, "Life Surge",               Self,   1, 5  ) },
        { 786,  new(786,  DRG, "Battle Litany",            Self,   1, 15 ) },
        { 802,  new(802,  DRG, "Fang and Claw Bared",      Self,   1, 30 ) },
        { 803,  new(803,  DRG, "Wheel in Motion",          Self,   1, 30 ) },
        { 1243, new(1243, DRG, "Dive Ready",               Self,   1, 15 ) },
        { 1863, new(1863, DRG, "Draconian Fire",           Self,   1, 30 ) },
        { 1864, new(1864, DRG, "Lance Charge",             Self,   1, 20 ) },
        { 1910, new(1910, DRG, "Right Eye",                Self,   1, 20 ) },
        { 2719, new(2719, DRG, "Chaotic Spring",           Target, 1, 24 ) },
        { 2720, new(2720, DRG, "Power Surge",              Self,   1, 30 ) },

        { 488,  new(488,  NIN, "Shade Shift",              Self,   1, 19 ) },
        { 496,  new(496,  NIN, "Mudra",                    Self,   1, 6  ) },
        { 497,  new(497,  NIN, "Kassatsu",                 Self,   1, 15 ) },
        { 501,  new(501,  NIN, "Doton",                    Self,   1, 18 ) },
        { 502,  new(502,  NIN, "Doton Heavy",              Target, 1, 5  ) },
        { 507,  new(507,  NIN, "Suiton",                   Self,   1, 20 ) },
        { 614,  new(614,  NIN, "Hidden",                   Self,   1, 0  ) },
        { 638,  new(638,  NIN, "Mug",                      Target, 1, 20 ) },
        { 1186, new(1186, NIN, "Ten Chi Jin",              Self,   1, 6  ) },
        { 1954, new(1954, NIN, "Bunshin",                  Self,   5, 30 ) },
        { 2689, new(2689, NIN, "Meisui",                   Self,   1, 30 ) },
        { 2690, new(2690, NIN, "Raiju Ready",              Self,   3, 30 ) },
        { 2723, new(2723, NIN, "Phantom Kamaitachi Ready", Self,   1, 45 ) },
        { 3254, new(3254, NIN, "Trick Attack",             Target, 1, 15 ) },

        { 1228, new(1228, SAM, "Higanbana",                Target, 1, 60 ) },
        { 1231, new(1231, SAM, "Meditate",                 Self,   1, 15 ) },
        { 1232, new(1232, SAM, "Third Eye",                Self,   1, 4  ) },
        { 1233, new(1233, SAM, "Meikyo Shisui",            Self,   3, 15 ) },
        { 1236, new(1236, SAM, "Enhanced Enpi",            Self,   1, 15 ) },
        { 1298, new(1298, SAM, "Fugetsu",                  Self,   1, 40 ) },
        { 1299, new(1299, SAM, "Fuka",                     Self,   1, 40 ) },
        { 2959, new(2959, SAM, "Ogi Namikiri Ready",       Self,   1, 30 ) },

        { 2586, new(2586, RPR, "Death's Design",           Target, 1, 30 ) },
        { 2587, new(2587, RPR, "Soul Reaver",              Self,   2, 30 ) },
        { 2588, new(2588, RPR, "Enhanced Gibbet",          Self,   1, 60 ) },
        { 2589, new(2589, RPR, "Enhanced Gallows",         Self,   1, 60 ) },
        { 2590, new(2590, RPR, "Enhanced Void Reaping",    Self,   1, 30 ) },
        { 2591, new(2591, RPR, "Enhanced Cross Reaping",   Self,   1, 30 ) },
        { 2592, new(2592, RPR, "Immortal Sacrifice",       Self,   8, 30 ) },
        { 2593, new(2593, RPR, "Enshrouded",               Self,   1, 30 ) },
        { 2594, new(2594, RPR, "Soulsow",                  Self,   1, 0  ) },
        { 2595, new(2595, RPR, "Threshold",                Self,   1, 10 ) },
        { 2597, new(2597, RPR, "Crest of Time Borrowed",   Self,   1, 5, Role.None, new(2596)) },
        { 2598, new(2598, RPR, "Crest of Time Returned",   Self,   1, 15  ) },
        { 2599, new(2599, RPR, "Arcane Circle",            Self,   1, 20 ) },
        { 2600, new(2600, RPR, "Circle of Sacrifice",      Self,   1, 5  ) },
        { 2845, new(2845, RPR, "Enhanced Harpe",           Self,   1, 20 ) },
        { 2972, new(2972, RPR, "Bloodsown Circle",         Self,   1, 6  ) },

         #endregion

         #region Ranged

        { 122,  new(122,  BRD, "Straight Shot Ready",      Self,   1, 30 ) },
        { 125,  new(125,  BRD, "Raging Strikes",           Self,   1, 20 ) },
        { 128,  new(128,  BRD, "Barrage",                  Self,   1, 10 ) },
        { 141,  new(141,  BRD, "Battle Voice",             Self,   1, 15 ) },
        { 866,  new(866,  BRD, "The Warden's Paean",       Self,   1, 30 ) },
        { 1200, new(1200, BRD, "Caustic Bite",             Target, 1, 45 , seeAlso: new() { 124 } ) },
        { 1201, new(1201, BRD, "Stormbite",                Target, 1, 45 , seeAlso: new() { 129 }) },
        { 1202, new(1202, BRD, "Nature's Minne",           Self,   1, 15 ) },
        { 1932, new(1932, BRD, "Army's Muse",              Self,   1, 10 ) },
        { 1934, new(1934, BRD, "Troubadour",               Self,   1, 15 ) },
        { 2216, new(2216, BRD, "The Wanderer's Minuet",    Self,   1, 5  ) },
        { 2217, new(2217, BRD, "Mage's Ballad",            Self,   1, 5  ) },
        { 2218, new(2218, BRD, "Army's Paeon",             Self,   1, 5  ) },
        { 2692, new(2692, BRD, "Blast Arrow Ready",        Self,   1, 10 ) },
        { 2722, new(2722, BRD, "Radiant Finale",           Self,   1, 15 ) },
        { 2964, new(2964, BRD, "Radiant Finale",           Self,   1, 15 ) },
        { 3002, new(3002, BRD, "Shadowbite Ready",         Self,   1, 30 ) },

        { 851,  new(851,  MCH, "Reassembled",              Self,   1, 5  ) },
        { 860,  new(860,  MCH, "Dismantled",              Target,   1, 5 ) },
        { 1205, new(1205, MCH, "Flamethrower",             Self,   1, 10 ) },
        { 1866, new(1866, MCH, "Bioblaster",               Target, 1, 15 ) },
        { 1946, new(1946, MCH, "Wildfire",                 Self,   1, 10, seeAlso: new() { 861 } ) },
        { 1951, new(1951, MCH, "Tactician",                Self,   1, 15 ) },
        { 2688, new(2688, MCH, "Overheated",               Self,   5, 10 ) },

        { 1818, new(1818, DNC, "Standard Step",            Self,   1, 15 ) },
        { 1819, new(1819, DNC, "Technical Step",           Self,   1, 15 ) },
        { 1820, new(1820, DNC, "Threefold Fan Dance",      Self,   1, 30 ) },
        { 1821, new(1821, DNC, "Standard Finish",          Self,   1, 60 ) },
        { 1822, new(1822, DNC, "Technical Finish",         Self,   1, 20 ) },
        { 1825, new(1825, DNC, "Devilment",                Self,   1, 20 ) },
        { 1826, new(1826, DNC, "Shield Samba",             Self,   1, 15 ) },
        { 1827, new(1827, DNC, "Improvisation",            Self,   1, 15 ) },
        { 1847, new(1847, DNC, "Esprit",                   Self,   1, 60 ) },
        { 2693, new(2693, DNC, "Silken Symmetry",          Self,   1, 30 ) },
        { 2694, new(2694, DNC, "Silken Flow",              Self,   1, 30 ) },
        { 2695, new(2695, DNC, "Improvisation",            Self,   1, 15 ) },
        { 2698, new(2698, DNC, "Flourishing Finish",       Self,   1, 30 ) },
        { 2699, new(2699, DNC, "Fourfold Fan Dance",       Self,   1, 30 ) },
        { 2700, new(2700, DNC, "Flourishing Starfall",     Self,   1, 20 ) },
        { 3017, new(3017, DNC, "Flourishing Symmetry",     Self,   1, 30 ) },
        { 3018, new(3018, DNC, "Flourishing Flow",         Self,   1, 30 ) },

         #endregion

         #region Caster

        { 163,  new(163,  BLM, "Thunder",                  Target, 1, 30 , seeAlso: new() { 161 } ) },
        { 164,  new(164,  BLM, "Thundercloud",             Self,   1, 40 ) },
        { 165,  new(165,  BLM, "Firestarter",              Self,   1, 30 ) },
        { 168,  new(168,  BLM, "Manaward",                 Self,   1, 20 ) },
        { 737,  new(737,  BLM, "Ley Lines",                Self,   1, 30 ) },
        { 738,  new(738,  BLM, "Circle of Power",          Self,   1, 5  ) },
        { 867,  new(867,  BLM, "Sharpcast",                Self,   1, 30 ) },
        { 1210, new(1210, BLM, "Thunder IV",               Target, 1, 18 ) },
        { 1211, new(1211, BLM, "Triplecast",               Self,   3, 15 ) },

        { 304,  new(304,  SMN, "Aetherflow",               Self,   3, 0  ) },
        { 1868, new(1868, SMN, "Everlasting Flight",       Self,   1, 21 ) },
        { 2701, new(2701, SMN, "Further Ruin",             Self,   1, 60 ) },
        { 2702, new(2702, SMN, "Radiant Aegis",            Self,   1, 30 ) },
        { 2703, new(2703, SMN, "Searing Light",            Self,   1, 30 ) },
        { 2704, new(2704, SMN, "Rekindle",                 Self,   1, 30 ) },
        { 2705, new(2705, SMN, "Undying Flame",            Self,   1, 15 ) },
        { 2706, new(2706, SMN, "Slipstream",               Self,   1, 15 ) },
        { 2724, new(2724, SMN, "Ifrit's Favor",            Self,   1, 0  ) },
        { 2725, new(2725, SMN, "Garuda's Favor",           Self,   1, 0  ) },
        { 2853, new(2853, SMN, "Titan's Favor",            Self,   1, 0  ) },

        { 1234, new(1234, RDM, "Verfire Ready",            Self,   1, 30 ) },
        { 1235, new(1235, RDM, "Verstone Ready",           Self,   1, 30 ) },
        { 1238, new(1238, RDM, "Acceleration",             Self,   1, 20 ) },
        { 1239, new(1239, RDM, "Embolden",                 Self,   1, 20 ) },
        { 1249, new(1249, RDM, "Dualcast",                 Self,   1, 15 ) },
        { 1971, new(1971, RDM, "Manafication",             Self,   6, 15 ) },
        { 2707, new(2707, RDM, "Magick Barrier",           Self,   1, 10 ) },

         #endregion

         #region Tank

        { 74,   new(74,   PLD, "Sentinel",                 Self,   1, 15 ) },
        { 76,   new(76,   PLD, "Fight or Flight",          Self,   1, 20 ) },
        { 77,   new(77,   PLD, "Bulwark",                  Self,   1, 10 ) },
        { 79,   new(79,   PLD, "Iron Will",                Self,   1, 0  ) },
        { 248,  new(248,  PLD, "Circle of Scorn",          Target, 1, 15 ) },
        { 1175, new(1175, PLD, "Passage of Arms",          Self,   1, 18 ) },
        { 1362, new(1362, PLD, "Divine Veil",              Self,   1, 30 ) },
        { 1368, new(1368, PLD, "Requiescat",               Self,   4, 30 ) },
        { 1902, new(1902, PLD, "Sword Oath",               Self,   3, 30 ) },
        { 2673, new(2673, PLD, "Divine Might",             Self,   1, 30 ) },
        { 2674, new(2674, PLD, "Sheltron",                 Self,   1, 8  , seeAlso: new() { 728, 1856 }) },
        { 2675, new(2675, PLD, "Knight's Resolve",         Self,   1, 4  ) },
        { 2676, new(2676, PLD, "Knight's Benediction",     Self,   1, 12 ) },
        { 3019, new(3019, PLD, "Confiteor Ready",          Self,   1, 30 ) },

        { 87,   new(87,   WAR, "Thrill of Battle",         Self,   1, 10 ) },
        { 89,   new(89,   WAR, "Vengeance",                Self,   1, 15 ) },
        { 91,   new(91,   WAR, "Defiance",                 Self,   1, 0  ) },
        { 1177, new(1177, WAR, "Inner Release",            Self,   3, 15 , seeAlso: new() { 86 }) },
        { 1457, new(1457, WAR, "Shake It Off",             Self,   1, 30 ) },
        { 1897, new(1897, WAR, "Nascent Chaos",            Self,   1, 30 ) },
        { 2108, new(2108, WAR, "Shake It Off (Over Time)", Self,   1, 15) },
        { 2624, new(2624, WAR, "Primal Rend Ready",        Self,   1, 30 ) },
        { 2663, new(2663, WAR, "Inner Strength",           Self,   1, 15 ) },
        { 2677, new(2677, WAR, "Surging Tempest",          Self,   1, 60 , seeAlso: new() { 735 }) },
        { 2678, new(2678, WAR, "Bloodwhetting",            Self,   1, 8  ) },
        { 2679, new(2679, WAR, "Stem the Flow",            Self,   1, 4  ) },
        { 2680, new(2680, WAR, "Stem the Tide",            Self,   1, 20 ) },

        { 742,  new(742,  DRK, "Blood Weapon",             Self,   5, 15 ) },
        { 743,  new(743,  DRK, "Grit",                     Self,   1, 0  ) },
        { 746,  new(746,  DRK, "Dark Mind",                Self,   1, 10 ) },
        { 747,  new(747,  DRK, "Shadow Wall",              Self,   1, 15 ) },
        { 749,  new(749,  DRK, "Salted Earth",             Self,   1, 15 ) },
        { 810,  new(810,  DRK, "Living Dead",              Self,   1, 10 ) },
        { 1178, new(1178, DRK, "Blackest Night",           Self,   1, 7  ) },
        { 1894, new(1894, DRK, "Dark Missionary",          Self,   1, 15 ) },
        { 1972, new(1972, DRK, "Delirium",                 Self,   3, 15 ) },
        { 2682, new(2682, DRK, "Oblation",                 Self,   1, 10 ) },

        { 1831, new(1831, GNB, "No Mercy",                 Self,   1, 20 ) },
        { 1832, new(1832, GNB, "Camouflage",               Self,   1, 20 ) },
        { 1833, new(1833, GNB, "Royal Guard",              Self,   1, 0  ) },
        { 1834, new(1834, GNB, "Nebula",                   Self,   1, 15 ) },
        { 1835, new(1835, GNB, "Aurora",                   Self,   1, 18 ) },
        { 1836, new(1836, GNB, "Superbolide",              Self,   1, 10 ) },
        { 1837, new(1837, GNB, "Sonic Break",              Target, 1, 30 ) },
        { 1838, new(1838, GNB, "Bow Shock",                Target, 1, 15 ) },
        { 1839, new(1839, GNB, "Heart of Light",           Self,   1, 15 ) },
        { 1842, new(1842, GNB, "Ready to Rip",             Self,   1, 10 ) },
        { 1843, new(1843, GNB, "Ready to Tear",            Self,   1, 10 ) },
        { 1844, new(1844, GNB, "Ready to Gouge",           Self,   1, 10 ) },
        { 1898, new(1898, GNB, "Brutal Shell",             Self,   1, 30 ) },
        { 2683, new(2683, GNB, "Heart of Corundum",        Self,   1, 8 , seeAlso: new() { 1840 }) },
        { 2684, new(2684, GNB, "Clarity of Corundum",      Self,   1, 4  ) },
        { 2685, new(2685, GNB, "Catharsis of Corundum",    Self,   1, 20 ) },
        { 2686, new(2686, GNB, "Ready to Blast",           Self,   1, 10 ) },

         #endregion

         #region Healer

        { 3,    new(3,    WHM, "Sleep",                    Target, 1, 30 ) },
        { 150,  new(150,  WHM, "Medica II",                Self,   1, 15 ) },
        { 155,  new(155,  WHM, "Freecure",                 Self,   1, 15 ) },
        { 157,  new(157,  WHM, "Presence of Mind",         Self,   1, 15 ) },
        { 158,  new(158,  WHM, "Regen",                    Self,   1, 18 ) },
        { 1217, new(1217, WHM, "Thin Air",                 Self,   1, 12 ) },
        { 1218, new(1218, WHM, "Divine Benison",           Self,   1, 15 ) },
        { 1219, new(1219, WHM, "Confession",               Self,   1, 10 ) },
        { 1871, new(1871, WHM, "Dia",                      Target, 1, 30, seeAlso: new() { 143, 144, 798 }) },
        { 1872, new(1872, WHM, "Temperance",               Self,   1, 20, seeAlso: new() { 1873 } ) },
        { 1911, new(1911, WHM, "Asylum",                   Self,   1, 24, seeAlso: new() { 1912 } ) },
        { 2708, new(2708, WHM, "Aquaveil",                 Self,   1, 8  ) },
        { 2709, new(2709, WHM, "Liturgy of the Bell",      Self,   5, 20 ) },

        { 297,  new(297,  SCH, "Galvanize",                Self,   1, 30 ) },
        { 315,  new(315,  SCH, "Whispering Dawn",          Self,   1, 21 ) },
        { 317,  new(317,  SCH, "Fey Illumination",         Self,   1, 20 ) },
        { 791,  new(791,  SCH, "Dissipation",              Self,   1, 30 ) },
        { 792,  new(792,  SCH, "Emergency Tactics",        Self,   1, 15 ) },
        { 1220, new(1220, SCH, "Excogitation",             Self,   1, 45 ) },
        { 1221, new(1221, SCH, "Chain Stratagem",          Target, 1, 15 ) },
        { 1223, new(1223, SCH, "Fey Union",                Self,   1, 60 ) },
        { 1895, new(1895, SCH, "Biolysis",                 Target, 1, 30 , seeAlso: new() { 189, 179 }) },
        { 1896, new(1896, SCH, "Recitation",               Self,   1, 15 ) },
        { 1917, new(1917, SCH, "Seraphic Veil",            Self,   1, 30 ) },
        { 2710, new(2710, SCH, "Protraction",              Self,   1, 10 ) },

        { 835,  new(835,  AST, "Aspected Benefic",         Self,   1, 15 ) },
        { 836,  new(836,  AST, "Aspected Helios",          Self,   1, 15 ) },
        { 841,  new(841,  AST, "Lightspeed",               Self,   1, 15 ) },
        { 848,  new(848,  AST, "Collective Unconscious",   Self,   1, 18 ) },
        { 849,  new(849,  AST, "Collective Unconscious",   Self,   1, 5  ) },
        { 913,  new(913,  AST, "Balance Drawn",            Self          ) },
        { 914,  new(914,  AST, "Bole Drawn",               Self          ) },
        { 915,  new(915,  AST, "Arrow Drawn",              Self          ) },
        { 916,  new(916,  AST, "Spear Drawn",              Self          ) },
        { 917,  new(917,  AST, "Ewer Drawn",               Self          ) },
        { 918,  new(918,  AST, "Spire Drawn",              Self          ) },
        { 956,  new(956,  AST, "Wheel of Fortune",         Self,   1, 15 ) },
        { 1224, new(1224, AST, "Earthly Dominance",        Self,   1, 10 ) },
        { 1248, new(1248, AST, "Giant Dominance",          Self,   1, 10 ) },
        { 1878, new(1878, AST, "Divination",               Self,   1, 15 ) },
        { 1879, new(1879, AST, "Opposition",               Self,   1, 15 ) },
        { 1881, new(1881, AST, "Combust",                  Target, 1, 30 , seeAlso: new() { 838, 843 }) },
        { 1882, new(1883, AST, "The Balance",              Self,   1, 15 ) },
        { 1883, new(1883, AST, "The Bole",                 Self,   1, 15 ) },
        { 1884, new(1884, AST, "The Arrow",                Self,   1, 15 ) },
        { 1885, new(1885, AST, "The Spear",                Self,   1, 15 ) },
        { 1886, new(1886, AST, "The Ewer",                 Self,   1, 15 ) },
        { 1887, new(1887, AST, "The Spire",                Self,   1, 15 ) },
        { 1889, new(1889, AST, "Intersection",             Self,   1, 30 ) },
        { 1890, new(1890, AST, "Horoscope",                Self,   1, 10 ) },
        { 1892, new(1892, AST, "Neutral Sect",             Self,   1, 20 ) },
        { 1921, new(1921, AST, "Neutral Sect",             Self,   1, 30 ) },
        { 2054, new(2054, AST, "Lord of Crowns Drawn",     Self          ) },
        { 2055, new(2055, AST, "Lady of Crowns Drawn",     Self          ) },
        { 2713, new(2713, AST, "Clarifying Draw",          Self,   1, 30 ) },
        { 2714, new(2714, AST, "Harmony of Spirit",        Self,   1, 15 ) },
        { 2715, new(2715, AST, "Harmony of Body",          Self,   1, 15 ) },
        { 2717, new(2717, AST, "Exaltation",               Self,   1, 8  ) },
        { 2718, new(2718, AST, "Macrocosmos",              Self,   1, 15 ) },

        { 2604, new(2604, SGE, "Kardia",                   Self,   1, 60 ) },
        { 2605, new(2605, SGE, "Kardion",                  Self,   1, 60 ) },
        { 2606, new(2606, SGE, "Eukrasia",                 Self,   1, 0  ) },
        { 2607, new(2607, SGE, "Eukrasian Diagnosis",      Self,   1, 30 ) },
        { 2609, new(2609, SGE, "Eukrasian Prognosis",      Self,   1, 30 ) },
        { 2610, new(2610, SGE, "Soteria",                  Self,   4, 15 ) },
        { 2611, new(2611, SGE, "Zoe",                      Self,   1, 30 ) },
        { 2612, new(2612, SGE, "Haima",                    Self,   1, 15 ) },
        { 2613, new(2613, SGE, "Panhaima",                 Self,   1, 15 ) },
        { 2616, new(2616, SGE, "Eukrasian Dosis",          Target, 1, 30 , seeAlso: new() { 2614, 2615 }) },
        { 2618, new(2618, SGE, "Kerachole",                Self,   1, 15 ) },
        { 2619, new(2619, SGE, "Taurochole",               Self,   1, 15 ) },
        { 2620, new(2620, SGE, "Physis",                   Self,   1, 15 , seeAlso: new() { 2617 }) },
        { 2621, new(2621, SGE, "Autophysis",               Self,   1, 10 ) },
        { 2622, new(2622, SGE, "Krasis",                   Self,   1, 10 ) },
        { 2642, new(2642, SGE, "Haimatinon",               Self,   5, 15 ) },
        { 2643, new(2643, SGE, "Panhaimatinon",            Self,   5, 15 ) },
        { 2938, new(2938, SGE, "Kerakeia",                 Self,   1, 15 ) },
        { 3003, new(3003, SGE, "Holos",                    Self,   1, 20 ) },
        { 3365, new(3365, SGE, "Holosakos",                Self,   1, 30 ) },

        #endregion
    };

   /* public static void StatusHarvest(Configuration configuration)
    {
        if (PlayerStatus != null)
        {
            foreach (var desc in from status in PlayerStatus.Where(static s => s.StatusId != 0)
                                 let desc = $"{ClientState.LocalPlayer!.ClassJob.GameData!.Abbreviation},{ status.StatusId},{ status.GameData.Name}, Self,{status.GameData.MaxStacks},{status.RemainingTime}"
                                 where configuration.StatusCollection.TryAdd(status.StatusId, desc)
                                 select desc)
            {
                Log.Warning(desc);
                configuration.Save();
            }
        }

        if (TargetStatus != null)
        {
            foreach (var desc in from status in TargetStatus.Where(static s => s.StatusId != 0)
                                 let desc = $"{ClientState.LocalPlayer!.ClassJob.GameData!.Abbreviation},{ status.StatusId},{ status.GameData.Name}, Target,{status.GameData.MaxStacks},{status.RemainingTime}"
                                 where configuration.StatusCollection.TryAdd(status.StatusId, desc)
                                 select desc)
            {
                Log.Warning(desc);
                configuration.Save();
            }
        }
    }*/

}
