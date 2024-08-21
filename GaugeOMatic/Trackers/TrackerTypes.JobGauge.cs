using FFXIVClientStructs.FFXIV.Client.UI;
using GaugeOMatic.GameData;
using System;
using static FFXIVClientStructs.FFXIV.Client.UI.AddonJobHudACN0;
using static FFXIVClientStructs.FFXIV.Client.UI.AddonJobHudBLM0;
using static FFXIVClientStructs.FFXIV.Client.UI.AddonJobHudBLM1;
using static FFXIVClientStructs.FFXIV.Client.UI.AddonJobHudBRD0;
using static FFXIVClientStructs.FFXIV.Client.UI.AddonJobHudDNC0;
using static FFXIVClientStructs.FFXIV.Client.UI.AddonJobHudDNC1;
using static FFXIVClientStructs.FFXIV.Client.UI.AddonJobHudDRG0;
using static FFXIVClientStructs.FFXIV.Client.UI.AddonJobHudDRK0;
using static FFXIVClientStructs.FFXIV.Client.UI.AddonJobHudDRK1;
using static FFXIVClientStructs.FFXIV.Client.UI.AddonJobHudGFF0;
using static FFXIVClientStructs.FFXIV.Client.UI.AddonJobHudGFF1;
using static FFXIVClientStructs.FFXIV.Client.UI.AddonJobHudGNB0;
using static FFXIVClientStructs.FFXIV.Client.UI.AddonJobHudMCH0;
using static FFXIVClientStructs.FFXIV.Client.UI.AddonJobHudMNK1;
using static FFXIVClientStructs.FFXIV.Client.UI.AddonJobHudNIN0;
using static FFXIVClientStructs.FFXIV.Client.UI.AddonJobHudNIN1;
using static FFXIVClientStructs.FFXIV.Client.UI.AddonJobHudPLD0;
using static FFXIVClientStructs.FFXIV.Client.UI.AddonJobHudRDB0;
using static FFXIVClientStructs.FFXIV.Client.UI.AddonJobHudRDB1;
using static FFXIVClientStructs.FFXIV.Client.UI.AddonJobHudRDM0;
using static FFXIVClientStructs.FFXIV.Client.UI.AddonJobHudRPM0;
using static FFXIVClientStructs.FFXIV.Client.UI.AddonJobHudRPM1;
using static FFXIVClientStructs.FFXIV.Client.UI.AddonJobHudRRP0;
using static FFXIVClientStructs.FFXIV.Client.UI.AddonJobHudRRP1;
using static FFXIVClientStructs.FFXIV.Client.UI.AddonJobHudSAM0;
using static FFXIVClientStructs.FFXIV.Client.UI.AddonJobHudSAM1;
using static FFXIVClientStructs.FFXIV.Client.UI.AddonJobHudSCH0;
using static FFXIVClientStructs.FFXIV.Client.UI.AddonJobHudSMN0;
using static FFXIVClientStructs.FFXIV.Client.UI.AddonJobHudSMN1;
using static FFXIVClientStructs.FFXIV.Client.UI.AddonJobHudWAR0;
using static FFXIVClientStructs.FFXIV.Client.UI.AddonJobHudWHM0;
using static GaugeOMatic.GameData.JobData.Job;

namespace GaugeOMatic.Trackers;

#region Melee

[TrackerDisplay("Chakra Gauge", MNK, null, "Shows Chakra Count", "Shows if 5 chakras are up")]
public sealed unsafe class ChakraGaugeTracker : JobGaugeTracker<ChakraGaugeData>
{
    public override string GaugeAddonName => "JobHudMNK1";
    public override string TermCount => "Chakras";

    public override TrackerData GetCurrentData(float? preview = null) =>
        GaugeAddon == null || GaugeData == null ?
            new(0, 5, 0, 5, 0, 1, preview) :
            new(GaugeData->ChakraCount,
                5,
                GaugeData->ChakraCount,
                5,
                GaugeData->ChakraCount >= 5 ? 1 : 0, 1,
                preview);
}

[TrackerDisplay("Firstminds' Focus", DRG, null, "Shows Firstminds' Focus Count", "Shows if 2 stacks are up")]
public sealed unsafe class FirstmindsFocusTracker : JobGaugeTracker<DragonGaugeData>
{
    public override string GaugeAddonName => "JobHudDRG0";
    public override string TermCount => "Firstminds' Focus";

    public override TrackerData GetCurrentData(float? preview = null) =>
        GaugeAddon == null || GaugeData == null ?
            new(0, 2, 0, 2, 0, 1, preview) :
            new(GaugeData->FirstMindsFocusCount,
                2,
                GaugeData->FirstMindsFocusCount,
                2,
                GaugeData->FirstMindsFocusCount >= 2 ? 1 : 0, 1,
                preview);
}

[TrackerDisplay("Ninki Gauge", NIN, "Shows Ninki Value", "Shows spendable Ninki abilities", "Shows if gauge is at least 50")]
public sealed unsafe class NinkiGaugeTracker : JobGaugeTracker<NinkiGaugeData>
{
    public override string GaugeAddonName => "JobHudNIN0";
    public override string TermGauge => "Gauge";

    public override TrackerData GetCurrentData(float? preview = null) =>
        GaugeAddon == null ?
            new(0, 2, 0, 100, 0, 1, preview) :
            new(GaugeData->NinkiValue / 50,
                2,
                GaugeData->NinkiValue,
                100,
                GaugeData->NinkiValue >= 50 ? 1 : 0, 1,
                preview);
}

[TrackerDisplay("Kazematoi Stacks", NIN, null, "Shows Kazematoi Stacks", "Shows Inactive/Active/Full")]
public sealed unsafe class KazematoiTracker : JobGaugeTracker<KazematoiGaugeData>
{
    public override string GaugeAddonName => "JobHudNIN1v70";
    public override string TermCount => "Stacks";
    public override string[] StateNames => ["Inactive", "Active", "Full"];

