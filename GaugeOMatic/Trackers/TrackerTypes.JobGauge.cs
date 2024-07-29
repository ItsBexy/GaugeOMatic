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
using static FFXIVClientStructs.FFXIV.Client.UI.AddonJobHudWAR0;
using static FFXIVClientStructs.FFXIV.Client.UI.AddonJobHudWHM0;
using static GaugeOMatic.GameData.JobData.Job;

namespace GaugeOMatic.Trackers;

#region Melee

[TrackerDisplay(MNK, ToolText)]
public sealed unsafe class ChakraGaugeTracker : JobGaugeTracker<ChakraGaugeData>
{
    public override string DisplayName => "Chakra Gauge";
    public override string GaugeAddonName => "JobHudMNK1";
    public override string TermCount => "Chakras";

    private const string ToolText = "Counter: Shows Chakra Count\n" +
                                    "State: Shows if 5 chakras are up";

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

[TrackerDisplay(DRG, ToolText)]
public sealed unsafe class FirstmindsFocusTracker : JobGaugeTracker<DragonGaugeData>
{
    public override string DisplayName => "Firstminds' Focus";
    public override string GaugeAddonName => "JobHudDRG0";
    public override string TermCount => "Firstminds' Focus";

    private const string ToolText = "Counter: Shows Firstminds' Focus Count\n" +
                                    "State: Shows if 2 stacks are up";

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

[TrackerDisplay(NIN, ToolText)]
public sealed unsafe class NinkiGaugeTracker : JobGaugeTracker<NinkiGaugeData>
{
    public override string DisplayName => "Ninki Gauge";
    public override string GaugeAddonName => "JobHudNIN0";
    public override string TermGauge => "Gauge";

