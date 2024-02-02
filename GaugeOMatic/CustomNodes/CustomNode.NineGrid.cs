using FFXIVClientStructs.FFXIV.Component.GUI;
using System;
using static FFXIVClientStructs.FFXIV.Component.GUI.NodeFlags;
using static FFXIVClientStructs.FFXIV.Component.GUI.NodeType;
using static GaugeOMatic.Utility.MemoryHelper;

namespace CustomNodes;

public unsafe partial class CustomNodeManager
{
    public static AtkNineGridNode* CreateNineGridNode(CustomPartsList customPartsList, ushort partId)
    {
        try
        {
            var partsList = customPartsList.AtkUldPartsList;
            var node = CleanAlloc<AtkNineGridNode>();

            node->Ctor();
            node->PartID = partId;
            node->PartsList = partsList;

            node->AtkResNode.NodeID = GetFreeId();

            RegisteredNodes.Add(node->AtkResNode.NodeID, (AtkResNode*)node);

            node->AtkResNode.Type = NineGrid;
            node->AtkResNode.NodeFlags = Visible | AnchorLeft | AnchorTop | Enabled;
            node->AtkResNode.Width = partsList->Parts[partId].Width;
            node->AtkResNode.Height = partsList->Parts[partId].Height;

            InitializePosition(node, 0, 0);

            return node;
        }
        catch (Exception ex)
        {
            Log.Error(ex + "");
            return null;
        }
    }

    private static void InitializePosition(AtkNineGridNode* node, float x, float y)
    {
        node->AtkResNode.SetPositionFloat(1, 1);
        node->AtkResNode.SetPositionFloat(x, y);
    }
}
