using System;
using System.Collections.Generic;

namespace ArtyECS.Core
{
    /// <summary>
    /// Manager for ECS components, organized by world scope.
    /// Supports multiple worlds, each with isolated component storage.
    /// </summary>
    /// <remarks>
    /// **API-009: This class is internal implementation. Use World API instead.**
    /// 
    /// This class is kept public for internal framework use, but should not be used directly
    /// by framework users. Use World class methods instead:
    /// - World.GetComponent&lt;T&gt;(entity) instead of ComponentsManager.GetComponent&lt;T&gt;(entity)
    /// - World.AddComponent&lt;T&gt;(entity, component) instead of ComponentsManager.AddComponent&lt;T&gt;(entity, component)
    /// - World.GetEntitiesWith&lt;T1, T2&gt;() instead of ComponentsManager.GetEntitiesWith&lt;T1, T2&gt;()
    /// 
    /// See World class documentation for the public API.
    /// 
    /// This class manages component storage per world. Each world has its own
    /// registry of component type -> storage mappings.
    /// 
    /// Core-002: Basic structure with world-scoped storage dictionaries (COMPLETED)
    /// Core-003: Single component type storage with ComponentTable&lt;T&gt; (COMPLETED)
    /// Core-004: AddComponent method implementation (COMPLETED)
    /// Core-005: RemoveComponent method implementation (COMPLETED)
    /// Core-006: GetComponent method for single entity (COMPLETED)
        /// Core-007: GetComponents method for single type query (COMPLETED)
        /// Core-008: GetComponents method for multiple AND query (REMOVED - API-004)
        /// Core-009: GetComponentsWithout query for WITHOUT operations (REMOVED - API-003)
        /// Core-010: Deferred component modifications system (COMPLETED)
        /// API-005: Entity-Component Mapping Support (COMPLETED)
        ///   - GetEntitiesWith&lt;T1, T2, ...&gt;() methods for entity-centric queries (1-6 parameters)
        ///   - Returns ReadOnlySpan&lt;Entity&gt; for zero-allocation iteration
        ///   - Uses efficient set intersection algorithm with smallest-set optimization
    /// Core-012: RemoveAllComponents method for entity destruction (COMPLETED)
    /// API-006: Use Exceptions Instead of Try Pattern (COMPLETED)
    ///   - All methods use exceptions consistently (no Try pattern)
    ///   - AddComponent throws DuplicateComponentException for duplicates
    ///   - GetComponent throws ComponentNotFoundException if component not found
    ///   - All methods throw InvalidEntityException if entity is invalid or deallocated
    ///   - Exceptions are lightweight and safe in builds (minimal overhead)
    ///   - Entity validation via ValidateEntity() helper method
    /// World-002: World-Scoped Storage Integration (COMPLETED)
    ///   - All methods support optional World? parameter (default: global world)
    ///   - Automatic world resolution via ResolveWorld() method (null → global world)
    ///   - World-scoped storage via Dictionary&lt;World, Dictionary&lt;Type, IComponentTable&gt;&gt;
    ///   - Uses shared global world singleton from World.GetGlobalWorld()
    /// World-003: World Persistence Across Scenes (COMPLETED)
    ///   - Component storage uses static dictionaries that persist across Unity scene changes
    ///   - All component data remains valid after scene transitions
    ///   - No data loss on scene load/unload operations
    /// Perf-001: Component Storage Optimization (COMPLETED)
    ///   - Contiguous memory layout: components stored in arrays for cache efficiency
    ///   - Minimal pointer chasing: direct array access, no linked structures
    ///   - Efficient array growth: doubling strategy provides amortized O(1) insertion
    ///   - Dictionary capacity optimization: pre-allocated with load factor consideration
    ///   - Memory alignment: arrays naturally aligned, cache-line friendly for sequential access
        /// Perf-002: Query Optimization - Single Component (COMPLETED)
        ///   - Zero-allocation ReadOnlySpan return: already implemented via ComponentTable.GetComponents()
        ///   - Table reference caching: caches ComponentTable&lt;T&gt; references per world+type to avoid repeated dictionary lookups
        ///   - Efficient iteration: ReadOnlySpan provides zero-allocation iteration with minimal bounds checking overhead
        ///   - Cache invalidation: cache is automatically updated when tables are created or world is cleared
        /// Perf-003: Query Optimization - Multiple Components (COMPLETED)
        ///   - Efficient set intersection: uses smallest set as base for intersection to minimize HashSet operations
        ///   - Optimized algorithm: adapts to which table has fewer entities for optimal performance
        ///   - Minimized allocations: creates HashSet from smallest set first, reducing memory overhead
        ///   - Entity set caching: considered but deferred - component sets change frequently, smallest-set optimization provides
        ///     significant benefit, HashSet creation is O(n) with pre-allocated capacity (relatively cheap). Can be added later
        ///     if profiling shows repeated queries with stable sets are common use case.
        /// Perf-004: Query Optimization - Without Components (REMOVED - API-003)
        ///   - GetComponentsWithout methods removed as part of API simplification
        ///   - Users should filter results manually or use GetComponents with manual filtering
    /// </remarks>
    public static class ComponentsManager
    {
        /// <summary>
        /// Gets the global/default world instance. Used when no world is specified.
        /// Uses World.GetGlobalWorld() to ensure shared singleton instance.
        /// </summary>
        private static World GlobalWorld => World.GetGlobalWorld();

        /// <summary>
        /// Registry of worlds to their component storage instances.
        /// Each world has its own dictionary mapping component types to storage.
        /// Uses IComponentTable interface to allow type-erased removal without reflection.
        /// </summary>
        private static readonly Dictionary<World, Dictionary<Type, IComponentTable>> WorldTables =
            new Dictionary<World, Dictionary<Type, IComponentTable>>();

        /// <summary>
        /// Perf-002: Cache for ComponentTable references per component type and world.
        /// This avoids repeated dictionary lookups in hot paths (GetComponents queries).
        /// Key: (World, Type) tuple, Value: ComponentTable reference.
        /// </summary>
        /// <remarks>
        /// The cache is automatically kept in sync with WorldTables:
        /// - When a table is created, it's added to both WorldTables and the cache
        /// - When a world is cleared, the cache entries for that world are removed
        /// - Cache lookup is O(1) and avoids the double dictionary lookup (world -> type -> table)
        /// </remarks>
        private static readonly Dictionary<(World world, Type type), IComponentTable> TableCache =
            new Dictionary<(World world, Type type), IComponentTable>();

