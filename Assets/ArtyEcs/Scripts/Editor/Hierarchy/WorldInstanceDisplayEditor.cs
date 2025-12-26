#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using ArtyECS.Core;

namespace ArtyECS.Editor
{
    [CustomEditor(typeof(WorldInstanceDisplay))]
    public class WorldInstanceDisplayEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            var display = (WorldInstanceDisplay)target;
            serializedObject.Update();

            EditorGUILayout.LabelField("World Instance Display", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            if (Application.isPlaying)
            {
                DrawWorldInfo(display);
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

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(world.Name ?? "Unknown", EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();
            
            bool isGlobal = ReferenceEquals(world, World.GlobalWorld);
            EditorGUI.BeginDisabledGroup(isGlobal);
            if (DrawDeleteButton("Delete", 60f))
            {
                if (World.Destroy(world))
                {
                    EcsHierarchyManager.UnloadWorld(world);
                    Debug.Log($"World '{world.Name}' deleted");
                    if (EcsHierarchyManager.Instance != null)
                    {
                        EditorApplication.delayCall += () =>
                        {
                            if (EcsHierarchyManager.Instance != null)
                            {
                                EcsHierarchyManager.Instance.UpdateHierarchy();
                            }
                        };
                    }
                }
                else
                {
                    EditorUtility.DisplayDialog("Error", "Failed to delete world", "OK");
                }
            }
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();

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

        private bool DrawDeleteButton(string label, float width = 60f)
        {
            Color originalColor = GUI.color;
            GUI.color = Color.red;
            bool clicked = GUILayout.Button(label, GUILayout.Width(width), GUILayout.Height(20));
            GUI.color = originalColor;
            return clicked;
        }

    }
}
#endif

