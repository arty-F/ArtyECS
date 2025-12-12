using System;

namespace ArtyECS.Core
{
    /// <summary>
    /// Exception thrown when attempting to add a component that already exists on an entity.
    /// </summary>
    /// <remarks>
    /// This exception is thrown by AddComponent&lt;T&gt;() when the specified entity
    /// already has a component of the requested type.
    /// 
    /// API-006: Use Exceptions Instead of Try Pattern (COMPLETED)
    /// - AddComponent&lt;T&gt;() throws DuplicateComponentException if component already exists
    /// - Exceptions are safe in builds (minimize overhead, no stack trace if not needed)
    /// - Lightweight exception with simple constructors for minimal overhead
    /// 
    /// Usage:
    /// <code>
    /// try
    /// {
    ///     ComponentsManager.AddComponent&lt;Hp&gt;(entity, new Hp { Amount = 100f });
    /// }
    /// catch (DuplicateComponentException)
    /// {
    ///     // Entity already has this component
    /// }
    /// </code>
    /// </remarks>
    public class DuplicateComponentException : Exception
    {
        /// <summary>
        /// Initializes a new instance of DuplicateComponentException with a default message.
        /// </summary>
        public DuplicateComponentException()
            : base("Entity already has a component of this type.")
        {
        }

        /// <summary>
        /// Initializes a new instance of DuplicateComponentException with a specified message.
        /// </summary>
        /// <param name="message">The message that describes the error</param>
        public DuplicateComponentException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of DuplicateComponentException with a message that includes
        /// the entity and component type information.
        /// </summary>
        /// <param name="entity">The entity that already has the component</param>
        /// <param name="componentType">The component type that already exists</param>
        public DuplicateComponentException(Entity entity, Type componentType)
            : base($"Entity {entity} already has a component of type {componentType.Name}. Each entity can have at most one component of each type.")
        {
        }
    }
}

