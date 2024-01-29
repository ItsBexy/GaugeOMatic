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
        public AtkUldAsset* Asset;
        public string TexturePath;
        public int Count;

        public static implicit operator AtkUldPartsList*(CustomPartsList c) => c.AtkPartsList;

        public CustomPartsList(string texturePath, params Vector4[] customParts)
        {
            Count = customParts.Length;
            TexturePath = texturePath;
            Asset = CreateAsset(TexturePath, 1);
            AtkPartsList = CreateAtkPartsList(customParts,Asset);
        }

        private static AtkUldAsset* CreateAsset(string texturePath, uint assetID)
        {
            var atkAsset = (AtkUldAsset*)MemoryHelper.Alloc((ulong)sizeof(AtkUldAsset));
            atkAsset->AtkTexture.Ctor();
            atkAsset->Id = assetID;
            atkAsset->AtkTexture.LoadTexture(texturePath, 2);
            return atkAsset;
        }

        private static AtkUldPartsList* CreateAtkPartsList(IReadOnlyList<Vector4> customParts, AtkUldAsset* asset)
        {
            var count = customParts.Count;

            var atkPartsList = MemoryHelper.Alloc<AtkUldPartsList>();
            var atkParts = (AtkUldPart*)MemoryHelper.Alloc((ulong)sizeof(AtkUldPart) * (ulong)count);

            for (var i = 0; i < count; i++)
            {
                var part = customParts[i];
                atkParts[i].U = (ushort)part.X;
                atkParts[i].V = (ushort)part.Y;
                atkParts[i].Width = (ushort)part.Z;
                atkParts[i].Height = (ushort)part.W;
                atkParts[i].UldAsset = asset;
            }

            atkPartsList->Id = 1;
            atkPartsList->PartCount = (uint)count;
            atkPartsList->Parts = atkParts;

            return atkPartsList;
        }

        public readonly void Dispose()
        {
            try
            {
                IMemorySpace.Free(AtkPartsList->Parts, (ulong)sizeof(AtkUldPart) * AtkPartsList->PartCount);
                IMemorySpace.Free(AtkPartsList, (ulong)sizeof(AtkUldPartsList));
                Asset->AtkTexture.ReleaseTexture();
                IMemorySpace.Free(Asset,(ulong)sizeof(AtkUldAsset));
            }
            catch (Exception ex)
            {
                Log.Error("Error Disposing parts list!\n" +
                          $"Texture path: {TexturePath}\n" +
                          $"Parts: {Count}\n" +
                          $"{ex.StackTrace}");
            }
        }
    }
}
