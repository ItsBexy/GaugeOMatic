using FFXIVClientStructs.FFXIV.Component.GUI;
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using static CustomNodes.CustomNodeManager.Tween;
using static GaugeOMatic.GaugeOMatic.Service;
using static GaugeOMatic.Utility.Color;

namespace CustomNodes;

public unsafe partial class CustomNodeManager
{
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public partial struct CustomNode : IDisposable
    {
        public AtkResNode* Node;
        public CustomNode[] Children { get; set; }

        public CustomNode(AtkImageNode* node) : this((AtkResNode*)node) { }
        public CustomNode(AtkNineGridNode* node) : this((AtkResNode*)node) { }
        public CustomNode(AtkTextNode* node) : this((AtkResNode*)node) { }
        public static implicit operator AtkResNode*(CustomNode c) => c.Node;

        public readonly CustomNode this[int i]
        {
            get
            {
                if (i < Children.Length) return Children[i];
                var ex = new StackTrace();
                Log.Error("No child node found at index "+i+"\n"+ex);
                return this;
            }
        }

        public CustomNode(AtkResNode* node)
        {
            Node = node;
            Children = null!;
        }

        public CustomNode(AtkResNode* node,params CustomNode[] children)
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
            if (Children == null || Children.Length == 0)
            {
                return Node->ChildCount;
            }

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
            if (Children != null)
                for (var i = 0; i < Children.Length; i++)
                    Children[i].Dispose();
            
            Node->Destroy(true);
            Node = null;
        }

        public readonly Tween CreateTween(params KeyFrame[] keyFrames) => new(this, keyFrames);

        public readonly Tween CreateTweenTo(KeyFrame keyFrame) => new(this,this,keyFrame);

        public static implicit operator KeyFrame(CustomNode n) =>
            new(0)
            {
                X = n.Node->X,
                Y = n.Node->Y,
                Width = n.Node->Width,
                Height = n.Node->Height,
                ScaleX = n.Node->ScaleX,
                ScaleY = n.Node->ScaleY,
                Alpha = n.Node->Color.A,
                Rotation = n.Node->Rotation,
                RGB = (ColorRGB)n.Node->Color,
                AddRGB = new(n.Node->AddRed,n.Node->AddGreen,n.Node->AddBlue),
                MultRGB = new(n.Node->MultiplyRed, n.Node->MultiplyGreen, n.Node->MultiplyBlue)
            };
    }

    public static void Attach(AtkUnitBase* parentAddon, AtkResNode* newChildNode)
    {
        if (parentAddon == null) return;
        Attach(parentAddon->RootNode, newChildNode);
        parentAddon->UldManager.UpdateDrawNodeList();
    }

    public static void Attach(AtkResNode* parentNode, AtkResNode* newChildNode)
    {
        if (parentNode == null) return;
        try
        {
            var lastChildNode = parentNode->ChildNode;
            if (lastChildNode == null) return;

            while (lastChildNode->PrevSiblingNode != null)
            {
                if (lastChildNode == newChildNode) return;
                lastChildNode = lastChildNode->PrevSiblingNode;
            }

            if (lastChildNode == newChildNode) return;

            LinkSiblings(lastChildNode, newChildNode);
        }
        catch (Exception ex) { Log.Error(ex + ""); }
    }

    private static void LinkSiblings(AtkResNode* nodeA, AtkResNode* nodeB)
    {
        if (nodeA == null || nodeB == null) return;
        nodeA->PrevSiblingNode = nodeB;
        nodeB->NextSiblingNode = nodeA;
    }

    public static void Detach(CustomNode customNode) => Detach(customNode.Node);

    public static void Detach(AtkResNode* node)
    {
        var sibling = node->NextSiblingNode;
        if (sibling != null && sibling->PrevSiblingNode == node) sibling->PrevSiblingNode = null;
        node->NextSiblingNode = null;
    }
}
