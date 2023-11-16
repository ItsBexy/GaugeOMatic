using FFXIVClientStructs.FFXIV.Component.GUI;
using GaugeOMatic.Utility;

namespace CustomNodes;

public unsafe partial class CustomNodeManager
{
    public static AtkResNode* CreateResNode()
    {
        var node = MemoryHelper.CleanAlloc<AtkResNode>();
        node->Ctor();
        node->NodeID = GetFreeId();

        RegisteredNodes.Add(node->NodeID,node);

        node->Type = NodeType.Res;
        node->NodeFlags = NodeFlags.Visible | NodeFlags.AnchorLeft | NodeFlags.AnchorTop | NodeFlags.Enabled;

        InitializePosition(node, 0, 0);

        return node;
    }

    private static void InitializePosition(AtkResNode* node, int x, int y)
    {
        node->SetPositionFloat(1, 1);
        node->SetPositionFloat(x, y);
    }
}
