using System;
using System.Collections.Generic;

namespace ArtyECS.Core
{
    public class Entity
    {
        public int Id { get; private set; }
        public Archetype Archetype { get; private set; }

        private Dictionary<int, IComponent> _components = new(Constants.ENTITY_COMPONENTS_CAPACITY);

        internal Entity(int id)
        {
            Id = id;
            Archetype = new Archetype();
        }

        public void AddComponent(IComponent component)
        {
            var typeId = ComponentsManager.GetComponentTypeId(component);
#if UNITY_EDITOR
            if (_components.ContainsKey(typeId))
            {
                throw new ArgumentException($"Entity already has same component");
            }
#endif
            _components[typeId] = component;
            Archetype.SetBit(typeId);
        }

        public void RemoveComponent(IComponent component)
        {
            var typeId = ComponentsManager.GetComponentTypeId(component);
            _components.Remove(typeId);
            Archetype.ClearBit(typeId);
        }

        public T GetComponent<T>() where T : IComponent
        {
            var componentType = typeof(T);
            var typeId = ComponentsManager.GetComponentTypeId(componentType);
            return (T)_components[typeId];
        }

        internal void Clear()
        {
            _components.Clear();
            Archetype = new Archetype();
        }
    }
}

