using System;
using System.Collections.Generic;

namespace ArtyECS.Core
{
    /// <summary>
    /// Central registry for ECS systems, organized by world scope.
    /// Manages system execution queues (Update and FixedUpdate) per world.
    /// </summary>
    /// <remarks>
    /// This class implements:
    /// - System-001: SystemsRegistry - Basic Structure ✅
    /// - System-002: SystemsRegistry - Update Queue Management ✅
    /// - System-003: SystemsRegistry - FixedUpdate Queue Management ✅
    /// - System-004: SystemsRegistry - Manual Execution ✅
    /// - System-005: SystemsRegistry - Queue Execution (Sync) ✅
    /// 
    /// Features:
    /// - World-scoped instance support: each world has its own system queues
    /// - Two separate queues: Update queue and FixedUpdate queue
    /// - Support for optional World parameter (default: global world)
    /// - Singleton/static access pattern for global world
    /// - Update queue management: add to end or insert at specific index
    /// - FixedUpdate queue management: add to end or insert at specific index
    /// - Queue execution: ExecuteUpdate() and ExecuteFixedUpdate() execute all systems in respective queues
    /// - Sequential execution in order (index 0, 1, 2, ...)
    /// - Graceful error handling: continues execution even if one system fails
    /// - Manual execution: ExecuteOnce() executes a system immediately without adding to any queue
    /// 
    /// The registry maintains separate queues for Update and FixedUpdate execution contexts.
    /// Systems can be added to either queue and will be executed in order during their respective Unity callbacks.
    /// 
    /// Future tasks:
    /// - Async-002: Async Queue Execution (async system support)
    /// </remarks>
    public static class SystemsRegistry
    {
        /// <summary>
        /// Global/default world instance. Used when no world is specified.
        /// </summary>
        private static readonly World GlobalWorld = new World("Global");

        /// <summary>
        /// Internal class to hold system queues for a single world.
        /// Each world has its own Update and FixedUpdate queues.
        /// </summary>
        private class SystemStorageInstance
        {
            /// <summary>
            /// Queue of systems to execute during Update().
            /// Systems are executed in order (index 0, 1, 2, ...).
            /// </summary>
            public readonly List<SystemHandler> UpdateQueue = new List<SystemHandler>();

            /// <summary>
            /// Queue of systems to execute during FixedUpdate().
            /// Systems are executed in order (index 0, 1, 2, ...).
            /// </summary>
            public readonly List<SystemHandler> FixedUpdateQueue = new List<SystemHandler>();
        }

        /// <summary>
        /// Registry of worlds to their system storage instances.
        /// Each world has its own Update and FixedUpdate queues.
        /// </summary>
        private static readonly Dictionary<World, SystemStorageInstance> WorldStorages =
            new Dictionary<World, SystemStorageInstance>();

