using System;

namespace ArtyECS.Core
{
    public struct Entity : IEquatable<Entity>
    {
        public readonly int Id;

        public readonly int Generation;

        public static readonly Entity Invalid = new Entity(-1, 0);

        public Entity(int id, int generation = 0)
        {
            Id = id;
            Generation = generation;
        }

        public bool IsValid => Id >= 0;

        public bool Equals(Entity other)
        {
            return Id == other.Id && Generation == other.Generation;
        }

        public override bool Equals(object obj)
        {
            return obj is Entity other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + Id.GetHashCode();
                hash = hash * 31 + Generation.GetHashCode();
                return hash;
            }
        }

        public static bool operator ==(Entity left, Entity right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Entity left, Entity right)
        {
            return !left.Equals(right);
        }

        public override string ToString()
        {
            return $"Entity({Id}, Gen:{Generation})";
        }
    }
}

