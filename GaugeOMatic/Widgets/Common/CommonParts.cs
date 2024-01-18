using static CustomNodes.CustomNodeManager;

namespace GaugeOMatic.Widgets.Common;
internal static class CommonParts
{
    public static CustomPartsList BLM0Parts =>
        new ("ui/uld/JobHudBLM0.tex",
             new(0,0,162,144),   // 0  moon
             new(290,0,52,52),   // 1  blue orb
             new(290,52,52,52),  // 2  red orb
             new(342,0,20,48),   // 3  blue crystal
             new(342,48,20,48),  // 4  red crystal
             new(182,236,24,68), // 5  icicle
             new(342,96,20,48),  // 6  blue glow
             new(342,144,20,48), // 7  red glow
             new(162,0,128,124), // 8  lattice
             new(0,146,90,90),   // 9  curved plate
             new(95,153,80,78),  // 10 bar fill
             new(180,146,90,90), // 11 bar backdrop
             new(362,0,30,128),  // 12 clock hand
             new(324,192,30,46), // 13 diamond
             new(354,192,30,46), // 14 diamond glow
             new(0,236,90,90),   // 15 flash
             new(90,236,68,68),  // 16 eclipse
             new(290,104,46,46), // 17 halo
             new(362,128,28,28), // 18 glowball
             new(270,150,54,83), // 19 diamond frame
             new(158,236,24,68), // 20 icicle glow
             new(206,236,64,36), // 21 text bg
             new(206,272,32,32), // 22 simple icon thingy
             new(270,233,32,42), // 23 diamond cover
             new(302,238,52,52), // 24 grey orb
             new(307,290,85,36), // 25 double pointy
             new(0,324,86,40),   // 26 null paradox gem
             new(86,324,86,40),  // 27 active paradox gem
             new(172,324,86,40), // 28 paradox glow
             new(90,306,29,18),  // 29 blue sparkles
             new(119,306,29,36)  // 30 red sparkles
        );

    public static CustomPartsList RPR0Parts =>
        new ("ui/uld/JobHudRRP0.tex",
             new(0,96,264,100), // 0  scytheblade
             new(0,42,206,52),  // 1  bottom right frame
             new(207,1,8,40),   // 2  middle ticks
             new(216,0,34,38),  // 3  corner ticks
             new(216,38,10,4),  // 4  ??? tiny line
             new(250,0,12,12),  // 5  tiny box
             new(205,70,18,12), // 6  bw gradient
             new(1,1,181,12),   // 7  backdrop
             new(2,16,180,10),  // 8  red bar
             new(2,30,180,10),  // 9  teal bar
             new(250,12,14,16), // 10 white spot
             new(207,43,26,26), // 11 frame
             new(2,198,180,10), // 12 grey bar
             new(234,38,10,12), // 13 tickmark
             new(0,215,184,20), // 14 glow frame
             new(0,240,162,56), // 15 glow scythe
             new(206,82,58,14)  // 16 streaks
        );

    public static CustomPartsList MCH0Parts =>
        new ("ui/uld/jobhudmch0.tex",
             new (0,0,60,26),      // 0  dark tab
             new (60,0,60,26),     // 1  red tab
             new (0,32,192,46),    // 2  barrel
             new (0,78,200,72),    // 3  parallelogram
             new (148,150,64,64),  // 4  heatclock
             new (148,214,68,64),  // 5  batteryclock
             new (212,150,64,64),  // 6  blue ring
             new (0,330,228,78),   // 7  heat glow
             new (216,214,48,48),  // 8  eye shaped glow thing?
             new (148,278,72,40),  // 9  tab border
             new (0,150,148,36),   // 10 bar bg
             new (0,186,148,36),   // 11 blue bar
             new (0,222,148,36),   // 12 red bar
             new (0,258,148,36),   // 13 white bar
             new (0,294,148,36),   // 14 grey bar
             new (192,0,64,42),    // 15 text bg
             new (212,42,64,85),   // 16 smoke
             new (200,42,12,37),   // 17 needle?
             new (160,0,32,32),    // 18 simple heat icon
             new (276,0,62,124),   // 19 lightning1
             new (276,124,62,124), // 20 lightning2
             new (128,0,32,32));   // 21 simple battery icon
}
