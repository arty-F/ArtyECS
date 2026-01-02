using System.Collections.Generic;

namespace ArtyECS.Core
{
    internal static class EntitiesPool
    {
        private static int _currentId;

        private static Queue<Entity> _pool = new(Constants.ENTITY_POOL_CAPACITY);

        internal static Entity GetEntity()
        {
            if (_pool.Count == 0)
            {
                while (_pool.Count < Constants.ENTITY_POOL_CAPACITY)
                {
                    _pool.Enqueue(new Entity(_currentId++));
                }
            }

            return _pool.Dequeue();
        }

        internal static void Release(Entity entity)
        {
            entity.Clear();
            _pool.Enqueue(entity);
        }

        internal static void Clear()
        {
            _currentId = 0;
            _pool.Clear();
        }
    }
}
