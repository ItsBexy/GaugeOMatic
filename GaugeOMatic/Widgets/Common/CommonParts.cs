using System.Numerics;
using static CustomNodes.CustomNodeManager;

namespace GaugeOMatic.Widgets.Common;
internal static class CommonParts
{
    #region Shared (Dispose with plugin)

    public static CustomPartsList BgPart { get; } = new("ui/uld/JobHudNumBg.tex", new Vector4(0, 0, 60, 40));

    public static void DisposeSharedParts()
    {
        BgPart.Dispose();
    }

    #endregion

    #region Instanced (dispose with Widget)

    public static CustomPartsList CircleMask => new("ui/uld/navimap_mask.tex",
                                                     new(0, 0, 176, 176),
                                                     new(84, 0, 8, 20));

    public static CustomPartsList SquareMask => new("ui/uld/bgpartsmask.tex", new Vector4(0, 0, 32, 32));

    public static CustomPartsList BarMask => new("ui/uld/fgs_mask.tex",
                                                 new Vector4(0, 0, 128, 16),
                                                 new Vector4(0, 0, 16, 16),
                                                 new Vector4(112, 0, 16, 16));

    #region JobHud

    #region Melee

    public static CustomPartsList DRG0 =>
        new("ui/uld/JobHudDRG0.tex",
            new(0, 0, 320, 108),    // 0 teal dragon bg
            new(0, 108, 320, 100),  // 1 grey dragon
            new(0, 208, 244, 40),   // 2 bar top
            new(356, 208, 36, 36),  // 3 open eyeball
            new(320, 0, 100, 84),   // 4 scales1
            new(320, 80, 100, 48),  // 5 scales2
            new(380, 124, 40, 84),  // 6 smear
            new(328, 124, 44, 84),  // 7 two smears
            new(392, 208, 36, 36),  // 8 black spot
            new(244, 296, 40, 28),  // 9 white eye glow
            new(244, 248, 96, 48),  // 10 eye frame
            new(0, 248, 244, 40),   // 11 bar bottom
            new(0, 288, 245, 40),   // 12 spear
            new(0, 328, 245, 40),   // 13 chevron scales
            new(308, 384, 56, 68),  // 14 pointy thrust effect
            new(340, 248, 68, 32),  // 15 eye closed
            new(340, 280, 68, 32),  // 16 eye semi open with pupil
            new(340, 312, 68, 32),  // 17 eye semi open no pupil
            new(340, 344, 68, 32),  // 18 eyeopen
            new(1, 369, 152, 18),   // 19 white bar glossy
            new(1, 389, 152, 18),   // 20 black bar
            new(155, 389, 152, 18), // 21 white bar flat
            new(1, 409, 70, 42),    // 22 2 frames
            new(1, 409, 21, 42),    // 23 left frame endcap
            new(22, 409, 24, 42),   // 24 frame connector
            new(46, 409, 25, 42),   // 25 right frame endcap
            new(73, 409, 30, 30),   // 26 scale1
            new(104, 409, 26, 30),  // 27 scale2
            new(131, 409, 42, 42),  // 28 glow1
            new(174, 409, 38, 42)   // 29 glow2
        );

    public static CustomPartsList SAM0 =>
        new("ui/uld/JobHudSAM0.tex",
            new(0, 0, 116, 50),     // 0 hilt
            new(116, 0, 278, 50),   // 1 red blade
            new(116, 50, 278, 50),  // 2 blade
            new(116, 100, 278, 50), // 3 glow
            new(116, 150, 190, 26), // 4 gem pulsar
            new(32, 50, 34, 60),    // 5 tickmark
            new(0, 50, 32, 66),     // 6 tassel
            new(66, 50, 28, 28),    // 7 orb?
            new(0, 116, 60, 60),    // 8 sparkle
            new(102, 176, 62, 34),  // 9 numBg
            new(0, 176, 102, 62),   // 10 gem plate
            new(66, 78, 24, 20),    // 11 gem
            new(66, 98, 46, 46)     // 12 gem glow
        );

    public static CustomPartsList SAM1 =>
        new("ui/uld/JobHudSAM1.tex",
            new(0, 0, 80, 80),     // 0 Setsu
            new(80, 0, 80, 80),    // 1 Getsu
            new(160, 0, 80, 80),   // 2 Ka
            new(0, 80, 80, 80),    // 3 Setsu Inactive
            new(80, 80, 80, 80),   // 4 Getsu Inactive
            new(160, 80, 80, 80),  // 5 Ka Inactive
            new(0, 160, 80, 80),   // 6 Setsu Kanji
            new(80, 160, 80, 80),  // 7 Getsu Kanji
            new(160, 160, 80, 80), // 8 Ka Kanji
            new(240, 0, 58, 80),   // 9 Sparkle1
            new(298, 0, 58, 80),   // 10 Sparkle1 Glow Teal
            new(240, 80, 58, 80),  // 11 Sparkle2
            new(298, 80, 58, 80),  // 12 Sparkle2 Glow Red
            new(240, 160, 58, 80), // 13 White Eclipse
            new(298, 160, 58, 80), // 14 Blue Halo
            new(0, 240, 80, 80),   // 15 Setsu Glow
            new(80, 240, 80, 80),  // 16 Getsu Glow
            new(160, 240, 80, 80), // 17 Ka Glow
            new(240, 240, 78, 78), // 18 Twinkle
            new(318, 240, 54, 54), // 19 White Halo
            new(0, 320, 80, 80),   // 20 Setsu Kanji Glow
            new(80, 320, 80, 80),  // 21 Getsu Kanji Glow
            new(160, 320, 80, 80)  // 22 Ka Kanji Glow
        );

