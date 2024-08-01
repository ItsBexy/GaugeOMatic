using Dalamud.Interface.Textures.TextureWraps;
using ImGuiNET;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using static Dalamud.Interface.Utility.ImGuiHelpers;
using static GaugeOMatic.GameData.ActionFlags;
using static GaugeOMatic.GameData.ActionRef.BarType;
using static GaugeOMatic.GaugeOMatic;
using static GaugeOMatic.Utility.ImGuiHelpy;

namespace GaugeOMatic.GameData;

public partial class ActionRef
{
    public Dictionary<ActionFlags, string> FlagNames = new()
    {
        { LongCooldown, "Cooldown" },
        { HasCharges, "Charges" },
        { ComboBonus, "Combo" },
        { Unassignable, "Unassignable" },
        { RequiresStatus, "Status-Based" },
        { CanGetAnts, "Conditional" },
        { RoleAction, "Role" }
    };

    public static IDalamudTextureWrap? FrameTex => TextureProvider.GetFromFile(Path.Combine(PluginDirPath, @"TextureAssets\iconFrame.png"))
                                                                  .GetWrapOrDefault();

    public override void TooltipHeaderText()
    {
        MulticolorText((Plain, NameChain), (Disabled, $" [{ID}]"));
        ImGui.TextDisabled(string.Join(", ", FlagNames.Where(f => Flags.HasFlag(f.Key)).Select(static f => f.Value)));
    }

    public override void DrawTooltipIcon(Vector2 startPos)
    {
        var texture = GetAdjustedAction().GetIconTexture();

        var frameTex = FrameTex;
        if (texture != null && frameTex != null)
        {
            ImGui.Image(texture.ImGuiHandle, new Vector2(40 * GlobalScale));
            ImGui.SetCursorPos(startPos - (new Vector2(4, 3) * GlobalScale));

            ImGui.Image(frameTex.ImGuiHandle, new Vector2(48 * GlobalScale));
            ImGui.SameLine();
        }

        ImGui.SetCursorPos(new(startPos.X + (50 * GlobalScale),
                               startPos.Y + (2 * GlobalScale)));
    }

    public override bool UseCounterAsState() => !HasFlag(HasCharges);

    public override void PrintBarTimerDesc()
    {
        var barType = ((LongCooldown & Flags) != 0) switch
        {
            false when (Flags & RequiresStatus) != 0 && ReadyStatus != null => StatusTimer,
            false when (Flags & ComboBonus) != 0 => ComboTimer,
            _ => Cooldown
        };

        if (barType == StatusTimer) MulticolorText((Plain, "Shows time remaining on"), (Yellow, ReadyStatus?.Name ?? "?"));
        else if (barType == ComboTimer) ImGui.Text("Shows combo time remaining for this action");
        else ImGui.Text($"Shows recast time remaining ({LastKnownCooldown}s)");
    }

    public override void PrintCounterDesc() => ImGui.Text($"Shows Charges ({GetMaxCharges()})");

    public override void PrintStateDesc() => ImGui.Text("Shows if ready");

    public override void FooterContents()
    {
        var readyStatus = ReadyStatus?.Name ?? "?";
        ImGui.TextDisabled("Ready Conditions");

        if (HasFlag(TransformedButton)) MulticolorText((Plain, "•"), (Orange, GetBaseAction().Name), (Plain, "has changed to this action"));
        if (HasFlag(RequiresStatus) && readyStatus.Length > 1) MulticolorText((Plain, "•"), (Yellow, readyStatus), (Plain, "is active"));

        if (HasFlag(ComboBonus)) ImGui.Text("• This action is the next step in an active combo");
        else if (HasFlag(CanGetAnts)) ImGui.Text("• This action is highlighted");

        if (HasFlag(HasCharges)) ImGui.Text("• At least one charge is available");
        else if (HasFlag(LongCooldown)) ImGui.Text("• This action is off cooldown");
    }
}
