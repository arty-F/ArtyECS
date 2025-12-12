using System;
using System.Collections.Generic;

namespace ArtyECS.Core
{
    /// <summary>
    /// Manager for ECS systems, organized by world scope.
    /// Manages system execution queues (Update and FixedUpdate) per world.
    /// </summary>
    /// <remarks>
    /// **API-010: This class is internal implementation. Use World API instead.**
    /// 
    /// This class is internal and should not be used directly by framework users.
    /// Use World class methods instead:
    /// - World.AddToUpdate(system) instead of SystemsManager.AddToUpdate(system)
    /// - World.AddToFixedUpdate(system) instead of SystemsManager.AddToFixedUpdate(system)
    /// - World.ExecuteOnce(system) instead of SystemsManager.ExecuteOnce(system)
    /// - World.ExecuteUpdate() instead of SystemsManager.ExecuteUpdate()
    /// - World.ExecuteFixedUpdate() instead of SystemsManager.ExecuteFixedUpdate()
    /// - World.RemoveFromUpdate(system) instead of SystemsManager.RemoveFromUpdate(system)
    /// - World.RemoveFromFixedUpdate(system) instead of SystemsManager.RemoveFromFixedUpdate(system)
    /// 
    /// See World class documentation for the public API.
    /// 
    /// This class implements:
    /// - System-001: SystemsManager - Basic Structure ✅
    /// - System-002: SystemsManager - Update Queue Management ✅
    /// - System-003: SystemsManager - FixedUpdate Queue Management ✅
    /// - System-004: SystemsManager - Manual Execution ✅
    /// - System-005: SystemsManager - Queue Execution (Sync) ✅
    /// - API-001: Fix ExecuteOnce World Parameter ✅
    ///   - ExecuteOnce() passes World parameter to system.Execute(world)
    ///   - Queue execution methods pass world context to system.Execute(world)
    ///   - Systems can use World parameter for component queries
    /// - API-010: World is now required parameter (not optional)
    ///   - All methods require World parameter
    ///   - SystemsManager is now internal (not public)
    ///   - Added RemoveFromUpdate() and RemoveFromFixedUpdate() methods
    /// - World-002: World-Scoped Storage Integration ✅
    ///   - All methods require World parameter
    ///   - World-scoped storage via Dictionary&lt;World, SystemStorageInstance&gt;
    ///   - Uses shared global world singleton from World.GetOrCreate()
    /// - World-003: World Persistence Across Scenes ✅
    ///   - System queues use static dictionaries that persist across Unity scene changes
    ///   - All system queues remain valid after scene transitions
    ///   - Systems continue executing in new scenes via UpdateProvider persistence
    /// 
    /// Features:
    /// - World-scoped instance support: each world has its own system queues
    /// - Two separate queues: Update queue and FixedUpdate queue
    /// - World parameter is required (not optional)
    /// - Update queue management: add to end or insert at specific index
    /// - FixedUpdate queue management: add to end or insert at specific index
    /// - Queue execution: ExecuteUpdate() and ExecuteFixedUpdate() execute all systems in respective queues
    /// - System removal: RemoveFromUpdate() and RemoveFromFixedUpdate() remove systems from queues
    /// - Sequential execution in order (index 0, 1, 2, ...)
    /// - Graceful error handling: continues execution even if one system fails
    /// - Manual execution: ExecuteOnce() executes a system immediately without adding to any queue
    /// 
    /// The manager maintains separate queues for Update and FixedUpdate execution contexts.
    /// Systems can be added to either queue and will be executed in order during their respective Unity callbacks.
    /// 
    /// Future tasks:
    /// - Async-002: Async Queue Execution (async system support)
    /// </remarks>
    internal static class SystemsManager
    {
        /// <summary>
        /// Gets the global/default world instance. Used when no world is specified.
        /// Uses World.GetOrCreate() to ensure shared singleton instance.
        /// </summary>
        private static World GlobalWorld => World.GetOrCreate();

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
        /// <param name="world">World instance (required)</param>
        /// <returns>SystemStorageInstance containing Update and FixedUpdate queues</returns>
        /// <remarks>
        /// API-010: World is now required parameter (not optional)
        /// </remarks>
        private static SystemStorageInstance GetWorldStorage(World world)
        {
            if (world == null)
                throw new ArgumentNullException(nameof(world));

            if (!WorldStorages.TryGetValue(world, out var storage))
            {
                storage = new SystemStorageInstance();
                WorldStorages[world] = storage;
            }

            return storage;
        }

