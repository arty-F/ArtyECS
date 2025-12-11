namespace ArtyECS.Core
{
    /// <summary>
    /// Extension methods for System class to provide convenient API for queue management and execution.
    /// These methods call the corresponding static methods in SystemsRegistry.
    /// </summary>
    /// <remarks>
    /// These extension methods implement:
    /// - System-002: Update Queue Management ✅
    /// - System-003: FixedUpdate Queue Management ✅
    /// - System-004: Manual Execution ✅
    /// 
    /// They provide a fluent API for adding systems to queues and executing systems manually.
    /// 
    /// Usage:
    /// <code>
    /// var movementSystem = new MovementSystem();
    /// movementSystem.AddToUpdate(); // Add to end of Update queue
    /// movementSystem.AddToUpdate(3); // Insert at index 3 in Update queue
    /// 
    /// var physicsSystem = new PhysicsSystem();
    /// physicsSystem.AddToFixedUpdate(); // Add to end of FixedUpdate queue
    /// physicsSystem.AddToFixedUpdate(2); // Insert at index 2 in FixedUpdate queue
    /// 
    /// var initializationSystem = new InitializationSystem();
    /// initializationSystem.ExecuteOnce(); // Execute immediately without adding to queue
    /// </code>
    /// </remarks>
    public static class SystemExtensions
    {
        /// <summary>
        /// Adds this system to the end of the Update queue for the specified world.
        /// </summary>
        /// <param name="system">System instance (this)</param>
        /// <param name="world">Optional world instance (default: global world)</param>
        /// <remarks>
        /// This is an extension method that calls SystemsRegistry.AddToUpdate().
        /// The system will be added to the end of the Update queue.
        /// </remarks>
        public static void AddToUpdate(this SystemHandler system, World world = null)
        {
            SystemsRegistry.AddToUpdate(system, world);
        }

        /// <summary>
        /// Inserts this system at the specified index in the Update queue for the specified world.
        /// All systems at and after the specified index will be shifted forward.
        /// </summary>
        /// <param name="system">System instance (this)</param>
        /// <param name="order">Index at which to insert the system (0-based)</param>
        /// <param name="world">Optional world instance (default: global world)</param>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown if order is negative or greater than queue count</exception>
        /// <remarks>
        /// This is an extension method that calls SystemsRegistry.AddToUpdate().
        /// The system will be inserted at the specified index, shifting existing systems forward.
        /// </remarks>
        public static void AddToUpdate(this SystemHandler system, int order, World world = null)
        {
            SystemsRegistry.AddToUpdate(system, order, world);
        }

        /// <summary>
        /// Adds this system to the end of the FixedUpdate queue for the specified world.
        /// </summary>
        /// <param name="system">System instance (this)</param>
        /// <param name="world">Optional world instance (default: global world)</param>
        /// <remarks>
        /// This is an extension method that calls SystemsRegistry.AddToFixedUpdate().
        /// The system will be added to the end of the FixedUpdate queue.
        /// </remarks>
        public static void AddToFixedUpdate(this SystemHandler system, World world = null)
        {
            SystemsRegistry.AddToFixedUpdate(system, world);
        }

        /// <summary>
        /// Inserts this system at the specified index in the FixedUpdate queue for the specified world.
        /// All systems at and after the specified index will be shifted forward.
        /// </summary>
        /// <param name="system">System instance (this)</param>
        /// <param name="order">Index at which to insert the system (0-based)</param>
        /// <param name="world">Optional world instance (default: global world)</param>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown if order is negative or greater than queue count</exception>
        /// <remarks>
        /// This is an extension method that calls SystemsRegistry.AddToFixedUpdate().
        /// The system will be inserted at the specified index, shifting existing systems forward.
        /// </remarks>
        public static void AddToFixedUpdate(this SystemHandler system, int order, World world = null)
        {
            SystemsRegistry.AddToFixedUpdate(system, order, world);
        }

        /// <summary>
        /// Executes this system immediately, bypassing all queues.
        /// The system is executed synchronously without being added to any queue.
        /// </summary>
        /// <param name="system">System instance (this)</param>
        /// <param name="world">Optional world instance (default: global world). Note: This parameter is for API consistency but doesn't affect execution since systems are not world-scoped during execution.</param>
        /// <remarks>
        /// This is an extension method that calls SystemsRegistry.ExecuteOnce().
        /// The system will be executed immediately without being added to any queue.
        /// 
        /// This method is useful for:
        /// - One-time system execution (e.g., initialization systems)
        /// - Testing systems in isolation
        /// - Manual system execution outside of normal update loops
        /// </remarks>
        public static void ExecuteOnce(this SystemHandler system, World world = null)
        {
            SystemsRegistry.ExecuteOnce(system, world);
        }
    }
}

