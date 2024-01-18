using GaugeOMatic.Trackers;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using GaugeOMatic.Windows;
using static CustomNodes.CustomNodeManager;
using static CustomNodes.CustomNodeManager.Tween;
using static GaugeOMatic.Widgets.GaugeBarWidget.DrainGainType;
using static GaugeOMatic.Widgets.MilestoneType;
using static GaugeOMatic.Widgets.WidgetUI;

namespace GaugeOMatic.Widgets;

public abstract class GaugeBarWidgetConfig
{
    protected virtual NumTextProps NumTextDefault => new();
    public bool HideEmpty;
    public bool Invert;
    public int AnimationLength = 250;
    public NumTextProps NumTextProps;
    public bool SplitCharges = false;
    public MilestoneType MilestoneType; //todo: implement on more bars
    public float Milestone = 0.5f;

    public static void MilestoneControls(string label, ref MilestoneType milestoneType, ref float milestone, ref UpdateFlags update)
    {
        RadioControls(label, ref milestoneType, new() { None, Above, Below }, new() { "Never", "Above Threshold", "Below Threshold" }, ref update);
        if (milestoneType > 0) PercentControls("Threshold", ref milestone, ref update);
    }

    public static void SplitChargeControls(ref bool splitCharges, RefType refType, int maxCount, ref UpdateFlags update)
    {
        if (refType != RefType.Action || maxCount <= 1) return;
        RadioControls("Cooldown Style", ref splitCharges, new() { false, true }, new() { "All Charges", "Per Charge" }, ref update);
    }
}

public enum MilestoneType {None,Above,Below}

[SuppressMessage("ReSharper", "UnusedParameter.Global")]
public abstract class GaugeBarWidget : Widget
{
    public enum DrainGainType { Width, Height, Rotation, X }

    protected GaugeBarWidget(Tracker tracker) : base(tracker) { }

    public abstract DrainGainType DGType { get; }
    public abstract GaugeBarWidgetConfig GetConfig { get; }
    public NumTextProps NumTextProps => GetConfig.NumTextProps;
    public bool Invert => GetConfig.Invert;
    public int AnimDelay => GetConfig.AnimationLength;

    public float Milestone => GetConfig.Milestone;
    public MilestoneType MilestoneType => GetConfig.MilestoneType;
    public float GainTolerance { get; set; } = 0.049f;
    public float DrainTolerance { get; set; } = 0.049f;

    public bool FirstRun = true;
    public CustomNode Drain { get; set; } = new();
    public CustomNode Gain { get; set; } = new();
    public CustomNode Main { get; set; } = new();
    public NumTextNode NumTextNode = null!;

    public virtual void OnFirstRun(float prog) { } // used to instantly jump to the correct current state on first setup

    // handlers that can be used to trigger animations/behaviours at certain progress points

    public virtual void OnIncrease(float prog, float prevProg) { }
    public virtual void OnIncreaseFromMin(float prog, float prevProg) { }

    public virtual void OnDecrease(float prog, float prevProg) { }
    public virtual void OnDecreaseToMin(float prog, float prevProg) { }

    public virtual void PlaceTickMark(float prog) { }

    public virtual void PreUpdate(float prog, float prevProg) { }
    public virtual void PostUpdate(float prog, float prevProg) { }

    protected virtual void StartMilestoneAnim() { }
    protected virtual void StopMilestoneAnim() { }

    public bool MilestoneActive;
    protected void HandleMilestone(float prog,bool reset = false)
    {
        if (reset)
        {
            StopMilestoneAnim();
            MilestoneActive = false;
        }
        var check = prog > 0 && ((MilestoneType == Above && prog >= Milestone) || (MilestoneType == Below && prog <= Milestone));
        if (!MilestoneActive && check) StartMilestoneAnim();
        else if (MilestoneActive && !check) StopMilestoneAnim();

        MilestoneActive = check;
    }

    protected float CalcProg(bool usePrev = false)
    {
        var prog = (usePrev? PreviousGauge : CurrentGauge) / MaxGauge;
        return Invert ? 1 - prog : prog;
    }

    private float PreviousGauge => Tracker.PreviousData.GaugeValue;
    public float CurrentGauge => Tracker.CurrentData.GaugeValue;
    private float MaxGauge => Tracker.CurrentData.MaxGauge;

    public override void Update()
    {
        var current = CurrentGauge;
        var max = MaxGauge;
        var prog = CalcProg();
        var prevProg = CalcProg(true);

        if (GetConfig.SplitCharges && Tracker.RefType == RefType.Action) AdjustForCharges(ref current, ref max, ref prog, ref prevProg);

        NumTextNode.UpdateValue(current, max);
        PreUpdate(prog, prevProg);

        if (FirstRun) OnFirstRun(prog);
        else AnimateDrainGain(DGType, ref Tweens, Main, Gain, Drain, CalcBarProperty(prog), CalcBarProperty(prevProg), CalcBarProperty(0f), AnimDelay);

        if (prog > prevProg)
        {
            if (prog - prevProg >= GainTolerance) OnIncrease(prog, prevProg);
            if (prog > 0 && prevProg <= 0) OnIncreaseFromMin(prog, prevProg);
        }

        if (prevProg > prog)
        {
            if (prevProg - prog >= DrainTolerance) OnDecrease(prog, prevProg);
            if (prevProg > 0 && prog <= 0) OnDecreaseToMin(prog, prevProg);
        }

        RunTweens();
        HandleMilestone(prog);

        PostUpdate(prog, prevProg);
        PlaceTickMark(prog);
        FirstRun = false;
    }

    public void AdjustForCharges(ref float currentTime, ref float maxTime, ref float prog, ref float prevProg, bool splitTimerText = true, bool splitProg = true)
    {
        var maxCharges = Tracker.CurrentData.MaxCount;
        if (maxCharges <= 1) return;

        if (splitTimerText)
        {
            maxTime /= maxCharges;
            currentTime %= maxTime;
        }

        if (splitProg)
        {
            var progPerCharge = 1f / maxCharges;
            prog = prog / progPerCharge % 1f;
            prevProg = prevProg / progPerCharge % 1f;
        }
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
        Func<int, float, KeyFrame> createKeyFrame = static (kfTime, _) => new(kfTime);

        switch (type)
        {
            case Width:
                getProp = static node => node.Node->Width;
                setProp = static (node, val) => node.SetWidth(val);
                createKeyFrame = static (kfTime, val) => new(kfTime) { Width = val };
                break;
            case Height:
                getProp = static node => node.Node->Height;
                setProp = static (node, val) => node.SetHeight(val);
                createKeyFrame = static (kfTime, val) => new(kfTime) { Height = val };
                break;
            case X:
                getProp = static node => node.Node->X;
                setProp = static (node, val) => node.SetX(val);
                createKeyFrame = static (kfTime, val) => new(kfTime) { X = val };
                break;
            case Rotation:
                getProp = static node => node.Node->Rotation;
                setProp = static (node, val) => node.SetRotation(val);
                createKeyFrame = static (kfTime, val) => new(kfTime) { Rotation = val };
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
