using System;

namespace ArtyECS.Core
{
    internal interface IComponentTable
    {
        bool TryRemoveComponentForEntity(Entity entity);
    }
}

