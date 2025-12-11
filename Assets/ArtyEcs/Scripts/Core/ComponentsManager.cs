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
    /// Core-009: GetComponentsWithout query for WITHOUT operations (COMPLETED)
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
        /// </summary>
        /// <typeparam name="T">Component type (must be struct implementing IComponent)</typeparam>
        /// <param name="world">Optional world instance (default: global world)</param>
        /// <returns>Storage instance for component type T</returns>
        /// <remarks>
        /// Creates ComponentTable&lt;T&gt; on first access for the specified world.
        /// </remarks>
        internal static ComponentTable<T> GetOrCreateTable<T>(World world = null) where T : struct, IComponent
        {
            World targetWorld = ResolveWorld(world);
            var worldTable = GetWorldTable(targetWorld);
            Type componentType = typeof(T);

            if (!worldTable.TryGetValue(componentType, out var table))
            {
                table = new ComponentTable<T>();
                worldTable[componentType] = table;
            }

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
        /// <returns>Component value if found, null if entity doesn't have the component</returns>
        /// <remarks>
        /// This method implements Core-006: GetComponent functionality.
        /// 
        /// Features:
        /// - Fast O(1) lookup via entity-to-index mapping dictionary
        /// - Returns nullable value (T?) - null if component not found, component value if found
        /// - Zero-allocation lookup (only dictionary lookup, no allocations)
        /// - Supports optional World parameter with default to global world
        /// 
        /// Usage:
        /// <code>
        /// var hp = ComponentsManager.GetComponent&lt;Hp&gt;(entity);
        /// if (hp.HasValue)
        /// {
        ///     float currentHp = hp.Value.Amount;
        /// }
        /// </code>
        /// </remarks>
        public static T? GetComponent<T>(Entity entity, World world = null) where T : struct, IComponent
        {
            // Get storage for component type T in the specified world
            var table = GetOrCreateTable<T>(world);

            // Try to get component using fast lookup
            if (table.TryGetComponent(entity, out T component))
            {
                return component;
            }

            // Entity doesn't have this component, return null
            return null;
        }

        /// <summary>
        /// Gets all components of type T in the specified world as a ReadOnlySpan for zero-allocation iteration.
        /// </summary>
        /// <typeparam name="T">Component type (must be struct implementing IComponent)</typeparam>
        /// <param name="world">Optional world instance (default: global world)</param>
        /// <returns>ReadOnlySpan containing all components of type T</returns>
        /// <remarks>
        /// This method implements Core-007: GetComponents (Single Type Query) functionality.
        /// 
        /// Features:
        /// - Returns ReadOnlySpan&lt;T&gt; for zero-allocation iteration
        /// - Efficient iteration support over all components of type T
        /// - Handles sparse components (only entities that have the component are included)
        /// - Supports optional World parameter with default to global world
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
            // Get storage for component type T in the specified world
            var table = GetOrCreateTable<T>(world);

            // Return ReadOnlySpan over all stored components for zero-allocation iteration
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
        /// 
        /// Features:
        /// - Returns entities that have ALL specified components (AND query)
        /// - Efficient set intersection algorithm using HashSet
        /// - Returns ReadOnlySpan&lt;T1&gt; for zero-allocation iteration
        /// - Supports optional World parameter with default to global world
        /// 
        /// The algorithm:
        /// 1. Gets all entities with T1 component
        /// 2. Gets all entities with T2 component
        /// 3. Finds intersection (entities that have both)
        /// 4. Builds result array of T1 components for matching entities
        /// 5. Returns span over result array
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

            // Get entity sets for efficient intersection
            var entities1 = table1.GetEntitiesSet();
            var entities2 = table2.GetEntitiesSet();

            // Find intersection: entities that have both components
            entities1.IntersectWith(entities2);

            // If no intersection, return empty span
            if (entities1.Count == 0)
            {
                return ReadOnlySpan<T1>.Empty;
            }

            // Build result array: T1 components for matching entities
            var result = new T1[entities1.Count];
            int index = 0;
            var entitiesSpan = table1.GetEntities();
            var componentsSpan = table1.GetComponents();

            for (int i = 0; i < entitiesSpan.Length; i++)
            {
                if (entities1.Contains(entitiesSpan[i]))
                {
                    result[index++] = componentsSpan[i];
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
        /// 
        /// Features:
        /// - Returns entities that have ALL specified components (AND query)
        /// - Efficient set intersection algorithm using HashSet
        /// - Returns ReadOnlySpan&lt;T1&gt; for zero-allocation iteration
        /// - Supports optional World parameter with default to global world
        /// 
        /// The algorithm:
        /// 1. Gets all entities with T1 component
        /// 2. Gets all entities with T2 component
        /// 3. Gets all entities with T3 component
        /// 4. Finds intersection (entities that have all three)
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

            // Get entity sets for efficient intersection
            var entities1 = table1.GetEntitiesSet();
            var entities2 = table2.GetEntitiesSet();
            var entities3 = table3.GetEntitiesSet();

            // Find intersection: entities that have all three components
            entities1.IntersectWith(entities2);
            entities1.IntersectWith(entities3);

            // If no intersection, return empty span
            if (entities1.Count == 0)
            {
                return ReadOnlySpan<T1>.Empty;
            }

            // Build result array: T1 components for matching entities
            var result = new T1[entities1.Count];
            int index = 0;
            var entitiesSpan = table1.GetEntities();
            var componentsSpan = table1.GetComponents();

            for (int i = 0; i < entitiesSpan.Length; i++)
            {
                if (entities1.Contains(entitiesSpan[i]))
                {
                    result[index++] = componentsSpan[i];
                }
            }

            return new ReadOnlySpan<T1>(result);
        }

        /// <summary>
        /// Gets all components of type T1 in the specified world.
        /// This is equivalent to GetComponents&lt;T1&gt;() when no exclusions are specified.
        /// </summary>
        /// <typeparam name="T1">Component type (must be struct implementing IComponent)</typeparam>
        /// <param name="world">Optional world instance (default: global world)</param>
        /// <returns>ReadOnlySpan containing all components of type T1</returns>
        /// <remarks>
        /// This method implements Core-009: GetComponentsWithout Query functionality.
        /// 
        /// When no exclusion components are specified, this returns all entities with T1,
        /// which is equivalent to GetComponents&lt;T1&gt;(). This method is provided for
        /// API consistency with the multi-parameter GetComponentsWithout overloads.
        /// 
        /// Usage:
        /// <code>
        /// var allHealth = ComponentsManager.GetComponentsWithout&lt;Health&gt;();
        /// // Equivalent to: ComponentsManager.GetComponents&lt;Health&gt;();
        /// </code>
        /// </remarks>
        public static ReadOnlySpan<T1> GetComponentsWithout<T1>(World world = null) 
            where T1 : struct, IComponent
        {
            // When no exclusions are specified, return all components of type T1
            // This is equivalent to GetComponents<T1>()
            return GetComponents<T1>(world);
        }

        /// <summary>
        /// Gets all components of type T1 for entities that have T1 but NOT T2 (WITHOUT query).
        /// Returns a ReadOnlySpan of T1 components for matching entities.
        /// </summary>
        /// <typeparam name="T1">Component type to query (must be struct implementing IComponent)</typeparam>
        /// <typeparam name="T2">Component type to exclude (must be struct implementing IComponent)</typeparam>
        /// <param name="world">Optional world instance (default: global world)</param>
        /// <returns>ReadOnlySpan containing T1 components for entities that have T1 but NOT T2</returns>
        /// <remarks>
        /// This method implements Core-009: GetComponentsWithout Query functionality.
        /// 
        /// Features:
        /// - Returns entities that have T1 but NOT T2 (WITHOUT query)
        /// - Efficient set difference algorithm using HashSet
        /// - Returns ReadOnlySpan&lt;T1&gt; for zero-allocation iteration
        /// - Supports optional World parameter with default to global world
        /// 
        /// The algorithm:
        /// 1. Gets all entities with T1 component
        /// 2. Gets all entities with T2 component
        /// 3. Finds set difference: entities with T1 that don't have T2
        /// 4. Builds result array of T1 components for matching entities
        /// 5. Returns span over result array
        /// 
        /// Usage:
        /// <code>
        /// var aliveEntities = ComponentsManager.GetComponentsWithout&lt;Health, Dead&gt;();
        /// foreach (var health in aliveEntities)
        /// {
        ///     // This entity has Health but NOT Dead component
        ///     health.Amount -= 1f;
        /// }
        /// </code>
        /// 
        /// Note: If no entities match the criteria, returns an empty span.
        /// </remarks>
        public static ReadOnlySpan<T1> GetComponentsWithout<T1, T2>(World world = null) 
            where T1 : struct, IComponent 
            where T2 : struct, IComponent
        {
            // Get storage for both component types
            var table1 = GetOrCreateTable<T1>(world);
            var table2 = GetOrCreateTable<T2>(world);

            // If T1 storage is empty, return empty span
            if (table1.Count == 0)
            {
                return ReadOnlySpan<T1>.Empty;
            }

            // Get entity sets for efficient set difference
            var entities1 = table1.GetEntitiesSet();
            var entities2 = table2.GetEntitiesSet();

            // Find set difference: entities with T1 that don't have T2
            entities1.ExceptWith(entities2);

            // If no entities remain after exclusion, return empty span
            if (entities1.Count == 0)
            {
                return ReadOnlySpan<T1>.Empty;
            }

            // Build result array: T1 components for matching entities
            var result = new T1[entities1.Count];
            int index = 0;
            var entitiesSpan = table1.GetEntities();
            var componentsSpan = table1.GetComponents();

            for (int i = 0; i < entitiesSpan.Length; i++)
            {
                if (entities1.Contains(entitiesSpan[i]))
                {
                    result[index++] = componentsSpan[i];
                }
            }

            return new ReadOnlySpan<T1>(result);
        }

        /// <summary>
        /// Gets all components of type T1 for entities that have T1 but NOT T2 and NOT T3 (WITHOUT query).
        /// Returns a ReadOnlySpan of T1 components for matching entities.
        /// </summary>
        /// <typeparam name="T1">Component type to query (must be struct implementing IComponent)</typeparam>
        /// <typeparam name="T2">First component type to exclude (must be struct implementing IComponent)</typeparam>
        /// <typeparam name="T3">Second component type to exclude (must be struct implementing IComponent)</typeparam>
        /// <param name="world">Optional world instance (default: global world)</param>
        /// <returns>ReadOnlySpan containing T1 components for entities that have T1 but NOT T2 and NOT T3</returns>
        /// <remarks>
        /// This method implements Core-009: GetComponentsWithout Query functionality.
        /// 
        /// Features:
        /// - Returns entities that have T1 but NOT T2 and NOT T3 (WITHOUT query)
        /// - Efficient set difference algorithm using HashSet
        /// - Returns ReadOnlySpan&lt;T1&gt; for zero-allocation iteration
        /// - Supports optional World parameter with default to global world
        /// 
        /// The algorithm:
        /// 1. Gets all entities with T1 component
        /// 2. Gets all entities with T2 component
        /// 3. Gets all entities with T3 component
        /// 4. Finds set difference: entities with T1 that don't have T2 or T3
        /// 5. Builds result array of T1 components for matching entities
        /// 6. Returns span over result array
        /// 
        /// Usage:
        /// <code>
        /// var activeEntities = ComponentsManager.GetComponentsWithout&lt;Health, Dead, Destroyed&gt;();
        /// foreach (var health in activeEntities)
        /// {
        ///     // This entity has Health but NOT Dead and NOT Destroyed components
        ///     health.Amount -= 1f;
        /// }
        /// </code>
        /// 
        /// Note: If no entities match the criteria, returns an empty span.
        /// </remarks>
        public static ReadOnlySpan<T1> GetComponentsWithout<T1, T2, T3>(World world = null) 
            where T1 : struct, IComponent 
            where T2 : struct, IComponent
            where T3 : struct, IComponent
        {
            // Get storage for all component types
            var table1 = GetOrCreateTable<T1>(world);
            var table2 = GetOrCreateTable<T2>(world);
            var table3 = GetOrCreateTable<T3>(world);

            // If T1 storage is empty, return empty span
            if (table1.Count == 0)
            {
                return ReadOnlySpan<T1>.Empty;
            }

            // Get entity sets for efficient set difference
            var entities1 = table1.GetEntitiesSet();
            var entities2 = table2.GetEntitiesSet();
            var entities3 = table3.GetEntitiesSet();

            // Find set difference: entities with T1 that don't have T2 or T3
            entities1.ExceptWith(entities2);
            entities1.ExceptWith(entities3);

            // If no entities remain after exclusion, return empty span
            if (entities1.Count == 0)
            {
                return ReadOnlySpan<T1>.Empty;
            }

            // Build result array: T1 components for matching entities
            var result = new T1[entities1.Count];
            int index = 0;
            var entitiesSpan = table1.GetEntities();
            var componentsSpan = table1.GetComponents();

            for (int i = 0; i < entitiesSpan.Length; i++)
            {
                if (entities1.Contains(entitiesSpan[i]))
                {
                    result[index++] = componentsSpan[i];
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

            WorldTables.Remove(world);
        }

        /// <summary>
        /// Clears all component storage for all worlds.
        /// This is primarily used for testing to reset state between tests.
        /// </summary>
        /// <remarks>
        /// WARNING: This method clears ALL component data from ALL worlds.
        /// Use with caution - typically only for testing scenarios.
        /// </remarks>
        public static void ClearAll()
        {
            WorldTables.Clear();
        }
    }
}

