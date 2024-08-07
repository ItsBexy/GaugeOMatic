using FFXIVClientStructs.FFXIV.Client.Game;
using System;
using static FFXIVClientStructs.FFXIV.Client.Game.ActionManager;
using static GaugeOMatic.GameData.ActionFlags;
using static GaugeOMatic.GameData.ActionRef.BarType;
using static GaugeOMatic.Trackers.Tracker;

namespace GaugeOMatic.GameData;

public partial class ActionRef
{
    public static unsafe ActionManager* ActionManager => Instance();

    public int GetMaxCharges() => FFXIVClientStructs.FFXIV.Client.Game.ActionManager.GetMaxCharges(GetAdjustedId(), 0);

    public unsafe int GetCurrentCharges() => (int)ActionManager->GetCurrentCharges(GetAdjustedId());

    public unsafe float GetCooldownTotal()
    {
        var cooldownLength = ActionManager->GetRecastTime(ActionType.Action, GetAdjustedId());
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

    public unsafe TrackerData GetTrackerData(float? preview)
    {
        var barType = GetBarType();

        var cooldownTotal = GetCooldownTotal();
        var elapsed = GetCooldownElapsed();
        var cooldownRemaining = elapsed == 0 ? 0 : cooldownTotal - elapsed;

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
                                 StatusTimer => Math.Abs(ReadyStatus!.TryGetStatus(out var status) ? status?.RemainingTime ?? 0 : 0),
                                 ComboTimer => HasAnts() ? ActionManager->Combo.Timer : 0,
                                 _ => cooldownRemaining
                             } : (float)(preview * maxGauge);

        if (preview != null)
        {
            state = (int)Math.Round(preview.Value);
            count = (int)(preview * maxCount);
        }
        else
        {
            var transformCheck = !HasFlag(TransformedButton) || GetBaseAction().GetAdjustedId() == ID;
            var chargeCheck = !HasFlag(HasCharges) || GetCurrentCharges() != 0;
            var cooldownCheck = !HasFlag(LongCooldown, exclude: HasCharges) || !(cooldownRemaining > 0);
            var statusCheck = !HasFlag(RequiresStatus) || (ReadyStatus?.TryGetStatus() ?? false);
            var antCheck = !HasFlag(CanGetAnts | ComboBonus) || HasAnts();

            state = transformCheck && cooldownCheck && chargeCheck && statusCheck && antCheck ? 1 : 0;

            count = maxCount > 1 ? GetCurrentCharges() : state;
        }

        return new(count, maxCount, gaugeValue, maxGauge, state, 1, preview);
    }
}

