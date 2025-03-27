using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.ClientState.Statuses;
using Dalamud.Plugin.Services;
using System.Collections.Generic;
using System.Linq;
using static Dalamud.Game.ClientState.Objects.Enums.ObjectKind;
using static GaugeOMatic.GameData.JobData;

namespace GaugeOMatic.GameData;

public static class FrameworkData
{
    internal struct PlayerData
    {
        internal Job Job = 0;
        internal byte Lvl = 1;
        internal uint CurHp;
        internal uint MaxHp;
        internal uint CurMp;
        internal uint MaxMp;
        internal bool IsCasting;
        internal float CurCastTime;
        internal float TotalCastTime;
        internal ulong? ObjId;
        internal uint? CastActionId;
        internal ulong? TargetObjId;
        internal List<Status>? PlayerStatus;
        internal List<Status>? EnemyStatus;

        public PlayerData(IBattleChara? localPlayer)
        {
            Job = (Job)(localPlayer?.ClassJob.RowId ?? 0);
            Lvl = localPlayer?.Level ?? 1;
            CurHp = localPlayer?.CurrentHp ?? 0;
            MaxHp = localPlayer?.MaxHp ?? 1;
            CurMp = localPlayer?.CurrentMp ?? 0;
            MaxMp = localPlayer?.MaxMp ?? 1;
            IsCasting = localPlayer?.IsCasting ?? false;
            CurCastTime = localPlayer?.CurrentCastTime ?? 0;
            TotalCastTime = localPlayer?.TotalCastTime ?? 1;
            ObjId = localPlayer?.GameObjectId ?? null;
            CastActionId = localPlayer?.CastActionId ?? null;
            TargetObjId = localPlayer?.TargetObject?.GameObjectId ?? null;

            PlayerStatus = localPlayer?.StatusList.ToList();

            var target = localPlayer?.TargetObject;
            if (target?.ObjectKind == BattleNpc)
            {
                var enemyTarget = (IBattleNpc)target;
                EnemyStatus = enemyTarget.StatusList.ToList();
            }
            else
            {
                EnemyStatus = null;
            }

        }
    }

    internal static PlayerData LocalPlayer;

    public static void UpdatePlayerData(IFramework framework) => LocalPlayer = new PlayerData(ClientState.LocalPlayer);
}
