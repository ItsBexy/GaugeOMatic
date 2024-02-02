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
        internal static uint NextPLId;
        internal static uint NextAssetId;

        public string TexturePath;
        public Vector4[] Coordinates;
        public int Count;

        public AtkUldPartsList* AtkUldPartsList;
        public AtkUldAsset* Asset;
        public AtkTexture Texture;

        public static implicit operator AtkUldPartsList*(CustomPartsList c) => c.AtkUldPartsList;

        public CustomPartsList(string texturePath, params Vector4[] coords)
        {
            Coordinates = coords;
            Count = Coordinates.Length;

            TexturePath = texturePath;
            Asset = CreateAsset(TexturePath, NextPLId++);
            Texture = Asset->AtkTexture;

            AtkUldPartsList = CreateAtkUldPartsList(Coordinates, Asset, NextAssetId++);
        }

        private static AtkUldAsset* CreateAsset(string texturePath, uint assetID)
        {
            var atkAsset = (AtkUldAsset*)MemoryHelper.Alloc((ulong)sizeof(AtkUldAsset));
            atkAsset->AtkTexture.Ctor();
            atkAsset->Id = assetID;
            atkAsset->AtkTexture.LoadTexture(texturePath, 2);
            return atkAsset;
        }

        private static AtkUldPartsList* CreateAtkUldPartsList(IReadOnlyList<Vector4> coords, AtkUldAsset* asset, uint id)
        {
            var count = coords.Count;

            var atkUldPartsList = MemoryHelper.Alloc<AtkUldPartsList>();
            var atkParts = (AtkUldPart*)MemoryHelper.Alloc((ulong)sizeof(AtkUldPart) * (ulong)count);

            for (var i = 0; i < count; i++)
            {
                var part = coords[i];
                atkParts[i].U = (ushort)part.X;
                atkParts[i].V = (ushort)part.Y;
                atkParts[i].Width = (ushort)part.Z;
                atkParts[i].Height = (ushort)part.W;
                atkParts[i].UldAsset = asset;
            }

            atkUldPartsList->Id = id;
            atkUldPartsList->PartCount = (uint)count;
            atkUldPartsList->Parts = atkParts;

            return atkUldPartsList;
        }

        public void Dispose()
        {
            try
            {
                IMemorySpace.Free(AtkUldPartsList->Parts, (ulong)sizeof(AtkUldPart) * AtkUldPartsList->PartCount);
                IMemorySpace.Free(AtkUldPartsList, (ulong)sizeof(AtkUldPartsList));
                IMemorySpace.Free(Asset, (ulong)sizeof(AtkUldAsset));
                Texture.ReleaseTexture();
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
