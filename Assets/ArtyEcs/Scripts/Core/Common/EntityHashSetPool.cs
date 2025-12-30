using System;
using System.Collections.Generic;

namespace ArtyECS.Core
{
    internal static class EntityHashSetPool
    {
        private static readonly Stack<HashSet<Entity>> _pool = new Stack<HashSet<Entity>>();
        private static readonly object _lock = new object();

        public static HashSet<Entity> Rent(int minimumCapacity = 0)
        {
            if (minimumCapacity > 0)
            {
                lock (_lock)
                {
                    if (_pool.Count > 0)
                    {
                        var set = _pool.Pop();
                        set.Clear();
                        
                        set.EnsureCapacity(minimumCapacity);
                        return set;
                    }
                }
                
                return new HashSet<Entity>(minimumCapacity);
            }
            
            lock (_lock)
            {
                if (_pool.Count > 0)
                {
                    var set = _pool.Pop();
                    set.Clear();
                    return set;
                }
            }

            return new HashSet<Entity>();
        }

        public static void Return(HashSet<Entity> set)
        {
            if (set == null)
                return;

            lock (_lock)
            {
                set.Clear();
                _pool.Push(set);
            }
        }

        public static PooledHashSet<Entity> RentPooled(int minimumCapacity = 0)
        {
            var set = Rent(minimumCapacity);
            return new PooledHashSet<Entity>(set);
        }
    }
}

