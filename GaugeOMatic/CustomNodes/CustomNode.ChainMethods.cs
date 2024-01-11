using FFXIVClientStructs.FFXIV.Component.GUI;
using GaugeOMatic.Widgets;
using System;
using System.Numerics;
using static GaugeOMatic.Utility.Color;

// ReSharper disable UnusedMember.Global

namespace CustomNodes;

public unsafe partial class CustomNodeManager
{
    public partial struct CustomNode
    {
        public readonly bool Visible
        {
            get => Node->IsVisible;
            set => Node->ToggleVisibility(value);
        }

        public readonly float X
        {
            get => Node->X;
            set => SetX(value);
        }

        public readonly float Y
        {
            get => Node->Y;
            set => SetY(value);
        }

        public readonly Vector2 Position
        {
            get => new(Node->X, Node->Y);
            set => SetPos(value);
        }

        public readonly ushort Width
        {
            get => Node->Width;
            set => SetWidth(value);
        }

        public readonly ushort Height
        {
            get => Node->Height;
            set => SetHeight(value);
        }

        public readonly Vector2 Size
        {
            get => new(Node->Width, Node->Height);
            set => SetSize(value);
        }

        public readonly float ScaleX
        {
            get => Node->ScaleX;
            set => SetScaleX(value);
        }

        public readonly float ScaleY
        {
            get => Node->ScaleY;
            set => SetScaleY(value);
        }

        public readonly Vector2 Scale
        {
            get => new(Node->ScaleX, Node->ScaleY);
            set => SetScale(value); 
        }

        public readonly Vector2 Origin
        {
            get => new(Node->OriginX, Node->OriginY);
            set => SetOrigin(value);
        }

        public readonly float Rotation
        {
            get => Node->Rotation;
            set => SetRotation(value);
        }

        public readonly ColorRGB Color
        {
            get => Node->Color;
            set => SetRGBA(value);
        }

        public readonly byte Alpha
        {
            get => Node->Color.A;
            set => SetAlpha(value);
        }

        public readonly AddRGB Add
        {
            get => new(Node->AddRed, Node->AddGreen, Node->AddBlue);
            set => SetAddRGB(value);
        }

        public readonly ColorRGB Multiply
        {
            get => new(Node->MultiplyRed, Node->MultiplyGreen, Node->MultiplyBlue);
            set => SetMultiply(value);
        }

        public readonly CustomNode SetVis(bool show)
        {
            Node->ToggleVisibility(show);
            return this;
        }

        public readonly CustomNode Hide() => SetVis(false);
        public readonly CustomNode Show() => SetVis(true);

        public readonly CustomNode SetPos(float x, float y)
        {
            Node->SetPositionFloat(x, y);
            return this;
        }

        public readonly CustomNode SetPos(Vector2 pos) => SetPos(pos.X, pos.Y);

        public readonly CustomNode SetPos(Vector2? pos) => SetPos(pos ?? new Vector2(0));

        public readonly CustomNode SetX(float x)
        {
            Node->SetPositionFloat(x, Node->Y);
            return this;
        }

        public readonly CustomNode SetY(float y)
        {
            Node->SetPositionFloat(Node->X, y);
            return this;
        }

        public readonly CustomNode SetWidth(float w) => SetWidth((ushort)w);

        public readonly CustomNode SetWidth(int w) => SetWidth((ushort)w);

        public readonly CustomNode SetWidth(ushort width)
        {
            if (Node != null) Node->Width = width;
            return this;
        }

        public readonly CustomNode SetHeight(float h) => SetHeight((ushort)h);

        public readonly CustomNode SetHeight(int h) => SetHeight((ushort)h);

        public readonly CustomNode SetHeight(ushort height)
        {
            if (Node != null) Node->Height = height;
            return this;
        }

        public readonly CustomNode SetSize(float w, float h) => SetWidth(w).SetHeight(h);

        public readonly CustomNode SetSize(int w, int h) => SetWidth(w).SetHeight(h);

        public readonly CustomNode SetSize(ushort w, ushort h) => SetWidth(w).SetHeight(h);

        public readonly CustomNode SetSize(Vector2 s) => SetSize((ushort)s.X, (ushort)s.Y);

