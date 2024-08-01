using CustomNodes;
using GaugeOMatic.CustomNodes.Animation;
using GaugeOMatic.GameData;
using GaugeOMatic.Trackers;
using System;
using System.ComponentModel;
using Newtonsoft.Json;
using static GaugeOMatic.GameData.ActionFlags;
using static GaugeOMatic.Trackers.Tracker;
using static GaugeOMatic.Widgets.MilestoneType;
using static GaugeOMatic.Widgets.WidgetUI;
using static Newtonsoft.Json.DefaultValueHandling;

// ReSharper disable VirtualMemberCallInConstructor
// ReSharper disable UnusedMethodReturnValue.Global

namespace GaugeOMatic.Widgets;

public abstract class GaugeBarWidgetConfig : WidgetTypeConfig
{
    protected GaugeBarWidgetConfig() => NumTextProps = NumTextDefault;

    protected GaugeBarWidgetConfig(GaugeBarWidgetConfig? config)
    {
        NumTextProps = NumTextDefault;

        if (config == null) { return; }

        HideEmpty = config.HideEmpty;
        HideFull = config.HideFull;
        Invert = config.Invert;
        AnimationLength = config.AnimationLength;
        NumTextProps = config.NumTextProps;
        SplitCharges = config.SplitCharges;
        MilestoneType = config.MilestoneType;
        Milestone = config.Milestone;
    }

    protected virtual NumTextProps NumTextDefault => new();
    public bool HideEmpty;
    public bool HideFull;
    public bool Invert;
    [DefaultValue(250)] public int AnimationLength = 250;
    public NumTextProps NumTextProps;
    public bool SplitCharges;
    [JsonProperty(DefaultValueHandling = Include)] public MilestoneType MilestoneType;
    [DefaultValue(0.5f)] public float Milestone = 0.5f;

    public static bool MilestoneControls(string label, ref MilestoneType milestoneType, ref float milestone, ref UpdateFlags update)
    {
        var r = RadioControls(label, ref milestoneType, new() { MilestoneType.None, Above, Below }, new() { "Never", "Above Threshold", "Below Threshold" }, ref update);
        var p = milestoneType > 0 && PercentControls("Threshold", ref milestone, ref update);

        return r || p;
    }

    public static bool SplitChargeControls(ref bool splitCharges, RefType refType, int maxCount, ref UpdateFlags update)
    {
        return refType == RefType.Action && maxCount > 1 &&
               RadioControls("Cooldown Style", ref splitCharges, new() { false, true }, new() { "All Charges", "Per Charge" }, ref update);
    }

    public static bool HideControls(string labelE, string labelF,ref bool hideEmpty, ref bool hideFull, Action emptyCheck, Action fullCheck, ref UpdateFlags update)
    {
        var toggle1 = ToggleControls(labelE, ref hideEmpty, ref update);
        var toggle2 = ToggleControls(labelF, ref hideFull, ref update);
        if (toggle1) emptyCheck();
        if (toggle2) fullCheck();
        return toggle1 || toggle2;
    }

    public static bool HideControls(ref bool hideEmpty, ref bool hideFull, Action emptyCheck, Action fullCheck, ref UpdateFlags update) => HideControls("Hide Empty", "Hide Full", ref hideEmpty, ref hideFull, emptyCheck, fullCheck, ref update);
}

public enum MilestoneType { None, Above, Below }

public abstract class GaugeBarWidget : Widget
{
    protected GaugeBarWidget(Tracker tracker) : base(tracker) { }

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

    public virtual void OnFirstRun(float prog) // used to instantly jump to the correct current state on first setup
    {
        Main.SetProgress(prog);
        Gain.SetProgress(0);
        Drain.SetProgress(0);
        if ((prog <= 0 && GetConfig.HideEmpty) || (prog >= 1 && GetConfig.HideFull)) HideBar(true);
    }

    // handlers that can be used to trigger animations/behaviours at certain progress points

    public virtual void OnIncrease(float prog, float prevProg) { }
    public virtual void OnIncreaseFromMin() { }
    public virtual void OnIncreaseToMax() { }

    public virtual void OnDecrease(float prog, float prevProg) { }
    public virtual void OnDecreaseToMin() { }
    public virtual void OnDecreaseFromMax() { }

    public virtual void PlaceTickMark(float prog) { }

    public virtual void PreUpdate(float prog, float prevProg) { }
    public virtual void PostUpdate(float prog) { }

    protected virtual void StartMilestoneAnim() { }
    protected virtual void StopMilestoneAnim() { }

    public virtual void HideBar(bool instant = false) { }
    public virtual void RevealBar(bool instant = false) { }

    public bool MilestoneActive;
    protected void HandleMilestone(float prog, bool reset = false)
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

    public virtual float AdjustProg(float prog) => prog;

    private float PreviousGauge => Tracker.PreviousData.GaugeValue;
    public float CurrentGauge => Tracker.CurrentData.GaugeValue;
    public float MaxGauge => Tracker.CurrentData.MaxGauge;

    public bool ShouldInvertByDefault => Tracker.RefType == RefType.Action &&
                                         !ActionRef.ActionData[Tracker.ItemRef?.ID??0].HasFlag(RequiresStatus | ComboBonus);

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
        else AnimateDrainGain(AdjustProg(prog), AdjustProg(prevProg), AnimDelay);

        if (prog > prevProg)
        {
            if (prog - prevProg >= GainTolerance) OnIncrease(prog, prevProg);
            if (prog > 0 && prevProg <= 0) OnIncreaseFromMin();
            if (prog >= 1 && prevProg < 1) OnIncreaseToMax();
        } else if (prevProg > prog)
        {
            if (prevProg - prog >= DrainTolerance) OnDecrease(prog, prevProg);
            if (prevProg >= 1 && prog < 1) OnDecreaseFromMax();
            if (prevProg > 0 && prog <= 0) OnDecreaseToMin();
        }

        Animator.RunTweens();
        HandleMilestone(prog);

        PostUpdate(prog);
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

    public void AnimateDrainGain(float current, float previous, int time = 250) => AnimateDrainGain(Main, Gain, Drain, current, previous, time);

    public unsafe void AnimateDrainGain(CustomNode main, CustomNode gain, CustomNode drain, float current, float previous, int time = 250)
    {
        var increasing = current > previous;
        var decreasing = previous > current;
        gain.SetProgress(main.Progress >= current ? 0 : current);
        drain.SetProgress(decreasing ? previous : 0);
        main.SetProgress(increasing || drain.Node == null ? previous : current);

        if (Math.Abs(current - previous) < 0.001f) return;

        Animator = Animator.Remove("DrainGain", main, drain)
                   + new Tween(increasing || drain.Node == null ? main : drain,
                                               new(0) { TimelineProg = previous },
                                               new(time) { TimelineProg = current })
                                               { Label = "DrainGain" };
    }

    public void EmptyCheck()
    {
        if (Tracker.CurrentData.GaugeValue == 0 || (GetConfig.Invert && Math.Abs(Tracker.CurrentData.GaugeValue - MaxGauge) < 0.01f))
            if (GetConfig.HideEmpty) HideBar();
            else RevealBar();
    }

    public void FullCheck()
    {
        if ((GetConfig.Invert && Tracker.CurrentData.GaugeValue == 0) || Math.Abs(Tracker.CurrentData.GaugeValue - MaxGauge) < 0.01f)
            if (GetConfig.HideFull) HideBar();
            else RevealBar();
    }
}
