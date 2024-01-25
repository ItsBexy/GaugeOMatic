using FFXIVClientStructs.FFXIV.Client.System.Memory;
using FFXIVClientStructs.FFXIV.Component.GUI;
using GaugeOMatic.Utility;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace CustomNodes;

public unsafe partial class CustomNodeManager
{
    public struct CustomPartsList : IDisposable
    {
        public AtkUldPartsList* AtkPartsList;
        private static AtkUldAsset* Asset;

        public static implicit operator AtkUldPartsList*(CustomPartsList c) => c.AtkPartsList;

        public CustomPartsList(string texturePath, params Vector4[] customParts)
        {
            Asset = CreateAsset(texturePath, 1);
            AtkPartsList = CreateAtkPartsList(customParts);
        }

        private static AtkUldAsset* CreateAsset(string texturePath, uint assetID)
        {
            var atkAsset = (AtkUldAsset*)MemoryHelper.Alloc((ulong)sizeof(AtkUldAsset));
            atkAsset->AtkTexture.Ctor();
            atkAsset->Id = assetID;
            atkAsset->AtkTexture.LoadTexture(texturePath, 2);
            return atkAsset;
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
            IMemorySpace.Free(AtkPartsList->Parts, (ulong)sizeof(AtkUldPart) * AtkPartsList->PartCount);
            IMemorySpace.Free(AtkPartsList, (ulong)sizeof(AtkUldPartsList));
        }
    }
}
