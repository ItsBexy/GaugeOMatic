using FFXIVClientStructs.FFXIV.Component.GUI;
using System;
using static GaugeOMatic.Utility.MemoryHelper;

namespace CustomNodes;

public unsafe partial class CustomNodeManager
{
    public static AtkImageNode* CreateImageNode(CustomPartsList customPartsList, ushort partId)
    {
        try
        {
            var node = CleanAlloc<AtkImageNode>();
            node->Ctor();

            node->PartsList = customPartsList;
            node->PartId = partId;

            node->AtkResNode.Type = NodeType.Image;
            node->AtkResNode.NodeFlags = NodeFlags.Visible | NodeFlags.AnchorLeft | NodeFlags.AnchorTop | NodeFlags.Enabled;
            node->AtkResNode.Width = customPartsList.AtkUldPartsList->Parts[partId].Width;
            node->AtkResNode.Height = customPartsList.AtkUldPartsList->Parts[partId].Height;

            node->AtkResNode.NodeId = GetFreeId();
            RegisteredNodes.Add(node->AtkResNode.NodeId, (AtkResNode*)node);

            return node;
        }
        catch (Exception ex)
        {
            Log.Error("Failed to create image node!\n"+ex.StackTrace);
            return CreateResNode()->GetAsAtkImageNode();
        }
    }

    public static AtkImageNode* CreateClippingMaskNode(CustomPartsList customPartsList, ushort partId)
    {
        try
        {
            var node = CleanAlloc<AtkImageNode>();
            node->Ctor();

            node->PartsList = customPartsList;
            node->PartId = partId;

            node->AtkResNode.Type = NodeType.UnknownNode10;
            node->AtkResNode.NodeFlags = NodeFlags.Visible | NodeFlags.AnchorLeft | NodeFlags.AnchorTop | NodeFlags.Enabled;
            node->AtkResNode.Width = customPartsList.AtkUldPartsList->Parts[partId].Width;
            node->AtkResNode.Height = customPartsList.AtkUldPartsList->Parts[partId].Height;

            node->AtkResNode.NodeId = GetFreeId();
            RegisteredNodes.Add(node->AtkResNode.NodeId, (AtkResNode*)node);

            return node;
        }
        catch (Exception ex)
        {
            Log.Error("Failed to create clipping mask node!\n" + ex.StackTrace);
            return CreateResNode()->GetAsAtkImageNode();
        }
    }
}
