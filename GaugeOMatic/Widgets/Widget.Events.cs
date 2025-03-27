using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Dalamud.Game.ClientState.Conditions;
using GaugeOMatic.CustomNodes.Animation;
using GaugeOMatic.GameData;
using GaugeOMatic.Utility;
using static GaugeOMatic.GameData.FrameworkData;

namespace GaugeOMatic.Widgets;

public abstract partial class Widget
{
    public virtual string? SharedEventGroup => null;

    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public struct SharedEventArgs
    {
        public Color.ColorRGB? ColorRGB;
        public Color.AddRGB? AddRGB;
        public int? Intval;
        public float? Floatval;
    }

    public Dictionary<string, Action<SharedEventArgs?>> SharedEvents = new();

    public void InvokeSharedEvent(string group, string eventLabel, SharedEventArgs? args = null)
    {
        foreach (var widget in Tracker.JobModule.WidgetList.Where(widget => widget?.SharedEventGroup == group))
            if (widget!.SharedEvents.TryGetValue(eventLabel, out var action))
                action.Invoke(args);
    }

    public bool IsVisible { get; set; } = true;

    public void ApplyDisplayRules()
    {
        if (!ClientState.IsPvP && (Tracker.UsePreviewValue || (CheckLevel() && CheckFlags())))
            Show();
        else
            Hide();
        return;

        bool CheckLevel()
        {
            if (!TrackerConfig.LimitLevelRange) return true;
            if (TrackerConfig is { LevelMin: 1, LevelMax: JobData.LevelCap }) return true;
            var level = LocalPlayer.Lvl;
            return level >= TrackerConfig.LevelMin && level <= TrackerConfig.LevelMax;
        }

        bool CheckFlags() => !TrackerConfig.HideOutsideCombatDuty ||
                             Condition.Any(ConditionFlag.InCombat, ConditionFlag.BoundByDuty,
                                           ConditionFlag.BoundByDuty56, ConditionFlag.BoundByDuty95,
                                           ConditionFlag.InDeepDungeon);
    }

    public void Hide()
    {
        if (!IsVisible) return;

        IsVisible = false;
        Animator += new Tween(WidgetRoot, WidgetRoot, new(100) { Alpha = 0 });
    }

    public void Show()
    {
        if (IsVisible) return;

        IsVisible = true;
        Animator += new Tween(WidgetRoot, WidgetRoot, new(100) { Alpha = 255 });
    }
}
