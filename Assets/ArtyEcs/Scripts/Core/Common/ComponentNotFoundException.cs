using System;

namespace ArtyECS.Core
{
    public class ComponentNotFoundException : Exception
    {
        public ComponentNotFoundException()
            : base("Component not found on entity.")
        {
        }

        public ComponentNotFoundException(string message)
            : base(message)
        {
        }

        public ComponentNotFoundException(Entity entity, Type componentType)
            : base($"Entity {entity} does not have a component of type {componentType.Name}.")
        {
        }
    }
}