        /// <summary>
        /// Gets the storage dictionary for the specified world.
        /// Creates a new storage dictionary if the world doesn't exist yet.
        /// </summary>
        /// <param name="world">World instance (required)</param>
        /// <returns>Dictionary mapping component types to their storage instances</returns>
        private static Dictionary<Type, IComponentTable> GetWorldTable(World world)
        {
            if (world == null)
                throw new ArgumentNullException(nameof(world));

            if (!WorldTables.TryGetValue(world, out var table))
            {
                table = new Dictionary<Type, IComponentTable>();
                WorldTables[world] = table;
            }

            return table;
        }

        /// <summary>
        /// Validates that an entity is valid and allocated in the specified world.
        /// Throws InvalidEntityException if entity is invalid or deallocated.
        /// </summary>
        /// <param name="entity">Entity to validate</param>
        /// <param name="world">World instance (required)</param>
        /// <exception cref="InvalidEntityException">Thrown if entity is invalid or deallocated</exception>
        /// <remarks>
        /// API-006: Use Exceptions Instead of Try Pattern (COMPLETED)
        /// API-010: World is now required parameter (not optional)
        /// - Validates entity is valid (Id >= 0)
        /// - Validates entity is allocated (generation matches current generation in pool)
        /// - Throws InvalidEntityException if validation fails
        /// - Lightweight validation with minimal overhead
        /// </remarks>
        private static void ValidateEntity(Entity entity, World world)
        {
            if (world == null)
                throw new ArgumentNullException(nameof(world));

            if (!entity.IsValid)
            {
                throw new InvalidEntityException(entity);
            }

            if (!EntitiesManager.IsAllocated(entity, world))
            {
                throw new InvalidEntityException(entity);
            }
        }

        /// <summary>
        /// Gets or creates the storage instance for a specific component type in the specified world.
        /// Perf-002: Uses table cache to avoid repeated dictionary lookups in hot paths.
        /// </summary>
        /// <typeparam name="T">Component type (must be struct implementing IComponent)</typeparam>
        /// <param name="world">World instance (required)</param>
        /// <returns>Storage instance for component type T</returns>
        /// <remarks>
        /// API-010: World is now required parameter (not optional)
        /// 
        /// Creates ComponentTable&lt;T&gt; on first access for the specified world.
        /// 
        /// Perf-002 Optimization:
        /// - First checks table cache for O(1) lookup
        /// - If not in cache, performs dictionary lookup and adds to cache
        /// - Subsequent calls for same world+type use cached reference (zero dictionary lookups)
        /// </remarks>
        internal static ComponentTable<T> GetOrCreateTable<T>(World world) where T : struct, IComponent
        {
            if (world == null)
                throw new ArgumentNullException(nameof(world));

            Type componentType = typeof(T);
            var cacheKey = (world, componentType);

            // Perf-002: Check cache first for fast lookup
            if (TableCache.TryGetValue(cacheKey, out var cachedTable))
            {
                return (ComponentTable<T>)cachedTable;
            }

            // Cache miss: perform dictionary lookup and update cache
            var worldTable = GetWorldTable(world);

            if (!worldTable.TryGetValue(componentType, out var table))
            {
                table = new ComponentTable<T>();
                worldTable[componentType] = table;
            }

            // Perf-002: Add to cache for future lookups
            TableCache[cacheKey] = table;

            return (ComponentTable<T>)table;
        }

        /// <summary>
        /// Gets the number of worlds currently registered.
        /// </summary>
        /// <returns>Number of worlds (including global world)</returns>
        public static int GetWorldCount()
        {
            return WorldTables.Count;
        }

        /// <summary>
        /// Checks if a world has been initialized (has storage dictionary).
        /// </summary>
        /// <param name="world">World to check (required)</param>
        /// <returns>True if world has been initialized</returns>
        /// <remarks>
        /// API-010: World is now required parameter (not optional)
        /// </remarks>
        public static bool IsWorldInitialized(World world)
        {
            if (world == null)
                throw new ArgumentNullException(nameof(world));
            return WorldTables.ContainsKey(world);
        }

        /// <summary>
        /// Gets the global world instance.
        /// </summary>
        /// <returns>Global world instance</returns>
        public static World GetGlobalWorld()
        {
            return GlobalWorld;
        }

        /// <summary>
        /// Adds a component to the specified entity in the specified world.
        /// </summary>
        /// <typeparam name="T">Component type (must be struct implementing IComponent)</typeparam>
        /// <param name="entity">Entity to add component to</param>
        /// <param name="component">Component value to add</param>
        /// <param name="world">World instance (required)</param>
        /// <exception cref="InvalidEntityException">Thrown if entity is invalid or deallocated</exception>
        /// <exception cref="DuplicateComponentException">Thrown if entity already has a component of type T</exception>
        /// <remarks>
        /// This method implements Core-004: AddComponent functionality.
        /// API-006: Use Exceptions Instead of Try Pattern (COMPLETED)
        /// API-010: World is now required parameter (not optional)
        /// 
        /// Features:
        /// - Entity validation: throws InvalidEntityException if entity is invalid or deallocated
        /// - Duplicate component detection: throws DuplicateComponentException if entity already has this component type
        /// - Efficient entity-to-index mapping via dictionary lookup
        /// - Automatic array growth if capacity is insufficient
        /// - Zero-allocation in hot path (only allocates on array growth)
        /// - Exceptions are safe in builds (minimize overhead, no stack trace if not needed)
        /// 
        /// The component is added at the end of the storage array (index = count),
        /// maintaining contiguous memory layout for cache efficiency.
        /// 
        /// Usage:
        /// <code>
        /// try
        /// {
        ///     ComponentsManager.AddComponent&lt;Hp&gt;(entity, new Hp { Amount = 100f }, world);
        /// }
        /// catch (InvalidEntityException)
        /// {
        ///     // Entity is invalid or deallocated
        /// }
        /// catch (DuplicateComponentException)
        /// {
        ///     // Entity already has this component
        /// }
        /// </code>
        /// </remarks>
        public static void AddComponent<T>(Entity entity, T component, World world) where T : struct, IComponent
        {
            // API-006: Validate entity is valid and allocated
            ValidateEntity(entity, world);

            // Get or create storage for component type T in the specified world
            var table = GetOrCreateTable<T>(world);

            // API-006: Duplicate component detection: throw DuplicateComponentException if entity already has this component type
            if (table.HasComponent(entity))
            {
                throw new DuplicateComponentException(entity, typeof(T));
            }

            // Get internal storage with capacity check (need space for count + 1)
            var currentCount = table.Count;
            var (components, entities, entityToIndex) = table.GetInternalTableForAdd(currentCount + 1);
            ref int count = ref table.GetCountRef();

            // Add component at the end of the array (index = count)
            components[count] = component;
            entities[count] = entity;

            // Update entity-to-index mapping for O(1) lookup
            entityToIndex[entity] = count;

            // Increment count to reflect the new component
            count++;
        }

