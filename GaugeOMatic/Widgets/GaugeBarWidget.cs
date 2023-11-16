using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using GaugeOMatic.Trackers;
using static CustomNodes.CustomNodeManager;
using static CustomNodes.CustomNodeManager.Tween;
using static GaugeOMatic.Widgets.GaugeBarWidget.DrainGainType;

namespace GaugeOMatic.Widgets;

public abstract class GaugeBarWidgetConfig
{
    public NumTextProps NumTextProps;
    public int AnimationLength = 250;
    public bool Invert;
    protected virtual NumTextProps NumTextDefault => new();
}

[SuppressMessage("ReSharper", "UnusedParameter.Global")]
[SuppressMessage("ReSharper", "VirtualMemberNeverOverridden.Global")]
public abstract class GaugeBarWidget : Widget
{
    protected GaugeBarWidget(Tracker tracker) : base(tracker) { }

    public void UpdateNumText() => NumTextNode.UpdateNumText(NumTextProps, Tracker.CurrentData.GaugeValue, Tracker.CurrentData.MaxGauge);

    public abstract DrainGainType DGType { get; }
    public abstract GaugeBarWidgetConfig GetConfig { get; }
    public NumTextProps NumTextProps => GetConfig.NumTextProps;
    public bool Invert => GetConfig.Invert;
    public int AnimDelay => GetConfig.AnimationLength;

    public float GainThreshold { get; set; } = 0.049f;
    public float DrainThreshold { get; set; } = 0.049f;
    public float MidPoint { get; set; } = 0.5f;
    public bool FirstRun = true;
    public virtual CustomNode Drain { get; set; } = new();
    public virtual CustomNode Gain { get; set; } = new();
    public virtual CustomNode Main { get; set; } = new();
    public virtual CustomNode NumTextNode { get; set; }

    // handlers that can be used to trigger animations/behaviours at certain progress points

    public virtual void OnIncrease(float prog, float prevProg) { }
    public virtual void OnIncreaseFromMin(float prog, float prevProg) { }
    public virtual void OnIncreaseToMax(float prog, float prevProg) { }
    public virtual void OnIncreasePastMid(float prog, float prevProg) { }
    
    public virtual void OnDecrease(float prog, float prevProg) { }
    public virtual void OnDecreaseFromMax(float prog, float prevProg) { }
    public virtual void OnDecreaseToMin(float prog, float prevProg) { }
    public virtual void OnDecreasePastMid(float prog, float prevProg) { }

    public virtual void OnFirstRun(float prog) { } // used to instantly jump to the correct current state on first setup
    public virtual void PlaceTickMark(float prog) { }

    public virtual void PostUpdate(float prog) { }

    protected float CalcProg(bool usePrev = false)
    {
        var data = usePrev? Tracker.PreviousData : Tracker.CurrentData;
        var prog = data.GaugeValue / data.MaxGauge;
        return Invert ? 1 - prog : prog;
    }

    public override void Update()
    {
        UpdateNumText();

        var prog = CalcProg();
        var prevProg = CalcProg(true);

        if (FirstRun) OnFirstRun(prog);
        else AnimateDrainGain(DGType, ref Tweens, Main, Gain, Drain, CalcBarSize(prog), CalcBarSize(prevProg), AnimDelay);

        if (prog > prevProg)
        {
            if (prog - prevProg >= GainThreshold) OnIncrease(prog, prevProg);
            if (prog > 0 && prevProg <= 0) OnIncreaseFromMin(prog, prevProg);
            if (prog >= 1f && prevProg < 1f) OnIncreaseToMax(prog, prevProg);
            if (prog >= MidPoint && prevProg < MidPoint) OnIncreasePastMid(prog, prevProg);
        }

        if (prevProg > prog)
        {
            if (prevProg - prog >= DrainThreshold) OnDecrease(prog, prevProg);
            if (prevProg >= 1f && prog < 1f) OnDecreaseFromMax(prog, prevProg);
            if (prevProg > 0 && prog <= 0) OnDecreaseToMin(prog, prevProg);
            if (prevProg >= MidPoint && prog < MidPoint) OnDecreasePastMid(prog, prevProg);
        }

        RunTweens();
        PlaceTickMark(prog);
        PostUpdate(prog);
        FirstRun = false;
    }

