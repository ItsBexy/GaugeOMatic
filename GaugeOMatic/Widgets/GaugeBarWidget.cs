using CustomNodes;
using Dalamud.Interface.Utility.Raii;
using FFXIVClientStructs.FFXIV.Client.UI;
using GaugeOMatic.CustomNodes.Animation;
using GaugeOMatic.GameData;
using GaugeOMatic.Trackers;
using ImGuiNET;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Numerics;
using static GaugeOMatic.GameData.ActionFlags;
using static GaugeOMatic.Widgets.GaugeBarWidgetConfig;
using static GaugeOMatic.Widgets.MilestoneType;
using static GaugeOMatic.Widgets.WidgetUI;
using static Newtonsoft.Json.DefaultValueHandling;
// ReSharper disable UnusedMethodReturnValue.Global

namespace GaugeOMatic.Widgets;

public enum MilestoneType { None, Above, Below }

public abstract class GaugeBarWidget(Tracker tracker) : Widget(tracker)
{
    public abstract override GaugeBarWidgetConfig Config { get; }
    public NumTextProps NumTextProps => Config.NumTextProps;
    public bool Invert => Config.Invert;
    public int AnimDelay => Config.AnimationLength;

    public float Milestone => Config.Milestone;
    public MilestoneType MilestoneType => Config.MilestoneType;
    public float SoundMilestone => Config.SoundMilestone;
    public MilestoneType SoundType => Config.SoundType;
    public uint SoundId => Config.SoundId;
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
        if ((prog <= 0 && Config.HideEmpty) || (prog >= 1 && Config.HideFull)) HideBar(true);
    }

    // handlers that can be used to trigger animations/behaviours at certain progress points

    public virtual void OnIncrease(float prog, float prevProg) { }

    public virtual void OnDecrease(float prog, float prevProg) { }

    public virtual void OnDecreaseToMin() { if (Config.HideEmpty) HideBar(); }
    public virtual void OnIncreaseFromMin() { if (Config.HideEmpty) RevealBar(); }
    public virtual void OnDecreaseFromMax() { if (Config.HideFull) RevealBar(); }
    public virtual void OnIncreaseToMax() { if (Config.HideFull) HideBar(); }

    public virtual void PlaceTickMark(float prog) { }

    public virtual void PreUpdate(float prog, float prevProg) { }
    public virtual void PostUpdate(float prog) { }

    protected virtual void StartMilestoneAnim() { }
    protected virtual void StopMilestoneAnim() { }

    public virtual void HideBar(bool instant = false) { }
    public virtual void RevealBar(bool instant = false) { }

    public bool MilestoneActive;
    public bool SoundMilestoneActive;
    protected void HandleMilestone(float prog, bool reset = false)
    {
        if (reset)
        {
            StopMilestoneAnim();
            MilestoneActive = false;
        }
        var check = prog > 0 && ((MilestoneType == Above && prog >= Milestone) || (MilestoneType == Below && prog <= Milestone));
        if (!MilestoneActive && check) {StartMilestoneAnim();}
        else if (MilestoneActive && !check) StopMilestoneAnim();

        MilestoneActive = check;

        var soundCheck = prog > 0 && ((SoundType == Above && prog >= SoundMilestone) || (SoundType == Below && prog < SoundMilestone));
        if (!SoundMilestoneActive && soundCheck && !SoundBlackList.Contains(SoundId))
        {
            UIGlobals.PlaySoundEffect(SoundId);
        }
        SoundMilestoneActive = soundCheck;
    }

    protected float CalcProg(bool usePrev = false)
    {
        var prog = (usePrev ? PreviousGauge : CurrentGauge) / MaxGauge;
        return Invert ? 1 - prog : prog;
    }

    public virtual float AdjustProg(float prog) => prog;

    private float PreviousGauge => Tracker.PreviousData.GaugeValue;
    public float CurrentGauge => Tracker.CurrentData.GaugeValue;
    public float MaxGauge => Tracker.CurrentData.MaxGauge;

    public bool ShouldInvertByDefault => Tracker.RefType == RefType.Action &&
                                         !ActionRef.ActionData[Tracker.ItemRef?.ID ?? 0].HasFlag(RequiresStatus | ComboBonus);

    public override void Update()
    {
        var current = CurrentGauge;
        var max = MaxGauge;
        var prog = CalcProg();
        var prevProg = CalcProg(true);

        if (Config.SplitCharges && Tracker.RefType == RefType.Action) AdjustForCharges(ref current, ref max, ref prog, ref prevProg);

        NumTextNode.UpdateValue(current, max);
        PreUpdate(prog, prevProg);

        if (FirstRun) OnFirstRun(prog);
        else AnimateDrainGain(AdjustProg(prog), AdjustProg(prevProg), AnimDelay);

        if (prog > prevProg)
        {
            if (prog - prevProg >= GainTolerance) OnIncrease(prog, prevProg);
            if (prog > 0 && prevProg <= 0) OnIncreaseFromMin();
            if (prog >= 1 && prevProg < 1) OnIncreaseToMax();
        }
        else if (prevProg > prog)
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

    public bool HideControls() => HideControls("Hide Empty", "Hide Full");

    public bool HideControls(string labelE, string labelF)
    {
        var emptyToggle = ToggleControls(labelE, ref Config.HideEmpty);
        var fullToggle = ToggleControls(labelF, ref Config.HideFull);

        var isEmpty = Tracker.CurrentData.GaugeValue == 0;
        var isFull = Math.Abs(Tracker.CurrentData.GaugeValue - MaxGauge) < 0.01f;

        if (emptyToggle && (isEmpty || (Config.Invert && isFull)))
        {
            if (Config.HideEmpty) HideBar();
            else RevealBar();
        }

        if (fullToggle && ((Config.Invert && isEmpty) || isFull))
        {
            if (Config.HideFull) HideBar();
            else RevealBar();
        }

        return emptyToggle || fullToggle;
    }

    public override void DrawUI()
    {
        base.DrawUI();
        switch (UiTab)
        {
            case WidgetUiTab.Sound:
                SoundControls(ref Config.SoundType, ref Config.SoundMilestone, ref Config.SoundId, MaxGauge);
                break;
            default:
                break;
        }
    }
}