        /// <summary>
        /// Gets the storage instance for the specified world.
        /// Creates a new storage instance if the world doesn't exist yet.
        /// </summary>
        /// <param name="world">World instance, or null for global world</param>
        /// <returns>SystemStorageInstance containing Update and FixedUpdate queues</returns>
        private static SystemStorageInstance GetWorldStorage(World world = null)
        {
            World targetWorld = world ?? GlobalWorld;

            if (!WorldStorages.TryGetValue(targetWorld, out var storage))
            {
                storage = new SystemStorageInstance();
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
        /// Gets the number of worlds currently registered.
        /// </summary>
        /// <returns>Number of worlds (including global world)</returns>
        public static int GetWorldCount()
        {
            return WorldStorages.Count;
        }

        /// <summary>
        /// Checks if a world has been initialized (has system storage).
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
        /// Gets the Update queue for the specified world.
        /// </summary>
        /// <param name="world">Optional world instance (default: global world)</param>
        /// <returns>List of systems in the Update queue</returns>
        /// <remarks>
        /// This method provides access to the Update queue for a specific world.
        /// The queue will be managed by System-002: Update Queue Management.
        /// Primarily used for testing and debugging.
        /// </remarks>
        public static List<SystemHandler> GetUpdateQueue(World world = null)
        {
            var storage = GetWorldStorage(world);
            return storage.UpdateQueue;
        }

        /// <summary>
        /// Gets the FixedUpdate queue for the specified world.
        /// </summary>
        /// <param name="world">Optional world instance (default: global world)</param>
        /// <returns>List of systems in the FixedUpdate queue</returns>
        /// <remarks>
        /// This method provides access to the FixedUpdate queue for a specific world.
        /// The queue will be managed by System-003: FixedUpdate Queue Management.
        /// Primarily used for testing and debugging.
        /// </remarks>
        public static List<SystemHandler> GetFixedUpdateQueue(World world = null)
        {
            var storage = GetWorldStorage(world);
            return storage.FixedUpdateQueue;
        }

        /// <summary>
        /// Adds a system to the end of the Update queue for the specified world.
        /// </summary>
        /// <param name="system">System to add to the Update queue</param>
        /// <param name="world">Optional world instance (default: global world)</param>
        /// <remarks>
        /// This method implements System-002: Update Queue Management.
        /// 
        /// The system will be added to the end of the Update queue and will be executed
        /// after all existing systems in the queue.
        /// 
        /// Usage:
        /// <code>
        /// var movementSystem = new MovementSystem();
        /// SystemsRegistry.AddToUpdate(movementSystem);
        /// </code>
        /// 
        /// Or using extension method:
        /// <code>
        /// var movementSystem = new MovementSystem();
        /// movementSystem.AddToUpdate();
        /// </code>
        /// </remarks>
        public static void AddToUpdate(SystemHandler system, World world = null)
        {
            if (system == null)
            {
                throw new ArgumentNullException(nameof(system));
            }

            var storage = GetWorldStorage(world);
            storage.UpdateQueue.Add(system);
        }

        /// <summary>
        /// Inserts a system at the specified index in the Update queue for the specified world.
        /// All systems at and after the specified index will be shifted forward (index+1, index+2, etc.).
        /// </summary>
        /// <param name="system">System to insert into the Update queue</param>
        /// <param name="order">Index at which to insert the system (0-based)</param>
        /// <param name="world">Optional world instance (default: global world)</param>
        /// <exception cref="ArgumentNullException">Thrown if system is null</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if order is negative or greater than queue count</exception>
        /// <remarks>
        /// This method implements System-002: Update Queue Management.
        /// 
        /// The system will be inserted at the specified index. If the index is occupied,
        /// all systems from that index onwards will be shifted forward by one position.
        /// 
        /// Index validation:
        /// - If order is 0, system is inserted at the beginning
        /// - If order equals queue count, system is added at the end (same as AddToUpdate without order)
        /// - If order is greater than queue count, ArgumentOutOfRangeException is thrown
        /// 
        /// Usage:
        /// <code>
        /// var movementSystem = new MovementSystem();
        /// SystemsRegistry.AddToUpdate(movementSystem, order: 3); // Insert at index 3
        /// </code>
        /// 
        /// Or using extension method:
        /// <code>
        /// var movementSystem = new MovementSystem();
        /// movementSystem.AddToUpdate(3); // Insert at index 3
        /// </code>
        /// </remarks>
        public static void AddToUpdate(SystemHandler system, int order, World world = null)
        {
            if (system == null)
            {
                throw new ArgumentNullException(nameof(system));
            }

            var storage = GetWorldStorage(world);
            var queue = storage.UpdateQueue;

            // Validate index bounds
            if (order < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(order), "Order cannot be negative.");
            }

            if (order > queue.Count)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(order),
                    $"Order {order} is out of range. Queue has {queue.Count} systems. Valid range: 0 to {queue.Count}.");
            }

            // Insert at specified index (shifts existing systems forward)
            queue.Insert(order, system);
        }

