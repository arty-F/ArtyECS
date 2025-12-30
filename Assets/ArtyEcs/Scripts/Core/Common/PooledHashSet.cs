using System;
using System.Collections.Generic;

namespace ArtyECS.Core
{
    internal struct PooledHashSet<T> : IDisposable
    {
        private readonly HashSet<T> _set;
        private readonly object _pooledObject;

        internal PooledHashSet(HashSet<T> set)
        {
            _set = set;
            _pooledObject = set;
        }

        public HashSet<T> Set => _set;

        public void Dispose()
        {
            if (_pooledObject is HashSet<Entity> entitySet)
            {
                EntityHashSetPool.Return(entitySet);
            }
        }
    }
}

