#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEditor;
using ArtyECS.Core;

namespace ArtyECS.Editor
{
    public class ECSPerformanceWindow : EditorWindow
    {
        private const float DEFAULT_REFRESH_INTERVAL = 1.0f;
        private const string PREFS_KEY_AUTO_REFRESH = "ArtyECS.PerformanceWindow.AutoRefresh";
        private const string PREFS_KEY_REFRESH_INTERVAL = "ArtyECS.PerformanceWindow.RefreshInterval";
        private const string PREFS_KEY_SELECTED_WORLD = "ArtyECS.PerformanceWindow.SelectedWorld";

        private bool _autoRefresh = true;
        private float _refreshInterval = DEFAULT_REFRESH_INTERVAL;
        private float _lastRefreshTime;
        private string _selectedWorldName = null;
        private WorldInstance _selectedWorld = null;
        private Vector2 _scrollPosition;

        [MenuItem("Window/ArtyECS/Performance Monitor")]
        public static void ShowWindow()
        {
            var window = GetWindow<ECSPerformanceWindow>("ECS Performance");
            window.minSize = new Vector2(400, 300);
            window.Show();
        }

        private void OnEnable()
        {
            LoadPreferences();
            EditorApplication.update += OnEditorUpdate;
        }

        private void OnDisable()
        {
            EditorApplication.update -= OnEditorUpdate;
            SavePreferences();
        }

        private void LoadPreferences()
        {
            _autoRefresh = EditorPrefs.GetBool(PREFS_KEY_AUTO_REFRESH, true);
            _refreshInterval = EditorPrefs.GetFloat(PREFS_KEY_REFRESH_INTERVAL, DEFAULT_REFRESH_INTERVAL);
            _selectedWorldName = EditorPrefs.GetString(PREFS_KEY_SELECTED_WORLD, null);
            
            bool monitoringEnabled = EditorPrefs.GetBool("ArtyECS.PerformanceMonitoring.Enabled", false);
            PerformanceMonitoring.IsEnabled = monitoringEnabled;
            
            bool showWarnings = EditorPrefs.GetBool("ArtyECS.PerformanceMonitoring.ShowWarnings", false);
            PerformanceMonitoring.ShowWarnings = showWarnings;
            
            UpdateSelectedWorld();
        }

        private void SavePreferences()
        {
            EditorPrefs.SetBool(PREFS_KEY_AUTO_REFRESH, _autoRefresh);
            EditorPrefs.SetFloat(PREFS_KEY_REFRESH_INTERVAL, _refreshInterval);
            EditorPrefs.SetString(PREFS_KEY_SELECTED_WORLD, _selectedWorldName ?? "");
        }

        private void OnEditorUpdate()
        {
            if (_autoRefresh && Application.isPlaying)
            {
                float currentTime = (float)EditorApplication.timeSinceStartup;
                if (currentTime - _lastRefreshTime >= _refreshInterval)
                {
                    Repaint();
                    _lastRefreshTime = currentTime;
                }
            }
        }

        private void OnGUI()
        {
            DrawHeader();
            
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
            
            DrawWorldSelector();
            EditorGUILayout.Space(10);
            
            DrawSystemExecutionTimesSection();
            EditorGUILayout.Space(10);
            
            DrawQueryPerformanceSection();
            EditorGUILayout.Space(10);
            
            DrawMemoryUsageSection();
            EditorGUILayout.Space(10);
            
            DrawAllocationTrackingSection();
            
            EditorGUILayout.EndScrollView();
        }

