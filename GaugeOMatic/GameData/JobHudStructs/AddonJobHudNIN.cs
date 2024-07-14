using FFXIVClientStructs.Attributes;
using FFXIVClientStructs.FFXIV.Component.GUI;
using System.Runtime.InteropServices;
// ReSharper disable PartialTypeWithSinglePart

namespace FFXIVClientStructs.FFXIV.Client.UI;

/// <summary>
/// NIN - Ninki Gauge
/// </summary>
[Addon("JobHudNIN0")]


[StructLayout(LayoutKind.Explicit, Size = 0x318)]
public unsafe partial struct AddonJobHudNIN0 {
    [FieldOffset(0x270)] public NinkiGaugeData DataPrevious;
    [FieldOffset(0x288)] public NinkiGaugeData DataCurrent;
    [FieldOffset(0x2A0)] public NinkiGauge GaugeStandard;
    [FieldOffset(0x2E0)] public NinkiGaugeSimple GaugeSimple;



    [StructLayout(LayoutKind.Explicit, Size = 0x18)]
    public partial struct NinkiGaugeData {
        [FieldOffset(0x08)] public bool Enabled;
        [FieldOffset(0x0C)] public int NinkiValue;
        [FieldOffset(0x10)] public int Max;
        [FieldOffset(0x14)] public int Mid;
    }



    [StructLayout(LayoutKind.Explicit, Size = 0x40)]
    public partial struct NinkiGauge {
        [FieldOffset(0x10)] public AtkResNode* Container;
        [FieldOffset(0x18)] public AtkResNode* Scroll;
        [FieldOffset(0x20)] public AtkTextNode* ValueText;
        [FieldOffset(0x28)] public AtkComponentGaugeBar* GaugeBar0; // bar for 0-50
        [FieldOffset(0x30)] public AtkComponentGaugeBar* GaugeBar1; // bar for 50-100
        [FieldOffset(0x38)] public bool CanSpend;
    }



    [StructLayout(LayoutKind.Explicit, Size = 0x38)]
    public partial struct NinkiGaugeSimple {
        [FieldOffset(0x10)] public AtkResNode* Container;
        [FieldOffset(0x18)] public AtkResNode* BarTextNode;
        [FieldOffset(0x20)] public AtkComponentTextNineGrid* ValueDisplay;
        [FieldOffset(0x28)] public AtkComponentGaugeBar* GaugeBar;
    }
}

/// <summary>
/// NIN - Kazematoi
/// </summary>
[Addon("JobHudNIN1v70")]


[StructLayout(LayoutKind.Explicit, Size = 0x330)]
public unsafe partial struct AddonJobHudNIN1 {
    [FieldOffset(0x270)] public KazematoiGaugeData DataPrevious;
    [FieldOffset(0x280)] public KazematoiGaugeData DataCurrent;
    [FieldOffset(0x290)] public KazematoiGauge GaugeStandard;
    [FieldOffset(0x2F0)] public KazematoiGaugeSimple GaugeSimple;



    [StructLayout(LayoutKind.Explicit, Size = 0x10)]
    public partial struct KazematoiGaugeData {
        [FieldOffset(0x08)] public bool Enabled;
        [FieldOffset(0x0C)] public int StackCount;
    }



    [StructLayout(LayoutKind.Explicit, Size = 0x60)]
    public partial struct KazematoiGauge {

        [FieldOffset(0x18)] public AtkComponentBase* SwipeEffect;
        [FieldOffset(0x10)] public AtkResNode* Container;
        [FieldOffset(0x20)] public AtkComponentBase* ScrollingClouds;

       // [FieldOffset(0x30), FixedSizeArray] internal FixedSizeArray5<Pointer<AtkComponentBase>> _stackIcons;

        [FieldOffset(0x58)] public int StackCount;
    }



    [StructLayout(LayoutKind.Explicit, Size = 0x40)]
    public partial struct KazematoiGaugeSimple {
      //  [FieldOffset(0x10), FixedSizeArray] internal FixedSizeArray5<Pointer<AtkComponentBase>> _stackIcons;
    }
}
