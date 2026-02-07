using System.Collections.Generic;
using UnityEngine;

namespace ArtyECS.Core
{
    internal class UpdateProvider : MonoBehaviour
    {
        private static UpdateProvider _instance;

        internal List<SystemEntry> UpdateQueue = new List<SystemEntry>(Constants.SYSTEMS_CAPACITY);
        internal List<SystemEntry> FixedUpdateQueue = new List<SystemEntry>(Constants.SYSTEMS_CAPACITY);

        internal static UpdateProvider GetOrCreate()
        {
            if (_instance == null)
            {
                var go = new GameObject(Constants.GAMEOBJECT_NAME);
                _instance = go.AddComponent<UpdateProvider>();
                DontDestroyOnLoad(go);
            }
            return _instance;
        }

        internal void RegisterSystem(SystemHandler system, WorldInstance world, UpdateType type)
        {
            var entry = new SystemEntry(system, world);

            if (type == UpdateType.Update)
            {
                UpdateQueue.Add(entry);
            }
            else
            {
                FixedUpdateQueue.Add(entry);
            }
        }

        internal void ExecuteSystems(WorldInstance world, UpdateType type)
        {
            var queue = type == UpdateType.Update ? UpdateQueue : FixedUpdateQueue;

            for (int i = 0; i < queue.Count; i++)
            {
                var entry = queue[i];
                if (entry.World == world)
                {
                    ExecuteSystems(entry.System, entry.World);
                }
            }
        }

        internal static void Clear()
        {
            if (_instance == null)
            {
                return;
            }

            _instance.UpdateQueue.Clear();
            _instance.FixedUpdateQueue.Clear();
        }

        private void Update()
        {
            for (int i = 0; i < UpdateQueue.Count; i++)
            {
                var entry = UpdateQueue[i];
                ExecuteSystems(entry.System, entry.World);
            }
        }

        private void FixedUpdate()
        {
            for (int i = 0; i < FixedUpdateQueue.Count; i++)
            {
                var entry = FixedUpdateQueue[i];
                ExecuteSystems(entry.System, entry.World);
            }
        }

        private void OnDestroy()
        {
            World.Clear();
            _instance = null;
        }

        private void ExecuteSystems(SystemHandler system, WorldInstance world)
        {
            system.Execute(world);
            world.ResetQueryBuilders();
        }
    }
}

