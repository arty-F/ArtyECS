using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ArtyECS.Core
{
    public class WorldInstance
    {
        public string Name { get; private set; }

        internal Dictionary<int, Entity> _entities = new(Constants.WORLD_ENTITIES_CAPACITY);
        internal Dictionary<Archetype, Entity> _archetypesMap = new(Constants.WORLD_ARCHETYPES_CAPACITY);
        private QueryBuilder[] _queryBuilders = new QueryBuilder[Constants.QUERY_BUILDERS_CAPACITY];
        private int _currentQueryBuilder;

        internal WorldInstance(string name)
        {
            Name = name;
            for (int i = 0; i < _queryBuilders.Length; i++)
            {
                _queryBuilders[i] = new QueryBuilder(this);
            }
        }

        public Entity CreateEntity(GameObject gameObject = null)
        {
            var entity = EntitiesPool.GetEntity(gameObject);
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

        public QueryBuilder Query()
        {
            var queryBuilder = _queryBuilders[_currentQueryBuilder++];
            if (_currentQueryBuilder == Constants.QUERY_BUILDERS_CAPACITY)
            {
                _currentQueryBuilder = 0;
            }
            queryBuilder.StartQuery();
            return queryBuilder;
        }

        public void RegisterSystem(SystemHandler system, UpdateType type = UpdateType.Update)
        {
            UpdateProvider.GetOrCreate().RegisterSystem(system, this, type);
        }

        public void ExecuteSystems(UpdateType type)
        {
            UpdateProvider.GetOrCreate().ExecuteSystems(this, type);
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
