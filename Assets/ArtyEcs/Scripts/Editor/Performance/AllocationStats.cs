#if UNITY_EDITOR
namespace ArtyECS.Core
{
    public struct AllocationStats
    {
        public long QueryAllocations;
        public long SystemAllocations;
        public long TotalAllocations;
        public int AllocationCount;

        public AllocationStats(long queryAllocations, long systemAllocations, int allocationCount)
        {
            QueryAllocations = queryAllocations;
            SystemAllocations = systemAllocations;
            TotalAllocations = queryAllocations + systemAllocations;
            AllocationCount = allocationCount;
        }

        public static AllocationStats operator +(AllocationStats left, AllocationStats right)
        {
            return new AllocationStats(
                left.QueryAllocations + right.QueryAllocations,
                left.SystemAllocations + right.SystemAllocations,
                left.AllocationCount + right.AllocationCount
            );
        }
    }
}
#endif

