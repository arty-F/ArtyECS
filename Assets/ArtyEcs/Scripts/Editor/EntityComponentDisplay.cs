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
            if (!_entity.IsValid || _world == null)
            {
                _components.Clear();
                return;
            }

            try
            {
                var componentInfos = _world.GetAllComponentInfos(_entity);
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
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error refreshing components for entity {_entity}: {ex.Message}");
            }
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
    }
}
#endif

