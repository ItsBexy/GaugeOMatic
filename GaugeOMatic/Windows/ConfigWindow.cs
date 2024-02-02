using Dalamud.Interface;
using Dalamud.Interface.Windowing;
using GaugeOMatic.Config;
using GaugeOMatic.Trackers;
using GaugeOMatic.Utility;
using ImGuiNET;
using System;
using System.Numerics;
using static Dalamud.Interface.Utility.ImGuiHelpers;
using static GaugeOMatic.GameData.JobData;
using static GaugeOMatic.GameData.JobData.Job;
using static GaugeOMatic.Utility.Color;
using static GaugeOMatic.Windows.ConfigWindow.GeneralTab;
using static ImGuiNET.ImGuiCol;
using static System.Enum;

namespace GaugeOMatic.Windows;

public partial class ConfigWindow : Window, IDisposable
{
    public TrackerManager TrackerManager;
    public Configuration Configuration;

    public enum GeneralTab { Jobs, Settings, Help }

    public ConfigWindow(TrackerManager trackerManager) : base("Gauge-O-Matic")
    {
        TrackerManager = trackerManager;
        Configuration = TrackerManager.Configuration;

        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new(1050f, 330f),
            MaximumSize = new(3200f)
        };
    }

    public void Dispose() { }

    public override void Draw()
    {
        //todo: fix this to not be localization reliant
        if (JobChanged && TryParse(JobAbbr, out Job newJob) && newJob <= (Job)21) TrackerManager.Configuration.JobTab = newJob;

        ConfigWindowPos = ImGui.GetWindowPos();
        ConfigWindowSize = ImGui.GetWindowSize();
        ImGui.Spacing();
        ImGui.Spacing();

        if (ImGui.BeginTable("LayoutTable", 3, ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.BordersInnerV))
        {
            ImGui.TableSetupColumn("VertTabBar", ImGuiTableColumnFlags.WidthFixed, 40f * GlobalScale);
            ImGui.TableSetupColumn("VertTabBar2", ImGuiTableColumnFlags.WidthFixed, 40f * GlobalScale);
            ImGui.TableSetupColumn("body", ImGuiTableColumnFlags.WidthFixed, 1200f * GlobalScale);

            VerticalTabBar();

            ImGui.TableNextColumn();
            ImGui.Indent(10);

            switch (Configuration.GeneralTab)
            {
                case Settings:
                    ImGui.Text("WELCOME TO THE SETTINGS TAB");
                    break;
                case Help:
                    DrawHelpTab();
                    break;
                default:
                {
                    var jobModule = TrackerManager.GetActiveModule();
                    if (jobModule != null) DrawJobModuleTab(jobModule);
                    break;
                }
            }

            ImGui.EndTable();
        }
    }

    private void VerticalTabBar()
    {
        void VerticalTabButton(Job job, Vector4 tabActive, Vector4 tabHovered, Vector4 tab)
        {
            var active = Configuration.GeneralTab == Jobs && Configuration.JobTab == job;

            ImGuiHelpy.PushStyleColorMulti(new(ButtonActive, tabActive), new(ButtonHovered, tabHovered), new(Button, active ? tabActive : tab));
            if (ImGui.Button($"{job}      "))
            {
                Configuration.JobTab = job;
                Configuration.GeneralTab = Jobs;
                Configuration.Save();
            }
            ImGui.PopStyleColor(3);
        }

        void GeneralButton(string label, FontAwesomeIcon icon, GeneralTab genTab, string toolTip)
        {
            var active = Configuration.GeneralTab == genTab;
            ImGuiHelpy.PushStyleColorMulti(new(ButtonActive, TabActive), new(ButtonHovered, TabHovered), new(Button, active ? TabActive : Tab));
            if (ImGuiHelpy.IconButton(label, icon, 32f))
            {
                Configuration.GeneralTab = genTab;
                Configuration.Save();
            }
            ImGui.PopStyleColor(3);

            if (ImGui.IsItemHovered()) ImGui.SetTooltip(toolTip);
        }

        void TankButton(Job job) => VerticalTabButton(job, (ColorRGB)0x026999ff, (ColorRGB)0x1090a7ff, (ColorRGB)0x052657ff);
        void HealButton(Job job) => VerticalTabButton(job, (ColorRGB)0x0c723aff, (ColorRGB)0x02992bff, (ColorRGB)0x0a2d23ff);
        void DPSButton(Job job) => VerticalTabButton(job, (ColorRGB)0xc20c15ff, (ColorRGB)0xe5482fff, (ColorRGB)0x4c0b1cff);

        ImGui.TableNextRow();
        ImGui.TableNextColumn();

       // GeneralButton("Settings", FontAwesomeIcon.Cog, Settings, "General Settings");
        GeneralButton("Help", FontAwesomeIcon.Question, Help, "Help");

        ImGui.Spacing();
        ImGui.Spacing();

        TankButton(PLD);
        TankButton(WAR);
        TankButton(DRK);
        TankButton(GNB);
        ImGui.Spacing();
        ImGui.Spacing();
        DPSButton(MNK);
        DPSButton(DRG);
        DPSButton(NIN);
        DPSButton(SAM);
        DPSButton(RPR);

        ImGui.TableNextColumn();

        HealButton(WHM);
        HealButton(SCH);
        HealButton(AST);
        HealButton(SGE);
        ImGui.Spacing();
        ImGui.Spacing();
        DPSButton(BRD);
        DPSButton(MCH);
        DPSButton(DNC);
        ImGui.Spacing();
        ImGui.Spacing();
        DPSButton(BLM);
        DPSButton(SMN);
        DPSButton(RDM);
    }

    internal static Vector4 TabActive = ImGuiHelpy.GetStyleColorUsableVec4(ImGuiCol.TabActive);
    internal static Vector4 Tab = ImGuiHelpy.GetStyleColorUsableVec4(ImGuiCol.Tab);
    internal static Vector4 TabHovered = ImGuiHelpy.GetStyleColorUsableVec4(ImGuiCol.TabHovered);

    internal static Vector2 ConfigWindowSize;
    internal static Vector2 ConfigWindowPos;
}
