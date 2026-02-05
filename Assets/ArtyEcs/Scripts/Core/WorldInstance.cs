using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

namespace ArtyECS.Core
{
    public class WorldInstance
    {
        public string Name { get; private set; }

        private QueryBuilder[] _queryBuilders = new QueryBuilder[Constants.QUERY_BUILDERS_CAPACITY];
        private int _currentQueryBuilder;

        private ArrayData<Entity>[] _arrayData = new ArrayData<Entity>[Constants.ARRAY_DATA_CAPACITY];
        private int _currentArrayData;

        private int _currentEntityIndex;
        private Entity[] _entities = new Entity[Constants.WORLD_ENTITIES_CAPACITY];
        private Dictionary<int, int> _entityIdIndexMap = new(Constants.WORLD_ENTITIES_CAPACITY);

        internal WorldInstance(string name)
        {
            Name = name;
            for (int i = 0; i < _queryBuilders.Length; i++)
            {
                _queryBuilders[i] = new QueryBuilder(this);
            }
            for (int i = 0; i < _arrayData.Length; i++)
            {
                _arrayData[i] = new ArrayData<Entity>();
            }
        }

        internal ArrayData<Entity> GetAllEntities()
        {
            var arrayData = _arrayData[_currentArrayData++];
            if (_currentArrayData == Constants.ARRAY_DATA_CAPACITY)
            {
                _currentArrayData = 0;
            }
            arrayData.Collection = _entities;
            arrayData.Elements = _currentEntityIndex;

            return arrayData;
        }

        public Entity CreateEntity(GameObject gameObject = null)
        {
            if (_entities.Length == _currentEntityIndex)
            {
                var newEntitiesArray = new Entity[_entities.Length * 2];
                Array.Copy(_entities, newEntitiesArray, _entities.Length);
                _entities = newEntitiesArray;
            }

            var entity = EntitiesPool.GetEntity(gameObject);
            entity.SetWorld(this);

            _entities[_currentEntityIndex] = entity;
            _entityIdIndexMap.Add(entity.Id, _currentEntityIndex);
            _currentEntityIndex++;

            return entity;
        }

        public void DestroyEntity(Entity entity)
        {
            _currentEntityIndex--;
            var entityIndex = _entityIdIndexMap[entity.Id];
            _entities[entityIndex] = _entities[_currentEntityIndex];
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