        public readonly CustomNode SetScale(float s)
        {
            Node->SetScale(s, s);
            Node->DrawFlags |= 0xD;
            return this;
        }

        public readonly CustomNode SetScale(float x, float y) => SetScaleX(x).SetScaleY(y);

        public readonly CustomNode SetScale(Vector2 scale) => SetScale(scale.X, scale.Y);

        public readonly CustomNode SetScale(Vector2? scale) => SetScale(scale ?? new Vector2(1));

        public readonly CustomNode SetScaleX(float x)
        {
            Node->ScaleX = x;
            Node->DrawFlags |= 0xD;
            return this;
        }

        public readonly CustomNode SetScaleY(float y)
        {
            Node->ScaleY = y;
            Node->DrawFlags |= 0xD;
            return this;
        }

        public readonly CustomNode SetOrigin(Vector2 origin)
        {
            Node->OriginX = origin.X;
            Node->OriginY = origin.Y;
            Node->DrawFlags |= 0xD;
            return this;
        }

        public readonly CustomNode SetOrigin(float x, float y) => SetOrigin(new(x, y));

        public readonly CustomNode SetRotation(float angle, bool deg = false)
        {
            if (deg) angle *= 0.01745329f;

            Node->Rotation = angle;
            Node->DrawFlags |= 0xD;
            return this;
        }

        public readonly CustomNode SetRotation(double angle, bool deg = false) => SetRotation((float)angle, deg);

        public readonly CustomNode SetRotation(int angle) => SetRotation(angle, true);

        public CustomNode SetRGB(ColorRGB c) => SetRGB((Vector4)c);
        public CustomNode SetRGB(Vector4 vec4) => SetRGB(new Vector3(vec4.X, vec4.Y, vec4.Z));
        public CustomNode SetRGB(Vector3 vec3)
        {
            Node->Color.R = (byte)(vec3.X * 255);
            Node->Color.G = (byte)(vec3.Y * 255);
            Node->Color.B = (byte)(vec3.Z * 255);
            return this;
        }

        public readonly CustomNode SetRGBA(Vector4 vec4)
        {
            Node->Color.R = (byte)(vec4.X * 255);
            Node->Color.G = (byte)(vec4.Y * 255);
            Node->Color.B = (byte)(vec4.Z * 255);
            Node->Color.A = (byte)(vec4.W * 255);
            return this;
        }

        public readonly CustomNode SetRGBA(byte r, byte g, byte b, byte? a = null)
        {
            Node->Color.R = r;
            Node->Color.G = g;
            Node->Color.B = b;
            if (a != null) { Node->Color.A = a.Value; }
            return this;
        }

        public readonly CustomNode SetAlpha(byte a)
        {
            Node->Color.A = a;
            return this;
        }

        public readonly CustomNode SetAlpha(float a)
        {
            Node->Color.A = (byte)(a > 1?a:a*255);
            return this;
        }

        public readonly CustomNode SetAddRGB(AddRGB add, bool alpha = false)
        {
            Node->AddRed = add.R;
            Node->AddGreen = add.G;
            Node->AddBlue = add.B;
            if (alpha) { Node->Color.A = add.A; }
            return this;
        }

        public readonly CustomNode SetAddRGB(short r, short g, short b, byte? a = null)
        {
            Node->AddRed = r;
            Node->AddGreen = g;
            Node->AddBlue = b;
            if (a != null) { Node->Color.A = a.Value; }
            return this;
        }

        public readonly CustomNode SetAddRGB(short all)
        {
            Node->AddRed = all;
            Node->AddGreen = all;
            Node->AddBlue = all;
            return this;
        }

        public readonly CustomNode SetMultiply(Vector3 m)
        {
            Node->MultiplyRed = (byte)(m.X * 255f);
            Node->MultiplyGreen = (byte)(m.Y * 255f);
            Node->MultiplyBlue = (byte)(m.Z * 255f);
            return this;
        }

        public readonly CustomNode SetMultiply(byte r, byte g, byte b)
        {
            Node->MultiplyRed = r;
            Node->MultiplyGreen = g;
            Node->MultiplyBlue = b;
            return this;
        }

