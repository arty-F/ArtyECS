using System;
using System.Collections.Generic;

namespace ArtyECS.Core
{
    /// <summary>
    /// Represents an ECS World scope for component and system isolation.
    /// Multiple worlds can exist simultaneously, each with its own component and system storage.
    /// 
    /// **API-009, API-010: This is the main public API for the ECS framework.**
    /// All operations should be performed through World class methods (static for global world, instance for scoped worlds).
    /// All manager methods require World parameter (passed through World API).
    /// </summary>
    /// <remarks>
    /// This class implements:
    /// - World-000: World Class Implementation ✅
    /// - World-003: World Persistence Across Scenes ✅
    /// - API-009: Unified External API (World + Extension Methods) ✅
    /// - API-010: World as Required Parameter and System Management ✅
    /// 
    /// Features:
    /// - World identifier/name for identification and debugging
    /// - Support for multiple worlds with isolated component and system storage
    /// - World-scoped ComponentsManager and SystemsManager integration
    /// - World lifecycle management (create, destroy)
    /// - Default global world singleton with lazy initialization
    /// - Shared global world instance across ComponentsManager and SystemsManager
    /// - **Persistence across Unity scene changes**: ECS World data (components, entities, systems) 
    ///   persists across scene loads/unloads automatically via static storage and DontDestroyOnLoad
    /// 
    /// Core-012: Entity Creation/Destruction API (COMPLETED)
    /// 
    /// Scene Persistence:
    /// The ECS World persists across Unity scene changes automatically:
    /// - Component data is stored in static dictionaries in ComponentsManager (persists between scenes)
    /// - System queues are stored in static dictionaries in SystemsManager (persists between scenes)
    /// - Entity pools are stored in static dictionaries in EntitiesManager (persists between scenes)
    /// - UpdateProvider uses DontDestroyOnLoad to persist across scene changes
    /// - All ECS data (entities, components, systems) remains valid after scene transitions
    /// 
    /// This means that entities and components created in one scene will still exist after loading
    /// a new scene. This is by design - the ECS World is independent of Unity's scene system.
    /// If you need to clear ECS data on scene change, explicitly call World.ClearAllECSState() 
    /// or World.Destroy() for specific worlds.
    /// </remarks>
    public class World
    {
        /// <summary>
        /// Global/default world instance. Used when no world is specified.
        /// Lazy initialization ensures thread-safe singleton pattern.
        /// Shared across ComponentsManager, SystemsManager, and EntitiesManager.
        /// </summary>
        private static World _globalWorld;

        /// <summary>
        /// Lock object for thread-safe lazy initialization of global world.
        /// </summary>
        private static readonly object _globalWorldLock = new object();

        /// <summary>
        /// Set of destroyed worlds. Used to track which worlds have been destroyed
        /// to prevent double-destruction and handle worlds that were created but never used.
        /// </summary>
        private static readonly HashSet<World> _destroyedWorlds = new HashSet<World>();

        /// <summary>
        /// World identifier/name for identification and debugging.
        /// </summary>
        public readonly string Name;

        /// <summary>
        /// Creates a new World with the specified name.
        /// </summary>
        /// <param name="name">World identifier/name</param>
        /// <exception cref="ArgumentNullException">Thrown when name is null</exception>
        public World(string name)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }

        /// <summary>
        /// Dictionary to store named worlds (for GetOrCreate).
        /// </summary>
        private static readonly Dictionary<string, World> _namedWorlds = new Dictionary<string, World>();

        /// <summary>
        /// Lock object for thread-safe access to _namedWorlds.
        /// </summary>
        private static readonly object _namedWorldsLock = new object();

