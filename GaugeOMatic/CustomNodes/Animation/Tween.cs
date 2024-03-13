using CustomNodes;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using static GaugeOMatic.CustomNodes.Animation.KeyFrame;
using static GaugeOMatic.Utility.MiscMath;
using static System.DateTime;
using static System.TimeSpan;
using static GaugeOMatic.CustomNodes.Animation.Tween.EaseType;

namespace GaugeOMatic.CustomNodes.Animation;

[SuppressMessage("ReSharper", "UnassignedField.Global")]
public unsafe class Tween
{
    public enum EaseType { Linear, SinInOut, Overshoot }

    public static readonly Dictionary<EaseType, Func<float, float>> EaseFuncs = new()
    {
        { Linear,static p => p},
        { SinInOut,static p => (float)((-0.5f * Math.Cos(p * Math.PI)) + 0.5f)},
        { Overshoot,static p => PolyCalc(p, 0, 0.76686507936507d, 2.96130952380954d, -2.72817460317462d)}
    };

    public CustomNode Target;
    public DateTime StartTime;
    public List<KeyFrame> KeyFrames { get; set; }
    public float Length;
    public bool IsStale;
    public int Delay;

    public bool Repeat { get; set; }
    public string Label { get; set; } = string.Empty;
    public EaseType Ease = Linear;
    public Action? Complete { get; set; }
    public Action? PerCycle { get; set; }

    public Tween(CustomNode target, params KeyFrame[] keyFrames)
    {
        StartTime = Now;
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

            return Math.Abs(end.Time - start.Time) > 0.01f ? (float)Math.Clamp((timePassed - start.Time) / (end.Time - start.Time), 0f, 1f) : 1;
        }

        start = KeyFrames[^1];
        end = KeyFrames[^1];
        return 1f;
    }

    public Tween Update(float? progOverride = null)
    {
        if ((progOverride == null && IsStale) || Target.Node == null)
        {
            IsStale = true;
            return this;
        }

        var timePassed = progOverride == null ? (Now - (StartTime + FromMilliseconds(Delay))).TotalMilliseconds : progOverride.Value * Length;

        if (timePassed < 0) return this;

        if (timePassed > Length)
        {
            if (Repeat)
            {
                PerCycle?.Invoke();
                StartTime = StartTime.AddMilliseconds(Length);
                timePassed %= Length;
            }
            else
            {
                IsStale = true;
                Complete?.Invoke();
            }
        }


        var easeFunc = EaseFuncs.TryGetValue(Ease, out var e) ? e : static p => p;
        var subProg = CalculateProg(timePassed, out var start, out var end);

        Interpolate(start, end, easeFunc(subProg)).ApplyToNode(Target);

        return this;
    }
}
