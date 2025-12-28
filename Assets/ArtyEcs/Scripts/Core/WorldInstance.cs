using System;
using System.Collections.Generic;
using UnityEngine;

namespace ArtyECS.Core
{
    public class WorldInstance
    {
        public readonly string Name;

        private readonly Dictionary<Entity, GameObject> _entityToGameObject = new();
        private readonly Dictionary<int, Entity> _gameObjectIdToEntity = new();

        internal WorldInstance(string name)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }

        public Entity CreateEntity()
        {
            UpdateProvider.EnsureCreated();
            return EntitiesManager.Allocate(this);
        }

        public Entity CreateEntity(GameObject gameObject)
        {
            if (gameObject == null)
                throw new ArgumentNullException(nameof(gameObject));

            Entity entity = CreateEntity();
            LinkEntity(entity, gameObject);
            return entity;
        }

        public bool DestroyEntity(Entity entity)
        {
            if (!entity.IsValid)
            {
                return false;
            }

            GameObject linkedGameObject = GetGameObject(entity);
            
            UnlinkEntity(entity);

            if (linkedGameObject != null)
            {
                UnityEngine.Object.Destroy(linkedGameObject);
            }

            ComponentsManager.RemoveAllComponents(entity, this);

            return EntitiesManager.Deallocate(entity, this);
        }

        internal void LinkEntity(Entity entity, GameObject gameObject)
        {
            if (gameObject == null)
                throw new ArgumentNullException(nameof(gameObject));

            var gameObjectId = gameObject.GetInstanceID();

            if (_gameObjectIdToEntity.TryGetValue(gameObjectId, out var existingEntity))
            {
                if (existingEntity != entity)
                {
                    throw new InvalidOperationException(
                        $"GameObject {gameObject.name} is already linked to entity {existingEntity}");
                }
                return;
            }

            if (_entityToGameObject.TryGetValue(entity, out var existingGameObject))
            {
                var existingGameObjectId = existingGameObject.GetInstanceID();
                _entityToGameObject.Remove(entity);
                _gameObjectIdToEntity.Remove(existingGameObjectId);
            }

            _entityToGameObject[entity] = gameObject;
            _gameObjectIdToEntity[gameObjectId] = entity;
        }

        internal void UnlinkEntity(Entity entity)
        {
            if (_entityToGameObject.TryGetValue(entity, out var gameObject))
            {
                _entityToGameObject.Remove(entity);
                if (gameObject != null)
                {
                    var gameObjectId = gameObject.GetInstanceID();
                    _gameObjectIdToEntity.Remove(gameObjectId);
                }
            }
        }

        public GameObject GetGameObject(Entity entity)
        {
            if (_entityToGameObject.TryGetValue(entity, out var gameObject))
            {
                if (gameObject == null)
                {
                    UnlinkEntity(entity);
                    return null;
                }
                return gameObject;
            }
            return null;
        }

        public Entity? GetEntity(GameObject gameObject)
        {
            if (gameObject == null)
                return null;

            var gameObjectId = gameObject.GetInstanceID();
            if (_gameObjectIdToEntity.TryGetValue(gameObjectId, out var entity))
            {
                return entity;
            }
            return null;
        }

        public bool IsEntityValid(Entity entity)
        {
            return EntitiesManager.IsAllocated(entity, this);
        }

        public ReadOnlySpan<Entity> GetEntitiesWith<T1>() where T1 : struct, IComponent
        {
            return ComponentsManager.GetEntitiesWith<T1>(this);
        }

        public ReadOnlySpan<Entity> GetEntitiesWith<T1, T2>() 
            where T1 : struct, IComponent 
            where T2 : struct, IComponent
        {
            return ComponentsManager.GetEntitiesWith<T1, T2>(this);
        }

        public ReadOnlySpan<Entity> GetEntitiesWith<T1, T2, T3>() 
            where T1 : struct, IComponent 
            where T2 : struct, IComponent
            where T3 : struct, IComponent
        {
            return ComponentsManager.GetEntitiesWith<T1, T2, T3>(this);
        }

        public ReadOnlySpan<Entity> GetEntitiesWithout<T1>() where T1 : struct, IComponent
        {
            return ComponentsManager.GetEntitiesWithout<T1>(this);
        }

        public ReadOnlySpan<Entity> GetEntitiesWithout<T1, T2>() 
            where T1 : struct, IComponent 
            where T2 : struct, IComponent
        {
            return ComponentsManager.GetEntitiesWithout<T1, T2>(this);
        }

