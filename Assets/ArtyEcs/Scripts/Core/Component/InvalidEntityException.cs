using System;

namespace ArtyECS.Core
{
    /// <summary>
    /// Exception thrown when attempting to use an invalid or deallocated entity.
    /// </summary>
    /// <remarks>
    /// This exception is thrown by component operations when the specified entity
    /// is invalid (Id &lt; 0) or has been deallocated (generation mismatch).
    /// 
    /// API-006: Use Exceptions Instead of Try Pattern (COMPLETED)
    /// - Component operations throw InvalidEntityException if entity is invalid
    /// - Exceptions are safe in builds (minimize overhead, no stack trace if not needed)
    /// - Lightweight exception with simple constructors for minimal overhead
    /// 
    /// Usage:
    /// <code>
    /// try
    /// {
    ///     ComponentsManager.AddComponent&lt;Hp&gt;(entity, new Hp { Amount = 100f });
    /// }
    /// catch (InvalidEntityException)
    /// {
    ///     // Entity is invalid or deallocated
    /// }
    /// </code>
    /// </remarks>
    public class InvalidEntityException : Exception
    {
        /// <summary>
        /// Initializes a new instance of InvalidEntityException with a default message.
        /// </summary>
        public InvalidEntityException()
            : base("Entity is invalid or has been deallocated.")
        {
        }

        /// <summary>
        /// Initializes a new instance of InvalidEntityException with a specified message.
        /// </summary>
        /// <param name="message">The message that describes the error</param>
        public InvalidEntityException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of InvalidEntityException with a message that includes
        /// the entity information.
        /// </summary>
        /// <param name="entity">The entity that is invalid</param>
        public InvalidEntityException(Entity entity)
            : base($"Entity {entity} is invalid or has been deallocated. Ensure the entity is valid and allocated before use.")
        {
        }
    }
}

