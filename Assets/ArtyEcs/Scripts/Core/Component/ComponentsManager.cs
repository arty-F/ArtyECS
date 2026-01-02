using System;
using System.Collections.Generic;

namespace ArtyECS.Core
{
    internal static class ComponentsManager
    {
        private static int _componentTypeWrapIdMapKey;
        private static Dictionary<Type, int> _componentTypeWrapIdMap = new();

        internal static ComponentWrapper WrapComponent(IComponent component)
        {
            var componentType = component.GetType();
            if (!_componentTypeWrapIdMap.ContainsKey(componentType))
            {
                _componentTypeWrapIdMap.Add(componentType, _componentTypeWrapIdMapKey++);
            }

            var wrapId = _componentTypeWrapIdMap[componentType];
            return new ComponentWrapper(wrapId, component);
        }

        internal static int GetWrapperId(IComponent component)
        {
            if (_componentTypeWrapIdMap.TryGetValue(component.GetType(), out var id))
            {
                return id;
            }

            return -1;
        }

        internal static int GetWrapperId(Type componentType)
        {
            if (_componentTypeWrapIdMap.TryGetValue(componentType, out var id))
            {
                return id;
            }

            return -1;
        }

        internal static void Clear()
        {
            _componentTypeWrapIdMapKey = 0;
            _componentTypeWrapIdMap.Clear();
        }
    }
}
