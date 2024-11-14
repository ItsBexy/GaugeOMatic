using CustomNodes;
using FFXIVClientStructs.FFXIV.Component.GUI;
using GaugeOMatic.CustomNodes.Animation;
using GaugeOMatic.Trackers;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using static CustomNodes.CustomNode;
using static CustomNodes.CustomNodeManager;
using static Dalamud.Game.ClientState.Conditions.ConditionFlag;
using static GaugeOMatic.GameData.JobData;
using static GaugeOMatic.Utility.Color;
using static GaugeOMatic.Widgets.WidgetAttribute;
using static GaugeOMatic.Widgets.WidgetUI;
using static GaugeOMatic.Widgets.WidgetUI.WidgetUiTab;
using static System.Activator;

// ReSharper disable VirtualMemberCallInConstructor

namespace GaugeOMatic.Widgets;

public abstract unsafe class Widget : IDisposable
{
    [SuppressMessage("ReSharper", "VirtualMemberCallInConstructor")]
    protected Widget(Tracker tracker)
    {
        Tracker = tracker;
        InitConfigs();

        if (!GetAttributes.AddonPermitted(Tracker.AddonName)) Tracker.AddonName = Tracker.JobModule.AddonOptions.First(a => GetAttributes.AddonPermitted(a.Name)).Name;

        Addon = (AtkUnitBase*)GameGui.GetAddonByName(Tracker.AddonName);

        WidgetContainer = BuildContainer();
        WidgetRoot = new(CreateResNode(), WidgetContainer);
        WidgetRoot.AssembleNodeTree();
        Attach();

        ApplyConfigs();
    }

    public static Widget? Create(Tracker tracker)
    {
        if (string.IsNullOrEmpty(tracker.WidgetType)) return null;

        var type = Type.GetType($"{typeof(Widget).Namespace}.{tracker.WidgetType}");

        return type == null ? null :
               string.IsNullOrEmpty(tracker.AddonName) ? null :
               (AtkUnitBase*)GameGui.GetAddonByName(tracker.AddonName) == null ? null :
               (Widget?)CreateInstance(type, tracker);
    }

    public Tracker Tracker { get; set; }
    public TrackerConfig TrackerConfig => Tracker.TrackerConfig;
    public AtkUnitBase* Addon;

    public WidgetAttribute GetAttributes => WidgetList.TryGetValue(GetType().Name, out var widgetInfo) ? widgetInfo : new();

    public abstract WidgetTypeConfig Config { get; }

    public Animator Animator = new();
    public CustomNode WidgetContainer;
    public CustomNode WidgetRoot;
    public virtual CustomNode BuildContainer() => new(CreateResNode());
    public virtual CustomPartsList[] PartsLists { get; } = [];

    public void Dispose()
    {
        Animator.Dispose();
        Detach();
        if (WidgetRoot.Node != null) WidgetRoot.Dispose();
        foreach (var partsList in PartsLists) partsList.Dispose();
    }

    public void Attach()
    {
        if (Addon == null || WidgetRoot.Node == null) return;
        WidgetRoot.AttachTo(Addon);
    }

    public void Detach()
    {
        if (WidgetRoot.Node != null) WidgetRoot.Detach();
        if (Addon != null) Addon->UldManager.UpdateDrawNodeList();
    }

    public WidgetUiTab UiTab { get; set; } = Layout;

    public virtual void DrawUI()
    {
        switch (UiTab)
        {
            case Layout:
                PositionControls("Position", ref Config.Position);
                ScaleControls("Scale", ref Config.Scale);
                break;
            default:
                break;
        }
    }

    public abstract void Update();
    public abstract void InitConfigs();
    public abstract void ResetConfigs();
    public abstract void ApplyConfigs();

    public CustomNode ImageNodeFromPart(int list, ushort partId) => new(CreateImageNode(PartsLists[list], partId));
    public CustomNode ClippingMaskFromPart(int list, ushort partId) => new(CreateClippingMaskNode(PartsLists[list], partId));
    public CustomNode NineGridFromPart(int list, ushort partId) => new(CreateNineGridNode(PartsLists[list], partId));
    public CustomNode NineGridFromPart(int list, ushort partId, int x, int y, int z, int w) => new CustomNode(CreateNineGridNode(PartsLists[list], partId)).SetNineGridOffset(x, y, z, w);

    public virtual string? SharedEventGroup => null;

    public bool IsVisible { get; set; } = true;

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

    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public struct SharedEventArgs
    {
        public ColorRGB? ColorRGB;
        public AddRGB? AddRGB;
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
            if (TrackerConfig is { LevelMin: 1, LevelMax: LevelCap }) return true;
            var level = GameData.FrameworkData.LocalPlayer?.Level ?? 1;
            return level >= TrackerConfig.LevelMin && level <= TrackerConfig.LevelMax;
        }

        bool CheckFlags() => !TrackerConfig.HideOutsideCombatDuty ||
                             Condition.Any(InCombat, BoundByDuty, BoundByDuty56, BoundByDuty95, InDeepDungeon);
    }

    public virtual Bounds GetBounds() => WidgetRoot.GetDescendants().Where(static n => n.Size is { X: > 0, Y: > 0 } && n.Visible).ToList();

    public void DrawBounds(uint col = 0xffffffffu, int thickness = 1) => GetBounds().Draw(col, thickness);

    public virtual void ChangeScale(float amt)
    {
        Config.Scale += 0.05f * amt;
    }
}