        /// <summary>
        /// Gets the number of worlds currently registered.
        /// </summary>
        /// <returns>Number of worlds (including global world)</returns>
        /// <remarks>
        /// API-010: This method is internal.
        /// </remarks>
        internal static int GetWorldCount()
        {
            return WorldStorages.Count;
        }

        /// <summary>
        /// Checks if a world has been initialized (has system storage).
        /// </summary>
        /// <param name="world">World to check (required)</param>
        /// <returns>True if world has been initialized</returns>
        /// <remarks>
        /// API-010: World is now required parameter (not optional)
        /// </remarks>
        internal static bool IsWorldInitialized(World world)
        {
            if (world == null)
                throw new ArgumentNullException(nameof(world));
            return WorldStorages.ContainsKey(world);
        }


        /// <summary>
        /// Gets the Update queue for the specified world.
        /// </summary>
        /// <param name="world">World instance (required)</param>
        /// <returns>List of systems in the Update queue</returns>
        /// <remarks>
        /// API-010: World is now required parameter (not optional)
        /// 
        /// This method provides access to the Update queue for a specific world.
        /// The queue will be managed by System-002: Update Queue Management.
        /// Primarily used for testing and debugging.
        /// </remarks>
        internal static List<SystemHandler> GetUpdateQueue(World world)
        {
            var storage = GetWorldStorage(world);
            return storage.UpdateQueue;
        }

        /// <summary>
        /// Gets the FixedUpdate queue for the specified world.
        /// </summary>
        /// <param name="world">World instance (required)</param>
        /// <returns>List of systems in the FixedUpdate queue</returns>
        /// <remarks>
        /// API-010: World is now required parameter (not optional)
        /// 
        /// This method provides access to the FixedUpdate queue for a specific world.
        /// The queue will be managed by System-003: FixedUpdate Queue Management.
        /// Primarily used for testing and debugging.
        /// </remarks>
        internal static List<SystemHandler> GetFixedUpdateQueue(World world)
        {
            var storage = GetWorldStorage(world);
            return storage.FixedUpdateQueue;
        }

        /// <summary>
        /// Adds a system to the end of the Update queue for the specified world.
        /// </summary>
        /// <param name="system">SystemHandler instance to add to the Update queue</param>
        /// <param name="world">World instance (required)</param>
        /// <remarks>
        /// This method implements System-002: Update Queue Management.
        /// API-010: World is now required parameter (not optional)
        /// 
        /// The system will be added to the end of the Update queue and will be executed
        /// after all existing systems in the queue.
        /// 
        /// Usage:
        /// <code>
        /// var movementSystem = new MovementSystem();
        /// SystemsManager.AddToUpdate(movementSystem, world);
        /// </code>
        /// </remarks>
        internal static void AddToUpdate(SystemHandler system, World world)
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
        /// <param name="system">SystemHandler instance to insert into the Update queue</param>
        /// <param name="order">Index at which to insert the system (0-based)</param>
        /// <param name="world">World instance (required)</param>
        /// <exception cref="ArgumentNullException">Thrown if system is null</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if order is negative or greater than queue count</exception>
        /// <remarks>
        /// This method implements System-002: Update Queue Management.
        /// API-010: World is now required parameter (not optional)
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
        /// SystemsManager.AddToUpdate(movementSystem, order: 3, world); // Insert at index 3
        /// </code>
        /// </remarks>
        internal static void AddToUpdate(SystemHandler system, int order, World world)
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
        /// <param name="world">World instance (required). The world context is passed to each system's Execute() method.</param>
        /// <remarks>
        /// This method implements:
        /// - System-005: SystemsManager - Queue Execution (Sync) ✅
        /// - API-001: Fix ExecuteOnce World Parameter ✅
        /// - API-010: World is now required parameter (not optional)
        /// 
        /// Execution behavior:
        /// - Systems are executed in the order they appear in the queue (index 0, 1, 2, ...)
        /// - World context is passed to each system's Execute(world) method
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
        ///     SystemsManager.ExecuteUpdate(world); // Executes systems in specified world
        /// }
        /// </code>
        /// 
        /// Note: Async system support will be added in Async-002. Currently, all systems
        /// are executed synchronously.
        /// </remarks>
        internal static void ExecuteUpdate(World world)
        {
            var storage = GetWorldStorage(world);
            var queue = storage.UpdateQueue;

            // Execute all systems in order, passing world context
            for (int i = 0; i < queue.Count; i++)
            {
                var system = queue[i];
                try
                {
                    system.Execute(world);
                }
                catch (Exception ex)
                {
                    // Log error but continue execution with next system
                    // This allows other systems to continue even if one fails
                    // In production, you might want to use a proper logging system
                    UnityEngine.Debug.LogError($"System '{system.GetType().Name}' execution failed in world '{world.Name}': {ex}");
                }
            }
        }

