using FFXIVClientStructs.Attributes;
using FFXIVClientStructs.FFXIV.Component.GUI;
using System.Runtime.InteropServices;
// ReSharper disable PartialTypeWithSinglePart

namespace FFXIVClientStructs.FFXIV.Client.UI;

/// <summary>
/// ACN / SCH - Aetherflow Gauge<br/>
/// NOTE: this loads the UldResourceHandle "JobHudSCH0"
/// </summary>
[Addon("JobHudACN0")]

[StructLayout(LayoutKind.Explicit, Size = 0x350)]
public unsafe partial struct AddonJobHudACN0 {
    [FieldOffset(0x270)] public AetherflowACNGaugeData DataPrevious;
    [FieldOffset(0x280)] public AetherflowACNGaugeData DataCurrent;
    [FieldOffset(0x290)] public AetherflowACNGauge GaugeStandard;
    [FieldOffset(0x2F0)] public AetherflowACNGaugeSimple GaugeSimple;

    [StructLayout(LayoutKind.Explicit, Size = 0x10)]
    public partial struct AetherflowACNGaugeData {
        [FieldOffset(0x08)] public byte AetherflowStacks;
        [FieldOffset(0x09)] public byte Enabled;
    }

    [StructLayout(LayoutKind.Explicit, Size = 0x60)]
    public partial struct AetherflowACNGauge {
        //[FieldOffset(0x10), FixedSizeArray] internal FixedSizeArray3<AetherflowACNStack> _aetherflowStacks;
        [FieldOffset(0x58)] public int TimelineFrameId;

        [StructLayout(LayoutKind.Explicit, Size = 0x18)]
        public struct AetherflowACNStack {
            [FieldOffset(0x00)] public AtkComponentBase* StackContainer;
            [FieldOffset(0x08)] public AtkResNode* StackNode;
            [FieldOffset(0x10)] public bool StackReady;
        }
    }

    [StructLayout(LayoutKind.Explicit, Size = 0x60)]
    public partial struct AetherflowACNGaugeSimple {
       // [FieldOffset(0x10), FixedSizeArray] internal FixedSizeArray3<AetherflowACNStackSimple> _aetherflowStacks;
        [FieldOffset(0x58)] public int TimelineFrameId;

        [StructLayout(LayoutKind.Explicit, Size = 0x18)]
        public struct AetherflowACNStackSimple {
            [FieldOffset(0x00)] public AtkComponentBase* StackContainer;
            [FieldOffset(0x08)] public AtkResNode* StackNode;
            [FieldOffset(0x10)] public bool StackReady;
        }
    }
}

/// <summary>
/// SCH - Faerie Gauge<br/>
/// NOTE: this loads the UldResourceHandle "JobHudSCH1"
/// </summary>
[Addon("JobHudSCH0")]


[StructLayout(LayoutKind.Explicit, Size = 0x398)]
public unsafe partial struct AddonJobHudSCH0 {
    [FieldOffset(0x270)] public FaerieGaugeData DataPrevious;
    [FieldOffset(0x290)] public FaerieGaugeData DataCurrent;
    [FieldOffset(0x2B0)] public FaerieGauge GaugeStandard;
    [FieldOffset(0x340)] public FaerieGaugeSimple GaugeSimple;



    [StructLayout(LayoutKind.Explicit, Size = 0x20)]
    public partial struct FaerieGaugeData {
        [FieldOffset(0x08)] public int FaeValue;
        [FieldOffset(0x0C)] public int FaeMax;
        [FieldOffset(0x10)] public bool Aetherpact;
        [FieldOffset(0x11)] public bool Enabled;
        [FieldOffset(0x12)] public bool FaerieSummoned;
        [FieldOffset(0x14)] public int FaeMilestone;
        [FieldOffset(0x18)] public int SeraphTimeLeft;
        [FieldOffset(0x1C)] public int SeraphMaxTime;
    }



    [StructLayout(LayoutKind.Explicit, Size = 0x90)]
    public partial struct FaerieGauge {
        [FieldOffset(0x10)] public AtkResNode* Container;
        [FieldOffset(0x18)] public AtkResNode* FaeriePlateContainer;
        [FieldOffset(0x20)] public AtkTextNode* SeraphTimerText;
        [FieldOffset(0x30)] public AtkResNode* FaeGaugeTextContainer;
        [FieldOffset(0x38)] public AtkTextNode* FaeGaugeText;
        [FieldOffset(0x40)] public AtkImageNode* FaeBarFillAbsent;
        [FieldOffset(0x48)] public AtkImageNode* FaeBarFillSeraph;
        [FieldOffset(0x50)] public AtkImageNode* FaeBarFillStandard;
        [FieldOffset(0x58)] public AtkResNode* FaeBarGain;
        [FieldOffset(0x60)] public AtkResNode* FaeBarLoss;
        [FieldOffset(0x68)] public AtkResNode* FaeriePlate;
        [FieldOffset(0x70)] public int FaeBarMaxWidth;
        [FieldOffset(0x74)] public int FaeBarWidth;
        [FieldOffset(0x78)] public int FaeBarTargetWidth;
        [FieldOffset(0x7C)] public int FaeBarWidthChange;
        [FieldOffset(0x84)] public bool FaeBarAnimating;
        [FieldOffset(0x87)] public bool HasFaerie;
    }



