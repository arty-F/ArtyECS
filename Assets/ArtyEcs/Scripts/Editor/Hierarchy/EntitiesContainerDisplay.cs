#if UNITY_EDITOR
using UnityEngine;
using ArtyECS.Core;

namespace ArtyECS.Editor
{
    public class EntitiesContainerDisplay : MonoBehaviour
    {
        [SerializeField]
        private string _worldName;

        private WorldInstance _world;

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

        public WorldInstance GetWorld()
        {
            return _world;
        }
    }
}
#endif