    private const string ToolText = "Counter: Shows spendable Ninki abilities\n" +
                                    "Gauge: Shows Ninki Value\n" +
                                    "State: If gauge is at least 50";

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

[TrackerDisplay(NIN, ToolText)]
public sealed unsafe class KazematoiTracker : JobGaugeTracker<KazematoiGaugeData>
{
    public override string DisplayName => "Kazematoi Stacks";
    public override string GaugeAddonName => "JobHudNIN1v70";
    public override string TermCount => "Stacks";
    public override string[] StateNames => new[] { "Inactive", "Active", "Full" };
    private const string ToolText = "Counter: Shows Kazematoi Stacks\n" +
                                    "State: Shows whether any stacks remain";

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

[TrackerDisplay(SAM, ToolText)]
public sealed unsafe class KenkiGaugeTracker : JobGaugeTracker<KenkiGaugeData>
{
    public override string DisplayName => "Kenki Gauge";
    public override string GaugeAddonName => "JobHudSAM0";
    private const string ToolText = "Counter: Shows maximum spendable Kenki abilities\n" +
                                    "Gauge: Shows Kenki value\n" +
                                    "State: Shows if gauge is at least 10";

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

[TrackerDisplay(SAM, ToolText)]
public sealed unsafe class MeditationGaugeTracker : JobGaugeTracker<KenkiGaugeData>
{
    public override string DisplayName => "Meditation Gauge";
    public override string GaugeAddonName => "JobHudSAM0";
    public override string TermCount => "Stacks";
    private const string ToolText = "Counter: Shows Meditation stacks\n" +
                                    "State: Shows if 3 stacks are up";

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

[TrackerDisplay(SAM, ToolText)]
public sealed unsafe class SenGaugeSetsuTracker : JobGaugeTracker<SenGaugeData>
{
    public override string DisplayName => "Sen Gauge - Setsu Seal";
    public override string GaugeAddonName => "JobHudSAM1";
    private const string ToolText = "State: Shows if Setsu is Active";

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

[TrackerDisplay(SAM, ToolText)]
public sealed unsafe class SenGaugeGetsuTracker : JobGaugeTracker<SenGaugeData>
{
    public override string DisplayName => "Sen Gauge - Getsu Seal";
    public override string GaugeAddonName => "JobHudSAM1";
    private const string ToolText = "State: Shows if Getsu is Active";

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

[TrackerDisplay(SAM, ToolText)]
public sealed unsafe class SenGaugeKaTracker : JobGaugeTracker<SenGaugeData>
{
    public override string DisplayName => "Sen Gauge - Ka Seal";
    public override string GaugeAddonName => "JobHudSAM1";
    private const string ToolText = "State: Shows if Ka is Active";

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

[TrackerDisplay(SAM, ToolText)]
public sealed unsafe class SenSealTracker : JobGaugeTracker<SenGaugeData>
{
    public override string DisplayName => "Sen Gauge - Seal Count";
    public override string GaugeAddonName => "JobHudSAM1";
    public override string[] StateNames => new[] { "None", "Higanbana", "Tenka Goken", "Midare Setsugekka" };
    private const string ToolText = "Counter/State: Shows number of seals active";

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

[TrackerDisplay(RPR, ToolText)]
public sealed unsafe class SoulGaugeTracker : JobGaugeTracker<SoulGaugeData>
{
    public override string DisplayName => "Soul Gauge";
    public override string GaugeAddonName => "JobHudRRP0";
    private const string ToolText = "Counter: Shows spendable Soul Gauge abilities\n" +
                                    "Gauge: Shows Soul Gauge value\n" +
                                    "State: Shows if gauge is at least 50";

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

[TrackerDisplay(RPR, ToolText)]
public sealed unsafe class ShroudGaugeTracker : JobGaugeTracker<SoulGaugeData>
{
    public override string DisplayName => "Shroud Gauge";
    public override string GaugeAddonName => "JobHudRRP0";
    private const string ToolText = "Counter: Shows Enshroud uses available\n" +
                                    "Gauge: Shows Shroud Gauge value\n" +
                                    "State: Shows if gauge is at least 50";

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

[TrackerDisplay(RPR, ToolText)]
public sealed unsafe class LemureShroudTracker : JobGaugeTracker<DeathGaugeData>
{
    public override string DisplayName => "Lemure Shroud";
    public override string GaugeAddonName => "JobHudRRP1";
    public override string TermCount => "Stacks";
    private const string ToolText = "Counter: Shows Lemure Shroud stacks\n" +
                                    "Gauge: Shows Enshroud timer\n" +
                                    "State: Shows if Enshroud is active";

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

[TrackerDisplay(RPR, ToolText)]
public sealed unsafe class VoidShroudTracker : JobGaugeTracker<DeathGaugeData>
{
    public override string DisplayName => "Void Shroud";
    public override string GaugeAddonName => "JobHudRRP1";
    public override string TermCount => "Stacks";
    private const string ToolText = "Counter: Shows Void Shroud stacks\n" +
                                    "Gauge: Shows Enshroud timer\n" +
                                    "State: Shows if Enshroud is active";

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

[TrackerDisplay(VPR, ToolText)]
public sealed unsafe class RattlingCoilTracker : JobGaugeTracker<VipersightGaugeData>
{
    public override string DisplayName => "Rattling Coils";
    public override string GaugeAddonName => "JobHudRDB0";
    public override string TermCount => "Stacks";
    private const string ToolText = "Counter: Shows Rattling Coils\n" +
                                    "State: Shows whether any Coils remain";

    public override TrackerData GetCurrentData(float? preview = null) =>
        1 == 1 || GaugeAddon == null ?
            new(0, 3, 0, 3, 0, 1, preview) :
            new(GaugeData->CoilStacks,
                GaugeData->CoilMax,
                GaugeData->CoilStacks,
                GaugeData->CoilMax,
                GaugeData->CoilStacks > 0 ? 1 : 0,
                1,
                preview);
}

[TrackerDisplay(VPR, ToolText)]
public sealed unsafe class SerpentGaugeTracker : JobGaugeTracker<SerpentOfferingsGaugeData>
{
    public override string DisplayName => "Serpent Offerings Gauge";
    public override string GaugeAddonName => "JobHudRDB1";
    private const string ToolText = "Counter: Shows Anguine Tribute Stacks\n" +
                                    "Gauge: Shows Serpent Offerings Gauge value\n" +
                                    "State: Shows whether gauge can be spent";

