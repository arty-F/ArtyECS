using System;
using System.Collections.Generic;

namespace ArtyECS.Core
{
    /// <summary>
    /// Storage for components of a specific type.
    /// Uses array-based storage with entity-to-index mapping for efficient access.
    /// </summary>
    /// <typeparam name="T">Component type (must be struct implementing IComponent)</typeparam>
    /// <remarks>
    /// This class implements Core-003: Single Component Type Storage.
    /// Perf-001: Component Storage Optimization (COMPLETED)
    /// 
    /// Features:
    /// - Array-based storage for contiguous memory layout (cache-friendly)
    /// - Entity-to-index mapping for O(1) component lookup by entity
    /// - Index-to-entity reverse mapping for efficient iteration
    /// - Dynamic capacity management with doubling growth strategy (amortized O(1))
    /// - Only stores components for entities that have them (sparse storage)
    /// - Implements IComponentTable for type-erased removal without reflection
    /// 
    /// Performance Optimizations (Perf-001):
    /// - Contiguous memory layout: components stored in arrays for cache efficiency
    /// - Minimal pointer chasing: direct array access, no linked structures
    /// - Efficient array growth: doubling strategy provides amortized O(1) insertion
    /// - Dictionary capacity optimization: pre-allocated with load factor consideration
    /// - Memory alignment: arrays naturally aligned, cache-line friendly for sequential access
    /// 
    /// The storage maintains two arrays:
    /// 1. Components array: stores actual component values (contiguous memory)
    /// 2. Entities array: stores entity identifiers for reverse lookup (contiguous memory)
    /// 
    /// Both arrays grow together to maintain index alignment.
    /// Components are stored contiguously in memory for optimal cache performance.
    /// </remarks>
    internal class ComponentTable<T> : IComponentTable where T : struct, IComponent
    {
        /// <summary>
        /// Default initial capacity for component storage.
        /// Increased from 16 to 32 for better performance with typical workloads.
        /// </summary>
        private const int DefaultInitialCapacity = 32;

        /// <summary>
        /// Dictionary load factor (approximately 0.72 for .NET Dictionary).
        /// Used to calculate optimal dictionary capacity to minimize rehashing.
        /// </summary>
        private const double DictionaryLoadFactor = 0.72;

        /// <summary>
        /// Array storing component values. Indices correspond to Entities array.
        /// </summary>
        private T[] _components;

        /// <summary>
        /// Array storing entity identifiers. Indices correspond to Components array.
        /// Used for reverse lookup: index -> entity.
        /// </summary>
        private Entity[] _entities;

        /// <summary>
        /// Current number of components stored (active count).
        /// </summary>
        private int _count;

        /// <summary>
        /// Mapping from entity to index in the components/entities arrays.
        /// </summary>
        private readonly Dictionary<Entity, int> _entityToIndex;

        /// <summary>
        /// Creates a new ComponentTable with default initial capacity.
        /// </summary>
        public ComponentTable() : this(DefaultInitialCapacity)
        {
        }

        /// <summary>
        /// Creates a new ComponentTable with specified initial capacity.
        /// </summary>
        /// <param name="initialCapacity">Initial capacity for component arrays</param>
        /// <remarks>
        /// Perf-001: Dictionary capacity is optimized to account for load factor,
        /// reducing rehashing overhead during component additions.
        /// </remarks>
        public ComponentTable(int initialCapacity)
        {
            if (initialCapacity < 1)
                throw new ArgumentException("Initial capacity must be at least 1", nameof(initialCapacity));

            _components = new T[initialCapacity];
            _entities = new Entity[initialCapacity];
            _count = 0;
            
            // Perf-001: Optimize dictionary capacity to account for load factor (~0.72)
            // This reduces rehashing overhead when adding components
            // Calculate capacity that allows initialCapacity items without rehashing
            int dictionaryCapacity = (int)Math.Ceiling(initialCapacity / DictionaryLoadFactor);
            _entityToIndex = new Dictionary<Entity, int>(dictionaryCapacity);
        }