public abstract class GaugeBarWidgetConfig : WidgetTypeConfig
{
    protected GaugeBarWidgetConfig(GaugeBarWidgetConfig? config) : base(config)
    {
        if (config == null)
        {
            NumTextProps = NumTextDefault;
            return;
        }

        HideEmpty = config.HideEmpty;
        HideFull = config.HideFull;
        Invert = config.Invert;
        AnimationLength = config.AnimationLength;
        NumTextProps = config.NumTextProps;
        SplitCharges = config.SplitCharges;
        MilestoneType = config.MilestoneType;
        Milestone = config.Milestone;
        SoundType = config.SoundType;
        SoundMilestone = config.SoundMilestone;
        SoundId = config.SoundId;
    }

    protected GaugeBarWidgetConfig() => NumTextProps = NumTextDefault;

    protected virtual NumTextProps NumTextDefault => new();
    public bool HideEmpty;
    public bool HideFull;
    public bool Invert;
    [DefaultValue(250)] public int AnimationLength = 250;
    public NumTextProps NumTextProps;
    public bool SplitCharges;
    [JsonProperty(DefaultValueHandling = Include)] public MilestoneType MilestoneType;
    [DefaultValue(0.5f)] public float Milestone = 0.5f;
    [JsonProperty(DefaultValueHandling = Include)] public MilestoneType SoundType;
    [DefaultValue(0.5f)] public float SoundMilestone = 0.5f;
    [DefaultValue(78)] public uint SoundId = 78;

    public static bool MilestoneControls(string label, ref MilestoneType milestoneType, ref float milestone, float max)
    {
        var input1 = RadioControls(label, ref milestoneType, [MilestoneType.None, Above, Below], ["Never", "Above Threshold", "Below Threshold"]);
        var scaledMilestone = milestone * max;

        var input2 = milestoneType > 0 && FloatControls("Threshold", ref scaledMilestone,0,max,1,$"%.0f ({Math.Round(milestone*100)}%%)");
        if (input2)
        {
            milestone = scaledMilestone / max;
        }

        return input1 || input2;
    }

    public static readonly List<uint> SoundBlackList = [19,21,74];
    public static bool SoundControls(ref MilestoneType soundType, ref float soundMilestone, ref uint soundId, float max)
    {
        var input1 = RadioControls("Play Sound", ref soundType, [MilestoneType.None, Above, Below], ["Never", "Above Threshold", "Below Threshold"]);

        var scaledMilestone = soundMilestone * max;
        var input2 = soundType > 0 && FloatControls("Threshold", ref scaledMilestone,0,max,1,$"%.0f ({Math.Round(soundMilestone*100)}%%)");
        if (input2)
        {
            soundMilestone = scaledMilestone / max;
        }

        using  var col = ImRaii.PushColor(ImGuiCol.Text,new Vector4(1f,0.35f,0.35f,1), SoundBlackList.Contains(soundId));
        var input3 = soundType > 0 && IntControls("Sound ID", ref soundId, 0, 79, 1);
        if (SoundBlackList.Contains(soundId))
        {
            ImGui.TableNextColumn();
            using (ImRaii.TextWrapPos(ImGui.GetWindowSize().X - 10))
            {
                ImGui.TextWrapped($"Sound effect #{soundId} will not be played. It should never be played. We will not play it. We will not help you play it.");
            }
        }
        else
        {
            if (input3)
            {
                UIGlobals.PlaySoundEffect(soundId);
            }
        }

        return input1 || input3 || input2;
    }

    public static bool SplitChargeControls(ref bool splitCharges, RefType refType, int maxCount)
    {
        return refType == RefType.Action && maxCount > 1 &&
               RadioControls("Cooldown Style", ref splitCharges, [false, true], ["All Charges", "Per Charge"]);
    }
}
