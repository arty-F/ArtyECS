#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using ArtyECS.Core;

namespace ArtyECS.Editor
{
    [CustomEditor(typeof(SystemDisplay))]
    public class SystemDisplayEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            var display = (SystemDisplay)target;
            serializedObject.Update();

            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("System management is only available in Play Mode", MessageType.Info);
                return;
            }

            var system = display.GetSystem();
            var world = display.GetWorld();

            if (system == null || world == null)
            {
                EditorGUILayout.HelpBox("System or world not found", MessageType.Warning);
                return;
            }

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("System Information", EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();
            
            if (DrawDeleteButton("Delete", 60f))
            {
                bool removed = false;
                string queueName = display.GetQueueName();
                if (queueName == "Update")
                {
                    removed = world.RemoveFromUpdate(system);
                }
                else if (queueName == "FixedUpdate")
                {
                    removed = world.RemoveFromFixedUpdate(system);
                }

                if (removed)
                {
                    Debug.Log($"System {system.GetType().Name} removed from {queueName} queue");

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
                    EditorUtility.DisplayDialog("Error", "Failed to remove system", "OK");
                }
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.TextField("System Type", system.GetType().Name);
            EditorGUILayout.TextField("Queue", display.GetQueueName());
            EditorGUI.EndDisabledGroup();
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