        public ReadOnlySpan<Entity> GetEntitiesWithout<T1, T2, T3>() 
            where T1 : struct, IComponent 
            where T2 : struct, IComponent
            where T3 : struct, IComponent
        {
            return ComponentsManager.GetEntitiesWithout<T1, T2, T3>(this);
        }

        public QueryBuilder Query()
        {
            return new QueryBuilder(this);
        }

        public ReadOnlySpan<Entity> GetAllEntities()
        {
            var entitySet = ComponentsManager.GetAllEntitiesInWorld(this);
            if (entitySet.Count == 0)
            {
                return ReadOnlySpan<Entity>.Empty;
            }
            
            var entityArray = new Entity[entitySet.Count];
            int index = 0;
            foreach (var entity in entitySet)
            {
                entityArray[index++] = entity;
            }
            
            return new ReadOnlySpan<Entity>(entityArray);
        }

        public T GetComponent<T>(Entity entity) where T : struct, IComponent
        {
            return ComponentsManager.GetComponent<T>(entity, this);
        }

        public ref T GetModifiableComponent<T>(Entity entity) where T : struct, IComponent
        {
            return ref ComponentsManager.GetModifiableComponent<T>(entity, this);
        }

        public void AddComponent<T>(Entity entity, T component) where T : struct, IComponent
        {
            ComponentsManager.AddComponent(entity, component, this);
        }

        public bool RemoveComponent<T>(Entity entity) where T : struct, IComponent
        {
            return ComponentsManager.RemoveComponent<T>(entity, this);
        }

        public ReadOnlySpan<T> GetComponents<T>() where T : struct, IComponent
        {
            return ComponentsManager.GetComponents<T>(this);
        }

        public ModifiableComponentCollection<T> GetModifiableComponents<T>() where T : struct, IComponent
        {
            return ComponentsManager.GetModifiableComponents<T>(this);
        }

        public ComponentInfo[] GetAllComponentInfos(Entity entity)
        {
            return ComponentsManager.GetAllComponentInfos(entity, this);
        }

        public Entity CloneEntity(Entity source)
        {
            return ComponentsManager.CloneEntity(source, this);
        }

        public void AddToUpdate(SystemHandler system)
        {
            SystemsManager.AddToUpdate(system, this);
        }

        public void AddToUpdate(SystemHandler system, int order)
        {
            SystemsManager.AddToUpdate(system, order, this);
        }

        public void AddToFixedUpdate(SystemHandler system)
        {
            SystemsManager.AddToFixedUpdate(system, this);
        }

        public void AddToFixedUpdate(SystemHandler system, int order)
        {
            SystemsManager.AddToFixedUpdate(system, order, this);
        }

        public void ExecuteOnce(SystemHandler system)
        {
            SystemsManager.ExecuteOnce(system, this);
        }

        public void ExecuteUpdate()
        {
            SystemsManager.ExecuteUpdate(this);
        }

        public void ExecuteFixedUpdate()
        {
            SystemsManager.ExecuteFixedUpdate(this);
        }

        public bool RemoveFromUpdate(SystemHandler system)
        {
            return SystemsManager.RemoveFromUpdate(system, this);
        }

        public bool RemoveFromFixedUpdate(SystemHandler system)
        {
            return SystemsManager.RemoveFromFixedUpdate(system, this);
        }

        public IReadOnlyList<SystemHandler> GetUpdateQueue()
        {
            return SystemsManager.GetUpdateQueue(this);
        }

        public IReadOnlyList<SystemHandler> GetFixedUpdateQueue()
        {
            return SystemsManager.GetFixedUpdateQueue(this);
        }

#if UNITY_EDITOR
        public SystemTimingData? GetSystemTiming(SystemHandler system)
        {
            if (system == null)
                return null;
            return SystemsManager.GetSystemTiming(system, this);
        }

        public System.Collections.Generic.List<SystemTimingData> GetAllSystemTimings()
        {
            return SystemsManager.GetAllSystemTimings(this);
        }

        public QueryTimingData? GetQueryTiming(QueryType queryType)
        {
            return ComponentsManager.GetQueryTiming(queryType, this);
        }

        public System.Collections.Generic.List<QueryTimingData> GetAllQueryTimings()
        {
            return ComponentsManager.GetAllQueryTimings(this);
        }

#endif

#if UNITY_EDITOR
        internal static (int EntityToGameObjectCount, int GameObjectIdToEntityCount)? GetLinksForMonitoring(WorldInstance world)
        {
            if (world == null)
                return null;

            return (world._entityToGameObject.Count, world._gameObjectIdToEntity.Count);
        }
#endif

        public override string ToString()
        {
            return $"WorldInstance({Name})";
        }

        public override bool Equals(object obj)
        {
            return ReferenceEquals(this, obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
