using System;
using System.Collections.Generic;

namespace ArtyECS.Core
{
    /// <summary>
    /// Pool for managing reusable entity IDs with generation tracking for safety.
    /// Provides fast allocation and deallocation of entities with ID recycling.
    /// </summary>
    /// <remarks>
    /// This class implements Core-011: Entity Pool Implementation.
    /// 
    /// Features:
    /// - Entity ID recycling for memory efficiency
    /// - Generation number tracking for safety (prevents use-after-free bugs)
    /// - Fast O(1) allocation and deallocation
    /// - World-scoped pools (each world has its own entity pool)
    /// - Zero-allocation in hot path (only allocates on pool growth)
    /// 
    /// The pool maintains:
    /// - A stack of available entity IDs for fast allocation
    /// - A dictionary tracking generation numbers per entity ID
    /// - A counter for the next new entity ID
    /// 
    /// When an entity is deallocated, its ID is returned to the pool and its generation is incremented.
    /// When an entity is allocated, if the pool has available IDs, one is reused with incremented generation.
    /// Otherwise, a new ID is assigned.
    /// 
    /// Generation numbers ensure that old entity references become invalid when IDs are recycled,
    /// preventing accidental use of destroyed entities.
    /// </remarks>
    public static class EntityPool
    {
        /// <summary>
        /// Default initial capacity for entity pools.
        /// </summary>
        private const int DefaultInitialCapacity = 64;

        /// <summary>
        /// Registry of worlds to their entity pool instances.
        /// Each world has its own isolated entity pool.
        /// </summary>
        private static readonly Dictionary<World, EntityPoolInstance> WorldPools =
            new Dictionary<World, EntityPoolInstance>();

        /// <summary>
        /// Global/default world instance. Used when no world is specified.
        /// Lazily initialized to ensure ComponentsStorage is ready.
        /// </summary>
        private static World GlobalWorld => ComponentsStorage.GetGlobalWorld();

        /// <summary>
        /// Gets or creates the entity pool instance for the specified world.
        /// </summary>
        /// <param name="world">World instance, or null for global world</param>
        /// <returns>Entity pool instance for the specified world</returns>
        private static EntityPoolInstance GetOrCreatePool(World world = null)
        {
            World targetWorld = world ?? GlobalWorld;

            if (!WorldPools.TryGetValue(targetWorld, out var pool))
            {
                pool = new EntityPoolInstance(DefaultInitialCapacity);
                WorldPools[targetWorld] = pool;
            }

            return pool;
        }

        /// <summary>
        /// Allocates a new entity from the pool in the specified world.
        /// Reuses an available entity ID if possible, otherwise creates a new one.
        /// </summary>
        /// <param name="world">Optional world instance (default: global world)</param>
        /// <returns>Newly allocated entity with unique ID and generation</returns>
        /// <remarks>
        /// This method provides fast O(1) entity allocation.
        /// 
        /// If the pool has available recycled IDs, one is reused with incremented generation.
        /// Otherwise, a new ID is assigned starting from 0.
        /// 
        /// The returned entity is guaranteed to be unique (different ID or generation) from
        /// any currently active entities in the same world.
        /// 
        /// Usage:
        /// <code>
        /// var entity = EntityPool.Allocate();
        /// // Use entity...
        /// EntityPool.Deallocate(entity);
        /// </code>
        /// </remarks>
        public static Entity Allocate(World world = null)
        {
            var pool = GetOrCreatePool(world);
            return pool.Allocate();
        }

        /// <summary>
        /// Deallocates an entity, returning its ID to the pool for reuse.
        /// Increments the generation number to invalidate old references.
        /// </summary>
        /// <param name="entity">Entity to deallocate</param>
        /// <param name="world">Optional world instance (default: global world)</param>
        /// <returns>True if entity was deallocated, false if entity was invalid or already deallocated</returns>
        /// <remarks>
        /// This method provides fast O(1) entity deallocation.
        /// 
        /// When an entity is deallocated:
        /// 1. Its ID is added to the available pool stack
        /// 2. Its generation number is incremented
        /// 3. The next allocation of this ID will use the new generation
        /// 
        /// This ensures that any old references to the entity become invalid
        /// (they will have the old generation number and won't match).
        /// 
        /// Note: This method does NOT remove components from ComponentsStorage.
        /// Component cleanup should be handled separately (see Core-012).
        /// 
        /// Usage:
        /// <code>
        /// EntityPool.Deallocate(entity);
        /// // Entity ID is now available for reuse
        /// </code>
        /// </remarks>
        public static bool Deallocate(Entity entity, World world = null)
        {
            if (!entity.IsValid)
            {
                return false;
            }

            var pool = GetOrCreatePool(world);
            return pool.Deallocate(entity);
        }

        /// <summary>
        /// Checks if an entity is currently allocated (not in the pool).
        /// </summary>
        /// <param name="entity">Entity to check</param>
        /// <param name="world">Optional world instance (default: global world)</param>
        /// <returns>True if entity is allocated, false if invalid or deallocated</returns>
        /// <remarks>
        /// This method checks if the entity's ID and generation match the current
        /// generation for that ID in the pool. If the generation matches, the entity
        /// is considered allocated. If the generation is lower, the entity was
        /// deallocated and its ID was recycled.
        /// </remarks>
        public static bool IsAllocated(Entity entity, World world = null)
        {
            if (!entity.IsValid)
            {
                return false;
            }

            var pool = GetOrCreatePool(world);
            return pool.IsAllocated(entity);
        }

