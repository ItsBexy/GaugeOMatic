using FFXIVClientStructs.FFXIV.Component.GUI;
using Newtonsoft.Json;
using static GaugeOMatic.GaugeOMatic;
using static GaugeOMatic.Utility.Color;
using static GaugeOMatic.Windows.ConfigWindow;

namespace GaugeOMatic.JobModules;

internal static class Tweaks
{
    public static unsafe void VisibilityTweak(bool hide, bool simple, AtkResNode* standardNode, AtkResNode* simpleNode)
    {
        if (standardNode != null) standardNode->SetAlpha((byte)(hide || simple ? 0 : 255));
        if (simpleNode != null) simpleNode->SetAlpha((byte)(hide || !simple ? 0 : 255));
    }
}

public partial class TweakConfigs
{
    public TweakConfigs()
    {
        if (SCHDissHideText)
        {
            SCH1FaerieLess = 1;
            SCHDissHideText = false;
        }
    }
    [JsonIgnore] public bool Preview = false;
    [JsonIgnore] public bool ShowPreviews => Preview && ConfigWindow.IsOpen && JobModuleTab == JobModuleTabs.Tweaks;
    [JsonIgnore] public AddRGB? TestColor;
}