        /// <summary>
        /// Gets or creates a world instance by name.
        /// If name is null or empty, returns the global world.
        /// If world with the specified name doesn't exist, creates a new one.
        /// </summary>
        /// <param name="name">World name (null or empty for global world)</param>
        /// <returns>World instance</returns>
        /// <remarks>
        /// This method implements API-009: Unified External API (World + Extension Methods).
        /// 
        /// Features:
        /// - Returns global world if name is null or empty
        /// - Creates new world if name doesn't exist
        /// - Returns existing world if name already exists
        /// - Thread-safe world creation
        /// 
        /// Usage:
        /// <code>
        /// // Get global world
        /// var globalWorld = World.GetOrCreate();
        /// var entity = globalWorld.CreateEntity();
        /// 
        /// // Get or create named world
        /// var localWorld = World.GetOrCreate("Local");
        /// var localEntity = localWorld.CreateEntity();
        /// </code>
        /// </remarks>
        public static World GetOrCreate(string name = null)
        {
            // Return global world if name is null or empty
            if (string.IsNullOrEmpty(name))
            {
                // Lazy initialization of global world
                if (_globalWorld == null)
                {
                    lock (_globalWorldLock)
                    {
                        if (_globalWorld == null)
                        {
                            _globalWorld = new World("Global");
                        }
                    }
                }
                return _globalWorld;
            }

            // Check if world already exists
            lock (_namedWorldsLock)
            {
                if (_namedWorlds.TryGetValue(name, out var existingWorld))
                {
                    return existingWorld;
                }

                // Create new world
                var newWorld = new World(name);
                _namedWorlds[name] = newWorld;
                return newWorld;
            }
        }




        // ========== API-009: World Instance Methods ==========
        // These methods use 'this World' context

        /// <summary>
        /// Creates a new entity in this world.
        /// Allocates an entity from the entity pool and returns it.
        /// </summary>
        /// <returns>Newly created entity</returns>
        /// <remarks>
        /// This method implements API-009: Unified External API (World + Extension Methods).
        /// 
        /// Features:
        /// - Fast O(1) entity allocation from entity pool
        /// - ID recycling support via EntitiesManager
        /// - Zero-allocation in hot path
        /// - Automatically creates UpdateProvider when first entity is created
        /// 
        /// Usage:
        /// <code>
        /// var world = World.GetOrCreate("MyWorld");
        /// var entity = world.CreateEntity();
        /// world.AddComponent&lt;Position&gt;(entity, new Position { X = 1f, Y = 2f, Z = 3f });
        /// </code>
        /// </remarks>
        public Entity CreateEntity()
        {
            // Ensure UpdateProvider is created when first entity is created
            UpdateProvider.EnsureCreated();
            
            return EntitiesManager.Allocate(this);
        }

        /// <summary>
        /// Destroys an entity in this world, removing all its components and returning it to the entity pool.
        /// </summary>
        /// <param name="entity">Entity to destroy</param>
        /// <returns>True if entity was destroyed, false if entity was invalid or already destroyed</returns>
        /// <remarks>
        /// This method implements API-009: Unified External API (World + Extension Methods).
        /// 
        /// Features:
        /// - Automatic component cleanup (removes all components for the entity)
        /// - Entity deallocation (returns entity ID to pool)
        /// - Fast operation: O(n) where n is number of component types (typically small)
        /// 
        /// Usage:
        /// <code>
        /// var world = World.GetOrCreate("MyWorld");
        /// var entity = world.CreateEntity();
        /// // ... use entity ...
        /// world.DestroyEntity(entity);
        /// </code>
        /// </remarks>
        public bool DestroyEntity(Entity entity)
        {
            if (!entity.IsValid)
            {
                return false;
            }

            // Remove all components for this entity (automatic cleanup)
            ComponentsManager.RemoveAllComponents(entity, this);

            // Deallocate entity and return ID to pool
            return EntitiesManager.Deallocate(entity, this);
        }

