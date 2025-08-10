using Dalamud.Interface.Utility;
using Dalamud.Bindings.ImGui;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using static System.Math;

namespace CustomNodes;

public unsafe partial class CustomNode
{
    // ReSharper disable once UnusedMember.Global
    public void DrawBounds(uint col = 0xffffffff, int thickness = 1) => new Bounds(this).Draw(col, thickness);

    public Vector2 GetAbsoluteScale()
    {
        var scale = Scale;
        var parent = Node->ParentNode;
        while (parent != null)
        {
            scale *= new Vector2(parent->ScaleX, parent->ScaleY);
            parent = parent->ParentNode;
        }

        return scale;
    }

    public partial struct Bounds
    {
        public bool IsConvex { get; set; } = false;

        public List<Vector2> Points { get; set; } = [];

        public readonly void Draw(uint col = 0xffffffff, int thickness = 1)
        {
            if (Points == null || Points.Count == 0) return;

            var path = new ImVectorWrapper<Vector2>(Points.Count);

            foreach (var p in Points) path.Add(p);

            ImGui.GetBackgroundDrawList().AddPolyline(ref path[0], path.Length, col, ImDrawFlags.Closed, thickness);

            path.Dispose();
        }

        public static implicit operator Bounds(CustomNode node) => new(node);

        public static implicit operator Bounds(List<CustomNode> nodes) => GetConvexHull(nodes);

        public static Bounds operator +(Bounds b, Vector2 v) => new(b.Points.Select(p => p + v)) { IsConvex = b.IsConvex };

        public static Bounds operator -(Bounds b, Vector2 v) => new(b.Points.Select(p => p - v)) { IsConvex = b.IsConvex };

        public Bounds(params Vector2[] points) : this(points.ToList()) { }

        public Bounds(IEnumerable<Vector2> points) => Points = points.Distinct().ToList();

        public Bounds(CustomNode node)
        {
            if (node.Node == null) return;

            Points =
            [
                new(0),
                node.Size with { Y = 0 },
                node.Size,
                node.Size with { X = 0 }
            ];

            var transformNode = node.Node;
            while (transformNode != null)
            {
                var currentNode = (CustomNode)transformNode;

                var offset = currentNode.Position;
                var origin = offset + currentNode.Origin;
                var rotation = currentNode.Rotation;
                var scale = currentNode.Scale;

                Points = Points.Select(b => TransformPoint(b + offset, origin, rotation, scale)).ToList();

                transformNode = transformNode->ParentNode;
            }

            IsConvex = true;
        }

        public Bounds(params CustomNode[] nodes) : this(nodes.ToList()) { }

        public Bounds(IEnumerable<CustomNode> nodes) => this = GetConvexHull(nodes.ToList());

        public static Vector2 TransformPoint(Vector2 p, Vector2 o, float r, Vector2 s)
        {
            var cosR = (float)Cos(r);
            var sinR = (float)Sin(r);
            var d = (p - o) * s;

            return new(o.X + (d.X * cosR) - (d.Y * sinR),
                       o.Y + (d.X * sinR) + (d.Y * cosR));
        }

        #region Hulls

        #region MaxBox

        public readonly Bounds GetMaxBox()
        {
            float? x1 = null, x2 = null, y1 = null, y2 = null;

            foreach (var p in Points)
            {
                x1 = Min(p.X, x1 ?? p.X);
                x2 = Max(p.X, x2 ?? p.X);
                y1 = Min(p.Y, y1 ?? p.Y);
                y2 = Max(p.Y, y2 ?? p.Y);
            }

            return new(new Vector2(x1 ?? 0, y1 ?? 0),
                       new Vector2(x2 ?? 0, y1 ?? 0),
                       new Vector2(x2 ?? 0, y2 ?? 0),
                       new Vector2(x1 ?? 0, y2 ?? 0))
            { IsConvex = true };
        }

        #endregion

        #region Convex

        public readonly Bounds GetConvexHull() => GetConvexHull(Points);

        public static Bounds GetConvexHull(List<CustomNode> nodes)
        {
            if (nodes.Count == 0) { return new(); }

            var points = new List<Vector2>();
            foreach (var node in nodes) points.AddRange(((Bounds)node).Points);

            return GetConvexHull(points);
        }

        public static Bounds GetConvexHull(List<Vector2> points)
        {
            if (points.Count <= 1) return new(points);

            int n = points.Count, k = 0;
            var h = new List<Vector2>(new Vector2[2 * n]);

            points.Sort(static (a, b) => a.X.Equals(b.X) ? a.Y.CompareTo(b.Y) : a.X.CompareTo(b.X));

            // Build lower hull
            for (var i = 0; i < n; ++i)
            {
                while (k >= 2 && Cross(h[k - 2], h[k - 1], points[i]) <= 0) k--;
                h[k++] = points[i];
            }

            // Build upper hull
            for (int i = n - 2, t = k + 1; i >= 0; i--)
            {
                while (k >= t && Cross(h[k - 2], h[k - 1], points[i]) <= 0) k--;
                h[k++] = points[i];
            }

            return new(h.Take(k - 1).ToList()) { IsConvex = true };

            static float Cross(Vector2 o, Vector2 a, Vector2 b) => ((a.X - o.X) * (b.Y - o.Y)) - ((a.Y - o.Y) * (b.X - o.X));
        }

        #endregion

        #endregion

        public readonly bool ContainsPoint(Vector2 p)
        {
            var hull = IsConvex ? this : GetConvexHull();

            var count = hull.Points.Count;
            var inside = false;

            for (var i = 0; i < count; i++)
            {
                var p1 = hull.Points[i];
                var p2 = hull.Points[(i + 1) % count];


                if (p.Y > Min(p1.Y, p2.Y) &&
                    p.Y <= Max(p1.Y, p2.Y) &&
                    p.X <= Max(p1.X, p2.X) &&
                    (p1.X.Equals(p2.X) || p.X <= ((p.Y - p1.Y) * (p2.X - p1.X) / (p2.Y - p1.Y)) + p1.X))
                    inside = !inside;
            }

            return inside;
        }

        public readonly bool ContainsCursor() => ContainsPoint(ImGui.GetMousePos());
    }
}
