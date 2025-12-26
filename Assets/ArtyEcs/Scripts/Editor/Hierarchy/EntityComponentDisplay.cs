#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using ArtyECS.Core;

namespace ArtyECS.Editor
{
    public class EntityComponentDisplay : MonoBehaviour
    {
        [SerializeField]
        private int _entityId;

        [SerializeField]
        private int _entityGeneration;

        [SerializeField]
        private string _worldName;

        [SerializeField]
        private List<ComponentDisplayData> _components = new List<ComponentDisplayData>();

        private Entity _entity;
        private WorldInstance _world;
        private float _lastRefreshTime;
        private const float REFRESH_INTERVAL = 0.1f;
        
        private Dictionary<string, ComponentSnapshot> _previousComponentState = new Dictionary<string, ComponentSnapshot>();
        private bool _isDirty = false;
        private int _previousComponentCount = 0;

        public void Initialize(Entity entity, WorldInstance world)
        {
            _entity = entity;
            _world = world;
            _entityId = entity.Id;
            _entityGeneration = entity.Generation;
            _worldName = world != null ? world.Name : "Global";
            RefreshComponents();
        }

        private void Awake()
        {
            if (_entityId >= 0)
            {
                _entity = new Entity(_entityId, _entityGeneration);
            }

            if (!string.IsNullOrEmpty(_worldName))
            {
                if (_worldName == "Global")
                {
                    _world = World.GetOrCreate();
                }
                else
                {
                    _world = World.GetOrCreate(_worldName);
                }
            }
        }

        private void Update()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            if (!_entity.IsValid || _world == null)
            {
                return;
            }

            float currentTime = Time.realtimeSinceStartup;
            if (currentTime - _lastRefreshTime >= REFRESH_INTERVAL)
            {
                RefreshComponents();
                _lastRefreshTime = currentTime;
            }
        }

        public void RefreshComponents()
        {
            RefreshFromECS();
        }

        public bool RefreshFromECS()
        {
            if (!_entity.IsValid || _world == null)
            {
                bool hadComponents = _components.Count > 0;
                _components.Clear();
                _previousComponentState.Clear();
                _previousComponentCount = 0;
                if (hadComponents)
                {
                    _isDirty = true;
                    return true;
                }
                return false;
            }

            try
            {
                var componentInfos = _world.GetAllComponentInfos(_entity);
                int currentComponentCount = componentInfos != null ? componentInfos.Length : 0;
                
                bool componentCountChanged = currentComponentCount != _previousComponentCount;
                bool anyChanges = componentCountChanged;
                
                Dictionary<string, ComponentSnapshot> currentState = new Dictionary<string, ComponentSnapshot>();
                
                foreach (var info in componentInfos)
                {
                    string componentKey = info.ComponentType.FullName;
                    ComponentSnapshot currentSnapshot = CreateComponentSnapshot(info);
                    currentState[componentKey] = currentSnapshot;
                    
                    if (_previousComponentState.TryGetValue(componentKey, out ComponentSnapshot previousSnapshot))
                    {
                        if (!AreSnapshotsEqual(currentSnapshot, previousSnapshot))
                        {
                            anyChanges = true;
                        }
                    }
                    else
                    {
                        anyChanges = true;
                    }
                }
                
                foreach (var key in _previousComponentState.Keys)
                {
                    if (!currentState.ContainsKey(key))
                    {
                        anyChanges = true;
                        break;
                    }
                }
                
                if (anyChanges)
                {
                    _components.Clear();
                    
                    foreach (var info in componentInfos)
                    {
                        var displayData = new ComponentDisplayData
                        {
                            ComponentTypeName = info.ComponentType.Name,
                            Fields = new List<FieldDisplayData>()
                        };

                        if (info.Value != null)
                        {
                            var fields = info.ComponentType.GetFields(BindingFlags.Public | BindingFlags.Instance);
                            foreach (var field in fields)
                            {
                                var fieldValue = field.GetValue(info.Value);
                                displayData.Fields.Add(new FieldDisplayData
                                {
                                    FieldName = field.Name,
                                    FieldValue = fieldValue != null ? fieldValue.ToString() : "null",
                                    FieldType = field.FieldType.Name
                                });
                            }
                        }

                        _components.Add(displayData);
                    }
                    
                    _previousComponentState = currentState;
                    _previousComponentCount = currentComponentCount;
                    _isDirty = true;
                    return true;
                }
                
                _isDirty = false;
                return false;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error refreshing components for entity {_entity}: {ex.Message}");
                return false;
            }
        }

        public bool IsDirty()
        {
            return _isDirty;
        }

        public void ClearDirtyFlag()
        {
            _isDirty = false;
        }

        private ComponentSnapshot CreateComponentSnapshot(ComponentInfo componentInfo)
        {
            var snapshot = new ComponentSnapshot
            {
                ComponentTypeName = componentInfo.ComponentType.FullName,
                FieldValues = new Dictionary<string, object>()
            };
            
            if (componentInfo.Value != null)
            {
                var fields = componentInfo.ComponentType.GetFields(BindingFlags.Public | BindingFlags.Instance);
                foreach (var field in fields)
                {
                    var fieldValue = field.GetValue(componentInfo.Value);
                    snapshot.FieldValues[field.Name] = fieldValue;
                }
            }
            
            return snapshot;
        }

        private bool AreSnapshotsEqual(ComponentSnapshot a, ComponentSnapshot b)
        {
            if (a.ComponentTypeName != b.ComponentTypeName)
            {
                return false;
            }
            
            if (a.FieldValues.Count != b.FieldValues.Count)
            {
                return false;
            }
            
            foreach (var kvp in a.FieldValues)
            {
                if (!b.FieldValues.TryGetValue(kvp.Key, out object bValue))
                {
                    return false;
                }
                
                if (!AreValuesEqual(kvp.Value, bValue))
                {
                    return false;
                }
            }
            
            return true;
        }

        private bool AreValuesEqual(object a, object b)
        {
            if (ReferenceEquals(a, b))
            {
                return true;
            }
            
            if (a == null || b == null)
            {
                return false;
            }
            
            Type typeA = a.GetType();
            Type typeB = b.GetType();
            
            if (typeA != typeB)
            {
                return false;
            }
            
            if (typeA.IsValueType)
            {
                return a.Equals(b);
            }
            
            if (a.Equals(b))
            {
                return true;
            }
            
            return a.ToString() == b.ToString();
        }

        public ComponentInfo[] GetComponentInfos()
        {
            if (!_entity.IsValid || _world == null)
            {
                return Array.Empty<ComponentInfo>();
            }

            try
            {
                return _world.GetAllComponentInfos(_entity);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error getting component infos for entity {_entity}: {ex.Message}");
                return Array.Empty<ComponentInfo>();
            }
        }

        public Entity GetEntity()
        {
            return _entity;
        }

        public WorldInstance GetWorld()
        {
            return _world;
        }

        [Serializable]
        public class ComponentDisplayData
        {
            public string ComponentTypeName;
            public List<FieldDisplayData> Fields;
        }

        [Serializable]
        public class FieldDisplayData
        {
            public string FieldName;
            public string FieldValue;
            public string FieldType;
        }

        private class ComponentSnapshot
        {
            public string ComponentTypeName;
            public Dictionary<string, object> FieldValues;
        }
    }
}
#endif