    public abstract float CalcBarSize(float prog);
    
    public static void AnimateDrainGain(DrainGainType type, ref List<Tween> tweens, CustomNode main, CustomNode gain, CustomNode drain, float current, float previous, int time = 250)
    {
        var (getProp, setProp, createKeyFrame) = GetDrainGainFuncs(type);
        CreateDrainGainTweens(ref tweens, main, gain, drain, current, previous, time, getProp, setProp, createKeyFrame);
    }

    public static void AnimateDrainGain(DrainGainType type, ref List<Tween> tweens, CustomNode main, float current, float previous, int time = 250)
    {
        var (getProp, setProp, createKeyFrame) = GetDrainGainFuncs(type);
        CreateDrainGainTweens(ref tweens, main, new(), new(), current, previous, time, getProp, setProp, createKeyFrame);
    }

    private static unsafe (Func<CustomNode, float> getProp, Func<CustomNode, float, CustomNode> setProp, Func<int, float, KeyFrame> createKeyFrame) GetDrainGainFuncs(DrainGainType type)
    {
        Func<CustomNode, float> getProp = static _ => 0;
        Func<CustomNode, float, CustomNode> setProp = static (node, _) => node;
        Func<int, float, KeyFrame> createKeyFrame = static (kfTime, _) => new KeyFrame(kfTime);

        switch (type)
        {
            case Width:
                getProp = static node => node.Node->Width;
                setProp = static (node, val) => node.SetWidth(val);
                createKeyFrame = static (kfTime, val) => new KeyFrame(kfTime) { Width = val };
                break;
            case Height:
                getProp = static node => node.Node->Height;
                setProp = static (node, val) => node.SetHeight(val);
                createKeyFrame = static (kfTime, val) => new KeyFrame(kfTime) { Height = val };
                break;
            case X:
                getProp = static node => node.Node->X;
                setProp = static (node, val) => node.SetX(val);
                createKeyFrame = static (kfTime, val) => new KeyFrame(kfTime) { X = val };
                break;
            case Rotation:
                getProp = static node => node.Node->Rotation;
                setProp = static (node, val) => node.SetRotation(val);
                createKeyFrame = static (kfTime, val) => new KeyFrame(kfTime) { Rotation = val };
                break;
        }

        return (getProp, setProp, createKeyFrame);
    }

    private static unsafe void CreateDrainGainTweens(ref List<Tween> tweens, CustomNode main, CustomNode gain, CustomNode drain, float current, float previous, int time, Func<CustomNode, float> getProp, Func<CustomNode, float, CustomNode> setProp, Func<int, float, KeyFrame> createKeyFrame)
    {
        var barSize = getProp.Invoke(main);

        if (barSize >= current)
        {
            ClearNodeTweens(ref tweens, main);
            setProp.Invoke(main, current);
        }
        else if (current > previous)
        {
            ClearNodeTweens(ref tweens, main);
            tweens.Add(new(main, createKeyFrame.Invoke(0, barSize), createKeyFrame.Invoke(time, current)));
        }

        if (current < previous && drain.Node != null)
        {
            ClearNodeTweens(ref tweens, drain);
            tweens.Add(new(drain, createKeyFrame.Invoke(0, Math.Max(getProp.Invoke(drain), previous)),
                           createKeyFrame.Invoke(time, current)));
        }

        if (gain.Node != null) setProp.Invoke(gain, barSize >= current ? 0 : current);
    }

    public enum DrainGainType { Width, Height, Rotation, X }
}
