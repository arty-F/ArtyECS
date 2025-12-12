using System;

namespace ArtyECS.Core
{
    /// <summary>
    /// Exception thrown when attempting to get a component that doesn't exist on an entity.
    /// </summary>
    /// <remarks>
    /// This exception is thrown by GetComponent&lt;T&gt;() when the specified entity
    /// does not have a component of the requested type.
    /// 
    /// API-002: Keep GetComponent with Exceptions (COMPLETED)
    /// - GetComponent&lt;T&gt;() throws ComponentNotFoundException if component is absent
    /// - Exceptions are safe in builds (minimize overhead, no stack trace if not needed)
    /// - Aligns with API-006 decision to use exceptions throughout
    /// 
    /// Usage:
    /// <code>
    /// try
    /// {
    ///     var hp = ComponentsManager.GetComponent&lt;Hp&gt;(entity);
    ///     hp.Amount -= 1f;
    /// }
    /// catch (ComponentNotFoundException)
    /// {
    ///     // Component doesn't exist
    /// }
    /// </code>
    /// </remarks>
    public class ComponentNotFoundException : Exception
    {
        /// <summary>
        /// Initializes a new instance of ComponentNotFoundException with a default message.
        /// </summary>
        public ComponentNotFoundException()
            : base("Component not found on entity.")
        {
        }

        /// <summary>
        /// Initializes a new instance of ComponentNotFoundException with a specified message.
        /// </summary>
        /// <param name="message">The message that describes the error</param>
        public ComponentNotFoundException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of ComponentNotFoundException with a message that includes
        /// the entity and component type information.
        /// </summary>
        /// <param name="entity">The entity that was queried</param>
        /// <param name="componentType">The component type that was not found</param>
        public ComponentNotFoundException(Entity entity, Type componentType)
            : base($"Entity {entity} does not have a component of type {componentType.Name}.")
        {
        }
    }
}

