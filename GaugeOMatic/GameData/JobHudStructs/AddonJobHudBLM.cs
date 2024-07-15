using FFXIVClientStructs.Attributes;
using FFXIVClientStructs.FFXIV.Component.GUI;
using System.Runtime.InteropServices;
// ReSharper disable PartialTypeWithSinglePart
// ReSharper disable All

namespace FFXIVClientStructs.FFXIV.Client.UI;

/// <summary>
/// BLM - Elemental Gauge
/// </summary>
[Addon("JobHudBLM0")]
[StructLayout(LayoutKind.Explicit, Size = 0x510)]
public unsafe partial struct AddonJobHudBLM0
{
    [FieldOffset(0x270)] public ElementalGaugeData DataPrevious;
    [FieldOffset(0x2A8)] public ElementalGaugeData DataCurrent;
    [FieldOffset(0x2E0)] public ElementalGauge GaugeStandard;
    [FieldOffset(0x460)] public ElementalGaugeSimple GaugeSimple;

    [StructLayout(LayoutKind.Explicit, Size = 0x38)]
    public partial struct ElementalGaugeData
    {
        [FieldOffset(0x08)] public bool Enabled;
        [FieldOffset(0x09)] public bool ElementActive;
        [FieldOffset(0x0A)] public bool EnochianActive;
        [FieldOffset(0x0B)] public bool PolyglotEnabled;
        [FieldOffset(0x0C)] public bool ParadoxEnabled;
        [FieldOffset(0x10)] public int ElementStacks; // Positive = Fire, Negative = Ice
        [FieldOffset(0x14)] public int ElementStackMax;
        [FieldOffset(0x18)] public int ElementTimeLeft;
        [FieldOffset(0x1c)] public int ElementMaxTime;
        [FieldOffset(0x20)] public int UmbralHearts;
        [FieldOffset(0x24)] public int EnochianTimer;
        [FieldOffset(0x28)] public int EnochianMaxTime;
        [FieldOffset(0x2C)] public int PolyglotStacks;
        [FieldOffset(0x30)] public int PolyglotMax;
        [FieldOffset(0x34)] public bool ParadoxReady;
    }

    [StructLayout(LayoutKind.Explicit, Size = 0x180)]
    public partial struct ElementalGauge
    {
        [FieldOffset(0x010)] public AtkResNode* Container;
        [FieldOffset(0x018)] public AtkResNode* ElementalCrescent;
        [FieldOffset(0x020)] public bool ElementActive;
        [FieldOffset(0x028)] public AtkResNode* ElementStacks;

       // [FieldOffset(0x030), FixedSizeArray] internal FixedSizeArray3<Pointer<AtkComponentBase>> _elementStack;

        [FieldOffset(0x048)] public int TimelineStartFrameId;
        [FieldOffset(0x050)] public AtkTextNode* ElementTimerText;
        [FieldOffset(0x058)] public AtkComponentBase* ElementOrbContainer;
        [FieldOffset(0x060)] public AtkResNode* FireOrb;
        [FieldOffset(0x068)] public AtkResNode* IceOrb;

        [StructLayout(LayoutKind.Explicit, Size = 0x10)]
        public struct UmbralHeart
        {
            [FieldOffset(0x0)] public AtkComponentBase* Container;
            [FieldOffset(0x8)] public AtkResNode* Glow;
        }

       //[FieldOffset(0x070), FixedSizeArray] internal FixedSizeArray3<UmbralHeart> _umbralHearts;

        [FieldOffset(0x0A0)] public AtkResNode* UmbralHeartContainer;
        [FieldOffset(0x0A8)] public int UmbralHeartTimelineFrameId;
        [FieldOffset(0x0B8)] public AtkResNode* EnochianBar;
        [FieldOffset(0x0C0)] public AtkComponentBase* EnochianBarFill;
        [FieldOffset(0x0C8)] public AtkResNode* EnochianDialContainer;
        [FieldOffset(0x0D0)] public AtkComponentBase* EnochianDial;
        [FieldOffset(0x0D8)] public bool EnochianActive;
        [FieldOffset(0x0DC)] public int EnochianTimePassed; // seconds
        [FieldOffset(0x0E8)] public AtkResNode* PolyglotContainer;

        [StructLayout(LayoutKind.Explicit, Size = 0x18)]
        public struct PolyglotStack
        {
            [FieldOffset(0x00)] public AtkComponentBase* Container;
            [FieldOffset(0x08)] public AtkResNode* Gem;
            [FieldOffset(0x10)] public AtkResNode* Slot;
        }

