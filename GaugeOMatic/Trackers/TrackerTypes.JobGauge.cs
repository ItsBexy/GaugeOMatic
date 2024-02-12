using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.UI;
using System;
using static Dalamud.Interface.FontAwesomeIcon;
using static FFXIVClientStructs.FFXIV.Client.UI.AddonJobHudACN0;
using static FFXIVClientStructs.FFXIV.Client.UI.AddonJobHudBLM0;
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
using static FFXIVClientStructs.FFXIV.Client.UI.AddonJobHudRDM0;
using static FFXIVClientStructs.FFXIV.Client.UI.AddonJobHudRRP0;
using static FFXIVClientStructs.FFXIV.Client.UI.AddonJobHudRRP1;
using static FFXIVClientStructs.FFXIV.Client.UI.AddonJobHudSAM0;
using static FFXIVClientStructs.FFXIV.Client.UI.AddonJobHudSAM1;
using static FFXIVClientStructs.FFXIV.Client.UI.AddonJobHudSCH0;
using static FFXIVClientStructs.FFXIV.Client.UI.AddonJobHudSMN0;
using static FFXIVClientStructs.FFXIV.Client.UI.AddonJobHudWAR0;
using static FFXIVClientStructs.FFXIV.Client.UI.AddonJobHudWHM0;
using static GaugeOMatic.GameData.JobData.Job;
using static GaugeOMatic.Trackers.Tracker.IconColor;

namespace GaugeOMatic.Trackers;

#region Melee

[TrackerDisplay(Gauge, JobGaugeColor, "Job Gauge Tracker", MNK)]
public sealed unsafe class ChakraGaugeTracker : JobGaugeTracker<ChakraGaugeData>
{
    public override string DisplayName => "Chakra Gauge";
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

[TrackerDisplay(Gauge, JobGaugeColor, "Job Gauge Tracker", DRG)]
public sealed unsafe class LotDTracker : JobGaugeTracker<DragonGaugeData>
{
    public override string DisplayName => "Life of the Dragon";
    public override string GaugeAddonName => "JobHudDRG0";
    public override string TermCount => "Gaze";
    public override string TermGauge => "Timer";

    public override TrackerData GetCurrentData(float? preview = null) =>
        GaugeAddon == null || GaugeData == null ?
            new(0, 2, 0, 100, 0, 1, preview) :
            new(GaugeData->EyeCount,
                2,
                GaugeData->LotDTimer / 1000f,
                GaugeData->LotDMax / 1000f,
                GaugeData->EyeCount >= 2 ? 1 : 0, 1,
                preview);
}

[TrackerDisplay(Gauge, JobGaugeColor, "Job Gauge Tracker", DRG)]
public sealed unsafe class FirstmindsFocusTracker : JobGaugeTracker<DragonGaugeData>
{
    public override string DisplayName => "Firstminds' Focus";
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

[TrackerDisplay(Gauge, JobGaugeColor, "Job Gauge Tracker", NIN)]
public sealed unsafe class NinkiGaugeTracker : JobGaugeTracker<NinkiGaugeData>
{
    public override string DisplayName => "Ninki Gauge";
    public override string GaugeAddonName => "JobHudNIN0";
    public override string TermGauge => "Ninki";

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

[TrackerDisplay(Gauge, JobGaugeColor, "Job Gauge Tracker", NIN)]
public sealed unsafe class HutonGaugeTracker : JobGaugeTracker<HutonGaugeData>
{
    public override string DisplayName => "Huton Gauge";
    public override string GaugeAddonName => "JobHudNIN1";
    public override string TermGauge => "Timer";

    public override TrackerData GetCurrentData(float? preview = null) =>
        GaugeAddon == null || GaugeData == null ?
            new(0, 1, 0, 60, 0, 1, preview) :
            new(GaugeData->TimeLeft > 0 ? 1 : 0,
                1,
                GaugeData->TimeLeft / 1000f,
                60,
                GaugeData->TimeLeft > 0 ? 1 : 0, 1,
                preview);
}

[TrackerDisplay(Gauge, JobGaugeColor, "Job Gauge Tracker", SAM)]
public sealed unsafe class KenkiGaugeTracker : JobGaugeTracker<KenkiGaugeData>
{
    public override string DisplayName => "Kenki Gauge";
    public override string GaugeAddonName => "JobHudSAM0";

