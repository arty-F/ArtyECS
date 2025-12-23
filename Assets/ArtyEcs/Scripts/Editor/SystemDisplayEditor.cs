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

            EditorGUILayout.LabelField("System Information", EditorStyles.boldLabel);
            
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.TextField("System Type", system.GetType().Name);
            EditorGUILayout.TextField("Queue", display.GetQueueName());
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.Space();

            if (GUILayout.Button("Remove System", GUILayout.Height(25)))
            {
                bool removed = false;
                if (display.GetQueueName() == "Update")
                {
                    removed = world.RemoveFromUpdate(system);
                }
                else if (display.GetQueueName() == "FixedUpdate")
                {
                    removed = world.RemoveFromFixedUpdate(system);
                }

                if (removed)
                {
                    Debug.Log($"Removed {system.GetType().Name} from {display.GetQueueName()} queue");

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
                    EditorUtility.DisplayDialog("Error", $"Failed to remove system", "OK");
                }
            }
        }
    }
}
#endif