        /// <summary>
        /// Gets all entities that have component type T1 in this world.
        /// </summary>
        /// <typeparam name="T1">Component type (must be struct implementing IComponent)</typeparam>
        /// <returns>ReadOnlySpan containing all entities with component T1</returns>
        /// <remarks>
        /// This method implements API-009: Unified External API (World + Extension Methods).
        /// </remarks>
        public ReadOnlySpan<Entity> GetEntitiesWith<T1>() where T1 : struct, IComponent
        {
            return ComponentsManager.GetEntitiesWith<T1>(this);
        }

        /// <summary>
        /// Gets all entities that have ALL specified component types (T1 AND T2) in this world.
        /// </summary>
        /// <typeparam name="T1">First component type (must be struct implementing IComponent)</typeparam>
        /// <typeparam name="T2">Second component type (must be struct implementing IComponent)</typeparam>
        /// <returns>ReadOnlySpan containing all entities with both T1 and T2 components</returns>
        /// <remarks>
        /// This method implements API-009: Unified External API (World + Extension Methods).
        /// </remarks>
        public ReadOnlySpan<Entity> GetEntitiesWith<T1, T2>() 
            where T1 : struct, IComponent 
            where T2 : struct, IComponent
        {
            return ComponentsManager.GetEntitiesWith<T1, T2>(this);
        }

        /// <summary>
        /// Gets all entities that have ALL specified component types (T1 AND T2 AND T3) in this world.
        /// </summary>
        /// <typeparam name="T1">First component type (must be struct implementing IComponent)</typeparam>
        /// <typeparam name="T2">Second component type (must be struct implementing IComponent)</typeparam>
        /// <typeparam name="T3">Third component type (must be struct implementing IComponent)</typeparam>
        /// <returns>ReadOnlySpan containing all entities with T1, T2, and T3 components</returns>
        /// <remarks>
        /// This method implements API-009: Unified External API (World + Extension Methods).
        /// </remarks>
        public ReadOnlySpan<Entity> GetEntitiesWith<T1, T2, T3>() 
            where T1 : struct, IComponent 
            where T2 : struct, IComponent
            where T3 : struct, IComponent
        {
            return ComponentsManager.GetEntitiesWith<T1, T2, T3>(this);
        }

        /// <summary>
        /// Gets all entities that have ALL specified component types (T1 AND T2 AND T3 AND T4) in this world.
        /// </summary>
        /// <typeparam name="T1">First component type (must be struct implementing IComponent)</typeparam>
        /// <typeparam name="T2">Second component type (must be struct implementing IComponent)</typeparam>
        /// <typeparam name="T3">Third component type (must be struct implementing IComponent)</typeparam>
        /// <typeparam name="T4">Fourth component type (must be struct implementing IComponent)</typeparam>
        /// <returns>ReadOnlySpan containing all entities with T1, T2, T3, and T4 components</returns>
        /// <remarks>
        /// This method implements API-009: Unified External API (World + Extension Methods).
        /// </remarks>
        public ReadOnlySpan<Entity> GetEntitiesWith<T1, T2, T3, T4>() 
            where T1 : struct, IComponent 
            where T2 : struct, IComponent
            where T3 : struct, IComponent
            where T4 : struct, IComponent
        {
            return ComponentsManager.GetEntitiesWith<T1, T2, T3, T4>(this);
        }

        /// <summary>
        /// Gets all entities that have ALL specified component types (T1 AND T2 AND T3 AND T4 AND T5) in this world.
        /// </summary>
        /// <typeparam name="T1">First component type (must be struct implementing IComponent)</typeparam>
        /// <typeparam name="T2">Second component type (must be struct implementing IComponent)</typeparam>
        /// <typeparam name="T3">Third component type (must be struct implementing IComponent)</typeparam>
        /// <typeparam name="T4">Fourth component type (must be struct implementing IComponent)</typeparam>
        /// <typeparam name="T5">Fifth component type (must be struct implementing IComponent)</typeparam>
        /// <returns>ReadOnlySpan containing all entities with T1, T2, T3, T4, and T5 components</returns>
        /// <remarks>
        /// This method implements API-009: Unified External API (World + Extension Methods).
        /// </remarks>
        public ReadOnlySpan<Entity> GetEntitiesWith<T1, T2, T3, T4, T5>() 
            where T1 : struct, IComponent 
            where T2 : struct, IComponent
            where T3 : struct, IComponent
            where T4 : struct, IComponent
            where T5 : struct, IComponent
        {
            return ComponentsManager.GetEntitiesWith<T1, T2, T3, T4, T5>(this);
        }

