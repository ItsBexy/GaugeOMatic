using FFXIVClientStructs.FFXIV.Client.Game;
using static GaugeOMatic.GameData.JobData;
using static GaugeOMatic.GameData.JobData.Role;

namespace GaugeOMatic.GameData;

public unsafe partial class ActionData
{
    public class ActionRef : ItemRef
    {
        public int MaxCharges;
        public float CooldownLength;

        public ActionRef(uint id, Job job, string name, float cooldownLength, int maxCharges = 1, Role role = None)
        {
            Job = job;
            Role = role;
            ID = id;
            Name = name;
            MaxCharges = maxCharges;
            CooldownLength = cooldownLength;
        }

        public ActionRef() { }

        public static implicit operator ActionRef(uint i) => Actions.TryGetValue(i, out var result) ? result : new();
    }

    internal static ActionManager* ActionManager => FFXIVClientStructs.FFXIV.Client.Game.ActionManager.Instance();

    public static int GetCharges(uint id) => (int)ActionManager->GetCurrentCharges(id);

    public static float GetCooldownTime(uint id, float maxValue)
    {
        var elapsed = ActionManager->GetRecastTimeElapsed(ActionType.Action, id);
        return elapsed == 0 ? 0 : maxValue - elapsed;
    }
}
