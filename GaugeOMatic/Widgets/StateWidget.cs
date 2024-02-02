using GaugeOMatic.Trackers;

namespace GaugeOMatic.Widgets;

public abstract class StateWidget : Widget
{
    public bool FirstRun = true;
    public abstract void Activate(int current);
    public abstract void Deactivate(int previous);
    public abstract void OnFirstRun(int current);
    public virtual void PostUpdate() { }
    public abstract void StateChange(int current, int previous);

    public override void Update()
    {
        if (FirstRun) { OnFirstRun(Tracker.CurrentData.State); FirstRun = false; }
        else
        {
            var current = Tracker.CurrentData.State;
            var previous = Tracker.PreviousData.State;
            if (current != previous)
            {
                if (previous == 0) Activate(current);
                else if (current == 0) Deactivate(Tracker.PreviousData.State);
                else StateChange(current, previous);
            }
        }
        PostUpdate();
        Animator.RunTweens();
    }

    protected StateWidget(Tracker tracker) : base(tracker) { }
}
