using Dalamud.Interface.Windowing;
using GaugeOMatic.Config;
using GaugeOMatic.JobModules;
using GaugeOMatic.Trackers;
using GaugeOMatic.Utility;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Interface.Utility.Raii;
using GaugeOMatic.Utility.DalamudComponents;
using static Dalamud.Interface.Utility.ImGuiHelpers;
using static GaugeOMatic.GameData.JobData;
using static GaugeOMatic.GameData.JobData.Job;
using static GaugeOMatic.Utility.Color;
using static GaugeOMatic.Windows.ConfigWindow.GeneralTab;
using static ImGuiNET.ImGuiCol;
using static ImGuiNET.ImGuiTableColumnFlags;
using static ImGuiNET.ImGuiTableFlags;

namespace GaugeOMatic.Windows;

public partial class ConfigWindow : Window, IDisposable
{
    public TrackerManager TrackerManager;
    public Configuration Configuration;
    public List<JobModule> JobModules;

    // ReSharper disable once UnusedMember.Global
    public enum GeneralTab { Jobs, Settings, Help }

    public ConfigWindow(TrackerManager trackerManager) : base("Gauge-O-Matic")
    {
        TrackerManager = trackerManager;
        Configuration = TrackerManager.Configuration;
        JobModules = TrackerManager.JobModules;

        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new(1050f, 330f),
            MaximumSize = new(3200f)
        };
    }

    public void Dispose() { }

    public override void Draw()
    {
        if (JobChanged && JobModules.Any(static j => j.Job == Current)) Configuration.JobTab = Current;

        ConfigWindowPos = ImGui.GetWindowPos();
        ConfigWindowSize = ImGui.GetWindowSize();
        ImGui.Spacing();
        ImGui.Spacing();

        using var table = ImRaii.Table("LayoutTable", 3, SizingFixedFit);
        if (table.Success)
        {
            ImGui.TableSetupColumn("VertTabBar");
            ImGui.TableSetupColumn("VertTabBar2");
            ImGui.TableSetupColumn("body", WidthFixed, 1200f * GlobalScale);

            VerticalTabBar();

            ImGui.TableNextColumn();
            ImGui.Indent(10);

            switch (Configuration.GeneralTab)
            {
                case Help:
                    DrawHelpTab();
                    break;
                default:
                {
                    var jobModule = GetModuleForTab(Configuration.JobTab, JobModules);
                    if (jobModule != null) DrawJobModuleTab(jobModule);
                    break;
                }
            }
        }
    }

    private void VerticalTabBar()
    {
        ImGui.TableNextRow();
        ImGui.TableNextColumn();

        // GeneralButton("Settings", Cog, Settings, "General Settings");
        GeneralButton(66313, Help, "Help");

        var jobTab = Configuration.JobTab;
        var input = false;

        ImGui.Spacing();
        ImGui.Spacing();
        input |= ImGuiComponents.GameIconButtonSelect("tankJobs",ref jobTab,JobIconZip(PLD,WAR,DRK,GNB),new(26),new(30),1, (ColorRGB)0x052657ff, (ColorRGB)0x1090a7ff, (ColorRGB)0x026999ff);
        ImGui.Spacing();
        ImGui.Spacing();
        input |= ImGuiComponents.GameIconButtonSelect("meleeJobs",ref jobTab,JobIconZip(MNK,DRG,NIN,SAM,RPR,VPR),new(26),new(30),1, (ColorRGB)0x4c0b1cff, (ColorRGB)0xe5482fff,(ColorRGB)0xc20c15ff);

        ImGui.TableNextColumn();


        input |= ImGuiComponents.GameIconButtonSelect("healerJobs",ref jobTab,JobIconZip(WHM,SCH,AST,SGE),new(26),new(30),1, (ColorRGB)0x0a2d23ff, (ColorRGB)0x02992bff,(ColorRGB)0x0c723aff);
        ImGui.Spacing();
        ImGui.Spacing();
        input |= ImGuiComponents.GameIconButtonSelect("rangedJobs",ref jobTab,JobIconZip(BRD,MCH,DNC),new(26),new(30),1, (ColorRGB)0x4c0b1cff, (ColorRGB)0xe5482fff,(ColorRGB)0xc20c15ff);
        ImGui.Spacing();
        ImGui.Spacing();
        input |= ImGuiComponents.GameIconButtonSelect("casterJobs",ref jobTab,JobIconZip(BLM,SMN,RDM,PCT),new(26),new(30),1, (ColorRGB)0x4c0b1cff, (ColorRGB)0xe5482fff,(ColorRGB)0xc20c15ff);

        if (input)
        {
            Configuration.GeneralTab = Jobs;
            Configuration.JobTab = jobTab;
        }

        return;

        static IEnumerable<KeyValuePair<uint, Job>> JobIconZip(params Job[] jobs) => jobs.Select(static j => new KeyValuePair<uint, Job>(GetJobIcon(j),j));

        void GeneralButton(uint icon, GeneralTab genTab, string tooltip)
        {
            var active = Configuration.GeneralTab == genTab;
            TextureProvider.GetFromGameIcon(new(icon)).TryGetWrap(out var tex, out _);

            using (ImRaii.PushColor(ButtonActive, TabActive)
                         .Push(ButtonHovered, TabHovered)
                         .Push(Button, active ? TabActive : Tab))
            {
                if (tex != null && ImGuiComponents.GameIconButton(66313,"Help",new(22),new Vector2(30)))
                {
                    Configuration.GeneralTab = genTab;
                    Configuration.Save();
                }
            }

            if (ImGui.IsItemHovered()) ImGui.SetTooltip(tooltip);
        }
    }


    internal static Vector4 TabActive = ImGuiHelpy.GetStyleColorVec4(ImGuiCol.TabActive);
    internal static Vector4 Tab = ImGuiHelpy.GetStyleColorVec4(ImGuiCol.Tab);
    internal static Vector4 TabHovered = ImGuiHelpy.GetStyleColorVec4(ImGuiCol.TabHovered);

    internal static Vector2 ConfigWindowSize;
    internal static Vector2 ConfigWindowPos;

}
