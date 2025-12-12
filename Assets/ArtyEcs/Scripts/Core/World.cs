using System;
using System.Collections.Generic;

namespace ArtyECS.Core
{
    public class World
    {
        private static World _globalWorld;

        private static readonly object _globalWorldLock = new object();

        private static readonly HashSet<World> _destroyedWorlds = new HashSet<World>();

        public readonly string Name;

        public World(string name)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }

        private static readonly Dictionary<string, World> _namedWorlds = new Dictionary<string, World>();

        private static readonly object _namedWorldsLock = new object();

        public static World GetOrCreate(string name = null)
        {
            if (string.IsNullOrEmpty(name))
            {
                if (_globalWorld == null)
                {
                    lock (_globalWorldLock)
                    {
                        if (_globalWorld == null)
                        {
                            _globalWorld = new World("Global");
                        }
                    }
                }
                return _globalWorld;
            }

            lock (_namedWorldsLock)
            {
                if (_namedWorlds.TryGetValue(name, out var existingWorld))
                {
                    return existingWorld;
                }

                var newWorld = new World(name);
                _namedWorlds[name] = newWorld;
                return newWorld;
            }
        }

        public Entity CreateEntity()
        {
            UpdateProvider.EnsureCreated();
            
            return EntitiesManager.Allocate(this);
        }

        public bool DestroyEntity(Entity entity)
        {
            if (!entity.IsValid)
            {
                return false;
            }

            ComponentsManager.RemoveAllComponents(entity, this);

            return EntitiesManager.Deallocate(entity, this);
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

        public ReadOnlySpan<Entity> GetEntitiesWith<T1, T2, T3, T4>() 
            where T1 : struct, IComponent 
            where T2 : struct, IComponent
            where T3 : struct, IComponent
            where T4 : struct, IComponent
        {
            return ComponentsManager.GetEntitiesWith<T1, T2, T3, T4>(this);
        }

        public ReadOnlySpan<Entity> GetEntitiesWith<T1, T2, T3, T4, T5>() 
            where T1 : struct, IComponent 
            where T2 : struct, IComponent
            where T3 : struct, IComponent
            where T4 : struct, IComponent
            where T5 : struct, IComponent
        {
            return ComponentsManager.GetEntitiesWith<T1, T2, T3, T4, T5>(this);
        }

        public ReadOnlySpan<Entity> GetEntitiesWith<T1, T2, T3, T4, T5, T6>() 
            where T1 : struct, IComponent 
            where T2 : struct, IComponent
            where T3 : struct, IComponent
            where T4 : struct, IComponent
            where T5 : struct, IComponent
            where T6 : struct, IComponent
        {
            return ComponentsManager.GetEntitiesWith<T1, T2, T3, T4, T5, T6>(this);
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

        public override string ToString()
        {
            return $"World({Name})";
        }

        public override bool Equals(object obj)
        {
            return ReferenceEquals(this, obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public static bool Destroy(World world)
        {
            if (world == null)
            {
                return false;
            }

            if (ReferenceEquals(world, _globalWorld))
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

            lock (_namedWorldsLock)
            {
                _namedWorlds.Remove(world.Name);
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