        private void DrawHeader()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            
            GUILayout.Label("ECS Performance Monitor", EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();
            
            bool monitoringEnabled = PerformanceMonitoring.IsEnabled;
            bool newMonitoringEnabled = GUILayout.Toggle(monitoringEnabled, "Enable Monitoring", EditorStyles.toolbarButton);
            if (newMonitoringEnabled != monitoringEnabled)
            {
                PerformanceMonitoring.IsEnabled = newMonitoringEnabled;
            }
            
            if (GUILayout.Button("Export", EditorStyles.toolbarButton))
            {
                ExportPerformanceData();
            }
            
            _autoRefresh = GUILayout.Toggle(_autoRefresh, "Auto-Refresh", EditorStyles.toolbarButton);
            
            if (GUILayout.Button("Refresh", EditorStyles.toolbarButton))
            {
                RefreshData();
            }
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Refresh Interval (seconds):", GUILayout.Width(150));
            _refreshInterval = EditorGUILayout.Slider(_refreshInterval, 0.1f, 2.0f);
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            bool showWarnings = PerformanceMonitoring.ShowWarnings;
            bool newShowWarnings = GUILayout.Toggle(showWarnings, "Show Warnings", GUILayout.Width(120));
            if (newShowWarnings != showWarnings)
            {
                PerformanceMonitoring.ShowWarnings = newShowWarnings;
            }
            EditorGUILayout.EndHorizontal();
            
            if (!PerformanceMonitoring.IsEnabled)
            {
                EditorGUILayout.HelpBox("Performance monitoring is disabled. Enable it to see metrics.", MessageType.Info);
            }
            
            EditorGUILayout.Space(5);
        }

        private void DrawWorldSelector()
        {
            var allWorlds = World.GetAllWorlds();
            
            if (allWorlds.Count == 0)
            {
                EditorGUILayout.HelpBox("No worlds available. Create entities to initialize the global world.", MessageType.Info);
                _selectedWorld = null;
                _selectedWorldName = null;
                return;
            }
            
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("World:", GUILayout.Width(50));
            
            string[] worldNames = allWorlds.Select(w => w.Name).ToArray();
            int currentIndex = -1;
            
            if (!string.IsNullOrEmpty(_selectedWorldName))
            {
                currentIndex = Array.IndexOf(worldNames, _selectedWorldName);
            }
            
            if (currentIndex < 0 && allWorlds.Count > 0)
            {
                currentIndex = 0;
                _selectedWorldName = allWorlds[0].Name;
                UpdateSelectedWorld();
            }
            
            int newIndex = EditorGUILayout.Popup(currentIndex, worldNames);
            
            if (newIndex != currentIndex && newIndex >= 0 && newIndex < worldNames.Length)
            {
                _selectedWorldName = worldNames[newIndex];
                UpdateSelectedWorld();
            }
            
            EditorGUILayout.EndHorizontal();
        }

        private void UpdateSelectedWorld()
        {
            var allWorlds = World.GetAllWorlds();
            
            if (allWorlds.Count == 0)
            {
                _selectedWorld = null;
                _selectedWorldName = null;
                return;
            }
            
            if (string.IsNullOrEmpty(_selectedWorldName))
            {
                _selectedWorld = allWorlds[0];
                _selectedWorldName = _selectedWorld.Name;
                return;
            }
            
            _selectedWorld = allWorlds.FirstOrDefault(w => w.Name == _selectedWorldName);
            
            if (_selectedWorld == null)
            {
                _selectedWorld = allWorlds[0];
                _selectedWorldName = _selectedWorld.Name;
            }
        }

        private void DrawSystemExecutionTimesSection()
        {
            EditorGUILayout.LabelField("System Execution Times", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("Enter Play Mode to see system execution times.", MessageType.Info);
                EditorGUILayout.EndVertical();
                return;
            }
            
            if (_selectedWorld == null)
            {
                EditorGUILayout.HelpBox("No world selected.", MessageType.Info);
                EditorGUILayout.EndVertical();
                return;
            }
            
            var timings = _selectedWorld.GetAllSystemTimings();
            
            if (timings.Count == 0)
            {
                EditorGUILayout.HelpBox("No system timing data available. Systems need to execute at least once.", MessageType.Info);
                EditorGUILayout.EndVertical();
                return;
            }
            
            var sortedTimings = timings.OrderBy(t => t.System?.GetType().Name ?? "Unknown")
                .ThenBy(t => t.InsertionOrder)
                .ToList();
            
            double totalSystemTime = sortedTimings.Sum(t => t.TotalExecutionTime);
            
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("System Name", EditorStyles.boldLabel, GUILayout.Width(200));
            GUILayout.Label("Last (ms)", EditorStyles.boldLabel, GUILayout.Width(80));
            GUILayout.Label("Avg (ms)", EditorStyles.boldLabel, GUILayout.Width(80));
            GUILayout.Label("Max (ms)", EditorStyles.boldLabel, GUILayout.Width(80));
            GUILayout.Label("Total (ms)", EditorStyles.boldLabel, GUILayout.Width(80));
            GUILayout.Label("Total %", EditorStyles.boldLabel, GUILayout.Width(80));
            GUILayout.Label("Count", EditorStyles.boldLabel, GUILayout.Width(60));
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space(2);
            
            foreach (var timing in sortedTimings)
            {
                DrawSystemTimingRow(timing, totalSystemTime);
            }
            
            EditorGUILayout.EndVertical();
        }
        
