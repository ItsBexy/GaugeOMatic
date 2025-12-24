using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Plugin.Services;
using static GaugeOMatic.GameData.JobData;

namespace GaugeOMatic.GameData;

public static class FrameworkData
{
    internal struct PlayerData
    {
        internal readonly Job Job = 0;
        internal readonly byte Lvl = 1;
        internal readonly uint CurHp;
        internal readonly uint MaxHp;
        internal readonly uint CurMp;
        internal readonly uint MaxMp;
        internal readonly bool IsCasting;
        internal readonly float CurCastTime;
        internal readonly float TotalCastTime;
        internal ulong? ObjId;
        internal uint? CastActionId;
        internal ulong? TargetObjId;

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
        }
    }

    internal static PlayerData LocalPlayer;

    public static bool PlayerCheck => ClientState.IsLoggedIn && ObjectTable.LocalPlayer != null;

    public static void UpdatePlayerData(IFramework framework)
    {
        LocalPlayer = new PlayerData(PlayerCheck ? ObjectTable.LocalPlayer : null);
    }
}