    public override TrackerData GetCurrentData(float? preview = null) =>
        1 == 1 || GaugeAddon == null ?
            new(0, 5, 0, 100, 0, 1, preview) :
            new(GaugeData->TributeStacks,
                GaugeData->TributeMax,
                GaugeData->GaugeValue,
                GaugeData->GaugeMax,
                GaugeData->GaugeValue >= GaugeData->GaugeMid ? 1 : 0,
                1,
                preview);
}

[TrackerDisplay(VPR, ToolText)]
public sealed unsafe class AnguineTributeTracker : JobGaugeTracker<SerpentOfferingsGaugeData>
{
    public override string DisplayName => "Anguine Tribute";
    public override string GaugeAddonName => "JobHudRDB1";
    public override string TermCount => "Stacks";
    private const string ToolText = "Counter: Shows Anguine Tribute Stacks\n" +
                                    "Gauge: Shows Anguine Tribute Timer\n" +
                                    "State: Shows whether any stacks remain";

    public override TrackerData GetCurrentData(float? preview = null) =>
        1 == 1 || GaugeAddon == null ?
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

[TrackerDisplay(BRD, ToolText)]
public sealed unsafe class SoulVoiceGaugeTracker : JobGaugeTracker<SongGaugeData>
{
    public override string DisplayName => "Soul Voice Gauge";
    public override string GaugeAddonName => "JobHudBRD0";
    private const string ToolText = "Counter/State: Shows if gauge is at least 80\n" +
                                    "Gauge: Shows gauge value";

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

[TrackerDisplay(MCH, ToolText)]
public sealed unsafe class HeatGaugeTracker : JobGaugeTracker<HeatGaugeData>
{
    public override string DisplayName => "Heat Gauge";
    public override string GaugeAddonName => "JobHudMCH0";
    private const string ToolText = "Counter: Shows Hypercharge uses available\n" +
                                    "Gauge: Shows Heat Gauge value\n" +
                                    "State: Shows if gauge is at least 50";

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

[TrackerDisplay(MCH, ToolText)]
public sealed unsafe class BatteryGaugeTracker : JobGaugeTracker<HeatGaugeData>
{
    public override string DisplayName => "Battery Gauge";
    public override string GaugeAddonName => "JobHudMCH0";
    private const string ToolText = "Gauge: Shows Battery Gauge value\n" +
                                    "Counter/State: Shows if gauge is at least 50";

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

[TrackerDisplay(MCH, ToolText)]
public sealed unsafe class AutomatonTracker : JobGaugeTracker<HeatGaugeData>
{
    public override string DisplayName => "Automaton Timer";
    public override string GaugeAddonName => "JobHudMCH0";
    public override string TermGauge => "Timer";
    private const string ToolText = "Gauge: Shows sSummon time left\n" +
                                    "Counter/State: Shows if Automaton Queen is active";

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

[TrackerDisplay(DNC, ToolText)]
public sealed unsafe class FourfoldTracker : JobGaugeTracker<FeatherGaugeData>
{
    public override string DisplayName => "Fourfold Feathers";
    public override string GaugeAddonName => "JobHudDNC1";
    public override string TermCount => "Feathers";
    private const string ToolText = "Counter: Shows Feather count\n" +
                                    "State: Shows if any feathers are up";

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

[TrackerDisplay(DNC, ToolText)]
public sealed unsafe class EspritGaugeTracker : JobGaugeTracker<FeatherGaugeData>
{
    public override string DisplayName => "Esprit Gauge";
    public override string GaugeAddonName => "JobHudDNC1";
    private const string ToolText = "Counter: Shows Sabre Dance uses available\n" +
                                    "Gauge: Shows Esprit Gauge value\n" +
                                    "State: Shows if gauge is at least 50";

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

[TrackerDisplay(DNC, ToolText)]
public sealed unsafe class DanceStepTracker : JobGaugeTracker<StepGaugeData>
{
    public override string DisplayName => "Dance Steps";
    public override string GaugeAddonName => "JobHudDNC0";
    public override string[] StateNames => new[] { "None", "Emboite", "Entrechat", "Jete", "Pirouette" };
    private const string ToolText = "Counter: Shows steps completed\n" +
                                    "State: Shows current step";

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

[TrackerDisplay(BLM, ToolText)]
public sealed unsafe class EnochianTracker : JobGaugeTracker<ElementalGaugeData>
{
    public override string DisplayName => "Enochian / Polyglot";
    public override string GaugeAddonName => "JobHudBLM0";
    public override string TermCount => "Polyglot Stacks";
    public override string TermGauge => "Timer";
    private const string ToolText = "Counter: Shows Polyglot stacks\n" +
                                    "Gauge: Shows Enochian timer\n" +
                                    "State: Shows if Enochian is filling";

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

[TrackerDisplay(BLM, ToolText)]
public sealed unsafe class ElementTracker : JobGaugeTracker<ElementalGaugeData>
{
    public override string DisplayName => "Element Status";
    public override string GaugeAddonName => "JobHudBLM0";
    public override string TermCount => "Element";
    public override string TermGauge => "Timer";
    public override string[] StateNames => new[] { "None", "Astral Fire", "Umbral Ice" };
    private const string ToolText = "Counter: Shows element stacks\n" +
                                    "Gauge: Shows element timer\n" +
                                    "State: Shows which element is active";

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

[TrackerDisplay(BLM, ToolText)]
public sealed unsafe class ParadoxTracker : JobGaugeTracker<ElementalGaugeData>
{
    public override string DisplayName => "Paradox";
    public override string GaugeAddonName => "JobHudBLM0";
    private const string ToolText = "Counter/State: Shows if Paradox is ready";

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

[TrackerDisplay(BLM, ToolText)]
public sealed unsafe class UmbralHeartTracker : JobGaugeTracker<ElementalGaugeData>
{
    public override string DisplayName => "Umbral Hearts";
    public override string GaugeAddonName => "JobHudBLM0";
    public override string TermCount => "Hearts";
    private const string ToolText = "Counter: Shows Umbral Heart count\n" +
                                    "State: Shows if any Umbral Hearts are available";

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

[TrackerDisplay(BLM, ToolText)]
public sealed unsafe class AstralFireTracker : JobGaugeTracker<ElementalGaugeData>
{
    public override string DisplayName => "Astral Fire";
    public override string GaugeAddonName => "JobHudBLM0";
    public override string TermCount => "Astral Fire Stacks";
    public override string TermGauge => "Timer";
    private const string ToolText = "Counter: Shows Astral Fire stacks\n" +
                                    "Gauge: Shows Astral Fire time left\n" +
                                    "State: Shows if Astral Fire is active";

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

[TrackerDisplay(BLM, ToolText)]
public sealed unsafe class UmbralIceTracker : JobGaugeTracker<ElementalGaugeData>
{
    public override string DisplayName => "Umbral Ice";
    public override string GaugeAddonName => "JobHudBLM0";
    public override string TermCount => "Umbral Ice Stacks";
    public override string TermGauge => "Timer";
    private const string ToolText = "Counter: Shows Umbral Ice stacks\n" +
                                    "Gauge: Shows Umbral Ice time left\n" +
                                    "State: Shows if Umbral Ice is active";

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

[TrackerDisplay(BLM, ToolText)]
public sealed unsafe class AstralSoulTracker : JobGaugeTracker<AstralGaugeData>
{
    public override string DisplayName => "Astral Soul Stacks";
    public override string GaugeAddonName => "JobHudBLM1";
    public override string TermCount => "Stacks";
    private const string ToolText = "Counter: Shows Astral Soul Stacks\n" +
                                    "State: Shows whether 6 stacks are up";

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

[TrackerDisplay(SMN, ToolText)]
public sealed unsafe class AetherflowSMNGaugeTracker : JobGaugeTracker<AetherflowSMNGaugeData>
{
    public override string DisplayName => "Aetherflow Gauge";
    public override string GaugeAddonName => "JobHudSMN0";
    public override string TermCount => "Stacks";
    private const string ToolText = "Counter: Shows Aetherflow stack count\n" +
                                    "State: Shows if there are any stacks available";

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

[TrackerDisplay(RDM, ToolText)]
public sealed unsafe class ManaStackTracker : JobGaugeTracker<BalanceGaugeData>
{
    public override string DisplayName => "Mana Stacks";
    public override string GaugeAddonName => "JobHudRDM0";
    public override string TermCount => "Mana Stacks";
    private const string ToolText = "Counter: Shows Mana stack count\n" +
                                    "Gauge: Shows the lesser of White or Black Mana\n" +
                                    "State: Shows if both gauges are at least 50";

