#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
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
            
            var sortedTimings = timings.OrderByDescending(t => t.LastExecutionTime).ToList();
            
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("System Name", EditorStyles.boldLabel, GUILayout.Width(200));
            GUILayout.Label("Last (ms)", EditorStyles.boldLabel, GUILayout.Width(80));
            GUILayout.Label("Avg (ms)", EditorStyles.boldLabel, GUILayout.Width(80));
            GUILayout.Label("Total (ms)", EditorStyles.boldLabel, GUILayout.Width(100));
            GUILayout.Label("Count", EditorStyles.boldLabel, GUILayout.Width(60));
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space(2);
            
            foreach (var timing in sortedTimings)
            {
                DrawSystemTimingRow(timing);
            }
            
            EditorGUILayout.EndVertical();
        }
        
        private void DrawSystemTimingRow(SystemTimingData timing)
        {
            EditorGUILayout.BeginHorizontal();
            
            string systemName = timing.System?.GetType().Name ?? "Unknown";
            GUILayout.Label(systemName, GUILayout.Width(200));
            
            double lastTime = timing.LastExecutionTime;
            double avgTime = timing.AverageTime;
            double totalTime = timing.TotalExecutionTime;
            long count = timing.ExecutionCount;
            
            Color originalColor = GUI.color;
            
            if (lastTime > 10.0)
            {
                GUI.color = Color.red;
            }
            else if (lastTime > 5.0)
            {
                GUI.color = Color.yellow;
            }
            else
            {
                GUI.color = Color.green;
            }
            
            GUILayout.Label($"{lastTime:F3}", GUILayout.Width(80));
            GUI.color = originalColor;
            
            GUILayout.Label($"{avgTime:F3}", GUILayout.Width(80));
            GUILayout.Label($"{totalTime:F2}", GUILayout.Width(100));
            GUILayout.Label(count.ToString(), GUILayout.Width(60));
            
            EditorGUILayout.EndHorizontal();
        }

        private void DrawQueryPerformanceSection()
        {
            EditorGUILayout.LabelField("Query Performance", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            EditorGUILayout.HelpBox("No data available. Metrics will be displayed here in subsequent tasks.", MessageType.Info);
            
            EditorGUILayout.EndVertical();
        }

        private void DrawMemoryUsageSection()
        {
            EditorGUILayout.LabelField("Memory Usage", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            EditorGUILayout.HelpBox("No data available. Metrics will be displayed here in subsequent tasks.", MessageType.Info);
            
            EditorGUILayout.EndVertical();
        }

        private void DrawAllocationTrackingSection()
        {
            EditorGUILayout.LabelField("Allocation Tracking", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            EditorGUILayout.HelpBox("No data available. Metrics will be displayed here in subsequent tasks.", MessageType.Info);
            
            EditorGUILayout.EndVertical();
        }

        private void RefreshData()
        {
            UpdateSelectedWorld();
            Repaint();
        }
    }
}
#endif