    public static CustomPartsList RPR0 =>
        new("ui/uld/JobHudRRP0.tex",
            new(0, 96, 264, 100), // 0  scytheblade
            new(0, 42, 206, 52),  // 1  bottom right frame
            new(207, 1, 8, 40),   // 2  middle ticks
            new(216, 0, 34, 38),  // 3  corner ticks
            new(216, 38, 10, 4),  // 4  ??? tiny line
            new(250, 0, 12, 12),  // 5  tiny box
            new(205, 70, 18, 12), // 6  bw gradient
            new(1, 1, 181, 12),   // 7  backdrop
            new(2, 16, 180, 10),  // 8  red bar
            new(2, 30, 180, 10),  // 9  teal bar
            new(250, 12, 14, 16), // 10 white spot
            new(207, 43, 26, 26), // 11 frame
            new(2, 198, 180, 10), // 12 grey bar
            new(234, 38, 10, 12), // 13 tickmark
            new(0, 215, 184, 20), // 14 glow frame
            new(0, 240, 162, 56), // 15 glow scythe
            new(206, 82, 58, 14)  // 16 streaks
        );

    #endregion

    #region Ranged

    public static CustomPartsList MCH0 =>
        new("ui/uld/jobhudmch0.tex",
            new(0, 0, 60, 26),      // 0  dark tab
            new(60, 0, 60, 26),     // 1  red tab
            new(0, 32, 192, 46),    // 2  barrel
            new(0, 78, 200, 72),    // 3  parallelogram
            new(148, 150, 64, 64),  // 4  heatclock
            new(148, 214, 68, 64),  // 5  batteryclock
            new(212, 150, 64, 64),  // 6  blue ring
            new(0, 330, 228, 78),   // 7  heat glow
            new(216, 214, 48, 48),  // 8  eye shaped glow thing?
            new(148, 278, 72, 40),  // 9  tab border
            new(0, 150, 148, 36),   // 10 bar bg
            new(0, 186, 148, 36),   // 11 blue bar
            new(0, 222, 148, 36),   // 12 red bar
            new(0, 258, 148, 36),   // 13 white bar
            new(0, 294, 148, 36),   // 14 grey bar
            new(192, 0, 64, 42),    // 15 text bg
            new(212, 42, 64, 85),   // 16 smoke
            new(200, 42, 12, 37),   // 17 needle?
            new(160, 0, 32, 32),    // 18 simple heat icon
            new(276, 0, 62, 124),   // 19 lightning1
            new(276, 124, 62, 124), // 20 lightning2
            new(128, 0, 32, 32)     // 21 simple battery icon
        );

    public static CustomPartsList DNC1 =>
        new("ui/uld/JobHudDNC1.tex",
            new(1, 0, 167, 76),    // 0  bar
            new(1, 77, 166, 74),   // 1  backdrop
            new(168, 0, 48, 108),  // 2  feather
            new(216, 0, 84, 100),  // 3  half fan
            new(216, 100, 84, 80), // 4  half frame
            new(168, 108, 48, 32), // 5  corner clip thingy
            new(168, 140, 48, 36), // 6  number bg
            new(2, 160, 76, 60),   // 7  feather glow
            new(80, 156, 54, 40),  // 8  spotlights
            new(79, 198, 54, 40),  // 9  streaks
            new(132, 153, 20, 20), // 10 star
            new(216, 180, 84, 80)  // 11 half frame cover
        );

    #endregion

    #region Caster

