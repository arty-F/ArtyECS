#if UNITY_EDITOR
namespace ArtyECS.Core
{
    public struct MemoryUsageData
    {
        public long ComponentMemory;
        public long EntityMemory;
        public long FrameworkMemory;
        public long TotalMemory;

        public MemoryUsageData(long componentMemory, long entityMemory, long frameworkMemory)
        {
            ComponentMemory = componentMemory;
            EntityMemory = entityMemory;
            FrameworkMemory = frameworkMemory;
            TotalMemory = componentMemory + entityMemory + frameworkMemory;
        }

        public static MemoryUsageData operator +(MemoryUsageData left, MemoryUsageData right)
        {
            return new MemoryUsageData(
                left.ComponentMemory + right.ComponentMemory,
                left.EntityMemory + right.EntityMemory,
                left.FrameworkMemory + right.FrameworkMemory
            );
        }
    }
}
#endif