    public override TrackerData GetCurrentData(float? preview = null) =>
        GaugeAddon == null ?
            new(0, 5, 0, 5, 0, 2, preview) :
            new(GaugeData->StackCount,
                5,
                GaugeData->StackCount,
                5,
                GaugeData->StackCount == 5 ? 2 : GaugeData->StackCount > 0 ? 1 : 0,
                2,
                preview);
}

[TrackerDisplay("Kenki Gauge", SAM, "Shows Kenki value", "Shows maximum spendable Kenki abilities", "Shows if gauge is at least 10")]
public sealed unsafe class KenkiGaugeTracker : JobGaugeTracker<KenkiGaugeData>
{
    public override string GaugeAddonName => "JobHudSAM0";

    public override TrackerData GetCurrentData(float? preview = null) =>
        GaugeAddon == null || GaugeData == null ?
            new(0, 10, 0, 100, 0, 1, preview) :
            new((int)Math.Floor(GaugeData->KenkiValue / 10f),
                10,
                GaugeData->KenkiValue,
                GaugeData->KenkiMax,
                GaugeData->KenkiValue > 10 ? 1 : 0, 1,
                preview);
}

[TrackerDisplay("Meditation Gauge", SAM, null, "Shows Meditation stacks", "Shows if 3 stacks are up")]
public sealed unsafe class MeditationGaugeTracker : JobGaugeTracker<KenkiGaugeData>
{
    public override string GaugeAddonName => "JobHudSAM0";
    public override string TermCount => "Stacks";

    public override TrackerData GetCurrentData(float? preview = null) =>
        GaugeAddon == null || GaugeData == null ?
            new(0, 3, 0, 100, 0, 1, preview) :
            new(GaugeData->MeditationStackCount,
                3,
                GaugeData->MeditationStackCount,
                3,
                GaugeData->MeditationStackCount >= 3 ? 1 : 0, 1,
                preview);
}

[TrackerDisplay("Setsu Seal", SAM, null, null, "Shows if Setsu is Active")]
public sealed unsafe class SenGaugeSetsuTracker : JobGaugeTracker<SenGaugeData>
{
    public override string GaugeAddonName => "JobHudSAM1";

    public override TrackerData GetCurrentData(float? preview = null) =>
        GaugeAddon == null || GaugeData == null ?
            new(0, 1, 0, 1, 0, 1, preview) :
            new(GaugeData->HasSetsu ? 1 : 0,
                1,
                GaugeData->HasSetsu ? 1 : 0,
                1,
                GaugeData->HasSetsu ? 1 : 0, 1,
                preview);
}

[TrackerDisplay("Getsu Seal", SAM, null, null, "Shows if Getsu is Active")]
public sealed unsafe class SenGaugeGetsuTracker : JobGaugeTracker<SenGaugeData>
{
    public override string GaugeAddonName => "JobHudSAM1";

    public override TrackerData GetCurrentData(float? preview = null) =>
        GaugeAddon == null || GaugeData == null ?
            new(0, 1, 0, 1, 0, 1, preview) :
            new(GaugeData->HasGetsu ? 1 : 0,
                1,
                GaugeData->HasGetsu ? 1 : 0,
                1,
                GaugeData->HasGetsu ? 1 : 0, 1,
                preview);
}

[TrackerDisplay("Ka Seal", SAM, null, null, " Shows if Ka is Active")]
public sealed unsafe class SenGaugeKaTracker : JobGaugeTracker<SenGaugeData>
{
    public override string GaugeAddonName => "JobHudSAM1";

    public override TrackerData GetCurrentData(float? preview = null) =>
        GaugeAddon == null || GaugeData == null ?
            new(0, 1, 0, 1, 0, 1, preview) :
            new(GaugeData->HasKa ? 1 : 0,
                1,
                GaugeData->HasKa ? 1 : 0,
                1,
                GaugeData->HasKa ? 1 : 0, 1,
                preview);
}

[TrackerDisplay("Seal Count", SAM, null, "Shows number of seals active")]
public sealed unsafe class SenSealTracker : JobGaugeTracker<SenGaugeData>
{
    public override string GaugeAddonName => "JobHudSAM1";
    public override string[] StateNames => ["None", "Higanbana", "Tenka Goken", "Midare Setsugekka"];

    public override TrackerData GetCurrentData(float? preview = null)
    {
        if (GaugeAddon == null || GaugeData == null) return new(0, 3, 0, 3, 0, 3, preview);

        var sum = 0;
        if (GaugeData->HasKa) sum++;
        if (GaugeData->HasGetsu) sum++;
        if (GaugeData->HasSetsu) sum++;

        return new(sum, 3, sum, 3, sum, 3);
    }
}

[TrackerDisplay("Soul Gauge", RPR, "Shows Soul Gauge value", "Shows spendable Soul Gauge abilities", "Shows if gauge is at least 50")]
public sealed unsafe class SoulGaugeTracker : JobGaugeTracker<SoulGaugeData>
{
    public override string GaugeAddonName => "JobHudRRP0";

    public override TrackerData GetCurrentData(float? preview = null) =>
        GaugeAddon == null || GaugeData == null ?
            new(0, 2, 0, 100, 0, 1, preview) :
            new(GaugeData->SoulValue / 50,
                2,
                GaugeData->SoulValue,
                GaugeData->SoulMax,
                GaugeData->SoulValue >= 50 ? 1 : 0, 1,
                preview);
}

[TrackerDisplay("Shroud Gauge", RPR, "Shows Shroud Gauge value", "Shows Enshroud uses available", "Shows if gauge is at least 50")]
public sealed unsafe class ShroudGaugeTracker : JobGaugeTracker<SoulGaugeData>
{
    public override string GaugeAddonName => "JobHudRRP0";

    public override TrackerData GetCurrentData(float? preview = null) =>
        GaugeAddon == null || GaugeData == null ?
            new(0, 2, 0, 100, 0, 1, preview) :
            new(GaugeData->ShroudValue / 50,
                2,
                GaugeData->ShroudValue,
                GaugeData->ShroudMax,
                GaugeData->ShroudValue >= 50 ? 1 : 0, 1,
                preview);
}

