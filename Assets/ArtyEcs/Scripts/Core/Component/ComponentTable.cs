using System;
using System.Collections.Generic;

namespace ArtyECS.Core
{
    internal class ComponentTable<T> : IComponentTable where T : struct, IComponent
    {
        private const int DefaultInitialCapacity = 32;

        private const double DictionaryLoadFactor = 0.72;

        private T[] _components;

        private Entity[] _entities;

        private int _count;

        private readonly Dictionary<Entity, int> _entityToIndex;

        internal ComponentTable() : this(DefaultInitialCapacity)
        {
        }

        internal ComponentTable(int initialCapacity)
        {
            if (initialCapacity < 1)
                throw new ArgumentException("Initial capacity must be at least 1", nameof(initialCapacity));

            _components = new T[initialCapacity];
            _entities = new Entity[initialCapacity];
            _count = 0;
            
            int dictionaryCapacity = (int)Math.Ceiling(initialCapacity / DictionaryLoadFactor);
            _entityToIndex = new Dictionary<Entity, int>(dictionaryCapacity);
        }

        internal int Count => _count;

        internal int Capacity => _components.Length;

        internal ReadOnlySpan<T> GetComponents()
        {
            return new ReadOnlySpan<T>(_components, 0, _count);
        }

        internal HashSet<Entity> GetEntitiesSet()
        {
            var entitiesSet = new HashSet<Entity>(_count);
            for (int i = 0; i < _count; i++)
            {
                entitiesSet.Add(_entities[i]);
            }
            return entitiesSet;
        }

        internal ReadOnlySpan<Entity> GetEntities()
        {
            return new ReadOnlySpan<Entity>(_entities, 0, _count);
        }

        internal Entity GetEntityAt(int index)
        {
            if (index < 0 || index >= _count)
                throw new IndexOutOfRangeException($"Index {index} is out of range [0, {_count})");

            return _entities[index];
        }

        internal bool HasComponent(Entity entity)
        {
            return _entityToIndex.ContainsKey(entity);
        }

        internal bool TryGetIndex(Entity entity, out int index)
        {
            return _entityToIndex.TryGetValue(entity, out index);
        }

        internal bool TryGetComponent(Entity entity, out T component)
        {
            if (_entityToIndex.TryGetValue(entity, out int index))
            {
                component = _components[index];
                return true;
            }

            component = default(T);
            return false;
        }

        internal ref T GetModifiableComponentRef(Entity entity)
        {
            if (!_entityToIndex.TryGetValue(entity, out int index))
            {
                throw new KeyNotFoundException($"Entity {entity} does not have a component of type {typeof(T).Name}");
            }

            return ref _components[index];
        }

        private void EnsureCapacity(int minCapacity)
        {
            if (_components.Length >= minCapacity)
                return;

            int newCapacity = Math.Max(_components.Length * 2, minCapacity);

            var newComponents = new T[newCapacity];
            Array.Copy(_components, 0, newComponents, 0, _count);
            _components = newComponents;

            var newEntities = new Entity[newCapacity];
            Array.Copy(_entities, 0, newEntities, 0, _count);
            _entities = newEntities;
        }

        internal (T[] components, Entity[] entities, Dictionary<Entity, int> entityToIndex) GetInternalTable()
        {
            return (_components, _entities, _entityToIndex);
        }

        public bool TryRemoveComponentForEntity(Entity entity)
        {
            if (!HasComponent(entity))
            {
                return false;
            }

            RemoveComponentInternal(entity);
            return true;
        }

        ReadOnlySpan<Entity> IComponentTable.GetEntities()
        {
            return GetEntities();
        }

        bool IComponentTable.HasComponentForEntity(Entity entity)
        {
            return HasComponent(entity);
        }

        object IComponentTable.GetComponentValue(Entity entity)
        {
            if (TryGetComponent(entity, out T component))
            {
                return component;
            }
            return null;
        }

        void IComponentTable.AddComponentForEntity(Entity entity, object componentValue)
        {
            if (componentValue == null)
                throw new ArgumentNullException(nameof(componentValue));

            if (!(componentValue is T))
                throw new ArgumentException($"Component value must be of type {typeof(T).Name}", nameof(componentValue));

            if (HasComponent(entity))
                throw new InvalidOperationException($"Entity already has component of type {typeof(T).Name}");

            T component = (T)componentValue;

            EnsureCapacity(_count + 1);

            _components[_count] = component;
            _entities[_count] = entity;
            _entityToIndex[entity] = _count;

            _count++;
        }

        internal void RemoveComponentInternal(Entity entity)
        {
            if (!_entityToIndex.TryGetValue(entity, out int removeIndex))
            {
                return;
            }

            int lastIndex = _count - 1;

            if (removeIndex == lastIndex)
            {
                _entityToIndex.Remove(entity);
                _count--;
                return;
            }

            Entity lastEntity = _entities[lastIndex];
            T lastComponent = _components[lastIndex];

            _components[removeIndex] = lastComponent;
            _entities[removeIndex] = lastEntity;

            _entityToIndex[lastEntity] = removeIndex;

            _entityToIndex.Remove(entity);

            _count--;
        }
    }
}

