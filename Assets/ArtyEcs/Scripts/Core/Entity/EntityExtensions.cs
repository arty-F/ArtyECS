using System;

namespace ArtyECS.Core
{
    /// <summary>
    /// Extension methods for Entity to provide convenient component access API.
    /// </summary>
    /// <remarks>
    /// This class implements API-005: Entity-Component Mapping Support.
    /// 
    /// Features:
    /// - Entity-centric API: query entities first, then get components one at a time
    /// - Extension methods for convenient usage: entity.Get&lt;T&gt;(), entity.Has&lt;T&gt;()
    /// - One component per request (not multiple)
    /// - Exception-based API: Get&lt;T&gt;() throws ComponentNotFoundException if not found
    /// - Supports optional World parameter with default to global world
    /// 
    /// Usage:
    /// <code>
    /// // Entity-centric pattern
    /// var entities = ComponentsManager.GetEntitiesWith&lt;Position, Velocity&gt;();
    /// foreach (var entity in entities)
    /// {
    ///     // Components guaranteed to exist (from GetEntitiesWith filtering)
    ///     var pos = entity.Get&lt;Position&gt;();
    ///     var vel = entity.Get&lt;Velocity&gt;();
    ///     pos.X += vel.X;
    /// }
    /// 
    /// // Or with check:
    /// if (entity.Has&lt;Hp&gt;())
    /// {
    ///     var hp = entity.Get&lt;Hp&gt;();
    ///     hp.Amount -= 1f;
    /// }
    /// 
    /// // With explicit world:
    /// var localWorld = new World("Local");
    /// var pos = entity.Get&lt;Position&gt;(localWorld);
    /// </code>
    /// </remarks>
    public static class EntityExtensions
    {
        /// <summary>
        /// Gets a component of type T for this entity.
        /// </summary>
        /// <typeparam name="T">Component type (must be struct implementing IComponent)</typeparam>
        /// <param name="entity">Entity to get component for</param>
        /// <param name="world">Optional world instance (default: global world)</param>
        /// <returns>Component value</returns>
        /// <exception cref="ComponentNotFoundException">Thrown if entity doesn't have a component of type T</exception>
        /// <remarks>
        /// This method implements API-005: Entity-Component Mapping Support.
        /// API-009: Updated to use World API.
        /// 
        /// Features:
        /// - One component per request (not multiple)
        /// - Returns T (non-nullable) - throws ComponentNotFoundException if not found
        /// - Fast O(1) lookup via entity-to-index mapping
        /// - Zero-allocation lookup (only dictionary lookup)
        /// - Supports optional World parameter with default to global world
        /// 
        /// Usage:
        /// <code>
        /// // Get component (throws if not found)
        /// try
        /// {
        ///     var hp = entity.Get&lt;Hp&gt;();
        ///     hp.Amount -= 1f;
        /// }
        /// catch (ComponentNotFoundException)
        /// {
        ///     // Component doesn't exist
        /// }
        /// 
        /// // Or check first:
        /// if (entity.Has&lt;Hp&gt;())
        /// {
        ///     var hp = entity.Get&lt;Hp&gt;();
        ///     hp.Amount -= 1f;
        /// }
        /// </code>
        /// </remarks>
        public static T Get<T>(this Entity entity, World world = null) where T : struct, IComponent
        {
            world ??= World.GetGlobalWorld();
            return world.GetComponent<T>(entity);
        }

        /// <summary>
        /// Checks if this entity has a component of type T.
        /// </summary>
        /// <typeparam name="T">Component type (must be struct implementing IComponent)</typeparam>
        /// <param name="entity">Entity to check</param>
        /// <param name="world">Optional world instance (default: global world)</param>
        /// <returns>True if entity has the component, false otherwise</returns>
        /// <remarks>
        /// This method implements API-005: Entity-Component Mapping Support.
        /// API-009: Updated to use World API.
        /// 
        /// Features:
        /// - Fast O(1) lookup via entity-to-index mapping
        /// - Zero-allocation lookup (only dictionary lookup)
        /// - Supports optional World parameter with default to global world
        /// - Useful for conditional logic to avoid exception overhead
        /// 
        /// Usage:
        /// <code>
        /// // Check before getting component
        /// if (entity.Has&lt;Hp&gt;())
        /// {
        ///     var hp = entity.Get&lt;Hp&gt;();
        ///     hp.Amount -= 1f;
        /// }
        /// 
        /// // Or use in conditional logic
        /// if (entity.Has&lt;Dead&gt;())
        /// {
        ///     // Entity is dead, skip processing
        ///     return;
        /// }
        /// </code>
        /// </remarks>
        public static bool Has<T>(this Entity entity, World world = null) where T : struct, IComponent
        {
            world ??= World.GetGlobalWorld();
            try
            {
                world.GetComponent<T>(entity);
                return true;
            }
            catch (ComponentNotFoundException)
            {
                return false;
            }
        }

        /// <summary>
        /// Adds a component to this entity.
        /// </summary>
        /// <typeparam name="T">Component type (must be struct implementing IComponent)</typeparam>
        /// <param name="entity">Entity to add component to</param>
        /// <param name="component">Component value to add</param>
        /// <param name="world">Optional world instance (default: global world)</param>
        /// <exception cref="InvalidEntityException">Thrown if entity is invalid or deallocated</exception>
        /// <exception cref="DuplicateComponentException">Thrown if entity already has a component of type T</exception>
        /// <remarks>
        /// This method implements API-009: Unified External API (World + Extension Methods).
        /// 
        /// Usage:
        /// <code>
        /// entity.AddComponent(new Hp { Amount = 100f });
        /// </code>
        /// </remarks>
        public static void AddComponent<T>(this Entity entity, T component, World world = null) where T : struct, IComponent
        {
            world ??= World.GetGlobalWorld();
            world.AddComponent(entity, component);
        }

        /// <summary>
        /// Removes a component from this entity.
        /// </summary>
        /// <typeparam name="T">Component type (must be struct implementing IComponent)</typeparam>
        /// <param name="entity">Entity to remove component from</param>
        /// <param name="world">Optional world instance (default: global world)</param>
        /// <returns>True if component was removed, false if entity didn't have the component</returns>
        /// <exception cref="InvalidEntityException">Thrown if entity is invalid or deallocated</exception>
        /// <remarks>
        /// This method implements API-009: Unified External API (World + Extension Methods).
        /// 
        /// Usage:
        /// <code>
        /// entity.RemoveComponent&lt;Hp&gt;();
        /// </code>
        /// </remarks>
        public static bool RemoveComponent<T>(this Entity entity, World world = null) where T : struct, IComponent
        {
            world ??= World.GetGlobalWorld();
            return world.RemoveComponent<T>(entity);
        }
    }
}

