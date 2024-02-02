global using static GaugeOMatic.GaugeOMatic.Service;
using Dalamud.Interface.Windowing;
using Dalamud.IoC;
using Dalamud.Plugin;
using GaugeOMatic.Config;
using GaugeOMatic.GameData;
using GaugeOMatic.Trackers;
using GaugeOMatic.Widgets;
using GaugeOMatic.Windows;
using static GaugeOMatic.Widgets.Common.CommonParts;

namespace GaugeOMatic;

public sealed partial class GaugeOMatic : IDalamudPlugin
{
    // ReSharper disable once UnusedMember.Global
    public static string Name => "Gauge-O-Matic";
    private const string CommandName = "/gomatic";

    internal static WindowSystem WindowSystem = new("Gauge-O-Matic");
    internal static ConfigWindow ConfigWindow { get; set; } = null!;
    internal static PresetWindow PresetWindow { get; set; } = null!;
    private Configuration Configuration { get; set; }
    private DalamudPluginInterface PluginInterface { get; init; }
    private TrackerManager TrackerManager { get; init; }

    public GaugeOMatic([RequiredVersion("1.0")] DalamudPluginInterface pluginInterface) {
        PluginInterface = pluginInterface;
        PluginInterface.Create<Service>();

        Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
        Configuration.Initialize(PluginInterface);

        PluginInterface.UiBuilder.Draw += DrawWindows;
        PluginInterface.UiBuilder.OpenConfigUi += OpenConfigWindow;

        WidgetInfo.BuildWidgetList();
        ActionData.SetupHooks();

        TrackerManager = new(Configuration);

        ConfigWindow = new(TrackerManager);
        PresetWindow = new(TrackerManager);

        WindowSystem.AddWindow(ConfigWindow);
        WindowSystem.AddWindow(PresetWindow);

        CommandManager.AddHandler(CommandName, new(OnCommand) { HelpMessage = "Open Gauge-O-Matic Settings" });
    }

    public void Dispose()
    {
        PluginInterface.UiBuilder.Draw -= DrawWindows;
        PluginInterface.UiBuilder.OpenConfigUi -= OpenConfigWindow;

        TrackerManager.Dispose();

        WindowSystem.RemoveAllWindows();
        ConfigWindow.Dispose();

        CommandManager.RemoveHandler(CommandName);

        DisposeSharedParts();
    }

    public static void OnCommand(string command, string args) => ConfigWindow.IsOpen = !ConfigWindow.IsOpen;

    public static void DrawWindows() => WindowSystem.Draw();
    public static void OpenConfigWindow() => ConfigWindow.IsOpen = true;
}
