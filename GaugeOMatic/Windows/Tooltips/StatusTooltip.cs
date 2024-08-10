using ImGuiNET;
using System.Linq;
using System.Numerics;
using static Dalamud.Interface.Utility.ImGuiHelpers;
using static GaugeOMatic.GameData.Sheets;
using static GaugeOMatic.Utility.ImGuiHelpy;

namespace GaugeOMatic.GameData;

public partial class StatusRef
{
    public override void TooltipHeaderText()
    {
        MulticolorText((Plain,Name),(Disabled,$" [{ID}]"));
        ImGui.TextDisabled(AppliedTo == StatusActor.Target ? "On Enemy Target" : "On Self");
    }

    public override void DrawTooltipIcon(Vector2 startPos)
    {
        var texture = GetIconTexture();
        if (texture != null)
        {
            ImGui.Image(texture.ImGuiHandle, new Vector2(30 * GlobalScale, 40 * GlobalScale) * 1.2f);
            ImGui.SameLine();
        }

        ImGui.SetCursorPos(new(startPos.X + (50 * GlobalScale),
                               startPos.Y + (4 * GlobalScale)));
    }

    public override bool UseCounterAsState() => MaxStacks <= 1;

    public override void PrintStateDesc() => ImGui.Text("Shows if active");
    public override void FooterContents()
    {
        var seeAlsoFiltered = SeeAlso?.Where(static s => ((StatusRef)s).HideFromDropdown == false).ToArray();
        if (seeAlsoFiltered?.Any() == true)
        {
            ImGui.TextDisabled($"Also checks the following status effect{(seeAlsoFiltered.Length > 1 ? "s" : "")}");
            foreach (var id in seeAlsoFiltered)
            {
                var statusRef = (StatusRef)id;
                var row = StatusSheet?.GetRow(id);
                var icon = statusRef.Icon ?? row?.Icon;
                var name = statusRef.Name is { Length: > 0 } ? statusRef.Name : row?.Name.ToString() ?? "UNKNOWN";

                if (icon != null)
                {
                    DrawGameIcon(icon.Value, 22f);
                    SameLineSquished();
                    ImGui.SetCursorPosY(ImGui.GetCursorPosY() + 2);
                }

                MulticolorText((Plain,$" {name}"), (Disabled, $" [{id}]"));


            }

        }
    }

    public override void PrintCounterDesc() => ImGui.Text($"Shows stacks ({MaxStacks})");
    public override void PrintBarTimerDesc() => ImGui.Text($"Shows time remaining{(MaxTime > 0 ? $" ({MaxTime}s)" : "")}");
}
