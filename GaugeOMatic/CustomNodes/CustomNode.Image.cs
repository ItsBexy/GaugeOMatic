using FFXIVClientStructs.FFXIV.Client.System.Memory;
using FFXIVClientStructs.FFXIV.Component.GUI;
using GaugeOMatic.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace CustomNodes;

public unsafe partial class CustomNodeManager
{
    public static AtkImageNode* CreateImageNode(CustomPartsList customPartsList, ushort partId)
    {
        try
        {
            var partsList = customPartsList.AtkPartsList;
            var node = MemoryHelper.CleanAlloc<AtkImageNode>();

            node->Ctor();
            node->PartId = partId;
            node->PartsList = partsList;

            node->AtkResNode.NodeID = GetFreeId();

            RegisteredNodes.Add(node->AtkResNode.NodeID, (AtkResNode*)node);

            node->AtkResNode.Type = NodeType.Image;
            node->AtkResNode.NodeFlags = NodeFlags.Visible | NodeFlags.AnchorLeft | NodeFlags.AnchorTop | NodeFlags.Enabled;
            node->AtkResNode.Width = partsList->Parts[partId].Width;
            node->AtkResNode.Height = partsList->Parts[partId].Height;

           /* node->AtkResNode.SetPositionFloat(1, 1);
            node->AtkResNode.SetPositionFloat(0, 0);*/

            return node;
        }
        catch (Exception ex)
        {
            GaugeOMatic.GaugeOMatic.Service.Log.Error(ex + "");
            return CreateResNode()->GetAsAtkImageNode();
        }
    }

    public static AtkImageNode* CreateImageNode(CustomPartsList customPartsList, string partKey)
    {
        var id = customPartsList.Keys.IndexOf(partKey);
        return id >= 0 ? CreateImageNode(customPartsList, (ushort)id) : CreateResNode()->GetAsAtkImageNode();
    }

    public struct CustomPartsList : IDisposable
    {
        public string TexturePath { get; set; }
        public AtkUldPartsList* AtkPartsList;
        private static AtkUldAsset* Asset;

        public List<string> Keys { get; set; } = new();

        public CustomPartsList(string texturePath, params Vector4[] customParts)
        {
            TexturePath = texturePath;
            Asset = CreateAsset(texturePath, 1);
            AtkPartsList = CreateAtkPartsList(customParts);
        }

        public CustomPartsList(string texturePath, Dictionary<string, Vector4> customParts)
        {
            TexturePath = texturePath;
            Asset = CreateAsset(texturePath, 1);
            Keys = customParts.Keys.ToList();
            AtkPartsList = CreateAtkPartsList(customParts.Values.ToArray());
        }

        private static AtkUldPartsList* CreateAtkPartsList(IReadOnlyList<Vector4> customParts)
        {
            var count = customParts.Count;

            var atkPartsList = MemoryHelper.Alloc<AtkUldPartsList>();
            var atkParts = (AtkUldPart*)MemoryHelper.Alloc((ulong)sizeof(AtkUldPart) * (ulong)count);

            for (var i = 0; i < count; i++)
            {
                atkParts[i].U = (ushort)customParts[i].X;
                atkParts[i].V = (ushort)customParts[i].Y;
                atkParts[i].Width = (ushort)customParts[i].Z;
                atkParts[i].Height = (ushort)customParts[i].W;
                atkParts[i].UldAsset = Asset;
            }

            atkPartsList->Id = 1;
            atkPartsList->PartCount = (uint)count;
            atkPartsList->Parts = atkParts;

            return atkPartsList;
        }

        public readonly void Dispose()
        {
            IMemorySpace.Free(AtkPartsList, (ulong)sizeof(AtkUldPartsList));
            IMemorySpace.Free(AtkPartsList->Parts, (ulong)sizeof(AtkUldPart) * AtkPartsList->PartCount);
        }

        private static AtkUldAsset* CreateAsset(string texturePath, uint assetID)
        {
            var atkAsset = (AtkUldAsset*)MemoryHelper.Alloc((ulong)sizeof(AtkUldAsset));
            atkAsset->AtkTexture.Ctor();
            atkAsset->Id = assetID;
            atkAsset->AtkTexture.LoadTexture(ConvertString(texturePath), 2);
            return atkAsset;
        }
    }

    public static byte* ConvertString(string texturePath)
    {
        var byteCount = Encoding.UTF8.GetByteCount(texturePath);
        var span = (Span<byte>)new byte[byteCount + 1];
        Encoding.UTF8.GetBytes((ReadOnlySpan<char>)texturePath, span);
        span[byteCount] = 0;
        fixed (byte* texturePath1 = &span.GetPinnableReference())
            return texturePath1;
    }
}
