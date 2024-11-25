using System;
using System.Collections.Generic;
using System.Numerics;
using Dalamud.Interface.Utility.Raii;
using FFXIVClientStructs.FFXIV.Client.UI;
using ImGuiNET;
using static GaugeOMatic.Widgets.MilestoneType;
using static GaugeOMatic.Widgets.Common.WidgetUI;
using static GaugeOMatic.Widgets.Common.WidgetUI.UpdateFlags;

namespace GaugeOMatic.Widgets;

public abstract partial class Widget
{
    public static readonly List<uint> SoundBlackList = [19,21,74];

    // ReSharper disable once UnusedMethodReturnValue.Global
    public static bool SoundControls(ref MilestoneType soundType, ref float soundMilestone, ref uint soundId, float max)
    {
        var input1 = RadioControls("Play Sound", ref soundType, [None, Above, Below], ["Never", "Above Threshold", "Below Threshold"]);

        var scaledMilestone = soundMilestone * max;

        var input2 = soundType > 0 && FloatControls("Threshold", ref scaledMilestone, 0, max, 1, $"%.0f ({Math.Round(soundMilestone * 100)}%%)");
        if (input2)
        {
            soundMilestone = scaledMilestone / max;
        }

        using var col = ImRaii.PushColor(ImGuiCol.Text, new Vector4(1f, 0.35f, 0.35f, 1), SoundBlackList.Contains(soundId));
        var input3 = soundType > 0 && IntControls("Sound ID (0-79)", ref soundId, 0, 79, 1);

        if (SoundBlackList.Contains(soundId))
        {
            ImGui.TableNextColumn();
            using (ImRaii.TextWrapPos(ImGui.GetWindowSize().X - 10))
            {
                ImGui.TextWrapped($"Sound effect #{soundId} will not be played. It should never be played. We will not play it. We will not help you play it.");
            }
        }
        else
        {
            if (input3)
            {
                UIGlobals.PlaySoundEffect(soundId);
            }
        }

        if (input1 || input2 || input3)
        {
            UpdateFlag |= Save;
            return true;
        }

        return false;
    }
}
