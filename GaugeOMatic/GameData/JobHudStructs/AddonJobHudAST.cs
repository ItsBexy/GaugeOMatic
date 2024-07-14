using FFXIVClientStructs.Attributes;
using FFXIVClientStructs.FFXIV.Component.GUI;
using System.Runtime.InteropServices;
// ReSharper disable PartialTypeWithSinglePart

namespace FFXIVClientStructs.FFXIV.Client.UI;

/// <summary>
/// AST - Arcana Gauge
/// </summary>
[Addon("JobHudAST0")]


[StructLayout(LayoutKind.Explicit, Size = 0x3F8)]
public unsafe partial struct AddonJobHudAST0 {
    [FieldOffset(0x270)] public ArcanaGaugeData DataPrevious;
    [FieldOffset(0x290)] public ArcanaGaugeData DataCurrent;
    [FieldOffset(0x2B0)] public ArcanaGauge GaugeStandard;
    [FieldOffset(0x358)] public ArcanaGaugeSimple GaugeSimple;



    [StructLayout(LayoutKind.Explicit, Size = 0x20)]
    public partial struct ArcanaGaugeData
    {
        [FieldOffset(0x08)] public bool Enabled;
        [FieldOffset(0x09)] public bool MinorArcanaEnabled;
        [FieldOffset(0x0C)] public int ArcanumIdI;
        [FieldOffset(0x10)] public int ArcanumIdII;
        [FieldOffset(0x14)] public int ArcanumIdIII;
        [FieldOffset(0x18)] public int MinorArcanumId;
        [FieldOffset(0x1C)] public int FrameColor;
    }



    [StructLayout(LayoutKind.Explicit, Size = 0xA8)]
    public partial struct ArcanaGauge {
        [FieldOffset(0x18)] public AtkResNode* Container;
        [FieldOffset(0x20)] public AtkResNode* GaugePlate;

        [FieldOffset(0x28)] public ArcanaCard CardI;
        [FieldOffset(0x48)] public ArcanaCard CardII;
        [FieldOffset(0x68)] public ArcanaCard CardIII;
        [FieldOffset(0x88)] public ArcanaCard MinorArcanaCard;
    }



    [StructLayout(LayoutKind.Explicit, Size = 0xA0)]
    public partial struct ArcanaGaugeSimple {
        [FieldOffset(0x18)] public AtkResNode* Container;

        [FieldOffset(0x20)] public ArcanaCard CardI;
        [FieldOffset(0x40)] public ArcanaCard CardII;
        [FieldOffset(0x60)] public ArcanaCard CardIII;
        [FieldOffset(0x80)] public ArcanaCard MinorArcanaCard;
    }

    [StructLayout(LayoutKind.Explicit, Size = 0x20)]
    public partial struct ArcanaCard
    {
        [FieldOffset(0x00)] public AtkComponentBase* CardComponent;
        [FieldOffset(0x08)] public AtkResNode* Symbol;
        [FieldOffset(0x10)] public AtkResNode* Frame;
        [FieldOffset(0x18)] public int ArcanumId;
    }
}