      //  [FieldOffset(0x0F0), FixedSizeArray] internal FixedSizeArray3<PolyglotStack> _polyglot;

        [FieldOffset(0x138)] public int PolyglotTimelineFrameId;
        [FieldOffset(0x13C)] public int PolyglotStacks;
        [FieldOffset(0x140)] public int PolyglotMax;
        [FieldOffset(0x148)] public AtkResNode* ParadoxContainer;
        [FieldOffset(0x150)] public AtkImageNode* ParadoxNeedle;
        [FieldOffset(0x160)] public AtkComponentBase* ParadoxGem;
        [FieldOffset(0x168)] public AtkResNode* ParadoxGemGlow;
        [FieldOffset(0x170)] public AtkResNode* ParadoxGemBase;
    }

    [StructLayout(LayoutKind.Explicit, Size = 0xB0)]
    public partial struct ElementalGaugeSimple
    {
        [FieldOffset(0x10)] public AtkResNode* Container;
        [FieldOffset(0x18)] public AtkResNode* Container2;
        [FieldOffset(0x20)] public AtkComponentTextNineGrid* ElementalTimerDisplay;

       // [FieldOffset(0x28), FixedSizeArray] internal FixedSizeArray3<Pointer<AtkComponentBase>> _elementStack;
       // [FieldOffset(0x40), FixedSizeArray] internal FixedSizeArray3<Pointer<AtkComponentBase>> _umbralHearts;

        [FieldOffset(0x58)] public AtkComponentBase* ElementalIcon;
        [FieldOffset(0x60)] public AtkResNode* EnochianGauge;
        [FieldOffset(0x68)] public AtkComponentGaugeBar* EnochianGaugeBar;

       // [FieldOffset(0x70), FixedSizeArray] internal FixedSizeArray3<Pointer<AtkComponentBase>> _polyglotGems;

        [FieldOffset(0x88)] public AtkComponentBase* ParadoxGem;
        [FieldOffset(0x90)] public int ElementStacks;
        [FieldOffset(0x94)] public int ElementStackMax;
        [FieldOffset(0x98)] public int UmbralHeartCount;
        [FieldOffset(0xA0)] public bool ParadoxReady;
        [FieldOffset(0xA4)] public int PolyglotStacks;
        [FieldOffset(0xA8)] public int TimelineFrameId;
    }
}

/// <summary>
/// BLM - Astral Gauge
/// </summary>
[Addon("JobHudBLM1")]
[StructLayout(LayoutKind.Explicit, Size = 0x3C0)]
public unsafe partial struct AddonJobHudBLM1
{
    [FieldOffset(0x270)] public AstralGaugeData DataPrevious;
    [FieldOffset(0x280)] public AstralGaugeData DataCurrent;
    [FieldOffset(0x290)] public AstralGauge GaugeStandard;
    [FieldOffset(0x310)] public AstralGaugeSimple GaugeSimple;

    [StructLayout(LayoutKind.Explicit, Size = 0x10)]
    public partial struct AstralGaugeData
    {
        [FieldOffset(0x08)] public bool Enabled;
        [FieldOffset(0x09)] public bool Active;
        [FieldOffset(0x0C)] public int StackCount;
    }

    [StructLayout(LayoutKind.Explicit, Size = 0x80)]
    public partial struct AstralGauge
    {

        [FieldOffset(0x18)] public AtkResNode* Container;

     //   [FieldOffset(0x28), FixedSizeArray] internal FixedSizeArray6<AstralSoulStack> _astralSoulStacks;

        [StructLayout(LayoutKind.Explicit, Size = 0x10)]
        public partial struct AstralSoulStack
        {
            [FieldOffset(0x0)] public AtkComponentBase* StackComponent;
            [FieldOffset(0x8)] public bool Active;
        }

    }

    [StructLayout(LayoutKind.Explicit, Size = 0xB0)]
    public partial struct AstralGaugeSimple
    {

       // [FieldOffset(0x28), FixedSizeArray] internal FixedSizeArray6<AstralStackSimple> _astralSoulStacks;

        [StructLayout(LayoutKind.Explicit, Size = 0x18)]
        public partial struct AstralStackSimple
        {
            [FieldOffset(0x0)] public AtkComponentBase* StackComponent;
            [FieldOffset(0x8)] public AtkResNode* GlowNode;
            [FieldOffset(0x10)] public byte Byte10;
        }
    }
}
