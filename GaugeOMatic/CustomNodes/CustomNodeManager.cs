using FFXIVClientStructs.FFXIV.Component.GUI;
using FFXIVClientStructs.Interop;
using System;
using System.Collections.Generic;

namespace CustomNodes;

public unsafe partial class CustomNodeManager
{
    public static void AttachNode(AtkUnitBase* parentAddon, AtkResNode* newChildNode)
    {
        if (parentAddon == null) return;
        AttachNode(parentAddon->RootNode, newChildNode);
        parentAddon->UldManager.UpdateDrawNodeList();
    }

    public static void AttachNode(AtkResNode* parentNode, AtkResNode* newChildNode)
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

    public static void LinkSiblings(AtkResNode* nodeA, AtkResNode* nodeB)
    {
        if (nodeA == null || nodeB == null) return;
        nodeA->PrevSiblingNode = nodeB;
        nodeB->NextSiblingNode = nodeA;
    }

    public static void DetachNode(AtkResNode* node)
    {
        var sibling = node->NextSiblingNode;
        if (sibling != null && sibling->PrevSiblingNode == node) sibling->PrevSiblingNode = null;
        node->NextSiblingNode = null;
    }
}

public partial class CustomNodeManager
{
    internal static Dictionary<uint, Pointer<AtkResNode>> RegisteredNodes = new();

    public static unsafe uint GetFreeId()
    {
        for (uint i = 10000; i < 90000; i++)
        {
            if (RegisteredNodes.TryGetValue(i, out var node) && node.Value != null && node.Value->NodeId == i) continue;

            RegisteredNodes.Remove(i);
            return i;
        }

        return 90001;
    }
}
