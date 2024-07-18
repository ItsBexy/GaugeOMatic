using FFXIVClientStructs.FFXIV.Component.GUI;
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using static CustomNodes.CustomNodeManager;

namespace CustomNodes;

[SuppressMessage("ReSharper", "UnusedMember.Global")]
public unsafe partial class CustomNode : IDisposable
{
    public AtkResNode* Node;
    public CustomNode[] Children { get; set; }

    public byte* TextBuffer;
    public int TextBufferLen;

    public CustomNode(AtkImageNode* node) : this((AtkResNode*)node) { }
    public CustomNode(AtkNineGridNode* node) : this((AtkResNode*)node) { }
    public CustomNode(AtkTextNode* node) : this((AtkResNode*)node) { }
    public CustomNode(AtkClippingMaskNode* node) : this((AtkResNode*)node) { }

    public static implicit operator AtkResNode*(CustomNode c) => c.Node;
    public static implicit operator CustomNode(AtkResNode* a) => new(a);

    public CustomNode this[int i]
    {
        get
        {
            if (i < Children.Length) return Children[i];
            var ex = new StackTrace();
            Log.Warning("No child node found at index " + i + "\n" + ex);
            return this;
        }
    }

    public CustomNode(AtkResNode* node)
    {
        Node = node;
        Children = Array.Empty<CustomNode>();
    }

    public CustomNode(AtkResNode* node, params CustomNode[] children)
    {
        Node = node;
        Children = children;
    }

    public CustomNode()
    {
        Node = null;
        Children = Array.Empty<CustomNode>();
    }

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
                System.Runtime.InteropServices.NativeMemory.Free(TextBuffer);
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