        /// <summary>
        /// Gets all entities that have ALL specified component types (T1 AND T2 AND T3 AND T4 AND T5 AND T6) in this world.
        /// </summary>
        /// <typeparam name="T1">First component type (must be struct implementing IComponent)</typeparam>
        /// <typeparam name="T2">Second component type (must be struct implementing IComponent)</typeparam>
        /// <typeparam name="T3">Third component type (must be struct implementing IComponent)</typeparam>
        /// <typeparam name="T4">Fourth component type (must be struct implementing IComponent)</typeparam>
        /// <typeparam name="T5">Fifth component type (must be struct implementing IComponent)</typeparam>
        /// <typeparam name="T6">Sixth component type (must be struct implementing IComponent)</typeparam>
        /// <returns>ReadOnlySpan containing all entities with T1, T2, T3, T4, T5, and T6 components</returns>
        /// <remarks>
        /// This method implements API-009: Unified External API (World + Extension Methods).
        /// </remarks>
        public ReadOnlySpan<Entity> GetEntitiesWith<T1, T2, T3, T4, T5, T6>() 
            where T1 : struct, IComponent 
            where T2 : struct, IComponent
            where T3 : struct, IComponent
            where T4 : struct, IComponent
            where T5 : struct, IComponent
            where T6 : struct, IComponent
        {
            return ComponentsManager.GetEntitiesWith<T1, T2, T3, T4, T5, T6>(this);
        }

        /// <summary>
        /// Gets a component of type T for the specified entity in this world.
        /// </summary>
        /// <typeparam name="T">Component type (must be struct implementing IComponent)</typeparam>
        /// <param name="entity">Entity to get component for</param>
        /// <returns>Component value</returns>
        /// <exception cref="ComponentNotFoundException">Thrown if entity doesn't have a component of type T</exception>
        /// <remarks>
        /// This method implements API-009: Unified External API (World + Extension Methods).
        /// API-010: World is now required parameter in ComponentsManager (passed as this).
        /// </remarks>
        public T GetComponent<T>(Entity entity) where T : struct, IComponent
        {
            return ComponentsManager.GetComponent<T>(entity, this);
        }

        /// <summary>
        /// Gets a modifiable reference to a component of type T for the specified entity in this world.
        /// Changes are applied immediately (no deferred application needed for single component).
        /// </summary>
        /// <typeparam name="T">Component type (must be struct implementing IComponent)</typeparam>
        /// <param name="entity">Entity to get component for</param>
        /// <returns>Reference to the component</returns>
        /// <exception cref="ComponentNotFoundException">Thrown if entity doesn't have a component of type T</exception>
        /// <remarks>
        /// API-010: Added for single component modification support.
        /// 
        /// This method provides direct ref access to a component for modification.
        /// Changes are applied immediately (no deferred application needed for single component).
        /// 
        /// Usage:
        /// <code>
        /// ref var hp = world.GetModifiableComponent&lt;Hp&gt;(entity);
        /// hp.Amount -= 1f; // Direct modification
        /// </code>
        /// </remarks>
        public ref T GetModifiableComponent<T>(Entity entity) where T : struct, IComponent
        {
            return ref ComponentsManager.GetModifiableComponent<T>(entity, this);
        }