        /// <summary>
        /// Removes a component from the specified entity in the specified world.
        /// </summary>
        /// <typeparam name="T">Component type (must be struct implementing IComponent)</typeparam>
        /// <param name="entity">Entity to remove component from</param>
        /// <param name="world">World instance (required)</param>
        /// <returns>True if component was removed, false if entity didn't have the component</returns>
        /// <exception cref="InvalidEntityException">Thrown if entity is invalid or deallocated</exception>
        /// <remarks>
        /// This method implements Core-005: RemoveComponent functionality.
        /// API-006: Use Exceptions Instead of Try Pattern (COMPLETED)
        /// API-010: World is now required parameter (not optional)
        /// 
        /// Features:
        /// - Entity validation: throws InvalidEntityException if entity is invalid or deallocated
        /// - Efficient O(1) removal using swap-with-last-element strategy
        /// - Updates entity-to-index mapping automatically
        /// - Zero-allocation in hot path
        /// - Returns false if component doesn't exist (no exception thrown for performance - component absence is not an error)
        /// 
        /// The removal uses swap-with-last strategy:
        /// 1. Find index of component to remove
        /// 2. If not last element, swap with last element
        /// 3. Update mapping for swapped element
        /// 4. Remove entity from dictionary
        /// 5. Decrement count
        /// 
        /// This maintains contiguous memory layout and O(1) removal complexity.
        /// 
        /// Note: Returns false (instead of throwing) if component doesn't exist, as component absence
        /// is not an error condition. Entity validation still throws InvalidEntityException.
        /// </remarks>
        public static bool RemoveComponent<T>(Entity entity, World world) where T : struct, IComponent
        {
            // API-006: Validate entity is valid and allocated
            ValidateEntity(entity, world);

            // Get storage for component type T in the specified world
            var table = GetOrCreateTable<T>(world);

            // Check if entity has this component
            if (!table.HasComponent(entity))
            {
                return false;
            }

            // Perform removal using swap-with-last strategy
            table.RemoveComponentInternal(entity);
            return true;
        }

        /// <summary>
        /// Gets a component for the specified entity in the specified world.
        /// </summary>
        /// <typeparam name="T">Component type (must be struct implementing IComponent)</typeparam>
        /// <param name="entity">Entity to get component for</param>
        /// <param name="world">World instance (required)</param>
        /// <returns>Component value</returns>
        /// <exception cref="InvalidEntityException">Thrown if entity is invalid or deallocated</exception>
        /// <exception cref="ComponentNotFoundException">Thrown if entity doesn't have a component of type T</exception>
        /// <remarks>
        /// This method implements Core-006: GetComponent functionality.
        /// API-002: Keep GetComponent with Exceptions (COMPLETED)
        /// API-006: Use Exceptions Instead of Try Pattern (COMPLETED)
        /// API-010: World is now required parameter (not optional)
        /// 
        /// Features:
        /// - Entity validation: throws InvalidEntityException if entity is invalid or deallocated
        /// - Fast O(1) lookup via entity-to-index mapping dictionary
        /// - Returns component value (T) - throws ComponentNotFoundException if component not found
        /// - Zero-allocation lookup (only dictionary lookup, no allocations)
        /// - Exceptions are safe in builds (minimize overhead, no stack trace if not needed)
        /// 
        /// Usage:
        /// <code>
        /// try
        /// {
        ///     var hp = ComponentsManager.GetComponent&lt;Hp&gt;(entity, world);
        ///     hp.Amount -= 1f;
        /// }
        /// catch (InvalidEntityException)
        /// {
        ///     // Entity is invalid or deallocated
        /// }
        /// catch (ComponentNotFoundException)
        /// {
        ///     // Component doesn't exist
        /// }
        /// </code>
        /// </remarks>
        public static T GetComponent<T>(Entity entity, World world) where T : struct, IComponent
        {
            // API-006: Validate entity is valid and allocated
            ValidateEntity(entity, world);

            // Get storage for component type T in the specified world
            var table = GetOrCreateTable<T>(world);

            // Try to get component using fast lookup
            if (table.TryGetComponent(entity, out T component))
            {
                return component;
            }

            // Entity doesn't have this component, throw exception
            throw new ComponentNotFoundException(entity, typeof(T));
        }

        /// <summary>
        /// Checks if the specified entity has a component of type T in the specified world.
        /// </summary>
        /// <typeparam name="T">Component type (must be struct implementing IComponent)</typeparam>
        /// <param name="entity">Entity to check</param>
        /// <param name="world">World instance (required)</param>
        /// <returns>True if entity has the component, false otherwise</returns>
        /// <exception cref="InvalidEntityException">Thrown if entity is invalid or deallocated</exception>
        /// <remarks>
        /// This method provides a way to check component existence without throwing exceptions.
        /// Useful for conditional logic where you want to avoid exception overhead.
        /// API-006: Use Exceptions Instead of Try Pattern (COMPLETED)
        /// API-010: World is now required parameter (not optional)
        /// 
        /// Features:
        /// - Entity validation: throws InvalidEntityException if entity is invalid or deallocated
        /// - Fast O(1) lookup via entity-to-index mapping dictionary
        /// - Zero-allocation lookup (only dictionary lookup, no allocations)
        /// - Returns false if component doesn't exist (no exception thrown for performance)
        /// 
        /// Usage:
        /// <code>
        /// try
        /// {
        ///     if (ComponentsManager.HasComponent&lt;Hp&gt;(entity, world))
        ///     {
        ///         var hp = ComponentsManager.GetComponent&lt;Hp&gt;(entity, world);
        ///         hp.Amount -= 1f;
        ///     }
        /// }
        /// catch (InvalidEntityException)
        /// {
        ///     // Entity is invalid or deallocated
        /// }
        /// </code>
        /// </remarks>
        public static bool HasComponent<T>(Entity entity, World world) where T : struct, IComponent
        {
            // API-006: Validate entity is valid and allocated
            ValidateEntity(entity, world);

            // Get storage for component type T in the specified world
            var table = GetOrCreateTable<T>(world);
            return table.HasComponent(entity);
        }

