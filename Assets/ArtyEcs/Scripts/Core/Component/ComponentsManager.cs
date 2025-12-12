using System;
using System.Collections.Generic;

namespace ArtyECS.Core
{
    /// <summary>
    /// Manager for ECS components, organized by world scope.
    /// Supports multiple worlds, each with isolated component storage.
    /// </summary>
    /// <remarks>
    /// This class manages component storage per world. Each world has its own
    /// registry of component type -> storage mappings.
    /// 
    /// Core-002: Basic structure with world-scoped storage dictionaries (COMPLETED)
    /// Core-003: Single component type storage with ComponentTable&lt;T&gt; (COMPLETED)
    /// Core-004: AddComponent method implementation (COMPLETED)
    /// Core-005: RemoveComponent method implementation (COMPLETED)
    /// Core-006: GetComponent method for single entity (COMPLETED)
        /// Core-007: GetComponents method for single type query (COMPLETED)
        /// Core-008: GetComponents method for multiple AND query (COMPLETED)
        /// Core-009: GetComponentsWithout query for WITHOUT operations (REMOVED - API-003)
        /// Core-010: Deferred component modifications system (COMPLETED)
    /// Core-012: RemoveAllComponents method for entity destruction (COMPLETED)
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
        /// <param name="world">World instance, or null for global world</param>
        /// <returns>Dictionary mapping component types to their storage instances</returns>
        private static Dictionary<Type, IComponentTable> GetWorldTable(World world = null)
        {
            World targetWorld = world ?? GlobalWorld;

            if (!WorldTables.TryGetValue(targetWorld, out var table))
            {
                table = new Dictionary<Type, IComponentTable>();
                WorldTables[targetWorld] = table;
            }

            return table;
        }

        /// <summary>
        /// Resolves the world instance from the optional parameter.
        /// Returns global world if null is provided.
        /// </summary>
        /// <param name="world">Optional world instance</param>
        /// <returns>World instance to use</returns>
        private static World ResolveWorld(World world = null)
        {
            return world ?? GlobalWorld;
        }

