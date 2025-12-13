using System;

namespace ArtyECS.Core
{
    public static class EntityExtensions
    {
        public static T Get<T>(this Entity entity, WorldInstance world = null) where T : struct, IComponent
        {
            world ??= World.GetOrCreate();
            return world.GetComponent<T>(entity);
        }

        public static bool Has<T>(this Entity entity, WorldInstance world = null) where T : struct, IComponent
        {
            world ??= World.GetOrCreate();
            try
            {
                world.GetComponent<T>(entity);
                return true;
            }
            catch (ComponentNotFoundException)
            {
                return false;
            }
        }

        public static void AddComponent<T>(this Entity entity, T component, WorldInstance world = null) where T : struct, IComponent
        {
            world ??= World.GetOrCreate();
            world.AddComponent(entity, component);
        }

        public static bool RemoveComponent<T>(this Entity entity, WorldInstance world = null) where T : struct, IComponent
        {
            world ??= World.GetOrCreate();
            return world.RemoveComponent<T>(entity);
        }
    }
}