        public readonly CustomNode SetMultiply(byte all)
        {
            Node->MultiplyRed = all;
            Node->MultiplyGreen = all;
            Node->MultiplyBlue = all;
            return this;
        }

        public readonly CustomNode SetAllColorData(ColorSet c) => SetRGBA(c.Base).SetAddRGB(c.Add).SetMultiply(c.Multiply);

        public readonly CustomNode SetNodeFlags(NodeFlags nodeFlags)
        {
            Node->NodeFlags |= nodeFlags;
            return this;
        }

        public readonly CustomNode UnsetNodeFlags(NodeFlags nodeFlags)
        {
            Node->NodeFlags &= ~nodeFlags;
            return this;
        }

        public readonly CustomNode SetDrawFlags(uint drawFlags)
        {
            Node->DrawFlags |= drawFlags;
            return this;
        }

        public readonly CustomNode SetImageFlag(byte flags)
        {
            if (Node->Type == NodeType.Image) Node->GetAsAtkImageNode()->Flags = flags;
            return this;
        }

        public readonly CustomNode SetImageWrap(byte wrap)
        {
            if (Node->Type == NodeType.Image) Node->GetAsAtkImageNode()->WrapMode = wrap;
            return this;
        }

        public readonly CustomNode SetPartId(ushort id)
        {
            if (Node->Type == NodeType.Image) Node->GetAsAtkImageNode()->PartId = id;
            return this;
        }

        public readonly CustomNode SetText(string text)
        {
            if (Node != null && Node->Type == NodeType.Text) ((AtkTextNode*)Node)->SetText(text);
            return this;
        }

        public readonly CustomNode SetTextColor(ColorRGB color, ColorRGB edgeColor)
        {
            if (Node->Type == NodeType.Text)
            {
                var textNode = (AtkTextNode*)Node;
                textNode->TextColor = color;
                textNode->EdgeColor = edgeColor;
            }

            return this;
        }

        public readonly CustomNode SetTextSize(byte size)
        {
            if (Node->Type == NodeType.Text)
            {
                var textNode = (AtkTextNode*)Node;
                textNode->FontSize = (byte)Math.Clamp((int)size, 1, 200);
            }

            return this;
        }

        public readonly CustomNode SetTextNumber(int number)
        {
            if (Node != null && Node->Type == NodeType.Text) ((AtkTextNode*)Node)->SetNumber(number);
            return this;
        }

        public readonly CustomNode SetTextFont(FontType font)
        {
            if (Node != null && Node->Type == NodeType.Text) ((AtkTextNode*)Node)->SetFont(font);
            return this;
        }

        public readonly CustomNode SetTextAlign(AlignmentType align)
        {
            if (Node != null && Node->Type == NodeType.Text) ((AtkTextNode*)Node)->SetAlignment(align);
            return this;
        }

        public readonly CustomNode UpdateNumText(NumTextProps props, float current, float max)
        {
            var text = props.ShowZero ? NumTextProps.PrecisionString(0) : " ";
            if (props.Enabled && current != 0)
            {
                if (props.Precision == 0) { current = (float)Math.Ceiling(current); }
                var numText = ((double)(props.Invert ? max - current : current)).ToString(NumTextProps.PrecisionString(props.Precision));
                if (numText != "0") text = numText;
            }

            return SetText(text);
        }

        public readonly CustomNode SetNineGridOffset(Vector4 coords)
        {
            if (Node->Type != NodeType.NineGrid) return this;
            Node->GetAsAtkNineGridNode()->TopOffset = (short)coords.X;
            Node->GetAsAtkNineGridNode()->RightOffset = (short)coords.Y;
            Node->GetAsAtkNineGridNode()->BottomOffset = (short)coords.Z;
            Node->GetAsAtkNineGridNode()->LeftOffset = (short)coords.W;
            return this;
        }

        public readonly CustomNode SetNineGridOffset(int x, int y, int z, int w) => SetNineGridOffset(new(x, y, z, w));

        public readonly CustomNode SetNineGridBlend(uint b)
        {
            if (Node->Type == NodeType.NineGrid) Node->GetAsAtkNineGridNode()->BlendMode = b;
            return this;
        }
    }
}
