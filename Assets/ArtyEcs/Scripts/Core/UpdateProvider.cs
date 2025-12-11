using System.Collections.Generic;
using UnityEngine;

namespace ArtyECS.Core
{
    /// <summary>
    /// MonoBehaviour provider for ECS system execution integration with Unity.
    /// Handles automatic execution of Update and FixedUpdate system queues.
    /// </summary>
    /// <remarks>
    /// This class implements System-006: System Execution Integration with Unity.
    /// 
    /// Features:
    /// - Calls SystemsRegistry.ExecuteUpdate() in Update() for all registered worlds
    /// - Calls SystemsRegistry.ExecuteFixedUpdate() in FixedUpdate() for all registered worlds
    /// - Supports multiple worlds - executes systems for all initialized worlds
    /// - DontDestroyOnLoad for persistence across scene changes
    /// - Initialization in Awake() to ensure early setup
    /// - **Automatic creation**: UpdateProvider is automatically created when first world is created
    /// 
    /// Usage:
    /// UpdateProvider is created automatically when you create the first entity (World.CreateEntity()).
    /// No manual setup required - just use the ECS framework:
    /// 
    /// <code>
    /// // Create entity - UpdateProvider created automatically here
    /// var entity = World.CreateEntity();
    /// ComponentsRegistry.AddComponent&lt;Position&gt;(entity, new Position { X = 1f, Y = 2f, Z = 3f });
    /// 
    /// // Add systems to queues
    /// var movementSystem = new MovementSystem();
    /// movementSystem.AddToUpdate(); // Will be executed in Update() each frame
    /// 
    /// var physicsSystem = new PhysicsSystem();
    /// physicsSystem.AddToFixedUpdate(); // Will be executed in FixedUpdate() each frame
    /// 
    /// // UpdateProvider automatically executes all systems each frame
    /// </code>
    /// 
    /// Multiple Worlds:
    /// The UpdateProvider executes systems for all initialized worlds. To execute systems
    /// for a specific world, use SystemsRegistry methods with the world parameter.
    /// 
    /// Note: UpdateProvider is created automatically as a visible GameObject named "UpdateProvider".
    /// Only one UpdateProvider instance exists at a time (singleton pattern).
    /// 
    /// Future tasks:
    /// - Entry-000: UpdateProvider MonoBehaviour (this task) ✅
    /// - Entry-001: Shutdown and Cleanup (will add OnDestroy/OnApplicationQuit handlers)
    /// </remarks>
    [DefaultExecutionOrder(-100)] // Execute early to ensure systems run before other scripts
    public class UpdateProvider : MonoBehaviour
    {
        /// <summary>
        /// Whether to execute systems for all worlds or only the global world.
        /// If true, executes systems for all initialized worlds.
        /// If false, executes systems only for the global world (default).
        /// </summary>
        [Tooltip("If true, executes systems for all worlds. If false, executes only for global world.")]
        public bool ExecuteAllWorlds = false;

        /// <summary>
        /// List of specific worlds to execute systems for (if ExecuteAllWorlds is false).
        /// If empty and ExecuteAllWorlds is false, only global world is executed.
        /// </summary>
        [Tooltip("Specific worlds to execute systems for (if ExecuteAllWorlds is false).")]
        public List<World> WorldsToExecute = new List<World>();

        /// <summary>
        /// Whether the UpdateProvider has been initialized.
        /// Used to prevent multiple initializations.
        /// </summary>
        private static bool _initialized = false;

        /// <summary>
        /// Singleton instance of UpdateProvider (for reference, though not strictly required).
        /// </summary>
        private static UpdateProvider _instance;

        /// <summary>
        /// Gets the singleton UpdateProvider instance, if it exists.
        /// </summary>
        public static UpdateProvider Instance => _instance;

        /// <summary>
        /// Automatically creates an UpdateProvider GameObject if it doesn't exist.
        /// This method is called when the first world is created (World.CreateEntity()).
        /// </summary>
        /// <returns>True if UpdateProvider was created or already exists, false if creation failed</returns>
        /// <remarks>
        /// UpdateProvider is created automatically when:
        /// - First world is created (via World.CreateEntity())
        /// - UpdateProvider doesn't already exist
        /// 
        /// Singleton pattern - only one UpdateProvider instance exists at a time.
        /// </remarks>
        internal static bool EnsureCreated()
        {
            // If UpdateProvider already exists and is valid, return true
            if (_instance != null && _instance.gameObject != null)
            {
                return true;
            }

            // Clear invalid instance reference if GameObject was destroyed
            if (_instance != null && _instance.gameObject == null)
            {
                _instance = null;
            }

            // Create UpdateProvider GameObject
            var go = new GameObject("UpdateProvider");
            _instance = go.AddComponent<UpdateProvider>();
            
            return true;
        }

