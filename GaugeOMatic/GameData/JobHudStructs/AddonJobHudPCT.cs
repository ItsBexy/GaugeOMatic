using FFXIVClientStructs.Attributes;
using FFXIVClientStructs.FFXIV.Component.GUI;
using System.Runtime.InteropServices;
// ReSharper disable PartialTypeWithSinglePart

namespace FFXIVClientStructs.FFXIV.Client.UI;

/// <summary>
/// PCT - Canvases
/// </summary>
[Addon("JobHudRPM0")]


[StructLayout(LayoutKind.Explicit, Size = 0x3C0)]
public unsafe partial struct AddonJobHudRPM0 {

    [FieldOffset(0x270)] public CanvasGaugeData DataPrevious;
    [FieldOffset(0x290)] public CanvasGaugeData DataCurrent;
    [FieldOffset(0x2B0)] public CanvasGauge GaugeStandard;
    [FieldOffset(0x350)] public CanvasGaugeSimple GaugeSimple;



    [StructLayout(LayoutKind.Explicit, Size = 0x20)]
    public partial struct CanvasGaugeData {
        [FieldOffset(0x08)] public bool Enabled;
        [FieldOffset(0x09)] public bool WeaponEnabled;
        [FieldOffset(0x0A)] public bool LandscapeEnabled;
        [FieldOffset(0x0B)] public bool MadeenEnabled;
        [FieldOffset(0x0C)] public byte CreaturePortrait;
        [FieldOffset(0x10)] public int CreaturePartFlags;
        [FieldOffset(0x14)] public int CreatureMotif;
        [FieldOffset(0x18)] public bool WeaponMotif;
        [FieldOffset(0x19)] public bool LandscapeMotif;

    }



    [StructLayout(LayoutKind.Explicit, Size = 0xA0)]
    public partial struct CanvasGauge {
        [FieldOffset(0x10)] public AtkResNode* Container;
        [FieldOffset(0x18)] public AtkResNode* CreaturePartBox;
        [FieldOffset(0x20)] public AtkComponentBase* CreaturePartPom;
        [FieldOffset(0x28)] public AtkComponentBase* CreaturePartWing;
        [FieldOffset(0x30)] public AtkComponentBase* CreaturePartClaw;
        [FieldOffset(0x38)] public AtkResNode* CreaturePortrait;
        [FieldOffset(0x40)] public AtkComponentBase* MooglePortrait;
        [FieldOffset(0x48)] public AtkComponentBase* MadeenPortrait;
        [FieldOffset(0x50)] public AtkResNode* CreatureCanvas;
        [FieldOffset(0x58)] public AtkComponentBase* CreaturePomPainting;
        [FieldOffset(0x60)] public AtkComponentBase* CreatureWingPainting;
        [FieldOffset(0x68)] public AtkComponentBase* CreatureClawPainting;
        [FieldOffset(0x78)] public AtkComponentBase* WeaponPaintingHammer;
        [FieldOffset(0x80)] public AtkComponentBase* LandscapePaintingStarrySky;
    }



    [StructLayout(LayoutKind.Explicit, Size = 0x70)]
    public partial struct CanvasGaugeSimple {
        [FieldOffset(0x10)] public AtkResNode* Container;
        [FieldOffset(0x18)] public AtkResNode* CreaturePartBox;
        [FieldOffset(0x20)] public AtkComponentBase* CreaturePartPom;
        [FieldOffset(0x28)] public AtkComponentBase* CreaturePartWing;
        [FieldOffset(0x30)] public AtkComponentBase* CreaturePartClaw;
        [FieldOffset(0x38)] public AtkComponentBase* CreaturePortrait;
        [FieldOffset(0x40)] public AtkComponentBase* CreatureCanvas;
        [FieldOffset(0x48)] public AtkComponentBase* WeaponCanvas;
        [FieldOffset(0x50)] public AtkComponentBase* LandscapeCanvas;
        [FieldOffset(0x58)] public int CreaturePartFlags;
        [FieldOffset(0x60)] public int CreatureMotif;
        [FieldOffset(0x68)] public int WeaponMotif;
    }

}

