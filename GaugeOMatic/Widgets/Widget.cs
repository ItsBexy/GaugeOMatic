using FFXIVClientStructs.FFXIV.Component.GUI;
using GaugeOMatic.Trackers;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using static CustomNodes.CustomNodeManager;
using static GaugeOMatic.Widgets.WidgetAttribute;
using static GaugeOMatic.Widgets.Common.WidgetUI;
using static GaugeOMatic.Widgets.Common.WidgetUI.WidgetUiTab;
using static System.Activator;

// ReSharper disable VirtualMemberCallInConstructor

namespace GaugeOMatic.Widgets;

public abstract unsafe partial class Widget : IDisposable
{
    [SuppressMessage("ReSharper", "VirtualMemberCallInConstructor")]
    protected Widget(Tracker tracker)
    {
        Tracker = tracker;
        InitConfigs();

        if (!GetAttributes.AddonPermitted(Tracker.AddonName)) Tracker.AddonName = Tracker.JobModule.AddonOptions.First(a => GetAttributes.AddonPermitted(a.Name)).Name;

        Addon = (AtkUnitBase*)GameGui.GetAddonByName(Tracker.AddonName);

        WidgetContainer = BuildContainer();
        WidgetIconContainer = BuildWidgetIcon(tracker);
        WidgetContainer.Append(WidgetIconContainer);
        WidgetRoot = new(CreateResNode(), WidgetContainer);

        WidgetIconContainer.AttachTo(WidgetContainer);
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

    public WidgetAttribute GetAttributes => WidgetList.TryGetValue(GetType().Name, out var widgetAttr) ? widgetAttr : new();
    public Tracker Tracker { get; set; }
    public TrackerConfig TrackerConfig => Tracker.TrackerConfig;

    public void Dispose()
    {
        Animator.Dispose();
        Detach();
        if (WidgetRoot.Node != null) WidgetRoot.Dispose();
        foreach (var partsList in PartsLists) partsList.Dispose();
    }

    public WidgetUiTab UiTab { get; set; } = Layout;

    public abstract WidgetTypeConfig Config { get; }

    public abstract void Update();
    public abstract void InitConfigs();
    public abstract void ResetConfigs();

    public virtual void ApplyConfigs()
    {
        WidgetContainer.SetPos(Config.Position)
                       .SetScale(Config.Scale);

        WidgetIconContainer.SetVis(Config.ShowIcon)
                           .SetPos(Config.IconPosition)
                           .SetScale(Config.IconScale);
    }

    public virtual void DrawUI()
    {
        switch (UiTab)
        {
            case Layout:
                PositionControls("Position", ref Config.Position);
                ScaleControls("Scale", ref Config.Scale);
                break;
            case Icon:
                ToggleControls("Show Icon", ref Config.ShowIcon);
                if (Config.ShowIcon)
                {
                    PositionControls("Position##iconPos", ref Config.IconPosition);
                    ScaleControls("Scale##iconScale", ref Config.IconScale);
                }
                break;
            default:
                break;
        }
    }
}