    public override TrackerData GetCurrentData(float? preview = null)
    {
        if (GaugeAddon == null) return new(0, 3, 0, 100, 0, 1, preview);

        var manaBoth = (float)Math.Min(GaugeData->BlackMana, GaugeData->WhiteMana);
        return new(GaugeData->ManaStacks, 3, manaBoth, 100, manaBoth >= 50 ? 1 : 0, 1, preview);
    }
}

[TrackerDisplay(RDM, ToolText)]
public sealed unsafe class BlackManaTracker : JobGaugeTracker<BalanceGaugeData>
{
    public override string DisplayName => "Black Mana";
    public override string GaugeAddonName => "JobHudRDM0";
    private const string ToolText = "Counter: Shows spendable combos\n" +
                                    "Gauge: Shows Black Mana\n" +
                                    "State: Shows if Black Mana is at least 50";

    public override TrackerData GetCurrentData(float? preview = null) =>
        GaugeAddon == null
            ? new(0, 2, 0, 100, 0, 1, preview)
            : new((int)Math.Floor((double)(Math.Min(GaugeData->BlackMana, GaugeData->WhiteMana) / 50)), 2, GaugeData->BlackMana, 100, GaugeData->BlackMana >= 50 ? 1 : 0, 1, preview);
}

[TrackerDisplay(RDM, ToolText)]
public sealed unsafe class WhiteManaTracker : JobGaugeTracker<BalanceGaugeData>
{
    public override string DisplayName => "White Mana";
    public override string GaugeAddonName => "JobHudRDM0";
    private const string ToolText = "Counter: Shows spendable combos\n" +
                                    "Gauge: Shows White Mana\n" +
                                    "State: Shows if White Mana is at least 50";

