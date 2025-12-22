#if UNITY_EDITOR
using System;
using ArtyECS.Core;

namespace ArtyECS.Editor
{
    internal struct EntityWorldKey : IEquatable<EntityWorldKey>
    {
        public readonly Entity Entity;
        public readonly WorldInstance World;

        public EntityWorldKey(Entity entity, WorldInstance world)
        {
            Entity = entity;
            World = world;
        }

        public bool Equals(EntityWorldKey other)
        {
            return Entity.Equals(other.Entity) && ReferenceEquals(World, other.World);
        }

        public override int GetHashCode()
        {
            return Entity.GetHashCode() ^ World.GetHashCode();
        }
    }
}
#endif

