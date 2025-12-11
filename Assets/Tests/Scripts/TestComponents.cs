using ArtyECS.Core;

// Test components for tests
public struct TestComponent : IComponent
{
    public int Value;
}

public struct Position : IComponent
{
    public float X;
    public float Y;
    public float Z;
}

public struct Velocity : IComponent
{
    public float X;
    public float Y;
    public float Z;
}

public struct Health : IComponent
{
    public float Amount;
}

public struct Dead : IComponent { }

public struct Destroyed : IComponent { }

// Additional test components for System tests
public struct CounterComponent : IComponent
{
    public int Value;
}

public struct UpdateCounter : IComponent
{
    public int Value;
}

public struct FixedUpdateCounter : IComponent
{
    public int Value;
}

public struct Spawner : IComponent
{
    public int SpawnCount;
}

public struct Acceleration : IComponent
{
    public float X;
    public float Y;
    public float Z;
}

