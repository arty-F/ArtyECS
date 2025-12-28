using System;
using System.Collections.Generic;

namespace ArtyECS.Core
{
    internal static class EntitiesManager
    {
        private const int DefaultInitialCapacity = 256;

        private static int _globalNextId = 0;

        private static readonly Dictionary<WorldInstance, EntityPoolInstance> WorldPools =
            new Dictionary<WorldInstance, EntityPoolInstance>();

        private static EntityPoolInstance GetOrCreatePool(WorldInstance world)
        {
            if (world == null)
                throw new ArgumentNullException(nameof(world));

            if (!WorldPools.TryGetValue(world, out var pool))
            {
                pool = new EntityPoolInstance(DefaultInitialCapacity, ref _globalNextId);
                WorldPools[world] = pool;
            }

            return pool;
        }

        internal static Entity Allocate(WorldInstance world)
        {
            var pool = GetOrCreatePool(world);
            return pool.Allocate(ref _globalNextId);
        }

        internal static bool Deallocate(Entity entity, WorldInstance world)
        {
            if (!entity.IsValid)
            {
                return false;
            }

            var pool = GetOrCreatePool(world);
            return pool.Deallocate(entity);
        }

        internal static bool IsAllocated(Entity entity, WorldInstance world)
        {
            if (!entity.IsValid)
            {
                return false;
            }

            var pool = GetOrCreatePool(world);
            return pool.IsAllocated(entity);
        }

        internal static int GetAllocatedCount(WorldInstance world)
        {
            var pool = GetOrCreatePool(world);
            return pool.GetAllocatedCount();
        }

        internal static int GetAvailableCount(WorldInstance world)
        {
            var pool = GetOrCreatePool(world);
            return pool.GetAvailableCount();
        }

        internal static void ClearWorld(WorldInstance world)
        {
            if (world == null)
            {
                return;
            }

            WorldPools.Remove(world);
        }

        internal static void Clear(WorldInstance world)
        {
            if (world == null)
                throw new ArgumentNullException(nameof(world));

            if (WorldPools.TryGetValue(world, out var pool))
            {
                pool.Clear();
            }
        }

        internal static void ClearAll()
        {
            WorldPools.Clear();
        }

#if UNITY_EDITOR
        internal static (int AvailableCount, int GenerationCount)? GetPoolDataForMonitoring(WorldInstance world)
        {
            if (world == null || !WorldPools.TryGetValue(world, out var pool))
                return null;

            return pool.GetPoolData();
        }
#endif
    }
}

