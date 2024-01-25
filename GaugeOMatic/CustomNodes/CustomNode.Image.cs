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
            node->AtkResNode.Width = customPartsList.AtkPartsList->Parts[partId].Width;
            node->AtkResNode.Height = customPartsList.AtkPartsList->Parts[partId].Height;

            node->AtkResNode.NodeID = GetFreeId();
            RegisteredNodes.Add(node->AtkResNode.NodeID, (AtkResNode*)node);

            return node;
        }
        catch (Exception ex)
        {
            Log.Error("Failed to create image node!\n"+ex.StackTrace);
            return CreateResNode()->GetAsAtkImageNode();
        }
    }
}