[TrackerDisplay("Lemure Shroud", RPR, "Shows Enshroud timer", "Shows Lemure Shroud stacks", "Shows if Enshroud is active")]
public sealed unsafe class LemureShroudTracker : JobGaugeTracker<DeathGaugeData>
{
    public override string GaugeAddonName => "JobHudRRP1";
    public override string TermCount => "Stacks";

    public override TrackerData GetCurrentData(float? preview = null) =>
        GaugeAddon == null || GaugeData == null ?
            new(0, 5, 0, 30, 0, 1, preview) :
            new(GaugeData->LemureShroudStacks,
                5,
                GaugeData->EnshroudTimer / 1000f,
                30,
                GaugeData->EnshroudTimer > 0 ? 1 : 0, 1,
                preview);
}

[TrackerDisplay("Void Shroud", RPR, "Shows Enshroud timer", "Shows Void Shroud stacks", "Shows if Enshroud is active")]
public sealed unsafe class VoidShroudTracker : JobGaugeTracker<DeathGaugeData>
{
    public override string GaugeAddonName => "JobHudRRP1";
    public override string TermCount => "Stacks";

    public override TrackerData GetCurrentData(float? preview = null) =>
        GaugeAddon == null || GaugeData == null ?
            new(0, 5, 0, 30, 0, 1, preview) :
            new(GaugeData->VoidShroudStacks,
                5,
                GaugeData->EnshroudTimer / 1000f,
                30,
                GaugeData->EnshroudTimer > 0 ? 1 : 0, 1,
                preview);
}

[TrackerDisplay("Rattling Coils", VPR, null, "Shows Rattling Coils", "Shows whether any Coils remain")]
public sealed unsafe class RattlingCoilTracker : JobGaugeTracker<VipersightGaugeData>
{
    public override string GaugeAddonName => "JobHudRDB0";
    public override string TermCount => "Stacks";

    public override TrackerData GetCurrentData(float? preview = null) =>
        GaugeAddon == null ?
            new(0, 3, 0, 3, 0, 1, preview) :
            new(GaugeData->CoilStacks,
                GaugeData->CoilMax,
                GaugeData->CoilStacks,
                GaugeData->CoilMax,
                GaugeData->CoilStacks > 0 ? 1 : 0,
                1,
                preview);
}

[TrackerDisplay("Serpent Offerings Gauge", VPR, "Shows Serpent Offerings Gauge value", "Shows Anguine Tribute Stacks", "Shows whether gauge can be spent")]
public sealed unsafe class SerpentGaugeTracker : JobGaugeTracker<SerpentOfferingsGaugeData>
{
    public override string GaugeAddonName => "JobHudRDB1";

    public override TrackerData GetCurrentData(float? preview = null) =>
        GaugeAddon == null ?
            new(0, 5, 0, 100, 0, 1, preview) :
            new(GaugeData->TributeStacks,
                GaugeData->TributeMax,
                GaugeData->GaugeValue,
                GaugeData->GaugeMax,
                GaugeData->GaugeValue >= GaugeData->GaugeMid ? 1 : 0,
                1,
                preview);
}

[TrackerDisplay("Anguine Tribute", VPR, "Shows Anguine Tribute Timer", "Shows Anguine Tribute Stacks", "Shows whether any stacks remain")]
public sealed unsafe class AnguineTributeTracker : JobGaugeTracker<SerpentOfferingsGaugeData>
{

    public override string GaugeAddonName => "JobHudRDB1";
    public override string TermCount => "Stacks";

    public override TrackerData GetCurrentData(float? preview = null) =>
        GaugeAddon == null ?
            new(0, 5, 0, 30, 0, 1, preview) :
            new(GaugeData->TributeStacks,
                GaugeData->TributeMax,
                GaugeData->TributeTimeLeft,
                GaugeData->TributeMaxTime,
                GaugeData->TributeStacks > 0 ? 1 : 0,
                1,
                preview);
}

#endregion

#region Ranged

[TrackerDisplay("Soul Voice Gauge", BRD, "Shows gauge value", null, "Shows if gauge is at least 80")]
public sealed unsafe class SoulVoiceGaugeTracker : JobGaugeTracker<SongGaugeData>
{
    public override string GaugeAddonName => "JobHudBRD0";

    public override TrackerData GetCurrentData(float? preview = null) =>
        GaugeAddon == null || GaugeData == null ?
            new(0, 2, 0, 100, 0, 1, preview) :
            new(GaugeData->SoulVoiceValue >= 80 ? 1 : 0,
                1,
                GaugeData->SoulVoiceValue,
                GaugeData->SoulVoiceMax,
                GaugeData->SoulVoiceValue >= GaugeData->SoulVoiceMinimumNeeded ? 1 : 0, 1,
                preview);
}

[TrackerDisplay("Heat Gauge", MCH, "Shows Heat Gauge value", "Shows Hypercharge uses available", "Shows if gauge is at least 50")]
public sealed unsafe class HeatGaugeTracker : JobGaugeTracker<HeatGaugeData>
{


    public override string GaugeAddonName => "JobHudMCH0";

    public override TrackerData GetCurrentData(float? preview = null) =>
        GaugeAddon == null ?
            new(0, 2, 0, 100, 0, 1, preview) :
            new(GaugeData->HeatValue / 50,
                2,
                GaugeData->HeatValue,
                GaugeData->HeatMax,
                GaugeData->HeatValue >= 50 ? 1 : 0, 1,
                preview);
}

[TrackerDisplay("Battery Gauge", MCH, "Shows Battery Gauge value", null, "Shows if gauge is at least 50")]
public sealed unsafe class BatteryGaugeTracker : JobGaugeTracker<HeatGaugeData>
{

    public override string GaugeAddonName => "JobHudMCH0";

