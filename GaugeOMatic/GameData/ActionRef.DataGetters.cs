using FFXIVClientStructs.FFXIV.Client.Game;
using System;
using static FFXIVClientStructs.FFXIV.Client.Game.ActionManager;
using static GaugeOMatic.GameData.ActionFlags;
using static GaugeOMatic.GameData.ActionRef.BarType;
using static GaugeOMatic.GameData.Overrides;
using static GaugeOMatic.GameData.StatusRef.StatusActor;
using static GaugeOMatic.Trackers.Tracker;

namespace GaugeOMatic.GameData;

public partial class ActionRef
{
    public static unsafe ActionManager* ActionManager => Instance();

    public int GetMaxCharges() => FFXIVClientStructs.FFXIV.Client.Game.ActionManager.GetMaxCharges(GetAdjustedId(), 0);
    public int GetActionCost() => FFXIVClientStructs.FFXIV.Client.Game.ActionManager.GetActionCost(ActionType.Action, ID, default, default, default, default);

    // ReSharper disable once UnusedMember.Global
    public int GetCurrentCharges()
    {
        var maxCharges = GetMaxCharges();
        var elapsed = GetCooldownElapsed();
        if (elapsed == 0) return maxCharges;

        var cooldownTotal = GetCooldownTotal();

        return (int)Math.Floor(elapsed / cooldownTotal * maxCharges);
    }

    public unsafe float GetCooldownTotal()
    {
        var cooldownLength = CooldownOverrides.TryGetValue(ID, out var cd) ?
                                 cd.Invoke() :
                                 ActionManager->GetRecastTime(ActionType.Action, GetAdjustedId());
        if (cooldownLength > 0) LastKnownCooldown = cooldownLength;
        return LastKnownCooldown;
    }

    public unsafe float GetCooldownElapsed() => ActionManager->GetRecastTimeElapsed(ActionType.Action, GetAdjustedId());

    // ReSharper disable once UnusedMember.Global
    public float GetCooldownRemaining()
    {
        var elapsed = GetCooldownElapsed();
        return elapsed == 0 ? 0 : GetCooldownTotal() - elapsed;
    }

    public unsafe bool HasAnts(bool adjusted = false) => ActionManager->IsActionHighlighted(ActionType.Action, adjusted ? GetAdjustedId() : ID);

    public BarType GetBarType() => HasFlag(RequiresStatus, exclude: LongCooldown) && ReadyStatus != null ? StatusTimer :
                                   HasFlag(ComboBonus, exclude: LongCooldown) ? ComboTimer :
                                   Cooldown;

    public enum BarType
    {
        Cooldown = 0,
        StatusTimer = 1,
        ComboTimer = 2
    }

    public override unsafe TrackerData GetTrackerData(float? preview)
    {
        var barType = GetBarType();

        var cooldownTotal = GetCooldownTotal();
        var elapsed = GetCooldownElapsed();
        var cooldownRemaining = elapsed == 0 || elapsed >= cooldownTotal ? 0 : cooldownTotal - elapsed;

        int count;
        var maxCount = GetMaxCharges();

        int state;

        var maxGauge = barType switch
        {
            StatusTimer => ReadyStatus!.MaxTime,
            ComboTimer => 30,
            _ => cooldownTotal
        };

        var gaugeValue = preview == null ? barType switch
        {
            StatusTimer => Math.Abs(ReadyStatus!.TryGetStatus(out var status, Self) ? status?.RemainingTime ?? 0 : 0),
            ComboTimer => HasAnts(true) ? ActionManager->Combo.Timer : 0,
            _ => cooldownRemaining
        } : (preview.Value * maxGauge)!;

        if (preview != null)
        {
            state = (int)Math.Round(preview.Value);
            count = (int)(preview.Value * maxCount)!;
        }
        else
        {
            var charges = elapsed == 0 ? maxCount : (int)Math.Floor(elapsed / cooldownTotal * maxCount);

            var transformCheck = !HasFlag(TransformedButton) || GetBaseAction().GetAdjustedId() == ID;
            var chargeCheck = !HasFlag(HasCharges) || charges != 0;
            var cooldownCheck = !HasFlag(LongCooldown, exclude: HasCharges) || !(cooldownRemaining > 0);
            var statusCheck = !HasFlag(RequiresStatus) || (ReadyStatus?.TryGetStatus(Self) ?? false);
            var antCheck = !HasFlag(CanGetAnts | ComboBonus) || HasAnts();
            var mpCheck = !HasFlag(CostsMP) || GetActionCost() < FrameworkData.LocalPlayer.CurMp;

            state = transformCheck && cooldownCheck && chargeCheck && statusCheck && antCheck && mpCheck ? 1 : 0;
            count = HasFlag(HasCharges) ? charges : state;
        }

        return new(count, maxCount, gaugeValue, maxGauge, state, 1, preview);
    }
}

