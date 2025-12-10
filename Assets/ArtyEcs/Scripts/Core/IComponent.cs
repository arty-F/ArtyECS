namespace ArtyECS.Core
{
    /// <summary>
    /// Marker interface for ECS components.
    /// All components must implement this interface to be recognized by the ECS framework.
    /// Components are value types (structs) for zero-allocation performance.
    /// </summary>
    /// <remarks>
    /// This is a marker interface with no methods - it serves only for type identification.
    /// Components should be implemented as structs that implement IComponent.
    /// </remarks>
    /// <example>
    /// <code>
    /// public struct Position : IComponent
    /// {
    ///     public float X;
    ///     public float Y;
    ///     public float Z;
    /// }
    /// </code>
    /// </example>
    public interface IComponent
    {
        // Marker interface - no methods required
        // Used for type identification and generic constraints
    }
}

