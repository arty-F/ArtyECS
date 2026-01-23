using System;
using System.Collections.Generic;

namespace ArtyECS.Core
{
    internal static class ComponentsManager
    {
        private static int _componentTypeIdMapKey;
        private static Dictionary<Type, int> _componentTypeIdMap = new();
        private static Dictionary<int, Queue<IComponent>> _poolMap = new(Constants.COMPONENT_TYPES_POOL_CAPACITY);

        internal static int ComponentTypesCount => _componentTypeIdMap.Keys.Count;

        internal static T GetComponent<T>() where T : IComponent, new()
        {
            var componentTypeId = GetComponentTypeId(typeof(T));
            if (!_poolMap.ContainsKey(componentTypeId))
            {
                _poolMap[componentTypeId] = new(Constants.COMPONENT_POOL_CAPACITY);
                _poolMap[componentTypeId].Enqueue(new T());
            }

            var pool = _poolMap[componentTypeId];
            if (pool.Count == 0)
            {
                while (pool.Count < Constants.COMPONENT_POOL_CAPACITY)
                {
                    pool.Enqueue(new T());
                }
            }

            return (T)pool.Dequeue();
        }

        internal static void Release(IComponent component)
        {
            //TODO из компонента
            var componentTypeId = GetComponentTypeId(component.GetType());
            _poolMap[componentTypeId].Enqueue(component);
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
