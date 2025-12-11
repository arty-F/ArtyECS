using System;

namespace ArtyECS.Core
{
    /// <summary>
    /// Lightweight entity identifier for ECS framework.
    /// Supports ID recycling through generation numbers for pooling.
    /// </summary>
    public struct Entity : IEquatable<Entity>
    {
        /// <summary>
        /// Entity identifier (index in pool)
        /// </summary>
        public readonly int Id;

        /// <summary>
        /// Generation number for ID recycling safety.
        /// Incremented when entity ID is reused.
        /// </summary>
        public readonly int Generation;

        /// <summary>
        /// Invalid entity identifier (Id = -1, Generation = 0)
        /// </summary>
        public static readonly Entity Invalid = new Entity(-1, 0);

        /// <summary>
        /// Creates a new Entity with specified ID and generation.
        /// </summary>
        /// <param name="id">Entity identifier</param>
        /// <param name="generation">Generation number for ID recycling</param>
        public Entity(int id, int generation = 0)
        {
            Id = id;
            Generation = generation;
        }

        /// <summary>
        /// Checks if entity is valid (Id >= 0)
        /// </summary>
        public bool IsValid => Id >= 0;

        /// <summary>
        /// Equality comparison with another Entity.
        /// </summary>
        public bool Equals(Entity other)
        {
            return Id == other.Id && Generation == other.Generation;
        }

        /// <summary>
        /// Equality comparison with object.
        /// </summary>
        public override bool Equals(object obj)
        {
            return obj is Entity other && Equals(other);
        }

        /// <summary>
        /// Hash code for dictionary usage.
        /// Combines ID and generation for uniqueness.
        /// </summary>
        public override int GetHashCode()
        {
            // Combine ID and generation using prime numbers for better distribution
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + Id.GetHashCode();
                hash = hash * 31 + Generation.GetHashCode();
                return hash;
            }
        }

        /// <summary>
        /// Equality operator.
        /// </summary>
        public static bool operator ==(Entity left, Entity right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Inequality operator.
        /// </summary>
        public static bool operator !=(Entity left, Entity right)
        {
            return !left.Equals(right);
        }

        /// <summary>
        /// String representation for debugging.
        /// </summary>
        public override string ToString()
        {
            return $"Entity({Id}, Gen:{Generation})";
        }
    }
}