        /// <summary>
        /// Adds a system to the end of the FixedUpdate queue for the specified world.
        /// </summary>
        /// <param name="system">SystemHandler instance to add to the FixedUpdate queue</param>
        /// <param name="world">World instance (required)</param>
        /// <exception cref="ArgumentNullException">Thrown if system is null</exception>
        /// <remarks>
        /// This method implements System-003: FixedUpdate Queue Management.
        /// API-010: World is now required parameter (not optional)
        /// 
        /// The system will be added to the end of the FixedUpdate queue and will be executed
        /// after all existing systems in the queue.
        /// 
        /// Usage:
        /// <code>
        /// var physicsSystem = new PhysicsSystem();
        /// SystemsManager.AddToFixedUpdate(physicsSystem, world);
        /// </code>
        /// </remarks>
        internal static void AddToFixedUpdate(SystemHandler system, World world)
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
        /// <param name="system">SystemHandler instance to insert into the FixedUpdate queue</param>
        /// <param name="order">Index at which to insert the system (0-based)</param>
        /// <param name="world">World instance (required)</param>
        /// <exception cref="ArgumentNullException">Thrown if system is null</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if order is negative or greater than queue count</exception>
        /// <remarks>
        /// This method implements System-003: FixedUpdate Queue Management.
        /// API-010: World is now required parameter (not optional)
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
        /// SystemsManager.AddToFixedUpdate(physicsSystem, order: 3, world); // Insert at index 3
        /// </code>
        /// </remarks>
        internal static void AddToFixedUpdate(SystemHandler system, int order, World world)
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
        /// <param name="world">World instance (required). The world context is passed to each system's Execute() method.</param>
        /// <remarks>
        /// This method implements:
        /// - System-005: SystemsManager - Queue Execution (Sync) ✅
        /// - API-001: Fix ExecuteOnce World Parameter ✅
        /// - API-010: World is now required parameter (not optional)
        /// 
        /// Execution behavior:
        /// - Systems are executed in the order they appear in the queue (index 0, 1, 2, ...)
        /// - World context is passed to each system's Execute(world) method
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
        ///     SystemsManager.ExecuteFixedUpdate(world); // Executes systems in specified world
        /// }
        /// </code>
        /// 
        /// Note: Async system support will be added in Async-002. Currently, all systems
        /// are executed synchronously.
        /// </remarks>
        internal static void ExecuteFixedUpdate(World world)
        {
            var storage = GetWorldStorage(world);
            var queue = storage.FixedUpdateQueue;

            // Execute all systems in order, passing world context
            for (int i = 0; i < queue.Count; i++)
            {
                var system = queue[i];
                try
                {
                    system.Execute(world);
                }
                catch (Exception ex)
                {
                    // Log error but continue execution with next system
                    // This allows other systems to continue even if one fails
                    // In production, you might want to use a proper logging system
                    UnityEngine.Debug.LogError($"System '{system.GetType().Name}' execution failed in world '{world.Name}': {ex}");
                }
            }
        }