    public override TrackerData GetCurrentData(float? preview = null) =>
        GaugeAddon == null || GaugeData == null ?
            new(0, 3, 0, 100, 0, 1, preview) :
            new((int)Math.Floor(GaugeData->KenkiValue / 10f),
                10,
                GaugeData->KenkiValue,
                GaugeData->KenkiMax,
                GaugeData->KenkiValue > 10 ? 1 : 0, 1,
                preview);
}

[TrackerDisplay(Gauge, JobGaugeColor, "Meditation Gauge", SAM)]
public sealed unsafe class MeditationGaugeTracker : JobGaugeTracker<KenkiGaugeData>
{
    public override string DisplayName => "";
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

[TrackerDisplay(Gauge, JobGaugeColor, "Job Gauge Tracker", SAM)]
public sealed unsafe class SenGaugeSetsuTracker : JobGaugeTracker<SenGaugeData>
{
    public override string DisplayName => "Sen Gauge - Setsu Seal";
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

[TrackerDisplay(Gauge, JobGaugeColor, "Job Gauge Tracker", SAM)]
public sealed unsafe class SenGaugeGetsuTracker : JobGaugeTracker<SenGaugeData>
{
    public override string DisplayName => "Sen Gauge - Getsu Seal";
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

[TrackerDisplay(Gauge, JobGaugeColor, "Job Gauge Tracker", SAM)]
public sealed unsafe class SenGaugeKaTracker : JobGaugeTracker<SenGaugeData>
{
    public override string DisplayName => "Sen Gauge - Ka Seal";
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

[TrackerDisplay(Gauge, JobGaugeColor, "Job Gauge Tracker", SAM)]
public sealed unsafe class SenSealTracker : JobGaugeTracker<SenGaugeData>
{
    public override string DisplayName => "Sen Gauge - Seal Count";
    public override string GaugeAddonName => "JobHudSAM1";
    public override string[] StateNames => new[] { "None", "Higanbana", "Tenka Goken", "Midare Setsugekka" };

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

[TrackerDisplay(Gauge, JobGaugeColor, "Job Gauge Tracker", RPR)]
public sealed unsafe class SoulGaugeTracker : JobGaugeTracker<SoulGaugeData>
{
    public override string DisplayName => "Soul Gauge";
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

[TrackerDisplay(Gauge, JobGaugeColor, "Job Gauge Tracker", RPR)]
public sealed unsafe class ShroudGaugeTracker : JobGaugeTracker<SoulGaugeData>
{
    public override string DisplayName => "Shroud Gauge";
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

[TrackerDisplay(Gauge, JobGaugeColor, "Job Gauge Tracker", RPR)]
public sealed unsafe class LemureShroudTracker : JobGaugeTracker<DeathGaugeData>
{
    public override string DisplayName => "Lemure Shroud";
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

[TrackerDisplay(Gauge, JobGaugeColor, "Job Gauge Tracker", RPR)]
public sealed unsafe class VoidShroudTracker : JobGaugeTracker<DeathGaugeData>
{
    public override string DisplayName => "Void Shroud";
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

#endregion

#region Ranged

[TrackerDisplay(Gauge, JobGaugeColor, "Job Gauge Tracker", BRD)]
public sealed unsafe class SoulVoiceGaugeTracker : JobGaugeTracker<SongGaugeData>
{
    public override string DisplayName => "Soul Voice Gauge";
    public override string GaugeAddonName => "JobHudBRD0";

    public override TrackerData GetCurrentData(float? preview = null) =>
        GaugeAddon == null || GaugeData == null ?
            new(0, 2, 0, 100, 0, 1, preview) :
            new(GaugeData->SoulVoiceValue >= GaugeData->SoulVoiceMinimumNeeded ? 1 : 0,
                1,
                GaugeData->SoulVoiceValue,
                GaugeData->SoulVoiceMax,
                GaugeData->SoulVoiceValue >= GaugeData->SoulVoiceMinimumNeeded ? 1 : 0, 1,
                preview);
}

[TrackerDisplay(Gauge, JobGaugeColor, "Job Gauge Tracker", MCH)]
public sealed unsafe class HeatGaugeTracker : JobGaugeTracker<HeatGaugeData>
{
    public override string DisplayName => "Heat Gauge";
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

[TrackerDisplay(Gauge, JobGaugeColor, "Job Gauge Tracker", MCH)]
public sealed unsafe class BatteryGaugeTracker : JobGaugeTracker<HeatGaugeData>
{
    public override string DisplayName => "Battery Gauge";
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

[TrackerDisplay(Gauge, JobGaugeColor, "Job Gauge Tracker", MCH)]
public sealed unsafe class AutomatonTracker : JobGaugeTracker<HeatGaugeData>
{
    public override string DisplayName => "Automaton Timer";
    public override string GaugeAddonName => "JobHudMCH0";
    public override string TermGauge => "Timer";

