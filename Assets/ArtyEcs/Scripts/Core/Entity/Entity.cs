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

        private Dictionary<int, Context> _components = new(Constants.ENTITY_COMPONENTS_CAPACITY);

        internal Entity(int id)
        {
            Id = id;
            Archetype = new Archetype();
        }

        internal void LinkWithGameObject(GameObject gameObject)
        {
            GameObject = gameObject;
        }

        public T Add<T>() where T : Context, new()
        {
            var component = ComponentsManager.GetComponent<T>(this);
            if (_components.ContainsKey(component.TypeId))
            {
                throw new ArgumentException($"Entity already has same component");
            }
            _components[component.TypeId] = component;
            Archetype.SetFlag(component.TypeId);
            return component;
        }

        internal void Add(Context context)
        {
            ComponentsManager.RegisterComponent(this, context);
            if (_components.ContainsKey(context.TypeId))
            {
                throw new ArgumentException($"Entity already has same component");
            }
            _components[context.TypeId] = context;
            Archetype.SetFlag(context.TypeId);
        }

        public T AddUniq<T>() where T : Context, new()
        {
            var component = Add<T>();
            component.IsUniq = true;
            World.SetUniqEntity(component);
            return component;
        }

        public void AddUniq(Context context)
        {
            Add(context);
            context.IsUniq = true;
            World.SetUniqEntity(context);
        }

        public T AddTag<T>() where T : Context, new()
        {
            var component = Add<T>();
            component.IsTag = true;
            World.AddTagged<T>(component);
            return component;
        }

        public void Remove<T>() where T : Context
        {
            var component = Get<T>();
            if (component.IsUniq)
            {
                World.RemoveUniqEntity(component);
            }
            Archetype.RemoveFlag(component.TypeId);
            ComponentsManager.Release(_components[component.TypeId]);
            _components.Remove(component.TypeId);
        }

        public T Get<T>() where T : Context
        {
            var componentType = typeof(T);
            var typeId = ComponentsManager.GetComponentTypeId(componentType);
            return (T)_components[typeId];
        }

        public bool Have<T>() where T : Context
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
            if (GameObject != null)
            {
                UnityEngine.Object.Destroy(GameObject);
                GameObject = null;
            }
            foreach (var component in _components.Values)
            {
                if (component.IsUniq)
                {
                    World.RemoveUniqEntity(component);
                }
                component.Clear();
                ComponentsManager.Release(component);
            }
            _components.Clear();
            World = null;
        }
    }
}

