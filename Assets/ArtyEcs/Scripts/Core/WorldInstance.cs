using System.Collections.Generic;
using UnityEngine;

namespace ArtyECS.Core
{
    public class WorldInstance
    {
        public string Name { get; private set; }

        private QueryBuilder[] _queryBuilders = new QueryBuilder[Constants.QUERY_BUILDERS_CAPACITY];
        private int _currentQueryBuilder;

        private int _currentEntityIndex;
        private List<Entity> _entities = new(Constants.WORLD_ENTITIES_CAPACITY);
        private Dictionary<int, int> _entityIdIndexMap = new(Constants.WORLD_ENTITIES_CAPACITY);

        internal WorldInstance(string name)
        {
            Name = name;
            for (int i = 0; i < _queryBuilders.Length; i++)
            {
                _queryBuilders[i] = new QueryBuilder(this);
            }
        }

        internal List<Entity> GetAllEntities()
        {
            return _entities;
        }

        public Entity CreateEntity(GameObject gameObject = null)
        {
            var entity = EntitiesPool.GetEntity(gameObject);
            entity.SetWorld(this);

            _entityIdIndexMap.Add(entity.Id, _entities.Count);
            _entities.Add(entity);

            return entity;
        }

        public void DestroyEntity(Entity entity)
        {
            //TODO когда удаляем у остальных индексы сдвигаются и начинают не соответствовать _entityIdIndexMap
            //TODO + есть какойто баг что powerups не собираются
            var entityIndex = _entityIdIndexMap[entity.Id];
            //_entities[entityIndex] = _entities[_currentEntityIndex];
            _entities.RemoveAt(entityIndex);
            _entityIdIndexMap.Remove(entity.Id);
            EntitiesPool.Release(entity);
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
            for (int i = 0; i < _currentEntityIndex; i++)
            {
                DestroyEntity(_entities[i]);
            }
        }
    }
}