        /// <summary>
        /// Gets a modifiable reference to a component for the specified entity in the specified world.
        /// Changes are applied immediately (no deferred application needed for single component).
        /// </summary>
        /// <typeparam name="T">Component type (must be struct implementing IComponent)</typeparam>
        /// <param name="entity">Entity to get component for</param>
        /// <param name="world">World instance (required)</param>
        /// <returns>Reference to the component</returns>
        /// <exception cref="InvalidEntityException">Thrown if entity is invalid or deallocated</exception>
        /// <exception cref="ComponentNotFoundException">Thrown if entity doesn't have a component of type T</exception>
        /// <remarks>
        /// API-010: Added for single component modification support.
        /// 
        /// This method provides direct ref access to a component for modification.
        /// Changes are applied immediately (no deferred application needed for single component).
        /// 
        /// Features:
        /// - Entity validation: throws InvalidEntityException if entity is invalid or deallocated
        /// - Fast O(1) lookup via entity-to-index mapping dictionary
        /// - Returns ref T for direct modification
        /// - Zero-allocation lookup (only dictionary lookup, no allocations)
        /// - Exceptions are safe in builds (minimize overhead, no stack trace if not needed)
        /// 
        /// Usage:
        /// <code>
        /// try
        /// {
        ///     ref var hp = ComponentsManager.GetModifiableComponent&lt;Hp&gt;(entity, world);
        ///     hp.Amount -= 1f; // Direct modification
        /// }
        /// catch (InvalidEntityException)
        /// {
        ///     // Entity is invalid or deallocated
        /// }
        /// catch (ComponentNotFoundException)
        /// {
        ///     // Component doesn't exist
        /// }
        /// </code>
        /// </remarks>
        public static ref T GetModifiableComponent<T>(Entity entity, World world) where T : struct, IComponent
        {
            // API-006: Validate entity is valid and allocated
            ValidateEntity(entity, world);

            // Get storage for component type T in the specified world
            var table = GetOrCreateTable<T>(world);
            
            // Get modifiable reference to component
            return ref table.GetModifiableComponentRef(entity);
        }

        /// <summary>
        /// Gets all components of type T in the specified world as a ReadOnlySpan for zero-allocation iteration.
        /// </summary>
        /// <typeparam name="T">Component type (must be struct implementing IComponent)</typeparam>
        /// <param name="world">World instance (required)</param>
        /// <returns>ReadOnlySpan containing all components of type T</returns>
        /// <remarks>
        /// This method implements Core-007: GetComponents (Single Type Query) functionality.
        /// Perf-002: Query Optimization - Single Component (COMPLETED)
        /// API-010: World is now required parameter (not optional)
        /// 
        /// Features:
        /// - Returns ReadOnlySpan&lt;T&gt; for zero-allocation iteration
        /// - Efficient iteration support over all components of type T
        /// - Handles sparse components (only entities that have the component are included)
        /// 
        /// Perf-002 Optimizations:
        /// - Zero-allocation ReadOnlySpan return: span is created over existing array (no allocations)
        /// - Table reference caching: GetOrCreateTable uses cache to avoid repeated dictionary lookups
        /// - Efficient iteration: ReadOnlySpan provides minimal bounds checking overhead (runtime-optimized)
        /// - Cache benefits: repeated calls to same component type+world use cached table reference
        /// 
        /// The returned span contains all components of type T that are currently stored.
        /// Components are stored contiguously in memory for cache efficiency.
        /// 
        /// Usage:
        /// <code>
        /// // For read-only iteration:
        /// var hpComponents = ComponentsManager.GetComponents&lt;Hp&gt;(world);
        /// foreach (var hp in hpComponents)
        /// {
        ///     // Read component values
        /// }
        /// 
        /// // For modifiable iteration with deferred application:
        /// using (var components = ComponentsManager.GetModifiableComponents&lt;Hp&gt;(world))
        /// {
        ///     for (int i = 0; i &lt; components.Count; i++)
        ///     {
        ///         components[i].Amount -= 1f; // Direct modification via ref
        ///     }
        /// } // Automatically applies all modifications
        /// </code>
        /// 
        /// Note: If no components of type T exist in the specified world, returns an empty span.
        /// 
        /// API-004: GetComponents with multiple type parameters (GetComponents&lt;T1, T2&gt;(), GetComponents&lt;T1, T2, T3&gt;()) 
        /// have been removed. Use the entity-centric pattern with GetEntitiesWith&lt;T1, T2&gt;() and entity.Get&lt;T&gt;() instead.
        /// </remarks>
        public static ReadOnlySpan<T> GetComponents<T>(World world) where T : struct, IComponent
        {
            // Perf-002: GetOrCreateTable uses cache to avoid repeated dictionary lookups
            var table = GetOrCreateTable<T>(world);

            // Perf-002: Zero-allocation ReadOnlySpan return over existing array
            // ReadOnlySpan bounds checking is optimized by the runtime
            return table.GetComponents();
        }

