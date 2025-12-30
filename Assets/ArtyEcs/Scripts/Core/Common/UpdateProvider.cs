using UnityEngine;

namespace ArtyECS.Core
{
    [DefaultExecutionOrder(-100)]
    public class UpdateProvider : MonoBehaviour
    {

        private static bool _initialized = false;

        private static UpdateProvider _instance;

        public static UpdateProvider Instance => _instance;

        internal static bool EnsureCreated()
        {
            if (_instance != null && _instance.gameObject != null)
            {
                return true;
            }

            if (_instance != null && _instance.gameObject == null)
            {
                _instance = null;
            }

            var go = new GameObject("UpdateProvider");
            _instance = go.AddComponent<UpdateProvider>();
            
            return true;
        }

        private void Awake()
        {
            if (_initialized && _instance != null && _instance != this)
            {
                UnityEngine.Debug.LogWarning("Multiple UpdateProvider instances detected. Destroying duplicate.");
                Destroy(gameObject);
                return;
            }

            _initialized = true;
            _instance = this;

            DontDestroyOnLoad(gameObject);

            var globalWorld = World.GetOrCreate();
            UnityEngine.Debug.Log($"UpdateProvider initialized. Global world: {globalWorld}");
        }

        private void Update()
        {
            ExecuteUpdateSystems();
            QueryContext.ReturnResourcesForAllContexts();
        }

        private void FixedUpdate()
        {
            ExecuteFixedUpdateSystems();
            QueryContext.ReturnResourcesForAllContexts();
        }

        private void ExecuteUpdateSystems()
        {
            SystemsManager.ExecuteUpdateAllWorlds();
        }

        private void ExecuteFixedUpdateSystems()
        {
            SystemsManager.ExecuteFixedUpdateAllWorlds();
        }

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

