using System;
using System.Collections.Generic;
using UnityEngine;

namespace ArtyECS.Core
{
    public class Entity
    {
        public int Id { get; private set; }
        internal Archetype Archetype { get; private set; }
        public GameObject GameObject { get; private set; }

        private Dictionary<int, IComponent> _components = new(Constants.ENTITY_COMPONENTS_CAPACITY);

        internal Entity(int id)
        {
            Id = id;
            Archetype = new Archetype();
        }

        internal void LinkWithGameObject(GameObject gameObject)
        {
            GameObject = gameObject;
        }

        public void AddComponent(IComponent component)
        {
            //TODO Component - class, internal int ComponentTypeId
            var typeId = ComponentsManager.GetComponentTypeId(component);
#if UNITY_EDITOR
            if (_components.ContainsKey(typeId))
            {
                throw new ArgumentException($"Entity already has same component");
            }
#endif
            _components[typeId] = component;
            Archetype.SetFlag(typeId);
        }

        public void RemoveComponent(Type componentType)
        {
            var typeId = ComponentsManager.GetComponentTypeId(componentType);
            _components.Remove(typeId);
            Archetype.RemoveFlag(typeId);
        }

        public void RemoveComponent<T>() where T : IComponent
        {
            var componentType = typeof(T);
            RemoveComponent(componentType);
        }

        public T GetComponent<T>() where T : IComponent
        {
            var componentType = typeof(T);
            var typeId = ComponentsManager.GetComponentTypeId(componentType);
            return (T)_components[typeId];
        }

        public bool HasComponent<T>() where T : IComponent
        {
            var componentType = typeof(T);
            var typeId = ComponentsManager.GetComponentTypeId(componentType);
            return _components.ContainsKey(typeId);
        }

        internal void Clear()
        {
            _components.Clear();
            Archetype.Clear();
            if (GameObject != null)
            {
                UnityEngine.Object.Destroy(GameObject);
                GameObject = null;
            }
        }
    }
}

