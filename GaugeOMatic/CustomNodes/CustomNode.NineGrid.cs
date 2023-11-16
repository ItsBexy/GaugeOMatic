using FFXIVClientStructs.FFXIV.Component.GUI;
using GaugeOMatic.Utility;
using System;

namespace CustomNodes;

public unsafe partial class CustomNodeManager
{
    public static AtkNineGridNode* CreateNineGridNode(CustomPartsList customPartsList, ushort partId)
    {
        try
        {
            var partsList = customPartsList.AtkPartsList;
            var node = MemoryHelper.CleanAlloc<AtkNineGridNode>();

            node->Ctor();
            node->PartID = partId;
            node->PartsList = partsList;

            node->AtkResNode.NodeID = GetFreeId();

            RegisteredNodes.Add(node->AtkResNode.NodeID, (AtkResNode*)node);

            node->AtkResNode.Type = NodeType.NineGrid;
            node->AtkResNode.NodeFlags =
                NodeFlags.Visible | NodeFlags.AnchorLeft | NodeFlags.AnchorTop | NodeFlags.Enabled;
            node->AtkResNode.Width = partsList->Parts[partId].Width;
            node->AtkResNode.Height = partsList->Parts[partId].Height;

            InitializePosition(node, 0, 0);

            return node;
        }
        catch (Exception ex)
        {
            GaugeOMatic.GaugeOMatic.Service.Log.Error(ex + "");
            return null;
        }
    }

    private static void InitializePosition(AtkNineGridNode* node, float x, float y)
    {
        node->AtkResNode.SetPositionFloat(1, 1);
        node->AtkResNode.SetPositionFloat(x, y);
    }
}