    public override TrackerData GetCurrentData(float? preview = null) =>
        GaugeAddon == null ?
            new(0, 2, 0, 100, 0, 1, preview) :
            new(GaugeData->SummonActive ? 1 : 0,
                1,
                GaugeData->SummonTimeLeft/1000f,
                12,
                GaugeData->SummonActive ? 1 : 0,
                1,
                preview);
}

[TrackerDisplay(Gauge, JobGaugeColor, "Job Gauge Tracker", DNC)]
public sealed unsafe class FourfoldTracker : JobGaugeTracker<FeatherGaugeData>
{
    public override string DisplayName => "Fourfold Feathers";
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

[TrackerDisplay(Gauge, JobGaugeColor, "Job Gauge Tracker", DNC)]
public sealed unsafe class EspritGaugeTracker : JobGaugeTracker<FeatherGaugeData>
{
    public override string DisplayName => "Esprit Gauge";
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

[TrackerDisplay(Gauge, JobGaugeColor, "Job Gauge Tracker", DNC)]
public sealed unsafe class DanceStepTracker : JobGaugeTracker<StepGaugeData>
{
    public override string DisplayName => "Dance Steps";
    public override string GaugeAddonName => "JobHudDNC0";
    public override string[] StateNames => new[] { "None", "Emboite", "Entrechat", "Jete", "Pirouette" };

    public override TrackerData GetCurrentData(float? preview = null) =>
        GaugeAddon == null
            ? new(0, 4, 0, 4, 0, 4, preview)
            : new(GaugeData->CompletedSteps,
                  4,
                  GaugeData->CompletedSteps,
                  4,
                  GaugeData->CompletedSteps == 4 ? 0 : GaugeData->Steps[GaugeData->CompletedSteps],
                  4,
                  preview);
}

#endregion

#region Caster

[TrackerDisplay(Gauge, JobGaugeColor, "Job Gauge Tracker", BLM)]
public sealed unsafe class EnochianTracker : JobGaugeTracker<ElementalGaugeData>
{
    public override string DisplayName => "Enochian / Polyglot";
    public override string GaugeAddonName => "JobHudBLM0";
    public override string TermCount => "Polyglot Stacks";
    public override string TermGauge => "Timer";

    public override TrackerData GetCurrentData(float? preview = null) =>
        GaugeAddon == null || GaugeData == null ?
            new(0, 2, 0, 30, 0, 1, preview) :
            new(GaugeData->PolyglotStacks,
                2,
                GaugeData->EnochianTimer > 0 ? (GaugeData->EnochianMaxTime - GaugeData->EnochianTimer) / 1000f:0,
                30,
                GaugeData->EnochianTimer > 0 ? 1 : 0,
                1,
                preview);
}

[TrackerDisplay(Gauge, JobGaugeColor, "Job Gauge Tracker", BLM)]
public sealed unsafe class ElementTracker : JobGaugeTracker<ElementalGaugeData>
{
    public override string DisplayName => "Element Status";
    public override string GaugeAddonName => "JobHudBLM0";
    public override string TermCount => "Element";
    public override string TermGauge => "Timer";
    public override string[] StateNames => new[] { "None", "Astral Fire", "Umbral Ice" };

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

[TrackerDisplay(Gauge, JobGaugeColor, "Job Gauge Tracker", BLM)]
public sealed unsafe class ParadoxTracker : JobGaugeTracker<ElementalGaugeData>
{
    public override string DisplayName => "Paradox";
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

[TrackerDisplay(Gauge, JobGaugeColor, "Job Gauge Tracker", BLM)]
public sealed unsafe class UmbralHeartTracker : JobGaugeTracker<ElementalGaugeData>
{
    public override string DisplayName => "Umbral Hearts";
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

[TrackerDisplay(Gauge, JobGaugeColor, "Job Gauge Tracker", BLM)]
public sealed unsafe class AstralFireTracker : JobGaugeTracker<ElementalGaugeData>
{
    public override string DisplayName => "Astral Fire";
    public override string GaugeAddonName => "JobHudBLM0";
    public override string TermCount => "Astral Fire Stacks";
    public override string TermGauge => "Timer";

