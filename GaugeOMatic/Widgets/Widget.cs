using CustomNodes;
using FFXIVClientStructs.FFXIV.Component.GUI;
using GaugeOMatic.CustomNodes.Animation;
using GaugeOMatic.Trackers;
using GaugeOMatic.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using static CustomNodes.CustomNodeManager;
using static Dalamud.Game.ClientState.Conditions.ConditionFlag;
using static GaugeOMatic.GameData.JobData;
using static GaugeOMatic.Utility.Color;
using static Newtonsoft.Json.JsonConvert;
using static System.Activator;
using static GaugeOMatic.Trackers.Tracker;

namespace GaugeOMatic.Widgets;

public abstract unsafe class Widget : IDisposable
{
    [SuppressMessage("ReSharper", "VirtualMemberCallInConstructor")]
    protected Widget(Tracker tracker)
    {
        Tracker = tracker;
        InitConfigs();

        if (!WidgetInfo.AddonPermitted(Tracker.AddonName))
        {
            Tracker.AddonName = Tracker.JobModule.AddonOptions.First(a => WidgetInfo.AddonPermitted(a.Name)).Name;
        }

        Addon = (AtkUnitBase*)GameGui.GetAddonByName(Tracker.AddonName);

        WidgetContainer = BuildContainer();
        WidgetRoot = new CustomNode(CreateResNode(), WidgetContainer);
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

    public abstract WidgetInfo WidgetInfo { get; }
    public Tracker Tracker { get; set; }
    public TrackerConfig TrackerConfig => Tracker.TrackerConfig;
    public AtkUnitBase* Addon;

    public Animator Animator = new();
    public CustomNode WidgetContainer;
    public CustomNode WidgetRoot;
    public virtual CustomNode BuildContainer() => new(CreateResNode());
    public virtual CustomPartsList[] PartsLists { get; } = Array.Empty<CustomPartsList>();

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

    public abstract void DrawUI(ref WidgetConfig widgetConfig, ref UpdateFlags update);
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
        Animator += new Tween(WidgetRoot, WidgetRoot, new (100) { Alpha = 255 });
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
        bool CheckLevel()
        {
            if (!TrackerConfig.LimitLevelRange) return true;
            if ((TrackerConfig.LevelMin ?? 1) == 1 && (TrackerConfig.LevelMax ?? LevelCap) == LevelCap) return true;
            var level = ClientState.LocalPlayer?.Level ?? 1;
            return !(level < TrackerConfig.LevelMin || level > TrackerConfig.LevelMax);
        }

        bool CheckFlags() => !TrackerConfig.HideOutsideCombatDuty || Condition.Any(InCombat, BoundByDuty, BoundByDuty56, BoundByDuty95, InDeepDungeon);

        if (!ClientState.IsPvP && (Tracker.UsePreviewValue || (CheckLevel() && CheckFlags())))
            Show();
        else
            Hide();
    }
}

public partial class WidgetConfig // each widget contributes a part to this in its own file
{
    public string? WidgetType { get; set; }
    public static implicit operator string(WidgetConfig w) => SerializeObject(w, Json.JsonSettings);
    public static implicit operator WidgetConfig?(string s) => DeserializeObject<WidgetConfig>(s) ?? null;

    public WidgetConfig CleanUp(string? widgetType)
    {
        foreach (var p in
                 from p in typeof(WidgetConfig).GetProperties()
                 where p.GetValue(this) != null
                 let decType = p.PropertyType.DeclaringType?.Name
                 where decType != null && decType != widgetType
                 select p)
            p.SetValue(this, null);

        return this;
    }
}

public abstract class WidgetTypeConfig
{

}
