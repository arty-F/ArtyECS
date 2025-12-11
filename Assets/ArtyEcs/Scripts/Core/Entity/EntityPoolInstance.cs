using System.Collections.Generic;

namespace ArtyECS.Core
{
    /// <summary>
    /// Instance of entity pool for a specific world.
    /// Manages entity ID allocation, deallocation, and generation tracking.
    /// </summary>
    internal class EntityPoolInstance
    {
        /// <summary>
        /// Default initial capacity for entity pools.
        /// </summary>
        private const int DefaultInitialCapacity = 64;

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
        /// <param name="globalNextId">Reference to global ID counter for uniqueness across worlds</param>
        /// <returns>Newly allocated entity</returns>
        public Entity Allocate(ref int globalNextId)
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
                // No available IDs, create a new one using global counter
                // This ensures uniqueness across all worlds
                id = globalNextId++;
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

