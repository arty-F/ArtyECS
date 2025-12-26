using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

namespace ArtyECS.Core
{
    public class World
    {
        private static WorldInstance _globalWorld;
        public static WorldInstance GlobalWorld
        {
            get 
            { 
                if (_globalWorld == null)
                {
                    _globalWorld = GetOrCreate();
                }
                return _globalWorld;
            }
            private set { _globalWorld = value; }
        }

        private static readonly Dictionary<string, WorldInstance> _localWorlds = new();
        private static readonly object _globalWorldLock = new();
        private static readonly object _localWorldsLock = new();
        private static readonly HashSet<WorldInstance> _destroyedWorlds = new();

        public static WorldInstance GetOrCreate(string name = null)
        {
            if (string.IsNullOrEmpty(name))
            {
                if (_globalWorld == null)
                {
                    lock (_globalWorldLock)
                    {
                        if (_globalWorld == null)
                        {
                            _globalWorld = new WorldInstance("Global");
                        }
                    }
                }
                return _globalWorld;
            }

            lock (_localWorldsLock)
            {
                if (_localWorlds.TryGetValue(name, out var existingWorld))
                {
                    return existingWorld;
                }

                var newWorld = new WorldInstance(name);
                _localWorlds[name] = newWorld;
                return newWorld;
            }
        }

        public static Entity CreateEntity()
        {
            UpdateProvider.EnsureCreated();
            return EntitiesManager.Allocate(GlobalWorld);
        }

        public static Entity CreateEntity(GameObject gameObject)
        {
            if (gameObject == null)
                throw new ArgumentNullException(nameof(gameObject));

            return GlobalWorld.CreateEntity(gameObject);
        }

        public static bool DestroyEntity(Entity entity)
        {
            return GlobalWorld.DestroyEntity(entity);
        }

        public static bool IsEntityValid(Entity entity)
        {
            return EntitiesManager.IsAllocated(entity, GlobalWorld);
        }

        public static GameObject GetGameObject(Entity entity)
        {
            return GlobalWorld.GetGameObject(entity);
        }

        public static Entity? GetEntity(GameObject gameObject)
        {
            return GlobalWorld.GetEntity(gameObject);
        }

        public static ReadOnlySpan<Entity> GetEntitiesWith<T1>() where T1 : struct, IComponent
        {
            return ComponentsManager.GetEntitiesWith<T1>(GlobalWorld);
        }

        public static ReadOnlySpan<Entity> GetEntitiesWith<T1, T2>() 
            where T1 : struct, IComponent 
            where T2 : struct, IComponent
        {
            return ComponentsManager.GetEntitiesWith<T1, T2>(GlobalWorld);
        }

        public static ReadOnlySpan<Entity> GetEntitiesWith<T1, T2, T3>() 
            where T1 : struct, IComponent 
            where T2 : struct, IComponent
            where T3 : struct, IComponent
        {
            return ComponentsManager.GetEntitiesWith<T1, T2, T3>(GlobalWorld);
        }

        public static ReadOnlySpan<Entity> GetEntitiesWith<T1, T2, T3, T4>() 
            where T1 : struct, IComponent 
            where T2 : struct, IComponent
            where T3 : struct, IComponent
            where T4 : struct, IComponent
        {
            return ComponentsManager.GetEntitiesWith<T1, T2, T3, T4>(GlobalWorld);
        }

        public static ReadOnlySpan<Entity> GetEntitiesWith<T1, T2, T3, T4, T5>() 
            where T1 : struct, IComponent 
            where T2 : struct, IComponent
            where T3 : struct, IComponent
            where T4 : struct, IComponent
            where T5 : struct, IComponent
        {
            return ComponentsManager.GetEntitiesWith<T1, T2, T3, T4, T5>(GlobalWorld);
        }

        public static ReadOnlySpan<Entity> GetEntitiesWith<T1, T2, T3, T4, T5, T6>() 
            where T1 : struct, IComponent 
            where T2 : struct, IComponent
            where T3 : struct, IComponent
            where T4 : struct, IComponent
            where T5 : struct, IComponent
            where T6 : struct, IComponent
        {
            return ComponentsManager.GetEntitiesWith<T1, T2, T3, T4, T5, T6>(GlobalWorld);
        }

        public static ReadOnlySpan<Entity> GetEntitiesWithout<T1>() where T1 : struct, IComponent
        {
            return ComponentsManager.GetEntitiesWithout<T1>(GlobalWorld);
        }

        public static ReadOnlySpan<Entity> GetEntitiesWithout<T1, T2>() 
            where T1 : struct, IComponent 
            where T2 : struct, IComponent
        {
            return ComponentsManager.GetEntitiesWithout<T1, T2>(GlobalWorld);
        }

        public static ReadOnlySpan<Entity> GetEntitiesWithout<T1, T2, T3>() 
            where T1 : struct, IComponent 
            where T2 : struct, IComponent
            where T3 : struct, IComponent
        {
            return ComponentsManager.GetEntitiesWithout<T1, T2, T3>(GlobalWorld);
        }

        public static ReadOnlySpan<Entity> GetEntitiesWithout<T1, T2, T3, T4>() 
            where T1 : struct, IComponent 
            where T2 : struct, IComponent
            where T3 : struct, IComponent
            where T4 : struct, IComponent
        {
            return ComponentsManager.GetEntitiesWithout<T1, T2, T3, T4>(GlobalWorld);
        }

        public static ReadOnlySpan<Entity> GetEntitiesWithout<T1, T2, T3, T4, T5>() 
            where T1 : struct, IComponent 
            where T2 : struct, IComponent
            where T3 : struct, IComponent
            where T4 : struct, IComponent
            where T5 : struct, IComponent
        {
            return ComponentsManager.GetEntitiesWithout<T1, T2, T3, T4, T5>(GlobalWorld);
        }