        /// <summary>
        /// Gets all entities that have component type T1 in the specified world.
        /// </summary>
        /// <typeparam name="T1">First component type (must be struct implementing IComponent)</typeparam>
        /// <param name="world">World instance (required)</param>
        /// <returns>ReadOnlySpan containing all entities with component T1</returns>
        /// <remarks>
        /// This method implements API-005: Entity-Component Mapping Support.
        /// API-010: World is now required parameter (not optional)
        /// 
        /// Features:
        /// - Returns ReadOnlySpan&lt;Entity&gt; for zero-allocation iteration
        /// - Returns all entities that have component type T1
        /// 
        /// Usage:
        /// <code>
        /// var entities = ComponentsManager.GetEntitiesWith&lt;Position&gt;(world);
        /// foreach (var entity in entities)
        /// {
        ///     var pos = entity.Get&lt;Position&gt;(world);
        ///     // Process entity with Position component
        /// }
        /// </code>
        /// </remarks>
        public static ReadOnlySpan<Entity> GetEntitiesWith<T1>(World world) where T1 : struct, IComponent
        {
            var table = GetOrCreateTable<T1>(world);
            return table.GetEntities();
        }

        /// <summary>
        /// Gets all entities that have ALL specified component types (T1 AND T2) in the specified world.
        /// </summary>
        /// <typeparam name="T1">First component type (must be struct implementing IComponent)</typeparam>
        /// <typeparam name="T2">Second component type (must be struct implementing IComponent)</typeparam>
        /// <param name="world">World instance (required)</param>
        /// <returns>ReadOnlySpan containing all entities with both T1 and T2 components</returns>
        /// <remarks>
        /// This method implements API-005: Entity-Component Mapping Support.
        /// API-010: World is now required parameter (not optional)
        /// 
        /// Features:
        /// - Returns ReadOnlySpan&lt;Entity&gt; for zero-allocation iteration
        /// - Returns entities that have ALL specified components (AND query)
        /// - Uses efficient set intersection algorithm
        /// 
        /// Usage:
        /// <code>
        /// var entities = ComponentsManager.GetEntitiesWith&lt;Position, Velocity&gt;(world);
        /// foreach (var entity in entities)
        /// {
        ///     var pos = entity.Get&lt;Position&gt;(world);
        ///     var vel = entity.Get&lt;Velocity&gt;(world);
        ///     // Process entities with both Position and Velocity
        /// }
        /// </code>
        /// </remarks>
        public static ReadOnlySpan<Entity> GetEntitiesWith<T1, T2>(World world) 
            where T1 : struct, IComponent 
            where T2 : struct, IComponent
        {
            var table1 = GetOrCreateTable<T1>(world);
            var table2 = GetOrCreateTable<T2>(world);

            // Early exit if either table is empty
            if (table1.Count == 0 || table2.Count == 0)
            {
                return ReadOnlySpan<Entity>.Empty;
            }

            // Use smallest set as base for intersection (Perf-003 optimization)
            HashSet<Entity> intersection;
            ReadOnlySpan<Entity> baseEntities;
            
            if (table1.Count <= table2.Count)
            {
                intersection = table1.GetEntitiesSet();
                baseEntities = table1.GetEntities();
                var set2 = table2.GetEntitiesSet();
                intersection.IntersectWith(set2);
            }
            else
            {
                intersection = table2.GetEntitiesSet();
                baseEntities = table2.GetEntities();
                var set1 = table1.GetEntitiesSet();
                intersection.IntersectWith(set1);
            }

            // Build result array from intersection
            if (intersection.Count == 0)
            {
                return ReadOnlySpan<Entity>.Empty;
            }

            var result = new Entity[intersection.Count];
            int index = 0;
            foreach (var entity in baseEntities)
            {
                if (intersection.Contains(entity))
                {
                    result[index++] = entity;
                }
            }

            return result;
        }

        /// <summary>
        /// Gets all entities that have ALL specified component types (T1 AND T2 AND T3) in the specified world.
        /// </summary>
        /// <typeparam name="T1">First component type (must be struct implementing IComponent)</typeparam>
        /// <typeparam name="T2">Second component type (must be struct implementing IComponent)</typeparam>
        /// <typeparam name="T3">Third component type (must be struct implementing IComponent)</typeparam>
        /// <param name="world">World instance (required)</param>
        /// <returns>ReadOnlySpan containing all entities with T1, T2, and T3 components</returns>
        /// <remarks>
        /// This method implements API-005: Entity-Component Mapping Support.
        /// API-010: World is now required parameter (not optional)
        /// 
        /// Features:
        /// - Returns ReadOnlySpan&lt;Entity&gt; for zero-allocation iteration
        /// - Returns entities that have ALL specified components (AND query)
        /// - Uses efficient set intersection algorithm
        /// 
        /// Usage:
        /// <code>
        /// var entities = ComponentsManager.GetEntitiesWith&lt;Position, Velocity, Health&gt;(world);
        /// foreach (var entity in entities)
        /// {
        ///     var pos = entity.Get&lt;Position&gt;(world);
        ///     var vel = entity.Get&lt;Velocity&gt;(world);
        ///     var health = entity.Get&lt;Health&gt;(world);
        ///     // Process entities with all three components
        /// }
        /// </code>
        /// </remarks>
        public static ReadOnlySpan<Entity> GetEntitiesWith<T1, T2, T3>(World world) 
            where T1 : struct, IComponent 
            where T2 : struct, IComponent
            where T3 : struct, IComponent
        {
            var table1 = GetOrCreateTable<T1>(world);
            var table2 = GetOrCreateTable<T2>(world);
            var table3 = GetOrCreateTable<T3>(world);

            // Early exit if any table is empty
            if (table1.Count == 0 || table2.Count == 0 || table3.Count == 0)
            {
                return ReadOnlySpan<Entity>.Empty;
            }

            // Find smallest table to use as base (Perf-003 optimization)
            int minCount = Math.Min(Math.Min(table1.Count, table2.Count), table3.Count);
            HashSet<Entity> intersection;
            ReadOnlySpan<Entity> baseEntities;

            if (table1.Count == minCount)
            {
                intersection = table1.GetEntitiesSet();
                baseEntities = table1.GetEntities();
                intersection.IntersectWith(table2.GetEntitiesSet());
                intersection.IntersectWith(table3.GetEntitiesSet());
            }
            else if (table2.Count == minCount)
            {
                intersection = table2.GetEntitiesSet();
                baseEntities = table2.GetEntities();
                intersection.IntersectWith(table1.GetEntitiesSet());
                intersection.IntersectWith(table3.GetEntitiesSet());
            }
            else
            {
                intersection = table3.GetEntitiesSet();
                baseEntities = table3.GetEntities();
                intersection.IntersectWith(table1.GetEntitiesSet());
                intersection.IntersectWith(table2.GetEntitiesSet());
            }

            // Build result array from intersection
            if (intersection.Count == 0)
            {
                return ReadOnlySpan<Entity>.Empty;
            }

            var result = new Entity[intersection.Count];
            int index = 0;
            foreach (var entity in baseEntities)
            {
                if (intersection.Contains(entity))
                {
                    result[index++] = entity;
                }
            }

            return result;
        }

