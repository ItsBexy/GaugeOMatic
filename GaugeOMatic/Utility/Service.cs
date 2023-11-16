using Dalamud.Game;
using Dalamud.IoC;
using Dalamud.Plugin.Services;

// ReSharper disable UnusedAutoPropertyAccessor.Local
// ReSharper disable UnusedMember.Global
#pragma warning disable CS8618

namespace GaugeOMatic;

public sealed partial class GaugeOMatic
{
    internal class Service
    {
        [PluginService] internal static IPluginLog Log { get; private set; }
        [PluginService] internal static IGameGui GameGui { get; private set; }
        [PluginService] internal static IAddonLifecycle AddonLifecycle { get; private set; }
        [PluginService] internal static IClientState ClientState { get; private set; }

        [PluginService] internal static IGameConfig GameConfig { get; private set; }
        [PluginService] internal static IGameInteropProvider GameInteropProvider { get; private set; }
        [PluginService] internal static ICommandManager CommandManager { get; private set; }
        [PluginService] internal static ICondition Condition { get; private set; }
        [PluginService] internal static ISigScanner SigScanner { get; private set; }
        [PluginService] internal static IFramework Framework { get; private set; }
        [PluginService] internal static IDataManager DataManager { get; private set; }
    }
}
