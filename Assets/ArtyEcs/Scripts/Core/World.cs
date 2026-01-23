using ArtyEcs.Core;
using System.Collections.Generic;
using UnityEngine;

namespace ArtyECS.Core
{
    public class World
    {
        private static readonly string GLOBAL_WORLD_NAME = "Global";

        private static WorldInstance _global;
        public static WorldInstance Global
        {
            get 
            { 
                if (_global == null)
                {
                    _global = new WorldInstance(GLOBAL_WORLD_NAME);
                }
                return _global; 
            }
        }

        private static Dictionary<string, WorldInstance> _localWorlds = new();

        private World() { }

        public static WorldInstance GetOrCreate(string name)
        {
            if (string.Equals(name, GLOBAL_WORLD_NAME))
            {
                return Global;
            }

            if(!_localWorlds.ContainsKey(name))
            {
                _localWorlds.Add(name, new WorldInstance(name));
            }

            return _localWorlds[name];
        }

        public static Entity CreateEntity(GameObject gameObject = null)
        {
            return Global.CreateEntity(gameObject);
        }

        public static void DestroyEntity(Entity entity)
        {
            Global.DestroyEntity(entity);
        }

        public static IEnumerable<Entity> GetAllEntities()
        {
            return Global.GetAllEntities();
        }

        public static QueryBuilder Query()
        {
            return Global.Query();
        }

        public static void RegisterSystem(SystemHandler system, UpdateType type = UpdateType.Update)
        {
            Global.RegisterSystem(system, type);
        }

        public static void ExecuteSystems(UpdateType type)
        {
            Global.ExecuteSystems(type);
        }

        public static void Clear()
        {
            /*Global.Clear();
            foreach (var key in _localWorlds.Keys)
            {
                _localWorlds[key].Clear();
            }*/
            _global = null;
            _localWorlds.Clear();

            EntitiesPool.Clear();
            UpdateProvider.Clear();
            ComponentsManager.Clear();
        }
    }
}