        /// <summary>
        /// Initializes the UpdateProvider and ensures it persists across scene changes.
        /// </summary>
        private void Awake()
        {
            // Prevent multiple UpdateProvider instances
            if (_initialized && _instance != null && _instance != this)
            {
                UnityEngine.Debug.LogWarning("Multiple UpdateProvider instances detected. Destroying duplicate.");
                Destroy(gameObject);
                return;
            }

            // Mark as initialized and set instance
            _initialized = true;
            _instance = this;

            // Ensure this GameObject persists across scene changes
            DontDestroyOnLoad(gameObject);

            // Initialize global ECS World (ensure it exists)
            // This is done implicitly when first accessing ComponentsRegistry or SystemsRegistry,
            // but we can explicitly ensure it's initialized here
            var globalWorld = SystemsRegistry.GetGlobalWorld();
            UnityEngine.Debug.Log($"UpdateProvider initialized. Global world: {globalWorld}");
        }

        /// <summary>
        /// Executes all systems in the Update queue for the specified worlds.
        /// Called automatically by Unity each frame.
        /// </summary>
        private void Update()
        {
            ExecuteUpdateSystems();
        }

        /// <summary>
        /// Executes all systems in the FixedUpdate queue for the specified worlds.
        /// Called automatically by Unity at fixed intervals.
        /// </summary>
        private void FixedUpdate()
        {
            ExecuteFixedUpdateSystems();
        }

        /// <summary>
        /// Executes Update systems for the configured worlds.
        /// </summary>
        private void ExecuteUpdateSystems()
        {
            if (ExecuteAllWorlds)
            {
                // Execute systems for all initialized worlds
                // Note: SystemsRegistry doesn't expose a method to get all worlds,
                // so we execute for global world and any explicitly specified worlds
                SystemsRegistry.ExecuteUpdate();

                // Execute for any additional worlds specified in WorldsToExecute
                foreach (var world in WorldsToExecute)
                {
                    if (world != null && SystemsRegistry.IsWorldInitialized(world))
                    {
                        SystemsRegistry.ExecuteUpdate(world);
                    }
                }
            }
            else
            {
                // Execute only for specified worlds (or global if none specified)
                if (WorldsToExecute.Count == 0)
                {
                    // Default: execute only for global world
                    SystemsRegistry.ExecuteUpdate();
                }
                else
                {
                    // Execute for each specified world
                    foreach (var world in WorldsToExecute)
                    {
                        if (world != null && SystemsRegistry.IsWorldInitialized(world))
                        {
                            SystemsRegistry.ExecuteUpdate(world);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Executes FixedUpdate systems for the configured worlds.
        /// </summary>
        private void ExecuteFixedUpdateSystems()
        {
            if (ExecuteAllWorlds)
            {
                // Execute systems for all initialized worlds
                // Note: SystemsRegistry doesn't expose a method to get all worlds,
                // so we execute for global world and any explicitly specified worlds
                SystemsRegistry.ExecuteFixedUpdate();

                // Execute for any additional worlds specified in WorldsToExecute
                foreach (var world in WorldsToExecute)
                {
                    if (world != null && SystemsRegistry.IsWorldInitialized(world))
                    {
                        SystemsRegistry.ExecuteFixedUpdate(world);
                    }
                }
            }
            else
            {
                // Execute only for specified worlds (or global if none specified)
                if (WorldsToExecute.Count == 0)
                {
                    // Default: execute only for global world
                    SystemsRegistry.ExecuteFixedUpdate();
                }
                else
                {
                    // Execute for each specified world
                    foreach (var world in WorldsToExecute)
                    {
                        if (world != null && SystemsRegistry.IsWorldInitialized(world))
                        {
                            SystemsRegistry.ExecuteFixedUpdate(world);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Cleanup when the UpdateProvider is destroyed.
        /// </summary>
        private void OnDestroy()
        {
            if (_instance == this)
            {
                _initialized = false;
                _instance = null;
            }
        }
    }
}

