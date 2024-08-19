global using static GaugeOMatic.GaugeOMatic.Service;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using GaugeOMatic.Config;
using GaugeOMatic.Trackers;
using GaugeOMatic.Windows;
using static GaugeOMatic.GameData.ActionRef;
using static GaugeOMatic.Widgets.Common.CommonParts;
using static GaugeOMatic.Widgets.WidgetAttribute;

namespace GaugeOMatic;

public sealed partial class GaugeOMatic : IDalamudPlugin
{
    // ReSharper disable once UnusedMember.Global
    public static string Name => "Gauge-O-Matic";
    private const string CommandName = "/gomatic";
    internal static string PluginDirPath = null!;

    internal static WindowSystem WindowSystem = new("Gauge-O-Matic");
    internal static ConfigWindow ConfigWindow { get; set; } = null!;
    internal static PresetWindow PresetWindow { get; set; } = null!;
    private Configuration Configuration { get; set; }
    private IDalamudPluginInterface PluginInterface { get; init; }
    private TrackerManager TrackerManager { get; init; }

    public GaugeOMatic(IDalamudPluginInterface pluginInterface) {
        PluginInterface = pluginInterface;
        PluginInterface.Create<Service>();
        PluginDirPath = PluginInterface.AssemblyLocation.DirectoryName!;

        Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
        Configuration.Initialize(PluginInterface);

        PluginInterface.UiBuilder.Draw += DrawWindows;
        PluginInterface.UiBuilder.OpenConfigUi += OpenConfigWindow;

        BuildWidgetList();
        PopulateActions();

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
