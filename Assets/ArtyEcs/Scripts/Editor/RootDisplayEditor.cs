#if UNITY_EDITOR
using System.Linq;
using UnityEngine;
using UnityEditor;
using ArtyECS.Core;

namespace ArtyECS.Editor
{
    [CustomEditor(typeof(RootDisplay))]
    public class RootDisplayEditor : UnityEditor.Editor
    {
        private string _newWorldName = "";

        public override void OnInspectorGUI()
        {
            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("World management is only available in Play Mode", MessageType.Info);
                return;
            }

            EditorGUILayout.LabelField("World Management", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            DrawWorldCreation();
            EditorGUILayout.Space();
            DrawWorldList();
        }

        private void DrawWorldCreation()
        {
            EditorGUILayout.LabelField("Create New World", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("World Name:", GUILayout.Width(100));
            _newWorldName = EditorGUILayout.TextField(_newWorldName);
            
            if (GUILayout.Button("Create", GUILayout.Width(80)))
            {
                if (string.IsNullOrEmpty(_newWorldName))
                {
                    EditorUtility.DisplayDialog("Error", "World name cannot be empty", "OK");
                    return;
                }

                if (World.Exists(_newWorldName))
                {
                    EditorUtility.DisplayDialog("Error", $"World '{_newWorldName}' already exists", "OK");
                    return;
                }

                var world = World.GetOrCreate(_newWorldName);
                Debug.Log($"World '{_newWorldName}' created successfully");
                
                if (EcsHierarchyManager.Instance != null)
                {
                    var worldGO = EcsHierarchyManager.Instance.GetOrCreateWorldGameObject(world);
                    if (worldGO != null)
                    {
                        EditorGUIUtility.PingObject(worldGO);
                        Selection.activeGameObject = worldGO;
                    }
                }
                
                _newWorldName = "";
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawWorldList()
        {
            EditorGUILayout.LabelField("Existing Worlds", EditorStyles.boldLabel);
            
            var allWorlds = World.GetAllWorlds();
            if (allWorlds.Count == 0)
            {
                EditorGUILayout.HelpBox("No worlds exist", MessageType.Info);
                return;
            }

            foreach (var world in allWorlds)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"World: {world.Name}", EditorStyles.boldLabel);
                
                bool canDelete = world.Name != "Global";
                EditorGUI.BeginDisabledGroup(!canDelete);
                if (GUILayout.Button("Delete", GUILayout.Width(80)))
                {
                    if (EditorUtility.DisplayDialog("Delete World", 
                        $"Are you sure you want to delete world '{world.Name}'? This action cannot be undone.", 
                        "Delete", "Cancel"))
                    {
                        if (World.Destroy(world))
                        {
                            Debug.Log($"World '{world.Name}' deleted successfully");
                            
                            if (EcsHierarchyManager.Instance != null)
                            {
                                EcsHierarchyManager.UnloadWorld(world);
                            }
                        }
                        else
                        {
                            EditorUtility.DisplayDialog("Error", $"Failed to delete world '{world.Name}'", "OK");
                        }
                    }
                }
                EditorGUI.EndDisabledGroup();
                EditorGUILayout.EndHorizontal();

                if (World.Exists(world.Name))
                {
                    int entityCount = world.GetAllEntities().Length;
                    int updateCount = world.GetUpdateQueue().Count;
                    int fixedUpdateCount = world.GetFixedUpdateQueue().Count;
                    
                    EditorGUILayout.LabelField($"Entities: {entityCount} | Systems: {updateCount + fixedUpdateCount} (Update: {updateCount}, FixedUpdate: {fixedUpdateCount})");
                }
                else
                {
                    EditorGUILayout.LabelField("Status: World does not exist");
                }
                
                EditorGUILayout.EndVertical();
            }
        }
    }
}
#endif

