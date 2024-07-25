using FFXIVClientStructs.FFXIV.Component.GUI;
using System;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using static CustomNodes.CustomNode.CustomNodeFlags;
using static FFXIVClientStructs.FFXIV.Component.GUI.NodeType;
using static GaugeOMatic.Utility.Color;
using static System.Math;
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedMethodReturnValue.Global

namespace CustomNodes;

public unsafe partial class CustomNode
{
    public CustomNode AddFlags(CustomNodeFlags f)
    {
        Flags |= f;
        return this;
    }

    public CustomNode RemoveFlags(CustomNodeFlags f)
    {
        Flags &= ~f;
        return this;
    }

    public CustomNode SetFlags(CustomNodeFlags f)
    {
        Flags = f;
        return this;
    }

    public static CustomNode operator +(CustomNode c, CustomNodeFlags f) => c.AddFlags(f);
    public static CustomNode operator -(CustomNode c, CustomNodeFlags f) => c.RemoveFlags(f);

    public CustomNode SetVis(bool show)
    {
        Node->ToggleVisibility(show);
        return this;
    }

    public CustomNode Hide() => SetVis(false);
    public CustomNode Show() => SetVis(true);

    public CustomNode SetPos(float x, float y)
    {
        Node->SetPositionFloat(x, y);
        return this;
    }

    public CustomNode SetPos(Vector2 pos) => SetPos(pos.X, pos.Y);

    public CustomNode SetPos(Vector2? pos) => SetPos(pos ?? new Vector2(0));

    public CustomNode SetX(float x)
    {
        Node->SetPositionFloat(x, Node->Y);
        return this;
    }

    public CustomNode SetY(float y)
    {
        Node->SetPositionFloat(Node->X, y);
        return this;
    }

    public CustomNode SetWidth(float w) => SetWidth((ushort)w);

    public CustomNode SetWidth(int w) => SetWidth((ushort)w);

    public CustomNode SetWidth(ushort width)
    {
        if (Node != null) Node->Width = width;
        return this;
    }

    public CustomNode SetHeight(float h) => SetHeight((ushort)h);

    public CustomNode SetHeight(int h) => SetHeight((ushort)h);

    public CustomNode SetHeight(ushort height)
    {
        if (Node != null) Node->Height = height;
        return this;
    }

    public CustomNode SetSize(float w, float h) => SetWidth(w).SetHeight(h);

    public CustomNode SetSize(int w, int h) => SetWidth(w).SetHeight(h);

    public CustomNode SetSize(ushort w, ushort h) => SetWidth(w).SetHeight(h);

    public CustomNode SetSize(Vector2 s) => SetSize((ushort)s.X, (ushort)s.Y);

    public CustomNode SetScale(float s)
    {
        Node->SetScale(s, s);
        Node->DrawFlags |= 0xD;
        return this;
    }

    public CustomNode SetScale(float x, float y) => SetScaleX(x).SetScaleY(y);

    public CustomNode SetScale(Vector2 scale) => SetScale(scale.X, scale.Y);

    public CustomNode SetScale(Vector2? scale) => SetScale(scale ?? new Vector2(1));

    public CustomNode SetScaleX(float x)
    {
        Node->ScaleX = x;
        Node->DrawFlags |= 0xD;
        return this;
    }

    public CustomNode SetScaleY(float y)
    {
        Node->ScaleY = y;
        Node->DrawFlags |= 0xD;
        return this;
    }

    public CustomNode SetOrigin(Vector2 origin)
    {
        Node->OriginX = origin.X;
        Node->OriginY = origin.Y;
        Node->DrawFlags |= 0xD;
        return this;
    }

    public CustomNode SetOrigin(float x, float y) => SetOrigin(new(x, y));

    public CustomNode SetRotation(float angle, bool deg = false)
    {
        if (deg) angle *= 0.0174532925199433f;

        Node->Rotation = angle;
        Node->DrawFlags |= 0xD;
        return this;
    }

    public CustomNode SetRotation(double angle, bool deg = false) => SetRotation((float)angle, deg);

    public CustomNode SetRotation(int angle) => SetRotation(angle, true);

    public CustomNode SetRGB(ColorRGB c) => SetRGB((Vector4)c);
    public CustomNode SetRGB(Vector4 vec4) => SetRGB(new Vector3(vec4.X, vec4.Y, vec4.Z));
    public CustomNode SetRGB(Vector3 vec3)
    {
        Node->Color.R = (byte)(vec3.X * 255);
        Node->Color.G = (byte)(vec3.Y * 255);
        Node->Color.B = (byte)(vec3.Z * 255);
        return this;
    }

