using System.Collections.Generic;
using System.Linq;

namespace ArtyECS.Core
{
    public class WorldInstance
    {
        public string Name { get; private set; }

        public Dictionary<int, Entity> _entities = new();

        internal WorldInstance(string name)
        {
            Name = name;
        }

        public Entity CreateEntity()
        {
            var entity = EntitiesPool.GetEntity();
            _entities.Add(entity.Id, entity);
            return entity;
        }

        public void DestroyEntity(Entity entity)
        {
            EntitiesPool.Release(entity);
            _entities.Remove(entity.Id);
        }

        public void Clear()
        {
            foreach (var key in _entities.Keys.ToList())
            {
                DestroyEntity(_entities[key]);
            }
        }
    }
}