        /// <summary>
        /// Executes a system immediately, bypassing all queues.
        /// The system is executed synchronously without being added to any queue.
        /// </summary>
        /// <param name="system">SystemHandler instance to execute immediately</param>
        /// <param name="world">World instance (required). The system will be executed in the context of this world.</param>
        /// <exception cref="ArgumentNullException">Thrown if system is null</exception>
        /// <remarks>
        /// This method implements:
        /// - System-004: SystemsManager - Manual Execution ✅
        /// - API-001: Fix ExecuteOnce World Parameter ✅
        /// - API-010: World is now required parameter (not optional)
        /// 
        /// Execution behavior:
        /// - System is executed immediately without being added to any queue
        /// - No side effects on Update or FixedUpdate queues
        /// - If a system throws an exception, it propagates to the caller (not caught)
        /// - World parameter is passed to system.Execute(world) to provide world context
        /// - The system should use this world for component queries
        /// 
        /// This method is useful for:
        /// - One-time system execution (e.g., initialization systems)
        /// - Testing systems in isolation
        /// - Manual system execution outside of normal update loops
        /// - Executing systems in specific world contexts
        /// 
        /// Usage:
        /// <code>
        /// var initializationSystem = new InitializationSystem();
        /// SystemsManager.ExecuteOnce(initializationSystem, world); // Executes in specified world
        /// </code>
        /// 
        /// Note: Async system support will be added in Async-001 and Async-002.
        /// Currently, all systems are executed synchronously via Execute() method.
        /// </remarks>
        internal static void ExecuteOnce(SystemHandler system, World world)
        {
            if (system == null)
            {
                throw new ArgumentNullException(nameof(system));
            }

            // Execute system immediately without adding to any queue
            // Pass World parameter to system.Execute(world) to provide world context
            // Systems can use this world parameter for component queries
            system.Execute(world);
        }

        /// <summary>
        /// Clears all system storage for the specified world.
        /// Removes all systems from Update and FixedUpdate queues for the world.
        /// </summary>
        /// <param name="world">World instance to clear</param>
        /// <remarks>
        /// This method is used by World.Destroy() to clean up world resources.
        /// 
        /// Features:
        /// - Clears Update queue for the specified world
        /// - Clears FixedUpdate queue for the specified world
        /// - World storage instance is removed from registry
        /// 
        /// Usage:
        /// <code>
        /// var localWorld = new World("Local");
        /// // ... use world ...
        /// SystemsManager.ClearWorld(localWorld); // Clean up systems
        /// </code>
        /// </remarks>
        internal static void ClearWorld(World world)
        {
            if (world == null)
            {
                return;
            }

            WorldStorages.Remove(world);
        }

        /// <summary>
        /// Executes all systems in the Update queue for all initialized worlds.
        /// This method iterates through all worlds and executes their Update queues.
        /// </summary>
        /// <remarks>
        /// API-010: This method is internal. Use World.ExecuteUpdate() instead.
        /// 
        /// This method executes Update systems for all worlds that have been initialized.
        /// Each world's systems are executed sequentially in the order they appear in that world's queue.
        /// 
        /// Execution behavior:
        /// - All worlds are processed in the order they appear in the internal storage
        /// - For each world, systems are executed in the order they appear in the Update queue
        /// - World context is passed to each system's Execute(world) method
        /// - If a system throws an exception, execution continues with the next system
        /// - Errors are logged but do not stop queue execution (graceful error handling)
        /// 
        /// This method should be called from Unity's Update() method (via UpdateProvider).
        /// 
        /// Usage:
        /// <code>
        /// // In MonoBehaviour Update() method:
        /// void Update()
        /// {
        ///     SystemsManager.ExecuteUpdateAllWorlds();
        /// }
        /// </code>
        /// </remarks>
        internal static void ExecuteUpdateAllWorlds()
        {
            foreach (var kvp in WorldStorages)
            {
                var world = kvp.Key;
                var queue = kvp.Value.UpdateQueue;

                // Execute all systems in order, passing world context
                for (int i = 0; i < queue.Count; i++)
                {
                    var system = queue[i];
                    try
                    {
                        system.Execute(world);
                    }
                    catch (Exception ex)
                    {
                        // Log error but continue execution with next system
                        UnityEngine.Debug.LogError($"System '{system.GetType().Name}' execution failed in world '{world.Name}': {ex}");
                    }
                }
            }
        }

