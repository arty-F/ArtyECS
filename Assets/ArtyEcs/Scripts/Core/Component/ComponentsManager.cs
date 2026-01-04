using System;
using System.Collections.Generic;

namespace ArtyECS.Core
{
    internal static class ComponentsManager
    {
        private static int _componentTypeIdMapKey;
        private static Dictionary<Type, int> _componentTypeIdMap = new();

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

        internal static Archetype GetArchetype(IComponent component)
        {
            return GetArchetype(component.GetType());
        }

        internal static Archetype GetArchetype(Type componentType)
        {
            var typeId = GetComponentTypeId(componentType);
            var archetype = new Archetype();
            archetype.SetBit(typeId);
            return archetype;
        }

        internal static Archetype GetArchetype(List<int> typeIds)
        {
            var archetype = new Archetype();
            for (int i = 0; i < typeIds.Count; i++)
            {
                archetype.SetBit(typeIds[i]);
            }
            return archetype;
        }

        internal static void Clear()
        {
            _componentTypeIdMapKey = 0;
            _componentTypeIdMap.Clear();
        }
    }
}