    public override TrackerData GetCurrentData(float? preview = null) =>
        GaugeAddon == null
            ? new(0, 2, 0, 100, 0, 1, preview)
            : new((int)Math.Floor((double)(GaugeData->WhiteMana / 50)), 2, GaugeData->WhiteMana, 100, GaugeData->WhiteMana >= 50 ? 1 : 0, 1, preview);
}

[TrackerDisplay(RDM, ToolText)]
public sealed unsafe class BalanceCrystalTracker : JobGaugeTracker<BalanceGaugeData>
{
    public override string DisplayName => "Balance Crystal";
    public override string GaugeAddonName => "JobHudRDM0";
    public override string[] StateNames { get; } = { "Neutral", "Excess Black", "Excess White", "Combo Ready" };
    private const string ToolText = "State: Shows Combo/Imbalance state";

    public override TrackerData GetCurrentData(float? preview = null)
    {
        if (GaugeAddon == null) return new(0, 1, 0, 1, 0, 3, preview);

        var crystalState = ((AddonJobHudRDM0*)GaugeAddon)->GaugeStandard.CrystalState;

        var combo = crystalState == 3 ? 1 : 0;
        return new(combo, 1, combo, 1, crystalState, 3, preview);
    }
}

[TrackerDisplay(PCT, ToolText)]
public sealed unsafe class CreatureMotifDeadline : JobGaugeTracker<CanvasGaugeData>
{
    public override string DisplayName => "Creature Motif Deadline";
    public override string GaugeAddonName => "JobHudRPM0";
    private const string ToolText = "Counter: Shows charges (3)\n" +
                                    "Gauge: Shows total cooldown time (120s)\n" +
                                    "State: Shows if ready\n\n" +
                                    "Gives a value of 0 (making the tracker hideable)\nif this motif has already been painted";