    public override TrackerData GetCurrentData(float? preview = null)
    {
        if (GaugeAddon == null || GaugeData == null) return new(0, 3, 0, 3, 0, 1, preview);

        var isFire = GaugeData->ElementStacks > 0;
        return new(isFire?GaugeData->ElementStacks:0,
                   3,
                   isFire?GaugeData->ElementTimeLeft / 1000f : 0,
                   GaugeData->ElementMaxTime/1000f,
                   isFire ? 1 : 0,
                   1,
                   preview);
    }
}

[TrackerDisplay(Gauge, JobGaugeColor, "Job Gauge Tracker", BLM)]
public sealed unsafe class UmbralIceTracker : JobGaugeTracker<ElementalGaugeData>
{
    public override string DisplayName => "Umbral Ice";
    public override string GaugeAddonName => "JobHudBLM0";
    public override string TermCount => "Umbral Ice Stacks";
    public override string TermGauge => "Timer";

    public override TrackerData GetCurrentData(float? preview = null)
    {
        if (GaugeAddon == null || GaugeData == null)
            return new(0, 3, 0, 3, 0, 1, preview);

        var isIce = GaugeData->ElementStacks < 0;
        return new(isIce ? -GaugeData->ElementStacks:0,
                   3,
                   isIce ? GaugeData->ElementTimeLeft / 1000f : 0,
                   GaugeData->ElementMaxTime / 1000f,
                   isIce ? 1 : 0,
                   1,
                   preview);
    }
}

[TrackerDisplay(Gauge, JobGaugeColor, "Job Gauge Tracker", SMN)]
public sealed unsafe class AetherflowSMNGaugeTracker : JobGaugeTracker<AetherflowSMNGaugeData>
{
    public override string DisplayName => "Aetherflow Gauge";
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

[TrackerDisplay(Gauge, JobGaugeColor, "Job Gauge Tracker", RDM)]
public sealed unsafe class ManaStackTracker : JobGaugeTracker<BalanceGaugeData>
{
    public override string DisplayName => "Mana Stacks";
    public override string GaugeAddonName => "JobHudRDM0";
    public override string TermCount => "Mana Stacks";

    public override TrackerData GetCurrentData(float? preview = null)
    {
        if (GaugeAddon == null) return new(0, 3, 0, 100, 0, 1, preview);

        var manaBoth = (float)Math.Min(GaugeData->BlackMana, GaugeData->WhiteMana);
        return new(GaugeData->ManaStacks, 3, manaBoth, 100, manaBoth >= 50 ? 1 : 0, 1, preview);
    }
}

[TrackerDisplay(Gauge, JobGaugeColor, "Job Gauge Tracker", RDM)]
public sealed unsafe class BlackManaTracker : JobGaugeTracker<BalanceGaugeData>
{
    public override string DisplayName => "Black Mana";
    public override string GaugeAddonName => "JobHudRDM0";

    public override TrackerData GetCurrentData(float? preview = null) =>
        GaugeAddon == null
            ? new(0, 2, 0, 100, 0, 1, preview)
            : new((int)Math.Floor((double)(GaugeData->BlackMana / 50)), 2, GaugeData->BlackMana, 100, GaugeData->BlackMana >= 50 ? 1 : 0, 1, preview);
}

[TrackerDisplay(Gauge, JobGaugeColor, "Job Gauge Tracker", RDM)]
public sealed unsafe class WhiteManaTracker : JobGaugeTracker<BalanceGaugeData>
{
    public override string DisplayName => "White Mana";
    public override string GaugeAddonName => "JobHudRDM0";