        /// <summary>
        /// Adds a component to the specified entity in this world.
        /// </summary>
        /// <typeparam name="T">Component type (must be struct implementing IComponent)</typeparam>
        /// <param name="entity">Entity to add component to</param>
        /// <param name="component">Component value to add</param>
        /// <exception cref="InvalidEntityException">Thrown if entity is invalid or deallocated</exception>
        /// <exception cref="DuplicateComponentException">Thrown if entity already has a component of type T</exception>
        /// <remarks>
        /// This method implements API-009: Unified External API (World + Extension Methods).
        /// </remarks>
        public void AddComponent<T>(Entity entity, T component) where T : struct, IComponent
        {
            ComponentsManager.AddComponent(entity, component, this);
        }

        /// <summary>
        /// Removes a component from the specified entity in this world.
        /// </summary>
        /// <typeparam name="T">Component type (must be struct implementing IComponent)</typeparam>
        /// <param name="entity">Entity to remove component from</param>
        /// <returns>True if component was removed, false if entity didn't have the component</returns>
        /// <exception cref="InvalidEntityException">Thrown if entity is invalid or deallocated</exception>
        /// <remarks>
        /// This method implements API-009: Unified External API (World + Extension Methods).
        /// </remarks>
        public bool RemoveComponent<T>(Entity entity) where T : struct, IComponent
        {
            return ComponentsManager.RemoveComponent<T>(entity, this);
        }

        /// <summary>
        /// Gets all components of type T in this world as a ReadOnlySpan for zero-allocation iteration.
        /// </summary>
        /// <typeparam name="T">Component type (must be struct implementing IComponent)</typeparam>
        /// <returns>ReadOnlySpan containing all components of type T</returns>
        /// <remarks>
        /// This method implements API-009: Unified External API (World + Extension Methods).
        /// </remarks>
        public ReadOnlySpan<T> GetComponents<T>() where T : struct, IComponent
        {
            return ComponentsManager.GetComponents<T>(this);
        }

        /// <summary>
        /// Gets modifiable components for iteration with automatic deferred application in this world.
        /// Returns a disposable collection that allows direct modification via ref returns.
        /// All modifications are automatically applied when the collection is disposed.
        /// </summary>
        /// <typeparam name="T">Component type (must be struct implementing IComponent)</typeparam>
        /// <returns>ModifiableComponentCollection that provides ref access to components</returns>
        /// <remarks>
        /// This method implements API-009: Unified External API (World + Extension Methods).
        /// </remarks>
        public ModifiableComponentCollection<T> GetModifiableComponents<T>() where T : struct, IComponent
        {
            return ComponentsManager.GetModifiableComponents<T>(this);
        }

        /// <summary>
        /// Adds a system to the end of the Update queue for this world.
        /// </summary>
        /// <param name="system">SystemHandler instance to add to the Update queue</param>
        /// <remarks>
        /// This method implements API-009: Unified External API (World + Extension Methods).
        /// </remarks>
        public void AddToUpdate(SystemHandler system)
        {
            SystemsManager.AddToUpdate(system, this);
        }

        /// <summary>
        /// Inserts a system at the specified index in the Update queue for this world.
        /// All systems at and after the specified index will be shifted forward.
        /// </summary>
        /// <param name="system">SystemHandler instance to insert into the Update queue</param>
        /// <param name="order">Index at which to insert the system (0-based)</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if order is negative or greater than queue count</exception>
        /// <remarks>
        /// This method implements API-009: Unified External API (World + Extension Methods).
        /// </remarks>
        public void AddToUpdate(SystemHandler system, int order)
        {
            SystemsManager.AddToUpdate(system, order, this);
        }

        /// <summary>
        /// Adds a system to the end of the FixedUpdate queue for this world.
        /// </summary>
        /// <param name="system">SystemHandler instance to add to the FixedUpdate queue</param>
        /// <remarks>
        /// This method implements API-009: Unified External API (World + Extension Methods).
        /// </remarks>
        public void AddToFixedUpdate(SystemHandler system)
        {
            SystemsManager.AddToFixedUpdate(system, this);
        }

