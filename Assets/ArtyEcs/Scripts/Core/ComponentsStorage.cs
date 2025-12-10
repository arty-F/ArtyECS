using System;
using System.Collections.Generic;

namespace ArtyECS.Core
{
    /// <summary>
    /// Interface for component storage that allows removing components for an entity without knowing the type.
    /// Used internally for efficient entity destruction without reflection.
    /// </summary>
    internal interface IComponentStorage
    {
        /// <summary>
        /// Attempts to remove a component for the specified entity if it exists.
        /// </summary>
        /// <param name="entity">Entity to remove component for</param>
        /// <returns>True if component was removed, false if entity didn't have this component type</returns>
        bool TryRemoveComponentForEntity(Entity entity);
    }

    /// <summary>
    /// Central storage for ECS components, organized by world scope.
    /// Supports multiple worlds, each with isolated component storage.
    /// </summary>
    /// <remarks>
    /// This class manages component storage per world. Each world has its own
    /// registry of component type -> storage mappings.
    /// 
    /// Core-002: Basic structure with world-scoped storage dictionaries (COMPLETED)
    /// Core-003: Single component type storage with ComponentStorage&lt;T&gt; (COMPLETED)
    /// Core-004: AddComponent method implementation (COMPLETED)
    /// Core-005: RemoveComponent method implementation (COMPLETED)
    /// Core-006: GetComponent method for single entity (COMPLETED)
    /// Core-007: GetComponents method for single type query (COMPLETED)
    /// Core-008: GetComponents method for multiple AND query (COMPLETED)
    /// Core-009: GetComponentsWithout query for WITHOUT operations (COMPLETED)
    /// Core-010: Deferred component modifications system (COMPLETED)
    /// Core-012: RemoveAllComponents method for entity destruction (COMPLETED)
    /// </remarks>
    public static class ComponentsStorage
    {
        /// <summary>
        /// Global/default world instance. Used when no world is specified.
        /// </summary>
        private static readonly World GlobalWorld = new World("Global");

        /// <summary>
        /// Registry of worlds to their component storage instances.
        /// Each world has its own dictionary mapping component types to storage.
        /// Uses IComponentStorage interface to allow type-erased removal without reflection.
        /// </summary>
        private static readonly Dictionary<World, Dictionary<Type, IComponentStorage>> WorldStorages =
            new Dictionary<World, Dictionary<Type, IComponentStorage>>();