        private void DrawSystemTimingRow(SystemTimingData timing, double totalSystemTime)
        {
            EditorGUILayout.BeginHorizontal();
            
            string systemName = timing.System?.GetType().Name ?? "Unknown";
            GUILayout.Label(systemName, GUILayout.Width(200));
            
            double lastTime = timing.LastExecutionTime;
            double avgTime = timing.AverageTime;
            double maxTime = timing.MaxExecutionTime;
            double totalTime = timing.TotalExecutionTime;
            long count = timing.ExecutionCount;
            
            double totalPercent = totalSystemTime > 0 ? (totalTime / totalSystemTime) * 100.0 : 0.0;
            
            GUILayout.Label($"{lastTime:F3}", GUILayout.Width(80));
            GUILayout.Label($"{avgTime:F3}", GUILayout.Width(80));
            GUILayout.Label($"{maxTime:F3}", GUILayout.Width(80));
            GUILayout.Label($"{totalTime:F3}", GUILayout.Width(80));
            GUILayout.Label($"{totalPercent:F1}%", GUILayout.Width(80));
            GUILayout.Label(count.ToString(), GUILayout.Width(60));
            
            EditorGUILayout.EndHorizontal();
        }

        private void DrawQueryPerformanceSection()
        {
            EditorGUILayout.LabelField("Query Performance", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("Enter Play Mode to see query performance metrics.", MessageType.Info);
                EditorGUILayout.EndVertical();
                return;
            }
            
            if (_selectedWorld == null)
            {
                EditorGUILayout.HelpBox("No world selected.", MessageType.Info);
                EditorGUILayout.EndVertical();
                return;
            }
            
            var timings = _selectedWorld.GetAllQueryTimings();
            
            if (timings.Count == 0)
            {
                EditorGUILayout.HelpBox("No query timing data available. Queries need to execute at least once.", MessageType.Info);
                EditorGUILayout.EndVertical();
                return;
            }
            
            var sortedTimings = timings.OrderBy(t => t.QueryType.ToString())
                .ThenBy(t => t.InsertionOrder)
                .ToList();
            
            double totalQueryTime = sortedTimings.Sum(t => t.TotalExecutionTime);
            
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Query Type", EditorStyles.boldLabel, GUILayout.Width(200));
            GUILayout.Label("Last (ms)", EditorStyles.boldLabel, GUILayout.Width(80));
            GUILayout.Label("Avg (ms)", EditorStyles.boldLabel, GUILayout.Width(80));
            GUILayout.Label("Max (ms)", EditorStyles.boldLabel, GUILayout.Width(80));
            GUILayout.Label("Total (ms)", EditorStyles.boldLabel, GUILayout.Width(80));
            GUILayout.Label("Total %", EditorStyles.boldLabel, GUILayout.Width(80));
            GUILayout.Label("Count", EditorStyles.boldLabel, GUILayout.Width(60));
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space(2);
            
            foreach (var timing in sortedTimings)
            {
                DrawQueryTimingRow(timing, totalQueryTime);
            }
            
            EditorGUILayout.EndVertical();
        }
        
