using System;
using System.Collections.Generic;

namespace ArtyECS.Core
{
    public class WorldInstance
    {
        public readonly string Name;

        internal WorldInstance(string name)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
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

        public ReadOnlySpan<Entity> GetEntitiesWithout<T1, T2, T3, T4>() 
            where T1 : struct, IComponent 
            where T2 : struct, IComponent
            where T3 : struct, IComponent
            where T4 : struct, IComponent
        {
            return ComponentsManager.GetEntitiesWithout<T1, T2, T3, T4>(this);
        }

        public ReadOnlySpan<Entity> GetEntitiesWithout<T1, T2, T3, T4, T5>() 
            where T1 : struct, IComponent 
            where T2 : struct, IComponent
            where T3 : struct, IComponent
            where T4 : struct, IComponent
            where T5 : struct, IComponent
        {
            return ComponentsManager.GetEntitiesWithout<T1, T2, T3, T4, T5>(this);
        }

        public ReadOnlySpan<Entity> GetEntitiesWithout<T1, T2, T3, T4, T5, T6>() 
            where T1 : struct, IComponent 
            where T2 : struct, IComponent
            where T3 : struct, IComponent
            where T4 : struct, IComponent
            where T5 : struct, IComponent
            where T6 : struct, IComponent
        {
            return ComponentsManager.GetEntitiesWithout<T1, T2, T3, T4, T5, T6>(this);
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
