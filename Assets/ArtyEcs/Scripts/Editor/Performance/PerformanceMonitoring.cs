#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace ArtyECS.Core
{
    public static class PerformanceMonitoring
    {
        private const string PREFS_KEY_MONITORING_ENABLED = "ArtyECS.PerformanceMonitoring.Enabled";
        private const string PREFS_KEY_SHOW_WARNINGS = "ArtyECS.PerformanceMonitoring.ShowWarnings";
        private static bool _isEnabled = false;
        private static bool _showWarnings = false;

        private static readonly Dictionary<(SystemHandler system, WorldInstance world), SystemTimingData> SystemTimings =
            new Dictionary<(SystemHandler system, WorldInstance world), SystemTimingData>();
        private static readonly Dictionary<(QueryType queryType, WorldInstance world), QueryTimingData> QueryTimings =
            new Dictionary<(QueryType queryType, WorldInstance world), QueryTimingData>();
        private static readonly Dictionary<(string operationType, WorldInstance world), (long bytes, int count)> Allocations =
            new Dictionary<(string operationType, WorldInstance world), (long bytes, int count)>();
        private static int _systemTimingInsertionCounter = 0;
        private static int _queryTimingInsertionCounter = 0;

        static PerformanceMonitoring()
        {
            _isEnabled = UnityEditor.EditorPrefs.GetBool(PREFS_KEY_MONITORING_ENABLED, false);
            _showWarnings = UnityEditor.EditorPrefs.GetBool(PREFS_KEY_SHOW_WARNINGS, true);
        }

        public static bool IsEnabled
        {
            get => _isEnabled;
            set
            {
                if (_isEnabled != value)
                {
                    _isEnabled = value;
                    UnityEditor.EditorPrefs.SetBool(PREFS_KEY_MONITORING_ENABLED, value);
                }
            }
        }

        public static bool ShowWarnings
        {
            get => _showWarnings;
            set
            {
                if (_showWarnings != value)
                {
                    _showWarnings = value;
                    UnityEditor.EditorPrefs.SetBool(PREFS_KEY_SHOW_WARNINGS, value);
                }
            }
        }

        public static SystemTimingScope StartSystemTiming(SystemHandler system, WorldInstance world)
        {
            return new SystemTimingScope(system, world);
        }

        public static QueryTimingScope StartQueryTiming(QueryType queryType, WorldInstance world, string componentTypes = null)
        {
            return new QueryTimingScope(queryType, world, componentTypes);
        }

        public static AllocationScope StartAllocationTracking(string operationType, WorldInstance world)
        {
            return new AllocationScope(operationType, world);
        }

        public static bool IsAllocationTrackingEnabled => IsEnabled;

        public static void ResetSystemTimings(WorldInstance world)
        {
            if (world == null || !IsEnabled)
                return;

            var keysToReset = new List<(SystemHandler system, WorldInstance world)>();
            foreach (var kvp in SystemTimings)
            {
                if (kvp.Key.world == world)
                {
                    keysToReset.Add(kvp.Key);
                }
            }

            foreach (var key in keysToReset)
            {
                if (SystemTimings.TryGetValue(key, out var timing))
                {
                    timing.LastExecutionTime = 0.0;
                    SystemTimings[key] = timing;
                }
            }
        }

        public static void ResetQueryTimings(WorldInstance world)
        {
            if (world == null || !IsEnabled)
                return;

            var keysToReset = new List<(QueryType queryType, WorldInstance world)>();
            foreach (var kvp in QueryTimings)
            {
                if (kvp.Key.world == world)
                {
                    keysToReset.Add(kvp.Key);
                }
            }

            foreach (var key in keysToReset)
            {
                if (QueryTimings.TryGetValue(key, out var timing))
                {
                    timing.LastExecutionTime = 0.0;
                    QueryTimings[key] = timing;
                }
            }
        }

        public static SystemTimingData? GetSystemTiming(SystemHandler system, WorldInstance world)
        {
            if (!IsEnabled)
                return null;

            var key = (system, world);
            if (SystemTimings.TryGetValue(key, out var timing))
            {
                return timing;
            }
            return null;
        }

        public static List<SystemTimingData> GetAllSystemTimings(WorldInstance world)
        {
            if (!IsEnabled)
                return new List<SystemTimingData>();

            var timings = new List<SystemTimingData>();
            foreach (var kvp in SystemTimings)
            {
                if (kvp.Key.world == world)
                {
                    timings.Add(kvp.Value);
                }
            }
            return timings;
        }

        public static QueryTimingData? GetQueryTiming(QueryType queryType, WorldInstance world)
        {
            if (!IsEnabled)
                return null;

            var key = (queryType, world);
            if (QueryTimings.TryGetValue(key, out var timing))
            {
                return timing;
            }
            return null;
        }

        public static List<QueryTimingData> GetAllQueryTimings(WorldInstance world)
        {
            if (!IsEnabled)
                return new List<QueryTimingData>();

            var timings = new List<QueryTimingData>();
            foreach (var kvp in QueryTimings)
            {
                if (kvp.Key.world == world)
                {
                    timings.Add(kvp.Value);
                }
            }
            return timings;
        }

        public static void ClearSystemTimings(WorldInstance world)
        {
            if (world == null || !IsEnabled)
                return;

            var keysToRemove = new List<(SystemHandler system, WorldInstance world)>();
            foreach (var kvp in SystemTimings)
            {
                if (kvp.Key.world == world)
                {
                    keysToRemove.Add(kvp.Key);
                }
            }

            foreach (var key in keysToRemove)
            {
                SystemTimings.Remove(key);
            }
        }

        public static void ClearQueryTimings(WorldInstance world)
        {
            if (world == null || !IsEnabled)
                return;

            var keysToRemove = new List<(QueryType queryType, WorldInstance world)>();
            foreach (var kvp in QueryTimings)
            {
                if (kvp.Key.world == world)
                {
                    keysToRemove.Add(kvp.Key);
                }
            }

            foreach (var key in keysToRemove)
            {
                QueryTimings.Remove(key);
            }
        }

        public static void ClearAll()
        {
            if (!IsEnabled)
                return;

            SystemTimings.Clear();
            QueryTimings.Clear();
            Allocations.Clear();
        }

        internal static void RecordSystemTiming(SystemHandler system, WorldInstance world, double milliseconds)
        {
            if (!IsEnabled || world == null || system == null)
                return;

            var key = (system, world);
            if (!SystemTimings.ContainsKey(key))
            {
                SystemTimings[key] = new SystemTimingData(system, world)
                {
                    LastExecutionTime = milliseconds,
                    TotalExecutionTime = milliseconds,
                    ExecutionCount = 1,
                    MaxExecutionTime = milliseconds,
                    InsertionOrder = _systemTimingInsertionCounter++
                };
            }
            else
            {
                var timing = SystemTimings[key];
                timing.LastExecutionTime = milliseconds;
                timing.TotalExecutionTime += milliseconds;
                timing.ExecutionCount++;
                timing.MaxExecutionTime = Math.Max(timing.MaxExecutionTime, milliseconds);
                SystemTimings[key] = timing;
            }
        }

        internal static void RecordQueryTiming(QueryType queryType, WorldInstance world, double milliseconds, string componentTypes = null)
        {
            if (!IsEnabled || world == null)
                return;

            var key = (queryType, world);
            if (!QueryTimings.ContainsKey(key))
            {
                QueryTimings[key] = new QueryTimingData(queryType, world)
                {
                    LastExecutionTime = milliseconds,
                    TotalExecutionTime = milliseconds,
                    ExecutionCount = 1,
                    MaxExecutionTime = milliseconds,
                    InsertionOrder = _queryTimingInsertionCounter++
                };
            }
            else
            {
                var timing = QueryTimings[key];
                timing.LastExecutionTime = milliseconds;
                timing.TotalExecutionTime += milliseconds;
                timing.ExecutionCount++;
                timing.MaxExecutionTime = Math.Max(timing.MaxExecutionTime, milliseconds);
                QueryTimings[key] = timing;
            }

            if (milliseconds > 1.0 && ShowWarnings)
            {
                var stackTrace = new StackTrace(skipFrames: 1, fNeedFileInfo: false);
                string systemInfo = GetSystemInfoFromStackTrace(stackTrace);

                string message = $"[ArtyECS] Slow query detected: {queryType} in world '{world.Name}' took {milliseconds:F3}ms";
                if (!string.IsNullOrEmpty(componentTypes))
                {
                    message += $" | Components: {componentTypes}";
                }
                if (!string.IsNullOrEmpty(systemInfo))
                {
                    message += $" | System: {systemInfo}";
                }

                UnityEngine.Debug.LogWarning(message);
            }
        }

        private static string GetSystemInfoFromStackTrace(StackTrace stackTrace)
        {
            for (int i = 0; i < stackTrace.FrameCount; i++)
            {
                var frame = stackTrace.GetFrame(i);
                if (frame == null)
                    continue;

                var method = frame.GetMethod();
                if (method == null)
                    continue;

                var declaringType = method.DeclaringType;
                if (declaringType == null)
                    continue;

                if (typeof(SystemHandler).IsAssignableFrom(declaringType))
                {
                    return declaringType.Name;
                }
            }
            return null;
        }

        public static MemoryUsageData GetMemoryUsage(WorldInstance world)
        {
            if (!IsEnabled || world == null)
                return new MemoryUsageData(0, 0, 0);

            long componentMemory = CalculateComponentMemory(world);
            long entityMemory = CalculateEntityMemory(world);
            long frameworkMemory = CalculateFrameworkMemory(world);

            return new MemoryUsageData(componentMemory, entityMemory, frameworkMemory);
        }

        public static MemoryUsageData GetTotalMemoryUsage()
        {
            if (!IsEnabled)
                return new MemoryUsageData(0, 0, 0);

            MemoryUsageData total = new MemoryUsageData(0, 0, 0);

            var allWorlds = World.GetAllWorlds();
            foreach (var world in allWorlds)
            {
                total = total + GetMemoryUsage(world);
            }

            return total;
        }

        private static long CalculateComponentMemory(WorldInstance world)
        {
            if (!ComponentsManager.IsWorldInitialized(world))
                return 0;

            long totalMemory = 0;

            var worldTables = GetWorldTables(world);
            if (worldTables == null)
                return 0;

            foreach (var kvp in worldTables)
            {
                var table = kvp.Value;
                var tableData = GetTableData(table);
                if (tableData.HasValue)
                {
                    var (componentType, componentsArray, entitiesArray, dictionaryCount) = tableData.Value;
                    int componentSize = GetComponentSize(componentType);
                    long componentArrayMemory = (long)componentSize * componentsArray.Length;
                    long entityArrayMemory = (long)Marshal.SizeOf<Entity>() * entitiesArray.Length;
                    long dictionaryOverhead = EstimateDictionaryOverhead(dictionaryCount, Marshal.SizeOf<Entity>(), sizeof(int));
                    totalMemory += componentArrayMemory + entityArrayMemory + dictionaryOverhead;
                }
            }

            return totalMemory;
        }

        private static long CalculateEntityMemory(WorldInstance world)
        {
            int allocatedCount = EntitiesManager.GetAllocatedCount(world);
            return (long)Marshal.SizeOf<Entity>() * allocatedCount;
        }

        private static long CalculateFrameworkMemory(WorldInstance world)
        {
            long frameworkMemory = 0;

            frameworkMemory += CalculateComponentStorageOverhead(world);
            frameworkMemory += CalculateEntityPoolOverhead(world);
            frameworkMemory += CalculateWorldOverhead(world);

            return frameworkMemory;
        }

        private static long CalculateComponentStorageOverhead(WorldInstance world)
        {
            var worldTables = GetWorldTables(world);
            if (worldTables == null)
                return 0;

            long overhead = 0;
            int pointerSize = IntPtr.Size;

            var allWorldTables = GetAllWorldTables();
            overhead += EstimateDictionaryOverhead(allWorldTables.Count, pointerSize * 2, pointerSize);
            overhead += EstimateDictionaryOverhead(worldTables.Count, pointerSize, pointerSize);

            var tableCache = GetTableCache();
            overhead += EstimateDictionaryOverhead(tableCache.Count, pointerSize * 2 + pointerSize, pointerSize);

            return overhead;
        }

        private static long CalculateEntityPoolOverhead(WorldInstance world)
        {
            var poolData = GetEntityPoolData(world);
            if (!poolData.HasValue)
                return 0;

            var (availableCount, generationCount) = poolData.Value;
            long stackOverhead = EstimateStackOverhead(availableCount);
            long dictionaryOverhead = EstimateDictionaryOverhead(generationCount, sizeof(int), sizeof(int));

            return stackOverhead + dictionaryOverhead;
        }

        private static long CalculateWorldOverhead(WorldInstance world)
        {
            var worldLinks = GetWorldLinks(world);
            if (!worldLinks.HasValue)
                return 0;

            var (entityToGameObjectCount, gameObjectIdToEntityCount) = worldLinks.Value;
            int pointerSize = IntPtr.Size;
            long entityToGameObjectOverhead = EstimateDictionaryOverhead(entityToGameObjectCount, pointerSize, pointerSize);
            long gameObjectIdToEntityOverhead = EstimateDictionaryOverhead(gameObjectIdToEntityCount, sizeof(int), pointerSize);

            return entityToGameObjectOverhead + gameObjectIdToEntityOverhead;
        }

        private static Dictionary<Type, IComponentTable> GetWorldTables(WorldInstance world)
        {
            return ComponentsManager.GetWorldTablesForMonitoring(world);
        }

        private static Dictionary<WorldInstance, Dictionary<Type, IComponentTable>> GetAllWorldTables()
        {
            return ComponentsManager.GetAllWorldTablesForMonitoring();
        }

        private static Dictionary<(WorldInstance world, Type type), IComponentTable> GetTableCache()
        {
            return ComponentsManager.GetTableCacheForMonitoring();
        }

        private static (Type componentType, Array componentsArray, Array entitiesArray, int dictionaryCount)? GetTableData(IComponentTable table)
        {
            return ComponentsManager.GetTableDataForMonitoring(table);
        }

        private static (int AvailableCount, int GenerationCount)? GetEntityPoolData(WorldInstance world)
        {
            return EntitiesManager.GetPoolDataForMonitoring(world);
        }

        private static (int EntityToGameObjectCount, int GameObjectIdToEntityCount)? GetWorldLinks(WorldInstance world)
        {
            return WorldInstance.GetLinksForMonitoring(world);
        }

        private static long EstimateDictionaryOverhead(int count, int keySize, int valueSize)
        {
            if (count == 0)
                return 0;

            int estimatedCapacity = (int)(count / 0.72) + 1;
            long bucketArrayMemory = (long)sizeof(int) * estimatedCapacity;
            long entryArrayMemory = (long)(keySize + valueSize + sizeof(int)) * count;

            return bucketArrayMemory + entryArrayMemory;
        }

        private static long EstimateStackOverhead(int count)
        {
            if (count == 0)
                return 0;

            int estimatedCapacity = Math.Max(count, 256);
            return (long)sizeof(int) * estimatedCapacity;
        }

        private static int GetComponentSize(Type componentType)
        {
            try
            {
                return Marshal.SizeOf(componentType);
            }
            catch
            {
                return 0;
            }
        }

        internal static void RecordAllocation(string operationType, WorldInstance world, long bytes, int allocations)
        {
            if (!IsEnabled || world == null)
                return;

            var key = (operationType, world);
            if (Allocations.TryGetValue(key, out var existing))
            {
                Allocations[key] = (existing.bytes + bytes, existing.count + allocations);
            }
            else
            {
                Allocations[key] = (bytes, allocations);
            }

            if (ShowWarnings && bytes > 1024)
            {
                UnityEngine.Debug.LogWarning($"[ArtyECS] Allocation detected in {operationType} (World: {world.Name}): {bytes} bytes, {allocations} GC collections");
            }
        }

        public static AllocationStats GetAllocationStats(WorldInstance world)
        {
            if (!IsEnabled || world == null)
                return new AllocationStats(0, 0, 0);

            long queryAllocations = 0;
            long systemAllocations = 0;
            int totalCount = 0;

            foreach (var kvp in Allocations)
            {
                if (kvp.Key.world == world)
                {
                    var (bytes, count) = kvp.Value;
                    if (kvp.Key.operationType.StartsWith("Query:"))
                    {
                        queryAllocations += bytes;
                    }
                    else if (kvp.Key.operationType.StartsWith("System:"))
                    {
                        systemAllocations += bytes;
                    }
                    totalCount += count;
                }
            }

            return new AllocationStats(queryAllocations, systemAllocations, totalCount);
        }

        public static AllocationStats GetTotalAllocationStats()
        {
            if (!IsEnabled)
                return new AllocationStats(0, 0, 0);

            AllocationStats total = new AllocationStats(0, 0, 0);

            var allWorlds = World.GetAllWorlds();
            foreach (var world in allWorlds)
            {
                total = total + GetAllocationStats(world);
            }

            return total;
        }

        public static void ClearAllocationStats(WorldInstance world)
        {
            if (!IsEnabled || world == null)
                return;

            var keysToRemove = new List<(string operationType, WorldInstance world)>();
            foreach (var kvp in Allocations)
            {
                if (kvp.Key.world == world)
                {
                    keysToRemove.Add(kvp.Key);
                }
            }

            foreach (var key in keysToRemove)
            {
                Allocations.Remove(key);
            }
        }

        public static void ClearAllAllocationStats()
        {
            if (!IsEnabled)
                return;

            Allocations.Clear();
        }
    }
}
#endif