        /// <summary>
        /// Gets or creates the storage instance for a specific component type in the specified world.
        /// Perf-002: Uses table cache to avoid repeated dictionary lookups in hot paths.
        /// </summary>
        /// <typeparam name="T">Component type (must be struct implementing IComponent)</typeparam>
        /// <param name="world">Optional world instance (default: global world)</param>
        /// <returns>Storage instance for component type T</returns>
        /// <remarks>
        /// Creates ComponentTable&lt;T&gt; on first access for the specified world.
        /// 
        /// Perf-002 Optimization:
        /// - First checks table cache for O(1) lookup
        /// - If not in cache, performs dictionary lookup and adds to cache
        /// - Subsequent calls for same world+type use cached reference (zero dictionary lookups)
        /// </remarks>
        internal static ComponentTable<T> GetOrCreateTable<T>(World world = null) where T : struct, IComponent
        {
            World targetWorld = ResolveWorld(world);
            Type componentType = typeof(T);
            var cacheKey = (targetWorld, componentType);

            // Perf-002: Check cache first for fast lookup
            if (TableCache.TryGetValue(cacheKey, out var cachedTable))
            {
                return (ComponentTable<T>)cachedTable;
            }

            // Cache miss: perform dictionary lookup and update cache
            var worldTable = GetWorldTable(targetWorld);

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
        /// <param name="world">World to check, or null for global world</param>
        /// <returns>True if world has been initialized</returns>
        public static bool IsWorldInitialized(World world = null)
        {
            World targetWorld = ResolveWorld(world);
            return WorldTables.ContainsKey(targetWorld);
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
        /// <param name="world">Optional world instance (default: global world)</param>
        /// <exception cref="InvalidOperationException">If entity already has a component of type T (duplicate component)</exception>
        /// <remarks>
        /// This method implements Core-004: AddComponent functionality.
        /// 
        /// Features:
        /// - Duplicate component detection: throws exception if entity already has this component type
        /// - Efficient entity-to-index mapping via dictionary lookup
        /// - Automatic array growth if capacity is insufficient
        /// - Zero-allocation in hot path (only allocates on array growth)
        /// 
        /// The component is added at the end of the storage array (index = count),
        /// maintaining contiguous memory layout for cache efficiency.
        /// </remarks>
        public static void AddComponent<T>(Entity entity, T component, World world = null) where T : struct, IComponent
        {
            // Get or create storage for component type T in the specified world
            var table = GetOrCreateTable<T>(world);

            // Duplicate component detection: check if entity already has this component type
            if (table.HasComponent(entity))
            {
                throw new InvalidOperationException(
                    $"Entity {entity} already has a component of type {typeof(T).Name}. " +
                    "Each entity can have at most one component of each type.");
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
        /// <param name="world">Optional world instance (default: global world)</param>
        /// <returns>True if component was removed, false if entity didn't have the component</returns>
        /// <remarks>
        /// This method implements Core-005: RemoveComponent functionality.
        /// 
        /// Features:
        /// - Efficient O(1) removal using swap-with-last-element strategy
        /// - Updates entity-to-index mapping automatically
        /// - Zero-allocation in hot path
        /// - Returns false if component doesn't exist (no exception thrown for performance)
        /// 
        /// The removal uses swap-with-last strategy:
        /// 1. Find index of component to remove
        /// 2. If not last element, swap with last element
        /// 3. Update mapping for swapped element
        /// 4. Remove entity from dictionary
        /// 5. Decrement count
        /// 
        /// This maintains contiguous memory layout and O(1) removal complexity.
        /// </remarks>
        public static bool RemoveComponent<T>(Entity entity, World world = null) where T : struct, IComponent
        {
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
        /// <param name="world">Optional world instance (default: global world)</param>
        /// <returns>Component value</returns>
        /// <exception cref="ComponentNotFoundException">Thrown if entity doesn't have a component of type T</exception>
        /// <remarks>
        /// This method implements Core-006: GetComponent functionality.
        /// API-002: Keep GetComponent with Exceptions (COMPLETED)
        /// 
        /// Features:
        /// - Fast O(1) lookup via entity-to-index mapping dictionary
        /// - Returns component value (T) - throws ComponentNotFoundException if component not found
        /// - Zero-allocation lookup (only dictionary lookup, no allocations)
        /// - Supports optional World parameter with default to global world
        /// - Exceptions are safe in builds (minimize overhead, no stack trace if not needed)
        /// 
        /// Usage:
        /// <code>
        /// try
        /// {
        ///     var hp = ComponentsManager.GetComponent&lt;Hp&gt;(entity);
        ///     hp.Amount -= 1f;
        /// }
        /// catch (ComponentNotFoundException)
        /// {
        ///     // Component doesn't exist
        /// }
        /// </code>
        /// </remarks>
        public static T GetComponent<T>(Entity entity, World world = null) where T : struct, IComponent
        {
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
        /// <param name="world">Optional world instance (default: global world)</param>
        /// <returns>True if entity has the component, false otherwise</returns>
        /// <remarks>
        /// This method provides a way to check component existence without throwing exceptions.
        /// Useful for conditional logic where you want to avoid exception overhead.
        /// 
        /// Features:
        /// - Fast O(1) lookup via entity-to-index mapping dictionary
        /// - Zero-allocation lookup (only dictionary lookup, no allocations)
        /// - Supports optional World parameter with default to global world
        /// 
        /// Usage:
        /// <code>
        /// if (ComponentsManager.HasComponent&lt;Hp&gt;(entity))
        /// {
        ///     var hp = ComponentsManager.GetComponent&lt;Hp&gt;(entity);
        ///     hp.Amount -= 1f;
        /// }
        /// </code>
        /// </remarks>
        public static bool HasComponent<T>(Entity entity, World world = null) where T : struct, IComponent
        {
            // Get storage for component type T in the specified world
            var table = GetOrCreateTable<T>(world);
            return table.HasComponent(entity);
        }

        /// <summary>
        /// Gets all components of type T in the specified world as a ReadOnlySpan for zero-allocation iteration.
        /// </summary>
        /// <typeparam name="T">Component type (must be struct implementing IComponent)</typeparam>
        /// <param name="world">Optional world instance (default: global world)</param>
        /// <returns>ReadOnlySpan containing all components of type T</returns>
        /// <remarks>
        /// This method implements Core-007: GetComponents (Single Type Query) functionality.
        /// Perf-002: Query Optimization - Single Component (COMPLETED)
        /// 
        /// Features:
        /// - Returns ReadOnlySpan&lt;T&gt; for zero-allocation iteration
        /// - Efficient iteration support over all components of type T
        /// - Handles sparse components (only entities that have the component are included)
        /// - Supports optional World parameter with default to global world
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
        /// var hpComponents = ComponentsManager.GetComponents&lt;Hp&gt;();
        /// foreach (var hp in hpComponents)
        /// {
        ///     // Read component values
        /// }
        /// 
        /// // For modifiable iteration with deferred application:
        /// using (var components = ComponentsManager.GetModifiableComponents&lt;Hp&gt;())
        /// {
        ///     for (int i = 0; i &lt; components.Count; i++)
        ///     {
        ///         components[i].Amount -= 1f; // Direct modification via ref
        ///     }
        /// } // Automatically applies all modifications
        /// </code>
        /// 
        /// Note: If no components of type T exist in the specified world, returns an empty span.
        /// </remarks>
        public static ReadOnlySpan<T> GetComponents<T>(World world = null) where T : struct, IComponent
        {
            // Perf-002: GetOrCreateTable uses cache to avoid repeated dictionary lookups
            var table = GetOrCreateTable<T>(world);

            // Perf-002: Zero-allocation ReadOnlySpan return over existing array
            // ReadOnlySpan bounds checking is optimized by the runtime
            return table.GetComponents();
        }

        /// <summary>
        /// Gets all components of type T1 for entities that have BOTH T1 and T2 components (AND query).
        /// Returns a ReadOnlySpan of T1 components for matching entities.
        /// </summary>
        /// <typeparam name="T1">First component type (must be struct implementing IComponent)</typeparam>
        /// <typeparam name="T2">Second component type (must be struct implementing IComponent)</typeparam>
        /// <param name="world">Optional world instance (default: global world)</param>
        /// <returns>ReadOnlySpan containing T1 components for entities that have both T1 and T2</returns>
        /// <remarks>
        /// This method implements Core-008: GetComponents (Multiple AND Query) functionality.
        /// Perf-003: Query Optimization - Multiple Components (COMPLETED)
        /// 
        /// Features:
        /// - Returns entities that have ALL specified components (AND query)
        /// - Efficient set intersection algorithm using HashSet
        /// - Returns ReadOnlySpan&lt;T1&gt; for zero-allocation iteration
        /// - Supports optional World parameter with default to global world
        /// 
        /// Perf-003 Optimizations:
        /// - Uses smallest set as base for intersection (minimizes HashSet operations)
        /// - Algorithm adapts to which table has fewer entities for optimal performance
        /// - Minimizes HashSet allocations by choosing smallest set first
        /// 
        /// The algorithm:
        /// 1. Gets storage for both component types
        /// 2. Determines which table has fewer entities (uses that as base for efficiency)
        /// 3. Creates HashSet from smallest table
        /// 4. Intersects with entities from larger table (fewer operations)
        /// 5. Builds result array of T1 components for matching entities
        /// 6. Returns span over result array
        /// 
        /// Usage:
        /// <code>
        /// var positionComponents = ComponentsManager.GetComponents&lt;Position, Velocity&gt;();
        /// foreach (var pos in positionComponents)
        /// {
        ///     // This entity has both Position and Velocity components
        ///     pos.X += 1f;
        /// }
        /// </code>
        /// 
        /// Note: If no entities have both components, returns an empty span.
        /// </remarks>
        public static ReadOnlySpan<T1> GetComponents<T1, T2>(World world = null) 
            where T1 : struct, IComponent 
            where T2 : struct, IComponent
        {
            // Get storage for both component types
            var table1 = GetOrCreateTable<T1>(world);
            var table2 = GetOrCreateTable<T2>(world);

            // If either storage is empty, return empty span
            if (table1.Count == 0 || table2.Count == 0)
            {
                return ReadOnlySpan<T1>.Empty;
            }

            // Perf-003: Use smallest set as base for intersection (more efficient)
            HashSet<Entity> intersection;
            ComponentTable<T1> resultTable;
            ReadOnlySpan<Entity> resultEntitiesSpan;
            ReadOnlySpan<T1> resultComponentsSpan;

            if (table1.Count <= table2.Count)
            {
                // T1 is smaller or equal: use T1 as base, intersect with T2
                intersection = table1.GetEntitiesSet();
                var entities2 = table2.GetEntitiesSet();
                intersection.IntersectWith(entities2);
                resultTable = table1;
                resultEntitiesSpan = table1.GetEntities();
                resultComponentsSpan = table1.GetComponents();
            }
            else
            {
                // T2 is smaller: use T2 as base, intersect with T1, but build result from T1
                intersection = table2.GetEntitiesSet();
                var entities1 = table1.GetEntitiesSet();
                intersection.IntersectWith(entities1);
                resultTable = table1;
                resultEntitiesSpan = table1.GetEntities();
                resultComponentsSpan = table1.GetComponents();
            }

            // If no intersection, return empty span
            if (intersection.Count == 0)
            {
                return ReadOnlySpan<T1>.Empty;
            }

            // Build result array: T1 components for matching entities
            var result = new T1[intersection.Count];
            int index = 0;

            for (int i = 0; i < resultEntitiesSpan.Length; i++)
            {
                if (intersection.Contains(resultEntitiesSpan[i]))
                {
                    result[index++] = resultComponentsSpan[i];
                }
            }

            return new ReadOnlySpan<T1>(result);
        }

        /// <summary>
        /// Gets all components of type T1 for entities that have T1, T2, AND T3 components (AND query).
        /// Returns a ReadOnlySpan of T1 components for matching entities.
        /// </summary>
        /// <typeparam name="T1">First component type (must be struct implementing IComponent)</typeparam>
        /// <typeparam name="T2">Second component type (must be struct implementing IComponent)</typeparam>
        /// <typeparam name="T3">Third component type (must be struct implementing IComponent)</typeparam>
        /// <param name="world">Optional world instance (default: global world)</param>
        /// <returns>ReadOnlySpan containing T1 components for entities that have T1, T2, and T3</returns>
        /// <remarks>
        /// This method implements Core-008: GetComponents (Multiple AND Query) functionality.
        /// Perf-003: Query Optimization - Multiple Components (COMPLETED)
        /// 
        /// Features:
        /// - Returns entities that have ALL specified components (AND query)
        /// - Efficient set intersection algorithm using HashSet
        /// - Returns ReadOnlySpan&lt;T1&gt; for zero-allocation iteration
        /// - Supports optional World parameter with default to global world
        /// 
        /// Perf-003 Optimizations:
        /// - Uses smallest set as base for intersection (minimizes HashSet operations)
        /// - Finds minimum of three tables and uses that as intersection base
        /// - Intersects with other two tables in sequence (fewer operations)
        /// - Minimizes HashSet allocations by choosing smallest set first
        /// 
        /// The algorithm:
        /// 1. Gets storage for all three component types
        /// 2. Determines which table has fewest entities (uses that as base for efficiency)
        /// 3. Creates HashSet from smallest table
        /// 4. Intersects with entities from the other two tables (fewer operations)
        /// 5. Builds result array of T1 components for matching entities
        /// 6. Returns span over result array
        /// 
        /// Usage:
        /// <code>
        /// var positionComponents = ComponentsManager.GetComponents&lt;Position, Velocity, Health&gt;();
        /// foreach (var pos in positionComponents)
        /// {
        ///     // This entity has Position, Velocity, and Health components
        ///     pos.X += 1f;
        /// }
        /// </code>
        /// 
        /// Note: If no entities have all three components, returns an empty span.
        /// </remarks>
        public static ReadOnlySpan<T1> GetComponents<T1, T2, T3>(World world = null) 
            where T1 : struct, IComponent 
            where T2 : struct, IComponent
            where T3 : struct, IComponent
        {
            // Get storage for all component types
            var table1 = GetOrCreateTable<T1>(world);
            var table2 = GetOrCreateTable<T2>(world);
            var table3 = GetOrCreateTable<T3>(world);

            // If any storage is empty, return empty span
            if (table1.Count == 0 || table2.Count == 0 || table3.Count == 0)
            {
                return ReadOnlySpan<T1>.Empty;
            }

            // Perf-003: Use smallest set as base for intersection (more efficient)
            // Find which table has the fewest entities
            int count1 = table1.Count;
            int count2 = table2.Count;
            int count3 = table3.Count;

            HashSet<Entity> intersection;
            ReadOnlySpan<Entity> resultEntitiesSpan;
            ReadOnlySpan<T1> resultComponentsSpan;

            if (count1 <= count2 && count1 <= count3)
            {
                // T1 is smallest: use T1 as base, intersect with T2 and T3
                intersection = table1.GetEntitiesSet();
                intersection.IntersectWith(table2.GetEntitiesSet());
                intersection.IntersectWith(table3.GetEntitiesSet());
                resultEntitiesSpan = table1.GetEntities();
                resultComponentsSpan = table1.GetComponents();
            }
            else if (count2 <= count1 && count2 <= count3)
            {
                // T2 is smallest: use T2 as base, intersect with T1 and T3, but build result from T1
                intersection = table2.GetEntitiesSet();
                intersection.IntersectWith(table1.GetEntitiesSet());
                intersection.IntersectWith(table3.GetEntitiesSet());
                resultEntitiesSpan = table1.GetEntities();
                resultComponentsSpan = table1.GetComponents();
            }
            else
            {
                // T3 is smallest: use T3 as base, intersect with T1 and T2, but build result from T1
                intersection = table3.GetEntitiesSet();
                intersection.IntersectWith(table1.GetEntitiesSet());
                intersection.IntersectWith(table2.GetEntitiesSet());
                resultEntitiesSpan = table1.GetEntities();
                resultComponentsSpan = table1.GetComponents();
            }

            // If no intersection, return empty span
            if (intersection.Count == 0)
            {
                return ReadOnlySpan<T1>.Empty;
            }

            // Build result array: T1 components for matching entities
            var result = new T1[intersection.Count];
            int index = 0;

            for (int i = 0; i < resultEntitiesSpan.Length; i++)
            {
                if (intersection.Contains(resultEntitiesSpan[i]))
                {
                    result[index++] = resultComponentsSpan[i];
                }
            }

            return new ReadOnlySpan<T1>(result);
        }

        /// <summary>
        /// Gets modifiable components for iteration with automatic deferred application.
        /// Returns a disposable collection that allows direct modification via ref returns.
        /// All modifications are automatically applied when the collection is disposed.
        /// </summary>
        /// <typeparam name="T">Component type (must be struct implementing IComponent)</typeparam>
        /// <param name="world">Optional world instance (default: global world)</param>
        /// <returns>ModifiableComponentCollection that provides ref access to components</returns>
        /// <remarks>
        /// This method implements Core-010: Deferred Component Modifications functionality.
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
        /// using (var components = ComponentsManager.GetModifiableComponents&lt;Hp&gt;())
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
        public static ModifiableComponentCollection<T> GetModifiableComponents<T>(World world = null) where T : struct, IComponent
        {
            var table = GetOrCreateTable<T>(world);
            return new ModifiableComponentCollection<T>(table, ResolveWorld(world));
        }

        /// <summary>
        /// Removes all components for the specified entity in the specified world.
        /// Used for entity destruction to clean up all associated components.
        /// </summary>
        /// <param name="entity">Entity to remove all components from</param>
        /// <param name="world">Optional world instance (default: global world)</param>
        /// <returns>Number of components removed</returns>
        /// <remarks>
        /// This method is used internally by entity destruction (Core-012).
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
        /// int removedCount = ComponentsManager.RemoveAllComponents(entity);
        /// </code>
        /// </remarks>
        internal static int RemoveAllComponents(Entity entity, World world = null)
        {
            World targetWorld = ResolveWorld(world);
            int removedCount = 0;

            // Get world storage (may not exist if world was never used)
            if (!WorldTables.TryGetValue(targetWorld, out var worldTable))
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