    [StructLayout(LayoutKind.Explicit, Size = 0x58)]
    public partial struct FaerieGaugeSimple {
        [FieldOffset(0x10)] public AtkResNode* Container;
        [FieldOffset(0x18)] public AtkResNode* Container2;
        [FieldOffset(0x20)] public AtkComponentGaugeBar* FaeGaugeBar;
        [FieldOffset(0x28)] public AtkResNode* FaeGaugeBarFill;
        [FieldOffset(0x30)] public AtkComponentTextNineGrid* FaeValueDisplay;
        [FieldOffset(0x38)] public int FaeBarState;
        [FieldOffset(0x40)] public AtkResNode* SeraphContainer;
        [FieldOffset(0x48)] public AtkComponentTextNineGrid* SeraphTimerDisplay;
    }
}

/// <summary>
/// SMN - Aetherflow Gauge
/// </summary>
[Addon("JobHudSMN0")]


[StructLayout(LayoutKind.Explicit, Size = 0x300)]
public unsafe partial struct AddonJobHudSMN0 {
    [FieldOffset(0x270)] public AetherflowSMNGaugeData DataPrevious;
    [FieldOffset(0x280)] public AetherflowSMNGaugeData DataCurrent;
    [FieldOffset(0x290)] public AetherflowSMNGauge GaugeStandard;
    [FieldOffset(0x2C8)] public AetherflowSMNGaugeSimple GaugeSimple;



    [StructLayout(LayoutKind.Explicit, Size = 0x10)]
    public partial struct AetherflowSMNGaugeData {
        [FieldOffset(0x08)] public int AetherflowStacks;
      //  [FieldOffset(0x0C), FixedSizeArray] internal FixedSizeArray1<byte> _prerequisites;
    }



    [StructLayout(LayoutKind.Explicit, Size = 0x38)]
    public partial struct AetherflowSMNGauge {
        [FieldOffset(0x10)] public AtkComponentBase* StackContainer1;
        [FieldOffset(0x18)] public AtkComponentBase* StackContainer2;
        [FieldOffset(0x20)] public AtkResNode* Stack1;
        [FieldOffset(0x28)] public AtkResNode* Stack2;
        [FieldOffset(0x30)] public bool Stack1Ready;
        [FieldOffset(0x31)] public bool Stack2Ready;
        [FieldOffset(0x34)] public int TimelineFrameId;
    }



    [StructLayout(LayoutKind.Explicit, Size = 0x38)]
    public partial struct AetherflowSMNGaugeSimple {
        [FieldOffset(0x10)] public AtkComponentBase* StackContainer1;
        [FieldOffset(0x18)] public AtkComponentBase* StackContainer2;
        [FieldOffset(0x20)] public AtkResNode* Stack1;
        [FieldOffset(0x28)] public AtkResNode* Stack2;
        [FieldOffset(0x30)] public bool Stack1Ready;
        [FieldOffset(0x31)] public bool Stack2Ready;
        [FieldOffset(0x34)] public int TimelineFrameId;
    }
}

/// <summary>
/// SMN - Trance Gauge
/// </summary>
[Addon("JobHudSMN1")]


[StructLayout(LayoutKind.Explicit, Size = 0x508)]
public unsafe partial struct AddonJobHudSMN1 {
    [FieldOffset(0x270)] public TranceGaugeData DataPrevious;
    [FieldOffset(0x2A8)] public TranceGaugeData DataCurrent;
    [FieldOffset(0x2E0)] public TranceGauge GaugeStandard;
    [FieldOffset(0x430)] public TranceGaugeSimple GaugeSimple;



    [StructLayout(LayoutKind.Explicit, Size = 0x38)]
    public partial struct TranceGaugeData {
        [FieldOffset(0x08)] public bool Enabled;
        [FieldOffset(0x09)] public bool BahamutPlateEnabled;
        [FieldOffset(0x0A)] public bool SolarBahamutEnabled;
        [FieldOffset(0x0C)] public int Phase;
        [FieldOffset(0x10)] public int SummonTimeLeft;
        [FieldOffset(0x14)] public int SummonTimeMax;
        [FieldOffset(0x18)] public int EgiCount;
        [FieldOffset(0x1C)] public bool IfritReady;
        [FieldOffset(0x1D)] public bool TitanReady;
        [FieldOffset(0x1E)] public bool GarudaReady;
        [FieldOffset(0x20)] public int int30;
        [FieldOffset(0x24)] public int int34;
        [FieldOffset(0x28)] public int int38;
        [FieldOffset(0x2C)] public int CurrentEgi;
        [FieldOffset(0x30)] public int Attunement;
        [FieldOffset(0x34)] public int EgiTimeLeft;
    }



