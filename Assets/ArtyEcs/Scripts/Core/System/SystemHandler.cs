using System;

namespace ArtyECS.Core
{
    /// <summary>
    /// Base class for all ECS systems.
    /// Systems are plain classes that contain game logic and can query/modify components.
    /// </summary>
    /// <remarks>
    /// This class implements:
    /// - System-000: System Base Class ✅
    /// - API-001: Fix ExecuteOnce World Parameter ✅
    /// 
    /// Features:
    /// - No abstract methods - systems are just classes that can be instantiated directly
    /// - Support for state - systems can have instance fields to store state
    /// - Optional Execute method - override Execute(World world = null) to implement system logic
    /// - Systems can be instantiated multiple times if needed (no singleton requirement)
    /// - World context support - Execute() method accepts optional World parameter for scoped execution
    /// 
    /// Usage:
    /// <code>
    /// public class MovementSystem : SystemHandler
    /// {
    ///     public override void Execute(World world = null)
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
    /// movementSystem.AddToUpdate(); // Add to Update queue (System-002)
    /// 
    /// // Execute in specific world context
    /// var localWorld = new World("Local");
    /// movementSystem.ExecuteOnce(localWorld); // Executes in Local world context
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
        /// <param name="world">Optional world context for system execution. When provided, the system should use this world for component queries. When null, the system should use the global world (default behavior for backward compatibility).</param>
        /// <remarks>
        /// By default, this method does nothing. Override it in derived classes
        /// to implement system logic.
        /// 
        /// This method is called by SystemsManager when executing the system queue.
        /// 
        /// World Context:
        /// - When World is provided (e.g., via ExecuteOnce(world)), the system should use this world for component queries
        /// - When World is null (e.g., via queue execution), the system should use the global world (default behavior)
        /// - Systems can pass the World parameter to ComponentsManager queries: ComponentsManager.GetComponents&lt;T&gt;(world)
        /// 
        /// Usage:
        /// <code>
        /// public class MovementSystem : SystemHandler
        /// {
        ///     public override void Execute(World world = null)
        ///     {
        ///         // Use world context for component queries
        ///         var positions = ComponentsManager.GetComponents&lt;Position&gt;(world);
        ///         var velocities = ComponentsManager.GetComponents&lt;Velocity&gt;(world);
        ///         // Process movement...
        ///     }
        /// }
        /// </code>
        /// 
        /// This method implements API-001: Fix ExecuteOnce World Parameter.
        /// </remarks>
        public virtual void Execute(World world = null)
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

