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

        private Dictionary<int, Component> _components = new(Constants.ENTITY_COMPONENTS_CAPACITY);


        internal Entity(int id)
        {
            Id = id;
            Archetype = new Archetype();
        }

        internal void LinkWithGameObject(GameObject gameObject)
        {
            GameObject = gameObject;
        }

        public T Add<T>() where T : Component, new()
        {
            var component = ComponentsManager.GetComponent<T>(this);
#if UNITY_EDITOR
            if (_components.ContainsKey(component.TypeId))
            {
                throw new ArgumentException($"Entity already has same component");
            }
#endif
            _components[component.TypeId] = component;
            Archetype.SetFlag(component.TypeId);
            return component;
        }

        public T AddUniq<T>() where T : Component, new()
        {
            var component = Add<T>();
            component.Uniq = true;
            World.SetUniq<T>(component);
            return component;
        }

        public void Remove<T>() where T : Component
        {
            var component = Get<T>();
            if (component.Uniq)
            {
                World.RemoveUniq(component);
            }
            Archetype.RemoveFlag(component.TypeId);
            ComponentsManager.Release(_components[component.TypeId]);
            _components.Remove(component.TypeId);
        }

        public T Get<T>() where T : Component
        {
            var componentType = typeof(T);
            var typeId = ComponentsManager.GetComponentTypeId(componentType);
            return (T)_components[typeId];
        }

        public bool Have<T>() where T : Component
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
                if (component.Uniq)
                {
                    World.RemoveUniq(component);
                }
                component.Clear();
                ComponentsManager.Release(component);
            }
            _components.Clear();
        }
    }
}