        /// <summary>
        /// Inserts a system at the specified index in the FixedUpdate queue for this world.
        /// All systems at and after the specified index will be shifted forward.
        /// </summary>
        /// <param name="system">SystemHandler instance to insert into the FixedUpdate queue</param>
        /// <param name="order">Index at which to insert the system (0-based)</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if order is negative or greater than queue count</exception>
        /// <remarks>
        /// This method implements API-009: Unified External API (World + Extension Methods).
        /// </remarks>
        public void AddToFixedUpdate(SystemHandler system, int order)
        {
            SystemsManager.AddToFixedUpdate(system, order, this);
        }

        /// <summary>
        /// Executes a system immediately in this world, bypassing all queues.
        /// </summary>
        /// <param name="system">SystemHandler instance to execute immediately</param>
        /// <remarks>
        /// This method implements API-009: Unified External API (World + Extension Methods).
        /// API-010: World is now required parameter in SystemsManager (passed as this).
        /// </remarks>
        public void ExecuteOnce(SystemHandler system)
        {
            SystemsManager.ExecuteOnce(system, this);
        }

        /// <summary>
        /// Executes all systems in the Update queue for this world in order.
        /// Systems are executed sequentially (index 0, 1, 2, ...).
        /// </summary>
        /// <remarks>
        /// API-010: Added for world-specific system execution.
        /// 
        /// Execution behavior:
        /// - Systems are executed in the order they appear in the queue (index 0, 1, 2, ...)
        /// - World context is passed to each system's Execute(world) method
        /// - If a system throws an exception, execution continues with the next system
        /// - Errors are logged but do not stop queue execution (graceful error handling)
        /// - Empty queues are handled gracefully (no-op if queue is empty)
        /// 
        /// Usage:
        /// <code>
        /// // Execute Update systems for this world
        /// world.ExecuteUpdate();
        /// </code>
        /// </remarks>
        public void ExecuteUpdate()
        {
            SystemsManager.ExecuteUpdate(this);
        }

        /// <summary>
        /// Executes all systems in the FixedUpdate queue for this world in order.
        /// Systems are executed sequentially (index 0, 1, 2, ...).
        /// </summary>
        /// <remarks>
        /// API-010: Added for world-specific system execution.
        /// 
        /// Execution behavior:
        /// - Systems are executed in the order they appear in the queue (index 0, 1, 2, ...)
        /// - World context is passed to each system's Execute(world) method
        /// - If a system throws an exception, execution continues with the next system
        /// - Errors are logged but do not stop queue execution (graceful error handling)
        /// - Empty queues are handled gracefully (no-op if queue is empty)
        /// 
        /// Usage:
        /// <code>
        /// // Execute FixedUpdate systems for this world
        /// world.ExecuteFixedUpdate();
        /// </code>
        /// </remarks>
        public void ExecuteFixedUpdate()
        {
            SystemsManager.ExecuteFixedUpdate(this);
        }

        /// <summary>
        /// Removes a system from the Update queue for this world.
        /// </summary>
        /// <param name="system">SystemHandler instance to remove from the Update queue</param>
        /// <returns>True if system was removed, false if system was not found in the queue</returns>
        /// <remarks>
        /// API-010: Added for system removal support.
        /// 
        /// This method removes the first occurrence of the system from the Update queue.
        /// If the system appears multiple times in the queue, only the first occurrence is removed.
        /// 
        /// Usage:
        /// <code>
        /// var movementSystem = new MovementSystem();
        /// world.AddToUpdate(movementSystem);
        /// // ... later ...
        /// world.RemoveFromUpdate(movementSystem); // Remove from Update queue
        /// </code>
        /// </remarks>
        public bool RemoveFromUpdate(SystemHandler system)
        {
            return SystemsManager.RemoveFromUpdate(system, this);
        }

