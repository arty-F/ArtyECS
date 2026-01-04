using System.Collections.Generic;

namespace ArtyECS.Core
{
    public class World
    {
        private static string GLOBAL_WORLD_NAME = "Global";

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

        public static Entity CreateEntity()
        {
            return Global.CreateEntity();
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

        public static void Clear()
        {
            Global.Clear();
            foreach (var key in _localWorlds.Keys)
            {
                _localWorlds[key].Clear();
            }
            _localWorlds.Clear();

            ComponentsManager.Clear();
            EntitiesPool.Clear();
        }
    }
}

