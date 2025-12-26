using System;
using System.Collections.Generic;
#if UNITY_EDITOR
using System.Diagnostics;
#endif

namespace ArtyECS.Core
{
    internal static class SystemsManager
    {
        private class SystemStorageInstance
        {
            public readonly List<SystemHandler> UpdateQueue = new List<SystemHandler>();

            public readonly List<SystemHandler> FixedUpdateQueue = new List<SystemHandler>();
        }

        private static readonly Dictionary<WorldInstance, SystemStorageInstance> WorldStorages =
            new Dictionary<WorldInstance, SystemStorageInstance>();

#if UNITY_EDITOR
        private static readonly Dictionary<(SystemHandler system, WorldInstance world), SystemTimingData> SystemTimings =
            new Dictionary<(SystemHandler system, WorldInstance world), SystemTimingData>();
        private static int _systemTimingInsertionCounter = 0;
#endif

        private static SystemStorageInstance GetWorldStorage(WorldInstance world)
        {
            if (world == null)
                throw new ArgumentNullException(nameof(world));

            if (!WorldStorages.TryGetValue(world, out var storage))
            {
                storage = new SystemStorageInstance();
                WorldStorages[world] = storage;
            }

            return storage;
        }

        internal static int GetWorldCount()
        {
            return WorldStorages.Count;
        }

        internal static bool IsWorldInitialized(WorldInstance world)
        {
            if (world == null)
                throw new ArgumentNullException(nameof(world));
            return WorldStorages.ContainsKey(world);
        }

        internal static List<SystemHandler> GetUpdateQueue(WorldInstance world)
        {
            var storage = GetWorldStorage(world);
            return storage.UpdateQueue;
        }

        internal static List<SystemHandler> GetFixedUpdateQueue(WorldInstance world)
        {
            var storage = GetWorldStorage(world);
            return storage.FixedUpdateQueue;
        }

        internal static void AddToUpdate(SystemHandler system, WorldInstance world)
        {
            if (system == null)
            {
                throw new ArgumentNullException(nameof(system));
            }

            var storage = GetWorldStorage(world);
            storage.UpdateQueue.Add(system);
        }

        internal static void AddToUpdate(SystemHandler system, int order, WorldInstance world)
        {
            if (system == null)
            {
                throw new ArgumentNullException(nameof(system));
            }

            var storage = GetWorldStorage(world);
            var queue = storage.UpdateQueue;

            if (order < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(order), "Order cannot be negative.");
            }

