#if UNITY_EDITOR
using System;
using ArtyECS.Core;

namespace ArtyECS.Editor
{
    internal struct WorldQueueKey : IEquatable<WorldQueueKey>
    {
        public readonly WorldInstance World;
        public readonly string QueueName;

        public WorldQueueKey(WorldInstance world, string queueName)
        {
            World = world;
            QueueName = queueName;
        }

        public bool Equals(WorldQueueKey other)
        {
            return ReferenceEquals(World, other.World) && QueueName == other.QueueName;
        }

        public override int GetHashCode()
        {
            return World.GetHashCode() ^ QueueName.GetHashCode();
        }
    }
}
#endif

