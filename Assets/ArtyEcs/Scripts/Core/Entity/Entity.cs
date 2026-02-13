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

        private Dictionary<int, Context> _contexts = new(Constants.ENTITY_CONTEXTS_CAPACITY);

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
            var context = ContextsManager.GetContext<T>(this);
            if (_contexts.ContainsKey(context.TypeId))
            {
                throw new ArgumentException($"Entity <{Id}> already has same context of type <{typeof(T).FullName}>");
            }
            _contexts[context.TypeId] = context;
            Archetype.SetFlag(context.TypeId);
            return context;
        }

        public T AddUniq<T>(Context context) where T : Context, new()
        {
            if (context == null)
            {
                context = Add<T>();
            }
            else
            {
                ContextsManager.RegisterContext(context);
            }
            context.IsUniq = true;
            World.SetUniq<T>(context);
            return (T)context;
        }

        public void Remove<T>() where T : Context
        {
            var context = Get<T>();
            if (context.IsUniq)
            {
                World.RemoveUniq(context);
            }
            Archetype.RemoveFlag(context.TypeId);
            ContextsManager.Release(_contexts[context.TypeId]);
            _contexts.Remove(context.TypeId);
        }

        public T Get<T>() where T : Context
        {
            var typeId = ContextsManager.GetContextTypeId(typeof(T));
            return (T)_contexts[typeId];
        }

        public bool Have<T>() where T : Context
        {
            var typeId = ContextsManager.GetContextTypeId(typeof(T));
            return _contexts.ContainsKey(typeId);
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
            foreach (var component in _contexts.Values)
            {
                if (component.IsUniq)
                {
                    World.RemoveUniq(component);
                }
                component.Clear();
                ContextsManager.Release(component);
            }
            _contexts.Clear();
            World = null;
        }
    }
}

