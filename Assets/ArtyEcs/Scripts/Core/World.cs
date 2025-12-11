using System;
using System.Collections.Generic;

namespace ArtyECS.Core
{
    /// <summary>
    /// Represents an ECS World scope for component and system isolation.
    /// Multiple worlds can exist simultaneously, each with its own component and system storage.
    /// </summary>
    /// <remarks>
    /// This class implements World-000: World Class Implementation.
    /// World-003: World Persistence Across Scenes (COMPLETED)
    /// 
    /// Features:
    /// - World identifier/name for identification and debugging
    /// - Support for multiple worlds with isolated component and system storage
    /// - World-scoped ComponentsRegistry and SystemsRegistry integration
    /// - World lifecycle management (create, destroy)
    /// - Default global world singleton with lazy initialization
    /// - Shared global world instance across ComponentsRegistry and SystemsRegistry
    /// - **Persistence across Unity scene changes**: ECS World data (components, entities, systems) 
    ///   persists across scene loads/unloads automatically via static storage and DontDestroyOnLoad
    /// 
    /// Core-012: Entity Creation/Destruction API (COMPLETED)
    /// 
    /// Scene Persistence:
    /// The ECS World persists across Unity scene changes automatically:
    /// - Component data is stored in static dictionaries in ComponentsRegistry (persists between scenes)
    /// - System queues are stored in static dictionaries in SystemsRegistry (persists between scenes)
    /// - Entity pools are stored in static dictionaries in EntityPool (persists between scenes)
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
        /// Shared across ComponentsRegistry, SystemsRegistry, and EntityPool.
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
        /// Gets the global/default world instance.
        /// Creates the instance on first access (lazy initialization).
        /// This is a singleton instance shared across ComponentsRegistry, SystemsRegistry, and EntityPool.
        /// </summary>
        /// <returns>Global world instance</returns>
        /// <remarks>
        /// This method implements World-000: Global World Singleton.
        /// 
        /// The global world is created lazily on first access and reused for all subsequent calls.
        /// This ensures that ComponentsRegistry, SystemsRegistry, and EntityPool all use the same
        /// global world instance for consistency.
        /// 
        /// Thread-safe: uses double-checked locking pattern for lazy initialization.
        /// 
        /// Usage:
        /// <code>
        /// var globalWorld = World.GetGlobalWorld();
        /// var entity = World.CreateEntity(); // Uses global world by default
        /// </code>
        /// </remarks>
        public static World GetGlobalWorld()
        {
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

        /// <summary>
        /// Creates a new entity in the specified world.
        /// Allocates an entity from the entity pool and returns it.
        /// </summary>
        /// <param name="world">Optional world instance (default: global world)</param>
        /// <returns>Newly created entity</returns>
        /// <remarks>
        /// This method implements Core-012: Entity Creation/Destruction API.
        /// 
        /// Features:
        /// - Fast O(1) entity allocation from entity pool
        /// - ID recycling support via EntityPool
        /// - World-scoped entity allocation
        /// - Zero-allocation in hot path
        /// - Automatically creates UpdateProvider when first entity is created
        /// 
        /// The returned entity is ready to use - you can immediately add components to it.
        /// 
        /// Usage:
        /// <code>
        /// // Create entity in global world - UpdateProvider created automatically here
        /// var entity = World.CreateEntity();
        /// ComponentsRegistry.AddComponent&lt;Position&gt;(entity, new Position { X = 1f, Y = 2f, Z = 3f });
        /// 
        /// // Create entity in scoped world
        /// var localWorld = new World("Local");
        /// var localEntity = World.CreateEntity(localWorld);
        /// </code>
        /// </remarks>
        public static Entity CreateEntity(World world = null)
        {
            // Ensure UpdateProvider is created when first entity is created
            UpdateProvider.EnsureCreated();
            
            return EntityPool.Allocate(world);
        }

        /// <summary>
        /// Destroys an entity, removing all its components and returning it to the entity pool.
        /// </summary>
        /// <param name="entity">Entity to destroy</param>
        /// <param name="world">Optional world instance (default: global world)</param>
        /// <returns>True if entity was destroyed, false if entity was invalid or already destroyed</returns>
        /// <remarks>
        /// This method implements Core-012: Entity Creation/Destruction API.
        /// 
        /// Features:
        /// - Automatic component cleanup (removes all components for the entity)
        /// - Entity deallocation (returns entity ID to pool)
        /// - Fast operation: O(n) where n is number of component types (typically small)
        /// - World-scoped entity destruction
        /// 
        /// The destruction process:
        /// 1. Removes all components for the entity from ComponentsRegistry
        /// 2. Deallocates the entity, returning its ID to the pool
        /// 3. Increments generation number to invalidate old references
        /// 
        /// After destruction, the entity becomes invalid and should not be used.
        /// Any components that were attached to the entity are automatically cleaned up.
        /// 
        /// Usage:
        /// <code>
        /// // Destroy entity in global world
        /// World.DestroyEntity(entity);
        /// 
        /// // Destroy entity in scoped world
        /// var localWorld = new World("Local");
        /// World.DestroyEntity(localEntity, localWorld);
        /// </code>
        /// 
        /// Note: Destroying an entity that has already been destroyed or is invalid returns false.
        /// </remarks>
        public static bool DestroyEntity(Entity entity, World world = null)
        {
            if (!entity.IsValid)
            {
                return false;
            }

            // Remove all components for this entity (automatic cleanup)
            ComponentsRegistry.RemoveAllComponents(entity, world);

            // Deallocate entity and return ID to pool
            return EntityPool.Deallocate(entity, world);
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
        /// 1. Removes all components from ComponentsRegistry for this world
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
            ComponentsRegistry.ClearWorld(world);
            EntityPool.ClearWorld(world);
            SystemsRegistry.ClearWorld(world);

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
            ComponentsRegistry.ClearAll();
            EntityPool.ClearAll();
            SystemsRegistry.ClearAll();
            _destroyedWorlds.Clear();
        }
    }
}

