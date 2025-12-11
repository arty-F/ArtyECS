using System;

namespace ArtyECS.Core
{
    /// <summary>
    /// Base class for all ECS systems.
    /// Systems are plain classes that contain game logic and can query/modify components.
    /// </summary>
    /// <remarks>
    /// This class implements System-000: System Base Class.
    /// 
    /// Features:
    /// - No abstract methods - systems are just classes that can be instantiated directly
    /// - Support for state - systems can have instance fields to store state
    /// - Optional Execute method - override Execute() to implement system logic
    /// - Systems can be instantiated multiple times if needed (no singleton requirement)
    /// 
    /// Usage:
    /// <code>
    /// public class MovementSystem : SystemHandler
    /// {
    ///     public override void Execute()
    ///     {
        ///         var positions = ComponentsManager.GetComponents&lt;Position&gt;();
        ///         var velocities = ComponentsManager.GetComponents&lt;Velocity&gt;();
    ///         // Process movement...
    ///     }
    /// }
    /// 
    /// // Instantiate and use
    /// var movementSystem = new MovementSystem();
    /// movementSystem.AddToUpdate(); // Add to Update queue (System-002)
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
        /// <remarks>
        /// By default, this method does nothing. Override it in derived classes
        /// to implement system logic.
        /// 
        /// This method is called by SystemsManager when executing the system queue.
        /// </remarks>
        public virtual void Execute()
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

