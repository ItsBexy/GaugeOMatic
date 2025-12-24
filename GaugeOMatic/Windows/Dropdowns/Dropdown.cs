using GaugeOMatic.Utility;
using Dalamud.Bindings.ImGui;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using Dalamud.Interface.Utility.Raii;
using static Dalamud.Interface.Utility.ImGuiHelpers;

namespace GaugeOMatic.Windows.Dropdowns;

public abstract class Dropdown<T>
{
    public abstract List<T> Values { get; }
    public abstract List<string> DisplayNames { get; }
    public string[] ComboList => DisplayNames.ToArray();
    public T CurrentSelection => Values[Index];

    public int Index;

    public bool Draw(string label, float size)
    {
        var index = Index;

        ImGui.SetNextItemWidth(size * GlobalScale);
        if (ImGui.Combo($"##{label}", ref index, ComboList, ComboList.Length) && index != Index)
        {
            Index = index;
            return true;
        }

        return false;
    }
}

public abstract class BranchingDropdown
{
    public bool IsOpen { get; set; }
    public abstract string DropdownText(string fallback);

    /// <summary>An indexed collection, whose values can inform the behaviour of the <see cref ="DrawSubMenu">DrawSubMenu()</see> method.</summary>
    public abstract ICollection SubMenus { get; }
    public abstract void DrawSubMenu(int i);

    public void Draw(string label, float width)
    {
        var i = 0;

        Vector2 windowPos;
        Vector2 cursorPos;
        using (ImRaii.PushColor(ImGuiCol.Button,ImGuiHelpy.GetStyleColorVec4(ImGuiCol.ButtonHovered),IsOpen))
        {
            windowPos = ImGui.GetWindowPos();
            cursorPos = ImGui.GetCursorPos();
            ImGui.SetNextItemWidth(width * GlobalScale);
            ImGui.Combo($"##{label}{GetHashCode()}FakeCombo", ref i, DropdownText(label));
        }

        var popupPos = new Vector2(windowPos.X + cursorPos.X, windowPos.Y + cursorPos.Y + 22f);
        CreateMenuPopup($"##{label}{GetHashCode()}MenuPopup", width * GlobalScale, popupPos);

        if (ImGui.IsItemClicked())
        {
            ImGui.OpenPopup($"##{label}{GetHashCode()}MenuPopup");
        }
    }

    public void CreateMenuPopup(string label, float width, Vector2 popupPos)
    {
        using var p = ImRaii.Popup(label);
        IsOpen = p.Success;
        if (IsOpen)
        {
            ImGui.SetWindowPos(popupPos);
            ImGui.Button("##", new(width - 16f, 0.01f));
            for (var i = 0; i < SubMenus.Count; i++) DrawSubMenu(i);
            ImGui.Button("##", new(width - 16f, 0.01f));
        }
    }
}
