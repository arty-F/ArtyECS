using System;

namespace ArtyECS.Core
{
    /// <summary>
    /// Interface for component storage that allows removing components for an entity without knowing the type.
    /// Used internally for efficient entity destruction without reflection.
    /// </summary>
    internal interface IComponentTable
    {
        /// <summary>
        /// Attempts to remove a component for the specified entity if it exists.
        /// </summary>
        /// <param name="entity">Entity to remove component for</param>
        /// <returns>True if component was removed, false if entity didn't have this component type</returns>
        bool TryRemoveComponentForEntity(Entity entity);
    }
}

