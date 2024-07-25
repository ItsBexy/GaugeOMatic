using FFXIVClientStructs.FFXIV.Client.Game;
using Lumina.Excel;
using System;
using static GaugeOMatic.GameData.ActionData.ActionRef.ReadyTypes;
using static GaugeOMatic.GameData.JobData;
using static GaugeOMatic.GameData.StatusData;
using Action = Lumina.Excel.GeneratedSheets.Action;

namespace GaugeOMatic.GameData;

public unsafe partial class ActionData
{
    internal static ActionManager* ActionManager => FFXIVClientStructs.FFXIV.Client.Game.ActionManager.Instance();

    public static ExcelSheet<Action>? ActionSheet { get; } = DataManager.Excel.GetSheet<Action>("Action");

    public static bool CheckForAnts(params uint[] ids)
    {
        foreach (var id in ids)
            if (ActionManager->IsActionHighlighted(ActionType.Action, id))
                return true;

        return false;
    }

    public class ActionRef : ItemRef
    {
        [Flags]
        public enum ReadyTypes
        {
            Ants = 0x01,
            StatusEffect = 0x02
        }

        public int MaxCharges;
        public float CooldownLength;
        public ReadyTypes ReadyType;
        public StatusRef? ReadyStatus;
        public bool HasUpgrades;
        public uint GetID => HasUpgrades ? ActionManager->GetAdjustedActionId(ID): ID;

        public ActionRef(uint id, Job job, string name, float cooldownLength, int maxCharges = 1 )
        {
            Job = job;
            ID = id;
            Name = name;
            MaxCharges = maxCharges;
            CooldownLength = cooldownLength;
        }

        private ActionRef() { }

        public static implicit operator ActionRef(uint i) => Actions.TryGetValue(i, out var result) ? result : new();

        public float GetCooldownTotal()
        {
            var cooldownLength = ActionManager->GetRecastTime(ActionType.Action, GetID);
            if (cooldownLength > 0) CooldownLength = cooldownLength;
            return CooldownLength;
        }

        public float GetCooldownElapsed() => ActionManager->GetRecastTimeElapsed(ActionType.Action, GetID);
        public float GetCooldownRemaining(float? cdOverride = null)
        {
            var elapsed = GetCooldownElapsed();
            return elapsed == 0 ? 0 : (cdOverride ?? CooldownLength) - elapsed;
        }

        public bool IsReady()
        {
            return GetCurrentCharges() != 0 &&
                   !(ReadyType.HasFlag(Ants) && !ActionManager->IsActionHighlighted(ActionType.Action, GetID)) &&
                   !(ReadyType.HasFlag(StatusEffect) && !(ReadyStatus?.TryGetStatus() ?? false));
        }

        public int GetMaxCharges()
        {
            var maxAtLevel = GetMaxChargesAtLevel?.Invoke(GetID, 0);
            if (maxAtLevel > 0) MaxCharges = (int)maxAtLevel;
            return MaxCharges;
        }

        public int GetCurrentCharges() => (int)ActionManager->GetCurrentCharges(GetID);
    }
}
