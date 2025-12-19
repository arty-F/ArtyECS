using System;
using System.Collections.Generic;

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

        public static bool DestroyEntity(Entity entity)
        {
            if (!entity.IsValid)
            {
                return false;
            }

            ComponentsManager.RemoveAllComponents(entity, GlobalWorld);
            return EntitiesManager.Deallocate(entity, GlobalWorld);
        }

        public static bool IsEntityValid(Entity entity)
        {
            return EntitiesManager.IsAllocated(entity, GlobalWorld);
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
        }
    }
}