        /// <summary>
        /// Executes all systems in the Update queue for the specified world in order.
        /// Systems are executed sequentially (index 0, 1, 2, ...).
        /// </summary>
        /// <param name="world">Optional world instance (default: global world)</param>
        /// <remarks>
        /// This method implements System-005: SystemsRegistry - Queue Execution (Sync).
        /// 
        /// Execution behavior:
        /// - Systems are executed in the order they appear in the queue (index 0, 1, 2, ...)
        /// - If a system throws an exception, execution continues with the next system
        /// - Errors are logged but do not stop queue execution (graceful error handling)
        /// - Empty queues are handled gracefully (no-op if queue is empty)
        /// 
        /// This method should be called from Unity's Update() method (via UpdateProvider or SystemExecutor).
        /// 
        /// Usage:
        /// <code>
        /// // In MonoBehaviour Update() method:
        /// void Update()
        /// {
        ///     SystemsRegistry.ExecuteUpdate();
        /// }
        /// </code>
        /// 
        /// Note: Async system support will be added in Async-002. Currently, all systems
        /// are executed synchronously.
        /// </remarks>
        public static void ExecuteUpdate(World world = null)
        {
            var storage = GetWorldStorage(world);
            var queue = storage.UpdateQueue;

            // Execute all systems in order
            for (int i = 0; i < queue.Count; i++)
            {
                var system = queue[i];
                try
                {
                    system.Execute();
                }
                catch (Exception ex)
                {
                    // Log error but continue execution with next system
                    // This allows other systems to continue even if one fails
                    // In production, you might want to use a proper logging system
                    UnityEngine.Debug.LogError($"System '{system.GetType().Name}' execution failed: {ex}");
                }
            }
        }

        /// <summary>
        /// Adds a system to the end of the FixedUpdate queue for the specified world.
        /// </summary>
        /// <param name="system">System to add to the FixedUpdate queue</param>
        /// <param name="world">Optional world instance (default: global world)</param>
        /// <exception cref="ArgumentNullException">Thrown if system is null</exception>
        /// <remarks>
        /// This method implements System-003: FixedUpdate Queue Management.
        /// 
        /// The system will be added to the end of the FixedUpdate queue and will be executed
        /// after all existing systems in the queue.
        /// 
        /// Usage:
        /// <code>
        /// var physicsSystem = new PhysicsSystem();
        /// SystemsRegistry.AddToFixedUpdate(physicsSystem);
        /// </code>
        /// 
        /// Or using extension method:
        /// <code>
        /// var physicsSystem = new PhysicsSystem();
        /// physicsSystem.AddToFixedUpdate();
        /// </code>
        /// </remarks>
        public static void AddToFixedUpdate(SystemHandler system, World world = null)
        {
            if (system == null)
            {
                throw new ArgumentNullException(nameof(system));
            }

            var storage = GetWorldStorage(world);
            storage.FixedUpdateQueue.Add(system);
        }

        /// <summary>
        /// Inserts a system at the specified index in the FixedUpdate queue for the specified world.
        /// All systems at and after the specified index will be shifted forward (index+1, index+2, etc.).
        /// </summary>
        /// <param name="system">System to insert into the FixedUpdate queue</param>
        /// <param name="order">Index at which to insert the system (0-based)</param>
        /// <param name="world">Optional world instance (default: global world)</param>
        /// <exception cref="ArgumentNullException">Thrown if system is null</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if order is negative or greater than queue count</exception>
        /// <remarks>
        /// This method implements System-003: FixedUpdate Queue Management.
        /// 
        /// The system will be inserted at the specified index. If the index is occupied,
        /// all systems from that index onwards will be shifted forward by one position.
        /// 
        /// Index validation:
        /// - If order is 0, system is inserted at the beginning
        /// - If order equals queue count, system is added at the end (same as AddToFixedUpdate without order)
        /// - If order is greater than queue count, ArgumentOutOfRangeException is thrown
        /// 
        /// Usage:
        /// <code>
        /// var physicsSystem = new PhysicsSystem();
        /// SystemsRegistry.AddToFixedUpdate(physicsSystem, order: 3); // Insert at index 3
        /// </code>
        /// 
        /// Or using extension method:
        /// <code>
        /// var physicsSystem = new PhysicsSystem();
        /// physicsSystem.AddToFixedUpdate(3); // Insert at index 3
        /// </code>
        /// </remarks>
        public static void AddToFixedUpdate(SystemHandler system, int order, World world = null)
        {
            if (system == null)
            {
                throw new ArgumentNullException(nameof(system));
            }

            var storage = GetWorldStorage(world);
            var queue = storage.FixedUpdateQueue;

            // Validate index bounds
            if (order < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(order), "Order cannot be negative.");
            }