    public override TrackerData GetCurrentData(float? preview = null) =>
        GaugeAddon == null
            ? new(0, 2, 0, 100, 0, 1, preview)
            : new((int)Math.Floor((double)(GaugeData->WhiteMana / 50)), 2, GaugeData->WhiteMana, 100, GaugeData->WhiteMana >= 50 ? 1 : 0, 1, preview);
}

[TrackerDisplay(Gauge, JobGaugeColor, "Job Gauge Tracker", RDM)]
public sealed unsafe class BalanceCrystalTracker : JobGaugeTracker<BalanceGaugeData>
{
    public override string DisplayName => "Balance Crystal";
    public override string GaugeAddonName => "JobHudRDM0";
    public override string[] StateNames { get; } = { "Neutral", "Black Imbalance", "White Imbalance", "Combo Ready" };

    public override TrackerData GetCurrentData(float? preview = null)
    {
        if (GaugeAddon == null) return new(0, 1, 0, 1, 0, 3, preview);

        var crystalState = ((AddonJobHudRDM0*)GaugeAddon)->GaugeStandard.CrystalState;
        var combo = crystalState == 3 ? 1 : 0;
        return new(combo, 1, combo, 1, crystalState, 3, preview);
    }
}

[TrackerDisplay(Gauge, JobGaugeColor, "Special Tracker", RDM)]
public sealed unsafe class WeaveMetronome : Tracker
{
    public override string DisplayName => "Weave Metronome";
    public override RefType RefType => RefType.Action;
    public override TrackerData GetCurrentData(float? preview = null)
    {
        var actionManager = ActionManager.Instance();
        var adjustedId = actionManager->GetAdjustedActionId(7524);
        var timeElapsed = actionManager->GetRecastTimeElapsed(ActionType.Action, adjustedId);
        var timeTotal = actionManager->GetRecastTime(ActionType.Action, adjustedId);

        var gcd = timeElapsed;
        if (timeTotal == 0)
        {
            gcd = 0;
            timeTotal = 1;
        }

        var casting = ClientState.LocalPlayer?.IsCasting ?? false;

        return new(0, 1, gcd, timeTotal, casting ? 1 : 0, 1, preview);
    }
}


#endregion

#region Tank

[TrackerDisplay(Gauge, JobGaugeColor, "Job Gauge Tracker", PLD)]
public sealed unsafe class OathGaugeTracker : JobGaugeTracker<OathGaugeData>
{
    public override string DisplayName => "Oath Gauge";
    public override string GaugeAddonName => "JobHudPLD0";

    public override TrackerData GetCurrentData(float? preview = null) =>
        GaugeAddon == null ?
            new(0, 2, 0, 100, 0, 1, preview) :
            new(GaugeData->OathValue / 50,
                2,
                GaugeData->OathValue,
                100,
                GaugeData->Prerequisites[0] > 0 ? 1 : 0, 1,
                preview);
}

[TrackerDisplay(Gauge, JobGaugeColor, "Job Gauge Tracker", WAR)]
public sealed unsafe class BeastGaugeTracker : JobGaugeTracker<BeastGaugeData>
{
    public override string DisplayName => "Beast Gauge";
    public override string GaugeAddonName => "JobHudWAR0";

    public override TrackerData GetCurrentData(float? preview = null) =>
        GaugeAddon == null ?
            new(0, 2, 0, 100, 0, 1, preview) :
            new(GaugeData->BeastValue / 50,
                2,
                GaugeData->BeastValue,
                100,
                GaugeData->Prerequisites[2] > 0 ? 1 : 0, 1,
                preview);
}

[TrackerDisplay(Gauge, JobGaugeColor, "Job Gauge Tracker", DRK)]
public sealed unsafe class BloodGaugeTracker : JobGaugeTracker<BloodGaugeData>
{
    public override string DisplayName => "Blood Gauge";
    public override string GaugeAddonName => "JobHudDRK0";

    public override TrackerData GetCurrentData(float? preview = null) =>
        GaugeAddon == null ?
            new(0, 2, 0, 100, 0, 1, preview) :
            new(GaugeData->BloodValue / 50,
                2,
                GaugeData->BloodValue,
                GaugeData->BloodMax,
                GaugeData->TankStance > 0 ? 1 : 0, 1,
                preview);
}

[TrackerDisplay(Gauge, JobGaugeColor, "Job Gauge Tracker", DRK)]
public sealed unsafe class DarksideGaugeTracker : JobGaugeTracker<DarksideGaugeData>
{
    public override string DisplayName => "Darkside Gauge";
    public override string GaugeAddonName => "JobHudDRK1";
    public override string TermGauge => "Timer";

