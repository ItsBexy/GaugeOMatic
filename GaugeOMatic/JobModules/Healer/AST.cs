using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;
using GaugeOMatic.Trackers;
using GaugeOMatic.Windows;
using ImGuiNET;
using System.Collections.Generic;
using static Dalamud.Interface.Utility.ImGuiHelpers;
using static FFXIVClientStructs.FFXIV.Component.GUI.FontType;
using static GaugeOMatic.GameData.JobData;
using static GaugeOMatic.GameData.JobData.Job;
using static GaugeOMatic.GameData.JobData.Role;
using static GaugeOMatic.Widgets.WidgetUI;
using static GaugeOMatic.Windows.ItemRefMenu;

namespace GaugeOMatic.JobModules;

public class ASTModule : JobModule
{
    public override Job Job => AST;
    public override Job Class => Job.None;
    public override Role Role => Healer;
    public override List<AddonOption> AddonOptions => new()
    {
        new("JobHudAST0", "Arcana Gauge"),
        new("_ParameterWidget", "Parameter Bar")
    };

    public ASTModule(TrackerManager trackerManager, TrackerConfig[] trackerConfigList) : base(trackerManager, trackerConfigList) { }

    public override void Save()
    {
        Configuration.TrackerConfigs.AST = SaveOrder;
        Configuration.Save();
    }

    public override List<MenuOption> JobGaugeMenu { get; } = new();

    public override void TweakUI(ref UpdateFlags update)
    {
        var fontIndex = FontList.IndexOf(TweakConfigs.ASTCardFont);

        LabelColumn("Card Font");

        ImGui.SetNextItemWidth(142 * GlobalScale);
        if (ImGui.Combo("##TweakASTCardFont", ref fontIndex, FontNames, FontNames.Length))
        {
            update |= UpdateFlags.Save;
            TweakConfigs.ASTCardFont = FontList[fontIndex];
        }

        if (update.HasFlag(UpdateFlags.Save)) ApplyTweaks();
    }

    public readonly List<FontType> FontList = new() { Axis, TrumpGothic, Jupiter };
    public readonly string[] FontNames = { "Axis", "Trump Gothic", "Jupiter" };

    public override unsafe void ApplyTweaks()
    {
        var arcanaGauge = (AddonJobHudAST0*)GameGui.GetAddonByName("JobHudAST0");
        if (arcanaGauge != null && arcanaGauge->GaugeStandard.Container != null) ApplyCardFont();

        void ApplyCardFont()
        {
            var cardFont = TweakConfigs.ASTCardFont;
            var size = (byte)(cardFont == TrumpGothic ? 18 : 12);
            var align = cardFont == TrumpGothic ? AlignmentType.Center : AlignmentType.Bottom;

            ApplyFontProps(arcanaGauge->GaugeStandard.CardName);
            ApplyFontProps(arcanaGauge->GaugeStandard.MinorArcanaName);
            ApplyFontProps(arcanaGauge->GaugeSimple.CardName->AtkTextNode);
            ApplyFontProps(arcanaGauge->GaugeSimple.MinorArcanaName->AtkTextNode);

            void ApplyFontProps(AtkTextNode* node)
            {
                node->FontType = cardFont;
                node->FontSize = size;
                node->AlignmentType = align;
            }
        }
    }
}
public partial class TweakConfigs
{
    public FontType ASTCardFont = Axis;
}
