using FFXIVClientStructs.FFXIV.Component.GUI;
using GaugeOMatic.Trackers;
using GaugeOMatic.Utility;
using GaugeOMatic.Windows;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using static CustomNodes.CustomNodeManager;
using static GaugeOMatic.GaugeOMatic.Service;
using static Newtonsoft.Json.JsonConvert;

namespace GaugeOMatic.Widgets;

public abstract unsafe class Widget : IDisposable
{
    [SuppressMessage("ReSharper", "VirtualMemberCallInConstructor")]
    protected Widget(Tracker tracker)
    {
        Tracker = tracker;
        InitConfigs();

        if (WidgetInfo.AllowedAddons?.Contains(Tracker.AddonName) == false) Tracker.AddonName = WidgetInfo.AllowedAddons[0];
        Addon = (AtkUnitBase*)GameGui.GetAddonByName(Tracker.AddonName);

        WidgetRoot = BuildRoot();
        WidgetRoot.AssembleNodeTree();
        Attach();
        ApplyConfigs();
    }

    public static Widget? Create(Tracker tracker) => string.IsNullOrEmpty(tracker.WidgetType) ? null :
                                                     string.IsNullOrEmpty(tracker.AddonName) ? null :
                                                     (AtkUnitBase*)GameGui.GetAddonByName(tracker.AddonName) == null ? null :
                                                     (Widget?)Activator.CreateInstance(Type.GetType($"{typeof(Widget).Namespace}.{tracker.WidgetType}") ?? typeof(Widget), tracker);

    public Tracker Tracker { get; set; }
    public abstract WidgetInfo WidgetInfo { get; }
    public AtkUnitBase* Addon;

    public CustomNode WidgetRoot;
    public List<Tween> Tweens = new();
    public virtual CustomNode BuildRoot() => new(CreateResNode());
    public virtual CustomPartsList[] PartsLists { get; } = Array.Empty<CustomPartsList>();

    public void Dispose()
    {
        Tweens.Clear();
        Detach();
        if (WidgetRoot.Node != null) WidgetRoot.Dispose();
        foreach (var partsList in PartsLists) partsList.Dispose();
    }

    public void RunTweens() => Tweens = new(Tweens.Where(static t => !t.IsStale).Select(static t => t.Run()));

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
    public CustomNode NineGridFromPart(int list, ushort partId) => new(CreateNineGridNode(PartsLists[list], partId));
    public CustomNode NineGridFromPart(int list, ushort partId, int x, int y, int z, int w) => new CustomNode(CreateNineGridNode(PartsLists[list], partId)).SetNineGridOffset(x,y,z,w);

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
    public void InvokeSharedEvent(string group, string eventLabel,SharedEventArgs? args = null)
    {
        foreach (var widget in Tracker.JobModule.WidgetList.Where(widget => widget?.SharedEventGroup == group))
            if (widget!.SharedEvents.TryGetValue(eventLabel, out var action))
                action.Invoke(args);
    }
}

public partial class WidgetConfig // each widget contributes a part to this in its own file
{
    public string? WidgetType { get; set; }
    public static implicit operator string(WidgetConfig w) => SerializeObject(w, Json.JsonSettings);
    public static implicit operator WidgetConfig?(string s) => DeserializeObject<WidgetConfig>(s) ?? null;
}