        /// <summary>
        /// Removes a system from the FixedUpdate queue for this world.
        /// </summary>
        /// <param name="system">SystemHandler instance to remove from the FixedUpdate queue</param>
        /// <returns>True if system was removed, false if system was not found in the queue</returns>
        /// <remarks>
        /// API-010: Added for system removal support.
        /// 
        /// This method removes the first occurrence of the system from the FixedUpdate queue.
        /// If the system appears multiple times in the queue, only the first occurrence is removed.
        /// 
        /// Usage:
        /// <code>
        /// var physicsSystem = new PhysicsSystem();
        /// world.AddToFixedUpdate(physicsSystem);
        /// // ... later ...
        /// world.RemoveFromFixedUpdate(physicsSystem); // Remove from FixedUpdate queue
        /// </code>
        /// </remarks>
        public bool RemoveFromFixedUpdate(SystemHandler system)
        {
            return SystemsManager.RemoveFromFixedUpdate(system, this);
        }

        /// <summary>
        /// String representation for debugging.
        /// </summary>
        public override string ToString()
        {
            return $"World({Name})";
        }

        /// <summary>
        /// Equality comparison based on reference (worlds are compared by instance).
        /// </summary>
        public override bool Equals(object obj)
        {
            return ReferenceEquals(this, obj);
        }

        /// <summary>
        /// Hash code based on instance reference.
        /// </summary>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        /// <summary>
        /// Destroys a world, cleaning up all its resources (components, entities, and systems).
        /// After destruction, the world should not be used.
        /// </summary>
        /// <param name="world">World instance to destroy</param>
        /// <returns>True if world was destroyed, false if world was null or is the global world</returns>
        /// <remarks>
        /// This method implements World-000: World lifecycle management.
        /// 
        /// Features:
        /// - Removes all components for all entities in the world
        /// - Clears all entity pools for the world
        /// - Clears all system queues (Update and FixedUpdate) for the world
        /// - Prevents destruction of global world (returns false)
        /// 
        /// The destruction process:
        /// 1. Removes all components from ComponentsManager for this world
        /// 2. Clears entity pool for this world
        /// 3. Clears system queues for this world
        /// 
        /// After destruction, the world instance should not be used for new operations.
        /// However, the World object itself is not disposed (it's a regular class, not IDisposable).
        /// 
        /// Usage:
        /// <code>
        /// var localWorld = new World("Local");
        /// // ... use world ...
        /// World.Destroy(localWorld); // Clean up all resources
        /// </code>
        /// 
        /// Note: The global world cannot be destroyed (returns false if attempted).
        /// </remarks>
        public static bool Destroy(World world)
        {
            if (world == null)
            {
                return false;
            }

            // Prevent destruction of global world
            if (ReferenceEquals(world, _globalWorld))
            {
                return false;
            }

            // Check if world was already destroyed
            if (_destroyedWorlds.Contains(world))
            {
                return false; // World was already destroyed
            }

            // Clean up all resources for this world
            ComponentsManager.ClearWorld(world);
            EntitiesManager.ClearWorld(world);
            SystemsManager.ClearWorld(world);

            // Remove from named worlds if it exists
            lock (_namedWorldsLock)
            {
                _namedWorlds.Remove(world.Name);
            }

            // Mark world as destroyed
            _destroyedWorlds.Add(world);

            return true;
        }

        /// <summary>
        /// Clears all ECS state (components, entity pools, and system queues) for all worlds.
        /// This is primarily used for testing to reset state between tests.
        /// </summary>
        /// <remarks>
        /// WARNING: This method clears ALL ECS data from ALL worlds.
        /// All entities become invalid, all components are removed, all system queues are cleared.
        /// Use with caution - typically only for testing scenarios.
        /// </remarks>
        public static void ClearAllECSState()
        {
            ComponentsManager.ClearAll();
            EntitiesManager.ClearAll();
            SystemsManager.ClearAll();
            _destroyedWorlds.Clear();
        }
    }
}