        /// <summary>
        /// Gets all entities that have ALL specified component types (T1 AND T2 AND T3 AND T4) in the specified world.
        /// </summary>
        /// <typeparam name="T1">First component type (must be struct implementing IComponent)</typeparam>
        /// <typeparam name="T2">Second component type (must be struct implementing IComponent)</typeparam>
        /// <typeparam name="T3">Third component type (must be struct implementing IComponent)</typeparam>
        /// <typeparam name="T4">Fourth component type (must be struct implementing IComponent)</typeparam>
        /// <param name="world">World instance (required)</param>
        /// <returns>ReadOnlySpan containing all entities with T1, T2, T3, and T4 components</returns>
        /// <remarks>
        /// This method implements API-005: Entity-Component Mapping Support.
        /// API-010: World is now required parameter (not optional)
        /// </remarks>
        public static ReadOnlySpan<Entity> GetEntitiesWith<T1, T2, T3, T4>(World world) 
            where T1 : struct, IComponent 
            where T2 : struct, IComponent
            where T3 : struct, IComponent
            where T4 : struct, IComponent
        {
            var table1 = GetOrCreateTable<T1>(world);
            var table2 = GetOrCreateTable<T2>(world);
            var table3 = GetOrCreateTable<T3>(world);
            var table4 = GetOrCreateTable<T4>(world);

            // Early exit if any table is empty
            if (table1.Count == 0 || table2.Count == 0 || table3.Count == 0 || table4.Count == 0)
            {
                return ReadOnlySpan<Entity>.Empty;
            }

            // Find smallest table to use as base
            int minCount = Math.Min(Math.Min(table1.Count, table2.Count), Math.Min(table3.Count, table4.Count));
            HashSet<Entity> intersection;
            ReadOnlySpan<Entity> baseEntities;

            if (table1.Count == minCount)
            {
                intersection = table1.GetEntitiesSet();
                baseEntities = table1.GetEntities();
                intersection.IntersectWith(table2.GetEntitiesSet());
                intersection.IntersectWith(table3.GetEntitiesSet());
                intersection.IntersectWith(table4.GetEntitiesSet());
            }
            else if (table2.Count == minCount)
            {
                intersection = table2.GetEntitiesSet();
                baseEntities = table2.GetEntities();
                intersection.IntersectWith(table1.GetEntitiesSet());
                intersection.IntersectWith(table3.GetEntitiesSet());
                intersection.IntersectWith(table4.GetEntitiesSet());
            }
            else if (table3.Count == minCount)
            {
                intersection = table3.GetEntitiesSet();
                baseEntities = table3.GetEntities();
                intersection.IntersectWith(table1.GetEntitiesSet());
                intersection.IntersectWith(table2.GetEntitiesSet());
                intersection.IntersectWith(table4.GetEntitiesSet());
            }
            else
            {
                intersection = table4.GetEntitiesSet();
                baseEntities = table4.GetEntities();
                intersection.IntersectWith(table1.GetEntitiesSet());
                intersection.IntersectWith(table2.GetEntitiesSet());
                intersection.IntersectWith(table3.GetEntitiesSet());
            }

            // Build result array from intersection
            if (intersection.Count == 0)
            {
                return ReadOnlySpan<Entity>.Empty;
            }

            var result = new Entity[intersection.Count];
            int index = 0;
            foreach (var entity in baseEntities)
            {
                if (intersection.Contains(entity))
                {
                    result[index++] = entity;
                }
            }

            return result;
        }

        /// <summary>
        /// Gets all entities that have ALL specified component types (T1 AND T2 AND T3 AND T4 AND T5) in the specified world.
        /// </summary>
        /// <typeparam name="T1">First component type (must be struct implementing IComponent)</typeparam>
        /// <typeparam name="T2">Second component type (must be struct implementing IComponent)</typeparam>
        /// <typeparam name="T3">Third component type (must be struct implementing IComponent)</typeparam>
        /// <typeparam name="T4">Fourth component type (must be struct implementing IComponent)</typeparam>
        /// <typeparam name="T5">Fifth component type (must be struct implementing IComponent)</typeparam>
        /// <param name="world">World instance (required)</param>
        /// <returns>ReadOnlySpan containing all entities with T1, T2, T3, T4, and T5 components</returns>
        /// <remarks>
        /// This method implements API-005: Entity-Component Mapping Support.
        /// API-010: World is now required parameter (not optional)
        /// </remarks>
        public static ReadOnlySpan<Entity> GetEntitiesWith<T1, T2, T3, T4, T5>(World world) 
            where T1 : struct, IComponent 
            where T2 : struct, IComponent
            where T3 : struct, IComponent
            where T4 : struct, IComponent
            where T5 : struct, IComponent
        {
            var table1 = GetOrCreateTable<T1>(world);
            var table2 = GetOrCreateTable<T2>(world);
            var table3 = GetOrCreateTable<T3>(world);
            var table4 = GetOrCreateTable<T4>(world);
            var table5 = GetOrCreateTable<T5>(world);

            // Early exit if any table is empty
            if (table1.Count == 0 || table2.Count == 0 || table3.Count == 0 || table4.Count == 0 || table5.Count == 0)
            {
                return ReadOnlySpan<Entity>.Empty;
            }

            // Find smallest table to use as base
            int minCount = Math.Min(Math.Min(Math.Min(table1.Count, table2.Count), Math.Min(table3.Count, table4.Count)), table5.Count);
            HashSet<Entity> intersection;
            ReadOnlySpan<Entity> baseEntities;

            if (table1.Count == minCount)
            {
                intersection = table1.GetEntitiesSet();
                baseEntities = table1.GetEntities();
                intersection.IntersectWith(table2.GetEntitiesSet());
                intersection.IntersectWith(table3.GetEntitiesSet());
                intersection.IntersectWith(table4.GetEntitiesSet());
                intersection.IntersectWith(table5.GetEntitiesSet());
            }
            else if (table2.Count == minCount)
            {
                intersection = table2.GetEntitiesSet();
                baseEntities = table2.GetEntities();
                intersection.IntersectWith(table1.GetEntitiesSet());
                intersection.IntersectWith(table3.GetEntitiesSet());
                intersection.IntersectWith(table4.GetEntitiesSet());
                intersection.IntersectWith(table5.GetEntitiesSet());
            }
            else if (table3.Count == minCount)
            {
                intersection = table3.GetEntitiesSet();
                baseEntities = table3.GetEntities();
                intersection.IntersectWith(table1.GetEntitiesSet());
                intersection.IntersectWith(table2.GetEntitiesSet());
                intersection.IntersectWith(table4.GetEntitiesSet());
                intersection.IntersectWith(table5.GetEntitiesSet());
            }
            else if (table4.Count == minCount)
            {
                intersection = table4.GetEntitiesSet();
                baseEntities = table4.GetEntities();
                intersection.IntersectWith(table1.GetEntitiesSet());
                intersection.IntersectWith(table2.GetEntitiesSet());
                intersection.IntersectWith(table3.GetEntitiesSet());
                intersection.IntersectWith(table5.GetEntitiesSet());
            }
            else
            {
                intersection = table5.GetEntitiesSet();
                baseEntities = table5.GetEntities();
                intersection.IntersectWith(table1.GetEntitiesSet());
                intersection.IntersectWith(table2.GetEntitiesSet());
                intersection.IntersectWith(table3.GetEntitiesSet());
                intersection.IntersectWith(table4.GetEntitiesSet());
            }

            // Build result array from intersection
            if (intersection.Count == 0)
            {
                return ReadOnlySpan<Entity>.Empty;
            }

            var result = new Entity[intersection.Count];
            int index = 0;
            foreach (var entity in baseEntities)
            {
                if (intersection.Contains(entity))
                {
                    result[index++] = entity;
                }
            }

            return result;
        }

