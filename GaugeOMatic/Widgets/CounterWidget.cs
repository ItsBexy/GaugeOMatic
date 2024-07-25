using CustomNodes;
using GaugeOMatic.Trackers;
using GaugeOMatic.Windows;
using System.Diagnostics.CodeAnalysis;
using static CustomNodes.CustomNodeManager;
using static GaugeOMatic.Widgets.WidgetUI;
using static System.Math;

namespace GaugeOMatic.Widgets;

public abstract class CounterWidgetConfig : WidgetTypeConfig
{
    public enum CounterPulse { Never, Always, AtMax }

    public bool AsTimer;
    public bool InvertTimer;
    public int TimerSize = 10;
}

[SuppressMessage("ReSharper", "VirtualMemberNeverOverridden.Global")]
[SuppressMessage("ReSharper", "UnusedParameter.Global")]
public abstract class CounterWidget : Widget
{
    public abstract CounterWidgetConfig GetConfig { get; }

    public virtual void OnFirstRun(int count, int max) { }

    public virtual void OnIncreaseToMax(int max) { }
    public virtual void OnIncreaseFromMin() { }

    public virtual void OnDecreaseFromMax(int max) { }
    public virtual void OnDecreaseToMin() { }

    public virtual void ShowStack(int i) { }
    public virtual void HideStack(int i) { }

    public virtual void PreUpdate(int i) { }
    public virtual void PostUpdate(int i) { }

    public bool FirstRun = true;

    public int Max;
    public int GetMax() => GetConfig.AsTimer ? GetConfig.TimerSize : Tracker.GetCurrentData().MaxCount;
    public unsafe void SizeChange()
    {
        Detach();
        WidgetRoot.Dispose();

        WidgetContainer = BuildContainer();
        WidgetRoot = new CustomNode(CreateResNode(), WidgetContainer);
        WidgetRoot.AssembleNodeTree();

        ApplyConfigs();
        FirstRun = true;

        Tracker.JobModule.SoftReset();
    }

    public override void Update()
    {
        int max;
        int current;
        int previous;

        if (GetConfig.AsTimer)
        {
            max = GetConfig.TimerSize;
            current = (int)Ceiling(Clamp(Tracker.CurrentData.GaugeValue / Tracker.CurrentData.MaxGauge, 0, 1) * max);
            previous = (int)Ceiling(Clamp(Tracker.PreviousData.GaugeValue / Tracker.CurrentData.MaxGauge, 0, 1) * max);

            if (GetConfig.InvertTimer)
            {
                current = max - current;
                previous = max - previous;
            }
        }
        else
        {
            max = Tracker.CurrentData.MaxCount;
            current = Clamp(Tracker.CurrentData.Count, 0, max);
            previous = Clamp(Tracker.PreviousData.Count, 0, max);
        }

        if (max != Max)
        {
            SizeChange();
            Max = max;
            return;
        }

        PreUpdate(current);

        if (FirstRun)
        {
            OnFirstRun(current, max);
            FirstRun = false;
        }
        else
        {
            if (current > previous)
            {
                if (current == max) OnIncreaseToMax(max);
                if (previous == 0) OnIncreaseFromMin();
                for (var i = previous; i < current; i++) ShowStack(i);
            }
            else if (current < previous)
            {
                if (current == 0) OnDecreaseToMin();
                if (previous == max) OnDecreaseFromMax(max);
                for (var i = current; i < previous; i++) HideStack(i);
            }
        }

        Animator.RunTweens();
        PostUpdate(current);
    }

    protected CounterWidget(Tracker tracker) : base(tracker) { }

    public void CounterAsTimerControls(ref bool useAsTimer, ref bool invertTimer, ref int timerSize, string term, ref UpdateFlags update)
    {
        if (term == "Cooldown") term = "Timer";

        if (ToggleControls($"Use as {term}", ref useAsTimer, ref update)) SizeChange();
        if (!useAsTimer) return;

        if (ToggleControls($"Invert {term}", ref invertTimer, ref update)) FirstRun = true;
        if (IntControls($"{term} Size", ref timerSize, 1, 30, 1, ref update)) SizeChange();
    }
}
