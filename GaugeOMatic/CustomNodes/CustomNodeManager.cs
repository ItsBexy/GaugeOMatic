using FFXIVClientStructs.FFXIV.Component.GUI;
using FFXIVClientStructs.Interop;
using System.Collections.Generic;

namespace CustomNodes;

public static partial class CustomNodeManager
{
    internal static Dictionary<uint, Pointer<AtkResNode>> RegisteredNodes = new();



    public static unsafe uint GetFreeId()
    {
        for (uint i = 10000; i < 90000; i++)
        {
            if (RegisteredNodes.TryGetValue(i, out var node) && node.Value != null && node.Value->NodeID == i) continue;

            RegisteredNodes.Remove(i);
            return i;
        }

        return 90001;
    }
}
