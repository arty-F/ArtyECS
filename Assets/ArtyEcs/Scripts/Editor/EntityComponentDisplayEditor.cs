#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using ArtyECS.Core;

namespace ArtyECS.Editor
{
    [CustomEditor(typeof(EntityComponentDisplay))]
    public class EntityComponentDisplayEditor : UnityEditor.Editor
    {
        private Dictionary<string, bool> _componentFoldouts = new Dictionary<string, bool>();
        private Dictionary<string, Color> _modifiedFields = new Dictionary<string, Color>();
        private double _lastFlashTime;
        private const double FLASH_DURATION = 0.5;

        private bool _showAddComponent = false;
        private int _selectedComponentIndex = 0;
        private string[] _componentTypeNames;
        private Type[] _componentTypes;
        private Dictionary<Type, Dictionary<string, object>> _componentFieldValues = new Dictionary<Type, Dictionary<string, object>>();

        public override void OnInspectorGUI()
        {
            var display = (EntityComponentDisplay)target;
            serializedObject.Update();

            if (Application.isPlaying)
            {
                bool hasChanges = display.RefreshFromECS();
                if (hasChanges)
                {
                    Repaint();
                }
            }

            EditorGUILayout.LabelField("Entity Component Display", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            DrawEntityInfo(display);
            EditorGUILayout.Space();

            DrawComponents(display);
            EditorGUILayout.Space();

            DrawAddComponent(display);

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawEntityInfo(EntityComponentDisplay display)
        {
            EditorGUI.BeginDisabledGroup(true);
            var entity = display.GetEntity();
            EditorGUILayout.IntField("Entity ID", entity.Id);
            EditorGUILayout.IntField("Entity Generation", entity.Generation);
            EditorGUILayout.TextField("World", display.GetWorld()?.Name ?? "None");
            EditorGUI.EndDisabledGroup();
        }

        private void DrawComponents(EntityComponentDisplay display)
        {
            EditorGUILayout.LabelField("Components", EditorStyles.boldLabel);

            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("Enter Play Mode to view component values", MessageType.Info);
                return;
            }

            var componentInfos = display.GetComponentInfos();
            int componentCount = componentInfos != null ? componentInfos.Length : 0;

            EditorGUILayout.LabelField($"Component Count: {componentCount}", EditorStyles.miniLabel);
            EditorGUILayout.Space();

            if (componentCount == 0)
            {
                EditorGUILayout.HelpBox("No components attached to this entity.", MessageType.Info);
                return;
            }

            foreach (var componentInfo in componentInfos)
            {
                DrawComponent(componentInfo);
            }
        }

        private void DrawComponent(ComponentInfo componentInfo)
        {
            if (componentInfo.Value == null)
            {
                return;
            }

            string componentKey = componentInfo.ComponentType.FullName;
            if (!_componentFoldouts.ContainsKey(componentKey))
            {
                _componentFoldouts[componentKey] = true;
            }

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            _componentFoldouts[componentKey] = EditorGUILayout.Foldout(
                _componentFoldouts[componentKey],
                componentInfo.ComponentType.Name,
                true
            );

            if (_componentFoldouts[componentKey])
            {
                EditorGUI.indentLevel++;
                DrawComponentFields(componentInfo.Value, componentInfo.ComponentType, componentInfo, componentKey);
                EditorGUI.indentLevel--;
                
                EditorGUILayout.Space(5);
                DrawComponentSystemInfo(componentInfo, componentKey);
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }

        private void DrawComponentSystemInfo(ComponentInfo componentInfo, string componentKey)
        {
            var display = (EntityComponentDisplay)target;
            var world = display.GetWorld();

            if (world == null || !Application.isPlaying)
            {
                return;
            }

            EditorGUILayout.LabelField("Systems:", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            
            try
            {
                var componentSystems = ComponentQueueAnalyzer.GetComponentSystems(world, componentInfo.ComponentType);
                
                if (componentSystems.Count == 0)
                {
                    EditorGUILayout.LabelField("Not processed", EditorStyles.miniLabel);
                }
                else
                {
                    foreach (var kvp in componentSystems)
                    {
                        string queueName = kvp.Key;
                        var systems = kvp.Value;
                        
                        foreach (var system in systems)
                        {
                            EditorGUILayout.LabelField($"- {system.GetType().Name} ({queueName} Queue)", EditorStyles.miniLabel);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                EditorGUILayout.HelpBox($"Error analyzing systems: {ex.Message}", MessageType.Error);
            }
            
            EditorGUI.indentLevel--;
        }

        private void DrawComponentFields(object componentValue, Type componentType, ComponentInfo componentInfo, string componentKey)
        {
            var fields = componentType.GetFields(BindingFlags.Public | BindingFlags.Instance);
            
            foreach (var field in fields)
            {
                string fieldKey = $"{componentKey}.{field.Name}";
                DrawField(componentValue, field, componentInfo, fieldKey);
            }
        }

        private void DrawField(object componentValue, FieldInfo field, ComponentInfo componentInfo, string fieldKey)
        {
            object fieldValue = field.GetValue(componentValue);
            Type fieldType = field.FieldType;
            bool isPlayMode = Application.isPlaying;
            bool wasModified = _modifiedFields.ContainsKey(fieldKey);

            if (wasModified)
            {
                double currentTime = EditorApplication.timeSinceStartup;
                if (currentTime - _lastFlashTime > FLASH_DURATION)
                {
                    _modifiedFields.Remove(fieldKey);
                }
                else
                {
                    Color originalColor = GUI.color;
                    GUI.color = Color.Lerp(Color.yellow, originalColor, (float)((currentTime - _lastFlashTime) / FLASH_DURATION));
                }
            }

            if (!isPlayMode)
            {
                EditorGUI.BeginDisabledGroup(true);
            }

            EditorGUI.BeginChangeCheck();

            object newValue = null;

            if (fieldType == typeof(int))
            {
                int oldValue = (int)fieldValue;
                newValue = EditorGUILayout.IntField(field.Name, oldValue);
            }
            else if (fieldType == typeof(float))
            {
                float oldValue = (float)fieldValue;
                newValue = EditorGUILayout.FloatField(field.Name, oldValue);
            }
            else if (fieldType == typeof(double))
            {
                double oldValue = (double)fieldValue;
                newValue = EditorGUILayout.DoubleField(field.Name, oldValue);
            }
            else if (fieldType == typeof(bool))
            {
                bool oldValue = (bool)fieldValue;
                newValue = EditorGUILayout.Toggle(field.Name, oldValue);
            }
            else if (fieldType == typeof(string))
            {
                string oldValue = (string)fieldValue ?? "";
                newValue = EditorGUILayout.TextField(field.Name, oldValue);
            }
            else if (fieldType == typeof(Vector2))
            {
                Vector2 oldValue = (Vector2)fieldValue;
                newValue = EditorGUILayout.Vector2Field(field.Name, oldValue);
            }
            else if (fieldType == typeof(Vector3))
            {
                Vector3 oldValue = (Vector3)fieldValue;
                newValue = EditorGUILayout.Vector3Field(field.Name, oldValue);
            }
            else if (fieldType == typeof(Vector4))
            {
                Vector4 oldValue = (Vector4)fieldValue;
                newValue = EditorGUILayout.Vector4Field(field.Name, oldValue);
            }
            else if (fieldType == typeof(Quaternion))
            {
                Quaternion quat = (Quaternion)fieldValue;
                Vector4 oldValue = new Vector4(quat.x, quat.y, quat.z, quat.w);
                Vector4 newVector4Value = EditorGUILayout.Vector4Field(field.Name, oldValue);
                newValue = new Quaternion(newVector4Value.x, newVector4Value.y, newVector4Value.z, newVector4Value.w);
            }
            else if (fieldType == typeof(Color))
            {
                Color oldValue = (Color)fieldValue;
                newValue = EditorGUILayout.ColorField(field.Name, oldValue);
            }
            else if (fieldType == typeof(Color32))
            {
                Color32 oldValue = (Color32)fieldValue;
                Color newColorValue = EditorGUILayout.ColorField(field.Name, oldValue);
                newValue = (Color32)newColorValue;
            }
            else if (fieldType == typeof(Rect))
            {
                Rect oldValue = (Rect)fieldValue;
                newValue = EditorGUILayout.RectField(field.Name, oldValue);
            }
            else if (fieldType == typeof(Bounds))
            {
                Bounds oldValue = (Bounds)fieldValue;
                newValue = EditorGUILayout.BoundsField(field.Name, oldValue);
            }
            else if (fieldType.IsArray)
            {
                DrawArrayField(field.Name, fieldValue, fieldType);
            }
            else if (fieldType.IsGenericType && typeof(IList).IsAssignableFrom(fieldType))
            {
                DrawListField(field.Name, fieldValue, fieldType);
            }
            else if (fieldType.IsValueType && !fieldType.IsPrimitive && fieldType != typeof(decimal))
            {
                DrawEditableStructField(field.Name, fieldValue, fieldType, componentInfo, fieldKey, componentValue, field);
            }
            else
            {
                EditorGUILayout.TextField(field.Name, fieldValue != null ? fieldValue.ToString() : "null");
            }

            bool valueChanged = isPlayMode && EditorGUI.EndChangeCheck() && newValue != null;

            if (!isPlayMode)
            {
                EditorGUI.EndDisabledGroup();
            }

            if (valueChanged)
            {
                ApplyComponentFieldChange(componentInfo, field, newValue, fieldKey);
            }

            if (wasModified)
            {
                GUI.color = Color.white;
            }
        }

        private void ApplyComponentFieldChange(ComponentInfo componentInfo, FieldInfo field, object newFieldValue, string fieldKey)
        {
            try
            {
                var display = (EntityComponentDisplay)target;
                var entity = display.GetEntity();
                var world = display.GetWorld();

                if (!entity.IsValid || world == null)
                {
                    Debug.LogWarning("Cannot apply component change: Invalid entity or world");
                    return;
                }

                object currentComponent = componentInfo.Value;
                if (currentComponent == null)
                {
                    Debug.LogWarning("Cannot apply component change: Component value is null");
                    return;
                }

                field.SetValue(currentComponent, newFieldValue);

                Type componentType = componentInfo.ComponentType;
                
                MethodInfo removeComponentMethod = typeof(WorldInstance).GetMethod("RemoveComponent").MakeGenericMethod(componentType);
                removeComponentMethod.Invoke(world, new object[] { entity });
                
                MethodInfo addComponentMethod = typeof(WorldInstance).GetMethod("AddComponent").MakeGenericMethod(componentType);
                addComponentMethod.Invoke(world, new object[] { entity, currentComponent });

                _modifiedFields[fieldKey] = Color.yellow;
                _lastFlashTime = EditorApplication.timeSinceStartup;
                
                bool hasChanges = display.RefreshFromECS();
                if (hasChanges)
                {
                    Repaint();
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error applying component field change: {ex.Message}\n{ex.StackTrace}");
            }
        }

        private void DrawArrayField(string fieldName, object arrayValue, Type arrayType)
        {
            if (arrayValue == null)
            {
                EditorGUILayout.LabelField(fieldName, "null");
                return;
            }

            Array array = (Array)arrayValue;
            string foldoutKey = $"{fieldName}_array";
            
            if (!_componentFoldouts.ContainsKey(foldoutKey))
            {
                _componentFoldouts[foldoutKey] = false;
            }

            _componentFoldouts[foldoutKey] = EditorGUILayout.Foldout(
                _componentFoldouts[foldoutKey],
                $"{fieldName} [{array.Length}]",
                true
            );

            if (_componentFoldouts[foldoutKey])
            {
                EditorGUI.indentLevel++;
                Type elementType = arrayType.GetElementType();
                
                for (int i = 0; i < array.Length; i++)
                {
                    object elementValue = array.GetValue(i);
                    DrawFieldValue($"[{i}]", elementValue, elementType);
                }
                
                EditorGUI.indentLevel--;
            }
        }

        private void DrawListField(string fieldName, object listValue, Type listType)
        {
            if (listValue == null)
            {
                EditorGUILayout.LabelField(fieldName, "null");
                return;
            }

            IList list = (IList)listValue;
            string foldoutKey = $"{fieldName}_list";
            
            if (!_componentFoldouts.ContainsKey(foldoutKey))
            {
                _componentFoldouts[foldoutKey] = false;
            }

            _componentFoldouts[foldoutKey] = EditorGUILayout.Foldout(
                _componentFoldouts[foldoutKey],
                $"{fieldName} [{list.Count}]",
                true
            );

            if (_componentFoldouts[foldoutKey])
            {
                EditorGUI.indentLevel++;
                Type elementType = listType.GetGenericArguments()[0];
                
                for (int i = 0; i < list.Count; i++)
                {
                    object elementValue = list[i];
                    DrawFieldValue($"[{i}]", elementValue, elementType);
                }
                
                EditorGUI.indentLevel--;
            }
        }

        private void DrawEditableStructField(string fieldName, object structValue, Type structType, ComponentInfo componentInfo, string parentFieldKey, object parentComponent, FieldInfo parentField)
        {
            if (structValue == null)
            {
                EditorGUILayout.LabelField(fieldName, "null");
                return;
            }

            bool isPlayMode = Application.isPlaying;
            string foldoutKey = $"{parentFieldKey}_struct";
            
            if (!_componentFoldouts.ContainsKey(foldoutKey))
            {
                _componentFoldouts[foldoutKey] = false;
            }

            if (!isPlayMode)
            {
                EditorGUI.BeginDisabledGroup(true);
            }

            _componentFoldouts[foldoutKey] = EditorGUILayout.Foldout(
                _componentFoldouts[foldoutKey],
                fieldName,
                true
            );

            if (_componentFoldouts[foldoutKey])
            {
                EditorGUI.indentLevel++;
                var fields = structType.GetFields(BindingFlags.Public | BindingFlags.Instance);
                
                object modifiedStruct = structValue;
                bool structModified = false;

                foreach (var field in fields)
                {
                    string nestedFieldKey = $"{parentFieldKey}.{field.Name}";
                    object fieldValue = field.GetValue(modifiedStruct);
                    
                    EditorGUI.BeginChangeCheck();
                    
                    object newValue = DrawEditableFieldValue(field.Name, fieldValue, field.FieldType);
                    
                    if (isPlayMode && EditorGUI.EndChangeCheck() && newValue != null)
                    {
                        field.SetValue(modifiedStruct, newValue);
                        structModified = true;
                    }
                }

                if (structModified)
                {
                    parentField.SetValue(parentComponent, modifiedStruct);
                    ApplyComponentFieldChange(componentInfo, parentField, modifiedStruct, parentFieldKey);
                }
                
                EditorGUI.indentLevel--;
            }

            if (!isPlayMode)
            {
                EditorGUI.EndDisabledGroup();
            }
        }

        private object DrawEditableFieldValue(string label, object value, Type valueType)
        {
            if (valueType == typeof(int))
            {
                return EditorGUILayout.IntField(label, (int)value);
            }
            else if (valueType == typeof(float))
            {
                return EditorGUILayout.FloatField(label, (float)value);
            }
            else if (valueType == typeof(double))
            {
                return EditorGUILayout.DoubleField(label, (double)value);
            }
            else if (valueType == typeof(bool))
            {
                return EditorGUILayout.Toggle(label, (bool)value);
            }
            else if (valueType == typeof(string))
            {
                return EditorGUILayout.TextField(label, (string)value ?? "");
            }
            else if (valueType == typeof(Vector2))
            {
                return EditorGUILayout.Vector2Field(label, (Vector2)value);
            }
            else if (valueType == typeof(Vector3))
            {
                return EditorGUILayout.Vector3Field(label, (Vector3)value);
            }
            else if (valueType == typeof(Vector4))
            {
                return EditorGUILayout.Vector4Field(label, (Vector4)value);
            }
            else if (valueType == typeof(Quaternion))
            {
                Quaternion quat = (Quaternion)value;
                Vector4 vec4 = EditorGUILayout.Vector4Field(label, new Vector4(quat.x, quat.y, quat.z, quat.w));
                return new Quaternion(vec4.x, vec4.y, vec4.z, vec4.w);
            }
            else if (valueType == typeof(Color))
            {
                return EditorGUILayout.ColorField(label, (Color)value);
            }
            else if (valueType == typeof(Color32))
            {
                Color color = EditorGUILayout.ColorField(label, (Color32)value);
                return (Color32)color;
            }
            else if (valueType == typeof(Rect))
            {
                return EditorGUILayout.RectField(label, (Rect)value);
            }
            else if (valueType == typeof(Bounds))
            {
                return EditorGUILayout.BoundsField(label, (Bounds)value);
            }
            else
            {
                EditorGUILayout.TextField(label, value != null ? value.ToString() : "null");
                return null;
            }
        }

        private void DrawFieldValue(string label, object value, Type valueType)
        {
            EditorGUI.BeginDisabledGroup(true);

            if (valueType == typeof(int))
            {
                EditorGUILayout.IntField(label, (int)value);
            }
            else if (valueType == typeof(float))
            {
                EditorGUILayout.FloatField(label, (float)value);
            }
            else if (valueType == typeof(double))
            {
                EditorGUILayout.DoubleField(label, (double)value);
            }
            else if (valueType == typeof(bool))
            {
                EditorGUILayout.Toggle(label, (bool)value);
            }
            else if (valueType == typeof(string))
            {
                EditorGUILayout.TextField(label, (string)value ?? "");
            }
            else if (valueType == typeof(Vector2))
            {
                EditorGUILayout.Vector2Field(label, (Vector2)value);
            }
            else if (valueType == typeof(Vector3))
            {
                EditorGUILayout.Vector3Field(label, (Vector3)value);
            }
            else if (valueType == typeof(Vector4))
            {
                EditorGUILayout.Vector4Field(label, (Vector4)value);
            }
            else if (valueType == typeof(Quaternion))
            {
                Quaternion quat = (Quaternion)value;
                EditorGUILayout.Vector4Field(label, new Vector4(quat.x, quat.y, quat.z, quat.w));
            }
            else if (valueType == typeof(Color))
            {
                EditorGUILayout.ColorField(label, (Color)value);
            }
            else if (valueType == typeof(Color32))
            {
                EditorGUILayout.ColorField(label, (Color32)value);
            }
            else if (valueType == typeof(Rect))
            {
                EditorGUILayout.RectField(label, (Rect)value);
            }
            else if (valueType == typeof(Bounds))
            {
                EditorGUILayout.BoundsField(label, (Bounds)value);
            }
            else
            {
                EditorGUILayout.TextField(label, value != null ? value.ToString() : "null");
            }

            EditorGUI.EndDisabledGroup();
        }

        private void DrawAddComponent(EntityComponentDisplay display)
        {
            if (!Application.isPlaying)
            {
                return;
            }

            var entity = display.GetEntity();
            var world = display.GetWorld();

            if (!entity.IsValid || world == null)
            {
                return;
            }

            if (GUILayout.Button(_showAddComponent ? "Hide" : "Add Component", GUILayout.Height(25)))
            {
                _showAddComponent = !_showAddComponent;
                if (_showAddComponent && _componentTypes == null)
                {
                    DiscoverComponentTypes();
                }
            }

            if (_showAddComponent)
            {
                DrawComponentSelection(display, entity, world);
            }
        }

        private void DiscoverComponentTypes()
        {
            var componentTypes = new List<Type>();

            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                try
                {
                    var types = assembly.GetTypes()
                        .Where(t => t.IsValueType && typeof(IComponent).IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface)
                        .ToArray();

                    componentTypes.AddRange(types);
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

            _componentTypes = componentTypes.OrderBy(t => t.Name).ToArray();
            _componentTypeNames = _componentTypes.Select(t => t.Name).ToArray();
        }

        private void DrawComponentSelection(EntityComponentDisplay display, Entity entity, WorldInstance world)
        {
            if (_componentTypeNames == null || _componentTypeNames.Length == 0)
            {
                EditorGUILayout.HelpBox("No component types found", MessageType.Warning);
                return;
            }

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            _selectedComponentIndex = EditorGUILayout.Popup("Component Type:", _selectedComponentIndex, _componentTypeNames);

            if (_selectedComponentIndex >= 0 && _selectedComponentIndex < _componentTypes.Length)
            {
                var selectedType = _componentTypes[_selectedComponentIndex];
                DrawComponentFields(selectedType);
            }

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Add Component"))
            {
                AddComponentToEntity(display, entity, world);
            }
            if (GUILayout.Button("Cancel"))
            {
                _showAddComponent = false;
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }

        private void DrawComponentFields(Type componentType)
        {
            if (!_componentFieldValues.ContainsKey(componentType))
            {
                _componentFieldValues[componentType] = new Dictionary<string, object>();
            }

            var fields = componentType.GetFields(BindingFlags.Public | BindingFlags.Instance);
            foreach (var field in fields)
            {
                string fieldKey = field.Name;
                Type fieldType = field.FieldType;

                if (!_componentFieldValues[componentType].ContainsKey(fieldKey))
                {
                    _componentFieldValues[componentType][fieldKey] = GetDefaultValue(fieldType);
                }

                object currentValue = _componentFieldValues[componentType][fieldKey];
                object newValue = DrawComponentFieldValue(field.Name, currentValue, fieldType);
                _componentFieldValues[componentType][fieldKey] = newValue;
            }
        }

        private object GetDefaultValue(Type type)
        {
            if (type.IsValueType)
            {
                return Activator.CreateInstance(type);
            }
            return null;
        }

        private object DrawComponentFieldValue(string label, object value, Type valueType)
        {
            if (valueType == typeof(int))
            {
                return EditorGUILayout.IntField(label, (int)value);
            }
            else if (valueType == typeof(float))
            {
                return EditorGUILayout.FloatField(label, (float)value);
            }
            else if (valueType == typeof(double))
            {
                return EditorGUILayout.DoubleField(label, (double)value);
            }
            else if (valueType == typeof(bool))
            {
                return EditorGUILayout.Toggle(label, (bool)value);
            }
            else if (valueType == typeof(string))
            {
                return EditorGUILayout.TextField(label, (string)value ?? "");
            }
            else if (valueType == typeof(Vector2))
            {
                return EditorGUILayout.Vector2Field(label, (Vector2)value);
            }
            else if (valueType == typeof(Vector3))
            {
                return EditorGUILayout.Vector3Field(label, (Vector3)value);
            }
            else if (valueType == typeof(Vector4))
            {
                return EditorGUILayout.Vector4Field(label, (Vector4)value);
            }
            else if (valueType == typeof(Quaternion))
            {
                Quaternion quat = (Quaternion)value;
                Vector4 vec4 = EditorGUILayout.Vector4Field(label, new Vector4(quat.x, quat.y, quat.z, quat.w));
                return new Quaternion(vec4.x, vec4.y, vec4.z, vec4.w);
            }
            else if (valueType == typeof(Color))
            {
                return EditorGUILayout.ColorField(label, (Color)value);
            }
            else if (valueType == typeof(Color32))
            {
                Color color = EditorGUILayout.ColorField(label, (Color32)value);
                return (Color32)color;
            }
            else if (valueType == typeof(Rect))
            {
                return EditorGUILayout.RectField(label, (Rect)value);
            }
            else if (valueType == typeof(Bounds))
            {
                return EditorGUILayout.BoundsField(label, (Bounds)value);
            }
            else
            {
                EditorGUILayout.LabelField(label, value != null ? value.ToString() : "null");
                return value;
            }
        }

        private void AddComponentToEntity(EntityComponentDisplay display, Entity entity, WorldInstance world)
        {
            if (_selectedComponentIndex < 0 || _selectedComponentIndex >= _componentTypes.Length)
            {
                EditorUtility.DisplayDialog("Error", "Please select a component type", "OK");
                return;
            }

            try
            {
                var componentType = _componentTypes[_selectedComponentIndex];
                object componentInstance = Activator.CreateInstance(componentType);

                if (_componentFieldValues.TryGetValue(componentType, out var fieldValues))
                {
                    var fields = componentType.GetFields(BindingFlags.Public | BindingFlags.Instance);
                    foreach (var field in fields)
                    {
                        if (fieldValues.TryGetValue(field.Name, out var fieldValue))
                        {
                            field.SetValue(componentInstance, fieldValue);
                        }
                    }
                }

                var addMethod = typeof(WorldInstance).GetMethod("AddComponent").MakeGenericMethod(componentType);
                addMethod.Invoke(world, new object[] { entity, componentInstance });

                Debug.Log($"Added {componentType.Name} to entity {entity.Id}_Gen{entity.Generation}");

                display.RefreshFromECS();
                Repaint();
            }
            catch (Exception ex)
            {
                EditorUtility.DisplayDialog("Error", $"Failed to add component: {ex.Message}", "OK");
            }
        }
    }
}
#endif
