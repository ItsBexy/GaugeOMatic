using FFXIVClientStructs.FFXIV.Component.GUI;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Numerics;
using static GaugeOMatic.Utility.Color;
using static GaugeOMatic.Utility.MiscMath;
using static System.Numerics.Vector4;
// ReSharper disable PropertyCanBeMadeInitOnly.Global

namespace CustomNodes;

public unsafe partial class CustomNodeManager
{
    public class Tween
    {
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        public static class Eases
        {
            public static float Linear(float p) => p;
            public static float Pow3InOut(float p) => PolyCalc(p, 0, 0.0277777777777778D, 2.91666666666667D, -1.94444444444444D);
            public static float Overshoot(float p) => PolyCalc(p, 0, 0.76686507936507d, 2.96130952380954d, -2.72817460317462d);
            public static float SinInOut(float p) => (float)((-0.5f * Math.Cos(p * Math.PI)) + 0.5f); // it's actually a cos function shhh
        }

        public DateTime StartTime;
        public AtkResNode* Target;
        public bool Repeat { get; set; }
        public List<KeyFrame> KeyFrames { get; set; }
        public int Length;
        public bool IsStale;
        public string Label = string.Empty;
        public Action? Complete { get; set; }
        public Action? PerCycle { get; set; }

        public Func<float, float> Ease { get; set; } = Eases.Linear;

        public Tween(AtkResNode* target, params KeyFrame[] keyFrames)
        {
            StartTime = DateTime.Now;
            Target = target;
            IsStale = false;

            KeyFrames = new(keyFrames.OrderBy(static k => k.Time));
            Length = 0;
            foreach (var k in KeyFrames) Length = Math.Max(Length, k.Time);
        }

        public float CalculateProg(double timePassed, out KeyFrame start, out KeyFrame end)
        {
            for (var i = 0; i < KeyFrames.Count - 1; i++)
            {
                start = KeyFrames[i];
                end = KeyFrames[i + 1];
                if (timePassed < start.Time || timePassed > end.Time) continue;

                return end.Time != start.Time ? (float)Math.Clamp((timePassed - start.Time) / (end.Time - start.Time), 0f, 1f) : 1;
            }

            start = KeyFrames[^1];
            end = KeyFrames[^1];
            return 1f;
        }

        public struct KeyFrame
        {
            public KeyFrame(int time) => Time = time;

            public KeyFrame(int time, CustomNode n)
            {
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

                if (n.Node->Type == NodeType.Image)
                {
                    var imageNode = n.Node->GetAsAtkImageNode();
                    var part = imageNode->PartsList->Parts[imageNode->PartId];
                    PartCoords = new(part.U,part.V,part.Width,part.Height);
                } else if (n.Node->Type == NodeType.NineGrid)
                {
                    var nineGridNode = n.Node->GetAsAtkNineGridNode();
                    var part = nineGridNode->PartsList->Parts[nineGridNode->PartID];
                    PartCoords = new(part.U, part.V, part.Width, part.Height);
                }
                else
                {
                    PartCoords = null;
                }
            }

            public int Time { get; set; } = 0;

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
        }

        private static KeyFrame Interpolate(KeyFrame start, KeyFrame end, float subProg) =>
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
                PartCoords = Interpolate(start.PartCoords,end.PartCoords,subProg)
            };

        private static Vector4? Interpolate(Vector4? start, Vector4? end, float subProg) => start.HasValue && end.HasValue ? Lerp(start.Value, end.Value, subProg) : null;

        public static float? Interpolate(float? start, float? end, float progress)
        {
            return start == null || end == null ? null : (progress * (end - start)) + start;
        }

        public static ushort? Interpolate(ushort? start, ushort? end, float progress)
        {
            if (start == null || end == null) return null;

            return end.Value == start.Value ? start.Value :
                   end.Value > start.Value ? Math.Clamp((ushort)Math.Floor(start.Value + (progress * (end.Value - start.Value + 1))), start.Value, end.Value) :
                                             Math.Clamp((ushort)Math.Ceiling(start.Value + (progress * (end.Value - start.Value - 1))), end.Value, start.Value);
        }

        public static ColorRGB? Interpolate(ColorRGB? start, ColorRGB? end, float progress) => start.HasValue && end.HasValue ? Lerp((Vector4)start, (Vector4)end, progress) : null;

