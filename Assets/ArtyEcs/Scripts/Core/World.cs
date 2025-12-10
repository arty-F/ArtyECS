using System;

namespace ArtyECS.Core
{
    /// <summary>
    /// Represents an ECS World scope for component and system isolation.
    /// Multiple worlds can exist simultaneously, each with its own component and system storage.
    /// </summary>
    /// <remarks>
    /// This is a minimal implementation for Core-002. Full implementation will be completed in World-000.
    /// Core-012: Entity Creation/Destruction API (COMPLETED)
    /// </remarks>
    public class World
    {
        /// <summary>
        /// World identifier/name for identification and debugging.
        /// </summary>
        public readonly string Name;

        /// <summary>
        /// Creates a new World with the specified name.
        /// </summary>
        /// <param name="name">World identifier/name</param>
        public World(string name)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
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
        /// 
        /// The returned entity is ready to use - you can immediately add components to it.
        /// 
        /// Usage:
        /// <code>
        /// // Create entity in global world
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
        /// Clears all ECS state (components and entity pools) for all worlds.
        /// This is primarily used for testing to reset state between tests.
        /// </summary>
        /// <remarks>
        /// WARNING: This method clears ALL ECS data from ALL worlds.
        /// All entities become invalid, all components are removed.
        /// Use with caution - typically only for testing scenarios.
        /// </remarks>
        public static void ClearAllECSState()
        {
            ComponentsRegistry.ClearAll();
            EntityPool.ClearAll();
        }
    }
}

