using FFXIVClientStructs.FFXIV.Client.System.Memory;
using FFXIVClientStructs.FFXIV.Component.GUI;
using GaugeOMatic.Utility;
using Lumina.Data.Files;
using System;
using System.Collections.Generic;
using System.Numerics;
using static FFXIVClientStructs.FFXIV.Client.Graphics.Kernel.Texture;

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
            Asset = CreateAsset(TexturePath);
            Texture = Asset->AtkTexture;

            AtkUldPartsList = CreateAtkUldPartsList(Coordinates, Asset);
        }

        public CustomPartsList(AtkUldAsset* asset, params Vector4[] coords)
        {
            Coordinates = coords;
            Count = Coordinates.Length;

            TexturePath = "";
            Asset = asset;
            Texture = Asset->AtkTexture;

            AtkUldPartsList = CreateAtkUldPartsList(Coordinates, Asset);
        }

        public static AtkUldAsset* CreateAsset(string texturePath)
        {
            var atkAsset = (AtkUldAsset*)MemoryHelper.Alloc((ulong)sizeof(AtkUldAsset));
            atkAsset->AtkTexture.Ctor();
            atkAsset->Id = NextAssetId++;
            atkAsset->AtkTexture.LoadTexture(texturePath, 2);
            return atkAsset;
        }

        public static AtkUldAsset* AssetFromFile(string filePath)
        {
            var data = DataManager.GameData.GetFileFromDisk<TexFile>(filePath);

            fixed (byte* dataPtr = data.TextureBuffer.RawData)
            {
                var newTexture = CreateTexture2D(data.TextureBuffer.Width, data.TextureBuffer.Height, (byte)data.Header.MipCount, (uint)data.Header.Format, 0u, 0u);
                newTexture->InitializeContents(dataPtr);

                var atkAsset = (AtkUldAsset*)MemoryHelper.Alloc((ulong)sizeof(AtkUldAsset));
                atkAsset->AtkTexture.Ctor();
                atkAsset->Id = NextAssetId++;

                atkAsset->AtkTexture.TextureType = TextureType.KernelTexture;
                atkAsset->AtkTexture.KernelTexture = newTexture;

                return atkAsset;
            }
        }

        private static AtkUldPartsList* CreateAtkUldPartsList(IReadOnlyList<Vector4> coords, AtkUldAsset* asset)
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

            atkUldPartsList->Id = NextPLId++;
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
