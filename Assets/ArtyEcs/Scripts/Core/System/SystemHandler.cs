using System;

namespace ArtyECS.Core
{
    /// <summary>
    /// Base class for all ECS systems.
    /// Systems are plain classes that contain game logic and can query/modify components.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <strong>Naming Convention:</strong>
    /// - <c>SystemHandler</c> (capitalized) refers to this base class
    /// - <c>system</c> (lowercase) refers to the general concept of an ECS system
    /// - In code examples and documentation, use <c>SystemHandler</c> when referring to the class,
    ///   and <c>system</c> when referring to the concept or an instance variable
    /// </para>
    /// 
    /// This class implements:
    /// - System-000: System Base Class ✅
    /// - API-001: Fix ExecuteOnce World Parameter ✅
    /// - API-010: World is now required parameter (not optional) ✅
    /// 
    /// Features:
    /// - No abstract methods - systems are just classes that can be instantiated directly
    /// - Support for state - systems can have instance fields to store state
    /// - Optional Execute method - override Execute(World world) to implement system logic
    /// - Systems can be instantiated multiple times if needed (no singleton requirement)
    /// - World context support - Execute() method requires World parameter for scoped execution
    /// 
    /// Usage:
    /// <code>
    /// public class MovementSystem : SystemHandler
    /// {
    ///     public override void Execute(World world)
    ///     {
    ///         // Use world context for component queries
    ///         var positions = ComponentsManager.GetComponents&lt;Position&gt;(world);
    ///         var velocities = ComponentsManager.GetComponents&lt;Velocity&gt;(world);
    ///         // Process movement...
    ///     }
    /// }
    /// 
    /// // Instantiate and use
    /// var movementSystem = new MovementSystem();
    /// var world = World.GetOrCreate();
    /// world.AddToUpdate(movementSystem); // Add to Update queue
    /// 
    /// // Execute in specific world context
    /// var localWorld = new World("Local");
    /// localWorld.ExecuteOnce(movementSystem); // Executes in Local world context
    /// </code>
    /// 
    /// Note: Async execution support will be added in Async-001.
    /// </remarks>
    public class SystemHandler
    {
        /// <summary>
        /// Optional Execute method that can be overridden by derived systems.
        /// This method is called when the system is executed.
        /// </summary>
        /// <param name="world">World context for system execution (required). The system should use this world for component queries.</param>
        /// <remarks>
        /// API-010: World is now required parameter (not optional).
        /// 
        /// By default, this method does nothing. Override it in derived classes
        /// to implement system logic.
        /// 
        /// This method is called by SystemsManager when executing the system queue.
        /// 
        /// World Context:
        /// - World parameter is always provided by SystemsManager when executing systems
        /// - Systems must pass the World parameter to ComponentsManager queries: ComponentsManager.GetComponents&lt;T&gt;(world)
        /// - Systems can use World API methods: world.GetComponents&lt;T&gt;(), world.GetEntitiesWith&lt;T1, T2&gt;(), etc.
        /// 
        /// Usage:
        /// <code>
        /// public class MovementSystem : SystemHandler
        /// {
        ///     public override void Execute(World world)
        ///     {
        ///         // Use world context for component queries
        ///         var positions = world.GetComponents&lt;Position&gt;();
        ///         var velocities = world.GetComponents&lt;Velocity&gt;();
        ///         // Process movement...
        ///     }
        /// }
        /// </code>
        /// 
        /// This method implements:
        /// - API-001: Fix ExecuteOnce World Parameter ✅
        /// - API-010: World is now required parameter (not optional) ✅
        /// </remarks>
        public virtual void Execute(World world)
        {
            // Default implementation does nothing
            // Derived classes should override this method to implement system logic
        }

        /// <summary>
        /// String representation for debugging.
        /// </summary>
        public override string ToString()
        {
            return $"SystemHandler({GetType().Name})";
        }
    }
}