        private void DrawQueryTimingRow(QueryTimingData timing, double totalQueryTime)
        {
            EditorGUILayout.BeginHorizontal();
            
            string queryTypeName = timing.QueryType.ToString();
            GUILayout.Label(queryTypeName, GUILayout.Width(200));
            
            double lastTime = timing.LastExecutionTime;
            double avgTime = timing.AverageTime;
            double maxTime = timing.MaxExecutionTime;
            double totalTime = timing.TotalExecutionTime;
            long count = timing.ExecutionCount;
            
            double totalPercent = totalQueryTime > 0 ? (totalTime / totalQueryTime) * 100.0 : 0.0;
            
            GUILayout.Label($"{lastTime:F3}", GUILayout.Width(80));
            GUILayout.Label($"{avgTime:F3}", GUILayout.Width(80));
            GUILayout.Label($"{maxTime:F3}", GUILayout.Width(80));
            GUILayout.Label($"{totalTime:F3}", GUILayout.Width(80));
            GUILayout.Label($"{totalPercent:F1}%", GUILayout.Width(80));
            GUILayout.Label(count.ToString(), GUILayout.Width(60));
            
            EditorGUILayout.EndHorizontal();
        }

        private void DrawMemoryUsageSection()
        {
            EditorGUILayout.LabelField("Memory Usage", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("Enter Play Mode to see memory usage.", MessageType.Info);
                EditorGUILayout.EndVertical();
                return;
            }
            
            if (_selectedWorld == null)
            {
                EditorGUILayout.HelpBox("No world selected.", MessageType.Info);
                EditorGUILayout.EndVertical();
                return;
            }
            
            if (!PerformanceMonitoring.IsEnabled)
            {
                EditorGUILayout.HelpBox("Performance monitoring is disabled. Enable it to see memory usage.", MessageType.Info);
                EditorGUILayout.EndVertical();
                return;
            }
            
            var memory = PerformanceMonitoring.GetMemoryUsage(_selectedWorld);
            var totalMemory = PerformanceMonitoring.GetTotalMemoryUsage();
            
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Memory Type", EditorStyles.boldLabel, GUILayout.Width(150));
            GUILayout.Label("Current World", EditorStyles.boldLabel, GUILayout.Width(120));
            GUILayout.Label("All Worlds", EditorStyles.boldLabel, GUILayout.Width(120));
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space(2);
            
            DrawMemoryRow("Component Memory", memory.ComponentMemory, totalMemory.ComponentMemory);
            DrawMemoryRow("Entity Memory", memory.EntityMemory, totalMemory.EntityMemory);
            DrawMemoryRow("Framework Memory", memory.FrameworkMemory, totalMemory.FrameworkMemory);
            
            EditorGUILayout.Space(5);
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Total Memory", EditorStyles.boldLabel, GUILayout.Width(150));
            GUILayout.Label(FormatMemory(memory.TotalMemory), EditorStyles.boldLabel, GUILayout.Width(120));
            GUILayout.Label(FormatMemory(totalMemory.TotalMemory), EditorStyles.boldLabel, GUILayout.Width(120));
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.EndVertical();
        }

        private void DrawMemoryRow(string label, long currentWorldMemory, long allWorldsMemory)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(label, GUILayout.Width(150));
            GUILayout.Label(FormatMemory(currentWorldMemory), GUILayout.Width(120));
            GUILayout.Label(FormatMemory(allWorldsMemory), GUILayout.Width(120));
            EditorGUILayout.EndHorizontal();
        }

        private string FormatMemory(long bytes)
        {
            if (bytes < 1024)
            {
                return $"{bytes} B";
            }
            else if (bytes < 1024 * 1024)
            {
                double kb = bytes / 1024.0;
                return $"{kb:F2} KB";
            }
            else
            {
                double mb = bytes / (1024.0 * 1024.0);
                return $"{mb:F2} MB";
            }
        }

        private string FormatAllocation(long bytes)
        {
            return FormatMemory(bytes);
        }

