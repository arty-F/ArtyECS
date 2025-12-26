#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using ArtyECS.Core;

namespace ArtyECS.Editor
{
    [CustomEditor(typeof(SystemsContainerDisplay))]
    public class SystemsContainerDisplayEditor : UnityEditor.Editor
    {
        private int _selectedSystemIndex = 0;
        private string[] _systemTypeNames;
        private Type[] _systemTypes;
        private Dictionary<Type, object> _systemConstructorParams = new Dictionary<Type, object>();

        private void OnEnable()
        {
            DiscoverSystemTypes();
        }

        private void DiscoverSystemTypes()
        {
            var systemTypes = new List<Type>();
            
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                try
                {
                    var types = assembly.GetTypes()
                        .Where(t => t.IsSubclassOf(typeof(SystemHandler)) && !t.IsAbstract && !t.IsInterface)
                        .ToArray();
                    
                    systemTypes.AddRange(types);
                }
                catch (ReflectionTypeLoadException ex)
                {
                    Debug.LogWarning($"Could not load types from assembly {assembly.FullName}: {ex.Message}");
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"Error scanning assembly {assembly.FullName}: {ex.Message}");
                }
            }

            _systemTypes = systemTypes.OrderBy(t => t.Name).ToArray();
            _systemTypeNames = _systemTypes.Select(t => t.Name).ToArray();

            foreach (var systemType in _systemTypes)
            {
                if (!_systemConstructorParams.ContainsKey(systemType))
                {
                    var constructors = systemType.GetConstructors();
                    if (constructors.Length > 0)
                    {
                        var paramConstructor = constructors.FirstOrDefault(c => c.GetParameters().Length > 0);
                        if (paramConstructor != null)
                        {
                            var parameters = paramConstructor.GetParameters();
                            if (parameters.Length == 1 && parameters[0].ParameterType == typeof(int))
                            {
                                _systemConstructorParams[systemType] = 0;
                            }
                        }
                    }
                }
            }
        }

        public override void OnInspectorGUI()
        {
            var display = (SystemsContainerDisplay)target;
            serializedObject.Update();

            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("System management is only available in Play Mode", MessageType.Info);
                return;
            }

            var world = display.GetWorld();
            if (world == null)
            {
                EditorGUILayout.HelpBox("World not found", MessageType.Warning);
                return;
            }

            EditorGUILayout.LabelField("System Management", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            DrawSystemCreation(world);
        }

        private void DrawSystemCreation(WorldInstance world)
        {
            if (_systemTypeNames == null || _systemTypeNames.Length == 0)
            {
                EditorGUILayout.HelpBox("No system types found", MessageType.Warning);
                return;
            }

            _selectedSystemIndex = EditorGUILayout.Popup("System Type:", _selectedSystemIndex, _systemTypeNames);
            
            if (_selectedSystemIndex >= 0 && _selectedSystemIndex < _systemTypes.Length)
            {
                var selectedType = _systemTypes[_selectedSystemIndex];
                DrawSystemParameters(selectedType);
            }

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Add to Update", GUILayout.Height(25)))
            {
                AddSystemToQueue(world, "Update");
            }
            if (GUILayout.Button("Add to FixedUpdate", GUILayout.Height(25)))
            {
                AddSystemToQueue(world, "FixedUpdate");
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawSystemParameters(Type systemType)
        {
            if (_systemConstructorParams.TryGetValue(systemType, out var paramValue))
            {
                if (paramValue is int intValue)
                {
                    _systemConstructorParams[systemType] = EditorGUILayout.IntField("Value:", intValue);
                }
            }
        }

        private void AddSystemToQueue(WorldInstance world, string queueName)
        {
            if (_selectedSystemIndex < 0 || _selectedSystemIndex >= _systemTypes.Length)
            {
                EditorUtility.DisplayDialog("Error", "Please select a system type", "OK");
                return;
            }

            try
            {
                var systemType = _systemTypes[_selectedSystemIndex];
                SystemHandler system = null;

                if (_systemConstructorParams.TryGetValue(systemType, out var paramValue))
                {
                    if (paramValue is int intValue)
                    {
                        system = (SystemHandler)Activator.CreateInstance(systemType, intValue);
                    }
                }
                else
                {
                    system = (SystemHandler)Activator.CreateInstance(systemType);
                }

                if (system != null)
                {
                    if (queueName == "Update")
                    {
                        world.AddToUpdate(system);
                    }
                    else if (queueName == "FixedUpdate")
                    {
                        world.AddToFixedUpdate(system);
                    }
                    else
                    {
                        EditorUtility.DisplayDialog("Error", $"Unknown queue: {queueName}", "OK");
                        return;
                    }

                    Debug.Log($"Added {systemType.Name} to {queueName} queue");

                    if (EcsHierarchyManager.Instance != null)
                    {
                        EcsHierarchyManager.Instance.UpdateHierarchy();
                    }
                }
            }
            catch (Exception ex)
            {
                EditorUtility.DisplayDialog("Error", $"Failed to add system: {ex.Message}", "OK");
            }
        }
    }
}
#endif