        /// <summary>
        /// Gets all entities that have ALL specified component types (T1 AND T2 AND T3 AND T4 AND T5 AND T6) in the specified world.
        /// </summary>
        /// <typeparam name="T1">First component type (must be struct implementing IComponent)</typeparam>
        /// <typeparam name="T2">Second component type (must be struct implementing IComponent)</typeparam>
        /// <typeparam name="T3">Third component type (must be struct implementing IComponent)</typeparam>
        /// <typeparam name="T4">Fourth component type (must be struct implementing IComponent)</typeparam>
        /// <typeparam name="T5">Fifth component type (must be struct implementing IComponent)</typeparam>
        /// <typeparam name="T6">Sixth component type (must be struct implementing IComponent)</typeparam>
        /// <param name="world">World instance (required)</param>
        /// <returns>ReadOnlySpan containing all entities with T1, T2, T3, T4, T5, and T6 components</returns>
        /// <remarks>
        /// This method implements API-005: Entity-Component Mapping Support.
        /// API-010: World is now required parameter (not optional)
        /// </remarks>
        public static ReadOnlySpan<Entity> GetEntitiesWith<T1, T2, T3, T4, T5, T6>(World world) 
            where T1 : struct, IComponent 
            where T2 : struct, IComponent
            where T3 : struct, IComponent
            where T4 : struct, IComponent
            where T5 : struct, IComponent
            where T6 : struct, IComponent
        {
            var table1 = GetOrCreateTable<T1>(world);
            var table2 = GetOrCreateTable<T2>(world);
            var table3 = GetOrCreateTable<T3>(world);
            var table4 = GetOrCreateTable<T4>(world);
            var table5 = GetOrCreateTable<T5>(world);
            var table6 = GetOrCreateTable<T6>(world);

            // Early exit if any table is empty
            if (table1.Count == 0 || table2.Count == 0 || table3.Count == 0 || table4.Count == 0 || table5.Count == 0 || table6.Count == 0)
            {
                return ReadOnlySpan<Entity>.Empty;
            }

            // Find smallest table to use as base
            int minCount = Math.Min(Math.Min(Math.Min(table1.Count, table2.Count), Math.Min(table3.Count, table4.Count)), 
                Math.Min(table5.Count, table6.Count));
            HashSet<Entity> intersection;
            ReadOnlySpan<Entity> baseEntities;

            if (table1.Count == minCount)
            {
                intersection = table1.GetEntitiesSet();
                baseEntities = table1.GetEntities();
                intersection.IntersectWith(table2.GetEntitiesSet());
                intersection.IntersectWith(table3.GetEntitiesSet());
                intersection.IntersectWith(table4.GetEntitiesSet());
                intersection.IntersectWith(table5.GetEntitiesSet());
                intersection.IntersectWith(table6.GetEntitiesSet());
            }
            else if (table2.Count == minCount)
            {
                intersection = table2.GetEntitiesSet();
                baseEntities = table2.GetEntities();
                intersection.IntersectWith(table1.GetEntitiesSet());
                intersection.IntersectWith(table3.GetEntitiesSet());
                intersection.IntersectWith(table4.GetEntitiesSet());
                intersection.IntersectWith(table5.GetEntitiesSet());
                intersection.IntersectWith(table6.GetEntitiesSet());
            }
            else if (table3.Count == minCount)
            {
                intersection = table3.GetEntitiesSet();
                baseEntities = table3.GetEntities();
                intersection.IntersectWith(table1.GetEntitiesSet());
                intersection.IntersectWith(table2.GetEntitiesSet());
                intersection.IntersectWith(table4.GetEntitiesSet());
                intersection.IntersectWith(table5.GetEntitiesSet());
                intersection.IntersectWith(table6.GetEntitiesSet());
            }
            else if (table4.Count == minCount)
            {
                intersection = table4.GetEntitiesSet();
                baseEntities = table4.GetEntities();
                intersection.IntersectWith(table1.GetEntitiesSet());
                intersection.IntersectWith(table2.GetEntitiesSet());
                intersection.IntersectWith(table3.GetEntitiesSet());
                intersection.IntersectWith(table5.GetEntitiesSet());
                intersection.IntersectWith(table6.GetEntitiesSet());
            }
            else if (table5.Count == minCount)
            {
                intersection = table5.GetEntitiesSet();
                baseEntities = table5.GetEntities();
                intersection.IntersectWith(table1.GetEntitiesSet());
                intersection.IntersectWith(table2.GetEntitiesSet());
                intersection.IntersectWith(table3.GetEntitiesSet());
                intersection.IntersectWith(table4.GetEntitiesSet());
                intersection.IntersectWith(table6.GetEntitiesSet());
            }
            else
            {
                intersection = table6.GetEntitiesSet();
                baseEntities = table6.GetEntities();
                intersection.IntersectWith(table1.GetEntitiesSet());
                intersection.IntersectWith(table2.GetEntitiesSet());
                intersection.IntersectWith(table3.GetEntitiesSet());
                intersection.IntersectWith(table4.GetEntitiesSet());
                intersection.IntersectWith(table5.GetEntitiesSet());
            }

            // Build result array from intersection
            if (intersection.Count == 0)
            {
                return ReadOnlySpan<Entity>.Empty;
            }

            var result = new Entity[intersection.Count];
            int index = 0;
            foreach (var entity in baseEntities)
            {
                if (intersection.Contains(entity))
                {
                    result[index++] = entity;
                }
            }

            return result;
        }