    public override TrackerData GetCurrentData(float? preview = null) =>
        GaugeAddon == null ?
            new(0, 2, 0, 100, 0, 1, preview) :
            new(GaugeData->BatteryValue >= 50 ? 1 : 0,
                1,
                GaugeData->BatteryValue,
                GaugeData->BatteryMax,
                GaugeData->BatteryValue >= 50 ? 1 : 0, 1,
                preview);
}

[TrackerDisplay("Automaton Timer", MCH, "Shows Summon time left", null, "Shows if Automaton Queen is active")]
public sealed unsafe class AutomatonTracker : JobGaugeTracker<HeatGaugeData>
{

    public override string GaugeAddonName => "JobHudMCH0";
    public override string TermGauge => "Timer";

    public override TrackerData GetCurrentData(float? preview = null) =>
        GaugeAddon == null ?
            new(0, 2, 0, 100, 0, 1, preview) :
            new(GaugeData->SummonActive ? 1 : 0,
                1,
                GaugeData->SummonTimeLeft / 1000f,
                12,
                GaugeData->SummonActive ? 1 : 0,
                1,
                preview);
}

[TrackerDisplay("Fourfold Feathers", DNC, null, "Shows Feather count", "Shows if any feathers are up")]
public sealed unsafe class FourfoldTracker : JobGaugeTracker<FeatherGaugeData>
{

    public override string GaugeAddonName => "JobHudDNC1";
    public override string TermCount => "Feathers";

    public override TrackerData GetCurrentData(float? preview = null) =>
        GaugeAddon == null ?
            new(0, 4, 0, 4, 0, 1, preview) :
            new(GaugeData->FeatherCount,
                4,
                GaugeData->FeatherCount,
                4,
                GaugeData->FeatherCount > 0 ? 1 : 0, 1,
                preview);
}

[TrackerDisplay("Esprit Gauge", DNC, "Shows Esprit Gauge value", "Shows Sabre Dance uses available", "Shows if gauge is at least 50")]
public sealed unsafe class EspritGaugeTracker : JobGaugeTracker<FeatherGaugeData>
{

    public override string GaugeAddonName => "JobHudDNC1";

    public override TrackerData GetCurrentData(float? preview = null) =>
        GaugeAddon == null ?
            new(0, 1, 0, 100, 0, 1, preview) :
            new(GaugeData->EspritValue / 50,
                GaugeData->EspritMax / 50,
                GaugeData->EspritValue,
                GaugeData->EspritMax,
                GaugeData->EspritValue >= 50 ? 1 : 0, 1,
                preview);
}

[TrackerDisplay("Dance Steps", DNC, null, "Shows steps completed", "Shows current step")]
public sealed unsafe class DanceStepTracker : JobGaugeTracker<StepGaugeData>
{

    public override string GaugeAddonName => "JobHudDNC0";
    public override string[] StateNames => ["None", "Emboite", "Entrechat", "Jete", "Pirouette"];

    public override TrackerData GetCurrentData(float? preview = null)
    {
        return GaugeAddon == null
                   ? new(0, 4, 0, 4, 0, 4, preview)
                   : new(GaugeData->CompletedSteps,
                         4,
                         GaugeData->CompletedSteps,
                         4,
                         GaugeData->CompletedSteps == 4 ? 0 : GaugeData->Steps[GaugeData->CompletedSteps],
                         4,
                         preview);
    }
}

#endregion

#region Caster

[TrackerDisplay("Enochian / Polyglot", BLM, "Shows Enochian timer", "Shows Polyglot stacks", "Shows if Enochian is filling")]
public sealed unsafe class EnochianTracker : JobGaugeTracker<ElementalGaugeData>
{

    public override string GaugeAddonName => "JobHudBLM0";
    public override string TermCount => "Polyglot Stacks";
    public override string TermGauge => "Timer";

    public override TrackerData GetCurrentData(float? preview = null) =>
        GaugeAddon == null || GaugeData == null ?
            new(0, 3, 0, 30, 0, 1, preview) :
            new(GaugeData->PolyglotStacks,
                3,
                GaugeData->EnochianTimer > 0 ? (GaugeData->EnochianMaxTime - GaugeData->EnochianTimer) / 1000f : 0,
                30,
                GaugeData->EnochianTimer > 0 ? 1 : 0,
                1,
                preview);
}

[TrackerDisplay("Element Status", BLM, "Shows element timer", "Shows element stacks", "Shows which element is active")]
public sealed unsafe class ElementTracker : JobGaugeTracker<ElementalGaugeData>
{

    public override string GaugeAddonName => "JobHudBLM0";
    public override string TermCount => "Element";
    public override string TermGauge => "Timer";
    public override string[] StateNames => ["None", "Astral Fire", "Umbral Ice"];

    public override TrackerData GetCurrentData(float? preview = null) =>
        GaugeAddon == null || GaugeData == null ?
            new(0, 3, 0, 15, 0, 2, preview) :
            new(Math.Abs(GaugeData->ElementStacks),
                GaugeData->ElementStackMax,
                GaugeData->ElementTimeLeft / 1000f,
                GaugeData->ElementMaxTime / 1000f,
                GaugeData->ElementStacks > 0 ? 1 : GaugeData->ElementStacks < 0 ? 2 : 0,
                2,
                preview);
}

[TrackerDisplay("Paradox", BLM, null, null, " Shows if Paradox is ready")]
public sealed unsafe class ParadoxTracker : JobGaugeTracker<ElementalGaugeData>
{
    public override string GaugeAddonName => "JobHudBLM0";

    public override TrackerData GetCurrentData(float? preview = null) =>
        GaugeAddon == null || GaugeData == null ?
            new(0, 1, 0, 1, 0, 1, preview) :
            new(GaugeData->ParadoxReady ? 1 : 0,
                1,
                GaugeData->ParadoxReady ? 1 : 0,
                1,
                GaugeData->ParadoxReady ? 1 : 0,
                1,
                preview);
}