    public CustomNode SetRGBA(Vector4 vec4)
    {
        Node->Color.R = (byte)(vec4.X * 255);
        Node->Color.G = (byte)(vec4.Y * 255);
        Node->Color.B = (byte)(vec4.Z * 255);
        Node->Color.A = (byte)(vec4.W * 255);
        return this;
    }

    public CustomNode SetRGBA(byte r, byte g, byte b, byte? a = null)
    {
        Node->Color.R = r;
        Node->Color.G = g;
        Node->Color.B = b;
        if (a != null) { Node->Color.A = a.Value; }
        return this;
    }

    public CustomNode SetAlpha(byte a)
    {
        Node->Color.A = a;
        if (Flags.HasFlag(SetVisByAlpha)) SetVis(a > 0);
        return this;
    }

    public CustomNode SetAlpha(float a)
    {
        Node->Color.A = (byte)(a > 1?a:a*255);
        if (Flags.HasFlag(SetVisByAlpha)) SetVis(a > 0);
        return this;
    }

    public CustomNode SetAlpha(bool b)
    {
        Node->Color.A = (byte)(b ? 255 : 0);
        if (Flags.HasFlag(SetVisByAlpha)) SetVis(b);
        return this;
    }

    public CustomNode SetAddRGB(AddRGB add, bool alpha = false)
    {
        Node->AddRed = add.R;
        Node->AddGreen = add.G;
        Node->AddBlue = add.B;
        if (alpha)
        {
            Node->Color.A = add.A;
            if (Flags.HasFlag(SetVisByAlpha)) SetVis(add.A > 0);
        }
        return this;
    }

    public CustomNode SetAddRGB(short r, short g, short b, byte? a = null)
    {
        Node->AddRed = r;
        Node->AddGreen = g;
        Node->AddBlue = b;
        if (a != null)
        {
            Node->Color.A = a.Value;
            if (Flags.HasFlag(SetVisByAlpha)) SetVis(a.Value > 0);
        }
        return this;
    }

    public CustomNode SetAddRGB(short all)
    {
        Node->AddRed = all;
        Node->AddGreen = all;
        Node->AddBlue = all;
        return this;
    }

    public CustomNode SetMultiply(Vector3 m)
    {
        Node->MultiplyRed = (byte)(m.X * 255f);
        Node->MultiplyGreen = (byte)(m.Y * 255f);
        Node->MultiplyBlue = (byte)(m.Z * 255f);
        return this;
    }

    public CustomNode SetMultiply(byte r, byte g, byte b)
    {
        Node->MultiplyRed = r;
        Node->MultiplyGreen = g;
        Node->MultiplyBlue = b;
        return this;
    }

    public CustomNode SetMultiply(byte all)
    {
        Node->MultiplyRed = all;
        Node->MultiplyGreen = all;
        Node->MultiplyBlue = all;
        return this;
    }

    public CustomNode SetAllColorData(ColorSet c) => SetRGBA(c.Base).SetAddRGB(c.Add).SetMultiply(c.Multiply);

    public CustomNode SetNodeFlags(NodeFlags nodeFlags)
    {
        Node->NodeFlags |= nodeFlags;
        return this;
    }

    public CustomNode UnsetNodeFlags(NodeFlags nodeFlags)
    {
        Node->NodeFlags &= ~nodeFlags;
        return this;
    }

    public CustomNode SetDrawFlags(uint drawFlags)
    {
        Node->DrawFlags |= drawFlags;
        return this;
    }

    public CustomNode SetImageFlag(byte flags)
    {
        if (Node->Type == Image) Node->GetAsAtkImageNode()->Flags = flags;
        return this;
    }

    public CustomNode SetImageWrap(byte wrap)
    {
        if (Node->Type == Image) Node->GetAsAtkImageNode()->WrapMode = wrap;
        return this;
    }

    public CustomNode SetText(string text)
    {
        if (Node != null && Node->Type == Text)
        {
            var strLen = Encoding.UTF8.GetByteCount(text); // get length of string as UTF-8 bytes

            if (TextBuffer == null || strLen + 1 > TextBufferLen)
            {
                NativeMemory.Free(TextBuffer);
                TextBuffer = (byte*)NativeMemory.Alloc((nuint)(strLen + 1)); // need one extra byte for the null terminator
                TextBufferLen = strLen + 1;
            }

            Span<byte> bufferSpan = new(TextBuffer, strLen + 1); // wrap buffer in a span so you can use GetBytes
            Encoding.UTF8.GetBytes(text, bufferSpan); // convert string to UTF-8 and store in your buffer
            bufferSpan[strLen] = 0; // add null terminator to the end of your string
            ((AtkTextNode*)Node)->SetText(TextBuffer);
        }
        return this;
    }

    public CustomNode SetTextColor(ColorRGB color, ColorRGB edgeColor)
    {
        if (Node->Type == Text)
        {
            var textNode = (AtkTextNode*)Node;
            textNode->TextColor = color;
            textNode->EdgeColor = edgeColor;
        }

        return this;
    }