    [StructLayout(LayoutKind.Explicit, Size = 0x150)]
    public partial struct TranceGauge {

        [FieldOffset(0x010)] public AtkResNode* Container;
        [FieldOffset(0x018)] public AtkResNode* CarbunclePlate;
        [FieldOffset(0x020)] public AtkImageNode* CarbuncleBar;
        [FieldOffset(0x028)] public AtkTextNode* CarbuncleGaugeValue;

        [FieldOffset(0x038)] public AtkResNode* SummonPlate;
        [FieldOffset(0x040)] public AtkResNode* SummonHead;
        [FieldOffset(0x048)] public AtkTextNode* TranceGaugeValue;
        [FieldOffset(0x058)] public AtkImageNode* TranceBarFill;
        [FieldOffset(0x068)] public AtkResNode* WingContainer;
        [FieldOffset(0x070)] public AtkResNode* Wing;

        // At level 100, the main portion of the gauge is hidden and replaced with a duplicate that includes Solar Bahamut
        [FieldOffset(0x078)] public AtkResNode* Lv100SummonPlate;
        [FieldOffset(0x080)] public AtkResNode* Lv100SummonHead;
        [FieldOffset(0x088)] public AtkTextNode* Lv100TranceGaugeValue;
        [FieldOffset(0x090)] public AtkImageNode* Lv100TranceBarFill;
        [FieldOffset(0x0A0)] public AtkResNode* Lv100WingContainer;
        [FieldOffset(0x0A8)] public AtkResNode* Lv100Wing;
        [FieldOffset(0x0B0)] public AtkResNode* Lv100BladeContainer;
        [FieldOffset(0x0B8)] public AtkResNode* Lv100Blade;

        [FieldOffset(0x0C0)] public AtkResNode* EgiGems;
        [FieldOffset(0x0C8)] public AtkResNode* EgiTimerDisplay;

        [FieldOffset(0x0D0)] public EgiGauge IfritGauge;
        [FieldOffset(0x0F8)] public EgiGauge TitanGauge;
        [FieldOffset(0x120)] public EgiGauge GarudaGauge;

        [StructLayout(LayoutKind.Explicit, Size = 0x28)]
        public partial struct EgiGauge
        {
            [FieldOffset(0x00)] public AtkComponentBase* Container;
            [FieldOffset(0x08)] public AtkTextNode* AttunementStackText;
            [FieldOffset(0x10)] public AtkResNode* Gem;
            [FieldOffset(0x18)] public AtkResNode* Silhouette;
            [FieldOffset(0x20)] public int Status; // 0 = Spent, 1 = Available, 2 = Active, 3 = Locked
        }
    }



    [StructLayout(LayoutKind.Explicit, Size = 0xD8)]
    public partial struct TranceGaugeSimple {

          [FieldOffset(0x10)] public AtkComponentGaugeBar* TranceGaugeBar;
          [FieldOffset(0x18)] public AtkResNode* SummonIcon;
          [FieldOffset(0x20)] public AtkComponentTextNineGrid* TranceTimerDisplay;

          [FieldOffset(0x30)] public AtkResNode* EgiContainer;
          [FieldOffset(0x38)] public AtkComponentTextNineGrid* EgiTimerDisplay;

          [FieldOffset(0x40)] public EgiGaugeSimple IfritGauge;
          [FieldOffset(0x60)] public EgiGaugeSimple TitanGauge;
          [FieldOffset(0x80)] public EgiGaugeSimple GarudaGauge;

          [FieldOffset(0xA0)] public AtkComponentBase* EgiIconContainer;
          [FieldOffset(0xA8)] public AtkResNode* EgiIcons;
          [FieldOffset(0xB0)] public AtkResNode* IfritIcon;
          [FieldOffset(0xB8)] public AtkResNode* TitanIcon;
          [FieldOffset(0xC0)] public AtkResNode* GarudaIcon;

          [FieldOffset(0xC8)] public bool EgiActive;
          [FieldOffset(0xD0)] public int TimelineFrameId;

          [StructLayout(LayoutKind.Explicit, Size = 0x20)]
          public struct EgiGaugeSimple {
              [FieldOffset(0x00)] public AtkComponentBase* Gem;
              [FieldOffset(0x08)] public AtkTextNode* AttunementStackText;
              [FieldOffset(0x10)] public AtkResNode* GemGlow;
              [FieldOffset(0x18)] public int Status;
          }

    }
}