    public override TrackerData GetCurrentData(float? preview = null) =>
        GaugeAddon == null || GaugeData->CreatureMotif > 0 ? new(0, 3, 0, 120, 0, 1, preview)
                                                           : new(ActionData.Actions[35347], preview);
}

[TrackerDisplay(PCT, ToolText)]
public sealed unsafe class WeaponMotifDeadline : JobGaugeTracker<CanvasGaugeData>
{
    public override string DisplayName => "Weapon Motif Deadline";
    public override string GaugeAddonName => "JobHudRPM0";
    private const string ToolText = "Counter: Shows charges (2)\n" +
                                    "Gauge: Shows total cooldown time (120s)\n" +
                                    "State: Shows if ready\n\n" +
                                    "Gives a value of 0 (making the tracker hideable)\nif this motif has already been painted";

    public override TrackerData GetCurrentData(float? preview = null) =>
        GaugeAddon == null || GaugeData->WeaponMotif ? new(0, 2, 0, 120, 0, 1, preview)
            : new(ActionData.Actions[35348], preview);
}

[TrackerDisplay(PCT, ToolText)]
public sealed unsafe class LandscapeMotifDeadline : JobGaugeTracker<CanvasGaugeData>
{
    public override string DisplayName => "Landscape Motif Deadline";
    public override string GaugeAddonName => "JobHudRPM0";
    private const string ToolText = "Gauge: Shows total cooldown time (120s)\n" +
                                    "State: Shows if ready\n\n" +
                                    "Gives a value of 0 (making the tracker hideable)\nif this motif has already been painted";

