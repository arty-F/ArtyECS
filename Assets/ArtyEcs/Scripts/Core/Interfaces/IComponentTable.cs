using System;

namespace ArtyECS.Core
{
    internal interface IComponentTable
    {
        bool TryRemoveComponentForEntity(Entity entity);
        ReadOnlySpan<Entity> GetEntities();
        bool HasComponentForEntity(Entity entity);
        object GetComponentValue(Entity entity);
    }
}

