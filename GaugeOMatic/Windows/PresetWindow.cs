using Dalamud.Interface.Components;
using Dalamud.Interface.Windowing;
using GaugeOMatic.Config;
using GaugeOMatic.JobModules;
using GaugeOMatic.Trackers;
using GaugeOMatic.Trackers.Presets;
using GaugeOMatic.Utility;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using static Dalamud.Interface.FontAwesomeIcon;
using static Dalamud.Interface.Utility.ImGuiHelpers;
using static GaugeOMatic.Widgets.WidgetInfo;

namespace GaugeOMatic.Windows;

public class PresetWindow : Window, IDisposable
{
    public TrackerManager TrackerManager;
    public Configuration Configuration;

    public PresetWindow(TrackerManager trackerManager) : base("Gauge-O-Matic Presets")
    {
        TrackerManager = trackerManager;
        Configuration = TrackerManager.Configuration;
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new(250f, 350f),
            MaximumSize = new(800f, 1100f)
        };

        UIData.PresetList = BuildPresetList();
    }

    internal static PresetUIData UIData = new();

    public List<Preset> BuildPresetList() => new(PluginPresets.Presets.Concat(Configuration.SavedPresets).OrderBy(static p => p.Name));

    public override void Draw()
    {
        var module = TrackerManager.GetActiveModule();
        if (module != null) DrawPresetWindow(module);
    }

    public void Dispose() { }

    public void DrawPresetWindow(JobModule module)
    {
        PresetListUI(module);

        ImGui.Spacing();
        ImGui.Spacing();

        PresetAddUI(module);

        ImGui.EndTabItem();
    }

    public void SetUpRenamePopup(ref Preset preset)
    {
        if (!ImGui.BeginPopup("Rename")) return;

        var name = UIData.NewPresetName ?? preset.Name;
        ImGui.SetNextItemWidth(180f * GlobalScale);
        if (ImGui.InputText("##Name", ref name, 40)) UIData.NewPresetName = string.IsNullOrWhiteSpace(name) ? preset.Name : name;

        ImGui.SameLine();
        if (ImGui.Button("Rename##Save"))
        {
            Configuration.SavedPresets.Remove(preset);
            preset.Name = UIData.NewPresetName ?? preset.Name;
            Configuration.SavedPresets.Add(preset);
            UIData.PresetList = BuildPresetList();
            Configuration.Save();
            UIData.NewPresetName = null;
            ImGui.CloseCurrentPopup();
        }

        ImGui.EndPopup();
    }

    public void PresetListUI(JobModule module)
    {
        ImGui.BeginGroup();

        var filter = Configuration.PresetFiltering;

        ImGui.SameLine();
        if (ImGui.RadioButton("All", ref filter, 0)) { Configuration.PresetFiltering = 0; Configuration.Save(); }
        ImGui.SameLine();
        if (ImGui.RadioButton("Role Only", ref filter, 1)) { Configuration.PresetFiltering = 1; Configuration.Save(); }
        ImGui.SameLine();
        if (ImGui.RadioButton("Job Only", ref filter, 2)) { Configuration.PresetFiltering = 2; Configuration.Save(); }


        var presetList = new List<Preset>(UIData.PresetList.Where(p => filter == 0||p.Trackers.Any(t => filter switch
        {
            1 => t.JobRoleMatch(module),
            2 => t.JobMatch(module),
            _ => true
        })));
        var presetNames = presetList.Select(static p => p.Name).ToArray();
        var selectedIndex = Math.Clamp(UIData.PresetSelectedIndex, 0, Math.Max(0, presetList.Count - 1));
        ImGui.SetNextItemWidth(200f * GlobalScale);
        if (ImGui.ListBox("##Presets", ref selectedIndex, presetNames, presetList.Count, 10)) UIData.PresetSelectedIndex = selectedIndex;

        if (presetList.Count > 0)
        {
            var selectedPreset = presetList[selectedIndex];

            ImGui.SameLine();
            ImGui.BeginGroup();

            var builtIn = selectedPreset.BuiltIn;

            if (builtIn) ImGuiHelpy.IconButtonDisabled(Edit);
            else
            {
                SetUpRenamePopup(ref selectedPreset);
                if (ImGuiComponents.IconButton(Edit)) ImGui.OpenPopup("Rename");
            }

            ImGui.SameLine();

            if (builtIn) ImGuiHelpy.IconButtonDisabled(TrashAlt);
            else if (ImGuiComponents.IconButton(TrashAlt))
            {
                Configuration.SavedPresets.Remove(selectedPreset);
                UIData.PresetList = BuildPresetList();
                Configuration.Save();
            }

            ImGui.SameLine();
            if (ImGuiComponents.IconButtonWithText(SignOutAlt, "Export to clipboard"))
            {
                ImGui.SetClipboardText(selectedPreset);
            }

            ImGui.Spacing();
            DisplayPresetContents(module, selectedPreset);
            ImGui.Spacing();
            if (ImGuiComponents.IconButtonWithText(Plus, $"Add all to {module.Abbr}")) ApplyPreset(module, selectedPreset.Clone());
            ImGui.SameLine();
            if (ImGuiComponents.IconButtonWithText(PaintRoller, "Overwrite Current")) ApplyPreset(module, selectedPreset.Clone(), true);

            ImGui.EndGroup();
        }

        ImGui.EndGroup();
    }

    private static void DisplayPresetContents(JobModule module, Preset selectedPreset)
    {
        if (ImGui.BeginTable("PresetInfo", 2, ImGuiTableFlags.SizingFixedFit))
        {
            ImGui.TableNextRow();
            ImGui.TableNextColumn();
            ImGui.TextColored(new(1, 1, 1, 0.6f), "TRACKER");

            ImGui.TableNextColumn();
            ImGui.TextColored(new(1, 1, 1, 0.6f), "WIDGET");

            foreach (var trackerConfig in selectedPreset.Trackers)
            {
                var attr = trackerConfig.DisplayAttributes();

                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                AddTrackerButton(trackerConfig);
                ImGui.SameLine();

                ImGuiHelpy.WriteIcon(attr.Icon, attr.TypeDesc, attr.Color);

                ImGui.TextColored(trackerConfig.JobRoleMatch(module) ? new(1) : new(1, 1, 1, 0.3f), trackerConfig.GetDisplayName ?? "");
                ImGui.TableNextColumn();
                CopyWidgetButton(trackerConfig);
                ImGui.SameLine();
                // ReSharper disable once ConditionalAccessQualifierIsNonNullableAccordingToAPIContract
                ImGui.Text($"{WidgetList[trackerConfig.WidgetType!]?.DisplayName ?? ""}");
            }

            ImGui.EndTable();
        }

        void AddTrackerButton(TrackerConfig trackerConfig)
        {
            if (ImGuiComponents.IconButton($"##add{trackerConfig.GetHashCode()}", Plus))
            {
                var newConfig = trackerConfig.Clone();
                if (newConfig != null) module.AddTrackerConfig(newConfig);
                else module.AddBlankTracker();
            }

            if (ImGui.IsItemHovered()) ImGui.SetTooltip("Add " + trackerConfig.GetDisplayName);
        }

        static void CopyWidgetButton(TrackerConfig trackerConfig)
        {
            if (ImGuiComponents.IconButton($"CopyWidget{trackerConfig.GetHashCode()}", Copy))
            {
                ConfigWindow.WidgetClipType = trackerConfig.WidgetType;
                ConfigWindow.WidgetClipboard = trackerConfig.WidgetConfig;
                ImGui.SetClipboardText(ConfigWindow.WidgetClipboard);
            }

            if (ImGui.IsItemHovered()) ImGui.SetTooltip("Copy Widget Settings");
        }
    }

    public static void ApplyPreset(JobModule module, Preset preset, bool replace = false)
    {
        var list = preset.Trackers;
        if (replace) module.TrackerConfigList = list;
        else
        {
            var currentLen = module.TrackerConfigList.Length;
            var newList = new TrackerConfig[currentLen + list.Length];

            module.TrackerConfigList.CopyTo(newList, 0);
            list.CopyTo(newList, currentLen);

            for (var i = 0; i < newList.Length; i++) newList[i].Index = i;

            module.TrackerConfigList = newList;
        }

        module.RebuildTrackerList();
        module.Save();
    }

    private void PresetAddUI(JobModule module)
    {
        ImGui.BeginGroup();

        ImGui.TextColored(new(1, 1, 1, 0.6f), "ADD PRESETS");
        var saveName = UIData.SaveName;
        ImGui.Text($"Save a preset from your current {module.Abbr} trackers:");
        ImGui.SetNextItemWidth(200f * GlobalScale);
        if (ImGui.InputTextWithHint("##SaveName", "New Preset Name", ref saveName, 30u)) UIData.SaveName = saveName;
        ImGui.SameLine();
        if (ImGuiComponents.IconButtonWithText(Save, "Save")) SaveNewPreset(module, saveName);

        ImGui.Spacing();
        ImGui.Text("Import a preset from the clipboard:");
        if (ImGuiComponents.IconButtonWithText(SignInAlt, "Import From Clipboard")) ImportNewPreset(ImGui.GetClipboardText());

        ImGui.EndGroup();
    }

    private void SaveNewPreset(JobModule module, string saveName)
    {
        var newPreset = new Preset(module.TrackerConfigList, saveName.Length == 0 ? "New Preset" : saveName);
        var hash = newPreset.ExportStr().GetHashCode();

        var dup = false;
        foreach (var preset in UIData.PresetList)
        {
            if (preset.ExportStr().GetHashCode() != hash) continue;

            dup = true;
            break;
        }

        if (!dup)
        {
            Configuration.SavedPresets.Add(newPreset);
            UIData.PresetList = BuildPresetList();
            Configuration.Save();
        }
    }

    private void ImportNewPreset(string importString)
    {
        try
        {
            var hash = importString.GetHashCode();
            var dup = false;
            foreach (var preset in UIData.PresetList)
            {
                if (preset.ExportStr().GetHashCode() != hash) continue;

                UIData.ImportHintText = "Already have it!";
                dup = true;
                break;
            }

            if (!dup)
            {
                var importResult = new Preset(importString, true);
                if (importResult.Trackers.Length > 0)
                {
                    if (importResult.Name.Length == 0) importResult.Name = "New Preset";
                    Configuration.SavedPresets.Add(importResult);
                    UIData.PresetList = BuildPresetList();
                    Configuration.Save();
                    UIData.ImportHintText = "Imported!";
                }
            }
        }
        catch (Exception ex)
        {
            UIData.ImportHintText = "Whoops, invalid input!";
            Log.Error(ex.Message);
        }

        UIData.ImportString = "";
    }

    public struct PresetUIData
    {
        public string? NewPresetName = null;
        public List<Preset> PresetList = new();
        public int PresetSelectedIndex = 0;
        public string ImportHintText { get; set; } = "Paste Here";
        public string ImportString { get; set; } = "";
        public string SaveName = "";

        public PresetUIData() { }
    }
}
