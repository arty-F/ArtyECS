using System;
using System.Collections.Generic;

namespace ArtyECS.Core
{
    /// <summary>
    /// Central registry for ECS systems, organized by world scope.
    /// Manages system execution queues (Update and FixedUpdate) per world.
    /// </summary>
    /// <remarks>
    /// This class implements System-001: SystemsRegistry - Basic Structure.
    /// 
    /// Features:
    /// - World-scoped instance support: each world has its own system queues
    /// - Two separate queues: Update queue and FixedUpdate queue
    /// - Support for optional World parameter (default: global world)
    /// - Singleton/static access pattern for global world
    /// 
    /// The registry maintains separate queues for Update and FixedUpdate execution contexts.
    /// Systems can be added to either queue and will be executed in order during their respective Unity callbacks.
    /// 
    /// Future tasks:
    /// - System-002: Update Queue Management (RunInUpdate methods)
    /// - System-003: FixedUpdate Queue Management (RunInFixedUpdate methods)
    /// - System-004: Manual Execution (ExecuteOnce method)
    /// - System-005: Queue Execution (ExecuteUpdate, ExecuteFixedUpdate methods)
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
            public readonly List<System> UpdateQueue = new List<System>();

            /// <summary>
            /// Queue of systems to execute during FixedUpdate().
            /// Systems are executed in order (index 0, 1, 2, ...).
            /// </summary>
            public readonly List<System> FixedUpdateQueue = new List<System>();
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
        /// </remarks>
        internal static List<System> GetUpdateQueue(World world = null)
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
        /// </remarks>
        internal static List<System> GetFixedUpdateQueue(World world = null)
        {
            var storage = GetWorldStorage(world);
            return storage.FixedUpdateQueue;
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

