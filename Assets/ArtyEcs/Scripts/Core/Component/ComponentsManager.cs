using System;
using System.Collections.Generic;

namespace ArtyECS.Core
{
    internal static class ComponentsManager
    {
        private static int _componentTypeIdMapKey;
        private static Dictionary<Type, int> _componentTypeIdMap = new();

        internal static int ComponentTypesCount => _componentTypeIdMap.Keys.Count;

        //TODO component pool?

        internal static int GetComponentTypeId(IComponent component)
        {
            return GetComponentTypeId(component.GetType());
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
        }
    }
}
