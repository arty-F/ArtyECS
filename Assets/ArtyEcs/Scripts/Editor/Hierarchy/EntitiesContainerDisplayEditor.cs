#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using ArtyECS.Core;

namespace ArtyECS.Editor
{
    [CustomEditor(typeof(EntitiesContainerDisplay))]
    public class EntitiesContainerDisplayEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            var display = (EntitiesContainerDisplay)target;
            serializedObject.Update();

            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("Entity management is only available in Play Mode", MessageType.Info);
                return;
            }

            var world = display.GetWorld();
            if (world == null)
            {
                EditorGUILayout.HelpBox("World not found", MessageType.Warning);
                return;
            }

            EditorGUILayout.LabelField("Entity Management", EditorStyles.boldLabel);

            if (GUILayout.Button("Create Entity", GUILayout.Height(25)))
            {
                var entity = world.CreateEntity();
                Debug.Log($"Created entity: {entity.Id}_Gen{entity.Generation}");

                if (EcsHierarchyManager.Instance != null)
                {
                    var hierarchyManager = EcsHierarchyManager.Instance;
                    var entityGO = hierarchyManager.ForceCreateEntityGameObject(entity, world);

                    if (entityGO != null)
                    {
                        EditorGUIUtility.PingObject(entityGO);
                        Selection.activeGameObject = entityGO;
                    }
                }
            }
        }
    }
}
#endif

