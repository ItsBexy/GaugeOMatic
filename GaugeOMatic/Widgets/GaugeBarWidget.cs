using GaugeOMatic.Trackers;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using static CustomNodes.CustomNodeManager;
using static CustomNodes.CustomNodeManager.Tween;
using static GaugeOMatic.Widgets.GaugeBarWidget.DrainGainType;
using static GaugeOMatic.Widgets.MilestoneType;

namespace GaugeOMatic.Widgets;

public abstract class GaugeBarWidgetConfig
{
    protected virtual NumTextProps NumTextDefault => new();
    public bool HideEmpty;
    public bool Invert;
    public int AnimationLength = 250;
    public NumTextProps NumTextProps;
    public bool SplitCharges;           //todo: implement on action trackers but not status trackers
    public MilestoneType MilestoneType; //todo: implement on more bars
    public float Milestone = 0.5f;
    public bool MilestoneCheck(float prog, float milestone) => prog > 0 && ((MilestoneType == Above && prog >= milestone) || (MilestoneType == Below && prog <= milestone));
}

public enum MilestoneType {None,Above,Below}

[SuppressMessage("ReSharper", "UnusedParameter.Global")]
[SuppressMessage("ReSharper", "VirtualMemberNeverOverridden.Global")]
public abstract class GaugeBarWidget : Widget
{
    public enum DrainGainType { Width, Height, Rotation, X }

    protected GaugeBarWidget(Tracker tracker) : base(tracker) { }

    public void UpdateNumText() => NumTextNode.UpdateNumText(NumTextProps, Tracker.CurrentData.GaugeValue, Tracker.CurrentData.MaxGauge);

    public abstract DrainGainType DGType { get; }
    public abstract GaugeBarWidgetConfig GetConfig { get; }
    public NumTextProps NumTextProps => GetConfig.NumTextProps;
    public bool Invert => GetConfig.Invert;
    public int AnimDelay => GetConfig.AnimationLength;

    public float GainTolerance { get; set; } = 0.049f;
    public float DrainTolerance { get; set; } = 0.049f;
    public float Milestone => GetConfig.Milestone;
    public bool FirstRun = true;
    public virtual CustomNode Drain { get; set; } = new();
    public virtual CustomNode Gain { get; set; } = new();
    public virtual CustomNode Main { get; set; } = new();
    public virtual CustomNode NumTextNode { get; set; }

    // handlers that can be used to trigger animations/behaviours at certain progress points

    public virtual void OnIncrease(float prog, float prevProg) { }
    public virtual void OnIncreaseFromMin(float prog, float prevProg) { }
    public virtual void OnIncreaseToMax(float prog, float prevProg) { }
    public virtual void OnIncreaseMilestone(float prog, float prevProg) { }
    
    public virtual void OnDecrease(float prog, float prevProg) { }
    public virtual void OnDecreaseFromMax(float prog, float prevProg) { }
    public virtual void OnDecreaseToMin(float prog, float prevProg) { }
    public virtual void OnDecreaseMilestone(float prog, float prevProg) { }

    public virtual void OnFirstRun(float prog) { } // used to instantly jump to the correct current state on first setup
    public virtual void PlaceTickMark(float prog) { }

    public virtual void PreUpdate(float prog, float prevProg) { }
    public virtual void PostUpdate(float prog, float prevProg) { }

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

        PreUpdate(prog, prevProg);

        if (FirstRun) OnFirstRun(prog);
        else AnimateDrainGain(DGType, ref Tweens, Main, Gain, Drain, CalcBarProperty(prog), CalcBarProperty(prevProg), CalcBarProperty(0f), AnimDelay);

        if (prog > prevProg)
        {
            if (prog - prevProg >= GainTolerance) OnIncrease(prog, prevProg);
            if (prog > 0 && prevProg <= 0) OnIncreaseFromMin(prog, prevProg);
            if (prog >= Milestone && prevProg < Milestone) OnIncreaseMilestone(prog, prevProg);
            if (prog >= 1f && prevProg < 1f) OnIncreaseToMax(prog, prevProg);
        }

        if (prevProg > prog)
        {
            if (prevProg - prog >= DrainTolerance) OnDecrease(prog, prevProg);
            if (prevProg >= 1f && prog < 1f) OnDecreaseFromMax(prog, prevProg);
            if (prevProg >= Milestone && prog < Milestone) OnDecreaseMilestone(prog, prevProg);
            if (prevProg > 0 && prog <= 0) OnDecreaseToMin(prog, prevProg);
        }

        RunTweens();
        PostUpdate(prog, prevProg);
        PlaceTickMark(prog);
        FirstRun = false;
    }

    public abstract float CalcBarProperty(float prog);
    
    public static void AnimateDrainGain(DrainGainType type, ref List<Tween> tweens, CustomNode main, CustomNode gain, CustomNode drain, float current, float previous, float min = 0f, int time = 250)
    {
        var (getProp, setProp, createKeyFrame) = GetDrainGainFuncs(type);
        CreateDrainGainTweens(ref tweens, main, gain, drain, current, previous, time, getProp, setProp, createKeyFrame, min);
    }

    public static void AnimateDrainGain(DrainGainType type, ref List<Tween> tweens, CustomNode main, float current, float previous, float min = 0f, int time = 250)
    {
        var (getProp, setProp, createKeyFrame) = GetDrainGainFuncs(type);
        CreateDrainGainTweens(ref tweens, main, new(), new(), current, previous, time, getProp, setProp, createKeyFrame, min);
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

    private static unsafe void CreateDrainGainTweens(ref List<Tween> tweens, CustomNode main, CustomNode gain, CustomNode drain, float current, float previous, int time, Func<CustomNode, float> getProp, Func<CustomNode, float, CustomNode> setProp, Func<int, float, KeyFrame> createKeyFrame, float min = 0f)
    {
        var mainProp = getProp.Invoke(main);

        var mainTween = tweens.Find(t => t.Target == main.Node && t.Label == "Main");
        if (mainProp >= current)
        {
            if (mainTween != null) UpdateKeyFrame(ref tweens, mainTween);
            else setProp.Invoke(main, current);
        }
        else if (current > previous)
        {
            if (mainTween != null) UpdateKeyFrame(ref tweens, mainTween);
            else tweens.Add(new(main,
                                createKeyFrame.Invoke(0, mainProp),
                                createKeyFrame.Invoke(time, current))
                                { Label = "Main" });
        }

        void UpdateKeyFrame(ref List<Tween> tweens, Tween tween)
        {
            tweens.Remove(tween);
            var frames = tween.KeyFrames;
            frames[1] = createKeyFrame.Invoke(time, current);
            tween.KeyFrames = frames;
            tweens.Add(tween);
        }

        if (current < previous && drain.Node != null)
        {
            var drainTween = tweens.Find(t => t.Target == drain.Node && t.Label == "Drain");

            if (drainTween == null)
                tweens.Add(new(drain, 
                               createKeyFrame.Invoke(0, Math.Max(getProp.Invoke(drain), previous)),
                               createKeyFrame.Invoke(time, current)) 
                               { Label = "Drain" });
            else UpdateKeyFrame(ref tweens, drainTween);
        }

        if (gain.Node != null) setProp.Invoke(gain, mainProp >= current ? min : current);
    }
}