    public static CustomPartsList BLM0 =>
        new("ui/uld/JobHudBLM0.tex",
            new(0, 0, 162, 144),   // 0  moon
            new(290, 0, 52, 52),   // 1  blue orb
            new(290, 52, 52, 52),  // 2  red orb
            new(342, 0, 20, 48),   // 3  blue crystal
            new(342, 48, 20, 48),  // 4  red crystal
            new(182, 236, 24, 68), // 5  icicle
            new(342, 96, 20, 48),  // 6  blue glow
            new(342, 144, 20, 48), // 7  red glow
            new(162, 0, 128, 124), // 8  lattice
            new(0, 146, 90, 90),   // 9  curved plate
            new(95, 153, 80, 78),  // 10 bar fill
            new(180, 146, 90, 90), // 11 bar backdrop
            new(362, 0, 30, 128),  // 12 clock hand
            new(324, 192, 30, 46), // 13 diamond
            new(354, 192, 30, 46), // 14 diamond glow
            new(0, 236, 90, 90),   // 15 flash
            new(90, 236, 68, 68),  // 16 eclipse
            new(290, 104, 46, 46), // 17 halo
            new(362, 128, 28, 28), // 18 glowball
            new(270, 150, 54, 83), // 19 diamond frame
            new(158, 236, 24, 68), // 20 icicle glow
            new(206, 236, 64, 36), // 21 text bg
            new(206, 272, 32, 32), // 22 simple icon thingy
            new(270, 233, 32, 42), // 23 diamond cover
            new(302, 238, 52, 52), // 24 grey orb
            new(307, 290, 85, 36), // 25 double pointy
            new(0, 324, 86, 40),   // 26 null paradox gem
            new(86, 324, 86, 40),  // 27 active paradox gem
            new(172, 324, 86, 40), // 28 paradox glow
            new(90, 306, 29, 18),  // 29 blue sparkles
            new(119, 306, 29, 36)  // 30 red sparkles
        );

            #endregion

            #region Tank

    public static CustomPartsList PLD0 =>
        new("ui/uld/JobHudPLD.tex",
            new(246, 0, 168, 18),    // 0 Bar
            new(246, 18, 168, 18),   // 1 Backdrop
            new(414, 0, 34, 60),     // 2 Tickmark
            new(0, 0, 246, 120),     // 3
            new(0, 0, 123, 120),     // 4 FrameL
            new(123, 0, 123, 120),   // 5 FrameR
            new(0, 0, 64, 120),      // 6
            new(182, 0, 64, 120),    // 7
            new(0, 301, 238, 120),   // 8
            new(0, 301, 123, 120),   // 9 GlowL
            new(123, 301, 118, 120), // 10 GlowR
            new(0, 301, 64, 120),    // 11
            new(182, 301, 58, 120),  // 12
            new(238, 300, 50, 50),   // 13 Halo
            new(246, 36, 102, 102),  // 14 Shine
            new(180, 274, 190, 26),  // 15 Streak
            new(0, 120, 180, 180),   // 16 Sigil
            new(316, 306, 78, 52),   // 17 Wing
            new(180, 138, 136, 136), // 18 Gem Frame
            new(384, 138, 68, 68),   // 19 Gem
            new(316, 138, 68, 68),   // 20 Gem Inactive
            new(348, 70, 68, 68)     // 21 Gem Glow
        );

    public static CustomPartsList WAR0 =>
        new("ui/uld/JobHudWAR.tex",
            new(0, 0, 242, 66),     // 0 Frame
            new(0, 224, 168, 22),   // 1 Bar
            new(0, 246, 168, 22),   // 2 Backdrop
            new(0, 66, 236, 98),    // 3
            new(0, 164, 146, 60),   // 4
            new(0, 268, 216, 64),   // 5 Frame Glow
            new(217, 236, 72, 100), // 6
            new(242, 172, 32, 32),  // 7
            new(242, 0, 108, 108),  // 8 Gem Base
            new(242, 108, 64, 64),  // 9 Gem
            new(306, 108, 54, 54)   // 10 Gem Glow
        );

    public static CustomPartsList DRK0 =>
        new("ui/uld/JobHudDRK0.tex",
            new(0, 0, 96, 88),       // 0 Ring
            new(96, 0, 228, 88),     // 1 Blade
            new(57, 92, 173, 16),    // 2 Red Bar
            new(57, 116, 173, 16),   // 3 Grey Bar
            new(280, 92, 96, 16),    // 4 Gritty Backdrop
            new(0, 88, 56, 56),      // 5 Active Gem
            new(0, 144, 276, 68),    // 6 Blade Glow
            new(0, 212, 240, 60),    // 7 Sigil Overlay
            new(274, 114, 100, 100), // 8 Arrow Ring
            new(272, 216, 80, 80),   // 9 Pink Halo
            new(324, 0, 28, 28),     // 10 Simple Star
            new(324, 28, 56, 56)     // 11 Inactive Gem
        );

            #endregion

            #region Healer

    public static CustomPartsList SCH1 =>
        new("ui/uld/JobHudSCH1.tex",
            new(0, 0, 248, 80),     // 0 frame
            new(3, 80, 174, 44),    // 1 bar
            new(3, 124, 174, 44),   // 2 empty bar
            new(0, 168, 196, 68),   // 3 butterfly wing glow
            new(0, 304, 68, 76),    // 4 simple wings
            new(192, 80, 44, 80),   // 5 faerie
            new(196, 168, 52, 52),  // 6 darts
            new(196, 220, 28, 44),  // 7 sparkles
            new(0, 236, 196, 68),   // 8 butterfly wing monotone
            new(248, 0, 68, 196),   // 9 butterfly wing bg (rotated)
            new(244, 192, 72, 188), // 10 feathery wing
            new(198, 264, 44, 80)   // 11 Seraph
        );

            #endregion

        #endregion

    #endregion
}
