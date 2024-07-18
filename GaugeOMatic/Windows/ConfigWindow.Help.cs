using Dalamud.Interface;
using ImGuiNET;
using static GaugeOMatic.Utility.ImGuiHelpy;

namespace GaugeOMatic.Windows;

public partial class ConfigWindow
{
    private static void DrawHelpTab()
    {
        ImGui.TextDisabled("GAUGE-O-MATIC HELP");

        ImGui.BeginTabBar("HelpTabs");

        AboutTab();
        HowToTab();

        ImGui.EndTabBar();

    }

    public const string AboutText =
        "Gauge-O-Matic allows you to customize your job gauges by adding extra " +
        "counters, bars, and indicators in various styles. The plugin is a work in progress, " +
        "so please be aware that you may run into bugs or performance issues, and future plugin " +
        "updates might affect your saved settings.\n\n" +
        "The plugin currently has built-in default presets set up for a few jobs, but most still " +
        "need work. The goal for these defaults is to pick out useful datapoints that the " +
        "current gauges don't show, and integrate them into the existing gauges in a way that suits " +
        "each job's aesthetic.\n\n" +
        "Feedback is very helpful! If you have a request for an element that " +
        "you'd like to see on your job's gauge, an idea for a widget design, or if you've come " +
        "up with a preset that you want to share, be sure to send feedback at the plugin's Github repo.";

    private static void AboutTab()
    {
        if (ImGui.BeginTabItem("About the Plugin"))
        {

            ImGui.TextWrapped(AboutText);



            ImGui.EndTabItem();
        }
    }

    private static void HowToTab()
    {
        if (ImGui.BeginTabItem("How-to"))
        {
            ImGui.TextWrapped("There are two ways to add elements to your job gauge: setting them up manually, or loading a preset.");

            ImGui.Spacing();
            ImGui.TextDisabled("ADDING ELEMENTS MANUALLY");

            ImGui.Text("1) Click ");
            ImGui.SameLine();
            IconButtonWithText("Add", FontAwesomeIcon.Plus, "dummyAdd");

            ImGui.Spacing();
            ImGui.Text("2) Select a tracker to use. There are three categories:");

            ImGui.Indent(30f);
            WriteIcon(FontAwesomeIcon.Tags, null, 0x1c6e68ff);
            ImGui.Text("Status Effects - Retrieves the time remaining or number of stacks.");

            WriteIcon(FontAwesomeIcon.FistRaised, null, 0xaa372dff);
            ImGui.Text("Actions - Retrieves the cooldown time or the number of charges available.");

            WriteIcon(FontAwesomeIcon.Gauge, null, 0x2b455cff);
            ImGui.Text("Job Gauge - Retrieves unique data options per job gauge.");

            ImGui.Indent(-30f);

            ImGui.Spacing();
            ImGui.TextWrapped("3) Select a Widget. Widgets fall into these categories:");

            ImGui.Indent(30f);
            ImGui.TextWrapped(
                              "Counters - Shows a count of stacks or charges\n" +
                              "Bars & Timers - Shows a timer or resource value\n" +
                              "State Indicators - Toggles between different visual states (usually on/off)\n" +
                              "Multi-Component - These are groups of widgets designed to layer on top of each other, making one combined design.");

            ImGui.Indent(-30f);

            ImGui.Spacing();
            ImGui.TextWrapped("4) Customize!");
            ImGui.Indent(30f);

            ImGui.TextWrapped("Each widget design has its own set of options to customize the way it looks and behaves. " +
                              "You can also choose which HUD element to pin each widget to, " +
                              "and control the order that widgets layer on top of each other.");

            ImGui.Indent(-30f);

            ImGui.Spacing();
            ImGui.Spacing();
            ImGui.TextDisabled("USING PRESETS");
            ImGui.TextWrapped(
                "Upon opening the preset window, you'll be presented with a list of installed presets. Selecting any one of these will show you its contents.\n\n" +
                "You can:");

            ImGui.Indent(30f);

            ImGui.Text("-Add elements from the preset individually");
            ImGui.SameLine();
            IconButton("", FontAwesomeIcon.Plus, 10f);

            ImGui.Text("-Add all elements at once");
            ImGui.SameLine();
            IconButtonWithText("Add all to...", FontAwesomeIcon.Plus,"dummyAddAll");

            ImGui.Text("-Copy a design onto one of your existing trackers");
            ImGui.SameLine();
            IconButton("", FontAwesomeIcon.Copy, 10f);

            ImGui.Text("-Replace your current settings with the contents of the preset.");
            ImGui.SameLine();
            IconButtonWithText("Overwrite Current", FontAwesomeIcon.PaintRoller, "dummyOverwrite");


            ImGui.Indent(-30f);
            ImGui.TextWrapped("If the preset contains trackers that are not applicable to the selected job, they'll be greyed out, but you can still use the widget designs.");


            ImGui.Spacing();
            ImGui.Spacing();
            ImGui.TextDisabled("ADDING PRESETS");
            ImGui.TextWrapped("To save your current options as a preset, simply type in a name and click");
            ImGui.SameLine();
            IconButtonWithText("Save", FontAwesomeIcon.Save, "dummySave");

            ImGui.TextWrapped("If you've copied a preset from an external source to your clipboard, you can import it by clicking ");
            ImGui.SameLine();
            IconButtonWithText("Import From Clipboard", FontAwesomeIcon.SignInAlt, "dummyImport");





            ImGui.EndTabItem();

        }
    }
}
