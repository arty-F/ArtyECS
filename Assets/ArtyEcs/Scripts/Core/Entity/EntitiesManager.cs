using System;
using System.Collections.Generic;

namespace ArtyECS.Core
{
    internal static class EntitiesManager
    {
        private const int DefaultInitialCapacity = 256;

        private static int _globalNextId = 0;

        private static readonly Dictionary<World, EntityPoolInstance> WorldPools =
            new Dictionary<World, EntityPoolInstance>();

        private static EntityPoolInstance GetOrCreatePool(World world)
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

        internal static Entity Allocate(World world)
        {
            var pool = GetOrCreatePool(world);
            return pool.Allocate(ref _globalNextId);
        }

        internal static bool Deallocate(Entity entity, World world)
        {
            if (!entity.IsValid)
            {
                return false;
            }

            var pool = GetOrCreatePool(world);
            return pool.Deallocate(entity);
        }

        internal static bool IsAllocated(Entity entity, World world)
        {
            if (!entity.IsValid)
            {
                return false;
            }

            var pool = GetOrCreatePool(world);
            return pool.IsAllocated(entity);
        }

        internal static int GetAllocatedCount(World world)
        {
            var pool = GetOrCreatePool(world);
            return pool.GetAllocatedCount();
        }

        internal static int GetAvailableCount(World world)
        {
            var pool = GetOrCreatePool(world);
            return pool.GetAvailableCount();
        }

        internal static void ClearWorld(World world)
        {
            if (world == null)
            {
                return;
            }

            WorldPools.Remove(world);
        }

        internal static void Clear(World world)
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
    }
}

