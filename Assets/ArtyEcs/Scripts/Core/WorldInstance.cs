using System;
using System.Collections.Generic;
using UnityEngine;

namespace ArtyECS.Core
{
    public class WorldInstance
    {
        public string Name { get; private set; }

        private List<QueryBuilder> _queryBuilders = new();
        private int _currentQueryBuilder;

        private int _currentEntityIndex;
        private Entity[] _entities = new Entity[Constants.WORLD_ENTITIES_CAPACITY];
        private Dictionary<int, int> _entityIdIndexMap = new(Constants.WORLD_ENTITIES_CAPACITY);

        private CollectionWrapper<Entity> _wrapper = new();

        private Dictionary<int, Context> _uniqContexts = new();

        internal WorldInstance(string name)
        {
            Name = name;
        }

        internal CollectionWrapper<Entity> GetAllEntities()
        {
            _wrapper.Collection = _entities;
            _wrapper.Length = _currentEntityIndex;
            return _wrapper;
        }

        internal void ResetQueryBuilders()
        {
            _currentQueryBuilder = 0;
        }

        internal void SetUniq<T>(Context context) where T : Context, new()
        {
            if (_uniqContexts.ContainsKey(context.TypeId))
            {
                throw new ArgumentException($"World <{Name}> already has uniq context <{typeof(T).FullName}>");
            }

            _uniqContexts[context.TypeId] = context;
        }

        internal void RemoveUniq(Context context)
        {
            _uniqContexts.Remove(context.TypeId);
        }

        public Entity CreateEntity(GameObject gameObject = null)
        {
            if (_currentEntityIndex == _entities.Length)
            {
                var newEntitiesArray = new Entity[_entities.Length * 2];
                Array.Copy(_entities, newEntitiesArray, _entities.Length);
                _entities = newEntitiesArray;
            }

            var entity = EntitiesPool.GetEntity(gameObject);
            entity.SetWorld(this);

            _entities[_currentEntityIndex] = entity;
            _entityIdIndexMap[entity.Id] = _currentEntityIndex;
            _currentEntityIndex++;

            return entity;
        }

        public void DestroyEntity(Entity entity)
        {
            _currentEntityIndex--;
            var removingEntityIndex = _entityIdIndexMap[entity.Id];
            _entityIdIndexMap.Remove(entity.Id);

            var lastEntity = _entities[_currentEntityIndex];
            _entities[removingEntityIndex] = lastEntity;
            _entityIdIndexMap[lastEntity.Id] = removingEntityIndex;

            EntitiesPool.Release(entity);
        }

        public QueryBuilder Query()
        {
            if (_currentQueryBuilder == _queryBuilders.Count)
            {
                _queryBuilders.Add(new QueryBuilder(this));
            }
            var queryBuilder = _queryBuilders[_currentQueryBuilder];
            _currentQueryBuilder++;
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

        public T GetUniqContext<T>() where T : Context, new()
        {
            var typeId = ComponentsManager.GetComponentTypeId(typeof(T));
            return (T)_uniqContexts[typeId];
        }

        public void Clear()
        {
            for (int i = 0; i < _currentEntityIndex; i++)
            {
                DestroyEntity(_entities[i]);
            }
            _uniqContexts.Clear();
        }
    }
}