        private void DrawAllocationTrackingSection()
        {
            EditorGUILayout.LabelField("Allocation Tracking", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("Enter Play Mode to see allocation tracking.", MessageType.Info);
                EditorGUILayout.EndVertical();
                return;
            }
            
            if (_selectedWorld == null)
            {
                EditorGUILayout.HelpBox("No world selected.", MessageType.Info);
                EditorGUILayout.EndVertical();
                return;
            }
            
            if (!PerformanceMonitoring.IsEnabled)
            {
                EditorGUILayout.HelpBox("Performance monitoring is disabled. Enable it to see allocation statistics.", MessageType.Info);
                EditorGUILayout.EndVertical();
                return;
            }
            
            var stats = PerformanceMonitoring.GetAllocationStats(_selectedWorld);
            var totalStats = PerformanceMonitoring.GetTotalAllocationStats();
            
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Allocation Type", EditorStyles.boldLabel, GUILayout.Width(150));
            GUILayout.Label("Current World", EditorStyles.boldLabel, GUILayout.Width(120));
            GUILayout.Label("All Worlds", EditorStyles.boldLabel, GUILayout.Width(120));
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space(2);
            
            DrawAllocationRow("Query Allocations", stats.QueryAllocations, totalStats.QueryAllocations);
            DrawAllocationRow("System Allocations", stats.SystemAllocations, totalStats.SystemAllocations);
            
            EditorGUILayout.Space(5);
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Total Allocations", EditorStyles.boldLabel, GUILayout.Width(150));
            GUILayout.Label(FormatAllocation(stats.TotalAllocations), GUILayout.Width(120));
            GUILayout.Label(FormatAllocation(totalStats.TotalAllocations), GUILayout.Width(120));
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space(5);
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("GC Collected", EditorStyles.boldLabel, GUILayout.Width(150));
            GUILayout.Label(stats.AllocationCount.ToString(), GUILayout.Width(120));
            GUILayout.Label(totalStats.AllocationCount.ToString(), GUILayout.Width(120));
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space(5);
            if (GUILayout.Button("Clear Allocation Stats"))
            {
                PerformanceMonitoring.ClearAllocationStats(_selectedWorld);
            }
            
            EditorGUILayout.EndVertical();
        }

