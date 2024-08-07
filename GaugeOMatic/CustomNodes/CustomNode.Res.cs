using FFXIVClientStructs.FFXIV.Component.GUI;
using static FFXIVClientStructs.FFXIV.Component.GUI.NodeFlags;
using static FFXIVClientStructs.FFXIV.Component.GUI.NodeType;
using static GaugeOMatic.Utility.MemoryHelper;

namespace CustomNodes;

public unsafe partial class CustomNodeManager
{
    public static AtkResNode* CreateResNode()
    {
        var node = CleanAlloc<AtkResNode>();
        node->Ctor();
        node->NodeId = GetFreeId();

        RegisteredNodes.TryAdd(node->NodeId, node);

        node->Type = Res;
        node->NodeFlags = Visible | AnchorLeft | AnchorTop | Enabled;

        InitializePosition(node, 0, 0);

        return node;
    }

    private static void InitializePosition(AtkResNode* node, int x, int y)
    {
        node->SetPositionFloat(1, 1);
        node->SetPositionFloat(x, y);
    }
}
