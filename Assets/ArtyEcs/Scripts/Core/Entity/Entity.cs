using System;
using System.Collections.Generic;

namespace ArtyECS.Core
{
    public class Entity
    {
        public int Id { get; private set; }

        private Dictionary<int, ComponentWrapper> _components = new(Constants.ENTITY_COMPONENTS_CAPACITY);

        internal Entity(int id)
        {
            Id = id;
        }

        public void AddComponent(IComponent component)
        {
            var wrapper = ComponentsManager.WrapComponent(component);
#if UNITY_EDITOR
            if (_components.ContainsKey(wrapper.Id))
            {
                throw new ArgumentException($"Entity already has same component");
            }
#endif
            _components[wrapper.Id] = wrapper;
        }

        public void RemoveComponent(IComponent component)
        {
            var wrapperId = ComponentsManager.GetWrapperId(component);
            _components.Remove(wrapperId);
        }

        public T GetComponent<T>() where T : IComponent
        {
            var componentType = typeof(T);
            var wrapperId = ComponentsManager.GetWrapperId(componentType);
            return (T)_components[wrapperId].Component;
        }

        internal void Clear()
        {
            _components.Clear();
        }
    }
}

