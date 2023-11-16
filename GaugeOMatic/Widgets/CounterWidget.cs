using System;
using GaugeOMatic.Trackers;

namespace GaugeOMatic.Widgets;

public abstract class CounterWidgetConfig
{
    public bool AsTimer;
    public bool InvertTimer;
    public int TimerSize = 10;
}

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

    public bool FirstRun = true;

    public override void Update()
    {
        int max;
        int current;
        int previous;
        if (GetConfig.AsTimer)
        {
            max = GetConfig.TimerSize;
            current = (int)Math.Ceiling(Math.Clamp(Tracker.CurrentData.GaugeValue / Tracker.CurrentData.MaxGauge,0,1) * max);
            previous = (int)Math.Ceiling(Math.Clamp(Tracker.PreviousData.GaugeValue / Tracker.CurrentData.MaxGauge,0,1) * max);

            if (GetConfig.InvertTimer)
            {
                current = max - current;
                previous = max - previous;
            }
        }
        else
        {
            max = Tracker.CurrentData.MaxCount;
            current = Math.Clamp(Tracker.CurrentData.Count, 0, max);
            previous = Math.Clamp(Tracker.PreviousData.Count, 0, max);
        }

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

        RunTweens();
    }

    protected CounterWidget(Tracker tracker) : base(tracker) { }
}