[TrackerDisplay("Umbral Hearts", BLM, null, "Shows Umbral Heart count", "Shows if any Umbral Hearts are available")]
public sealed unsafe class UmbralHeartTracker : JobGaugeTracker<ElementalGaugeData>
{
    public override string GaugeAddonName => "JobHudBLM0";
    public override string TermCount => "Hearts";

    public override TrackerData GetCurrentData(float? preview = null) =>
        GaugeAddon == null || GaugeData == null ?
            new(0, 3, 0, 3, 0, 1, preview) :
            new(GaugeData->UmbralHearts,
                3,
                GaugeData->UmbralHearts,
                3,
                GaugeData->UmbralHearts > 0 ? 1 : 0,
                1,
                preview);
}

[TrackerDisplay("Astral Fire", BLM, "Shows Astral Fire time left", "Shows Astral Fire stacks", "Shows if Astral Fire is active")]
public sealed unsafe class AstralFireTracker : JobGaugeTracker<ElementalGaugeData>
{

    public override string GaugeAddonName => "JobHudBLM0";
    public override string TermCount => "Astral Fire Stacks";
    public override string TermGauge => "Timer";

    public override TrackerData GetCurrentData(float? preview = null)
    {
        if (GaugeAddon == null || GaugeData == null) return new(0, 3, 0, 3, 0, 1, preview);

        var isFire = GaugeData->ElementStacks > 0;
        return new(isFire ? GaugeData->ElementStacks : 0,
                   3,
                   isFire ? GaugeData->ElementTimeLeft / 1000f : 0,
                   GaugeData->ElementMaxTime / 1000f,
                   isFire ? 1 : 0,
                   1,
                   preview);
    }
}

[TrackerDisplay("Umbral Ice", BLM, "Shows Umbral Ice time left", "Shows Umbral Ice stacks", "Shows if Umbral Ice is active")]
public sealed unsafe class UmbralIceTracker : JobGaugeTracker<ElementalGaugeData>
{
    public override string GaugeAddonName => "JobHudBLM0";
    public override string TermCount => "Umbral Ice Stacks";
    public override string TermGauge => "Timer";

    public override TrackerData GetCurrentData(float? preview = null)
    {
        if (GaugeAddon == null || GaugeData == null)
            return new(0, 3, 0, 3, 0, 1, preview);

        var isIce = GaugeData->ElementStacks < 0;
        return new(isIce ? -GaugeData->ElementStacks : 0,
                   3,
                   isIce ? GaugeData->ElementTimeLeft / 1000f : 0,
                   GaugeData->ElementMaxTime / 1000f,
                   isIce ? 1 : 0,
                   1,
                   preview);
    }
}

[TrackerDisplay("Astral Soul Stacks", BLM, null, "Shows Astral Soul Stacks", "Shows whether 6 stacks are up")]
public sealed unsafe class AstralSoulTracker : JobGaugeTracker<AstralGaugeData>
{

    public override string GaugeAddonName => "JobHudBLM1";
    public override string TermCount => "Stacks";

    public override TrackerData GetCurrentData(float? preview = null) =>
        GaugeAddon == null ?
            new(0, 6, 0, 6, 0, 1, preview) :
            new(GaugeData->StackCount,
                6,
                GaugeData->StackCount,
                6,
                GaugeData->StackCount == 6 ? 1 : 0,
                1,
                preview);
}

[TrackerDisplay("Aetherflow Gauge", SMN, null, "Shows Aetherflow stack count", "Shows if there are any stacks available")]
public sealed unsafe class AetherflowSMNGaugeTracker : JobGaugeTracker<AetherflowSMNGaugeData>
{

    public override string GaugeAddonName => "JobHudSMN0";
    public override string TermCount => "Stacks";

    public override TrackerData GetCurrentData(float? preview = null) =>
        GaugeAddon == null || GaugeData == null ?
            new(0, 2, 0, 2, 0, 1, preview) :
            new(GaugeData->AetherflowStacks,
                2,
                GaugeData->AetherflowStacks,
                2,
                GaugeData->AetherflowStacks > 0 ? 1 : 0, 1,
                preview);
}

[TrackerDisplay("Fire Attunement", SMN,"Shows Egi time remaining", "Shows Fire Attunement stacks", "Shows if Ruby/Ifrit is summoned")]
public sealed unsafe class RubyTracker : JobGaugeTracker<TranceGaugeData>
{
    public override string GaugeAddonName => "JobHudSMN1";
    public override string TermCount => "Stacks";

    public override TrackerData GetCurrentData(float? preview = null)
    {
        if (GaugeAddon == null || GaugeData == null) return new(0, 2, 0, 30, 0, 1, preview);

        var egiCheck = GaugeData->CurrentEgi == 1;
        return new(egiCheck ? GaugeData->Attunement : 0, 2, egiCheck ? GaugeData->EgiTimeLeft : 0, 30, egiCheck ? 1 : 0, 1, preview);
    }
}

[TrackerDisplay("Earth Attunement", SMN, "Shows Egi time remaining", "Shows Earth Attunement stacks", "Shows if Topaz/Titan is summoned")]
public sealed unsafe class TopazTracker : JobGaugeTracker<TranceGaugeData>
{
    public override string GaugeAddonName => "JobHudSMN1";
    public override string TermCount => "Stacks";

    public override TrackerData GetCurrentData(float? preview = null)
    {
        if (GaugeAddon == null || GaugeData == null) return new(0, 4, 0, 30, 0, 1, preview);

        var egiCheck = GaugeData->CurrentEgi == 2;
        return new(egiCheck ? GaugeData->Attunement : 0, 4, egiCheck ? GaugeData->EgiTimeLeft : 0, 30, egiCheck ? 1 : 0, 1, preview);
    }
}

[TrackerDisplay("Wind Attunement", SMN, "Shows Egi time remaining", "Shows Wind Attunement charges", "Shows if Emerald/Garuda is summoned")]
public sealed unsafe class EmeraldTracker : JobGaugeTracker<TranceGaugeData>
{
    public override string GaugeAddonName => "JobHudSMN1";
    public override string TermCount => "Stacks";

