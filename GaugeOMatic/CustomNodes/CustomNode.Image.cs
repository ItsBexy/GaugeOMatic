using FFXIVClientStructs.FFXIV.Component.GUI;
using System;
using System.Numerics;
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
            node->AtkResNode.NodeFlags = NodeFlags.Visible | NodeFlags.AnchorLeft | NodeFlags.AnchorTop | NodeFlags.Enabled | NodeFlags.EmitsEvents;
            node->AtkResNode.Width = customPartsList.AtkUldPartsList->Parts[partId].Width;
            node->AtkResNode.Height = customPartsList.AtkUldPartsList->Parts[partId].Height;

            node->AtkResNode.NodeId = GetFreeId();
            RegisteredNodes.Add(node->AtkResNode.NodeId, (AtkResNode*)node);

            return node;
        }
        catch (Exception ex)
        {
            Log.Error("Failed to create image node!\n" + ex.StackTrace);
            return CreateResNode()->GetAsAtkImageNode();
        }
    }

    public static AtkImageNode* CreateIconNode(uint iconId)
    {
        var iconNode = CreateImageNode(new CustomPartsList(CustomPartsList.CreateAsset(), new Vector4(0, 0, 40, 40)), 0);
        iconNode->LoadIconTexture(iconId,0);
        return iconNode;
    }

    public static AtkClippingMaskNode* CreateClippingMaskNode(CustomPartsList customPartsList, ushort partId)
    {
        try
        {
            var node = CleanAlloc<AtkClippingMaskNode>();
            node->Ctor();

            node->PartsList = customPartsList;
            node->PartId = partId;

            node->AtkResNode.Type = NodeType.ClippingMask;
            node->AtkResNode.NodeFlags = NodeFlags.Visible | NodeFlags.AnchorLeft | NodeFlags.AnchorTop | NodeFlags.Enabled | NodeFlags.EmitsEvents;
            node->AtkResNode.Width = customPartsList.AtkUldPartsList->Parts[partId].Width;
            node->AtkResNode.Height = customPartsList.AtkUldPartsList->Parts[partId].Height;

            node->AtkResNode.NodeId = GetFreeId();
            RegisteredNodes.Add(node->AtkResNode.NodeId, (AtkResNode*)node);

            return node;
        }
        catch (Exception ex)
        {
            Log.Error("Failed to create clipping mask node!\n" + ex.StackTrace);
            var node = (AtkClippingMaskNode*)CreateResNode();
            node->Type = NodeType.ClippingMask;
            return node;
        }
    }
}
