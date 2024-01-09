using FFXIVClientStructs.FFXIV.Component.GUI;
using GaugeOMatic.Utility;

namespace CustomNodes;

public unsafe partial class CustomNodeManager
{
    public static AtkTextNode* CreateTextNode(string text, int fontSize, int alignFontType)
    {
        var node = MemoryHelper.CleanAlloc<AtkTextNode>();
        node->Ctor();

        node->SetText(text);
        node->AlignmentFontType = (byte)alignFontType;
        node->FontSize = (byte)fontSize;
        node->TextFlags |= 24;
        node->AtkResNode.Width = (ushort)((fontSize - 3) * (text.Length + 1));

        node->AtkResNode.NodeID = GetFreeId();

        RegisteredNodes.Add(node->AtkResNode.NodeID, (AtkResNode*)node);
        node->AtkResNode.Type = NodeType.Text;
        node->AtkResNode.NodeFlags = NodeFlags.Visible | NodeFlags.AnchorLeft | NodeFlags.AnchorTop | NodeFlags.Enabled;
        node->AtkResNode.DrawFlags |= 8;

        InitializePosition((AtkResNode*)node,0,0);

        return node;
    }
}
