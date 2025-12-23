#if UNITY_EDITOR
using UnityEngine;
using ArtyECS.Core;

namespace ArtyECS.Editor
{
    public class SystemDisplay : MonoBehaviour
    {
        [SerializeField]
        private string _worldName;

        [SerializeField]
        private string _queueName;

        private WorldInstance _world;
        private SystemHandler _system;

        public void Initialize(SystemHandler system, WorldInstance world, string queueName)
        {
            _system = system;
            _world = world;
            _worldName = world != null ? world.Name : "Unknown";
            _queueName = queueName;
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

        public SystemHandler GetSystem()
        {
            return _system;
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

