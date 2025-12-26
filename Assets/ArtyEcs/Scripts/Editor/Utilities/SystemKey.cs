#if UNITY_EDITOR
using System;
using ArtyECS.Core;

namespace ArtyECS.Editor
{
    internal struct SystemKey : IEquatable<SystemKey>
    {
        public readonly SystemHandler System;
        public readonly WorldInstance World;
        public readonly string QueueName;

        public SystemKey(SystemHandler system, WorldInstance world, string queueName)
        {
            System = system;
            World = world;
            QueueName = queueName;
        }

        public bool Equals(SystemKey other)
        {
            return ReferenceEquals(System, other.System) && ReferenceEquals(World, other.World) && QueueName == other.QueueName;
        }

        public override int GetHashCode()
        {
            return System.GetHashCode() ^ World.GetHashCode() ^ QueueName.GetHashCode();
        }
    }
}
#endif

