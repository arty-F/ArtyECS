using System;
using System.Buffers;
using System.Collections.Generic;

namespace ArtyECS.Core
{
    internal class QueryContext
    {
        private static readonly ArrayPool<Entity> _arrayPool = ArrayPool<Entity>.Shared;
        private readonly List<Entity[]> _rentedArrays = new List<Entity[]>();
        private readonly List<HashSet<Entity>> _rentedHashSets = new List<HashSet<Entity>>();
        private static readonly Dictionary<WorldInstance, QueryContext> _contexts = 
            new Dictionary<WorldInstance, QueryContext>();

        public static QueryContext Get(WorldInstance world)
        {
            if (!_contexts.TryGetValue(world, out var context))
            {
                context = new QueryContext();
                _contexts[world] = context;
            }
            return context;
        }

        public Entity[] RentArray(int minimumLength)
        {
            var array = _arrayPool.Rent(minimumLength);
            _rentedArrays.Add(array);
            return array;
        }

        public HashSet<Entity> RentHashSet()
        {
            var set = EntityHashSetPool.Rent();
            _rentedHashSets.Add(set);
            return set;
        }

        private void ReturnRentedResources()
        {
            foreach (var array in _rentedArrays)
            {
                _arrayPool.Return(array);
            }
            _rentedArrays.Clear();

            foreach (var set in _rentedHashSets)
            {
                EntityHashSetPool.Return(set);
            }
            _rentedHashSets.Clear();
        }

        public static void ReturnResourcesForAllContexts()
        {
            foreach (var context in _contexts.Values)
            {
                context.ReturnRentedResources();
            }
        }
    }
}

