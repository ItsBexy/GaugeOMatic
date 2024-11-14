using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.ClientState.Statuses;
using Dalamud.Plugin.Services;
using static Dalamud.Game.ClientState.Objects.Enums.ObjectKind;

namespace GaugeOMatic.GameData;

public static class FrameworkData
{
    internal static IPlayerCharacter? LocalPlayer;
    internal static IGameObject? Target;
    internal static IBattleNpc? EnemyTarget;

    internal static List<Status>? PlayerStatus;
    internal static List<Status>? EnemyStatus;

    public static void UpdatePlayerData(IFramework framework)
    {
        LocalPlayer = ClientState.LocalPlayer;
        PlayerStatus = LocalPlayer?.StatusList.ToList();
        Target = LocalPlayer?.TargetObject;

        if (Target?.ObjectKind == BattleNpc)
        {
            EnemyTarget = (IBattleNpc)Target;
            EnemyStatus = EnemyTarget.StatusList.ToList();
        }
        else
        {
            EnemyTarget = null;
            EnemyStatus = null;
        }
    }
}
