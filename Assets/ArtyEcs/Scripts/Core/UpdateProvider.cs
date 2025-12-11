using UnityEngine;

namespace ArtyECS.Core
{
    /// <summary>
    /// MonoBehaviour provider for ECS system execution integration with Unity.
    /// Handles automatic execution of Update and FixedUpdate system queues.
    /// </summary>
    /// <remarks>
    /// This class implements System-006: System Execution Integration with Unity.
    /// World-003: World Persistence Across Scenes (COMPLETED)
    /// 
    /// Features:
    /// - Calls SystemsRegistry.ExecuteUpdateAllWorlds() in Update() for all registered worlds
    /// - Calls SystemsRegistry.ExecuteFixedUpdateAllWorlds() in FixedUpdate() for all registered worlds
    /// - Always executes systems for all initialized worlds (no configuration needed)
    /// - **DontDestroyOnLoad for persistence across scene changes** - ensures UpdateProvider 
    ///   and system execution continue across scene transitions
    /// - Initialization in Awake() to ensure early setup
    /// - **Automatic creation**: UpdateProvider is automatically created when first world is created
    /// 
    /// Scene Persistence:
    /// UpdateProvider uses DontDestroyOnLoad to persist across Unity scene changes.
    /// This ensures that:
    /// - System execution continues seamlessly across scene transitions
    /// - Update and FixedUpdate queues are executed in new scenes
    /// - ECS World state (components, entities, systems) is preserved between scenes
    /// 
    /// The UpdateProvider GameObject will persist in DontDestroyOnLoad scene and continue
    /// executing systems for all registered worlds regardless of active Unity scene.
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
    /// // UpdateProvider automatically executes all systems for all worlds each frame
    /// </code>
    /// 
    /// Multiple Worlds:
    /// The UpdateProvider always executes systems for all initialized worlds. To execute systems
    /// for a specific world only, use SystemsRegistry.ExecuteUpdate(world) or SystemsRegistry.ExecuteFixedUpdate(world)
    /// directly from your code.
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
        /// This method implements World-003: World Persistence Across Scenes.
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
            // This is critical for World-003: World Persistence Across Scenes
            // Without DontDestroyOnLoad, the UpdateProvider would be destroyed on scene unload,
            // breaking system execution in new scenes. With DontDestroyOnLoad, the UpdateProvider
            // persists in DontDestroyOnLoad scene and continues executing systems regardless
            // of which Unity scene is currently active.
            DontDestroyOnLoad(gameObject);

            // Initialize global ECS World (ensure it exists)
            // This is done implicitly when first accessing ComponentsRegistry or SystemsRegistry,
            // but we can explicitly ensure it's initialized here
            var globalWorld = SystemsRegistry.GetGlobalWorld();
            UnityEngine.Debug.Log($"UpdateProvider initialized. Global world: {globalWorld}");
        }

        /// <summary>
        /// Executes all systems in the Update queue for all initialized worlds.
        /// Called automatically by Unity each frame.
        /// </summary>
        private void Update()
        {
            ExecuteUpdateSystems();
        }

        /// <summary>
        /// Executes all systems in the FixedUpdate queue for all initialized worlds.
        /// Called automatically by Unity at fixed intervals.
        /// </summary>
        private void FixedUpdate()
        {
            ExecuteFixedUpdateSystems();
        }

        /// <summary>
        /// Executes Update systems for all initialized worlds.
        /// </summary>
        private void ExecuteUpdateSystems()
        {
            SystemsRegistry.ExecuteUpdateAllWorlds();
        }

        /// <summary>
        /// Executes FixedUpdate systems for all initialized worlds.
        /// </summary>
        private void ExecuteFixedUpdateSystems()
        {
            SystemsRegistry.ExecuteFixedUpdateAllWorlds();
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