    public override TrackerData GetCurrentData(float? preview = null)
    {
        if (GaugeAddon == null || GaugeData == null) return new(0, 4, 0, 30, 0, 1, preview);

        var egiCheck = GaugeData->CurrentEgi == 3;
        return new(egiCheck ? GaugeData->Attunement : 0, 4, egiCheck ? GaugeData->EgiTimeLeft : 0, 30, egiCheck ? 1 : 0, 1, preview);
    }
}

[TrackerDisplay("Summon Phase", SMN, null, null, "Shows current summon")]
public sealed unsafe class SummonTracker : JobGaugeTracker<TranceGaugeData>
{
    public override string GaugeAddonName => "JobHudSMN1";

    public override string[] StateNames { get; } =
    [
        "Inactive",  //0
        "Carbuncle", //1
        "Demi-Bahamut", //2
        "Bahamut",//3
        "Phoenix",//4
        "Solar Bahamut"//5
    ];

    public override TrackerData GetCurrentData(float? preview = null)
    {
        if (GaugeAddon == null || GaugeData == null) return new(0, 1, 0,15, 0, 5, preview);

        var lux = GaugeData->SolarBahamutEnabled;
        var baha = GaugeData->BahamutPlateEnabled;
        var carb = !lux && !baha;

        var state = GaugeData->Phase switch
        {
            1 when carb => 1,
            2 when baha => 2,
            4 when baha => 3,
            6 when baha => 4,
            8 when lux => 5,
            _ => 0
        };

        return new(state > 0 ? 1 : 0, 4, GaugeData->SummonTimeLeft, 15, state, 5, preview);
    }

}

[TrackerDisplay("Mana Stacks", RDM, "Shows the lesser of White or Black Mana", "Shows Mana stack count", "Shows if both gauges are at least 50")]
public sealed unsafe class ManaStackTracker : JobGaugeTracker<BalanceGaugeData>
{
    public override string GaugeAddonName => "JobHudRDM0";
    public override string TermCount => "Mana Stacks";

    public override TrackerData GetCurrentData(float? preview = null)
    {
        if (GaugeAddon == null) return new(0, 3, 0, 100, 0, 1, preview);

        var manaBoth = (float)Math.Min(GaugeData->BlackMana, GaugeData->WhiteMana);
        return new(GaugeData->ManaStacks, 3, manaBoth, 100, manaBoth >= 50 ? 1 : 0, 1, preview);
    }
}

[TrackerDisplay("Black Mana", RDM, "Shows Black Mana", "Shows spendable combos", "Shows if Black Mana is at least 50")]
public sealed unsafe class BlackManaTracker : JobGaugeTracker<BalanceGaugeData>
{

    public override string GaugeAddonName => "JobHudRDM0";

    public override TrackerData GetCurrentData(float? preview = null) =>
        GaugeAddon == null
            ? new(0, 2, 0, 100, 0, 1, preview)
            : new((int)Math.Floor((double)(Math.Min(GaugeData->BlackMana, GaugeData->WhiteMana) / 50)), 2, GaugeData->BlackMana, 100, GaugeData->BlackMana >= 50 ? 1 : 0, 1, preview);
}

[TrackerDisplay("White Mana", RDM, "Shows White Mana", "Shows spendable combos", "Shows if White Mana is at least 50")]
public sealed unsafe class WhiteManaTracker : JobGaugeTracker<BalanceGaugeData>
{
    public override string GaugeAddonName => "JobHudRDM0";

    public override TrackerData GetCurrentData(float? preview = null) =>
        GaugeAddon == null
            ? new(0, 2, 0, 100, 0, 1, preview)
            : new((int)Math.Floor((double)(GaugeData->WhiteMana / 50)), 2, GaugeData->WhiteMana, 100, GaugeData->WhiteMana >= 50 ? 1 : 0, 1, preview);
}

[TrackerDisplay("Balance Crystal", RDM, null, null, "Shows Combo/Imbalance state")]
public sealed unsafe class BalanceCrystalTracker : JobGaugeTracker<BalanceGaugeData>
{

    public override string GaugeAddonName => "JobHudRDM0";
    public override string[] StateNames { get; } = ["Neutral", "Excess Black", "Excess White", "Combo Ready"];

    public override TrackerData GetCurrentData(float? preview = null)
    {
        if (GaugeAddon == null) return new(0, 1, 0, 1, 0, 3, preview);

        var crystalState = ((AddonJobHudRDM0*)GaugeAddon)->GaugeStandard.CrystalState;

        var combo = crystalState == 3 ? 1 : 0;
        return new(combo, 1, combo, 1, crystalState, 3, preview);
    }
}

[TrackerDisplay("Creature Motif Deadline", PCT, "Shows total recast time", "Shows charges", "Shows if ready", "Gives a value of 0 (making the tracker hideable)\nif this motif has already been painted")]
public sealed unsafe class CreatureMotifDeadline : JobGaugeTracker<CanvasGaugeData>
{

    public override string GaugeAddonName => "JobHudRPM0";

    public override TrackerData GetCurrentData(float? preview = null)
    {
        var action = (ActionRef)35347;
        return GaugeAddon == null || GaugeData->CreatureMotif > 0
                   ? new(0, action.GetMaxCharges(), 0, action.GetCooldownTotal(), 0, 1, preview)
                   : action.GetTrackerData(preview);
    }
}

[TrackerDisplay("Weapon Motif Deadline", PCT, "Shows total recast time", "Shows charges", "Shows if ready", "Gives a value of 0 (making the tracker hideable)\nif this motif has already been painted")]
public sealed unsafe class WeaponMotifDeadline : JobGaugeTracker<CanvasGaugeData>
{
    public override string GaugeAddonName => "JobHudRPM0";

    public override TrackerData GetCurrentData(float? preview = null)
    {
        var action = (ActionRef)35348;
        return GaugeAddon == null || GaugeData->WeaponMotif
                   ? new(0, action.GetMaxCharges(), 0, action.GetCooldownTotal(), 0, 1, preview)
                   : action.GetTrackerData(preview);
    }
}

