using FFXIVClientStructs.FFXIV.Component.GUI;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using static CustomNodes.CustomNodeManager;

namespace CustomNodes;

[SuppressMessage("ReSharper", "UnusedMember.Global")]
public unsafe partial class CustomNode(AtkResNode* node, params CustomNode[] children) : IDisposable
{
    public AtkResNode* Node = node;
    public CustomNode[] Children { get; set; } = children;

    public byte* TextBuffer;
    public int TextBufferLen;

    public CustomNode(AtkImageNode* node) : this((AtkResNode*)node) { }
    public CustomNode(AtkNineGridNode* node) : this((AtkResNode*)node) { }
    public CustomNode(AtkTextNode* node) : this((AtkResNode*)node) { }
    public CustomNode(AtkClippingMaskNode* node) : this((AtkResNode*)node) { }
    public CustomNode(AtkComponentNode* node) : this((AtkResNode*)node) { }

    public static implicit operator CustomNode(AtkResNode* a) => new(a);
    public static implicit operator CustomNode(AtkImageNode* a) => new(a);
    public static implicit operator CustomNode(AtkTextNode* a) => new(a);
    public static implicit operator CustomNode(AtkNineGridNode* a) => new(a);
    public static implicit operator CustomNode(AtkClippingMaskNode* a) => new(a);
    public static implicit operator CustomNode(AtkComponentNode* a) => new(a);

    public static implicit operator AtkResNode*(CustomNode c) => c.Node;

    public CustomNode this[int i]
    {
        get
        {
            if (i < Children.Length) return Children[i];

            try
            {
                AtkComponentNode* comp;
                AtkUldManager uld;

                return Node == null ? Warning("Node is null and has no children") :
                           (comp = Node->GetAsAtkComponentNode()) == null || (uld = comp->Component->UldManager).NodeListSize < i ?
                               Warning($"No Child node found at index {i}") :
                               uld.NodeList[i];
            }
            catch (Exception)
            {
                return Warning($"Error retrieving child node at index {i}");
            }
        }
    }

    public CustomNode this[uint id]
    {
        get
        {
            try
            {
                AtkComponentNode* comp;
                if (Node == null) return Warning("Node is null and has no children");
                if ((comp = Node->GetAsAtkComponentNode()) == null) return Warning($"No Child node found with ID {id}");

                return comp->Component->UldManager.SearchNodeById(id);
            }
            catch (Exception)
            {
                return Warning($"Error retrieving child node with ID {id}");
            }
        }
    }

    public CustomNode(AtkResNode* node) : this(node, []) { }

    public CustomNode() : this(null, []) { }

    public int AssembleNodeTree()
    {
        if (Children.Length == 0) return Node->ChildCount;

        var count = Children.Length;
        for (var i = 0; i < Children.Length; i++)
        {
            Children[i].Node->ParentNode = Node;
            count += Children[i].AssembleNodeTree();
            if (i < Children.Length - 1) LinkSiblings(Children[i].Node, Children[i + 1].Node);
        }

        Node->ChildNode = Children[0].Node;
        Node->ChildCount = (ushort)count;
        return count;
    }

    public void Dispose()
    {
        foreach (var child in Children) child.Dispose();

        if (Node != null)
        {
            if (TextBuffer != null)
            {
                NativeMemory.Free(TextBuffer);
                TextBufferLen = 0;
            }
            Node->Destroy(true);
            Node = null;
        }
    }

    public void Detach() => DetachNode(Node);
    public void AttachTo(AtkUnitBase* parentAddon) => AttachNode(parentAddon, Node);
    public void AttachTo(AtkResNode* parentNode) => AttachNode(parentNode, Node);
}