/// <summary>
/// PCT - Palette Gauge
/// </summary>
[Addon("JobHudRPM1")]


[StructLayout(LayoutKind.Explicit, Size = 0x2F0)]
public unsafe partial struct AddonJobHudRPM1 {

    [FieldOffset(0x270)] public PaletteGaugeData DataPrevious;
    [FieldOffset(0x290)] public PaletteGaugeData DataCurrent;
    [FieldOffset(0x2B0)] public PaletteGauge GaugeStandard;
    [FieldOffset(0x2D0)] public PaletteGaugeSimple GaugeSimple;



    [StructLayout(LayoutKind.Explicit, Size = 0x20)]
    public partial struct PaletteGaugeData {
        [FieldOffset(0x08)] public bool Enabled;
        [FieldOffset(0x09)] public bool SubtractiveActive;
        [FieldOffset(0x0A)] public bool WhitePaintEnabled;
        [FieldOffset(0x0B)] public bool BlackPaintEnabled;
        [FieldOffset(0x0C)] public int PaletteValue;
        [FieldOffset(0x10)] public int PaletteMax;
        [FieldOffset(0x14)] public int PaletteMid;
        [FieldOffset(0x18)] public int WhitePaint;
        [FieldOffset(0x1C)] public int BlackPaint;
    }



    [StructLayout(LayoutKind.Explicit, Size = 0x20)]
    public partial struct PaletteGauge {
        [FieldOffset(0x10)] public PaletteBarStandard* PaletteBar;
        [FieldOffset(0x18)] public PaintStacksStandard* PaintStacks;

        [StructLayout(LayoutKind.Explicit, Size = 0x60)]
        public partial struct PaletteBarStandard
        {
            [FieldOffset(0x10)] public AtkTextNode* ValueText;
            [FieldOffset(0x18)] public AtkResNode* ValueDisplay;
            [FieldOffset(0x20)] public AtkResNode* GaugeBarContainer;

            [FieldOffset(0x28)] public bool CanSpend;

            [FieldOffset(0x30)] public AtkResNode* MainFillNode;
            [FieldOffset(0x38)] public AtkImageNode* IncreaseFillNode;
            [FieldOffset(0x40)] public AtkImageNode* DecreaseFillNode;
        }

        [StructLayout(LayoutKind.Explicit, Size = 0x58)]
        public partial struct PaintStacksStandard
        {
            [FieldOffset(0x10)] public AtkResNode* Container;
         //   [FieldOffset(0x18), FixedSizeArray] internal FixedSizeArray5<Pointer<AtkComponentBase>> _stacks;
        }
    }



    [StructLayout(LayoutKind.Explicit, Size = 0x20)]
    public partial struct PaletteGaugeSimple {
        [FieldOffset(0x10)] public PaletteBarSimple* PaletteBar;
        [FieldOffset(0x18)] public PaintStacksSimple* PaintStacks;

        [StructLayout(LayoutKind.Explicit, Size = 0x40)]
        public partial struct PaletteBarSimple
        {
            [FieldOffset(0x10)] public AtkTextNode* BarTextNode;
            [FieldOffset(0x18)] public AtkResNode* ValueDisplay;
            [FieldOffset(0x20)] public AtkComponentGaugeBar* GaugeBarComponent;
            [FieldOffset(0x28)] public AtkResNode* Container;
        }

        [StructLayout(LayoutKind.Explicit, Size = 0x48)]
        public partial struct PaintStacksSimple
        {
            [FieldOffset(0x10)] public AtkResNode* Container;
         //   [FieldOffset(0x18), FixedSizeArray] internal FixedSizeArray5<Pointer<AtkComponentBase>> _stacks;
        }
    }
}
