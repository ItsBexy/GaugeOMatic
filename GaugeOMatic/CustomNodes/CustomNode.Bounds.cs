using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using static System.Math;
// ReSharper disable All

namespace CustomNodes;

public unsafe partial class CustomNode
{
    public bool TryGetBounds(out Vector2[]? bounds)
    {
        if (Node == null)
        {
            bounds = null;
            return false;
        }

        bounds = new[]
        {
            new(0),
            Size with { Y = 0 },
            Size,
            Size with { X = 0 }
        };

        var node = Node;
        while (node != null)
        {
            var currentNode = (CustomNode)node;

            var offset = currentNode.Position;
            var origin = offset + currentNode.Origin;
            var rotation = currentNode.Rotation;
            var scale = currentNode.Scale;

            bounds = bounds.Select(b => Transform(b + offset, origin, rotation, scale)).ToArray();

            node = node->ParentNode;
        }

        return true;
    }

    public static Vector2 Transform(Vector2 p, Vector2 o, float r, Vector2 s)
    {
        var cosR = (float)Cos(r);
        var sinR = (float)Sin(r);
        var d = (p - o) * s;

        return new(o.X + (d.X * cosR) - (d.Y * sinR),
                   o.Y + (d.X * sinR) + (d.Y * cosR));
    }

    public struct Line
    {
        public Line(Vector2 a, Vector2 b)
        {
            A = a;
            B = b;
        }

        public Vector2 A { get; set; }
        public Vector2 B { get; set; }
    }

    public static List<Line> GetMaxBox(List<CustomNode> nodes)
    {

        var points = new List<Vector2>();

        foreach (var node in nodes)
            if (node.TryGetBounds(out var bounds) && bounds != null)
                points = points.Concat(bounds).ToList();

        var xList = points.Select(static p => p.X).ToList();
        var minX = xList.Min();
        var maxX = xList.Max();

        var yList = points.Select(static p => p.Y).ToList();
        var minY = yList.Min();
        var maxY = yList.Max();

        var corner1 = new Vector2(minX, minY);
        var corner2 = new Vector2(maxX, minY);
        var corner3 = new Vector2(maxX, maxY);
        var corner4 = new Vector2(minX, maxY);

        return new List<Line>
        {
            new(corner1, corner2),
            new(corner2, corner3),
            new(corner3, corner4),
            new(corner4, corner1),
        };
    }

    public static List<Line> GetConcaveHull(List<CustomNode> nodes, float aFactor, float maxScale = 1f)
    {
        var points = new List<Vector2>();

        foreach (var node in nodes)
            if (node.TryGetBounds(out var bounds) && bounds != null)
                points = points.Concat(bounds).ToList();

        return GetConcaveHull(points, 100f);
    }

    public static List<Line> GetConcaveHull(IReadOnlyList<Vector2> points, float alpha)
    {
        var borderEdges = new List<Line>();
        // 0. error checking, init
        if (points.Count < 2) return borderEdges;

        var alphaSq = alpha * alpha;

        // 1. run through all pairs of points
        for (var i = 0; i < points.Count - 1; i++)
        {
            for (var j = i + 1; j < points.Count; j++)
            {
                var p1 = points[i];
                var p2 = points[j];
                if (p1 == p2)
                {
                    continue;
                }

                var dist = Vector2.Distance(p1, p2);

                if (dist > 2 * alpha) continue;

                // find two circles that contain p1 and p2; note that center1 == center2 if dist == 2*alpha
                var radiusVector = new Vector2(p1.Y - p2.Y, p2.X - p1.X) *
                                   ((float)Sqrt(alphaSq - (dist * dist / 4f)) / dist);

                var mid = (p1 + p2) / 2;
                var center1 = mid + radiusVector;
                var center2 = mid - radiusVector;

                // check if one of the circles is alpha-exposed, i.e. no other point lies in it
                var c1Empty = true;
                var c2Empty = true;
                for (var k = 0; k < points.Count && (c1Empty || c2Empty); k++)
                {
                    if (points[k] == p1 || points[k] == p2)
                    {
                        continue;
                    }

                    var d1 = center1 - points[k];
                    var d2 = center2 - points[k];

                    var d1Sq = d1 * d1;
                    var d2Sq = d2 * d2;

                    if (d1Sq.X + d1Sq.Y < alphaSq) c1Empty = false;
                    if (d2Sq.X + d2Sq.Y < alphaSq) c2Empty = false;
                }

                if (c1Empty || c2Empty) borderEdges.Add(new Line { A = p1, B = p2 });
            }
        }

        return borderEdges;
    }
}