        /// <summary>
        /// Gets the current number of components stored.
        /// </summary>
        public int Count => _count;

        /// <summary>
        /// Gets the current capacity of the storage arrays.
        /// </summary>
        public int Capacity => _components.Length;

        /// <summary>
        /// Gets a ReadOnlySpan over all stored components for zero-allocation iteration.
        /// </summary>
        /// <returns>ReadOnlySpan containing all components</returns>
        public ReadOnlySpan<T> GetComponents()
        {
            return new ReadOnlySpan<T>(_components, 0, _count);
        }

        /// <summary>
        /// Gets all entities that have this component type.
        /// Returns a HashSet for efficient set operations (intersection, etc.).
        /// </summary>
        /// <returns>HashSet containing all entities with this component type</returns>
        /// <remarks>
        /// Perf-001: HashSet capacity is pre-allocated with exact count to avoid rehashing.
        /// </remarks>
        internal HashSet<Entity> GetEntitiesSet()
        {
            // Perf-001: Pre-allocate HashSet with exact capacity to avoid rehashing
            var entitiesSet = new HashSet<Entity>(_count);
            for (int i = 0; i < _count; i++)
            {
                entitiesSet.Add(_entities[i]);
            }
            return entitiesSet;
        }

        /// <summary>
        /// Gets all entities that have this component type as a span.
        /// </summary>
        /// <returns>ReadOnlySpan containing all entities with this component type</returns>
        internal ReadOnlySpan<Entity> GetEntities()
        {
            return new ReadOnlySpan<Entity>(_entities, 0, _count);
        }

        /// <summary>
        /// Gets the entity at the specified index.
        /// </summary>
        /// <param name="index">Index in the storage arrays</param>
        /// <returns>Entity at the index</returns>
        /// <exception cref="IndexOutOfRangeException">If index is out of range</exception>
        public Entity GetEntityAt(int index)
        {
            if (index < 0 || index >= _count)
                throw new IndexOutOfRangeException($"Index {index} is out of range [0, {_count})");

            return _entities[index];
        }

        /// <summary>
        /// Checks if an entity has a component of this type stored.
        /// </summary>
        /// <param name="entity">Entity to check</param>
        /// <returns>True if entity has a component, false otherwise</returns>
        public bool HasComponent(Entity entity)
        {
            return _entityToIndex.ContainsKey(entity);
        }

        /// <summary>
        /// Gets the index of the component for the specified entity.
        /// </summary>
        /// <param name="entity">Entity to look up</param>
        /// <param name="index">Output parameter: index if found</param>
        /// <returns>True if entity has a component, false otherwise</returns>
        public bool TryGetIndex(Entity entity, out int index)
        {
            return _entityToIndex.TryGetValue(entity, out index);
        }

        /// <summary>
        /// Gets the component for the specified entity.
        /// </summary>
        /// <param name="entity">Entity to get component for</param>
        /// <param name="component">Output parameter: component value if found</param>
        /// <returns>True if entity has a component, false otherwise</returns>
        /// <remarks>
        /// Zero-allocation lookup using entity-to-index mapping.
        /// Returns false and default value if entity doesn't have the component.
        /// </remarks>
        public bool TryGetComponent(Entity entity, out T component)
        {
            if (_entityToIndex.TryGetValue(entity, out int index))
            {
                component = _components[index];
                return true;
            }

            component = default(T);
            return false;
        }

        /// <summary>
        /// Ensures the storage has at least the specified capacity.
        /// Grows the arrays if necessary using doubling strategy (amortized O(1)).
        /// </summary>
        /// <param name="minCapacity">Minimum required capacity</param>
        /// <remarks>
        /// Perf-001: Array growth optimization
        /// - Doubling strategy provides amortized O(1) insertion cost
        /// - Array.Copy is optimized for value types (structs) by the runtime
        /// - Contiguous memory layout is maintained after growth
        /// - Both arrays grow together to maintain index alignment
        /// </remarks>
        private void EnsureCapacity(int minCapacity)
        {
            if (_components.Length >= minCapacity)
                return;

            // Perf-001: Doubling strategy for amortized O(1) growth
            // Double the capacity, but at least reach minCapacity
            int newCapacity = Math.Max(_components.Length * 2, minCapacity);

            // Perf-001: Grow components array (contiguous memory layout maintained)
            // Array.Copy is optimized for value types by the runtime
            var newComponents = new T[newCapacity];
            Array.Copy(_components, 0, newComponents, 0, _count);
            _components = newComponents;

            // Perf-001: Grow entities array (maintains index alignment with components)
            var newEntities = new Entity[newCapacity];
            Array.Copy(_entities, 0, newEntities, 0, _count);
            _entities = newEntities;
        }

