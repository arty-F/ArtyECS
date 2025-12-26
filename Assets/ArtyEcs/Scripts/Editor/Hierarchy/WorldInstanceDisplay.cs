#if UNITY_EDITOR
using UnityEngine;
using ArtyECS.Core;

namespace ArtyECS.Editor
{
    public class WorldInstanceDisplay : MonoBehaviour
    {
        [SerializeField]
        private string _worldName;

        private WorldInstance _world;
        private float _lastRefreshTime;
        private const float REFRESH_INTERVAL = 0.5f;

        public void Initialize(WorldInstance world)
        {
            _world = world;
            _worldName = world != null ? world.Name : "Unknown";
        }

        private void Awake()
        {
            if (!string.IsNullOrEmpty(_worldName))
            {
                if (_worldName == "Global")
                {
                    _world = World.GetOrCreate();
                }
                else
                {
                    _world = World.GetOrCreate(_worldName);
                }
            }
        }

        private void Update()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            if (_world == null)
            {
                return;
            }

            float currentTime = Time.realtimeSinceStartup;
            if (currentTime - _lastRefreshTime >= REFRESH_INTERVAL)
            {
                _lastRefreshTime = currentTime;
            }
        }

        public WorldInstance GetWorld()
        {
            return _world;
        }

        public string GetWorldName()
        {
            return _worldName;
        }
    }
}
#endif

