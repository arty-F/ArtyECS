#if UNITY_EDITOR
using UnityEngine;
using ArtyECS.Core;

namespace ArtyECS.Editor
{
    public class SystemsContainerDisplay : MonoBehaviour
    {
        [SerializeField]
        private string _worldName;

        [SerializeField]
        private string _queueName;

        private WorldInstance _world;

        public void Initialize(WorldInstance world, string containerName)
        {
            _world = world;
            _worldName = world != null ? world.Name : "Unknown";
            _queueName = containerName;
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

        public WorldInstance GetWorld()
        {
            return _world;
        }

        public string GetQueueName()
        {
            return _queueName;
        }
    }
}
#endif

