using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace CustomNodes;

public partial class CustomNode
{
    public struct Line : IEquatable<Line>
    {
        public Line(Vector2 a, Vector2 b) { A = a; B = b; }
        public Vector2 A { get; set; }
        public Vector2 B { get; set; }
        public readonly bool Equals(Line l2) => (A == l2.A && B == l2.B) || (A == l2.B && B == l2.A);

        public readonly bool Intersects(Line l2) => Intersects(l2, out _);

        // ReSharper disable once OutParameterValueIsAlwaysDiscarded.Global
        public readonly bool Intersects(Line l2, out Vector2 intersection)
        {
            intersection = default;
            if (l2.Equals(this)) return false;

            var m1 = Slope();
            var m2 = l2.Slope();

            if (m1 == null && m2 == null) return false;

            float iX;
            float iY;

            float? b1 = m1 != null ? A.Y - (m1.Value * A.X) : null;
            float? b2 = m2 != null ? l2.A.Y - (m2.Value * l2.A.X) : null;

            if (m1 == null) // l1 is vertical
            {
                iX = A.X;
                iY = (m2!.Value * iX) + b2!.Value;
            }
            else
            {
                if (m2 == null) iX = l2.A.X; // l2 is vertical
                else
                {
                    var mDiff = m1.Value - m2.Value;
                    if (Math.Abs(mDiff) < 0.00000001f) return false;
                    iX = (b2!.Value - b1!.Value) / mDiff;
                }
                iY = (m1.Value * iX) + b1!.Value;
            }

            var xList = new List<float> { A.X, B.X, l2.A.X, l2.B.X };
            var yList = new List<float> { A.Y, B.Y, l2.A.Y, l2.B.Y };
            if (iX < xList.Min() || iX > xList.Max()) return false;
            if (iY > yList.Max() || iY < yList.Min()) return false;

            intersection = new(iX, iY);

            return true;
        }

        public readonly float? Slope()
        {
            var dX = B.X - A.X;
            var dY = B.Y - A.Y;
            return Math.Abs(dX) > 0.00000001f ? dY / dX : null;
        }
    }
}
