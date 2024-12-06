using GaugeOMatic.Trackers;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using static CustomNodes.CustomNodeManager;
using static GaugeOMatic.Widgets.Common.WidgetUI;
using static GaugeOMatic.Widgets.Common.WidgetUI.WidgetUiTab;
using static System.Math;

namespace GaugeOMatic.Widgets;

[SuppressMessage("ReSharper", "VirtualMemberNeverOverridden.Global")]
[SuppressMessage("ReSharper", "SwitchStatementMissingSomeEnumCasesNoDefault")]
[SuppressMessage("ReSharper", "UnusedParameter.Global")]
public abstract class CounterWidget(Tracker tracker) : Widget(tracker)
{
    public abstract override CounterWidgetConfig Config { get; }

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
    public int GetMax() => Config.AsTimer ? Config.TimerSize : Tracker.GetCurrentData().MaxCount;
    public unsafe void SizeChange()
    {
        Detach();
        WidgetRoot.Dispose();

        WidgetContainer = BuildContainer();
        WidgetRoot = new(CreateResNode(), WidgetContainer);
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

        if (Config.AsTimer)
        {
            max = Config.TimerSize;
            current = (int)Ceiling(Clamp(Tracker.CurrentData.GaugeValue / Tracker.CurrentData.MaxGauge, 0, 1) * max);
            previous = (int)Ceiling(Clamp(Tracker.PreviousData.GaugeValue / Tracker.CurrentData.MaxGauge, 0, 1) * max);

            if (Config.InvertTimer)
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

    public void CounterAsTimerControls(string term)
    {
        if (term == "Cooldown") term = "Timer";

        if (ToggleControls($"Use as {term}", ref Config.AsTimer)) SizeChange();
        if (!Config.AsTimer) return;

        if (ToggleControls($"Invert {term}", ref Config.InvertTimer)) FirstRun = true;
        if (IntControls($"{term} Size", ref Config.TimerSize, 1, 60, 1)) SizeChange();
    }

    public override void DrawUI()
    {
        base.DrawUI();
        switch (UiTab)
        {
            case Behavior:
                CounterAsTimerControls(Tracker.TermGauge);
                break;
        }
    }
}

public abstract class CounterWidgetConfig : WidgetTypeConfig
{
    public enum CounterPulse { Never, Always, AtMax }

    public bool AsTimer;
    public bool InvertTimer;
    [DefaultValue(10)] public int TimerSize = 10;

    protected CounterWidgetConfig(CounterWidgetConfig? config) : base(config)
    {
        if (config == null) return;

        Scale = config.Scale;
        AsTimer = config.AsTimer;
        InvertTimer = config.InvertTimer;
        TimerSize = config.TimerSize;
    }

    protected CounterWidgetConfig() { }
}
