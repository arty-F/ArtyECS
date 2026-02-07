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
        public WorldInstance World { get; private set; }

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

        public T Add<T>() where T : IComponent, new()
        {
            var component = ComponentsManager.GetComponent<T>();
            var typeId = ComponentsManager.GetComponentTypeId(typeof(T));
#if UNITY_EDITOR
            if (_components.ContainsKey(typeId))
            {
                throw new ArgumentException($"Entity already has same component");
            }
#endif
            _components[typeId] = component;
            Archetype.SetFlag(typeId);
            return component;
        }

        public void Remove<T>() where T : IComponent
        {
            var componentType = typeof(T);
            var typeId = ComponentsManager.GetComponentTypeId(componentType);
            Archetype.RemoveFlag(typeId);
            ComponentsManager.Release(_components[typeId]);
            _components.Remove(typeId);
        }

        public T Get<T>() where T : IComponent
        {
            var componentType = typeof(T);
            var typeId = ComponentsManager.GetComponentTypeId(componentType);
            return (T)_components[typeId];
        }

        public bool Have<T>() where T : IComponent
        {
            var componentType = typeof(T);
            var typeId = ComponentsManager.GetComponentTypeId(componentType);
            return _components.ContainsKey(typeId);
        }

        internal void SetWorld(WorldInstance world)
        {
            World = world;
        }

        internal void Clear()
        {
            Archetype.Clear();
            World = null;
            if (GameObject != null)
            {
                UnityEngine.Object.Destroy(GameObject);
                GameObject = null;
            }
            foreach (var component in _components.Values)
            {
                ComponentsManager.Release(component);
            }
            _components.Clear();
        }
    }
}

