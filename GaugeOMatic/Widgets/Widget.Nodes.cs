using System.Linq;
using CustomNodes;
using FFXIVClientStructs.FFXIV.Component.GUI;
using GaugeOMatic.CustomNodes.Animation;
using static CustomNodes.CustomNodeManager;

namespace GaugeOMatic.Widgets;

public abstract unsafe partial class Widget
{
    public virtual CustomPartsList[] PartsLists { get; } = [];

    public virtual CustomNode BuildContainer() => new(CreateResNode());

    public Animator Animator = new();
    public CustomNode WidgetContainer;
    public CustomNode WidgetRoot;

    public CustomNode ImageNodeFromPart(int list, ushort partId) => new(CreateImageNode(PartsLists[list], partId));
    public CustomNode ClippingMaskFromPart(int list, ushort partId) => new(CreateClippingMaskNode(PartsLists[list], partId));
    public CustomNode NineGridFromPart(int list, ushort partId) => new(CreateNineGridNode(PartsLists[list], partId));
    public CustomNode NineGridFromPart(int list, ushort partId, int x, int y, int z, int w) => new CustomNode(CreateNineGridNode(PartsLists[list], partId)).SetNineGridOffset(x, y, z, w);

    public virtual CustomNode.Bounds GetBounds() => WidgetRoot.GetDescendants().Where(static n => n.Size is { X: > 0, Y: > 0 } && n.Visible).ToList();

    public void DrawBounds(uint col = 0xffffffffu, int thickness = 1) => GetBounds().Draw(col, thickness);

    public virtual void ChangeScale(float amt)
    {
        Config.Scale += 0.05f * amt;
    }

    public AtkUnitBase* Addon;

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
}
