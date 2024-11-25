using CustomNodes;
using GaugeOMatic.CustomNodes.Animation;
using GaugeOMatic.Trackers;
using static CustomNodes.CustomNodeManager;
using static GaugeOMatic.GameData.ParamRef;
using static GaugeOMatic.Widgets.Common.CommonParts;

namespace GaugeOMatic.Widgets;

public abstract unsafe partial class Widget
{
    public CustomNode WidgetIconContainer;
    public CustomNode WidgetIcon = null!;
    public CustomNode WidgetIconFrame = null!;

    public void UpdateIcon()
    {
        if (Tracker.CurrentData.IconOverride != null)
        {
            WidgetIcon.SetIcon(Tracker.CurrentData.IconOverride.Value);
        }
    }

    public void FadeIcon(bool show, int time = 300) =>
        Animator.Add(new Tween(WidgetIconContainer,
                               new(0, WidgetIconContainer),
                               new(time) { Alpha = show ? 255 : 0 }) { Label = "ShowHide" });

    public CustomNode BuildWidgetIcon(Tracker tracker)
    {
        WidgetIcon = new CustomNode(CreateIconNode(tracker.DisplayAttr.GameIcon))
                     .SetImageWrap(1)
                     .SetSize(tracker.RefType switch
                     {
                         RefType.Status => new(24,32),
                         RefType.JobGauge => new(32,32),
                         _ => new(40)
                     });

        WidgetIconFrame = new CustomNode(CreateImageNode(IconFrame,0))
                          .SetScale(0.5f)
                          .SetPos(-4,-3)
                          .SetVis(tracker.RefType switch
                          {
                              RefType.Action => true,
                              RefType.Parameter when tracker.ItemRef?.ID == (uint)ParamTypes.Castbar => true,
                              _ => false
                          });

        return new CustomNode(CreateResNode(), WidgetIcon, WidgetIconFrame).SetScale(1f,1f).RemoveFlags(CustomNode.CustomNodeFlags.SetVisByAlpha);
    }
}