        /// <summary>
        /// Gets or creates a reference to the internal storage arrays.
        /// Used internally by ComponentsManager for add/remove/get operations.
        /// </summary>
        /// <returns>Internal storage arrays (components, entities, entity-to-index mapping)</returns>
        internal (T[] components, Entity[] entities, Dictionary<Entity, int> entityToIndex) GetInternalTable()
        {
            return (_components, _entities, _entityToIndex);
        }

        /// <summary>
        /// Gets a reference to the count field for modification.
        /// </summary>
        /// <returns>Reference to the count field</returns>
        internal ref int GetCountRef()
        {
            return ref _count;
        }

        /// <summary>
        /// Ensures capacity and returns internal storage for modification.
        /// Called by ComponentsManager before adding components.
        /// </summary>
        /// <param name="minCapacity">Minimum capacity needed</param>
        /// <returns>Internal table arrays ready for modification</returns>
        internal (T[] components, Entity[] entities, Dictionary<Entity, int> entityToIndex) GetInternalTableForAdd(int minCapacity)
        {
            EnsureCapacity(minCapacity);
            return GetInternalTable();
        }

        /// <summary>
        /// Attempts to remove a component for the specified entity if it exists.
        /// Implements IComponentTable interface for type-erased removal without reflection.
        /// </summary>
        /// <param name="entity">Entity to remove component for</param>
        /// <returns>True if component was removed, false if entity didn't have this component type</returns>
        /// <remarks>
        /// This method allows removing components without knowing the type at compile time,
        /// enabling efficient entity destruction without reflection.
        /// </remarks>
        public bool TryRemoveComponentForEntity(Entity entity)
        {
            if (!HasComponent(entity))
            {
                return false;
            }

            RemoveComponentInternal(entity);
            return true;
        }

        /// <summary>
        /// Removes a component for the specified entity using swap-with-last-element strategy.
        /// This is an O(1) operation that maintains contiguous memory layout.
        /// </summary>
        /// <param name="entity">Entity to remove component from</param>
        /// <remarks>
        /// Removal strategy:
        /// 1. Get index of component to remove
        /// 2. If it's the last element, just decrement count and remove from dictionary
        /// 3. Otherwise, swap with last element:
        ///    - Copy last element to removal index
        ///    - Update entity-to-index mapping for swapped element
        ///    - Remove original entity from dictionary
        ///    - Decrement count
        /// 
        /// This ensures O(1) removal and maintains array contiguity.
        /// </remarks>
        internal void RemoveComponentInternal(Entity entity)
        {
            // Get index of component to remove
            if (!_entityToIndex.TryGetValue(entity, out int removeIndex))
            {
                // Entity doesn't have this component (shouldn't happen if called correctly)
                return;
            }

            int lastIndex = _count - 1;

            // If removing the last element, just decrement count and remove from dictionary
            if (removeIndex == lastIndex)
            {
                _entityToIndex.Remove(entity);
                _count--;
                return;
            }

            // Swap with last element for O(1) removal
            Entity lastEntity = _entities[lastIndex];
            T lastComponent = _components[lastIndex];

            // Move last element to removal position
            _components[removeIndex] = lastComponent;
            _entities[removeIndex] = lastEntity;

            // Update mapping for the swapped element (last element now at removeIndex)
            _entityToIndex[lastEntity] = removeIndex;

            // Remove original entity from dictionary
            _entityToIndex.Remove(entity);

            // Decrement count
            _count--;
        }
    }
}