        /// <summary>
        /// Gets the number of entities currently allocated in the specified world.
        /// </summary>
        /// <param name="world">Optional world instance (default: global world)</param>
        /// <returns>Number of allocated entities</returns>
        public static int GetAllocatedCount(World world = null)
        {
            var pool = GetOrCreatePool(world);
            return pool.GetAllocatedCount();
        }

        /// <summary>
        /// Gets the number of entity IDs available for reuse in the specified world.
        /// </summary>
        /// <param name="world">Optional world instance (default: global world)</param>
        /// <returns>Number of available entity IDs in the pool</returns>
        public static int GetAvailableCount(World world = null)
        {
            var pool = GetOrCreatePool(world);
            return pool.GetAvailableCount();
        }

        /// <summary>
        /// Clears the entity pool for the specified world, resetting all state.
        /// All entities become invalid after this operation.
        /// </summary>
        /// <param name="world">Optional world instance (default: global world)</param>
        /// <remarks>
        /// This method should be used with caution. It resets the entire pool,
        /// making all previously allocated entities invalid.
        /// 
        /// Typically used for cleanup or world reset scenarios.
        /// </remarks>
        public static void Clear(World world = null)
        {
            World targetWorld = world ?? GlobalWorld;
            if (WorldPools.TryGetValue(targetWorld, out var pool))
            {
                pool.Clear();
            }
        }

        /// <summary>
        /// Instance of entity pool for a specific world.
        /// Manages entity ID allocation, deallocation, and generation tracking.
        /// </summary>
        private class EntityPoolInstance
        {
            /// <summary>
            /// Stack of available entity IDs for fast allocation.
            /// IDs are pushed when deallocated and popped when allocated.
            /// </summary>
            private readonly Stack<int> _availableIds;

            /// <summary>
            /// Dictionary tracking current generation number for each entity ID.
            /// Key: entity ID, Value: current generation number
            /// </summary>
            private readonly Dictionary<int, int> _generations;

            /// <summary>
            /// Next new entity ID to assign when pool is empty.
            /// Incremented each time a new ID is allocated.
            /// </summary>
            private int _nextId;

            /// <summary>
            /// Number of currently allocated entities.
            /// </summary>
            private int _allocatedCount;

            /// <summary>
            /// Creates a new EntityPoolInstance with specified initial capacity.
            /// </summary>
            /// <param name="initialCapacity">Initial capacity for collections</param>
            public EntityPoolInstance(int initialCapacity = DefaultInitialCapacity)
            {
                _availableIds = new Stack<int>(initialCapacity);
                _generations = new Dictionary<int, int>(initialCapacity);
                _nextId = 0;
                _allocatedCount = 0;
            }

            /// <summary>
            /// Allocates a new entity from the pool.
            /// </summary>
            /// <returns>Newly allocated entity</returns>
            public Entity Allocate()
            {
                int id;
                int generation;

                if (_availableIds.Count > 0)
                {
                    // Reuse an available ID from the pool
                    id = _availableIds.Pop();
                    
                    // Get current generation and increment it
                    if (_generations.TryGetValue(id, out int currentGen))
                    {
                        generation = currentGen + 1;
                    }
                    else
                    {
                        // ID was never used before (shouldn't happen, but handle gracefully)
                        generation = 0;
                    }
                    
                    // Update generation for this ID
                    _generations[id] = generation;
                }
                else
                {
                    // No available IDs, create a new one
                    id = _nextId++;
                    generation = 0;
                    _generations[id] = generation;
                }

                _allocatedCount++;
                return new Entity(id, generation);
            }

            /// <summary>
            /// Deallocates an entity, returning its ID to the pool.
            /// </summary>
            /// <param name="entity">Entity to deallocate</param>
            /// <returns>True if entity was deallocated, false if invalid or already deallocated</returns>
            public bool Deallocate(Entity entity)
            {
                if (!entity.IsValid)
                {
                    return false;
                }

                int id = entity.Id;
                int generation = entity.Generation;

                // Check if entity is currently allocated (generation matches)
                if (!_generations.TryGetValue(id, out int currentGen) || currentGen != generation)
                {
                    // Entity was already deallocated or never existed
                    return false;
                }

                // Increment generation to invalidate old references
                _generations[id] = generation + 1;

                // Return ID to pool for reuse
                _availableIds.Push(id);

                _allocatedCount--;
                return true;
            }

            /// <summary>
            /// Checks if an entity is currently allocated.
            /// </summary>
            /// <param name="entity">Entity to check</param>
            /// <returns>True if entity is allocated, false otherwise</returns>
            public bool IsAllocated(Entity entity)
            {
                if (!entity.IsValid)
                {
                    return false;
                }

                int id = entity.Id;
                int generation = entity.Generation;

                // Entity is allocated if its generation matches the current generation for this ID
                return _generations.TryGetValue(id, out int currentGen) && currentGen == generation;
            }

            /// <summary>
            /// Gets the number of currently allocated entities.
            /// </summary>
            /// <returns>Number of allocated entities</returns>
            public int GetAllocatedCount()
            {
                return _allocatedCount;
            }

            /// <summary>
            /// Gets the number of available entity IDs in the pool.
            /// </summary>
            /// <returns>Number of available IDs</returns>
            public int GetAvailableCount()
            {
                return _availableIds.Count;
            }

            /// <summary>
            /// Clears the pool, resetting all state.
            /// </summary>
            public void Clear()
            {
                _availableIds.Clear();
                _generations.Clear();
                _nextId = 0;
                _allocatedCount = 0;
            }
        }
    }
}