        public static ReadOnlySpan<Entity> GetEntitiesWithout<T1, T2, T3, T4, T5, T6>() 
            where T1 : struct, IComponent 
            where T2 : struct, IComponent
            where T3 : struct, IComponent
            where T4 : struct, IComponent
            where T5 : struct, IComponent
            where T6 : struct, IComponent
        {
            return ComponentsManager.GetEntitiesWithout<T1, T2, T3, T4, T5, T6>(GlobalWorld);
        }

        public static QueryBuilder Query()
        {
            return new QueryBuilder(GlobalWorld);
        }

        public static ReadOnlySpan<Entity> GetAllEntities()
        {
            return GlobalWorld.GetAllEntities();
        }

        public static T GetComponent<T>(Entity entity) where T : struct, IComponent
        {
            return ComponentsManager.GetComponent<T>(entity, GlobalWorld);
        }

        public static ref T GetModifiableComponent<T>(Entity entity) where T : struct, IComponent
        {
            return ref ComponentsManager.GetModifiableComponent<T>(entity, GlobalWorld);
        }

        public static void AddComponent<T>(Entity entity, T component) where T : struct, IComponent
        {
            ComponentsManager.AddComponent(entity, component, GlobalWorld);
        }

        public static bool RemoveComponent<T>(Entity entity) where T : struct, IComponent
        {
            return ComponentsManager.RemoveComponent<T>(entity, GlobalWorld);
        }

        public static ReadOnlySpan<T> GetComponents<T>() where T : struct, IComponent
        {
            return ComponentsManager.GetComponents<T>(GlobalWorld);
        }

        public static ModifiableComponentCollection<T> GetModifiableComponents<T>() where T : struct, IComponent
        {
            return ComponentsManager.GetModifiableComponents<T>(GlobalWorld);
        }

        public static ComponentInfo[] GetAllComponentInfos(Entity entity)
        {
            return GlobalWorld.GetAllComponentInfos(entity);
        }

        public static Entity CloneEntity(Entity source)
        {
            return GlobalWorld.CloneEntity(source);
        }

        public static void AddToUpdate(SystemHandler system)
        {
            SystemsManager.AddToUpdate(system, GlobalWorld);
        }

        public static void AddToUpdate(SystemHandler system, int order)
        {
            SystemsManager.AddToUpdate(system, order, GlobalWorld);
        }

        public static void AddToFixedUpdate(SystemHandler system)
        {
            SystemsManager.AddToFixedUpdate(system, GlobalWorld);
        }

        public static void AddToFixedUpdate(SystemHandler system, int order)
        {
            SystemsManager.AddToFixedUpdate(system, order, GlobalWorld);
        }

        public static void ExecuteOnce(SystemHandler system)
        {
            SystemsManager.ExecuteOnce(system, GlobalWorld);
        }

        public static void ExecuteUpdate()
        {
            SystemsManager.ExecuteUpdate(GlobalWorld);
        }

        public static void ExecuteFixedUpdate()
        {
            SystemsManager.ExecuteFixedUpdate(GlobalWorld);
        }

        public static bool RemoveFromUpdate(SystemHandler system)
        {
            return SystemsManager.RemoveFromUpdate(system, GlobalWorld);
        }

        public static bool RemoveFromFixedUpdate(SystemHandler system)
        {
            return SystemsManager.RemoveFromFixedUpdate(system, GlobalWorld);
        }

        public static IReadOnlyList<SystemHandler> GetUpdateQueue()
        {
            return GlobalWorld.GetUpdateQueue();
        }

        public static IReadOnlyList<SystemHandler> GetFixedUpdateQueue()
        {
            return GlobalWorld.GetFixedUpdateQueue();
        }

        public static IReadOnlyList<WorldInstance> GetAllWorlds()
        {
            var worlds = new List<WorldInstance>();
            
            if (_globalWorld != null)
            {
                worlds.Add(_globalWorld);
            }
            
            lock (_localWorldsLock)
            {
                foreach (var kvp in _localWorlds)
                {
                    var world = kvp.Value;
                    if (!_destroyedWorlds.Contains(world))
                    {
                        worlds.Add(world);
                    }
                }
            }
            
            return new ReadOnlyCollection<WorldInstance>(worlds);
        }

        public static bool Exists(string name)
        {
            if (string.IsNullOrEmpty(name) || name == "Global")
            {
                return _globalWorld != null;
            }
            
            lock (_localWorldsLock)
            {
                if (_localWorlds.TryGetValue(name, out var world))
                {
                    return !_destroyedWorlds.Contains(world);
                }
            }
            
            return false;
        }

        public override string ToString()
        {
            return $"World({GlobalWorld.Name})";
        }

        public override bool Equals(object obj)
        {
            return ReferenceEquals(GlobalWorld, obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public static bool Destroy(WorldInstance world)
        {
            if (world == null)
            {
                return false;
            }

            if (ReferenceEquals(world, GlobalWorld))
            {
                return false;
            }

            if (_destroyedWorlds.Contains(world))
            {
                return false;
            }

            ComponentsManager.ClearWorld(world);
            EntitiesManager.ClearWorld(world);
            SystemsManager.ClearWorld(world);

            lock (_localWorldsLock)
            {
                _localWorlds.Remove(world.Name);
            }

            _destroyedWorlds.Add(world);

            return true;
        }

        public static void ClearAllECSState()
        {
            ComponentsManager.ClearAll();
            EntitiesManager.ClearAll();
            SystemsManager.ClearAll();
            _destroyedWorlds.Clear();
            
            lock (_localWorldsLock)
            {
                _localWorlds.Clear();
            }
            
            _globalWorld = null;
        }
    }
}

