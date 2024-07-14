using FFXIVClientStructs.FFXIV.Component.GUI;
using System.Runtime.InteropServices;
// ReSharper disable PartialTypeWithSinglePart

namespace FFXIVClientStructs.FFXIV.Client.UI;

/// <summary>
/// Base struct for all job gauge addons.
/// </summary>

[StructLayout(LayoutKind.Explicit, Size = 0x270)]
public unsafe partial struct AddonJobHud {
    [FieldOffset(0x230)] public byte Unk220;
    [FieldOffset(0x231)] public bool UseSimpleGauge;
    [FieldOffset(0x232)] public byte Unk222;

    // these 4 pointers get set in vf72, and point to varying offsets for each type of gauge
    [FieldOffset(0x238)] public AddonJobHudGauge* GaugeStandardPointer;
    [FieldOffset(0x240)] public AddonJobHudGauge* GaugeSimplePointer;
    [FieldOffset(0x248)] public AddonJobHudGaugeData* DataPreviousPointer;
    [FieldOffset(0x250)] public AddonJobHudGaugeData* DataCurrentPointer; // Current is always used to apply updates, and then copied onto Previous. The two are compared to detect changes.

    [FieldOffset(0x258)] public AtkResNode* JobHudRootNode;

    [FieldOffset(0x260)] public int TimelineLabelStandard; // always set to 19 by vf75
    [FieldOffset(0x264)] public int TimelineLabelSimple; // always set to 101 by vf75

    /// <summary>
    /// Base struct containing the data that each particular gauge relies on.<br/>
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Size = 0x08)]
    public partial struct AddonJobHudGaugeData {

    }

    /// <summary>
    /// Base struct for the gauges themselves.<br/>
    /// The majority of the fields in any given gauge will be pointers to Nodes and Components within its addon, but data values are usually sprinkled in too.
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Size = 0x10)]
    public partial struct AddonJobHudGauge {
        [FieldOffset(0x8)] public AddonJobHud* JobHud;
    }
}
