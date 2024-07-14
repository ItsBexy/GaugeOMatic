using FFXIVClientStructs.Attributes;
using FFXIVClientStructs.FFXIV.Component.GUI;
using System.Runtime.InteropServices;
// ReSharper disable PartialTypeWithSinglePart

namespace FFXIVClientStructs.FFXIV.Client.UI;

/// <summary>
/// DRG - Dragon Gauge
/// </summary>
[Addon("JobHudDRG0")]


[StructLayout(LayoutKind.Explicit, Size = 0x358)]
public unsafe partial struct AddonJobHudDRG0
{
    [FieldOffset(0x270)] public DragonGaugeData DataPrevious;
    [FieldOffset(0x290)] public DragonGaugeData DataCurrent;
    [FieldOffset(0x2B0)] public DragonGauge GaugeStandard;
    [FieldOffset(0x2F8)] public DragonGaugeSimple GaugeSimple;



    [StructLayout(LayoutKind.Explicit, Size = 0x20)]
    public partial struct DragonGaugeData
    {
        [FieldOffset(0x08)] public bool Enabled;
        [FieldOffset(0x09)] public bool FirstMindsFocusEnabled;
        [FieldOffset(0x0C)] public int LotDStatus;
        [FieldOffset(0x10)] public int LotDTimer;
        [FieldOffset(0x14)] public int LotDMax;
        [FieldOffset(0x18)] public int FirstMindsFocusCount;
    }



    [StructLayout(LayoutKind.Explicit, Size = 0x48)]
    public partial struct DragonGauge
    {
        [FieldOffset(0x10)] public AtkResNode* Container;
        [FieldOffset(0x18)] public AtkResNode* Container2;
        [FieldOffset(0x20)] public int LotDStatus;
        [FieldOffset(0x28)] public AtkTextNode* LotDTimerText;
        [FieldOffset(0x30)] public AtkComponentGaugeBar* LotDTimerGaugeBar;
        [FieldOffset(0x38)] public AtkResNode* FirstMindsFocusContainer;
        [FieldOffset(0x40)] public AtkResNode* FirstMindsFocusContainer2;
    }



    [StructLayout(LayoutKind.Explicit, Size = 0x60)]
    public partial struct DragonGaugeSimple
    {
        [FieldOffset(0x10)] public AtkResNode* Container;
        [FieldOffset(0x18)] public AtkComponentTextNineGrid* LotDTimerDisplay;
        [FieldOffset(0x20)] public AtkComponentGaugeBar* LotDTimerGaugeBar;
        [FieldOffset(0x28)] public AtkResNode* FirstMindsFocusContainer;
        [FieldOffset(0x30)] public AtkResNode* FirstMindsFocusContainer2;
        [FieldOffset(0x38)] public bool FirstMindsFocusReady;
        [FieldOffset(0x40)] public AtkComponentBase* FirstMindsFocus1;
        [FieldOffset(0x48)] public AtkResNode* FirstMindsFocusGlow1;
        [FieldOffset(0x50)] public AtkComponentBase* FirstMindsFocus2;
        [FieldOffset(0x58)] public AtkResNode* FirstMindsFocusGlow2;
    }
}
