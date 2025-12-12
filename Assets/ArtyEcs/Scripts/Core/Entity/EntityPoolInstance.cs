using System.Collections.Generic;

namespace ArtyECS.Core
{
    /// <summary>
    /// Instance of entity pool for a specific world.
    /// Manages entity ID allocation, deallocation, and generation tracking.
    /// Optimized for zero-allocation reuse and large entity counts (Perf-000).
    /// </summary>
    internal class EntityPoolInstance
    {
        /// <summary>
        /// Default initial capacity for entity pools.
        /// Increased from 64 to 256 for better performance with large entity counts.
        /// </summary>
        private const int DefaultInitialCapacity = 256;

        /// <summary>
        /// Stack of available entity IDs for fast allocation.
        /// IDs are pushed when deallocated and popped when allocated.
        /// </summary>
        private readonly Stack<int> _availableIds;

        /// <summary>
        /// Dictionary tracking current generation number for each entity ID.
        /// Key: entity ID, Value: current generation number
        /// Pre-allocated with capacity to reduce rehashing overhead.
        /// </summary>
        private readonly Dictionary<int, int> _generations;

        /// <summary>
        /// Number of currently allocated entities.
        /// </summary>
        private int _allocatedCount;

        /// <summary>
        /// Initial capacity for lazy pre-allocation.
        /// </summary>
        private readonly int _initialCapacity;

        /// <summary>
        /// Flag indicating whether lazy pre-allocation has been performed.
        /// </summary>
        private bool _preAllocated;

        /// <summary>
        /// Number of pre-allocated IDs that haven't been used yet.
        /// Used to correctly calculate GetAvailableCount() - only IDs that were used
        /// and then deallocated should be counted as available.
        /// </summary>
        private int _unusedPreAllocatedCount;

        /// <summary>
        /// Creates a new EntityPoolInstance with specified initial capacity.
        /// Uses lazy pre-allocation for better performance without changing pool semantics.
        /// </summary>
        /// <param name="initialCapacity">Initial capacity for collections and lazy pre-allocation</param>
        /// <param name="globalNextId">Reference to global ID counter (unused in constructor, used in lazy pre-allocation)</param>
        /// <remarks>
        /// Perf-000: Uses lazy pre-allocation - entity IDs are pre-allocated on first Allocate()
        /// instead of at pool creation. This provides faster O(1) allocation without changing
        /// the semantic behavior (GetAvailableCount() returns 0 for new pools).
        /// </remarks>
        public EntityPoolInstance(int initialCapacity, ref int globalNextId)
        {
            // Pre-allocate with capacity to reduce rehashing
            _availableIds = new Stack<int>(initialCapacity);
            _generations = new Dictionary<int, int>(initialCapacity);
            _initialCapacity = initialCapacity;
            _preAllocated = false;
            _unusedPreAllocatedCount = 0;
            _allocatedCount = 0;
            // Note: globalNextId is stored for lazy pre-allocation, but we don't modify it here
            // to avoid changing pool semantics (GetAvailableCount should return 0 for new pools)
        }

        /// <summary>
        /// Allocates a new entity from the pool.
        /// Optimized for O(1) allocation with lazy pre-allocated IDs (Perf-000).
        /// </summary>
        /// <param name="globalNextId">Reference to global ID counter for uniqueness across worlds</param>
        /// <returns>Newly allocated entity</returns>
        /// <remarks>
        /// Perf-000: On first allocation, performs lazy pre-allocation of N IDs (where N = initial capacity)
        /// to avoid global counter access for subsequent allocations. This provides better performance
        /// while maintaining correct pool semantics (GetAvailableCount returns 0 for new pools).
        /// </remarks>
        public Entity Allocate(ref int globalNextId)
        {
            // Lazy pre-allocation: pre-allocate IDs on first allocation
            // This provides performance benefits without changing pool semantics
            if (!_preAllocated)
            {
                // Pre-allocate entity IDs into the pool for faster subsequent allocations
                // This avoids global counter access for the first N allocations
                for (int i = 0; i < _initialCapacity; i++)
                {
                    int preAllocatedId = globalNextId++;
                    _availableIds.Push(preAllocatedId);
                    _generations[preAllocatedId] = 0; // Initialize generation to 0
                }
                _preAllocated = true;
                _unusedPreAllocatedCount = _initialCapacity;
            }

            int id;
            int generation;

            if (_availableIds.Count > 0)
            {
                // Reuse an available ID from the pool
                id = _availableIds.Pop();
                
                // If this was a pre-allocated ID that hasn't been used yet, decrement counter
                if (_unusedPreAllocatedCount > 0)
                {
                    _unusedPreAllocatedCount--;
                }
                
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
        /// Only counts IDs that were used and then deallocated, not pre-allocated unused IDs.
        /// </summary>
        /// <returns>Number of available IDs (excluding unused pre-allocated IDs)</returns>
        public int GetAvailableCount()
        {
            // Subtract unused pre-allocated IDs to maintain correct semantics:
            // GetAvailableCount should only return IDs that were used and deallocated
            return _availableIds.Count - _unusedPreAllocatedCount;
        }

        /// <summary>
        /// Clears the pool, resetting all state.
        /// </summary>
        public void Clear()
        {
            _availableIds.Clear();
            _generations.Clear();
            _allocatedCount = 0;
            _preAllocated = false; // Reset pre-allocation flag so lazy pre-allocation happens again
            _unusedPreAllocatedCount = 0;
        }
    }
}

