using CustomNodes;
using FFXIVClientStructs.FFXIV.Component.GUI;
using System.Numerics;
using static GaugeOMatic.Utility.Color;
using static System.Math;

namespace GaugeOMatic.CustomNodes.Animation;

public unsafe struct KeyFrame
{
    public float Time { get; set; } = 0;

    public float? X { get; set; } = null;
    public float? Y { get; set; } = null;
    public float? Width { get; set; } = null;
    public float? Height { get; set; } = null;
    public float? ScaleX { get; set; } = null;
    public float? ScaleY { get; set; } = null;
    public float? Scale { get; set; } = null;
    public float? Alpha { get; set; } = null;
    public float? Rotation { get; set; } = null;
    public ColorRGB? RGB { get; set; } = null;
    public AddRGB? AddRGB { get; set; } = null;
    public ColorRGB? MultRGB { get; set; } = null;
    public ushort? PartId { get; set; } = null;
    public Vector4? PartCoords { get; set; } = null;
    public float? TimelineProg { get; set; } = null;

    internal static KeyFrame Visible = new() { Alpha = 255 };
    internal static KeyFrame Hidden = new() { Alpha = 0 };

    public KeyFrame(float time = 0) => Time = time;

    public readonly KeyFrame this[int i] => this with { Time = i };

    public KeyFrame(float time, CustomNode n)
    {
        if (n.Node == null)
        {
            Time = 0;
            return;
        }

        Time = time;
        X = n.X;
        Y = n.Y;
        Width = n.Width;
        Height = n.Height;
        ScaleX = n.ScaleX;
        ScaleY = n.ScaleY;
        Alpha = n.Alpha;
        Rotation = n.Rotation;
        RGB = n.Color;
        AddRGB = n.Add;
        MultRGB = n.Multiply;
        TimelineProg = n.Progress;

        if (n.Node->Type == NodeType.Image)
        {
            var imageNode = n.Node->GetAsAtkImageNode();
            var part = imageNode->PartsList->Parts[imageNode->PartId];
            PartCoords = new(part.U, part.V, part.Width, part.Height);
        }
        else if (n.Node->Type == NodeType.NineGrid)
        {
            var nineGridNode = n.Node->GetAsAtkNineGridNode();
            var part = nineGridNode->PartsList->Parts[nineGridNode->PartId];
            PartCoords = new(part.U, part.V, part.Width, part.Height);
        }
        else PartCoords = null;
    }

    public static implicit operator KeyFrame(CustomNode n) => new(0, n);

    public static KeyFrame Interpolate(KeyFrame start, KeyFrame end, float subProg) =>
        new()
        {
            X = Interpolate(start.X, end.X, subProg),
            Y = Interpolate(start.Y, end.Y, subProg),
            Width = Interpolate(start.Width, end.Width, subProg),
            Height = Interpolate(start.Height, end.Height, subProg),
            ScaleX = Interpolate(start.ScaleX ?? start.Scale, end.ScaleX ?? end.Scale, subProg),
            ScaleY = Interpolate(start.ScaleY ?? start.Scale, end.ScaleY ?? end.Scale, subProg),
            Alpha = Interpolate(start.Alpha, end.Alpha, subProg),
            Rotation = Interpolate(start.Rotation, end.Rotation, subProg),
            RGB = Interpolate(start.RGB, end.RGB, subProg),
            AddRGB = Interpolate(start.AddRGB, end.AddRGB, subProg),
            MultRGB = Interpolate(start.MultRGB, end.MultRGB, subProg),
            PartId = Interpolate(start.PartId, end.PartId, subProg),
            PartCoords = Interpolate(start.PartCoords, end.PartCoords, subProg),
            TimelineProg = Interpolate(start.TimelineProg, end.TimelineProg, subProg)
        };

    private static Vector4? Interpolate(Vector4? start, Vector4? end, float progress) =>
        !start.HasValue || !end.HasValue ? null :
            Vector4.Lerp(start.Value, end.Value, progress);

    public static float? Interpolate(float? start, float? end, float progress) =>
        start == null || end == null ? null :
            (progress * (end - start)) + start;

    public static ushort? Interpolate(ushort? start, ushort? end, float progress) =>
        start == null || end == null ? null :
            end.Value == start.Value ? start.Value :
                end.Value > start.Value ?
                    Clamp((ushort)Floor(start.Value + (progress * (end.Value - start.Value + 1))), start.Value, end.Value) :
                    Clamp((ushort)Ceiling(start.Value + (progress * (end.Value - start.Value - 1))), end.Value, start.Value);

    public static ColorRGB? Interpolate(ColorRGB? start, ColorRGB? end, float progress) =>
        !start.HasValue || !end.HasValue ? null :
            Vector4.Lerp((Vector4)start, (Vector4)end, progress);

    public static AddRGB? Interpolate(AddRGB? start, AddRGB? end, float progress) =>
        !start.HasValue || !end.HasValue ? null :
            Vector4.Lerp((Vector4)start, (Vector4)end, progress);

    public readonly void ApplyToNode(CustomNode target)
    {
        if (Alpha.HasValue) target.SetAlpha((byte)Alpha);
        if (X.HasValue || Y.HasValue) target.SetPos(X ?? target.X, Y ?? target.Y);
        if (Width.HasValue) target.SetWidth((ushort)Max(0f, (float)Width));
        if (Height.HasValue) target.SetHeight((ushort)Max(0f, (float)Height));
        if (ScaleX.HasValue || ScaleY.HasValue) target.SetScale(ScaleX ?? target.ScaleX, ScaleY ?? target.ScaleY).SetDrawFlags(0xD);
        if (Rotation.HasValue) target.SetRotation((float)Rotation).SetDrawFlags(0xD);
        if (RGB.HasValue) target.SetRGB(RGB.Value);
        if (AddRGB.HasValue) target.SetAddRGB(AddRGB.Value);
        if (MultRGB.HasValue) target.SetMultiply(MultRGB.Value);
        if (PartId.HasValue) target.SetPartId(PartId.Value);
        if (PartCoords.HasValue) target.SetPartCoords(PartCoords.Value);

        if (TimelineProg.HasValue) target.SetProgress(TimelineProg.Value);
    }
}
