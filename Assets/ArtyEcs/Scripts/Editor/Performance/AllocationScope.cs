#if UNITY_EDITOR
using System;

namespace ArtyECS.Core
{
    public struct AllocationScope : IDisposable
    {
        private readonly string _operationType;
        private readonly WorldInstance _world;
        private readonly long _memoryBefore;
        private readonly int _gcCountBefore;
        private readonly bool _isEnabled;

        internal AllocationScope(string operationType, WorldInstance world)
        {
            _operationType = operationType;
            _world = world;
            _isEnabled = PerformanceMonitoring.IsAllocationTrackingEnabled;
            _memoryBefore = _isEnabled ? GC.GetTotalMemory(false) : 0;
            _gcCountBefore = _isEnabled ? GC.CollectionCount(0) : 0;
        }

        public void Dispose()
        {
            if (!_isEnabled)
                return;

            long memoryAfter = GC.GetTotalMemory(false);
            int gcCountAfter = GC.CollectionCount(0);

            long allocatedBytes = memoryAfter - _memoryBefore;
            int allocations = gcCountAfter - _gcCountBefore;

            if (allocatedBytes > 0 || allocations > 0)
            {
                PerformanceMonitoring.RecordAllocation(_operationType, _world, allocatedBytes, allocations);
            }
        }
    }
}
#endif