[TrackerDisplay("Landscape Motif Deadline", PCT, "Shows total recast time", "Shows if ready", "Shows if ready", "Gives a value of 0 (making the tracker hideable)\nif this motif has already been painted")]
public sealed unsafe class LandscapeMotifDeadline : JobGaugeTracker<CanvasGaugeData>
{
    public override string GaugeAddonName => "JobHudRPM0";

    public override TrackerData GetCurrentData(float? preview = null)
    {
        var action = (ActionRef)35349;
        return GaugeAddon == null || GaugeData->LandscapeMotif
                   ? new(0, action.GetMaxCharges(), 0, action.GetCooldownTotal(), 0, 1, preview)
                   : action.GetTrackerData(preview);
    }
}

[TrackerDisplay("Palette Gauge", PCT, "Shows Palette Gauge value", "Shows White Paint Stacks", "Shows whether Black Paint is available")]
public sealed unsafe class PaletteGaugeTracker : JobGaugeTracker<PaletteGaugeData>
{
    public override string GaugeAddonName => "JobHudRPM1";
    public override string TermCount => "Stacks";

    public override TrackerData GetCurrentData(float? preview = null) =>
        GaugeAddon == null ?
            new(0, 5, 0, 5, 0, 1, preview) :
            new(GaugeData->WhitePaint,
                5,
                GaugeData->PaletteValue,
                GaugeData->PaletteMax,
                GaugeData->BlackPaint > 0 ? 1 : 0,
                1,
                preview);
}

#endregion

#region Tank

[TrackerDisplay("Oath Gauge", PLD, "Shows Oath Gauge value", "Shows spendable Oath Gauge abilities", "Shows if tank stance is on")]
public sealed unsafe class OathGaugeTracker : JobGaugeTracker<OathGaugeData>
{
    public override string GaugeAddonName => "JobHudPLD0";

    public override TrackerData GetCurrentData(float? preview = null) =>
        GaugeAddon == null ?
            new(0, 2, 0, 100, 0, 1, preview) :
            new(GaugeData->OathValue / 50,
                2,
                GaugeData->OathValue,
                100,
                GaugeData->TankStance ? 1 : 0, 1,
                preview);
}

[TrackerDisplay("Beast Gauge", WAR, "Shows Beast Gauge value", "Shows spendable Beast Gauge abilities", "Shows if tank stance is on")]
public sealed unsafe class BeastGaugeTracker : JobGaugeTracker<BeastGaugeData>
{

    public override string GaugeAddonName => "JobHudWAR0";

    public override TrackerData GetCurrentData(float? preview = null) =>
        GaugeAddon == null ?
            new(0, 2, 0, 100, 0, 1, preview) :
            new(GaugeData->BeastValue / 50,
                2,
                GaugeData->BeastValue,
                100,
                GaugeData->TankStance ? 1 : 0, 1,
                preview);
}

[TrackerDisplay("JobHudDRK0", DRK, "Shows Blood Gauge value", "Shows spendable Blood Gauge abilities", "Shows if tank stance is on")]
public sealed unsafe class BloodGaugeTracker : JobGaugeTracker<BloodGaugeData>
{

    public override string GaugeAddonName => "JobHudDRK0";

    public override TrackerData GetCurrentData(float? preview = null) =>
        GaugeAddon == null ?
            new(0, 2, 0, 100, 0, 1, preview) :
            new(GaugeData->BloodValue / 50,
                2,
                GaugeData->BloodValue,
                GaugeData->BloodMax,
                GaugeData->TankStance ? 1 : 0, 1,
                preview);
}

[TrackerDisplay("JobHudDRK1", DRK, " Shows Darkside timer", null, "Shows if Darkside is active")]
public sealed unsafe class DarksideGaugeTracker : JobGaugeTracker<DarksideGaugeData>
{

    public override string GaugeAddonName => "JobHudDRK1";
    public override string TermGauge => "Timer";

    public override TrackerData GetCurrentData(float? preview = null) =>
        GaugeAddon == null ?
            new(0, 2, 0, 100, 0, 1, preview) :
            new(GaugeData->DarksideTimeLeft > 0 ? 1 : 0,
                1,
                GaugeData->DarksideTimeLeft / 1000f,
                GaugeData->DarksideTimeMax / 1000f,
                GaugeData->DarkArtsActive ? 1 : 0, 1,
                preview);
}

[TrackerDisplay("JobHudDRK1", DRK, "Shows Living Shadow timer", null, "Shows if Living Shadow is active")]
public sealed unsafe class LivingShadowTracker : JobGaugeTracker<DarksideGaugeData>
{
    public override string GaugeAddonName => "JobHudDRK1";
    public override string TermGauge => "Timer";

    public override TrackerData GetCurrentData(float? preview = null) =>
        GaugeAddon == null ?
            new(0, 2, 0, 100, 0, 1, preview) :
            new(GaugeData->LivingShadowTimeLeft > 0 ? 1 : 0,
                1,
                GaugeData->LivingShadowTimeLeft / 1000f,
                20,
                GaugeData->LivingShadowTimeLeft > 0 ? 1 : 0, 1,
                preview);
}

[TrackerDisplay("JobHudGNB0", GNB, null, "Shows Cartridge Count", "Shows if Tank Stance is on")]
public sealed unsafe class PowderGaugeTracker : JobGaugeTracker<PowderGaugeData>
{
    public override string GaugeAddonName => "JobHudGNB0";
    public override string TermCount => "Cartridges";

    public override TrackerData GetCurrentData(float? preview = null) =>
        GaugeAddon == null ?
            new(0, 3, 0, 100, 0, 1, preview) :
            new(GaugeData->Ammo,
                3,
                GaugeData->Ammo,
                3,
                GaugeData->TankStance ? 1 : 0, 1,
                preview);
}

#endregion

#region Healer