    public override TrackerData GetCurrentData(float? preview = null) =>
        GaugeAddon == null ?
            new(0, 2, 0, 100, 0, 1, preview) :
            new(GaugeData->DarksideTimeLeft > 0 ? 1 : 0,
                1,
                GaugeData->DarksideTimeLeft / 1000f,
                GaugeData->DarksideTimeMax / 1000f,
                GaugeData->Prerequisites[2] > 0 ? 1 : 0, 1,
                preview);
}

[TrackerDisplay(Gauge, JobGaugeColor, "Job Gauge Tracker", DRK)]
public sealed unsafe class LivingShadowTracker : JobGaugeTracker<DarksideGaugeData>
{
    public override string DisplayName => "Living Shadow";
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

[TrackerDisplay(Gauge, JobGaugeColor, "Job Gauge Tracker", GNB)]
public sealed unsafe class PowderGaugeTracker : JobGaugeTracker<PowderGaugeData>
{
    public override string DisplayName => "Powder Gauge";
    public override string GaugeAddonName => "JobHudGNB0";
    public override string TermCount => "Cartridges";

    public override TrackerData GetCurrentData(float? preview = null) =>
        GaugeAddon == null ?
            new(0, 3, 0, 100, 0, 1, preview) :
            new(GaugeData->Ammo,
                3,
                GaugeData->Ammo,
                3,
                GaugeData->Prerequisites[2] > 0 ? 1 : 0, 1,
                preview);
}

#endregion

#region Healer

[TrackerDisplay(Gauge, JobGaugeColor, "Job Gauge Tracker", WHM)]
public sealed unsafe class LilyTracker : JobGaugeTracker<HealingGaugeData>
{
    public override string DisplayName => "Lilies";
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

[TrackerDisplay(Gauge, JobGaugeColor, "Job Gauge Tracker", WHM)]
public sealed unsafe class BloodLilyTracker : JobGaugeTracker<HealingGaugeData>
{
    public override string DisplayName => "Blood Lily";
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

[TrackerDisplay(Gauge, JobGaugeColor, "Job Gauge Tracker", SCH)]
public sealed unsafe class AetherflowSCHGaugeTracker : JobGaugeTracker<AetherflowACNGaugeData>
{
    public override string DisplayName => "Aetherflow Gauge";
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

[TrackerDisplay(Gauge, JobGaugeColor, "Job Gauge Tracker", SCH)]
public sealed unsafe class FaerieGaugeTracker : JobGaugeTracker<FaerieGaugeData>
{
    public override string DisplayName => "Fae Aether";
    public override string GaugeAddonName => "JobHudSCH0";

    public override TrackerData GetCurrentData(float? preview = null) =>
        GaugeAddon == null || GaugeData == null ?
            new(0, 3, 0, 100, 0, 1, preview) :
            new(GaugeData->FaeValue > 0 ? 1 : 0,
                1,
                GaugeData->FaeValue,
                GaugeData->FaeMax,
                GaugeData->Prerequisites[2] > 0 ? 1 : 0, 1,
                preview);
}

[TrackerDisplay(Gauge, JobGaugeColor, "Job Gauge Tracker", SCH)]
public sealed unsafe class SeraphTracker : JobGaugeTracker<FaerieGaugeData>
{
    public override string DisplayName => "Seraph Timer";
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

[TrackerDisplay(Gauge, JobGaugeColor, "Job Gauge Tracker", SGE)]
public sealed unsafe class EukrasiaTracker : JobGaugeTracker<EukrasiaGaugeData>
{
    public override string DisplayName => "Eukrasia";
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

[TrackerDisplay(Gauge, JobGaugeColor, "Job Gauge Tracker", SGE)]
public sealed unsafe class AddersgallTracker : JobGaugeTracker<AddersgallGaugeData>
{
    public override string DisplayName => "Addersgall Gauge";
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
                GaugeData->Addersgall == 3 ? 1 : 0, 1,
                preview);
}

[TrackerDisplay(Gauge, JobGaugeColor, "Job Gauge Tracker", SGE)]
public sealed unsafe class AdderstingTracker : JobGaugeTracker<AddersgallGaugeData>
{
    public override string DisplayName => "Addersting Counter";
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