        /// <summary>
        /// Gets modifiable components for iteration with automatic deferred application.
        /// Returns a disposable collection that allows direct modification via ref returns.
        /// All modifications are automatically applied when the collection is disposed.
        /// </summary>
        /// <typeparam name="T">Component type (must be struct implementing IComponent)</typeparam>
        /// <param name="world">World instance (required)</param>
        /// <returns>ModifiableComponentCollection that provides ref access to components</returns>
        /// <remarks>
        /// This method implements Core-010: Deferred Component Modifications functionality.
        /// API-010: World is now required parameter (not optional)
        /// 
        /// Features:
        /// - Zero-allocation iteration (uses existing storage arrays)
        /// - Direct ref access to components for modification
        /// - Automatic deferred application when collection is disposed
        /// - No reflection used (type known at compile time)
        /// - Thread-safe (each thread has its own context)
        /// 
        /// Usage:
        /// <code>
        /// using (var components = ComponentsManager.GetModifiableComponents&lt;Hp&gt;(world))
        /// {
        ///     for (int i = 0; i &lt; components.Count; i++)
        ///     {
        ///         components[i].Amount -= 1f; // Direct modification via ref
        ///     }
        /// } // Automatically applies all modifications when disposed
        /// </code>
        /// 
        /// Note: The collection must be disposed (via using statement) to apply modifications.
        /// If not disposed, modifications will be lost.
        /// </remarks>
        public static ModifiableComponentCollection<T> GetModifiableComponents<T>(World world) where T : struct, IComponent
        {
            var table = GetOrCreateTable<T>(world);
            return new ModifiableComponentCollection<T>(table, world);
        }

        /// <summary>
        /// Removes all components for the specified entity in the specified world.
        /// Used for entity destruction to clean up all associated components.
        /// </summary>
        /// <param name="entity">Entity to remove all components from</param>
        /// <param name="world">World instance (required)</param>
        /// <returns>Number of components removed</returns>
        /// <remarks>
        /// This method is used internally by entity destruction (Core-012).
        /// API-010: World is now required parameter (not optional)
        /// 
        /// Features:
        /// - Iterates through all component storages in the world
        /// - Removes all components for the specified entity using interface method (no reflection)
        /// - Returns count of components removed
        /// - Zero-allocation: only dictionary iteration and direct method calls
        /// - Efficient: only checks component types that exist in the world
        /// 
        /// Usage:
        /// <code>
        /// // Called automatically by World.DestroyEntity()
        /// int removedCount = ComponentsManager.RemoveAllComponents(entity, world);
        /// </code>
        /// </remarks>
        internal static int RemoveAllComponents(Entity entity, World world)
        {
            if (world == null)
                throw new ArgumentNullException(nameof(world));

            int removedCount = 0;

            // Get world storage (may not exist if world was never used)
            if (!WorldTables.TryGetValue(world, out var worldTable))
            {
                return 0; // No components to remove
            }

            // Iterate through all component storages in this world
            // Use IComponentTable interface to remove components without reflection
            foreach (var table in worldTable.Values)
            {
                if (table.TryRemoveComponentForEntity(entity))
                {
                    removedCount++;
                }
            }

            return removedCount;
        }

        /// <summary>
        /// Clears all component storage for the specified world.
        /// Removes all components for all entities in the world.
        /// </summary>
        /// <param name="world">World instance to clear</param>
        /// <remarks>
        /// This method is used by World.Destroy() to clean up world resources.
        /// 
        /// Features:
        /// - Removes all component storage for the specified world
        /// - All entities in the world lose their components
        /// - World storage dictionary is removed from registry
        /// - Perf-002: Invalidates table cache entries for this world
        /// 
        /// Usage:
        /// <code>
        /// var localWorld = new World("Local");
        /// // ... use world ... 
        /// ComponentsManager.ClearWorld(localWorld); // Clean up components
        /// </code>
        /// </remarks>
        internal static void ClearWorld(World world)
        {
            if (world == null)
            {
                return;
            }

            // Perf-002: Invalidate cache entries for this world
            var keysToRemove = new List<(World world, Type type)>();
            foreach (var cacheKey in TableCache.Keys)
            {
                if (cacheKey.world.Equals(world))
                {
                    keysToRemove.Add(cacheKey);
                }
            }

            foreach (var key in keysToRemove)
            {
                TableCache.Remove(key);
            }

            WorldTables.Remove(world);
        }

        /// <summary>
        /// Clears all component storage for all worlds.
        /// This is primarily used for testing to reset state between tests.
        /// </summary>
        /// <remarks>
        /// WARNING: This method clears ALL component data from ALL worlds.
        /// Use with caution - typically only for testing scenarios.
        /// 
        /// Perf-002: Also clears the table cache to maintain consistency.
        /// </remarks>
        public static void ClearAll()
        {
            // Perf-002: Clear table cache when clearing all worlds
            TableCache.Clear();
            WorldTables.Clear();
        }
    }
}



