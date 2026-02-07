using System;
using System.Collections.Generic;

namespace ArtyECS.Core
{
    internal static class ComponentsManager
    {
        private static int _componentTypeIdMapKey;
        private static Dictionary<Type, int> _componentTypeIdMap = new();
        private static Dictionary<int, Queue<Component>> _poolMap = new(Constants.COMPONENT_TYPES_POOL_CAPACITY);

        internal static int ComponentTypesCount => _componentTypeIdMap.Keys.Count;

        internal static T GetComponent<T>(Entity entity) where T : Component, new()
        {
            var componentTypeId = GetComponentTypeId(typeof(T));
            if (!_poolMap.ContainsKey(componentTypeId))
            {
                _poolMap[componentTypeId] = new(Constants.COMPONENT_POOL_CAPACITY);
                _poolMap[componentTypeId].Enqueue(CreateNewComponent<T>(componentTypeId));
            }

            var pool = _poolMap[componentTypeId];
            if (pool.Count == 0)
            {
                while (pool.Count < Constants.COMPONENT_POOL_CAPACITY)
                {
                    pool.Enqueue(CreateNewComponent<T>(componentTypeId));
                }
            }

            var component = (T)pool.Dequeue();
            component.SetEntity(entity);
            return component;
        }

        private static Component CreateNewComponent<T>(int typeId) where T : Component, new()
        {
            var component = new T();
            component.TypeId = typeId;
            return component;
        }

        internal static void Release(Component component)
        {
            _poolMap[component.TypeId].Enqueue(component);
        }

        internal static int GetComponentTypeId(Type componentType)
        {
            if (!_componentTypeIdMap.ContainsKey(componentType))
            {
                _componentTypeIdMap.Add(componentType, _componentTypeIdMapKey++);
            }

            return _componentTypeIdMap[componentType];
        }

        internal static void Clear()
        {
            _componentTypeIdMapKey = 0;
            _componentTypeIdMap.Clear();
            _poolMap.Clear();
        }
    }
}
