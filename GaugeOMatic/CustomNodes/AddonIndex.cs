using FFXIVClientStructs.FFXIV.Component.GUI;
using System;

// ReSharper disable UnusedMember.Global

namespace CustomNodes;

public sealed unsafe class AddonIndex
{
    public readonly AtkUnitBase* AtkUnitBase;

    public AddonIndex(string addonName) => AtkUnitBase = (AtkUnitBase*)GameGui.GetAddonByName(addonName).Address;
    public AddonIndex(AtkUnitBase* atkUnitBase) => AtkUnitBase = atkUnitBase;
    public AddonIndex(IntPtr ptr) => AtkUnitBase = (AtkUnitBase*)ptr;

    public AtkResNode** NodeList => AtkUnitBase->UldManager.NodeList;
    public ushort NodeListSize => AtkUnitBase->UldManager.NodeListSize;

    public CustomNode this[uint id]
    {
        get
        {
            try
            {
                return AtkUnitBase->GetNodeById(id);
            }
            catch (Exception ex)
            {
                Log.Warning($"Error retrieving child node width ID {id}\n{ex}");
                return new();
            }
        }
    }

    public CustomNode this[int i]
    {
        get
        {
            try
            {
                return new(NodeListSize > i ? NodeList[i] : null);
            }
            catch (Exception ex)
            {
                Log.Warning($"Error retrieving child node at index {i}\n{ex}");
                return new();
            }
        }
    }

    public CustomNode this[params dynamic[] arr]
    {
        get
        {
            CustomNode result = this[arr[0]];
            for (var i = 1; i < arr.Length; i++) result = result[arr[i]];
            return result;
        }
    }

    public static implicit operator AtkUnitBase*(AddonIndex ai) => ai.AtkUnitBase;
    public static implicit operator AddonIndex(AtkUnitBase* aub) => new(aub);
    public static implicit operator AddonIndex(IntPtr ptr) => new((AtkUnitBase*)ptr);
}
