using System.Collections.Generic;
using GaugeOMatic.CustomNodes.Animation;
using System.Linq;

// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedMethodReturnValue.Global

namespace CustomNodes;

public partial class CustomNode
{
    public Tween? Timeline;

    public float Progress
    {
        get => Prog;
        set
        {
            Prog = value;
            Timeline?.Update(value);
        }
    }

    internal float Prog;

    public CustomNode DefineTimeline(params KeyFrame[] keyFrames)
    {
        Timeline = new(this, keyFrames.Select(static k => k with { TimelineProg = null }).ToArray());
        return this;
    }

    public CustomNode DefineTimeline(Tween tween)
    {
        tween.Target = this;
        Timeline = tween;
        return this;
    }

    public CustomNode SetProgress(float f)
    {
        Progress = f;
        return this;
    }

    public CustomNode SetProgress(CustomNode n)
    {
        Progress = n.Progress;
        return this;
    }

    public static implicit operator Tween(CustomNode n) => new(n, new(0) { TimelineProg = 0 }, new(n.Timeline?.Length ?? 1) { TimelineProg = 1 });

    public IEnumerable<CustomNode> GetDescendants()
    {
        var descendants = Children.ToList();

        foreach (var node in descendants) descendants = descendants.Concat(node.GetDescendants()).ToList();

        return descendants.Distinct().ToList();
    }
}