            if (order > queue.Count)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(order),
                    $"Order {order} is out of range. Queue has {queue.Count} systems. Valid range: 0 to {queue.Count}.");
            }

            queue.Insert(order, system);
        }

        internal static void ExecuteUpdate(WorldInstance world)
        {
#if UNITY_EDITOR
            ResetFrameTimings(world);
#endif
            var storage = GetWorldStorage(world);
            var queue = storage.UpdateQueue;

            for (int i = 0; i < queue.Count; i++)
            {
                var system = queue[i];
                try
                {
#if UNITY_EDITOR
                    if (PerformanceMonitoring.IsEnabled)
                    {
                        var stopwatch = Stopwatch.StartNew();
                        system.Execute(world);
                        stopwatch.Stop();
                        RecordTiming(system, world, stopwatch.Elapsed.TotalMilliseconds);
                    }
                    else
                    {
                        system.Execute(world);
                    }
#else
                    system.Execute(world);
#endif
                }
                catch (Exception ex)
                {
                    UnityEngine.Debug.LogError($"System '{system.GetType().Name}' execution failed in world '{world.Name}': {ex}");
                }
            }
        }

        internal static void AddToFixedUpdate(SystemHandler system, WorldInstance world)
        {
            if (system == null)
            {
                throw new ArgumentNullException(nameof(system));
            }

            var storage = GetWorldStorage(world);
            storage.FixedUpdateQueue.Add(system);
        }

        internal static void AddToFixedUpdate(SystemHandler system, int order, WorldInstance world)
        {
            if (system == null)
            {
                throw new ArgumentNullException(nameof(system));
            }

            var storage = GetWorldStorage(world);
            var queue = storage.FixedUpdateQueue;

            if (order < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(order), "Order cannot be negative.");
            }

            if (order > queue.Count)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(order),
                    $"Order {order} is out of range. Queue has {queue.Count} systems. Valid range: 0 to {queue.Count}.");
            }

            queue.Insert(order, system);
        }

        internal static void ExecuteFixedUpdate(WorldInstance world)
        {
#if UNITY_EDITOR
            ResetFrameTimings(world);
#endif
            var storage = GetWorldStorage(world);
            var queue = storage.FixedUpdateQueue;

            for (int i = 0; i < queue.Count; i++)
            {
                var system = queue[i];
                try
                {
#if UNITY_EDITOR
                    var stopwatch = Stopwatch.StartNew();
                    system.Execute(world);
                    stopwatch.Stop();
                    RecordTiming(system, world, stopwatch.Elapsed.TotalMilliseconds);
#else
                    system.Execute(world);
#endif
                }
                catch (Exception ex)
                {
                    UnityEngine.Debug.LogError($"System '{system.GetType().Name}' execution failed in world '{world.Name}': {ex}");
                }
            }
        }

        internal static void ExecuteOnce(SystemHandler system, WorldInstance world)
        {
            if (system == null)
            {
                throw new ArgumentNullException(nameof(system));
            }

            system.Execute(world);
        }

        internal static void ClearWorld(WorldInstance world)
        {
            if (world == null)
            {
                return;
            }

            WorldStorages.Remove(world);
            
#if UNITY_EDITOR
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
#endif
        }

        internal static void ExecuteUpdateAllWorlds()
        {
            foreach (var kvp in WorldStorages)
            {
                var world = kvp.Key;
#if UNITY_EDITOR
                ResetFrameTimings(world);
#endif
                var queue = kvp.Value.UpdateQueue;

                for (int i = 0; i < queue.Count; i++)
                {
                    var system = queue[i];
                    try
                    {
#if UNITY_EDITOR
                        if (PerformanceMonitoring.IsEnabled)
                        {
                            var stopwatch = Stopwatch.StartNew();
                            system.Execute(world);
                            stopwatch.Stop();
                            RecordTiming(system, world, stopwatch.Elapsed.TotalMilliseconds);
                        }
                        else
                        {
                            system.Execute(world);
                        }
#else
                        system.Execute(world);
#endif
                    }
                    catch (Exception ex)
                    {
                        UnityEngine.Debug.LogError($"System '{system.GetType().Name}' execution failed in world '{world.Name}': {ex}");
                    }
                }
            }
        }

        internal static void ExecuteFixedUpdateAllWorlds()
        {
            foreach (var kvp in WorldStorages)
            {
                var world = kvp.Key;
#if UNITY_EDITOR
                ResetFrameTimings(world);
#endif
                var queue = kvp.Value.FixedUpdateQueue;

                for (int i = 0; i < queue.Count; i++)
                {
                    var system = queue[i];
                    try
                    {
#if UNITY_EDITOR
                        if (PerformanceMonitoring.IsEnabled)
                        {
                            var stopwatch = Stopwatch.StartNew();
                            system.Execute(world);
                            stopwatch.Stop();
                            RecordTiming(system, world, stopwatch.Elapsed.TotalMilliseconds);
                        }
                        else
                        {
                            system.Execute(world);
                        }
#else
                        system.Execute(world);
#endif
                    }
                    catch (Exception ex)
                    {
                        UnityEngine.Debug.LogError($"System '{system.GetType().Name}' execution failed in world '{world.Name}': {ex}");
                    }
                }
            }
        }

        internal static bool RemoveFromUpdate(SystemHandler system, WorldInstance world)
        {
            if (system == null)
                throw new ArgumentNullException(nameof(system));
            if (world == null)
                throw new ArgumentNullException(nameof(world));

            var storage = GetWorldStorage(world);
            return storage.UpdateQueue.Remove(system);
        }

        internal static bool RemoveFromFixedUpdate(SystemHandler system, WorldInstance world)
        {
            if (system == null)
                throw new ArgumentNullException(nameof(system));
            if (world == null)
                throw new ArgumentNullException(nameof(world));

            var storage = GetWorldStorage(world);
            return storage.FixedUpdateQueue.Remove(system);
        }

        internal static void ClearAll()
        {
            WorldStorages.Clear();
#if UNITY_EDITOR
            SystemTimings.Clear();
#endif
        }

#if UNITY_EDITOR
        private static void ResetFrameTimings(WorldInstance world)
        {
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
                var timing = SystemTimings[key];
                timing.LastExecutionTime = 0.0;
                SystemTimings[key] = timing;
            }
        }

        private static void RecordTiming(SystemHandler system, WorldInstance world, double milliseconds)
        {
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
                timing.MaxExecutionTime = System.Math.Max(timing.MaxExecutionTime, milliseconds);
                SystemTimings[key] = timing;
            }
        }

        internal static SystemTimingData? GetSystemTiming(SystemHandler system, WorldInstance world)
        {
            var key = (system, world);
            if (SystemTimings.TryGetValue(key, out var timing))
            {
                return timing;
            }
            return null;
        }

        internal static List<SystemTimingData> GetAllSystemTimings(WorldInstance world)
        {
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
#endif
    }
}

