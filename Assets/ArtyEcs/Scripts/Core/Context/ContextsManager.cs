using System;
using System.Collections.Generic;

namespace ArtyECS.Core
{
    internal static class ContextsManager
    {
        private static int _contextTypeIdMapKey;
        private static Dictionary<Type, int> _contextTypeIdMap = new();
        private static Dictionary<int, Queue<Context>> _poolMap = new(Constants.CONTEXT_TYPES_POOL_CAPACITY);

        internal static int ContextTypesCount => _contextTypeIdMap.Keys.Count;

        internal static T GetContext<T>(Entity entity) where T : Context, new()
        {
            var contextTypeId = GetContextTypeId(typeof(T));
            if (!_poolMap.ContainsKey(contextTypeId))
            {
                _poolMap[contextTypeId] = new(Constants.CONTEXT_POOL_CAPACITY);
                _poolMap[contextTypeId].Enqueue(ContextNewComponent<T>(contextTypeId));
            }

            var pool = _poolMap[contextTypeId];
            if (pool.Count == 0)
            {
                while (pool.Count < Constants.CONTEXT_POOL_CAPACITY)
                {
                    pool.Enqueue(ContextNewComponent<T>(contextTypeId));
                }
            }

            var component = (T)pool.Dequeue();
            component.SetEntity(entity);
            return component;
        }

        private static Context ContextNewComponent<T>(int typeId) where T : Context, new()
        {
            var context = new T();
            context.TypeId = typeId;
            return context;
        }

        internal static void RegisterContext(Context context)
        {
            var typeId = GetContextTypeId(context.GetType());
            context.TypeId = typeId;
            if (!_poolMap.ContainsKey(typeId))
            {
                _poolMap[typeId] = new();
            }
        }

        internal static void Release(Context component)
        {
            _poolMap[component.TypeId].Enqueue(component);
        }

        internal static int GetContextTypeId(Type contextType)
        {
            if (!_contextTypeIdMap.ContainsKey(contextType))
            {
                _contextTypeIdMap.Add(contextType, _contextTypeIdMapKey++);
            }

            return _contextTypeIdMap[contextType];
        }

        internal static void Clear()
        {
            _contextTypeIdMapKey = 0;
            _contextTypeIdMap.Clear();
            _poolMap.Clear();
        }
    }
}
