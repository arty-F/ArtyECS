#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Diagnostics;

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
    }
}
#endif

