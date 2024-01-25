using System;
using System.Numerics;
using static CustomNodes.CustomNode.CustomNodeFlags;
using static GaugeOMatic.Utility.Color;

// ReSharper disable UnusedMember.Global

namespace CustomNodes;

public unsafe partial class CustomNode
{
    [Flags]
    public enum CustomNodeFlags
    {
        SetVisByAlpha = 0x01 // toggles visibility when alpha changes to/from zero
    }

    public const CustomNodeFlags DefaultFlags = SetVisByAlpha;

    public CustomNodeFlags Flags = DefaultFlags;

    public bool Visible
    {
        get => Node->IsVisible;
        set => Node->ToggleVisibility(value);
    }

    public float X
    {
        get => Node->X;
        set => SetX(value);
    }

    public float Y
    {
        get => Node->Y;
        set => SetY(value);
    }

    public Vector2 Position
    {
        get => new(Node->X, Node->Y);
        set => SetPos(value);
    }

    public ushort Width
    {
        get => Node->Width;
        set => SetWidth(value);
    }

    public ushort Height
    {
        get => Node->Height;
        set => SetHeight(value);
    }

    public Vector2 Size
    {
        get => new(Node->Width, Node->Height);
        set => SetSize(value);
    }

    public float ScaleX
    {
        get => Node->ScaleX;
        set => SetScaleX(value);
    }

    public float ScaleY
    {
        get => Node->ScaleY;
        set => SetScaleY(value);
    }

    public Vector2 Scale
    {
        get => new(Node->ScaleX, Node->ScaleY);
        set => SetScale(value);
    }

    public Vector2 Origin
    {
        get => new(Node->OriginX, Node->OriginY);
        set => SetOrigin(value);
    }

    public float Rotation
    {
        get => Node->Rotation;
        set => SetRotation(value);
    }

    public ColorRGB Color
    {
        get => Node->Color;
        set => SetRGBA(value);
    }

    public byte Alpha
    {
        get => Node->Color.A;
        set => SetAlpha(value);
    }

    public AddRGB Add
    {
        get => new(Node->AddRed, Node->AddGreen, Node->AddBlue);
        set => SetAddRGB(value);
    }

    public ColorRGB Multiply
    {
        get => new(Node->MultiplyRed, Node->MultiplyGreen, Node->MultiplyBlue);
        set => SetMultiply(value);
    }
}
