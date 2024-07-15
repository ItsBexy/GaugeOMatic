using FFXIVClientStructs.Attributes;
using FFXIVClientStructs.FFXIV.Component.GUI;
using System.Runtime.InteropServices;
// ReSharper disable PartialTypeWithSinglePart
// ReSharper disable All

namespace FFXIVClientStructs.FFXIV.Client.UI;

/// <summary>
/// RPR - Soul Gauge
/// </summary>
[Addon("JobHudRRP0")]


[StructLayout(LayoutKind.Explicit, Size = 0x408)]
public unsafe partial struct AddonJobHudRRP0 {
    [FieldOffset(0x270)] public SoulGaugeData DataPrevious;
    [FieldOffset(0x298)] public SoulGaugeData DataCurrent;
    [FieldOffset(0x2C0)] public SoulGauge GaugeStandard;
    [FieldOffset(0x3B8)] public SoulGaugeSimple GaugeSimple;



    [StructLayout(LayoutKind.Explicit, Size = 0x28)]
    public partial struct SoulGaugeData {
        [FieldOffset(0x08)] public bool Enabled;
        [FieldOffset(0x09)] public bool ShroudEnabled;
        [FieldOffset(0x0C)] public int SoulValue;
        [FieldOffset(0x10)] public int ShroudValue;
        [FieldOffset(0x14)] public int SoulMax;
        [FieldOffset(0x18)] public int ShroudMax;
        [FieldOffset(0x1C)] public int SoulMid;
        [FieldOffset(0x20)] public int ShroudMid;
        [FieldOffset(0x24)] public bool Enshrouded;
    }



    [StructLayout(LayoutKind.Explicit, Size = 0xF8)]
    public partial struct SoulGauge {
        [FieldOffset(0x10)] public AtkResNode* Container;
        [FieldOffset(0x18)] public AtkResNode* Container2;
        [FieldOffset(0x20)] public AtkComponentBase* SoulMarker;
        [FieldOffset(0x28)] public AtkResNode* SoulBarOutline;
        [FieldOffset(0x30)] public AtkResNode* SoulBarEffects;
        [FieldOffset(0x38)] public AtkImageNode* SoulBarFill;
        [FieldOffset(0x40)] public AtkImageNode* SoulBarGain;
        [FieldOffset(0x48)] public AtkImageNode* SoulBarLoss;
        [FieldOffset(0x80)] public AtkComponentBase* SoulTextContainer;
        [FieldOffset(0x88)] public AtkComponentBase* ShroudMarker;
        [FieldOffset(0x90)] public AtkResNode* ShroudBarOutline;
        [FieldOffset(0x98)] public AtkResNode* ShroudBarEffects;
        [FieldOffset(0xA0)] public AtkImageNode* ShroudBarFill;
        [FieldOffset(0xA8)] public AtkImageNode* ShroudBarGain;
        [FieldOffset(0xB0)] public AtkImageNode* ShroudBarLoss;
        [FieldOffset(0xE8)] public AtkComponentBase* ShroudTextContainer;
    }



    [StructLayout(LayoutKind.Explicit, Size = 0x50)]
    public partial struct SoulGaugeSimple {
        [FieldOffset(0x10)] public AtkResNode* Container;
        [FieldOffset(0x18)] public AtkResNode* ShroudContainer; // no equiv. pointer to SoulContainer, strangely
        [FieldOffset(0x20)] public AtkComponentGaugeBar* SoulGaugeBar;
        [FieldOffset(0x28)] public AtkComponentTextNineGrid* SoulValueDisplay;
        [FieldOffset(0x38)] public AtkComponentGaugeBar* ShroudGaugeBar;
        [FieldOffset(0x40)] public AtkComponentTextNineGrid* ShroudValueDisplay;
        [FieldOffset(0x4C)] public int GlowTimelineFrameId;
    }
}

/// <summary>
/// RPR - Death Gauge
/// </summary>
[Addon("JobHudRRP1")]


[StructLayout(LayoutKind.Explicit, Size = 0x4A0)]
public unsafe partial struct AddonJobHudRRP1 {
    [FieldOffset(0x270)] public DeathGaugeData DataPrevious;
    [FieldOffset(0x290)] public DeathGaugeData DataCurrent;
    [FieldOffset(0x2B0)] public DeathGauge GaugeStandard;
    [FieldOffset(0x3C8)] public DeathGaugeSimple GaugeSimple;



    [StructLayout(LayoutKind.Explicit, Size = 0x20)]
    public partial struct DeathGaugeData {
        [FieldOffset(0x08)] public bool Enabled;
        [FieldOffset(0x09)] public bool Enshrouded;
        [FieldOffset(0x0C)] public int LemureShroudStacks;
        [FieldOffset(0x10)] public int VoidShroudStacks;
        [FieldOffset(0x14)] public int EnshroudTimer;
    }



    [StructLayout(LayoutKind.Explicit, Size = 0x118)]
    public partial struct DeathGauge {
        [FieldOffset(0x010)] public AtkResNode* Container;
    //    [FieldOffset(0x018), FixedSizeArray] internal FixedSizeArray5<ShroudStack> _shroudStacks;

        [FieldOffset(0x110)] public AtkComponentTextNineGrid* EnshroudTimerDisplay;

        [StructLayout(LayoutKind.Explicit, Size = 0x30)]
        public struct ShroudStack {
            [FieldOffset(0x08)] public AtkComponentBase* Container;
            [FieldOffset(0x10)] public AtkResNode* LemureStack;
            [FieldOffset(0x18)] public AtkResNode* LemureFlame;
            [FieldOffset(0x20)] public AtkResNode* VoidStack;
            [FieldOffset(0x28)] public AtkResNode* VoidFlame;
        }
    }



    [StructLayout(LayoutKind.Explicit, Size = 0xD8)]
    public partial struct DeathGaugeSimple {
        [FieldOffset(0x010)] public AtkResNode* Container;
        [FieldOffset(0x018)] public AtkResNode* EnshroudIndicator;
       // [FieldOffset(0x02C), FixedSizeArray] internal FixedSizeArray5<ShroudStackSimple> _shroudStacks;

        [FieldOffset(0xD0)] public AtkComponentTextNineGrid* EnshroudTimerDisplay;

        [StructLayout(LayoutKind.Explicit, Size = 0x20)]
        public struct ShroudStackSimple {
            [FieldOffset(0x04)] public AtkComponentBase* Container;
            [FieldOffset(0x0C)] public AtkResNode* GlowContainer;
            [FieldOffset(0x14)] public AtkResNode* Glow;
        }
    }
}