    public CustomNode SetTextSize(byte size)
    {
        if (Node->Type == Text)
        {
            var textNode = (AtkTextNode*)Node;
            textNode->FontSize = (byte)Clamp((int)size, 1, 200);
        }

        return this;
    }

    public CustomNode SetTextNumber(int number)
    {
        if (Node != null && Node->Type == Text) ((AtkTextNode*)Node)->SetNumber(number);
        return this;
    }

    public CustomNode SetTextFont(FontType font)
    {
        if (Node != null && Node->Type == Text) ((AtkTextNode*)Node)->SetFont(font);
        return this;
    }

    public CustomNode SetTextAlign(AlignmentType align)
    {
        if (Node != null && Node->Type == Text) ((AtkTextNode*)Node)->SetAlignment(align);
        return this;
    }

    public Vector2 GetTextDrawSize()
    {
        if (Node->Type == Text)
        {
            ushort w = 0;
            ushort h = 0;
            Node->GetAsAtkTextNode()->GetTextDrawSize(&w, &h);
            return new(w, h);
        }

        return new(Node->Width, Node->Height);
    }

    public CustomNode SetNineGridOffset(Vector4 coords)
    {
        if (Node->Type != NineGrid) return this;
        Node->GetAsAtkNineGridNode()->TopOffset = (short)coords.X;
        Node->GetAsAtkNineGridNode()->RightOffset = (short)coords.Y;
        Node->GetAsAtkNineGridNode()->BottomOffset = (short)coords.Z;
        Node->GetAsAtkNineGridNode()->LeftOffset = (short)coords.W;
        return this;
    }

    public CustomNode SetNineGridOffset(int x, int y, int z, int w) => SetNineGridOffset(new(x, y, z, w));

    public CustomNode SetNineGridBlend(uint b)
    {
        if (Node->Type == NineGrid) Node->GetAsAtkNineGridNode()->BlendMode = b;
        return this;
    }

    public CustomNode SetPartsList(AtkUldPartsList* partsList)
    {
        if (Node->Type == Image) Node->GetAsAtkImageNode()->PartsList = partsList;
        else if (Node->Type == NineGrid) Node->GetAsAtkNineGridNode()->PartsList = partsList;

        return this;
    }

    public CustomNode SetPartId(uint id) => SetPartId((ushort)id);

    public CustomNode SetPartId(int id) => SetPartId((ushort)id);

    public CustomNode SetPartId(ushort id)
    {
        if (Node->Type == Image) Node->GetAsAtkImageNode()->PartId = id;
        if (Node->Type == NineGrid) Node->GetAsAtkNineGridNode()->PartId = id;
        return this;
    }

    public CustomNode SetPartCoords(Vector4 coords)
    {
        if (Node->Type == Image)
        {
            var imageNode = Node->GetAsAtkImageNode();
            SetCoords(imageNode->PartsList, imageNode->PartId, coords);
        }
        else if (Node->Type == NineGrid)
        {
            var nineGridNode = Node->GetAsAtkNineGridNode();
            SetCoords(nineGridNode->PartsList, nineGridNode->PartId, coords);
        }

        static void SetCoords(AtkUldPartsList* list, uint partId, Vector4 coords)
        {
            list->Parts[partId].U = (ushort)Floor(coords.X);
            list->Parts[partId].V = (ushort)Floor(coords.Y);
            list->Parts[partId].Width = (ushort)Floor(coords.Z);
            list->Parts[partId].Height = (ushort)Floor(coords.W);
        }

        return this;
    }

    public CustomNode SetKeyFrameAddRGB(AddRGB add, int a, int kg, int kf)
    {
        if (Node != null && Node->Timeline != null)
        {
            var resource = Node->Timeline->Resource;
            if (resource != null && resource->AnimationCount > a)
            {
                var animation = resource->Animations[a];
                if (animation.KeyGroups.Length > kg)
                {
                    var keyGroup = animation.KeyGroups[kg];
                    if (keyGroup.KeyFrameCount > kf) keyGroup.KeyFrames[kf].Value.NodeTint.AddRGBBitfield = add.ToBitField();
                }
            }
        }

        return this;
    }

    public CustomNode SetKeyFrameAddRGB(AddRGB add, params (int a, int kg, int kf)[] keyFrameTuples)
    {
        foreach (var tup in keyFrameTuples) SetKeyFrameAddRGB(add, tup.a, tup.kg, tup.kf);
        return this;
    }

    public CustomNode Warning(string str)
    {
        GaugeOMatic.GaugeOMatic.Service.Log.Warning($"{str}\n{new StackTrace()}");
        return this;
    }
}
