using System;
using System.Collections.Generic;
using System.Linq;

namespace ArtyECS.Core
{
    public class WorldInstance
    {
        public string Name { get; private set; }

        public Dictionary<int, Entity> _entities = new();

        private QueryBuilder _query;

        internal WorldInstance(string name)
        {
            Name = name;
            _query = new(this);
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

        public IEnumerable<Entity> GetAllEntities()
        {
            foreach (var entity in _entities.Values)
            {
                yield return entity;
            }
        }

        internal IEnumerable<Entity> GetAllEntities(Archetype archetype)
        {
            foreach (var entity in _entities.Values)
            {
                if (entity.Archetype.Contains(archetype))
                {
                    yield return entity;
                }
            }
        }

        public QueryBuilder Query()
        {
            _query.StartQuery();
            return _query;
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
