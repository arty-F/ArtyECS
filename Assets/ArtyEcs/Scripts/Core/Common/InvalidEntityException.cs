using System;

namespace ArtyECS.Core
{
    public class InvalidEntityException : Exception
    {
        public InvalidEntityException()
            : base("Entity is invalid or has been deallocated.")
        {
        }

        public InvalidEntityException(string message)
            : base(message)
        {
        }

        public InvalidEntityException(Entity entity)
            : base($"Entity {entity} is invalid or has been deallocated. Ensure the entity is valid and allocated before use.")
        {
        }
    }
}