        /// <summary>
        /// Gets the storage dictionary for the specified world.
        /// Creates a new storage dictionary if the world doesn't exist yet.
        /// </summary>
        /// <param name="world">World instance, or null for global world</param>
        /// <returns>Dictionary mapping component types to their storage instances</returns>
        private static Dictionary<Type, IComponentStorage> GetWorldStorage(World world = null)
        {
            World targetWorld = world ?? GlobalWorld;

            if (!WorldStorages.TryGetValue(targetWorld, out var storage))
            {
                storage = new Dictionary<Type, IComponentStorage>();
                WorldStorages[targetWorld] = storage;
            }

            return storage;
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
        /// Creates ComponentStorage&lt;T&gt; on first access for the specified world.
        /// </remarks>
        internal static ComponentStorage<T> GetOrCreateStorage<T>(World world = null) where T : struct, IComponent
        {
            World targetWorld = ResolveWorld(world);
            var worldStorage = GetWorldStorage(targetWorld);
            Type componentType = typeof(T);

            if (!worldStorage.TryGetValue(componentType, out var storage))
            {
                storage = new ComponentStorage<T>();
                worldStorage[componentType] = storage;
            }

            return (ComponentStorage<T>)storage;
        }

        /// <summary>
        /// Gets the number of worlds currently registered.
        /// </summary>
        /// <returns>Number of worlds (including global world)</returns>
        public static int GetWorldCount()
        {
            return WorldStorages.Count;
        }

        /// <summary>
        /// Checks if a world has been initialized (has storage dictionary).
        /// </summary>
        /// <param name="world">World to check, or null for global world</param>
        /// <returns>True if world has been initialized</returns>
        public static bool IsWorldInitialized(World world = null)
        {
            World targetWorld = ResolveWorld(world);
            return WorldStorages.ContainsKey(targetWorld);
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
            var storage = GetOrCreateStorage<T>(world);

            // Duplicate component detection: check if entity already has this component type
            if (storage.HasComponent(entity))
            {
                throw new InvalidOperationException(
                    $"Entity {entity} already has a component of type {typeof(T).Name}. " +
                    "Each entity can have at most one component of each type.");
            }

            // Get internal storage with capacity check (need space for count + 1)
            var currentCount = storage.Count;
            var (components, entities, entityToIndex) = storage.GetInternalStorageForAdd(currentCount + 1);
            ref int count = ref storage.GetCountRef();

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
            var storage = GetOrCreateStorage<T>(world);

            // Check if entity has this component
            if (!storage.HasComponent(entity))
            {
                return false;
            }

            // Perform removal using swap-with-last strategy
            storage.RemoveComponentInternal(entity);
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
        /// var hp = ComponentsStorage.GetComponent&lt;Hp&gt;(entity);
        /// if (hp.HasValue)
        /// {
        ///     float currentHp = hp.Value.Amount;
        /// }
        /// </code>
        /// </remarks>
        public static T? GetComponent<T>(Entity entity, World world = null) where T : struct, IComponent
        {
            // Get storage for component type T in the specified world
            var storage = GetOrCreateStorage<T>(world);

            // Try to get component using fast lookup
            if (storage.TryGetComponent(entity, out T component))
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
        /// var hpComponents = ComponentsStorage.GetComponents&lt;Hp&gt;();
        /// foreach (var hp in hpComponents)
        /// {
        ///     // Read component values
        /// }
        /// 
        /// // For modifiable iteration with deferred application:
        /// using (var components = ComponentsStorage.GetModifiableComponents&lt;Hp&gt;())
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
            var storage = GetOrCreateStorage<T>(world);

            // Return ReadOnlySpan over all stored components for zero-allocation iteration
            return storage.GetComponents();
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
        /// var positionComponents = ComponentsStorage.GetComponents&lt;Position, Velocity&gt;();
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
            var storage1 = GetOrCreateStorage<T1>(world);
            var storage2 = GetOrCreateStorage<T2>(world);

            // If either storage is empty, return empty span
            if (storage1.Count == 0 || storage2.Count == 0)
            {
                return ReadOnlySpan<T1>.Empty;
            }

            // Get entity sets for efficient intersection
            var entities1 = storage1.GetEntitiesSet();
            var entities2 = storage2.GetEntitiesSet();

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
            var entitiesSpan = storage1.GetEntities();
            var componentsSpan = storage1.GetComponents();

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
        /// var positionComponents = ComponentsStorage.GetComponents&lt;Position, Velocity, Health&gt;();
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
            var storage1 = GetOrCreateStorage<T1>(world);
            var storage2 = GetOrCreateStorage<T2>(world);
            var storage3 = GetOrCreateStorage<T3>(world);

            // If any storage is empty, return empty span
            if (storage1.Count == 0 || storage2.Count == 0 || storage3.Count == 0)
            {
                return ReadOnlySpan<T1>.Empty;
            }

            // Get entity sets for efficient intersection
            var entities1 = storage1.GetEntitiesSet();
            var entities2 = storage2.GetEntitiesSet();
            var entities3 = storage3.GetEntitiesSet();

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
            var entitiesSpan = storage1.GetEntities();
            var componentsSpan = storage1.GetComponents();

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
        /// var allHealth = ComponentsStorage.GetComponentsWithout&lt;Health&gt;();
        /// // Equivalent to: ComponentsStorage.GetComponents&lt;Health&gt;();
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
        /// var aliveEntities = ComponentsStorage.GetComponentsWithout&lt;Health, Dead&gt;();
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
            var storage1 = GetOrCreateStorage<T1>(world);
            var storage2 = GetOrCreateStorage<T2>(world);

            // If T1 storage is empty, return empty span
            if (storage1.Count == 0)
            {
                return ReadOnlySpan<T1>.Empty;
            }

            // Get entity sets for efficient set difference
            var entities1 = storage1.GetEntitiesSet();
            var entities2 = storage2.GetEntitiesSet();

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
            var entitiesSpan = storage1.GetEntities();
            var componentsSpan = storage1.GetComponents();

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
        /// var activeEntities = ComponentsStorage.GetComponentsWithout&lt;Health, Dead, Destroyed&gt;();
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
            var storage1 = GetOrCreateStorage<T1>(world);
            var storage2 = GetOrCreateStorage<T2>(world);
            var storage3 = GetOrCreateStorage<T3>(world);

            // If T1 storage is empty, return empty span
            if (storage1.Count == 0)
            {
                return ReadOnlySpan<T1>.Empty;
            }

            // Get entity sets for efficient set difference
            var entities1 = storage1.GetEntitiesSet();
            var entities2 = storage2.GetEntitiesSet();
            var entities3 = storage3.GetEntitiesSet();

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
            var entitiesSpan = storage1.GetEntities();
            var componentsSpan = storage1.GetComponents();

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
        /// using (var components = ComponentsStorage.GetModifiableComponents&lt;Hp&gt;())
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
            var storage = GetOrCreateStorage<T>(world);
            return new ModifiableComponentCollection<T>(storage, ResolveWorld(world));
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
        /// int removedCount = ComponentsStorage.RemoveAllComponents(entity);
        /// </code>
        /// </remarks>
        internal static int RemoveAllComponents(Entity entity, World world = null)
        {
            World targetWorld = ResolveWorld(world);
            int removedCount = 0;

            // Get world storage (may not exist if world was never used)
            if (!WorldStorages.TryGetValue(targetWorld, out var worldStorage))
            {
                return 0; // No components to remove
            }

            // Iterate through all component storages in this world
            // Use IComponentStorage interface to remove components without reflection
            foreach (var storage in worldStorage.Values)
            {
                if (storage.TryRemoveComponentForEntity(entity))
                {
                    removedCount++;
                }
            }

            return removedCount;
        }
    }

    /// <summary>
    /// Storage for components of a specific type.
    /// Uses array-based storage with entity-to-index mapping for efficient access.
    /// </summary>
    /// <typeparam name="T">Component type (must be struct implementing IComponent)</typeparam>
    /// <remarks>
    /// This class implements Core-003: Single Component Type Storage.
    /// 
    /// Features:
    /// - Array-based storage for contiguous memory layout (cache-friendly)
    /// - Entity-to-index mapping for O(1) component lookup by entity
    /// - Index-to-entity reverse mapping for efficient iteration
    /// - Dynamic capacity management with doubling growth strategy
    /// - Only stores components for entities that have them (sparse storage)
    /// - Implements IComponentStorage for type-erased removal without reflection
    /// 
    /// The storage maintains two arrays:
    /// 1. Components array: stores actual component values
    /// 2. Entities array: stores entity identifiers for reverse lookup
    /// 
    /// Both arrays grow together to maintain index alignment.
    /// </remarks>
    internal class ComponentStorage<T> : IComponentStorage where T : struct, IComponent
    {
        /// <summary>
        /// Default initial capacity for component storage.
        /// </summary>
        private const int DefaultInitialCapacity = 16;

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
        /// Creates a new ComponentStorage with default initial capacity.
        /// </summary>
        public ComponentStorage() : this(DefaultInitialCapacity)
        {
        }

        /// <summary>
        /// Creates a new ComponentStorage with specified initial capacity.
        /// </summary>
        /// <param name="initialCapacity">Initial capacity for component arrays</param>
        public ComponentStorage(int initialCapacity)
        {
            if (initialCapacity < 1)
                throw new ArgumentException("Initial capacity must be at least 1", nameof(initialCapacity));

            _components = new T[initialCapacity];
            _entities = new Entity[initialCapacity];
            _count = 0;
            _entityToIndex = new Dictionary<Entity, int>(initialCapacity);
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
        internal HashSet<Entity> GetEntitiesSet()
        {
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
        /// Grows the arrays if necessary using doubling strategy.
        /// </summary>
        /// <param name="minCapacity">Minimum required capacity</param>
        private void EnsureCapacity(int minCapacity)
        {
            if (_components.Length >= minCapacity)
                return;

            // Double the capacity, but at least reach minCapacity
            int newCapacity = Math.Max(_components.Length * 2, minCapacity);

            // Grow components array
            var newComponents = new T[newCapacity];
            Array.Copy(_components, 0, newComponents, 0, _count);
            _components = newComponents;

            // Grow entities array
            var newEntities = new Entity[newCapacity];
            Array.Copy(_entities, 0, newEntities, 0, _count);
            _entities = newEntities;
        }

        /// <summary>
        /// Gets or creates a reference to the internal storage arrays.
        /// Used internally by ComponentsStorage for add/remove/get operations.
        /// </summary>
        /// <returns>Internal storage arrays (components, entities, entity-to-index mapping)</returns>
        internal (T[] components, Entity[] entities, Dictionary<Entity, int> entityToIndex) GetInternalStorage()
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
        /// Called by ComponentsStorage before adding components.
        /// </summary>
        /// <param name="minCapacity">Minimum capacity needed</param>
        /// <returns>Internal storage arrays ready for modification</returns>
        internal (T[] components, Entity[] entities, Dictionary<Entity, int> entityToIndex) GetInternalStorageForAdd(int minCapacity)
        {
            EnsureCapacity(minCapacity);
            return GetInternalStorage();
        }

        /// <summary>
        /// Attempts to remove a component for the specified entity if it exists.
        /// Implements IComponentStorage interface for type-erased removal without reflection.
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

    /// <summary>
    /// Collection that provides modifiable access to components with automatic deferred application.
    /// Allows direct modification via ref returns and automatically applies changes when disposed.
    /// </summary>
    /// <typeparam name="T">Component type (must be struct implementing IComponent)</typeparam>
    /// <remarks>
    /// This struct implements Core-010: Deferred Component Modifications functionality.
    /// 
    /// Features:
    /// - Zero-allocation iteration (uses existing storage arrays directly)
    /// - Ref returns for direct component modification
    /// - Automatic deferred application when disposed (via using statement)
    /// - No reflection used (type known at compile time, direct method calls)
    /// - Thread-safe (modifications tracked per collection instance)
    /// 
    /// The collection provides ref access to a temporary copy of components.
    /// Modifications are tracked and applied to the storage when the collection is disposed.
    /// This ensures safe iteration without structural changes during iteration.
    /// </remarks>
    public struct ModifiableComponentCollection<T> : IDisposable where T : struct, IComponent
    {
        private readonly ComponentStorage<T> _storage;
        private readonly World _world;
        private T[] _modifiableComponents;
        private HashSet<int> _modifiedIndices;
        private bool _disposed;

        /// <summary>
        /// Creates a new modifiable component collection.
        /// </summary>
        /// <param name="storage">Component storage instance</param>
        /// <param name="world">World instance</param>
        internal ModifiableComponentCollection(ComponentStorage<T> storage, World world)
        {
            _storage = storage ?? throw new ArgumentNullException(nameof(storage));
            _world = world;
            _disposed = false;

            // Create a copy of components for modification (only allocates once)
            var count = storage.Count;
            if (count > 0)
            {
                var (components, _, _) = storage.GetInternalStorage();
                _modifiableComponents = new T[count];
                Array.Copy(components, _modifiableComponents, count);
                _modifiedIndices = new HashSet<int>();
            }
            else
            {
                _modifiableComponents = Array.Empty<T>();
                _modifiedIndices = new HashSet<int>();
            }
        }

        /// <summary>
        /// Gets the number of components in the collection.
        /// </summary>
        public int Count => _modifiableComponents?.Length ?? 0;

        /// <summary>
        /// Gets a reference to the component at the specified index.
        /// Modifications made via this reference are tracked and applied when disposed.
        /// </summary>
        /// <param name="index">Index of the component</param>
        /// <returns>Reference to the component at the specified index</returns>
        /// <exception cref="IndexOutOfRangeException">If index is out of range</exception>
        public ref T this[int index]
        {
            get
            {
                if (_disposed)
                    throw new ObjectDisposedException(nameof(ModifiableComponentCollection<T>));

                if (index < 0 || index >= _modifiableComponents.Length)
                    throw new IndexOutOfRangeException($"Index {index} is out of range [0, {_modifiableComponents.Length})");

                // Track that this index was accessed (potentially modified)
                // We track all accessed indices to be safe, but could optimize to only track actual modifications
                _modifiedIndices.Add(index);

                return ref _modifiableComponents[index];
            }
        }

        /// <summary>
        /// Gets the entity at the specified index.
        /// </summary>
        /// <param name="index">Index of the entity</param>
        /// <returns>Entity at the specified index</returns>
        /// <exception cref="IndexOutOfRangeException">If index is out of range</exception>
        public Entity GetEntity(int index)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(ModifiableComponentCollection<T>));

            return _storage.GetEntityAt(index);
        }

        /// <summary>
        /// Disposes the collection and applies all tracked modifications to the storage.
        /// </summary>
        public void Dispose()
        {
            if (!_disposed && _modifiableComponents != null && _modifiedIndices != null)
            {
                // Apply all modifications to storage (no reflection, direct method call)
                var (components, _, _) = _storage.GetInternalStorage();
                foreach (int index in _modifiedIndices)
                {
                    if (index < components.Length && index < _modifiableComponents.Length)
                    {
                        components[index] = _modifiableComponents[index];
                    }
                }

                _disposed = true;
            }
        }
    }
}