        public static AddRGB? Interpolate(AddRGB? start, AddRGB? end, float progress) => start.HasValue && end.HasValue ? Lerp((Vector4)start, (Vector4)end, progress) : null;

        public Tween Run()
        {
            if (Target == null) IsStale = true;
            if (IsStale) return this;

            var timePassed = (DateTime.Now - StartTime).TotalMilliseconds;

            if (Repeat && timePassed > Length)
            {
                PerCycle?.Invoke();
                StartTime = StartTime.AddMilliseconds(Length);
                timePassed %= Length;
            }

            if (Target->IsVisible)
            {
                var subProg = CalculateProg(timePassed, out var start, out var end);

                var props = subProg switch
                {
                    0f => start,
                    1f => end,
                    _ => Interpolate(start, end, Ease(subProg))
                };

                ApplyFrameProps(props);
            }

            if (timePassed > Length && !Repeat)
            {
                IsStale = true;
                Complete?.Invoke();
            }

            return this;
        }
        
        private void ApplyFrameProps(KeyFrame props) {

            if (props.Alpha != null) Target->Color.A = (byte)props.Alpha;

            if (props.X != null || props.Y != null) Target->SetPositionFloat(props.X ?? Target->X, props.Y ?? Target->Y);

            if (props.Width != null) Target->SetWidth((ushort)Math.Max(0f,(float)props.Width));

            if (props.Height != null) Target->SetHeight((ushort)Math.Max(0f, (float)props.Height));

            if (props.ScaleX != null || props.ScaleY != null)
            {
                Target->SetScale(props.ScaleX ?? Target->ScaleX, props.ScaleY ?? Target->ScaleY);
                Target->DrawFlags |= 0xD;
            }

            if (props.Rotation != null)
            {
                Target->Rotation = (float)props.Rotation;
                Target->DrawFlags |= 0xD;
            }

            if (props.RGB.HasValue)
            {
                Target->Color.R = props.RGB.Value.R;
                Target->Color.G = props.RGB.Value.G;
                Target->Color.B = props.RGB.Value.B;
            }

            if (props.AddRGB.HasValue)
            {
                Target->AddRed = props.AddRGB.Value.R;
                Target->AddGreen = props.AddRGB.Value.G;
                Target->AddBlue = props.AddRGB.Value.B;
            }

            if (props.MultRGB.HasValue)
            {
                Target->MultiplyRed = props.MultRGB.Value.R;
                Target->MultiplyGreen = props.MultRGB.Value.G;
                Target->MultiplyBlue = props.MultRGB.Value.B;
            }

            if (props.PartId.HasValue)
            {
                if (Target->Type == NodeType.Image) Target->GetAsAtkImageNode()->PartId = props.PartId.Value;
                if (Target->Type == NodeType.NineGrid) Target->GetAsAtkNineGridNode()->PartID = props.PartId.Value;
            }

            if (props.PartCoords.HasValue)
            {
                if (Target->Type == NodeType.Image)
                {
                    var imageNode = Target->GetAsAtkImageNode();
                    var list = imageNode->PartsList;

                    list->Parts[imageNode->PartId].U = (ushort)Math.Floor(props.PartCoords.Value.X);
                    list->Parts[imageNode->PartId].V = (ushort)Math.Floor(props.PartCoords.Value.Y);
                    list->Parts[imageNode->PartId].Width = (ushort)Math.Floor(props.PartCoords.Value.Z);
                    list->Parts[imageNode->PartId].Height = (ushort)Math.Floor(props.PartCoords.Value.W);
                }
                if (Target->Type == NodeType.NineGrid)
                {
                    var nineGridNode = Target->GetAsAtkNineGridNode();
                    var list = nineGridNode->PartsList;

                    list->Parts[nineGridNode->PartID].U = (ushort)Math.Floor(props.PartCoords.Value.X);
                    list->Parts[nineGridNode->PartID].V = (ushort)Math.Floor(props.PartCoords.Value.Y);
                    list->Parts[nineGridNode->PartID].Width = (ushort)Math.Floor(props.PartCoords.Value.Z);
                    list->Parts[nineGridNode->PartID].Height = (ushort)Math.Floor(props.PartCoords.Value.W);
                }
            }
        }

        public static void ClearNodeTweens(ref List<Tween> tweens, AtkResNode* node) => tweens = new(tweens.Where(t => t.Target != node));
        public static void ClearLabelTweens(ref List<Tween> tweens, string label) => tweens = new(tweens.Where(t => t.Label != label));
    }
}