[TrackerDisplay("Lilies", WHM, "Shows Lily timer", "Shows Lilies available", "Shows if Blood Lily is available")]
public sealed unsafe class LilyTracker : JobGaugeTracker<HealingGaugeData>
{
    public override string GaugeAddonName => "JobHudWHM0";
    public override string TermCount => "Lilies";
    public override string TermGauge => "Timer";

    public override TrackerData GetCurrentData(float? preview = null) =>
        GaugeAddon == null || GaugeData == null ?
            new(0, 3, 0, 20, 0, 1, preview) :
            new(GaugeData->LilyCount,
                3,
                GaugeData->LilyTimer / 1000f,
                GaugeData->LilyTimerMax / 1000f,
                GaugeData->LiliesSpent >= 3 ? 1 : 0, 1,
                preview);
}

[TrackerDisplay("Blood Lily", WHM, "Shows Lily timer", "Shows Lilies spent", "Shows if Blood Lily is available")]
public sealed unsafe class BloodLilyTracker : JobGaugeTracker<HealingGaugeData>
{

    public override string GaugeAddonName => "JobHudWHM0";
    public override string TermCount => "Blood Lily";
    public override string TermGauge => "Timer";

    public override TrackerData GetCurrentData(float? preview = null) =>
        GaugeAddon == null || GaugeData == null ?
            new(0, 3, 0, 20, 0, 1, preview) :
            new(GaugeData->LiliesSpent,
                3,
                GaugeData->LilyTimer / 1000f,
                GaugeData->LilyTimerMax / 1000f,
                GaugeData->LiliesSpent >= 3 ? 1 : 0, 1,
                preview);
}

[TrackerDisplay("Aetherflow Gauge", SCH, null, "Shows Aetherflow stacks", "Shows if any stacks are available")]
public sealed unsafe class AetherflowSCHGaugeTracker : JobGaugeTracker<AetherflowACNGaugeData>
{

    public override string GaugeAddonName => "JobHudACN0";
    public override string TermCount => "Stacks";

    public override TrackerData GetCurrentData(float? preview = null) =>
        GaugeAddon == null || GaugeData == null ?
            new(0, 3, 0, 3, 0, 1, preview) :
            new(GaugeData->AetherflowStacks,
                3,
                GaugeData->AetherflowStacks,
                3,
                GaugeData->AetherflowStacks > 0 ? 1 : 0, 1,
                preview);
}

[TrackerDisplay("Fae Aether", SCH, "Shows Fae Aether value", null, "Shows if Faerie is summoned")]
public sealed unsafe class FaerieGaugeTracker : JobGaugeTracker<FaerieGaugeData>
{

    public override string GaugeAddonName => "JobHudSCH0";

    public override TrackerData GetCurrentData(float? preview = null) =>
        GaugeAddon == null || GaugeData == null ?
            new(0, 3, 0, 100, 0, 1, preview) :
            new(GaugeData->FaeValue > 0 ? 1 : 0,
                1,
                GaugeData->FaeValue,
                GaugeData->FaeMax,
                GaugeData->FaerieSummoned ? 1 : 0, 1,
                preview);
}

[TrackerDisplay("Seraph Timer", SCH, "Shows Seraph timer", null, "Shows if Seraph is active")]
public sealed unsafe class SeraphTracker : JobGaugeTracker<FaerieGaugeData>
{
    public override string GaugeAddonName => "JobHudSCH0";

    public override TrackerData GetCurrentData(float? preview = null) =>
        GaugeAddon == null || GaugeData == null ?
            new(0, 3, 0, 22, 0, 1, preview) :
            new(GaugeData->SeraphTimeLeft > 0 ? 1 : 0,
                1,
                GaugeData->SeraphTimeLeft / 1000f,
                GaugeData->SeraphMaxTime / 1000f,
                GaugeData->SeraphTimeLeft > 0 ? 1 : 0, 1,
                preview);
}

[TrackerDisplay("Eukrasia", SGE, null, null, "Shows if Eukrasia is active")]
public sealed unsafe class EukrasiaTracker : JobGaugeTracker<EukrasiaGaugeData>
{

    public override string GaugeAddonName => "JobHudGFF0";

    public override TrackerData GetCurrentData(float? preview = null) =>
        GaugeAddon == null || GaugeData == null ?
            new(0, 1, 0, 1, 0, 1, preview) :
            new(GaugeData->EukrasiaActive ? 1 : 0,
                1,
                GaugeData->EukrasiaActive ? 1 : 0,
                1,
                GaugeData->EukrasiaActive ? 1 : 0, 1,
                preview);
}

[TrackerDisplay("Addersgall Gauge", SGE, "Shows Addersgall timer", "Shows Addersgall stacks", "Shows if any stacks are available")]
public sealed unsafe class AddersgallTracker : JobGaugeTracker<AddersgallGaugeData>
{

    public override string GaugeAddonName => "JobHudGFF1";
    public override string TermCount => "Stacks";
    public override string TermGauge => "Timer";

    public override TrackerData GetCurrentData(float? preview = null) =>
        GaugeAddon == null || GaugeData == null ?
            new(0, 3, 0, 20, 0, 1, preview) :
            new(GaugeData->Addersgall,
                3,
                GaugeData->AddersgallTimer / 1000f,
                GaugeData->AddersgallTimerMax / 1000f,
                GaugeData->Addersgall > 0 ? 1 : 0, 1,
                preview);
}

[TrackerDisplay("Addersting Counter", SGE, null, "Shows Addersting stacks", "Shows if any stacks are available")]
public sealed unsafe class AdderstingTracker : JobGaugeTracker<AddersgallGaugeData>
{

    public override string GaugeAddonName => "JobHudGFF1";
    public override string TermCount => "Stacks";

    public override TrackerData GetCurrentData(float? preview = null) =>
        GaugeAddon == null || GaugeData == null ?
            new(0, 3, 0, 3, 0, 1, preview) :
            new(GaugeData->Addersting,
                3,
                GaugeData->Addersting,
                3,
                GaugeData->Addersting > 0 ? 1 : 0, 1,
                preview);
}

#endregion