        private void DrawAllocationRow(string label, long currentWorldAllocations, long allWorldsAllocations)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(label, GUILayout.Width(150));
            GUILayout.Label(FormatAllocation(currentWorldAllocations), GUILayout.Width(120));
            GUILayout.Label(FormatAllocation(allWorldsAllocations), GUILayout.Width(120));
            EditorGUILayout.EndHorizontal();
        }

        private void RefreshData()
        {
            UpdateSelectedWorld();
            Repaint();
        }

        private void ExportPerformanceData()
        {
            if (_selectedWorld == null)
            {
                EditorUtility.DisplayDialog("Export Failed", "No world selected. Please select a world to export performance data.", "OK");
                return;
            }

            if (!Application.isPlaying)
            {
                EditorUtility.DisplayDialog("Export Failed", "Performance data is only available in Play Mode. Please enter Play Mode first.", "OK");
                return;
            }

            try
            {
                string logDirectory = Path.Combine(Application.dataPath, "Logs");
                if (!Directory.Exists(logDirectory))
                {
                    Directory.CreateDirectory(logDirectory);
                }

                string filePath = Path.Combine(logDirectory, "PerformanceLog.txt");
                string content = BuildExportContent();

                File.WriteAllText(filePath, content, Encoding.UTF8);
                AssetDatabase.Refresh();

                EditorUtility.DisplayDialog("Export Successful", $"Performance data has been exported to:\n{filePath}", "OK");
            }
            catch (Exception ex)
            {
                EditorUtility.DisplayDialog("Export Failed", $"Failed to export performance data:\n{ex.Message}", "OK");
                Debug.LogError($"Failed to export performance data: {ex}");
            }
        }

        private string BuildExportContent()
        {
            var sb = new StringBuilder();
            string separator = new string('=', 80);
            string lineSeparator = new string('-', 80);

            sb.AppendLine(separator);
            sb.AppendLine("ArtyECS Performance Monitor Export");
            sb.AppendLine(separator);
            sb.AppendLine($"Export Time: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine($"World: {_selectedWorld.Name}");
            sb.AppendLine($"Monitoring Enabled: {(PerformanceMonitoring.IsEnabled ? "Yes" : "No")}");
            sb.AppendLine();

            AppendSystemExecutionTimes(sb, separator, lineSeparator);
            sb.AppendLine();
            AppendQueryPerformance(sb, separator, lineSeparator);
            sb.AppendLine();
            AppendMemoryUsage(sb, separator, lineSeparator);
            sb.AppendLine();
            AppendAllocationTracking(sb, separator, lineSeparator);

            return sb.ToString();
        }

        private void AppendSystemExecutionTimes(StringBuilder sb, string separator, string lineSeparator)
        {
            sb.AppendLine(separator);
            sb.AppendLine("1. SYSTEM EXECUTION TIMES");
            sb.AppendLine(separator);

            var timings = _selectedWorld.GetAllSystemTimings();
            if (timings.Count == 0)
            {
                sb.AppendLine("No system timing data available.");
                return;
            }

            var sortedTimings = timings.OrderBy(t => t.System?.GetType().Name ?? "Unknown")
                .ThenBy(t => t.InsertionOrder)
                .ToList();

            double totalSystemTime = sortedTimings.Sum(t => t.TotalExecutionTime);

            sb.AppendLine("| System Name                                      | Last (ms) | Avg (ms) | Max (ms) | Total (ms) | Total % | Count |");
            sb.AppendLine("|--------------------------------------------------|-----------|----------|----------|------------|---------|-------|");

            foreach (var timing in sortedTimings)
            {
                string systemName = timing.System?.GetType().Name ?? "Unknown";
                double lastTime = timing.LastExecutionTime;
                double avgTime = timing.AverageTime;
                double maxTime = timing.MaxExecutionTime;
                double totalTime = timing.TotalExecutionTime;
                long count = timing.ExecutionCount;
                double totalPercent = totalSystemTime > 0 ? (totalTime / totalSystemTime) * 100.0 : 0.0;

                sb.AppendLine($"| {systemName,-48} | {lastTime,9:F3} | {avgTime,8:F3} | {maxTime,8:F3} | {totalTime,10:F3} | {totalPercent,5:F1}% | {count,5} |");
            }

            sb.AppendLine("|--------------------------------------------------|-----------|----------|----------|------------|---------|-------|");
            long totalCount = sortedTimings.Sum(t => t.ExecutionCount);
            sb.AppendLine($"| TOTAL{' ',43} | {' ',9} | {' ',8} | {' ',8} | {totalSystemTime,10:F3} | 100.0% | {totalCount,5} |");
        }

        private void AppendQueryPerformance(StringBuilder sb, string separator, string lineSeparator)
        {
            sb.AppendLine(separator);
            sb.AppendLine("2. QUERY PERFORMANCE");
            sb.AppendLine(separator);

            var timings = _selectedWorld.GetAllQueryTimings();
            if (timings.Count == 0)
            {
                sb.AppendLine("No query timing data available.");
                return;
            }

            var sortedTimings = timings.OrderBy(t => t.QueryType.ToString())
                .ThenBy(t => t.InsertionOrder)
                .ToList();

            double totalQueryTime = sortedTimings.Sum(t => t.TotalExecutionTime);

            sb.AppendLine("| Query Type                   | Last (ms) | Avg (ms) | Max (ms) | Total (ms) | Total % | Count |");
            sb.AppendLine("|------------------------------|-----------|----------|----------|------------|---------|-------|");

            foreach (var timing in sortedTimings)
            {
                string queryTypeName = timing.QueryType.ToString();
                double lastTime = timing.LastExecutionTime;
                double avgTime = timing.AverageTime;
                double maxTime = timing.MaxExecutionTime;
                double totalTime = timing.TotalExecutionTime;
                long count = timing.ExecutionCount;
                double totalPercent = totalQueryTime > 0 ? (totalTime / totalQueryTime) * 100.0 : 0.0;

                sb.AppendLine($"| {queryTypeName,-28} | {lastTime,9:F3} | {avgTime,8:F3} | {maxTime,8:F3} | {totalTime,10:F3} | {totalPercent,5:F1}% | {count,5} |");
            }

            sb.AppendLine("|------------------------------|-----------|----------|----------|------------|---------|-------|");
            long totalCount = sortedTimings.Sum(t => t.ExecutionCount);
            sb.AppendLine($"| TOTAL{' ',23} | {' ',9} | {' ',8} | {' ',8} | {totalQueryTime,10:F3} | 100.0% | {totalCount,5} |");
        }

        private void AppendMemoryUsage(StringBuilder sb, string separator, string lineSeparator)
        {
            sb.AppendLine(separator);
            sb.AppendLine("3. MEMORY USAGE");
            sb.AppendLine(separator);

            if (!PerformanceMonitoring.IsEnabled)
            {
                sb.AppendLine("Performance monitoring is disabled. Enable it to see memory usage.");
                return;
            }

            var memory = PerformanceMonitoring.GetMemoryUsage(_selectedWorld);
            var totalMemory = PerformanceMonitoring.GetTotalMemoryUsage();

            sb.AppendLine("| Memory Type           | Current World | All Worlds |");
            sb.AppendLine("|-----------------------|---------------|------------|");

            AppendMemoryRow(sb, "Component Memory", memory.ComponentMemory, totalMemory.ComponentMemory);
            AppendMemoryRow(sb, "Entity Memory", memory.EntityMemory, totalMemory.EntityMemory);
            AppendMemoryRow(sb, "Framework Memory", memory.FrameworkMemory, totalMemory.FrameworkMemory);

            sb.AppendLine("|-----------------------|---------------|------------|");
            AppendMemoryRow(sb, "Total Memory", memory.TotalMemory, totalMemory.TotalMemory);
        }

        private void AppendMemoryRow(StringBuilder sb, string label, long currentWorldMemory, long allWorldsMemory)
        {
            string currentFormatted = FormatMemoryForExport(currentWorldMemory);
            string allFormatted = FormatMemoryForExport(allWorldsMemory);
            
            sb.AppendLine($"| {label,-21} | {currentFormatted,13} | {allFormatted,10} |");
        }

        private string FormatMemoryForExport(long bytes)
        {
            if (bytes < 1024)
            {
                return $"{bytes} B";
            }
            else if (bytes < 1024 * 1024)
            {
                double kb = bytes / 1024.0;
                return $"{kb:F2} KB";
            }
            else
            {
                double mb = bytes / (1024.0 * 1024.0);
                return $"{mb:F2} MB";
            }
        }

        private void AppendAllocationTracking(StringBuilder sb, string separator, string lineSeparator)
        {
            sb.AppendLine(separator);
            sb.AppendLine("4. ALLOCATION TRACKING");
            sb.AppendLine(separator);

            if (!PerformanceMonitoring.IsEnabled)
            {
                sb.AppendLine("Performance monitoring is disabled. Enable it to see allocation statistics.");
                return;
            }

            var stats = PerformanceMonitoring.GetAllocationStats(_selectedWorld);
            var totalStats = PerformanceMonitoring.GetTotalAllocationStats();

            sb.AppendLine("| Allocation Type        | Current World | All Worlds |");
            sb.AppendLine("|------------------------|---------------|------------|");

            AppendAllocationRow(sb, "Query Allocations", stats.QueryAllocations, totalStats.QueryAllocations);
            AppendAllocationRow(sb, "System Allocations", stats.SystemAllocations, totalStats.SystemAllocations);

            sb.AppendLine("|------------------------|---------------|------------|");
            AppendAllocationRow(sb, "Total Allocations", stats.TotalAllocations, totalStats.TotalAllocations);
            sb.AppendLine($"| GC Collected Count     | {stats.AllocationCount,13} | {totalStats.AllocationCount,10} |");
        }

        private void AppendAllocationRow(StringBuilder sb, string label, long currentWorldAllocations, long allWorldsAllocations)
        {
            string currentFormatted = FormatMemoryForExport(currentWorldAllocations);
            string allFormatted = FormatMemoryForExport(allWorldsAllocations);
            
            sb.AppendLine($"| {label,-22} | {currentFormatted,13} | {allFormatted,10} |");
        }
    }
}
#endif

