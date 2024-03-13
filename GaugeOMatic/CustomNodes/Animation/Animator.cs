using CustomNodes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using static GaugeOMatic.CustomNodes.Animation.Tween.EaseType;

// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedMethodReturnValue.Global

namespace GaugeOMatic.CustomNodes.Animation;

public class Animator : IDisposable, IEnumerable<Tween>
{
    public List<Tween> Tweens = new();

    public void RunTweens() => Tweens = new(Tweens.Where(static t => !t.IsStale)
                                                  .Select(static t => t.Update()));

    public Animator Add(Tween t)
    {
        Tweens.Add(t);
        return this;
    }

    public Animator Add(IEnumerable<Tween> t)
    {
        Tweens = Tweens.Concat(t).ToList();
        return this;
    }

    public Animator Add(params Tween[] t)
    {
        Tweens = Tweens.Concat(t).ToList();
        return this;
    }

    public static Animator operator +(Animator a, Tween t) => a.Add(t);
    public static Animator operator +(Animator a, IEnumerable<Tween> t) => a.Add(t);

    public Animator Clear()
    {
        Tweens.Clear();
        return this;
    }

    public Animator Remove(Tween t)
    {
        Tweens.Remove(t);
        return this;
    }

    public Animator Remove(CustomNode node)
    {
        Tweens = new(Tweens.Where(t => t.Target != node));
        return this;
    }

    public Animator Remove(string label)
    {
        Tweens = new(Tweens.Where(t => t.Label != label));
        return this;
    }

    public Animator Remove(string label, params CustomNode[] nodes)
    {
        Tweens = new(Tweens.Where(t => t.Label != label || !nodes.Contains(t.Target)));
        return this;
    }

    public static Animator operator -(Animator tm, Tween t) => tm.Remove(t);
    public static Animator operator -(Animator tm, string label) => tm.Remove(label);
    public static Animator operator -(Animator tm, CustomNode n) => tm.Remove(n);

    public Animator PlayNodeTimeline(CustomNode node, float time, bool repeat = false, bool sin = false, float start = 0, float end = 1) => Add(new Tween(node, new(0) { TimelineProg = start }, new(time) { TimelineProg = end }) { Ease = sin ? SinInOut : Linear, Repeat = repeat });
    public Animator TweenTo(CustomNode customNode, KeyFrame keyFrame) => Add(new Tween(customNode, customNode, keyFrame));

    public void Dispose() => Clear();

    public IEnumerator<Tween> GetEnumerator() => Tweens.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
