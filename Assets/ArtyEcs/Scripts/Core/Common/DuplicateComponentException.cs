using System;

namespace ArtyECS.Core
{
    public class DuplicateComponentException : Exception
    {
        public DuplicateComponentException()
            : base("Entity already has a component of this type.")
        {
        }

        public DuplicateComponentException(string message)
            : base(message)
        {
        }

        public DuplicateComponentException(Entity entity, Type componentType)
            : base($"Entity {entity} already has a component of type {componentType.Name}. Each entity can have at most one component of each type.")
        {
        }
    }
}