    public override TrackerData GetCurrentData(float? preview = null) =>
        GaugeAddon == null || GaugeData->LandscapeMotif ? new(0, 1, 0, 120, 0, 1, preview)
            : new(ActionData.Actions[35349], preview);
}

[TrackerDisplay(PCT, ToolText)]
public sealed unsafe class PaletteGaugeTracker : JobGaugeTracker<PaletteGaugeData>
{
    public override string DisplayName => "Palette Gauge";
    public override string GaugeAddonName => "JobHudRPM1";
    public override string TermCount => "Stacks";
    private const string ToolText = "Counter: Shows White Paint Stacks\n" +
                                    "Gauge: Shows Palette Gauge value\n" +
                                    "State: Shows whether Black Paint is available";

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

[TrackerDisplay(PLD, ToolText)]
public sealed unsafe class OathGaugeTracker : JobGaugeTracker<OathGaugeData>
{
    public override string DisplayName => "Oath Gauge";
    public override string GaugeAddonName => "JobHudPLD0";
    private const string ToolText = "Counter: Shows spendable Oath Gauge abilities\n" +
                                    "Gauge: Shows Oath Gauge value\n" +
                                    "State: Shows if tank stance is on";

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

[TrackerDisplay(WAR, ToolText)]
public sealed unsafe class BeastGaugeTracker : JobGaugeTracker<BeastGaugeData>
{
    public override string DisplayName => "Beast Gauge";
    public override string GaugeAddonName => "JobHudWAR0";
    private const string ToolText = "Counter: Shows spendable Beast Gauge abilities\n" +
                                    "Gauge: Shows Beast Gauge value\n" +
                                    "State: Shows if tank stance is on";

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

[TrackerDisplay(DRK, ToolText)]
public sealed unsafe class BloodGaugeTracker : JobGaugeTracker<BloodGaugeData>
{
    public override string DisplayName => "Blood Gauge";
    public override string GaugeAddonName => "JobHudDRK0";
    private const string ToolText = "Counter: Shows spendable Blood Gauge abilities\n" +
                                    "Gauge: Shows Blood Gauge value\n" +
                                    "State: Shows if tank stance is on";

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

[TrackerDisplay(DRK, ToolText)]
public sealed unsafe class DarksideGaugeTracker : JobGaugeTracker<DarksideGaugeData>
{
    public override string DisplayName => "Darkside Gauge";
    public override string GaugeAddonName => "JobHudDRK1";
    public override string TermGauge => "Timer";
    private const string ToolText = "Gauge: Shows Darkside timer\n" +
                                    "Counter/State: Shows if Darkside is active";

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

[TrackerDisplay(DRK, ToolText)]
public sealed unsafe class LivingShadowTracker : JobGaugeTracker<DarksideGaugeData>
{
    public override string DisplayName => "Living Shadow";
    public override string GaugeAddonName => "JobHudDRK1";
    public override string TermGauge => "Timer";
    private const string ToolText = "Gauge: Shows Living Shadow timer\n" +
                                    "Counter/State: Shows if Living Shadow is active";

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

[TrackerDisplay(GNB, ToolText)]
public sealed unsafe class PowderGaugeTracker : JobGaugeTracker<PowderGaugeData>
{
    public override string DisplayName => "Powder Gauge";
    public override string GaugeAddonName => "JobHudGNB0";
    public override string TermCount => "Cartridges";
    private const string ToolText = "Counter: Shows Cartridge Count\n" +
                                    "State: Shows if Tank Stance is on";

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

[TrackerDisplay(WHM, ToolText)]
public sealed unsafe class LilyTracker : JobGaugeTracker<HealingGaugeData>
{
    public override string DisplayName => "Lilies";
    public override string GaugeAddonName => "JobHudWHM0";
    public override string TermCount => "Lilies";
    public override string TermGauge => "Timer";
    private const string ToolText = "Counter: Shows Lilies available\n" +
                                    "Gauge: Shows Lily timer\n" +
                                    "State: Shows if Blood Lily is available";

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

[TrackerDisplay(WHM, ToolText)]
public sealed unsafe class BloodLilyTracker : JobGaugeTracker<HealingGaugeData>
{
    public override string DisplayName => "Blood Lily";
    public override string GaugeAddonName => "JobHudWHM0";
    public override string TermCount => "Blood Lily";
    public override string TermGauge => "Timer";
    private const string ToolText = "Counter: Shows Lilies spent\n" +
                                    "Gauge: Shows Lily timer\n" +
                                    "State: Shows if Blood Lily is available";

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

[TrackerDisplay(SCH, ToolText)]
public sealed unsafe class AetherflowSCHGaugeTracker : JobGaugeTracker<AetherflowACNGaugeData>
{
    public override string DisplayName => "Aetherflow Gauge";
    public override string GaugeAddonName => "JobHudACN0";
    public override string TermCount => "Stacks";
    private const string ToolText = "Counter: Shows Aetherflow stacks\n" +
                                    "State: Shows if any stacks are available";

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

[TrackerDisplay(SCH, ToolText)]
public sealed unsafe class FaerieGaugeTracker : JobGaugeTracker<FaerieGaugeData>
{
    public override string DisplayName => "Fae Aether";
    public override string GaugeAddonName => "JobHudSCH0";
    private const string ToolText = "Gauge: Shows Fae Aether value\n" +
                                    "State: Shows if Faerie is summoned";

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

[TrackerDisplay(SCH, ToolText)]
public sealed unsafe class SeraphTracker : JobGaugeTracker<FaerieGaugeData>
{
    public override string DisplayName => "Seraph Timer";
    public override string GaugeAddonName => "JobHudSCH0";
    private const string ToolText = "Gauge: Shows Seraph timer\n" +
                                    "Counter/State: Shows if Seraph is active";

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

[TrackerDisplay(SGE, ToolText)]
public sealed unsafe class EukrasiaTracker : JobGaugeTracker<EukrasiaGaugeData>
{
    public override string DisplayName => "Eukrasia";
    public override string GaugeAddonName => "JobHudGFF0";
    private const string ToolText = "Counter/State: Shows if Eukrasia is active";

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

[TrackerDisplay(SGE, ToolText)]
public sealed unsafe class AddersgallTracker : JobGaugeTracker<AddersgallGaugeData>
{
    public override string DisplayName => "Addersgall Gauge";
    public override string GaugeAddonName => "JobHudGFF1";
    public override string TermCount => "Stacks";
    public override string TermGauge => "Timer";
    private const string ToolText = "Counter: Shows Addersgall stacks\n" +
                                    "Gauge: Shows Addersgall timer\n" +
                                    "State: Shows if any stacks are available";

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

[TrackerDisplay(SGE, ToolText)]
public sealed unsafe class AdderstingTracker : JobGaugeTracker<AddersgallGaugeData>
{
    public override string DisplayName => "Addersting Counter";
    public override string GaugeAddonName => "JobHudGFF1";
    public override string TermCount => "Stacks";
    private const string ToolText = "Counter: Shows Addersting stacks\n" +
                                    "State: Shows if any stacks are available";

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