        /// <summary>
        /// Executes all systems in the FixedUpdate queue for all initialized worlds.
        /// This method iterates through all worlds and executes their FixedUpdate queues.
        /// </summary>
        /// <remarks>
        /// API-010: This method is internal. Use World.ExecuteFixedUpdate() instead.
        /// 
        /// This method executes FixedUpdate systems for all worlds that have been initialized.
        /// Each world's systems are executed sequentially in the order they appear in that world's queue.
        /// 
        /// Execution behavior:
        /// - All worlds are processed in the order they appear in the internal storage
        /// - For each world, systems are executed in the order they appear in the FixedUpdate queue
        /// - World context is passed to each system's Execute(world) method
        /// - If a system throws an exception, execution continues with the next system
        /// - Errors are logged but do not stop queue execution (graceful error handling)
        /// 
        /// This method should be called from Unity's FixedUpdate() method (via UpdateProvider).
        /// 
        /// Usage:
        /// <code>
        /// // In MonoBehaviour FixedUpdate() method:
        /// void FixedUpdate()
        /// {
        ///     SystemsManager.ExecuteFixedUpdateAllWorlds();
        /// }
        /// </code>
        /// </remarks>
        internal static void ExecuteFixedUpdateAllWorlds()
        {
            foreach (var kvp in WorldStorages)
            {
                var world = kvp.Key;
                var queue = kvp.Value.FixedUpdateQueue;

                // Execute all systems in order, passing world context
                for (int i = 0; i < queue.Count; i++)
                {
                    var system = queue[i];
                    try
                    {
                        system.Execute(world);
                    }
                    catch (Exception ex)
                    {
                        // Log error but continue execution with next system
                        UnityEngine.Debug.LogError($"System '{system.GetType().Name}' execution failed in world '{world.Name}': {ex}");
                    }
                }
            }
        }

        /// <summary>
        /// Removes a system from the Update queue for the specified world.
        /// </summary>
        /// <param name="system">SystemHandler instance to remove from the Update queue</param>
        /// <param name="world">World instance (required)</param>
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
        internal static bool RemoveFromUpdate(SystemHandler system, World world)
        {
            if (system == null)
                throw new ArgumentNullException(nameof(system));
            if (world == null)
                throw new ArgumentNullException(nameof(world));

            var storage = GetWorldStorage(world);
            return storage.UpdateQueue.Remove(system);
        }

        /// <summary>
        /// Removes a system from the FixedUpdate queue for the specified world.
        /// </summary>
        /// <param name="system">SystemHandler instance to remove from the FixedUpdate queue</param>
        /// <param name="world">World instance (required)</param>
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
        internal static bool RemoveFromFixedUpdate(SystemHandler system, World world)
        {
            if (system == null)
                throw new ArgumentNullException(nameof(system));
            if (world == null)
                throw new ArgumentNullException(nameof(world));

            var storage = GetWorldStorage(world);
            return storage.FixedUpdateQueue.Remove(system);
        }

        /// <summary>
        /// Clears all system storage for all worlds.
        /// This is primarily used for testing to reset state between tests.
        /// </summary>
        /// <remarks>
        /// WARNING: This method clears ALL system queues from ALL worlds.
        /// Use with caution - typically only for testing scenarios.
        /// </remarks>
        internal static void ClearAll()
        {
            WorldStorages.Clear();
        }
    }
}

