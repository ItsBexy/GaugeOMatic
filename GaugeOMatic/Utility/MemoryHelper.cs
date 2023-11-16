using FFXIVClientStructs.FFXIV.Client.System.Memory;

namespace GaugeOMatic.Utility;

internal static class MemoryHelper
{
    public static unsafe T* Alloc<T>() where T : unmanaged => (T*)Alloc((ulong)sizeof(T));
    public static unsafe T* CleanAlloc<T>() where T : unmanaged => (T*)CleanAlloc((ulong)sizeof(T));
    public static unsafe void* Alloc(ulong size) => IMemorySpace.GetUISpace()->Malloc(size, 8);

    public static unsafe void* CleanAlloc(ulong size)
    {
        var alloc = Alloc(size);
        IMemorySpace.Memset(alloc, 0, size);
        return alloc;
    }
}