            if (order > queue.Count)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(order),
                    $"Order {order} is out of range. Queue has {queue.Count} systems. Valid range: 0 to {queue.Count}.");
            }

            // Insert at specified index (shifts existing systems forward)
            queue.Insert(order, system);
        }

        /// <summary>
        /// Executes all systems in the FixedUpdate queue for the specified world in order.
        /// Systems are executed sequentially (index 0, 1, 2, ...).
        /// </summary>
        /// <param name="world">Optional world instance (default: global world)</param>
        /// <remarks>
        /// This method implements System-005: SystemsRegistry - Queue Execution (Sync).
        /// 
        /// Execution behavior:
        /// - Systems are executed in the order they appear in the queue (index 0, 1, 2, ...)
        /// - If a system throws an exception, execution continues with the next system
        /// - Errors are logged but do not stop queue execution (graceful error handling)
        /// - Empty queues are handled gracefully (no-op if queue is empty)
        /// 
        /// This method should be called from Unity's FixedUpdate() method (via UpdateProvider or SystemExecutor).
        /// 
        /// Usage:
        /// <code>
        /// // In MonoBehaviour FixedUpdate() method:
        /// void FixedUpdate()
        /// {
        ///     SystemsRegistry.ExecuteFixedUpdate();
        /// }
        /// </code>
        /// 
        /// Note: Async system support will be added in Async-002. Currently, all systems
        /// are executed synchronously.
        /// </remarks>
        public static void ExecuteFixedUpdate(World world = null)
        {
            var storage = GetWorldStorage(world);
            var queue = storage.FixedUpdateQueue;

            // Execute all systems in order
            for (int i = 0; i < queue.Count; i++)
            {
                var system = queue[i];
                try
                {
                    system.Execute();
                }
                catch (Exception ex)
                {
                    // Log error but continue execution with next system
                    // This allows other systems to continue even if one fails
                    // In production, you might want to use a proper logging system
                    UnityEngine.Debug.LogError($"System '{system.GetType().Name}' execution failed: {ex}");
                }
            }
        }

        /// <summary>
        /// Executes a system immediately, bypassing all queues.
        /// The system is executed synchronously without being added to any queue.
        /// </summary>
        /// <param name="system">System to execute immediately</param>
        /// <param name="world">Optional world instance (default: global world). Note: This parameter is for API consistency but doesn't affect execution since systems are not world-scoped during execution.</param>
        /// <exception cref="ArgumentNullException">Thrown if system is null</exception>
        /// <remarks>
        /// This method implements System-004: SystemsRegistry - Manual Execution.
        /// 
        /// Execution behavior:
        /// - System is executed immediately without being added to any queue
        /// - No side effects on Update or FixedUpdate queues
        /// - If a system throws an exception, it propagates to the caller (not caught)
        /// - World parameter is accepted for API consistency but doesn't affect execution
        /// 
        /// This method is useful for:
        /// - One-time system execution (e.g., initialization systems)
        /// - Testing systems in isolation
        /// - Manual system execution outside of normal update loops
        /// 
        /// Usage:
        /// <code>
        /// var initializationSystem = new InitializationSystem();
        /// SystemsRegistry.ExecuteOnce(initializationSystem);
        /// </code>
        /// 
        /// Or using extension method:
        /// <code>
        /// var initializationSystem = new InitializationSystem();
        /// initializationSystem.ExecuteOnce();
        /// </code>
        /// 
        /// Note: Async system support will be added in Async-001 and Async-002.
        /// Currently, all systems are executed synchronously via Execute() method.
        /// </remarks>
        public static void ExecuteOnce(SystemHandler system, World world = null)
        {
            if (system == null)
            {
                throw new ArgumentNullException(nameof(system));
            }

            // Execute system immediately without adding to any queue
            // World parameter is accepted for API consistency but doesn't affect execution
            // since systems execute in the context of ComponentsRegistry queries, not world queues
            system.Execute();
        }

        /// <summary>
        /// Clears all system storage for all worlds.
        /// This is primarily used for testing to reset state between tests.
        /// </summary>
        /// <remarks>
        /// WARNING: This method clears ALL system queues from ALL worlds.
        /// Use with caution - typically only for testing scenarios.
        /// </remarks>
        public static void ClearAll()
        {
            WorldStorages.Clear();
        }
    }
}

