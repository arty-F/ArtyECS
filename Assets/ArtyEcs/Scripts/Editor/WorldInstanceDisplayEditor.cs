#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using ArtyECS.Core;

namespace ArtyECS.Editor
{
    [CustomEditor(typeof(WorldInstanceDisplay))]
    public class WorldInstanceDisplayEditor : UnityEditor.Editor
    {
        private bool _autoRefresh = true;
        private double _lastRefreshTime;
        private const double AUTO_REFRESH_INTERVAL = 0.5;

        public override void OnInspectorGUI()
        {
            var display = (WorldInstanceDisplay)target;
            serializedObject.Update();

            EditorGUILayout.LabelField("World Instance Display", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            if (Application.isPlaying)
            {
                DrawWorldInfo(display);
                EditorGUILayout.Space();
                DrawRefreshControls(display);
            }
            else
            {
                EditorGUILayout.HelpBox("World information is only available in Play Mode", MessageType.Info);
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawWorldInfo(WorldInstanceDisplay display)
        {
            var world = display.GetWorld();
            if (world == null)
            {
                EditorGUILayout.HelpBox("World not found", MessageType.Warning);
                return;
            }

            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.TextField("World Name", world.Name ?? "Unknown");
            
            bool isLoaded = EcsHierarchyManager.IsWorldLoaded(world);
            EditorGUILayout.Toggle("Loaded", isLoaded);
            
            try
            {
                var entities = world.GetAllEntities();
                EditorGUILayout.IntField("Entity Count", entities.Length);
                
                var updateQueue = world.GetUpdateQueue();
                EditorGUILayout.IntField("Update Systems", updateQueue.Count);
                
                var fixedUpdateQueue = world.GetFixedUpdateQueue();
                EditorGUILayout.IntField("FixedUpdate Systems", fixedUpdateQueue.Count);
            }
            catch (System.Exception ex)
            {
                EditorGUILayout.HelpBox($"Error getting world info: {ex.Message}", MessageType.Error);
            }
            
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            if (isLoaded)
            {
                if (GUILayout.Button("Unload Contents", GUILayout.Height(25)))
                {
                    EcsHierarchyManager.UnloadWorld(world);
                    Repaint();
                }
            }
            else
            {
                if (GUILayout.Button("Load Contents", GUILayout.Height(25)))
                {
                    EcsHierarchyManager.LoadWorld(world);
                    Repaint();
                }
            }
            EditorGUILayout.EndHorizontal();

            if (!isLoaded)
            {
                EditorGUILayout.HelpBox("World contents are not loaded. Click 'Load Contents' to display entities and systems in hierarchy.", MessageType.Info);
            }
        }

        private void DrawRefreshControls(WorldInstanceDisplay display)
        {
            EditorGUILayout.BeginHorizontal();
            
            _autoRefresh = EditorGUILayout.Toggle("Auto Refresh", _autoRefresh);
            
            if (GUILayout.Button("Refresh Now", GUILayout.Width(100)))
            {
                Repaint();
            }

            if (_autoRefresh)
            {
                double currentTime = EditorApplication.timeSinceStartup;
                if (currentTime - _lastRefreshTime >= AUTO_REFRESH_INTERVAL)
                {
                    _lastRefreshTime = currentTime;
                    Repaint();
                }
            }
            
            EditorGUILayout.EndHorizontal();
        }
    }
}
#endif

